using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
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

            IList<DelftIniCategory> mdwCategories;
            using (var fileStream = new FileStream(MdwFilePath, FileMode.Open, FileAccess.Read))
            {
                mdwCategories = new DelftIniReader().ReadDelftIniFile(fileStream, MdwFilePath);
            }
            
            mdwFileMerger.Target = mdwCategories;

            string mdwDir = Path.GetDirectoryName(filePath);

            ConvertMdwCategoriesToModelDefinitionProperties(modelDefinition, mdwCategories, logHandler);

            // domain(s) and nesting
            IEnumerable<DelftIniCategory> domainCategories = mdwCategories.Where(c => c.Name == KnownWaveCategories.DomainCategory);
            List<WaveDomainData> allDomains = WaveDomainDataConverter.Convert(domainCategories, mdwDir, logHandler).ToList();
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

            ITimeFrameData timeFrameData = CreateTimePointData(mdwCategories,
                                                               modelDefinition.ModelReferenceDateTime,
                                                               logHandler);

            ReadWaveBoundaries(modelDefinition, mdwCategories, mdwDir, logHandler);

            modelDefinition.FeatureContainer.Obstacles.AddRange(CreateObstacleData(mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory),
                                                                                   modelDefinition));

            string locFile = mdwCategories.First(c => c.Name == KnownWaveCategories.OutputCategory)
                                          .GetPropertyValue(KnownWaveProperties.LocationFile);
            if (locFile != null)
            {
                modelDefinition.FeatureContainer.ObservationPoints.Clear();
                modelDefinition.FeatureContainer.ObservationPoints.AddRange(new ObsFile<Feature2DPoint>().Read(Path.Combine(mdwDir, locFile), false));
            }

            string curveFile = mdwCategories.First(c => c.Name == KnownWaveCategories.OutputCategory)
                                            .GetPropertyValue(KnownWaveProperties.CurveFile);
            if (curveFile != null)
            {
                modelDefinition.FeatureContainer.ObservationCrossSections.Clear();
                modelDefinition.FeatureContainer.ObservationCrossSections.AddRange(new EventedList<Feature2D>(new PliFile<Feature2D>().Read(Path.Combine(mdwDir, curveFile))));
            }

            SetInputTemplateFile(mdwCategories, modelDefinition, mdwDir);

            logHandler.LogReport();

            return new MdwFileDTO(modelDefinition, timeFrameData);
        }

        private static void ReadWaveBoundaries(WaveModelDefinition modelDefinition,
                                               IList<DelftIniCategory> mdwCategories,
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

            IEnumerable<DelftIniCategory> boundaryCategories = mdwCategories.GetAllByName(KnownWaveCategories.BoundaryCategory).ToArray();
            IDictionary<string, List<IFunction>> timeSeriesData = ReadBoundaryTimeSeriesData(mdwCategories, mdwDirPath);

            if (DomainWideBoundaryCategoryConverter.IsDomainWideBoundaryCategory(boundaryCategories))
            {
                DomainWideBoundaryCategoryConverter.Convert(boundaryContainer, boundaryCategories, mdwDirPath);
            }
            else
            {
                var boundariesConverter = new WaveBoundaryConverter(new ImportBoundaryConditionDataComponentFactory(new ForcingTypeDefinedParametersFactory()),
                                                                    new WaveBoundaryGeometricDefinitionFactory(boundaryContainer));
                IEnumerable<IWaveBoundary> waveBoundaries = boundariesConverter.Convert(boundaryCategories, timeSeriesData, mdwDirPath, logHandler);
                boundaryContainer.Boundaries.AddRange(waveBoundaries);
            }
        }

        private static IDictionary<string, List<IFunction>> ReadBoundaryTimeSeriesData(IEnumerable<DelftIniCategory> mdwCategories,
                                                                                       string mdwDirPath)
        {
            string relativeBcwFilePath = mdwCategories.GetByName(KnownWaveCategories.GeneralCategory)
                                                      .GetPropertyValue(KnownWaveProperties.TimeSeriesFile);

            return !string.IsNullOrEmpty(relativeBcwFilePath)
                       ? new BcwFile().Read(Path.Combine(mdwDirPath, relativeBcwFilePath))
                       : new Dictionary<string, List<IFunction>>();
        }

        /// <summary>
        /// Converting mdw categories to model definition properties.
        /// Second part is for linked properties, since the default value of a property can be dependent on another property value
        /// and this part will be used in situations if the property with multiple default values is missing in the mdw file.
        /// Based on the other property the correct one will be set. Otherwise the default value is based on the default value of
        /// the other property.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="mdwCategories">The delft ini categories from the mdw file.</param>
        /// <param name="logHandler">The log handler.</param>
        private static void ConvertMdwCategoriesToModelDefinitionProperties(WaveModelDefinition modelDefinition, IList<DelftIniCategory> mdwCategories, ILogHandler logHandler)
        {
            ConvertMdwCategoryProperties(modelDefinition, mdwCategories, logHandler);
            ConvertModelDefinitionProperties(modelDefinition, mdwCategories, logHandler);
        }

        private static void ConvertMdwCategoryProperties(WaveModelDefinition modelDefinition, IEnumerable<DelftIniCategory> mdwCategories, ILogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MdwFileBackwardsCompatibilityConfigurationValues());

            foreach (DelftIniCategory category in mdwCategories)
            {
                category.Name = GetCategoryName(category, backwardsCompatibilityHelper, logHandler);

                ModelPropertySchema<WaveModelPropertyDefinition> modelSchema = modelDefinition.ModelSchema;
                if (!modelSchema.ModelDefinitionCategory.ContainsKey(category.Name))
                {
                    continue;
                }

                ModelPropertyGroup definedCategory = modelSchema.ModelDefinitionCategory[category.Name];
                foreach (DelftIniProperty mdwProperty in category.Properties)
                {
                    if (IsObsoleteProperty(mdwProperty, backwardsCompatibilityHelper, logHandler))
                    {
                        continue;
                    }

                    string propertyName = GetPropertyName(mdwProperty, backwardsCompatibilityHelper, logHandler);

                    WaveModelPropertyDefinition definition = GetWaveModelPropertyDefinition(mdwProperty, modelSchema, category, definedCategory, propertyName);

                    modelDefinition.SetModelProperty(category.Name, propertyName, new WaveModelProperty(definition, mdwProperty.Value));
                }
            }
        }

        private static void ConvertModelDefinitionProperties(WaveModelDefinition modelDefinition, IEnumerable<DelftIniCategory> mdwCategories, ILogHandler logHandler)
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

                IEnumerable<DelftIniCategory> categoryOfDependentOnProperty = GetCategoriesWithPropertyName(mdwCategories, nameOfDependentOnProperty);
                IEnumerable<DelftIniCategory> categoryOfPropertyWithMultipleDefaultValues = GetCategoriesWithPropertyName(mdwCategories, nameOfPropertyWithMultipleDefaultValues);

                // Situation in which property with multiple default values is missing and corresponding default value will be set.
                if (categoryOfPropertyWithMultipleDefaultValues.Any() || !categoryOfDependentOnProperty.Any())
                {
                    continue;
                }

                logHandler.ReportWarningFormat(Resources.MdwFile_In_the_MDW_file_the_property__0__is_missing__Based_on_property__1__the_default_value_is_set,
                                               nameOfPropertyWithMultipleDefaultValues, nameOfDependentOnProperty);

                WaveModelProperty dependentOnProperty = modelDefinition.GetModelProperty(categoryOfDependentOnProperty.First().Name, nameOfDependentOnProperty);
                WaveModelProperty propertyWithMultipleDefaultValues = modelDefinition.GetModelProperty(categoryNameWithPropertyWithMultipleDefaultValues, nameOfPropertyWithMultipleDefaultValues);

                var index = (int)dependentOnProperty.Value;
                propertyWithMultipleDefaultValues.SetValueAsString(propertyWithMultipleDefaultValues.PropertyDefinition.MultipleDefaultValues[index]);
            }
        }

        private static string GetCategoryName(DelftIniCategory category, DelftIniBackwardsCompatibilityHelper backwardsCompatibilityHelper, ILogHandler logHandler)
        {
            return backwardsCompatibilityHelper.GetUpdatedCategoryName(category.Name, logHandler) ?? category.Name;
        }

        private static string GetPropertyName(DelftIniProperty mdwProperty, DelftIniBackwardsCompatibilityHelper backwardsCompatibilityHelper, ILogHandler logHandler)
        {
            return backwardsCompatibilityHelper.GetUpdatedPropertyName(mdwProperty.Name, logHandler) ?? mdwProperty.Name;
        }

        private static WaveModelPropertyDefinition GetWaveModelPropertyDefinition(DelftIniProperty mdwProperty, ModelPropertySchema<WaveModelPropertyDefinition> modelSchema, DelftIniCategory category, ModelPropertyGroup definedCategory, string propName)
        {
            return modelSchema.PropertyDefinitions.ContainsKey(propName.ToLower())
                       ? modelSchema.PropertyDefinitions[propName.ToLower()]
                       : CreateWaveModelPropertyDefinition(mdwProperty, category, definedCategory);
        }

        private static bool IsObsoleteProperty(DelftIniProperty mdwProperty, DelftIniBackwardsCompatibilityHelper backwardsCompatibilityHelper, ILogHandler logHandler)
        {
            if (!backwardsCompatibilityHelper.IsObsoletePropertyName(mdwProperty.Name))
            {
                return false;
            }

            logHandler.ReportWarningFormat(Common.Properties.Resources.Parameter__0__is_not_supported_by_our_computational_core_and_will_be_removed_from_your_input_file, mdwProperty.Name);
            return true;
        }

        private static WaveModelPropertyDefinition CreateWaveModelPropertyDefinition(DelftIniProperty mdwProperty, DelftIniCategory category, ModelPropertyGroup definedCategory)
        {
            return new WaveModelPropertyDefinition
            {
                Caption = mdwProperty.Name,
                DataType = typeof(string),
                FileCategoryName = category.Name,
                FilePropertyName = mdwProperty.Name,
                Category = definedCategory.Name,
                // default value as string should always be an empty string and not null.
                DefaultValueAsString = string.Empty
            };
        }

        private static IEnumerable<DelftIniCategory> GetCategoriesWithPropertyName(IEnumerable<DelftIniCategory> mdwCategories, string propertyName)
        {
            return mdwCategories.Where(mc => mc.Properties.Any(p => p.Name.Equals(propertyName))).ToList();
        }

        private ITimeFrameData CreateTimePointData(IEnumerable<DelftIniCategory> mdwCategories,
                                                       DateTime referenceDate,
                                                       ILogHandler logHandler)
        {
            var timePointData = new TimeFrameData
            {
                HydrodynamicsInputDataType = HydrodynamicsInputDataType.Constant,
                WindInputDataType = WindInputDataType.Constant
            };

            IList<DateTime> times = new List<DateTime>();
            List<DelftIniCategory> timePointCategories = mdwCategories.Where(c => c.Name == KnownWaveCategories.TimePointCategory).ToList();

            if (timePointCategories.Any(c => c.GetPropertyValue(KnownWaveProperties.WaterLevel) != null))
            {
                timePointData.HydrodynamicsInputDataType = HydrodynamicsInputDataType.TimeVarying;
            }

            if (timePointCategories.Any(c => c.GetPropertyValue(KnownWaveProperties.WindSpeed) != null))
            {
                timePointData.WindInputDataType = WindInputDataType.TimeVarying;
            }

            DelftIniCategory generalCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory);
            List<string> meteoFiles = generalCategory.GetPropertyValues(KnownWaveProperties.MeteoFile).ToList();

            if (meteoFiles.Any())
            {
                timePointData.WindInputDataType = WindInputDataType.FileBased;
                SetMeteoDataFromFiles(timePointData.WindFileData, meteoFiles, logHandler);
            }

            if (timePointData.HydrodynamicsInputDataType == HydrodynamicsInputDataType.Constant)
            {
                double waterLevel = double.Parse(generalCategory.GetPropertyValue(KnownWaveProperties.WaterLevel, "0.0"),
                                                 NumberStyles.Any, CultureInfo.InvariantCulture);
                double velocityX = double.Parse(generalCategory.GetPropertyValue(KnownWaveProperties.WaterVelocityX, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                double velocityY = double.Parse(generalCategory.GetPropertyValue(KnownWaveProperties.WaterVelocityY, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                timePointData.HydrodynamicsConstantData.WaterLevel = waterLevel;
                timePointData.HydrodynamicsConstantData.VelocityX = velocityX;
                timePointData.HydrodynamicsConstantData.VelocityY = velocityY;
            }

            if (timePointData.WindInputDataType == WindInputDataType.Constant)
            {
                double windSpeed = double.Parse(generalCategory.GetPropertyValue(KnownWaveProperties.WindSpeed, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                double windDirection = double.Parse(generalCategory.GetPropertyValue(KnownWaveProperties.WindDirection, "0.0"),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture);
                timePointData.WindConstantData.Speed = windSpeed;
                timePointData.WindConstantData.Direction = windDirection;
            }

            foreach (DelftIniCategory timePoint in timePointCategories)
            {
                DateTime time = referenceDate.AddMinutes(double.Parse(timePoint.GetPropertyValue(KnownWaveProperties.Time, "0.0"),
                                                                      NumberStyles.Any,
                                                                      CultureInfo.InvariantCulture));
                times.Add(time);

                if (!double.TryParse(timePoint.GetPropertyValue(KnownWaveProperties.WaterLevel), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double waterLevel))
                {
                    waterLevel = (double)timePointData.TimeVaryingData.Components[0].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValue(KnownWaveProperties.WaterVelocityX), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double velocityX))
                {
                    velocityX = (double)timePointData.TimeVaryingData.Components[1].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValue(KnownWaveProperties.WaterVelocityY), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double velocityY))
                {
                    velocityY = (double)timePointData.TimeVaryingData.Components[2].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValue(KnownWaveProperties.WindSpeed), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out double windSpeed))
                {
                    windSpeed = (double)timePointData.TimeVaryingData.Components[3].DefaultValue;
                }

                if (!double.TryParse(timePoint.GetPropertyValue(KnownWaveProperties.WindDirection), NumberStyles.Any,
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

        private IEnumerable<WaveObstacle> CreateObstacleData(DelftIniCategory generalCategory,
                                                             WaveModelDefinition modelDefinition)
        {
            string obstacleFile = generalCategory.GetPropertyValue(KnownWaveProperties.ObstacleFile, string.Empty);
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

            var delftIniReader = new DelftIniReader();
            IList<DelftIniCategory> obtCategories;
            using (var fileStream = new FileStream(obstacleFilePath, FileMode.Open, FileAccess.Read))
            {
                obtCategories = delftIniReader.ReadDelftIniFile(fileStream, obstacleFilePath);
            }

            DelftIniCategory fileInfo = obtCategories.First(c => c.Name == KnownWaveObsCategories.ObstacleFileInformation);
            string polylineFileName = fileInfo.GetPropertyValue(KnownWaveObsProperties.PolylineFile);
            string geometryFilePath = Path.Combine(mdwDirectory, polylineFileName);
            if (!File.Exists(geometryFilePath))
            {
                log.ErrorFormat(Resources.MdwFile_Obstacle_polyline_file__0__does_not_exist, geometryFilePath);
                yield break;
            }

            modelDefinition.ObstaclePolylineFile = polylineFileName;

            var pliFile = new PliFile<Feature2D>();
            Dictionary<string, Feature2D> features = pliFile.Read(geometryFilePath).ToDictionary(f => f.Name);

            foreach (DelftIniCategory obstacle in obtCategories.Where(o => o.Name == KnownWaveCategories.ObstacleCategory))
            {
                string name = obstacle.GetPropertyValue(MdwFileConstants.ObstaclePropertyName, "default name");
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

                obs.Type = obstacle.GetPropertyValue(MdwFileConstants.ObstaclePropertyType) == "dam"
                               ? ObstacleType.Dam
                               : ObstacleType.Sheet;

                string reflectionType = obstacle.GetPropertyValue(MdwFileConstants.ObstaclePropertyReflections);
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

        private static double GetObstaclePropertyAndLogIfFails(DelftIniCategory obstacle, string fileName, string property,
                                                               double defaultValue)
        {
            string input = obstacle.GetPropertyValue(property);
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

        private static void SetInputTemplateFile(IEnumerable<DelftIniCategory> mdwCategories, WaveModelDefinition modelDefinition, string mdwDir)
        {
            string inputTemplateFile = mdwCategories.GetByName(KnownWaveCategories.GeneralCategory)
                                                    .GetPropertyValue(KnownWaveProperties.InputTemplateFile);

            if (!string.IsNullOrEmpty(inputTemplateFile))
            {
                modelDefinition.InputTemplateFilePath = Path.Combine(mdwDir, inputTemplateFile);
            }
        }
    }
}