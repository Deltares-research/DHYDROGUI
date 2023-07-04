using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.Common.Logging;
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
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    public class MdwFile : NGHSFileBase
    {
        private const string waveObstaclePropertyName = "Name";
        private const string waveObstaclePropertyType = "Type";
        private const string waveObstaclePropertyTransmissionCoefficient = "TransmCoef";
        private const double waveObstacleDefaultValueTransmissionCoefficient = 0.0;
        private const string waveObstaclePropertyHeight = "Height";
        private const double waveObstacleDefaultValueHeight = 0.0;
        private const string waveObstaclePropertyAlpha = "Alpha";
        private const double waveObstacleDefaultValueAlpha = 0.0;
        private const string waveObstaclePropertyBeta = "Beta";
        private const double waveObstacleDefaultValueBeta = 0.0;
        private const string waveObstaclePropertyReflections = "Reflections";
        private const string waveObstaclePropertyReflectionCoefficient = "ReflecCoef";
        private const double waveObstacleDefaultValueReflectionCoefficient = 0.0;

        private static readonly ILog log = LogManager.GetLogger(typeof(MdwFile));

        /// <summary>
        /// These mdw categories can have multiplicity greater than 1 (or gui only),
        /// excluded them from the generic property treatment..
        /// </summary>
        public static IList<string> ExcludedCategories { get; } = new List<string>
        {
            KnownWaveCategories.TimePointCategory,
            KnownWaveCategories.DomainCategory,
            KnownWaveCategories.BoundaryCategory,
            KnownWaveCategories.GuiOnlyCategory
        };

        public string MdwFilePath { get; set; }

        public void SaveTo(string mdwTargetFilePath,
                           MdwFileDTO mdwFileDTO,
                           bool switchTo)
        {
            var logHandler = new LogHandler(Resources.MdwFile_SaveTo_exporting_the_D_Waves_model, log);
            var filesManager = new FilesManager();

            WaveModelDefinition modelDefinition = mdwFileDTO.WaveModelDefinition;

            string modelName = Path.GetFileNameWithoutExtension(mdwTargetFilePath);
            string targetDir = Path.GetDirectoryName(mdwTargetFilePath);

            if (targetDir != string.Empty && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            WriteObstacles(modelDefinition, modelName, targetDir);

            SetConstantWindProperties(modelDefinition,
                                      mdwFileDTO.TimeFrameData);
            SetConstantHydrodynamicsProperties(modelDefinition,
                                               mdwFileDTO.TimeFrameData);

            string locationFileName = modelDefinition
                                      .GetModelProperty(KnownWaveCategories.OutputCategory,
                                                        KnownWaveProperties.LocationFile)
                                      .GetValueAsString();

            if (modelDefinition.FeatureContainer.ObservationPoints.Any())
            {
                if (string.IsNullOrEmpty(locationFileName))
                {
                    locationFileName = modelName + ".loc";
                    modelDefinition
                        .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                        .SetValueAsString(locationFileName);
                }

                new ObsFile<Feature2DPoint>().Write(Path.Combine(targetDir, locationFileName),
                                                    modelDefinition.FeatureContainer.ObservationPoints, false);
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                               .SetValueAsString(string.Empty);
            }

            IEnumerable<DelftIniCategory> boundaryCategories = MdwBoundaryCategoriesCreator.CreateCategories(modelDefinition.BoundaryContainer,
                                                                                                             filesManager);

            WriteTimeSeriesFileForBoundaries(modelName, modelDefinition, targetDir);

            List<DelftIniCategory> mdwCategories = GroupPropertiesByMdwCategory(modelDefinition);
            CreateTimePointCategories(mdwFileDTO.TimeFrameData,
                                      mdwCategories,
                                      modelDefinition.ModelReferenceDateTime);
            mdwCategories.AddRange(boundaryCategories);

            if (MdwFilePath != null)
            {
                string sourceDir = Path.GetDirectoryName(MdwFilePath);

                // Not synchronized with modeldefintionproperties, since property is missing in csv file
                SaveMeteoFile(mdwFileDTO.TimeFrameData, mdwCategories, sourceDir, targetDir);

                // domain(s)
                IList<IWaveDomainData> allDomains = WaveDomainHelper.GetAllDomains(modelDefinition.OuterDomain);
                AddDomainCategories(allDomains, ref mdwCategories);
                allDomains.ForEach(d => CopyMeteoFilesTo(d.MeteoData, targetDir, switchTo));

                // grid is not edited within DS, so always in sync
                // and a plain file copy suffices
                CopyGridFiles(allDomains.Select(d => d.GridFileName), sourceDir, targetDir);
            }

            HandleInputTemplateFile(modelDefinition, filesManager, mdwCategories);

            DelftIniCategory outputCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.OutputCategory);
            outputCategory.SetProperty(KnownWaveProperties.COMFile, modelDefinition.CommunicationsFilePath.Replace('\\', '/'));

            // Not synchronized with modeldefintionproperties, since property is missing in csv file

            string curvesFileName = outputCategory.GetPropertyValue(KnownWaveProperties.CurveFile);
            if (modelDefinition.FeatureContainer.ObservationCrossSections.Any())
            {
                if (string.IsNullOrEmpty(curvesFileName))
                {
                    curvesFileName = modelName + ".cur";
                    outputCategory.SetProperty(KnownWaveProperties.CurveFile, curvesFileName);
                }

                new PliFile<Feature2D>().Write(Path.Combine(targetDir, curvesFileName),
                                               modelDefinition.FeatureContainer.ObservationCrossSections);
            }
            else
            {
                outputCategory.SetProperty(KnownWaveProperties.CurveFile, string.Empty);
            }

            // write mdw
            new DelftIniWriter().WriteDelftIniFile(mdwCategories, mdwTargetFilePath);

            // switch
            if (switchTo)
            {
                MdwFilePath = mdwTargetFilePath;
            }

            if (!string.IsNullOrEmpty(targetDir))
            {
                filesManager.CopyTo(targetDir, logHandler, switchTo);
            }

            logHandler.LogReport();
        }

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

        private static void SaveMeteoFile(ITimeFrameData timeFrameData,
                                          IEnumerable<DelftIniCategory> mdwCategories,
                                          string sourceDir,
                                          string targetDir)
        {
            DelftIniCategory generalCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory);

            generalCategory.RemoveAllPropertiesWhere(prop => prop.Name == KnownWaveProperties.MeteoFile);

            if (timeFrameData.WindInputDataType == WindInputDataType.FileBased)
            {
                List<string> meteoFiles = GetMeteoFiles(timeFrameData.WindFileData);
                meteoFiles.ForEach((mf, i) =>
                {
                    generalCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoFiles[i]);
                    CopyModelFile(meteoFiles[i], sourceDir, targetDir);
                });
            }
            else
            {
                generalCategory.SetProperty(KnownWaveProperties.MeteoFile, string.Empty);
            }
        }

        private static void SetConstantWindProperties(WaveModelDefinition modelDefinition,
                                                      ITimeFrameData timeFrameData)
        {
            if (timeFrameData.WindInputDataType == WindInputDataType.Constant)
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                               .Value = timeFrameData.WindConstantData.Speed;
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                               .Value = timeFrameData.WindConstantData.Direction;
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                               .SetValueAsString("0");
            }
        }

        private static void SetConstantHydrodynamicsProperties(WaveModelDefinition modelDefinition,
                                                               ITimeFrameData timeFrameData)
        {
            if (timeFrameData.HydrodynamicsInputDataType == HydrodynamicsInputDataType.Constant)
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                               .Value = timeFrameData.HydrodynamicsConstantData.WaterLevel;
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                               .Value = timeFrameData.HydrodynamicsConstantData.VelocityX;
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityY)
                               .Value = timeFrameData.HydrodynamicsConstantData.VelocityY;
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityY)
                               .SetValueAsString("0");
            }
        }

        private static List<DelftIniCategory> GroupPropertiesByMdwCategory(WaveModelDefinition modelDefinition)
        {
            var mdwCategories = new List<DelftIniCategory>();
            IEnumerable<IGrouping<string, WaveModelProperty>> groupedProperties = modelDefinition.Properties.GroupBy(p => p.PropertyDefinition.FileCategoryName);
            foreach (IGrouping<string, WaveModelProperty> grouping in groupedProperties)
            {
                string mdwCategoryName = grouping.Key;
                if (ExcludedCategories.Contains(mdwCategoryName))
                {
                    continue;
                }

                var mdwGroup = new DelftIniCategory(mdwCategoryName);
                foreach (WaveModelProperty property in grouping)
                {
                    string name = property.PropertyDefinition.FilePropertyName;
                    string value = property.GetValueAsString();
                    mdwGroup.AddProperty(name, value);
                }

                mdwCategories.Add(mdwGroup);
            }

            return mdwCategories;
        }

        private static List<string> GetMeteoFiles(WaveMeteoData meteoData)
        {
            var meteoFiles = new List<string>();
            if (meteoData.FileType == WindDefinitionType.WindXY)
            {
                meteoFiles.Add(meteoData.XYVectorFileName);
            }

            if (meteoData.FileType == WindDefinitionType.WindXWindY)
            {
                meteoFiles.Add(meteoData.XComponentFileName);
                meteoFiles.Add(meteoData.YComponentFileName);
            }

            if (meteoData.FileType == WindDefinitionType.SpiderWebGrid || meteoData.HasSpiderWeb)
            {
                meteoFiles.Add(meteoData.SpiderWebFileName);
            }

            return meteoFiles;
        }

        private static void CopyMeteoFilesTo(WaveMeteoData meteoData, string targetDirPath, bool switchTo)
        {
            if (meteoData.FileType == WindDefinitionType.WindXY)
            {
                string targetFilePath = CopyFileTo(meteoData.XYVectorFilePath, targetDirPath);
                if (switchTo)
                {
                    meteoData.XYVectorFilePath = targetFilePath;
                }
            }

            if (meteoData.FileType == WindDefinitionType.WindXWindY)
            {
                string targetXComponentFilePath = CopyFileTo(meteoData.XComponentFilePath, targetDirPath);
                string targetYComponentFilePath = CopyFileTo(meteoData.YComponentFilePath, targetDirPath);
                if (switchTo)
                {
                    meteoData.XComponentFilePath = targetXComponentFilePath;
                    meteoData.YComponentFilePath = targetYComponentFilePath;
                }
            }

            if (meteoData.FileType == WindDefinitionType.SpiderWebGrid || meteoData.HasSpiderWeb)
            {
                string targetFilePath = CopyFileTo(meteoData.SpiderWebFilePath, targetDirPath);
                if (switchTo)
                {
                    meteoData.SpiderWebFilePath = targetFilePath;
                }
            }
        }

        private static string CopyFileTo(string sourceFilePath, string targetDirPath)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                return null;
            }

            string targetFilePath = Path.Combine(targetDirPath, Path.GetFileName(sourceFilePath));
            if (File.Exists(sourceFilePath))
            {
                if (!File.Exists(targetFilePath))
                {
                    File.Copy(sourceFilePath, targetFilePath);
                }

                return targetFilePath;
            }

            return null;
        }

        private static void CopyGridFiles(IEnumerable<string> gridFilePaths, string sourceDir, string targetDir)
        {
            foreach (string filename in gridFilePaths)
            {
                if (sourceDir != null && File.Exists(Path.Combine(sourceDir, filename)))
                {
                    CopyModelFile(filename, sourceDir, targetDir);
                }
            }
        }

        private static void WriteTimeSeriesFileForBoundaries(string modelName, WaveModelDefinition modelDefinition, string targetFile)
        {
            IEventedList<IWaveBoundary> boundaries = modelDefinition.BoundaryContainer.Boundaries;

            var allTimeSeriesPerBoundary = new Dictionary<string, List<IFunction>>();

            foreach (IWaveBoundary boundary in boundaries)
            {
                List<IFunction> timeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(boundary.ConditionDefinition.DataComponent);

                if (timeSeries.Any())
                {
                    // update refdate before writing
                    timeSeries.ForEach(f => f.Attributes[BcwFile.RefDateAttributeName] = modelDefinition.ModelReferenceDateTime.ToString(BcwFile.DateFormatString));
                    allTimeSeriesPerBoundary.Add(boundary.Name, timeSeries);
                }
            }

            // write bcw file                                    
            if (allTimeSeriesPerBoundary.Any())
            {
                string tSeriesFile = modelName + ".bcw";
                modelDefinition
                    .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeSeriesFile)
                    .SetValueAsString(tSeriesFile);
                new BcwFile().Write(allTimeSeriesPerBoundary, Path.Combine(targetFile, tSeriesFile));
            }
            else
            {
                modelDefinition
                    .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeSeriesFile)
                    .SetValueAsString(string.Empty);
            }
        }

        private static void WriteObstacles(WaveModelDefinition modelDefinition, string modelName, string targetDir)
        {
            WaveModelProperty propertyObstacleFile = modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                                                      KnownWaveProperties.ObstacleFile);

            if (modelDefinition.FeatureContainer.Obstacles.Any())
            {
                var obtCategories = new List<DelftIniCategory>();

                string targetFile = propertyObstacleFile.GetValueAsString();
                if (targetFile == string.Empty)
                {
                    targetFile = modelName + ".obt";
                    propertyObstacleFile.SetValueAsString(targetFile);
                }

                string geometryFile = modelDefinition.ObstaclePolylineFile ?? "";
                if (geometryFile == string.Empty)
                {
                    geometryFile = modelName + ".pol";
                }

                IEventedList<WaveObstacle> features = modelDefinition.FeatureContainer.Obstacles;
                new PliFile<Feature2D>().Write(Path.Combine(targetDir, geometryFile), features);

                var fileInfo = new DelftIniCategory(KnownWaveObsCategories.ObstacleFileInformation);
                fileInfo.AddProperty("FileVersion", "02.00");
                fileInfo.AddProperty(KnownWaveObsProperties.PolylineFile, geometryFile);
                obtCategories.Add(fileInfo);

                foreach (WaveObstacle obstacle in modelDefinition.FeatureContainer.Obstacles.OfType<WaveObstacle>())
                {
                    var obstacleCategory = new DelftIniCategory(KnownWaveCategories.ObstacleCategory);

                    obstacleCategory.AddProperty(waveObstaclePropertyName, obstacle.Name);
                    obstacleCategory.AddProperty(waveObstaclePropertyType, obstacle.Type.ToString().ToLower());
                    if (obstacle.Type == ObstacleType.Sheet)
                    {
                        obstacleCategory.AddProperty(waveObstaclePropertyTransmissionCoefficient,
                                                     obstacle.TransmissionCoefficient);
                    }
                    else
                    {
                        obstacleCategory.AddProperty(waveObstaclePropertyHeight, obstacle.Height);
                        obstacleCategory.AddProperty(waveObstaclePropertyAlpha, obstacle.Alpha);
                        obstacleCategory.AddProperty(waveObstaclePropertyBeta, obstacle.Beta);
                    }

                    obstacleCategory.AddProperty(waveObstaclePropertyReflections,
                                                 obstacle.ReflectionType.ToString().ToLower());
                    if (obstacle.ReflectionType != ReflectionType.No)
                    {
                        obstacleCategory.AddProperty(waveObstaclePropertyReflectionCoefficient,
                                                     obstacle.ReflectionCoefficient);
                    }

                    obtCategories.Add(obstacleCategory);
                }

                new DelftIniWriter().WriteDelftIniFile(obtCategories, Path.Combine(targetDir, targetFile));
            }
            else
            {
                propertyObstacleFile.SetValueAsString(string.Empty);
            }
        }

        private static void AddDomainCategories(IList<IWaveDomainData> allDomains, ref List<DelftIniCategory> mdwCategories)
        {
            foreach (IWaveDomainData domain in allDomains)
            {
                var domainCategory = new DelftIniCategory(KnownWaveCategories.DomainCategory);
                domainCategory.AddProperty(KnownWaveProperties.Grid, domain.GridFileName);
                domainCategory.AddProperty(KnownWaveProperties.BedLevelGrid, domain.BedLevelGridFileName);
                domainCategory.AddProperty(KnownWaveProperties.BedLevel, domain.BedLevelFileName);

                if (!domain.SpectralDomainData.UseDefaultDirectionalSpace)
                {
                    domainCategory.AddProperty(KnownWaveProperties.DirectionalSpaceType,
                                               domain.SpectralDomainData.DirectionalSpaceType.GetDescription()
                                                     .ToLower());
                    domainCategory.AddProperty(KnownWaveProperties.NumberOfDirections, domain.SpectralDomainData.NDir);
                    domainCategory.AddProperty(KnownWaveProperties.StartDirection, domain.SpectralDomainData.StartDir);
                    domainCategory.AddProperty(KnownWaveProperties.EndDirection, domain.SpectralDomainData.EndDir);
                }

                if (!domain.SpectralDomainData.UseDefaultFrequencySpace)
                {
                    domainCategory.AddProperty(KnownWaveProperties.NumberOfFrequencies, domain.SpectralDomainData.NFreq);
                    domainCategory.AddProperty(KnownWaveProperties.StartFrequency, domain.SpectralDomainData.FreqMin);
                    domainCategory.AddProperty(KnownWaveProperties.EndFrequency, domain.SpectralDomainData.FreqMax);
                }

                if (!domain.UseGlobalMeteoData)
                {
                    List<string> meteoFiles = GetMeteoFiles(domain.MeteoData);
                    meteoFiles.ForEach((mf, i) => domainCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoFiles[i]));
                }

                if (!domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings)
                {
                    domainCategory.AddProperty(KnownWaveProperties.FlowBedLevelUsage, (int)domain.HydroFromFlowData.BedLevelUsage);
                    domainCategory.AddProperty(KnownWaveProperties.FlowWaterLevelUsage, (int)domain.HydroFromFlowData.WaterLevelUsage);
                    domainCategory.AddProperty(KnownWaveProperties.FlowVelocityUsage, (int)domain.HydroFromFlowData.VelocityUsage);
                    domainCategory.AddProperty(KnownWaveProperties.FlowVelocityUsageType, domain.HydroFromFlowData.VelocityUsageType.GetDescription());
                    domainCategory.AddProperty(KnownWaveProperties.FlowWindUsage, (int)domain.HydroFromFlowData.WindUsage);
                }

                if (domain.SuperDomain != null)
                {
                    // position in list == position in mdw file == domain number:
                    int index = allDomains.IndexOf(domain.SuperDomain);
                    domainCategory.AddProperty("NestedInDomain", index + 1);
                }

                domainCategory.AddProperty("Output", domain.Output.ToString().ToLower());
                mdwCategories.Add(domainCategory);
            }
        }

        private static void CopyModelFile(string name, string sourceDir, string targetDir)
        {
            string originalFilePath = Path.Combine(sourceDir, name);
            string targetFilePath = Path.Combine(targetDir, name);
            if (Path.GetFullPath(originalFilePath) != Path.GetFullPath(targetFilePath))
            {
                try
                {
                    File.Copy(originalFilePath, targetFilePath, true);
                }
                catch
                {
                    log.ErrorFormat("Failed to copy {0} to {1}", originalFilePath, targetFilePath);
                    throw;
                }
            }
        }

        private static void CreateTimePointCategories(ITimeFrameData timeFrameData,
                                                      List<DelftIniCategory> mdwCategories,
                                                      DateTime modelReferenceDateTime)
        {
            var categories = new List<DelftIniCategory>();

            IMultiDimensionalArray<DateTime> times =
                timeFrameData.TimeVaryingData.Arguments[0].GetValues<DateTime>();

            foreach (DateTime time in times)
            {
                var timeCategory = new DelftIniCategory(KnownWaveCategories.TimePointCategory);
                timeCategory.AddProperty(KnownWaveProperties.Time, (time - modelReferenceDateTime).TotalMinutes);

                double[] values = timeFrameData.TimeVaryingData.GetAllComponentValues(time).Cast<double>().ToArray();

                if (timeFrameData.HydrodynamicsInputDataType == HydrodynamicsInputDataType.TimeVarying)
                {
                    timeCategory.AddProperty(KnownWaveProperties.WaterLevel, values[0]);
                    timeCategory.AddProperty(KnownWaveProperties.WaterVelocityX, values[1]);
                    timeCategory.AddProperty(KnownWaveProperties.WaterVelocityY, values[2]);
                }

                if (timeFrameData.WindInputDataType == WindInputDataType.TimeVarying)
                {
                    timeCategory.AddProperty(KnownWaveProperties.WindSpeed, values[3]);
                    timeCategory.AddProperty(KnownWaveProperties.WindDirection, values[4]);
                }

                categories.Add(timeCategory);
            }

            DelftIniCategory generalCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory);
            int afterGeneralIndex = mdwCategories.IndexOf(generalCategory);
            mdwCategories.InsertRange(afterGeneralIndex + 1, categories);
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
                string name = obstacle.GetPropertyValue(waveObstaclePropertyName, "default name");
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

                obs.Type = obstacle.GetPropertyValue(waveObstaclePropertyType) == "dam"
                               ? ObstacleType.Dam
                               : ObstacleType.Sheet;

                string reflectionType = obstacle.GetPropertyValue(waveObstaclePropertyReflections);
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

                obs.TransmissionCoefficient = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, waveObstaclePropertyTransmissionCoefficient,
                                                                               waveObstacleDefaultValueTransmissionCoefficient);
                obs.Height = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, waveObstaclePropertyHeight,
                                                              waveObstacleDefaultValueHeight);
                obs.Alpha = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, waveObstaclePropertyAlpha,
                                                             waveObstacleDefaultValueAlpha);
                obs.Beta = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, waveObstaclePropertyBeta,
                                                            waveObstacleDefaultValueBeta);
                obs.ReflectionCoefficient = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, waveObstaclePropertyReflectionCoefficient,
                                                                             waveObstacleDefaultValueReflectionCoefficient);

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

        private static void HandleInputTemplateFile(WaveModelDefinition modelDefinition, IFilesManager filesManager, List<DelftIniCategory> mdwCategories)
        {
            if (string.IsNullOrEmpty(modelDefinition.InputTemplateFilePath))
            {
                return;
            }

            DelftIniCategory generalCategory = mdwCategories.GetByName(KnownWaveCategories.GeneralCategory);
            string fileName = Path.GetFileName(modelDefinition.InputTemplateFilePath);
            generalCategory.SetProperty(KnownWaveProperties.InputTemplateFile, fileName);

            filesManager.Add(modelDefinition.InputTemplateFilePath, s => modelDefinition.InputTemplateFilePath = s);
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