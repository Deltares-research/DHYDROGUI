using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Domain;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Provides MDW file reader functionality.
    /// </summary>
    public partial class MdwFile
    {
        /// <summary>
        /// Reads an MDW file from the specified location.
        /// </summary>
        /// <param name="filePath">The path of the MDW file to read.</param>
        /// <returns>An <see cref="MdwFileDTO"/> instance containing the MDW file contents.</returns>
        public MdwFileDTO Load(string filePath)
        {
            var logHandler = new LogHandler(Resources.MdwFile_Load_loading_the_D_Waves_model, log);
            MdwFilePath = filePath;

            var modelDefinition = new WaveModelDefinition();

            IniData iniData;
            using (var fileStream = new FileStream(MdwFilePath, FileMode.Open, FileAccess.Read))
            {
                iniData = new IniReader().ReadIniFile(fileStream, MdwFilePath);
            }
            
            mdwFileMerger.Original = iniData;
            
            string mdwDir = Path.GetDirectoryName(filePath);

            ConvertMdwSectionsToModelDefinitionProperties(modelDefinition, iniData, logHandler);

            // domain(s) and nesting
            IEnumerable<IniSection> domainSections = iniData.GetAllSections(KnownWaveSections.DomainSection);
            List<WaveDomainData> allDomains = WaveDomainDataConverter.Convert(domainSections, mdwDir, logHandler).ToList();
            foreach (WaveDomainData domain in allDomains)
            {
                if (domain.NestedInDomain == -1)
                {
                    modelDefinition.OuterDomain = domain;
                }
                else
                {
                    WaveDomainData superDomain = allDomains[domain.NestedInDomain - 1];
                    superDomain.SubDomains.Add(domain);
                    domain.SuperDomain = superDomain;
                }

                WaveModel.LoadGrid(mdwDir, domain);
            }

            ITimeFrameData timeFrameData = CreateTimePointData(iniData,
                                                               modelDefinition.ModelReferenceDateTime,
                                                               logHandler);

            ReadWaveBoundaries(modelDefinition, iniData, mdwDir, logHandler);

            modelDefinition.FeatureContainer.Obstacles.AddRange(CreateObstacleData(iniData.GetSection(KnownWaveSections.GeneralSection), modelDefinition));

            string locFile = iniData.Sections
                                    .First(c => c.Name == KnownWaveSections.OutputSection)
                                    .GetPropertyValueOrDefault(KnownWaveProperties.LocationFile);
            if (locFile != null)
            {
                modelDefinition.FeatureContainer.ObservationPoints.Clear();
                modelDefinition.FeatureContainer.ObservationPoints.AddRange(new ObsFile<Feature2DPoint>().Read(Path.Combine(mdwDir, locFile), false));
            }

            string curveFile = iniData.Sections
                                      .First(c => c.Name == KnownWaveSections.OutputSection)
                                      .GetPropertyValueOrDefault(KnownWaveProperties.CurveFile);
            if (curveFile != null)
            {
                modelDefinition.FeatureContainer.ObservationCrossSections.Clear();
                modelDefinition.FeatureContainer.ObservationCrossSections.AddRange(new EventedList<Feature2D>(new PliFile<Feature2D>().Read(Path.Combine(mdwDir, curveFile))));
            }

            SetInputTemplateFile(iniData, modelDefinition, mdwDir);

            logHandler.LogReport();

            return new MdwFileDTO(modelDefinition, timeFrameData);
        }

        private static void ReadWaveBoundaries(WaveModelDefinition modelDefinition,
                                               IniData iniData,
                                               string mdwDirPath,
                                               ILogHandler logHandler)
        {
            IBoundaryContainer boundaryContainer = modelDefinition.BoundaryContainer;
            CurvilinearGrid grid = modelDefinition.OuterDomain.Grid;
            if (grid.IsEmpty)
            {
                log.Warn(Resources.MdwFile_ReadWaveBoundaries_Boundaries_cannot_be_imported__because_there_is_no_grid_detected);
                return;
            }

            boundaryContainer.UpdateGridBoundary(new GridBoundary(grid));

            IEnumerable<IniSection> boundarySections = iniData.GetAllSections(KnownWaveSections.BoundarySection).ToArray();
            IDictionary<string, List<IFunction>> timeSeriesData = ReadBoundaryTimeSeriesData(iniData, mdwDirPath);

            if (DomainWideBoundarySectionConverter.IsDomainWideBoundarySection(boundarySections))
            {
                DomainWideBoundarySectionConverter.Convert(boundaryContainer, boundarySections, mdwDirPath);
            }
            else
            {
                var boundariesConverter = new WaveBoundaryConverter(new ImportBoundaryConditionDataComponentFactory(new ForcingTypeDefinedParametersFactory()),
                                                                    new WaveBoundaryGeometricDefinitionFactory(boundaryContainer));
                IEnumerable<IWaveBoundary> waveBoundaries = boundariesConverter.Convert(boundarySections, timeSeriesData, mdwDirPath, logHandler);
                boundaryContainer.Boundaries.AddRange(waveBoundaries);
            }
        }

        private static IDictionary<string, List<IFunction>> ReadBoundaryTimeSeriesData(IniData iniData,
                                                                                       string mdwDirPath)
        {
            string relativeBcwFilePath = iniData.GetSection(KnownWaveSections.GeneralSection)
                                                .GetPropertyValueOrDefault(KnownWaveProperties.TimeSeriesFile);

            return !string.IsNullOrEmpty(relativeBcwFilePath)
                       ? new BcwFile().Read(Path.Combine(mdwDirPath, relativeBcwFilePath))
                       : new Dictionary<string, List<IFunction>>();
        }

        /// <summary>
        /// Converting mdw sections to model definition properties.
        /// Second part is for linked properties, since the default value of a property can be dependent on another property value
        /// and this part will be used in situations if the property with multiple default values is missing in the mdw file.
        /// Based on the other property the correct one will be set. Otherwise the default value is based on the default value of
        /// the other property.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="iniData">The INI data from the mdw file.</param>
        /// <param name="logHandler">The log handler.</param>
        private static void ConvertMdwSectionsToModelDefinitionProperties(WaveModelDefinition modelDefinition, IniData iniData, ILogHandler logHandler)
        {
            ConvertMdwSectionProperties(modelDefinition, iniData, logHandler);
            ConvertModelDefinitionProperties(modelDefinition, iniData, logHandler);
        }

        private static void ConvertMdwSectionProperties(WaveModelDefinition modelDefinition, IniData iniData, ILogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(new MdwFileBackwardsCompatibilityConfigurationValues());

            foreach (IniSection section in iniData.Sections)
            {
                string updatedSectionName = backwardsCompatibilityHelper.GetUpdatedSectionName(section.Name, logHandler);
                if (updatedSectionName != null)
                {
                    iniData.RenameSections(section.Name, updatedSectionName);
                }
                
                ModelPropertySchema<WaveModelPropertyDefinition> modelSchema = modelDefinition.ModelSchema;
                if (!modelSchema.ModelDefinitionCategory.ContainsKey(section.Name))
                {
                    continue;
                }

                ModelPropertyGroup definedCategory = modelSchema.ModelDefinitionCategory[section.Name];
                foreach (IniProperty mdwProperty in section.Properties)
                {
                    if (IsObsoleteProperty(mdwProperty, backwardsCompatibilityHelper, logHandler))
                    {
                        continue;
                    }

                    string propertyKey = backwardsCompatibilityHelper.GetUpdatedPropertyKey(mdwProperty.Key, logHandler) ?? mdwProperty.Key;

                    WaveModelPropertyDefinition definition = GetWaveModelPropertyDefinition(mdwProperty, modelSchema, section, definedCategory, propertyKey);

                    modelDefinition.SetModelProperty(section.Name, propertyKey, new WaveModelProperty(definition, mdwProperty.Value));
                }
            }
        }

        private static void ConvertModelDefinitionProperties(WaveModelDefinition modelDefinition, IniData iniData, ILogHandler logHandler)
        {
            foreach (KeyValuePair<string, WaveModelPropertyDefinition> propertyDefinition in modelDefinition.ModelSchema.PropertyDefinitions)
            {
                WaveModelPropertyDefinition propertyValue = propertyDefinition.Value;

                if (!propertyValue.MultipleDefaultValuesAvailable)
                {
                    continue;
                }

                string nameOfDependentOnProperty = propertyValue.DefaultValueDependentOn;
                string nameOfPropertyWithMultipleDefaultValues = propertyValue.FilePropertyName;
                string categoryNameWithPropertyWithMultipleDefaultValues = propertyValue.FileCategoryName;

                IEnumerable<IniSection> sectionOfDependentOnProperty = GetSectionsWithPropertyKey(iniData, nameOfDependentOnProperty);
                IEnumerable<IniSection> sectionOfPropertyWithMultipleDefaultValues = GetSectionsWithPropertyKey(iniData, nameOfPropertyWithMultipleDefaultValues);

                // Situation in which property with multiple default values is missing and corresponding default value will be set.
                if (sectionOfPropertyWithMultipleDefaultValues.Any() || !sectionOfDependentOnProperty.Any())
                {
                    continue;
                }

                logHandler.ReportWarningFormat(Resources.MdwFile_In_the_MDW_file_the_property__0__is_missing__Based_on_property__1__the_default_value_is_set,
                                               nameOfPropertyWithMultipleDefaultValues, nameOfDependentOnProperty);

                WaveModelProperty dependentOnProperty = modelDefinition.GetModelProperty(sectionOfDependentOnProperty.First().Name, nameOfDependentOnProperty);
                WaveModelProperty propertyWithMultipleDefaultValues = modelDefinition.GetModelProperty(categoryNameWithPropertyWithMultipleDefaultValues, nameOfPropertyWithMultipleDefaultValues);

                var index = (int)dependentOnProperty.Value;
                propertyWithMultipleDefaultValues.SetValueAsString(propertyWithMultipleDefaultValues.PropertyDefinition.MultipleDefaultValues[index]);
            }
        }

        private static WaveModelPropertyDefinition GetWaveModelPropertyDefinition(IniProperty mdwProperty, ModelPropertySchema<WaveModelPropertyDefinition> modelSchema, IniSection section, ModelPropertyGroup definedCategory, string propertyKey)
        {
            return modelSchema.PropertyDefinitions.ContainsKey(propertyKey.ToLower())
                       ? modelSchema.PropertyDefinitions[propertyKey.ToLower()]
                       : CreateWaveModelPropertyDefinition(mdwProperty, section, definedCategory);
        }

        private static bool IsObsoleteProperty(IniProperty mdwProperty, IniBackwardsCompatibilityHelper backwardsCompatibilityHelper, ILogHandler logHandler)
        {
            if (!backwardsCompatibilityHelper.IsObsoletePropertyKey(mdwProperty.Key))
            {
                return false;
            }

            logHandler.ReportWarningFormat(Common.Properties.Resources.Parameter__0__is_not_supported_by_our_computational_core_and_will_be_removed_from_your_input_file, mdwProperty.Key);
            return true;
        }

        private static WaveModelPropertyDefinition CreateWaveModelPropertyDefinition(IniProperty mdwProperty, IniSection section, ModelPropertyGroup definedCategory)
        {
            return new WaveModelPropertyDefinition
            {
                Caption = mdwProperty.Key,
                DataType = typeof(string),
                FileCategoryName = section.Name,
                FilePropertyName = mdwProperty.Key,
                Category = definedCategory.Name,
                // default value as string should always be an empty string and not null.
                DefaultValueAsString = string.Empty
            };
        }

        private static IEnumerable<IniSection> GetSectionsWithPropertyKey(IniData iniData, string propertyKey)
        {
            return iniData.Sections.Where(mc => mc.Properties.Any(p => p.Key.Equals(propertyKey))).ToList();
        }

        private ITimeFrameData CreateTimePointData(IniData iniData, DateTime referenceDate, ILogHandler logHandler)
        {
            var timePointData = new TimeFrameData
            {
                HydrodynamicsInputDataType = HydrodynamicsInputDataType.Constant,
                WindInputDataType = WindInputDataType.Constant
            };

            IList<DateTime> times = new List<DateTime>();
            List<IniSection> timePointSections = iniData.GetAllSections(KnownWaveSections.TimePointSection).ToList();

            if (timePointSections.Exists(c => c.GetPropertyValueOrDefault(KnownWaveProperties.WaterLevel) != null))
            {
                timePointData.HydrodynamicsInputDataType = HydrodynamicsInputDataType.TimeVarying;
            }

            if (timePointSections.Exists(c => c.GetPropertyValueOrDefault(KnownWaveProperties.WindSpeed) != null))
            {
                timePointData.WindInputDataType = WindInputDataType.TimeVarying;
            }

            IniSection generalSection = iniData.GetSection(KnownWaveSections.GeneralSection);
            List<string> meteoFiles = generalSection.GetAllProperties(KnownWaveProperties.MeteoFile).Select(p => p.Value).ToList();

            if (meteoFiles.Any())
            {
                timePointData.WindInputDataType = WindInputDataType.FileBased;
                SetMeteoDataFromFiles(timePointData.WindFileData, meteoFiles, logHandler);
            }

            if (timePointData.HydrodynamicsInputDataType == HydrodynamicsInputDataType.Constant)
            {
                double waterLevel = double.Parse(generalSection.GetPropertyValueOrDefault(KnownWaveProperties.WaterLevel, "0.0"),
                                                 NumberStyles.Any, CultureInfo.InvariantCulture);
                double velocityX = double.Parse(generalSection.GetPropertyValueOrDefault(KnownWaveProperties.WaterVelocityX, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                double velocityY = double.Parse(generalSection.GetPropertyValueOrDefault(KnownWaveProperties.WaterVelocityY, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                timePointData.HydrodynamicsConstantData.WaterLevel = waterLevel;
                timePointData.HydrodynamicsConstantData.VelocityX = velocityX;
                timePointData.HydrodynamicsConstantData.VelocityY = velocityY;
            }

            if (timePointData.WindInputDataType == WindInputDataType.Constant)
            {
                double windSpeed = double.Parse(generalSection.GetPropertyValueOrDefault(KnownWaveProperties.WindSpeed, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                double windDirection = double.Parse(generalSection.GetPropertyValueOrDefault(KnownWaveProperties.WindDirection, "0.0"),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture);
                timePointData.WindConstantData.Speed = windSpeed;
                timePointData.WindConstantData.Direction = windDirection;
            }

            foreach (IniSection timePoint in timePointSections)
            {
                DateTime time = referenceDate.AddMinutes(double.Parse(timePoint.GetPropertyValueOrDefault(KnownWaveProperties.Time, "0.0"),
                                                                      NumberStyles.Any,
                                                                      CultureInfo.InvariantCulture));
                times.Add(time);

                if (!double.TryParse(timePoint.GetPropertyValueOrDefault(KnownWaveProperties.WaterLevel), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double waterLevel))
                {
                    waterLevel = (double)timePointData.TimeVaryingData.Components[0].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValueOrDefault(KnownWaveProperties.WaterVelocityX), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double velocityX))
                {
                    velocityX = (double)timePointData.TimeVaryingData.Components[1].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValueOrDefault(KnownWaveProperties.WaterVelocityY), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double velocityY))
                {
                    velocityY = (double)timePointData.TimeVaryingData.Components[2].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValueOrDefault(KnownWaveProperties.WindSpeed), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double windSpeed))
                {
                    windSpeed = (double)timePointData.TimeVaryingData.Components[3].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValueOrDefault(KnownWaveProperties.WindDirection), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double windDirection))
                {
                    windDirection = (double)timePointData.TimeVaryingData.Components[4].DefaultValue;
                }

                timePointData.TimeVaryingData[time] = new[]
                {
                    waterLevel,
                    velocityX,
                    velocityY,
                    windSpeed,
                    windDirection
                };
            }

            return timePointData;
        }

        private void SetMeteoDataFromFiles(WaveMeteoData windFileData,
                                           IReadOnlyCollection<string> meteoFiles,
                                           ILogHandler logHandler)
        {
            List<string> spwFiles = meteoFiles.Where(mf => mf.EndsWith(".spw")).ToList();
            List<string> otherFiles = meteoFiles.Where(mf => !mf.EndsWith(".spw")).ToList();

            if (spwFiles.Count > 1)
            {
                log.Error(Resources.MdwFile_Multiple_spider_web_files_specified_for_single_domain);
                return;
            }

            if (spwFiles.Count == 1 && otherFiles.Count == 0)
            {
                windFileData.FileType = WindDefinitionType.SpiderWebGrid;
                windFileData.SpiderWebFilePath = spwFiles[0];
                return;
            }

            switch (otherFiles.Count)
            {
                case 1:
                    windFileData.FileType = WindDefinitionType.WindXY;
                    windFileData.XYVectorFilePath = otherFiles[0];
                    break;
                case 2:
                    windFileData.FileType = WindDefinitionType.WindXWindY;
                    windFileData.XComponentFilePath = GetMeteoFile(otherFiles, KnownWaveProperties.MeteoXComponentValue, logHandler);
                    windFileData.YComponentFilePath = GetMeteoFile(otherFiles, KnownWaveProperties.MeteoYComponentValue, logHandler);
                    break;
                default:
                    log.Error(Resources.MdwFile_Invalid_number_of_meteo_files_specified_for_single_domain);
                    return;
            }

            if (spwFiles.Count == 1)
            {
                windFileData.HasSpiderWeb = true;
                windFileData.SpiderWebFilePath = spwFiles[0];
            }
        }

        private string GetMeteoFile(IEnumerable<string> meteoFiles, string quantityParameterValue, ILogHandler logHandler)
        {
            var meteoFileReader = new MeteoFileReader();

            foreach (string otherFile in meteoFiles)
            {
                string filePath = Path.Combine(Path.GetDirectoryName(MdwFilePath), otherFile);
                IEnumerable<MeteoFileProperty> meteoProperties = meteoFileReader.Read(filePath);

                if (meteoProperties.First(mp => mp.Property == KnownWaveProperties.MeteoQuantityField).Value == quantityParameterValue)
                {
                    return otherFile;
                }
            }

            logHandler.ReportError(string.Format(Resources.MdwFile_Could_not_find_meteo_file_for__0__, quantityParameterValue));
            return string.Empty;
        }

        private IEnumerable<WaveObstacle> CreateObstacleData(IniSection generalSection,
                                                             WaveModelDefinition modelDefinition)
        {
            string obstacleFile = generalSection.GetPropertyValueOrDefault(KnownWaveProperties.ObstacleFile, string.Empty);
            if (obstacleFile == string.Empty)
            {
                yield break;
            }

            string mdwDirectory = Path.GetDirectoryName(MdwFilePath);
            string obstacleFilePath = Path.Combine(mdwDirectory, obstacleFile);
            if (!File.Exists(obstacleFilePath))
            {
                log.ErrorFormat("Obstacle file {0} does not exist", obstacleFilePath);
                yield break;
            }

            var iniReader = new IniReader();
            IniData iniData;
            using (var fileStream = new FileStream(obstacleFilePath, FileMode.Open, FileAccess.Read))
            {
                iniData = iniReader.ReadIniFile(fileStream, obstacleFilePath);
            }

            IniSection fileInfo = iniData.GetSection(KnownWaveObsSections.ObstacleFileInformation);
            string polylineFileName = fileInfo.GetPropertyValueOrDefault(KnownWaveObsProperties.PolylineFile);
            string geometryFilePath = Path.Combine(mdwDirectory, polylineFileName);
            if (!File.Exists(geometryFilePath))
            {
                log.ErrorFormat(Resources.MdwFile_Obstacle_polyline_file__0__does_not_exist, geometryFilePath);
                yield break;
            }

            modelDefinition.ObstaclePolylineFile = polylineFileName;

            var pliFile = new PliFile<Feature2D>();
            Dictionary<string, Feature2D> features = pliFile.Read(geometryFilePath).ToDictionary(f => f.Name);

            foreach (IniSection obstacle in iniData.GetAllSections(KnownWaveSections.ObstacleSection))
            {
                string name = obstacle.GetPropertyValueOrDefault(MdwFileConstants.ObstaclePropertyName, "default name");
                if (!features.ContainsKey(name))
                {
                    log.ErrorFormat(Resources.MdwFile_Obstacle_polyline_file__0__does_not_contain_geometry__1__,
                                    geometryFilePath, name);
                    continue;
                }

                if (!features[name].Geometry.IsValid)
                {
                    log.ErrorFormat(Resources.MdwFile_Obstacle_polyline_file__0__contain_invalid_geometry__1__,
                                    geometryFilePath, name);
                    continue;
                }

                var obs = new WaveObstacle
                {
                    Name = name,
                    Geometry = features[name].Geometry
                };
                obs.Name = name;

                obs.Type = obstacle.GetPropertyValueOrDefault(MdwFileConstants.ObstaclePropertyType) == "dam"
                               ? ObstacleType.Dam
                               : ObstacleType.Sheet;

                string reflectionType = obstacle.GetPropertyValueOrDefault(MdwFileConstants.ObstaclePropertyReflections);
                obs.ReflectionType = ReflectionType.No;
                switch (reflectionType)
                {
                    case "specular":
                        obs.ReflectionType = ReflectionType.Specular;
                        break;
                    case "diffuse":
                        obs.ReflectionType = ReflectionType.Diffuse;
                        break;
                }

                obs.TransmissionCoefficient = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, 
                                                                               MdwFileConstants.ObstaclePropertyTransmissionCoefficient,
                                                                               MdwFileConstants.ObstacleDefaultValueTransmissionCoefficient);
                obs.Height = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, 
                                                              MdwFileConstants.ObstaclePropertyHeight,
                                                              MdwFileConstants.ObstacleDefaultValueHeight);
                obs.Alpha = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, 
                                                             MdwFileConstants.ObstaclePropertyAlpha,
                                                             MdwFileConstants.ObstacleDefaultValueAlpha);
                obs.Beta = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, 
                                                            MdwFileConstants.ObstaclePropertyBeta,
                                                            MdwFileConstants.ObstacleDefaultValueBeta);
                obs.ReflectionCoefficient = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, 
                                                                             MdwFileConstants.ObstaclePropertyReflectionCoefficient,
                                                                             MdwFileConstants.ObstacleDefaultValueReflectionCoefficient);

                yield return obs;
            }
        }

        private static double GetObstaclePropertyAndLogIfFails(IniSection obstacle, string fileName, string property,
                                                               double defaultValue)
        {
            string input = obstacle.GetPropertyValueOrDefault(property);
            if (input == null)
            {
                return defaultValue;
            }

            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            log.WarnFormat(Resources.MdwFile_Parsing_error_in_file__0__can_not_convert__1__to_double_property__2__has_default__3__,
                           fileName, input, property, defaultValue);
            return defaultValue;
        }

        private static void SetInputTemplateFile(IniData iniData, WaveModelDefinition modelDefinition, string mdwDir)
        {
            string inputTemplateFile = iniData.GetSection(KnownWaveSections.GeneralSection)
                                              .GetPropertyValueOrDefault(KnownWaveProperties.InputTemplateFile);

            if (!string.IsNullOrEmpty(inputTemplateFile))
            {
                modelDefinition.InputTemplateFilePath = Path.Combine(mdwDir, inputTemplateFile);
            }
        }
    }
}