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
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DHYDRO.Common.IO.Ini;
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
                                      .GetModelProperty(KnownWaveSections.OutputSection,
                                                        KnownWaveProperties.LocationFile)
                                      .GetValueAsString();

            if (modelDefinition.FeatureContainer.ObservationPoints.Any())
            {
                if (string.IsNullOrEmpty(locationFileName))
                {
                    locationFileName = modelName + ".loc";
                    modelDefinition
                        .GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.LocationFile)
                        .SetValueAsString(locationFileName);
                }

                new ObsFile<Feature2DPoint>().Write(Path.Combine(targetDir, locationFileName),
                                                    modelDefinition.FeatureContainer.ObservationPoints, false);
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.LocationFile)
                               .SetValueAsString(string.Empty);
            }

            IEnumerable<IniSection> boundarySections = MdwBoundarySectionsCreator.CreateSections(modelDefinition.BoundaryContainer, filesManager);

            WriteTimeSeriesFileForBoundaries(modelName, modelDefinition, targetDir);
            
            List<IniSection> mdwSections = GroupPropertiesByMdwSection(modelDefinition);
            CreateTimePointSections(mdwFileDTO.TimeFrameData, mdwSections, modelDefinition.ModelReferenceDateTime);
            mdwSections.AddRange(boundarySections);

            if (MdwFilePath != null)
            {
                string sourceDir = Path.GetDirectoryName(MdwFilePath);

                // Not synchronized with modeldefintionproperties, since property is missing in csv file
                SaveMeteoFile(mdwFileDTO.TimeFrameData, mdwSections, sourceDir, targetDir);

                // domain(s)
                IList<IWaveDomainData> allDomains = WaveDomainHelper.GetAllDomains(modelDefinition.OuterDomain);
                AddDomainSections(allDomains, ref mdwSections);
                allDomains.ForEach(d => CopyMeteoFilesTo(d.MeteoData, targetDir, switchTo));

                // grid is not edited within DS, so always in sync
                // and a plain file copy suffices
                CopyGridFiles(allDomains.Select(d => d.GridFileName), sourceDir, targetDir);
            }

            HandleInputTemplateFile(modelDefinition, filesManager, mdwSections);

            IniSection outputSection = mdwSections.First(c => c.Name == KnownWaveSections.OutputSection);
            outputSection.AddOrUpdateProperty(KnownWaveProperties.COMFile, modelDefinition.CommunicationsFilePath.Replace('\\', '/'));

            // Not synchronized with modeldefintionproperties, since property is missing in csv file

            string curvesFileName = outputSection.GetPropertyValueOrDefault(KnownWaveProperties.CurveFile);
            if (modelDefinition.FeatureContainer.ObservationCrossSections.Any())
            {
                if (string.IsNullOrEmpty(curvesFileName))
                {
                    curvesFileName = modelName + ".cur";
                    outputSection.AddOrUpdateProperty(KnownWaveProperties.CurveFile, curvesFileName);
                }

                new PliFile<Feature2D>().Write(Path.Combine(targetDir, curvesFileName),
                                               modelDefinition.FeatureContainer.ObservationCrossSections);
            }
            else
            {
                outputSection.AddOrUpdateProperty(KnownWaveProperties.CurveFile, string.Empty);
            }

            var iniData = new IniData();
            iniData.AddMultipleSections(mdwSections);
            
            // merge mdw
            mdwFileMerger.Modified = iniData;
            IniData merged = mdwFileMerger.Merge();
            
            // write mdw
            new IniWriter().WriteIniFile(merged, mdwTargetFilePath);

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
            WaveModelProperty propertyObstacleFile = modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection,
                                                                                      KnownWaveProperties.ObstacleFile);

            if (modelDefinition.FeatureContainer.Obstacles.Any())
            {
                var obtSections = new List<IniSection>();

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

                var fileInfo = new IniSection(KnownWaveObsSections.ObstacleFileInformation);
                fileInfo.AddProperty("FileVersion", "02.00");
                fileInfo.AddProperty(KnownWaveObsProperties.PolylineFile, geometryFile);
                obtSections.Add(fileInfo);

                foreach (WaveObstacle obstacle in modelDefinition.FeatureContainer.Obstacles.OfType<WaveObstacle>())
                {
                    var obstacleSection = new IniSection(KnownWaveSections.ObstacleSection);

                    obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyName, obstacle.Name);
                    obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyType, obstacle.Type.ToString().ToLower());
                    if (obstacle.Type == ObstacleType.Sheet)
                    {
                        obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyTransmissionCoefficient,
                                                     obstacle.TransmissionCoefficient);
                    }
                    else
                    {
                        obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyHeight, obstacle.Height);
                        obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyAlpha, obstacle.Alpha);
                        obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyBeta, obstacle.Beta);
                    }

                    obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyReflections,
                                                 obstacle.ReflectionType.ToString().ToLower());
                    if (obstacle.ReflectionType != ReflectionType.No)
                    {
                        obstacleSection.AddProperty(MdwFileConstants.ObstaclePropertyReflectionCoefficient,
                                                     obstacle.ReflectionCoefficient);
                    }

                    obtSections.Add(obstacleSection);
                }

                var iniData = new IniData();
                iniData.AddMultipleSections(obtSections);
                
                new IniWriter().WriteIniFile(iniData, Path.Combine(targetDir, targetFile));
            }
            else
            {
                propertyObstacleFile.SetValueAsString(string.Empty);
            }
        }

        private static void AddDomainSections(IList<IWaveDomainData> allDomains, ref List<IniSection> mdwSections)
        {
            foreach (IWaveDomainData domain in allDomains)
            {
                var domainSection = new IniSection(KnownWaveSections.DomainSection);
                domainSection.AddProperty(KnownWaveProperties.Grid, domain.GridFileName);
                domainSection.AddProperty(KnownWaveProperties.BedLevelGrid, domain.BedLevelGridFileName);
                domainSection.AddProperty(KnownWaveProperties.BedLevel, domain.BedLevelFileName);

                if (!domain.SpectralDomainData.UseDefaultDirectionalSpace)
                {
                    domainSection.AddProperty(KnownWaveProperties.DirectionalSpaceType,
                                               domain.SpectralDomainData.DirectionalSpaceType.GetDescription()
                                                     .ToLower());
                    domainSection.AddProperty(KnownWaveProperties.NumberOfDirections, domain.SpectralDomainData.NDir);
                    domainSection.AddProperty(KnownWaveProperties.StartDirection, domain.SpectralDomainData.StartDir);
                    domainSection.AddProperty(KnownWaveProperties.EndDirection, domain.SpectralDomainData.EndDir);
                }

                if (!domain.SpectralDomainData.UseDefaultFrequencySpace)
                {
                    domainSection.AddProperty(KnownWaveProperties.NumberOfFrequencies, domain.SpectralDomainData.NFreq);
                    domainSection.AddProperty(KnownWaveProperties.StartFrequency, domain.SpectralDomainData.FreqMin);
                    domainSection.AddProperty(KnownWaveProperties.EndFrequency, domain.SpectralDomainData.FreqMax);
                }

                if (!domain.UseGlobalMeteoData)
                {
                    List<string> meteoFiles = GetMeteoFiles(domain.MeteoData);
                    meteoFiles.ForEach((mf, i) => domainSection.AddProperty(KnownWaveProperties.MeteoFile, meteoFiles[i]));
                }

                if (!domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings)
                {
                    domainSection.AddProperty(KnownWaveProperties.FlowBedLevelUsage, (int)domain.HydroFromFlowData.BedLevelUsage);
                    domainSection.AddProperty(KnownWaveProperties.FlowWaterLevelUsage, (int)domain.HydroFromFlowData.WaterLevelUsage);
                    domainSection.AddProperty(KnownWaveProperties.FlowVelocityUsage, (int)domain.HydroFromFlowData.VelocityUsage);
                    domainSection.AddProperty(KnownWaveProperties.FlowVelocityUsageType, domain.HydroFromFlowData.VelocityUsageType.GetDescription());
                    domainSection.AddProperty(KnownWaveProperties.FlowWindUsage, (int)domain.HydroFromFlowData.WindUsage);
                }

                if (domain.SuperDomain != null)
                {
                    // position in list == position in mdw file == domain number:
                    int index = allDomains.IndexOf(domain.SuperDomain);
                    domainSection.AddProperty("NestedInDomain", index + 1);
                }

                domainSection.AddProperty("Output", domain.Output.ToString().ToLower());
                mdwSections.Add(domainSection);
            }
        }
        
        private static void SetConstantWindProperties(WaveModelDefinition modelDefinition,
                                                      ITimeFrameData timeFrameData)
        {
            if (timeFrameData.WindInputDataType == WindInputDataType.Constant)
            {
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WindSpeed)
                               .Value = timeFrameData.WindConstantData.Speed;
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WindDirection)
                               .Value = timeFrameData.WindConstantData.Direction;
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WindSpeed)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WindDirection)
                               .SetValueAsString("0");
            }
        }

        private static void SetConstantHydrodynamicsProperties(WaveModelDefinition modelDefinition,
                                                               ITimeFrameData timeFrameData)
        {
            if (timeFrameData.HydrodynamicsInputDataType == HydrodynamicsInputDataType.Constant)
            {
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WaterLevel)
                               .Value = timeFrameData.HydrodynamicsConstantData.WaterLevel;
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WaterVelocityX)
                               .Value = timeFrameData.HydrodynamicsConstantData.VelocityX;
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WaterVelocityY)
                               .Value = timeFrameData.HydrodynamicsConstantData.VelocityY;
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WaterLevel)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WaterVelocityX)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.WaterVelocityY)
                               .SetValueAsString("0");
            }
        }

        private static List<IniSection> GroupPropertiesByMdwSection(WaveModelDefinition modelDefinition)
        {
            var mdwSections = new List<IniSection>();
            IEnumerable<IGrouping<string, WaveModelProperty>> groupedProperties = modelDefinition.Properties.GroupBy(p => p.PropertyDefinition.FileCategoryName);
            foreach (IGrouping<string, WaveModelProperty> grouping in groupedProperties)
            {
                string mdwSectionName = grouping.Key;
                if (ExcludedSections.Contains(mdwSectionName))
                {
                    continue;
                }

                var mdwGroup = new IniSection(mdwSectionName);
                foreach (WaveModelProperty property in grouping)
                {
                    string name = property.PropertyDefinition.FilePropertyName;
                    string value = property.GetValueAsString();
                    mdwGroup.AddProperty(name, value);
                }

                mdwSections.Add(mdwGroup);
            }

            return mdwSections;
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
                    .GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeSeriesFile)
                    .SetValueAsString(tSeriesFile);
                new BcwFile().Write(allTimeSeriesPerBoundary, Path.Combine(targetFile, tSeriesFile));
            }
            else
            {
                modelDefinition
                    .GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeSeriesFile)
                    .SetValueAsString(string.Empty);
            }
        }
        
        private static void CreateTimePointSections(ITimeFrameData timeFrameData,
                                                      List<IniSection> mdwSections,
                                                      DateTime modelReferenceDateTime)
        {
            var sections = new List<IniSection>();

            IMultiDimensionalArray<DateTime> times =
                timeFrameData.TimeVaryingData.Arguments[0].GetValues<DateTime>();

            foreach (DateTime time in times)
            {
                var timeSection = new IniSection(KnownWaveSections.TimePointSection);
                timeSection.AddProperty(KnownWaveProperties.Time, (time - modelReferenceDateTime).TotalMinutes);

                double[] values = timeFrameData.TimeVaryingData.GetAllComponentValues(time).Cast<double>().ToArray();

                if (timeFrameData.HydrodynamicsInputDataType == HydrodynamicsInputDataType.TimeVarying)
                {
                    timeSection.AddProperty(KnownWaveProperties.WaterLevel, values[0]);
                    timeSection.AddProperty(KnownWaveProperties.WaterVelocityX, values[1]);
                    timeSection.AddProperty(KnownWaveProperties.WaterVelocityY, values[2]);
                }

                if (timeFrameData.WindInputDataType == WindInputDataType.TimeVarying)
                {
                    timeSection.AddProperty(KnownWaveProperties.WindSpeed, values[3]);
                    timeSection.AddProperty(KnownWaveProperties.WindDirection, values[4]);
                }

                sections.Add(timeSection);
            }

            IniSection generalSection = mdwSections.First(c => c.Name == KnownWaveSections.GeneralSection);
            int afterGeneralIndex = mdwSections.IndexOf(generalSection);
            mdwSections.InsertRange(afterGeneralIndex + 1, sections);
        }
        
        private static void SaveMeteoFile(ITimeFrameData timeFrameData,
                                          IEnumerable<IniSection> mdwSections,
                                          string sourceDir,
                                          string targetDir)
        {
            IniSection generalSection = mdwSections.First(c => c.Name == KnownWaveSections.GeneralSection);

            generalSection.RemoveAllProperties(KnownWaveProperties.MeteoFile);

            if (timeFrameData.WindInputDataType == WindInputDataType.FileBased)
            {
                List<string> meteoFiles = GetMeteoFiles(timeFrameData.WindFileData);
                meteoFiles.ForEach((mf, i) =>
                {
                    generalSection.AddProperty(KnownWaveProperties.MeteoFile, meteoFiles[i]);
                    CopyModelFile(meteoFiles[i], sourceDir, targetDir);
                });
            }
            else
            {
                generalSection.AddOrUpdateProperty(KnownWaveProperties.MeteoFile, string.Empty);
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
        
        private static void HandleInputTemplateFile(WaveModelDefinition modelDefinition, IFilesManager filesManager, List<IniSection> mdwSections)
        {
            if (string.IsNullOrEmpty(modelDefinition.InputTemplateFilePath))
            {
                return;
            }

            IniSection generalSection = mdwSections.First(s => s.IsNameEqualTo(KnownWaveSections.GeneralSection));
            string fileName = Path.GetFileName(modelDefinition.InputTemplateFilePath);
            generalSection.AddOrUpdateProperty(KnownWaveProperties.InputTemplateFile, fileName);

            filesManager.Add(modelDefinition.InputTemplateFilePath, s => modelDefinition.InputTemplateFilePath = s);
        }
    }
}