using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DHYDRO.Common.Logging;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Provides MDW file writer functionality.
    /// </summary>
    public partial class MdwFile
    {
        /// <summary>
        /// Saves the MDW file to the specified location.
        /// </summary>
        /// <param name="mdwTargetFilePath">The location to write the MDW file to.</param>
        /// <param name="mdwFileDTO">The MDW file contents to write.</param>
        /// <param name="switchTo">Whether use the specified location as the current file path.</param>
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
            mdwFileMerger.Source = mdwCategories;

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

            // category & property id's must be unique
            DelftIniCategory.UpdateIdentifiers(mdwFileMerger.Source.ToArray());
            DelftIniCategory.UpdateIdentifiers(mdwFileMerger.Target.ToArray());
            
            // merge mdw
            bool isMerged = mdwFileMerger.TryMerge(out IEnumerable<DelftIniCategory> merged);

            // write mdw
            new DelftIniWriter().WriteDelftIniFile(isMerged ? merged : mdwCategories, mdwTargetFilePath);

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

                    obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyName, obstacle.Name);
                    obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyType, obstacle.Type.ToString().ToLower());
                    if (obstacle.Type == ObstacleType.Sheet)
                    {
                        obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyTransmissionCoefficient,
                                                     obstacle.TransmissionCoefficient);
                    }
                    else
                    {
                        obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyHeight, obstacle.Height);
                        obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyAlpha, obstacle.Alpha);
                        obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyBeta, obstacle.Beta);
                    }

                    obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyReflections,
                                                 obstacle.ReflectionType.ToString().ToLower());
                    if (obstacle.ReflectionType != ReflectionType.No)
                    {
                        obstacleCategory.AddProperty(MdwFileConstants.ObstaclePropertyReflectionCoefficient,
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
    }
}