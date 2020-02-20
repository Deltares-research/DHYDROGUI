using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public class MdwFile : FMSuiteFileBase
    {
        private const string PolyfileName = "PolylineFile";

        private const string WaveObstaclePropertyName = "Name";
        private const string WaveObstaclePropertyType = "Type";
        private const string WaveObstaclePropertyTransmissionCoefficient = "TransmCoef";
        private const double WaveObstacleDefaultValueTransmissionCoefficient = 0.0;
        private const string WaveObstaclePropertyHeight = "Height";
        private const double WaveObstacleDefaultValueHeight = 0.0;
        private const string WaveObstaclePropertyAlpha = "Alpha";
        private const double WaveObstacleDefaultValueAlpha = 0.0;
        private const string WaveObstaclePropertyBeta = "Beta";
        private const double WaveObstacleDefaultValueBeta = 0.0;
        private const string WaveObstaclePropertyReflections = "Reflections";
        private const string WaveObstaclePropertyReflectionCoefficient = "ReflecCoef";
        private const double WaveObstacleDefaultValueReflectionCoefficient = 0.0;

        private static readonly ILog Log = LogManager.GetLogger(typeof(MdwFile));

        /// <summary>
        /// These mdw categories can have multiplicity greater than 1 (or gui only),
        /// excluded them from the generic property treatment..
        /// </summary>
        public static readonly IList<string> ExcludedCategories = new List<string>
        {
            KnownWaveCategories.TimePointCategory,
            KnownWaveCategories.DomainCategory,
            KnownWaveCategories.BoundaryCategory,
            KnownWaveCategories.GuiOnlyCategory
        };

        public string MdwFilePath { get; set; }

        public void SaveTo(string mdwTargetFilePath, WaveModelDefinition modelDefinition, bool switchTo)
        {
            string modelName = Path.GetFileNameWithoutExtension(mdwTargetFilePath);
            string targetDir = Path.GetDirectoryName(mdwTargetFilePath);
            if (targetDir != string.Empty && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            WriteObstacles(modelDefinition, modelName, targetDir);

            SetConstantWindProperties(modelDefinition);
            SetConstantHydrodynamicsProperties(modelDefinition);

            // save boundary data
            string tSeriesFile = modelName + ".bcw";
            modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeSeriesFile)
                           .SetValueAsString(modelDefinition.BoundaryConditions.Any(bc =>
                                                                                        bc.DataType ==
                                                                                        BoundaryConditionDataType
                                                                                            .ParameterizedSpectrumTimeseries)
                                                 ? tSeriesFile
                                                 : string.Empty);

            string locationFileName = modelDefinition
                                      .GetModelProperty(KnownWaveCategories.OutputCategory,
                                                        KnownWaveProperties.LocationFile)
                                      .GetValueAsString();

            if (modelDefinition.ObservationPoints.Any())
            {
                if (string.IsNullOrEmpty(locationFileName))
                {
                    locationFileName = modelName + ".loc";
                    modelDefinition
                        .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                        .SetValueAsString(locationFileName);
                }

                new ObsFile<Feature2DPoint>().Write(Path.Combine(targetDir, locationFileName),
                                                    modelDefinition.ObservationPoints, false);
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                               .SetValueAsString(string.Empty);
            }

            List<DelftIniCategory> mdwCategories = GroupPropertiesByMdwCategory(modelDefinition);

            CreateTimePointCategories(modelDefinition, ref mdwCategories);

            IEnumerable<DelftIniCategory> boundaryConditionCategories = modelDefinition.BoundaryConditions
                                                                        .Select(WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory);
            mdwCategories.AddRange(boundaryConditionCategories);
            SaveBoundaryConditions(modelDefinition, Path.Combine(targetDir, tSeriesFile),
                                   modelDefinition.ModelReferenceDateTime);

            if (MdwFilePath != null)
            {
                string sourceDir = Path.GetDirectoryName(MdwFilePath);

                #region Not synchronized with modeldefintionproperties, since property is missing in csv file

                DelftIniCategory generalCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory);
                if (modelDefinition.TimePointData.WindDataType == InputFieldDataType.FromInputFiles)
                {
                    List<string> meteoFiles = GetMeteoFiles(modelDefinition.TimePointData.MeteoData);
                    meteoFiles.ForEach((mf, i) =>
                    {
                        generalCategory.SetProperty(KnownWaveProperties.MeteoFile, meteoFiles[i]);
                        CopyModelFile(meteoFiles[i], sourceDir, targetDir);
                    });
                }
                else if (modelDefinition.TimePointData.WindDataType != InputFieldDataType.FromInputFiles)
                {
                    generalCategory.SetProperty(KnownWaveProperties.MeteoFile, string.Empty);
                }

                #endregion

                // domain(s)
                IList<WaveDomainData> allDomains = WaveDomainHelper.GetAllDomains(modelDefinition.OuterDomain);
                AddDomainCategories(allDomains, ref mdwCategories);
                allDomains.ForEach(d => CopyMeteoFilesTo(d.MeteoData, targetDir, switchTo));

                // grid is not edited within DS, so always in sync
                // and a plain file copy suffices
                CopyGridFiles(allDomains.Select(d => d.GridFileName), sourceDir, targetDir);

                IEnumerable<string> boundarySpectralFiles = mdwCategories
                                                            .Where(c => c.Name == KnownWaveCategories.BoundaryCategory)
                                                            .SelectMany(
                                                                c => c.GetPropertyValues(KnownWaveProperties.Spectrum));
                boundarySpectralFiles.ForEach(f => CopyModelFile(f, sourceDir, targetDir));

                if (modelDefinition.BoundaryIsDefinedBySpecFile)
                {
                    var sp2Boundary = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
                    sp2Boundary.AddProperty(KnownWaveProperties.Definition, "fromsp2file");
                    sp2Boundary.AddProperty(KnownWaveProperties.OverallSpecFile, modelDefinition.OverallSpecFile);
                    mdwCategories.Add(sp2Boundary);

                    // here spec file should always be relative
                    CopyModelFile(modelDefinition.OverallSpecFile, sourceDir, targetDir);
                }
            }

            DelftIniCategory outputCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.OutputCategory);
            outputCategory.SetProperty(KnownWaveProperties.COMFile, modelDefinition.CommunicationsFilePath.Replace('\\', '/'));

            #region Not synchronized with modeldefintionproperties, since property is missing in csv file

            string curvesFileName = outputCategory.GetPropertyValue(KnownWaveProperties.CurveFile);
            if (modelDefinition.ObservationCrossSections.Any())
            {
                if (string.IsNullOrEmpty(curvesFileName))
                {
                    curvesFileName = modelName + ".cur";
                    outputCategory.SetProperty(KnownWaveProperties.CurveFile, curvesFileName);
                }

                new PliFile<Feature2D>().Write(Path.Combine(targetDir, curvesFileName),
                                               modelDefinition.ObservationCrossSections);
            }
            else
            {
                outputCategory.SetProperty(KnownWaveProperties.CurveFile, string.Empty);
            }

            #endregion

            // write mdw
            new DelftIniWriter().WriteDelftIniFile(mdwCategories, mdwTargetFilePath);

            // switch
            if (switchTo)
            {
                MdwFilePath = mdwTargetFilePath;
            }
        }

        private void SetConstantWindProperties(WaveModelDefinition modelDefinition)
        {
            if (modelDefinition.TimePointData.WindDataType == InputFieldDataType.Constant)
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                               .Value = modelDefinition.TimePointData.WindSpeedConstant;
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                               .Value = modelDefinition.TimePointData.WindDirectionConstant;
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                               .SetValueAsString("0");
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                               .SetValueAsString("0");
            }
        }

        private void SetConstantHydrodynamicsProperties(WaveModelDefinition modelDefinition)
        {
            if (modelDefinition.TimePointData.HydroDataType == InputFieldDataType.Constant)
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                               .Value = modelDefinition.TimePointData.WaterLevelConstant;
                modelDefinition
                        .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                        .Value =
                    modelDefinition.TimePointData.VelocityXConstant;
                modelDefinition
                        .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityY)
                        .Value =
                    modelDefinition.TimePointData.VelocityYConstant;
            }
            else
            {
                modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                               .SetValueAsString("0");
                modelDefinition
                    .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .SetValueAsString("0");
                modelDefinition
                    .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityY)
                    .SetValueAsString("0");
            }
        }

        private static List<DelftIniCategory> GroupPropertiesByMdwCategory(WaveModelDefinition modelDefinition)
        {
            var mdwCategories = new List<DelftIniCategory>();
            IEnumerable<IGrouping<string, WaveModelProperty>> groupedProperties =
                modelDefinition.Properties.GroupBy(p => p.PropertyDefinition.FileCategoryName);
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

        private void CopyGridFiles(IEnumerable<string> gridFilePaths, string sourceDir, string targetDir)
        {
            foreach (string filename in gridFilePaths)
            {
                if (sourceDir != null && File.Exists(Path.Combine(sourceDir, filename)))
                {
                    CopyModelFile(filename, sourceDir, targetDir);
                }
            }
        }

        private void SaveBoundaryConditions(WaveModelDefinition modelDefinition, string targetFile, DateTime refDate)
        {
            IEventedList<WaveBoundaryCondition> boundaryConditions = modelDefinition.BoundaryConditions;

            // update refdate before writing
            foreach (WaveBoundaryCondition bc in boundaryConditions)
            {
                bc.PointData.ForEach(f => f.Attributes[BcwFile.RefDateAttributeName] =
                                              refDate.ToString(BcwFile.DateFormatString));
            }

            // get the bcwconditions with parameterizedspectrumtimeseries.
            // Make it a dictionary, but make sure to order the datapointindices while writing, because rekenhart demands it.
            Dictionary<string, List<IFunction>> bcwConditions = boundaryConditions
                                                                .Where(bc => bc.DataType == BoundaryConditionDataType
                                                                                 .ParameterizedSpectrumTimeseries)
                                                                .ToDictionary(
                                                                    b => b.Name,
                                                                    b => b.DataPointIndices.OrderBy(di => di)
                                                                          .Select(b.GetDataAtPoint).ToList());

            // write bcw file                                    
            if (bcwConditions.Any())
            {
                new BcwFile().Write(bcwConditions, targetFile);
            }
        }

        private static void WriteObstacles(WaveModelDefinition modelDefinition, string modelName, string targetDir)
        {
            WaveModelProperty propertyObstacleFile = modelDefinition.GetModelProperty(
                KnownWaveCategories.GeneralCategory,
                KnownWaveProperties.ObstacleFile);

            if (modelDefinition.Obstacles.Any())
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

                IEventedList<WaveObstacle> features = modelDefinition.Obstacles;
                new PliFile<Feature2D>().Write(Path.Combine(targetDir, geometryFile), features);

                var fileInfo = new DelftIniCategory(KnownWaveCategories.ObstacleFileInfoCategory);
                fileInfo.AddProperty("FileVersion", "02.00");
                fileInfo.AddProperty(PolyfileName, geometryFile);
                obtCategories.Add(fileInfo);

                foreach (WaveObstacle obstacle in modelDefinition.Obstacles.OfType<WaveObstacle>())
                {
                    var obstacleCategory = new DelftIniCategory(KnownWaveCategories.ObstacleCategory);

                    obstacleCategory.AddProperty(WaveObstaclePropertyName, obstacle.Name);
                    obstacleCategory.AddProperty(WaveObstaclePropertyType, obstacle.Type.ToString().ToLower());
                    if (obstacle.Type == ObstacleType.Sheet)
                    {
                        obstacleCategory.AddProperty(WaveObstaclePropertyTransmissionCoefficient,
                                                     obstacle.TransmissionCoefficient);
                    }
                    else
                    {
                        obstacleCategory.AddProperty(WaveObstaclePropertyHeight, obstacle.Height);
                        obstacleCategory.AddProperty(WaveObstaclePropertyAlpha, obstacle.Alpha);
                        obstacleCategory.AddProperty(WaveObstaclePropertyBeta, obstacle.Beta);
                    }

                    obstacleCategory.AddProperty(WaveObstaclePropertyReflections,
                                                 obstacle.ReflectionType.ToString().ToLower());
                    if (obstacle.ReflectionType != ReflectionType.No)
                    {
                        obstacleCategory.AddProperty(WaveObstaclePropertyReflectionCoefficient,
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

        private void AddDomainCategories(IList<WaveDomainData> allDomains, ref List<DelftIniCategory> mdwCategories)
        {
            foreach (WaveDomainData domain in allDomains)
            {
                var domainCategory = new DelftIniCategory(KnownWaveCategories.DomainCategory);
                domainCategory.AddProperty("Grid", domain.GridFileName);
                domainCategory.AddProperty("BedLevelGrid", domain.BedLevelGridFileName);
                domainCategory.AddProperty("BedLevel", domain.BedLevelFileName);

                if (!domain.SpectralDomainData.UseDefaultDirectionalSpace)
                {
                    domainCategory.AddProperty("DirSpace",
                                               domain.SpectralDomainData.DirectionalSpaceType.GetDescription()
                                                     .ToLower());
                    domainCategory.AddProperty("NDir", domain.SpectralDomainData.NDir);
                    domainCategory.AddProperty("StartDir", domain.SpectralDomainData.StartDir);
                    domainCategory.AddProperty("EndDir", domain.SpectralDomainData.EndDir);
                }

                if (!domain.SpectralDomainData.UseDefaultFrequencySpace)
                {
                    domainCategory.AddProperty("NFreq", domain.SpectralDomainData.NFreq);
                    domainCategory.AddProperty("FreqMin", domain.SpectralDomainData.FreqMin);
                    domainCategory.AddProperty("FreqMax", domain.SpectralDomainData.FreqMax);
                }

                if (!domain.UseGlobalMeteoData)
                {
                    List<string> meteoFiles = GetMeteoFiles(domain.MeteoData);
                    meteoFiles.ForEach(
                        (mf, i) => domainCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoFiles[i]));
                }

                if (!domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings)
                {
                    domainCategory.AddProperty("FlowBedLevel", (int) domain.HydroFromFlowData.BedLevelUsage);
                    domainCategory.AddProperty("FlowWaterLevel", (int) domain.HydroFromFlowData.WaterLevelUsage);
                    domainCategory.AddProperty("FlowVelocity", (int) domain.HydroFromFlowData.VelocityUsage);
                    domainCategory.AddProperty("FlowVelocityType",
                                               domain.HydroFromFlowData.VelocityUsageType.GetDescription());
                    domainCategory.AddProperty("FlowWind", (int) domain.HydroFromFlowData.WindUsage);
                }

                if (domain.SuperDomain != null)
                {
                    // position in list == position in mdw fiel == domain number:
                    int index = allDomains.IndexOf(domain.SuperDomain);
                    domainCategory.AddProperty("NestedInDomain", index + 1);
                }

                domainCategory.AddProperty("Output", domain.Output.ToString().ToLower());
                mdwCategories.Add(domainCategory);
            }
        }

        /// <summary>
        /// Copies model files, relative to model directory
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="sourceDir"> </param>
        /// <param name="targetDir"> </param>
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
                    Log.ErrorFormat("Failed to copy {0} to {1}", originalFilePath, targetFilePath);
                    throw;
                }
            }
        }

        private static void CreateTimePointCategories(WaveModelDefinition modelDefinition,
                                                      ref List<DelftIniCategory> mdwCategories)
        {
            var categories = new List<DelftIniCategory>();

            WaveInputFieldData timepointData = modelDefinition.TimePointData;
            IMultiDimensionalArray<DateTime> times =
                modelDefinition.TimePointData.InputFields.Arguments[0].GetValues<DateTime>();

            foreach (DateTime time in times)
            {
                var timeCategory = new DelftIniCategory(KnownWaveCategories.TimePointCategory);
                timeCategory.AddProperty(KnownWaveProperties.Time,
                                         (time - modelDefinition.ModelReferenceDateTime).TotalMinutes);

                double[] values = timepointData.InputFields.GetAllComponentValues(time).Cast<double>().ToArray();

                if (timepointData.HydroDataType == InputFieldDataType.TimeVarying)
                {
                    timeCategory.AddProperty(KnownWaveProperties.WaterLevel, values[0]);
                    timeCategory.AddProperty(KnownWaveProperties.WaterVelocityX, values[1]);
                    timeCategory.AddProperty(KnownWaveProperties.WaterVelocityY, values[2]);
                }

                if (timepointData.WindDataType == InputFieldDataType.TimeVarying)
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

        public WaveModelDefinition Load(string filePath)
        {
            MdwFilePath = filePath;

            var modelDefinition = new WaveModelDefinition();

            IList<DelftIniCategory> mdwCategories;
            using (var fileStream = new FileStream(MdwFilePath, FileMode.Open, FileAccess.Read))
            {
                mdwCategories = new DelftIniReader().ReadDelftIniFile(fileStream, MdwFilePath);
            }
            string mdwDir = Path.GetDirectoryName(filePath);

            ConvertMdwCategoriesToModelDefinitionProperties(modelDefinition, mdwCategories);

            // domain(s) and nesting
            List<WaveDomainData> allDomains = CreateWaveDomainData(mdwCategories).ToList();
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
            }

            // time frame
            IList<DateTime> times;
            modelDefinition.TimePointData =
                CreateTimePointData(mdwCategories, modelDefinition.ModelReferenceDateTime, out times);

            List<WaveBoundaryCondition> allConditions = CreateWaveBoundaries(mdwCategories, modelDefinition).ToList();
            List<WaveBoundaryCondition> orientedConditions = allConditions.Where(bc => bc.Feature.Attributes != null &&
                                                                                       bc.Feature.Attributes
                                                                                         .ContainsKey("orientation"))
                                                                          .ToList();
            modelDefinition.BoundaryConditions.AddRange(allConditions.Except(orientedConditions));
            modelDefinition.OrientedBoundaryConditions.AddRange(orientedConditions);

            modelDefinition.Obstacles.AddRange(
                CreateObstacleData(
                    mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory),
                    modelDefinition));

            string locFile = mdwCategories.First(c => c.Name == KnownWaveCategories.OutputCategory)
                                          .GetPropertyValue(KnownWaveProperties.LocationFile);
            if (locFile != null)
            {
                modelDefinition.ObservationPoints =
                    new ObsFile<Feature2DPoint>().Read(Path.Combine(mdwDir, locFile), false);
            }

            string curveFile = mdwCategories.First(c => c.Name == KnownWaveCategories.OutputCategory)
                                            .GetPropertyValue(KnownWaveProperties.CurveFile);
            if (curveFile != null)
            {
                modelDefinition.ObservationCrossSections =
                    new EventedList<Feature2D>(new PliFile<Feature2D>().Read(Path.Combine(mdwDir, curveFile)));
            }

            return modelDefinition;
        }

        /// <summary>
        /// Converting mdw categories to model definition properties.
        /// Second part is for linked properties, since the default value of a property can be dependent on another property value
        /// and this part will be used in situations if the property with multiple default values is missing in the mdw file.
        /// Based on the other property the correct one will be set. Otherwise the default value is based on the default value of
        /// the other property.
        /// </summary>
        /// <param name="modelDefinition"> </param>
        /// <param name="mdwCategories"> </param>
        private void ConvertMdwCategoriesToModelDefinitionProperties(WaveModelDefinition modelDefinition,
                                                                     IEnumerable<DelftIniCategory> mdwCategories)
        {
            var logHandler = new LogHandler(Resources.MdwFile_ConvertMdwCategoriesToModelDefinitionProperties_reading_the_mdw_file, Log);
            var backwardsCompatibilityHelper = 
                new DelftIniBackwardsCompatibilityHelper(new MdwFileBackwardsCompatibilityConfigurationValues());

            foreach (DelftIniCategory category in mdwCategories)
            {
                category.Name = backwardsCompatibilityHelper.GetUpdatedCategoryName(category.Name, logHandler) ?? category.Name;

                ModelPropertySchema<WaveModelPropertyDefinition> modelSchema = modelDefinition.ModelSchema;
                if (!modelSchema.ModelDefinitionCategory.ContainsKey(category.Name))
                {
                    continue;
                }

                ModelPropertyGroup definedCategory = modelSchema.ModelDefinitionCategory[category.Name];
                foreach (DelftIniProperty mdwProperty in category.Properties)
                {
                    if (backwardsCompatibilityHelper.IsObsoletePropertyName(mdwProperty.Name))
                    {
                        logHandler?.ReportWarningFormat(Common.Properties.Resources.Parameter__0__is_not_supported_by_our_computational_core_and_will_be_removed_from_your_input_file, mdwProperty.Name);
                        continue;
                    }

                    string propName = backwardsCompatibilityHelper.GetUpdatedPropertyName(mdwProperty.Name, logHandler) ?? mdwProperty.Name;
                    string propertyValue = mdwProperty.Value;

                    WaveModelPropertyDefinition definition =
                        modelSchema.PropertyDefinitions.ContainsKey(propName.ToLower())
                            ? modelSchema.PropertyDefinitions[propName.ToLower()]
                            : new WaveModelPropertyDefinition
                            {
                                Caption = mdwProperty.Name,
                                DataType = typeof(string),
                                FileCategoryName = category.Name,
                                FilePropertyName = mdwProperty.Name,
                                Category = definedCategory.Name, 
                                // default value as string should always be an empty string and not null.
                                DefaultValueAsString = string.Empty, 
                            };

                    modelDefinition.SetModelProperty(category.Name, propName,
                                                     new WaveModelProperty(definition, propertyValue));
                }
            }

            foreach (KeyValuePair<string, WaveModelPropertyDefinition> propertyDefinition in modelDefinition
                                                                                             .ModelSchema
                                                                                             .PropertyDefinitions)
            {
                if (propertyDefinition.Value.MultipleDefaultValuesAvailable)
                {
                    string nameOfDependentOnProperty = propertyDefinition.Value.DefaultValueDependentOn;
                    string nameOfPropertyWithMultipleDefaultValues = propertyDefinition.Value.FilePropertyName;
                    string categoryNameWithPropertyWithMultipleDefaultValues =
                        propertyDefinition.Value.FileCategoryName;

                    //Both available
                    List<DelftIniCategory> categoryOfDependentOnProperty =
                        mdwCategories.Where(mc => mc.Properties
                                                    .Any(p => p.Name.Equals(nameOfDependentOnProperty)))
                                     .ToList();

                    List<DelftIniCategory> categoryOfPropertyWithMultipleDefaultValues =
                        mdwCategories.Where(mc => mc.Properties
                                                    .Any(p => p.Name.Equals(nameOfPropertyWithMultipleDefaultValues)))
                                     .ToList();

                    //Situation in which property with multiple default values is missing and corresponding default value will be set.
                    if (!categoryOfPropertyWithMultipleDefaultValues.Any() && categoryOfDependentOnProperty.Any())
                    {
                        Log.WarnFormat(
                            "In the MDW file the property {0} is missing. Based on property {1} the default value is set",
                            nameOfPropertyWithMultipleDefaultValues, nameOfDependentOnProperty);

                        string categoryNameOfDependentOnProperty = categoryOfDependentOnProperty[0].Name;
                        WaveModelProperty dependentOnProperty =
                            modelDefinition.GetModelProperty(categoryNameOfDependentOnProperty,
                                                             nameOfDependentOnProperty);
                        WaveModelProperty propertyWithMultipleDefaultValues =
                            modelDefinition.GetModelProperty(categoryNameWithPropertyWithMultipleDefaultValues,
                                                             nameOfPropertyWithMultipleDefaultValues);
                        var index = (int) dependentOnProperty.Value;
                        propertyWithMultipleDefaultValues.SetValueAsString(
                            propertyWithMultipleDefaultValues.PropertyDefinition.MultipleDefaultValues[index]);
                    }
                }
            }
            
            logHandler.LogReport();
        }

        private IEnumerable<WaveDomainData> CreateWaveDomainData(IEnumerable<DelftIniCategory> categories)
        {
            foreach (DelftIniCategory domainCategory in categories.Where(
                c => c.Name == KnownWaveCategories.DomainCategory))
            {
                string gridFileName = domainCategory.GetPropertyValue("Grid", "");
                string domainName = Path.GetFileNameWithoutExtension(gridFileName);

                var domain = new WaveDomainData(domainName);
                domain.GridFileName = gridFileName;
                domain.BedLevelGridFileName = domainCategory.GetPropertyValue("BedLevelGrid", "");
                domain.BedLevelFileName = domainCategory.GetPropertyValue("BedLevel", "");
                domain.NestedInDomain = int.Parse(domainCategory.GetPropertyValue("NestedInDomain", "-1"),
                                                  NumberStyles.Any, CultureInfo.InvariantCulture);

                string spaceType = domainCategory.GetPropertyValue("DirSpace");
                if (spaceType != null)
                {
                    domain.SpectralDomainData.DirectionalSpaceType = spaceType == "circle"
                                                                         ? WaveDirectionalSpaceType.Circle
                                                                         : WaveDirectionalSpaceType.Sector;
                    domain.SpectralDomainData.NDir = int.Parse(domainCategory.GetPropertyValue("NDir", "0"),
                                                               NumberStyles.Any, CultureInfo.InvariantCulture);
                    domain.SpectralDomainData.StartDir = double.Parse(
                        domainCategory.GetPropertyValue("StartDir", "0.0"), NumberStyles.Any,
                        CultureInfo.InvariantCulture);
                    domain.SpectralDomainData.EndDir = double.Parse(domainCategory.GetPropertyValue("EndDir", "0.0"),
                                                                    NumberStyles.Any, CultureInfo.InvariantCulture);
                    domain.SpectralDomainData.UseDefaultDirectionalSpace = false;
                }
                else
                {
                    domain.SpectralDomainData.UseDefaultDirectionalSpace = true;
                }

                string nFreq = domainCategory.GetPropertyValue("NFreq");
                if (nFreq != null)
                {
                    domain.SpectralDomainData.NFreq =
                        (int) double.Parse(nFreq, NumberStyles.Any, CultureInfo.InvariantCulture);
                    domain.SpectralDomainData.FreqMin = double.Parse(domainCategory.GetPropertyValue("FreqMin", "0.0"),
                                                                     NumberStyles.Any, CultureInfo.InvariantCulture);
                    domain.SpectralDomainData.FreqMax = double.Parse(domainCategory.GetPropertyValue("FreqMax", "0.0"),
                                                                     NumberStyles.Any, CultureInfo.InvariantCulture);
                    domain.SpectralDomainData.UseDefaultFrequencySpace = false;
                }
                else
                {
                    domain.SpectralDomainData.UseDefaultFrequencySpace = true;
                }

                string bedLevelUsage = domainCategory.GetPropertyValue("FlowBedLevel");
                if (bedLevelUsage != null)
                {
                    domain.HydroFromFlowData.BedLevelUsage =
                        (UsageFromFlowType) int.Parse(bedLevelUsage, NumberStyles.Any, CultureInfo.InvariantCulture);
                    domain.HydroFromFlowData.WaterLevelUsage = (UsageFromFlowType) int.Parse(
                        domainCategory.GetPropertyValue("FlowWaterLevel", "0"), NumberStyles.Any,
                        CultureInfo.InvariantCulture);
                    domain.HydroFromFlowData.VelocityUsage = (UsageFromFlowType) int.Parse(
                        domainCategory.GetPropertyValue("FlowVelocity", "0"), NumberStyles.Any,
                        CultureInfo.InvariantCulture);
                    string velocityType = domainCategory.GetPropertyValue("FlowVelocityType", "not-specified");
                    domain.HydroFromFlowData.VelocityUsageType = velocityType == "wave-dependent"
                                                                     ? VelocityComputationType.WaveDependent
                                                                     : velocityType == "surface-layer"
                                                                         ? VelocityComputationType.SurfaceLayer
                                                                         : VelocityComputationType.DepthAveraged;
                    domain.HydroFromFlowData.WindUsage = (UsageFromFlowType) int.Parse(
                        domainCategory.GetPropertyValue("FlowWind", "0"), NumberStyles.Any,
                        CultureInfo.InvariantCulture);

                    domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings = false;
                }
                else
                {
                    domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings = true;
                }

                yield return domain;
            }
        }

        private IEnumerable<WaveBoundaryCondition> CreateWaveBoundaries(IList<DelftIniCategory> categories,
                                                                        WaveModelDefinition modelDefinition)
        {
            DelftIniCategory generalCategory = categories.First(c => c.Name == KnownWaveCategories.GeneralCategory);

            string bcwFilePath = generalCategory.GetPropertyValue(KnownWaveProperties.TimeSeriesFile);

            IDictionary<string, List<IFunction>> functionLookup = null;
            if (!string.IsNullOrEmpty(bcwFilePath))
            {
                functionLookup = new BcwFile().Read(Path.Combine(Path.GetDirectoryName(MdwFilePath), bcwFilePath));
            }

            List<DelftIniCategory> boundaries =
                categories.Where(c => c.Name == KnownWaveCategories.BoundaryCategory).ToList();
            if (boundaries.Count == 1 &&
                boundaries[0].GetPropertyValue(KnownWaveProperties.Definition) == "fromsp2file")
            {
                // sp2 file
                string sp2File = boundaries[0].GetPropertyValue(KnownWaveProperties.OverallSpecFile);
                if (sp2File == null)
                {
                    Log.ErrorFormat("Error loading boundary: \'OverallSpecfile\' should be defined");
                    yield break;
                }

                modelDefinition.BoundaryIsDefinedBySpecFile = true;
                modelDefinition.OverallSpecFile = sp2File;
                yield break;
            }

            foreach (DelftIniCategory boundaryData in boundaries)
            {
                string name = boundaryData.GetPropertyValue(KnownWaveProperties.Name);
                BoundaryConditionDataType dataType =
                    boundaryData.GetPropertyValue(KnownWaveProperties.SpectrumSpec) == "parametric"
                        ? functionLookup != null && functionLookup.ContainsKey(name) // check if timeseries
                              ? BoundaryConditionDataType.ParameterizedSpectrumTimeseries
                              : BoundaryConditionDataType.ParameterizedSpectrumConstant
                        : BoundaryConditionDataType.SpectrumFromFile; // "from file"

                WaveBoundaryImportDefinitionType definition =
                    GetImportDefinition(boundaryData.GetPropertyValue(KnownWaveProperties.Definition));

                if (definition == WaveBoundaryImportDefinitionType.FromSp2File ||
                    definition == WaveBoundaryImportDefinitionType.FromWaveWatchFile ||
                    definition == WaveBoundaryImportDefinitionType.GridIndexBased)
                {
                    Log.ErrorFormat("Unsupported definition: " + definition + "skipping boundary {0} in {1}", name,
                                    MdwFilePath);
                    continue;
                }

                List<double> condSpecAtDists =
                    boundaryData.GetPropertyValues(KnownWaveProperties.CondSpecAtDist)
                                .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                .ToList();

                List<double> sortedList = condSpecAtDists.OrderBy(c => c).ToList();
                if (!condSpecAtDists.SequenceEqual(sortedList))
                {
                    throw new NotImplementedException("CondSpecAtDist in mdw should be ordered");
                }

                var feature = new Feature2D {Name = name};
                if (definition == WaveBoundaryImportDefinitionType.Orientation)
                {
                    // create dummy feature, will be fixed later
                    WaveBoundaryImportHelper.CreateDummyFeature(feature, condSpecAtDists);
                    feature.Attributes["orientation"] = boundaryData.GetPropertyValue("Orientation");
                }
                else
                {
                    var startCoordinate =
                        new Coordinate(
                            double.Parse(boundaryData.GetPropertyValue(KnownWaveProperties.StartCoordinateX),
                                         NumberStyles.Any,
                                         CultureInfo.InvariantCulture),
                            double.Parse(boundaryData.GetPropertyValue(KnownWaveProperties.StartCoordinateY),
                                         NumberStyles.Any,
                                         CultureInfo.InvariantCulture));
                    var endCoordinate =
                        new Coordinate(
                            double.Parse(boundaryData.GetPropertyValue(KnownWaveProperties.EndCoordinateX),
                                         NumberStyles.Any,
                                         CultureInfo.InvariantCulture),
                            double.Parse(boundaryData.GetPropertyValue(KnownWaveProperties.EndCoordinateY),
                                         NumberStyles.Any,
                                         CultureInfo.InvariantCulture));

                    feature.Geometry = WaveBoundaryImportHelper.CreateBoundaryGeometry(startCoordinate, endCoordinate,
                                                                                       condSpecAtDists);
                }

                var boundaryCondition = (WaveBoundaryCondition) new WaveBoundaryConditionFactory()
                    .CreateBoundaryCondition(feature,
                                             WaveBoundaryCondition.WaveQuantityName,
                                             dataType);
                boundaryCondition.Name = name;
                boundaryCondition.SpatialDefinitionType = condSpecAtDists.Any()
                                                              ? WaveBoundaryConditionSpatialDefinitionType
                                                                  .SpatiallyVarying
                                                              : WaveBoundaryConditionSpatialDefinitionType.Uniform;

                // spectral data
                if (dataType == BoundaryConditionDataType.SpectrumFromFile)
                {
                    List<string> spectrumFiles = boundaryData.GetPropertyValues(KnownWaveProperties.Spectrum).ToList();
                    for (var i = 0; i < spectrumFiles.Count; ++i)
                    {
                        boundaryCondition.AddPoint(i);
                        boundaryCondition.SpectrumFiles[i] = spectrumFiles[i];
                    }
                }
                else
                {
                    WaveBoundarySpectralData spectralData = GetSpectralData(boundaryData);
                    CopySpectralDataToWaveBoundaryCondition(boundaryCondition, spectralData);

                    if (dataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
                    {
                        // get parameters for distances or uniform from mdw file
                        List<double> waveHeight =
                            boundaryData.GetPropertyValues(KnownWaveProperties.WaveHeight)
                                        .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                        .ToList();
                        List<double> period =
                            boundaryData.GetPropertyValues(KnownWaveProperties.Period)
                                        .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                        .ToList();
                        List<double> direction =
                            boundaryData.GetPropertyValues(KnownWaveProperties.Direction)
                                        .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                        .ToList();
                        List<double> directionalSpreading =
                            boundaryData.GetPropertyValues(KnownWaveProperties.DirectionalSpreadingValue)
                                        .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                        .ToList();

                        int expectedMultiplicity = Math.Max(condSpecAtDists.Count, 1);
                        if (waveHeight.Count != expectedMultiplicity ||
                            period.Count != expectedMultiplicity ||
                            direction.Count != expectedMultiplicity ||
                            directionalSpreading.Count != expectedMultiplicity)
                        {
                            Log.ErrorFormat(
                                "Inconsistent parameter specification for boundary \'{0}\', boundary excluded", name);
                            continue;
                        }

                        // uniform or multiple support points, constant in time, if we have 
                        // waveheigth, we should have the other parameters as well
                        if (boundaryCondition.IsHorizontallyUniform)
                        {
                            boundaryCondition.AddPoint(0);
                            boundaryCondition.SpectrumParameters[0] = new WaveBoundaryParameters
                            {
                                Height = waveHeight[0],
                                Period = period[0],
                                Direction = direction[0],
                                Spreading = directionalSpreading[0]
                            };
                        }
                        else
                        {
                            // when first data point doesn't equal first coordinate in feature, we skip that one:
                            int offset = condSpecAtDists[0] > 0.0 ? 1 : 0;
                            int count = Math.Min(condSpecAtDists.Count,
                                                 boundaryCondition.Feature.Geometry.Coordinates.Count());
                            for (var i = 0; i < count; ++i)
                            {
                                boundaryCondition.AddPoint(i + offset);
                                boundaryCondition.SpectrumParameters[i + offset] = new WaveBoundaryParameters
                                {
                                    Height = waveHeight[i],
                                    Period = period[i],
                                    Direction = direction[i],
                                    Spreading = directionalSpreading[i]
                                };
                            }
                        }
                    }

                    if (dataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
                    {
                        // from time series file (.bcw for now..)
                        if (functionLookup == null || !functionLookup.ContainsKey(name))
                        {
                            Log.ErrorFormat("Unexpected missing data in bcw file for boundary {0}, excluding boundary",
                                            name);
                            continue;
                        }

                        List<IFunction> functions = functionLookup[name].ToList();

                        if (condSpecAtDists.Count > 0)
                        {
                            // add distances, functions will follow from bcw fil3
                            int offset = condSpecAtDists[0] > 0.0 ? 1 : 0;
                            for (var i = 0; i < condSpecAtDists.Count; ++i)
                            {
                                boundaryCondition.SetTimeSeriesAtSupportPoint(i + offset, functions[i]);
                            }
                        }
                        else
                        {
                            // ASSUMPTION: There will be no condSpecAtDist in a uniform time series, because points without data aren't saved in the file format.
                            // if there are no condSpecAtDists, we need to add the information of the boundary to the first point.
                            // first try to get the function.
                            IFunction func = functions.FirstOrDefault();

                            if (func != null)
                            {
                                // add it to the time series' first support point.
                                boundaryCondition.SetTimeSeriesAtSupportPoint(0, func);
                            }
                        }
                    }
                }

                yield return boundaryCondition;
            }
        }

        private WaveBoundarySpectralData GetSpectralData(DelftIniCategory boundaryData)
        {
            return new WaveBoundarySpectralData
            {
                ShapeType = GetSpectrumShapeType(boundaryData.GetPropertyValue(KnownWaveProperties.ShapeType)),
                PeriodType = boundaryData.GetPropertyValue(KnownWaveProperties.PeriodType) == "peak"
                                 ? WavePeriodType.Peak
                                 : WavePeriodType.Mean,
                DirectionalSpreadingType =
                    boundaryData.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType) == "power"
                        ? WaveDirectionalSpreadingType.Power
                        : WaveDirectionalSpreadingType.Degrees,
                PeakEnhancementFactor =
                    double.Parse(boundaryData.GetPropertyValue(KnownWaveProperties.PeakEnhancementFactor),
                                 NumberStyles.Any, CultureInfo.InvariantCulture),
                GaussianSpreadingValue =
                    double.Parse(boundaryData.GetPropertyValue(KnownWaveProperties.GaussianSpreading), NumberStyles.Any,
                                 CultureInfo.InvariantCulture)
            };
        }

        private static void CopySpectralDataToWaveBoundaryCondition(WaveBoundaryCondition boundaryCondition,
                                                                    WaveBoundarySpectralData spectralData)
        {
            boundaryCondition.ShapeType = spectralData.ShapeType;
            boundaryCondition.PeriodType = spectralData.PeriodType;
            boundaryCondition.DirectionalSpreadingType = spectralData.DirectionalSpreadingType;
            boundaryCondition.PeakEnhancementFactor = spectralData.PeakEnhancementFactor;
            boundaryCondition.GaussianSpreadingValue = spectralData.GaussianSpreadingValue;
        }

        private WaveInputFieldData CreateTimePointData(IEnumerable<DelftIniCategory> mdwCategories,
                                                       DateTime referenceDate, out IList<DateTime> times)
        {
            var timepointData = new WaveInputFieldData
            {
                HydroDataType = InputFieldDataType.Constant,
                WindDataType = InputFieldDataType.Constant
            };

            times = new List<DateTime>();
            List<DelftIniCategory> timePointCategories =
                mdwCategories.Where(c => c.Name == KnownWaveCategories.TimePointCategory).ToList();
            if (timePointCategories.Any(c => c.GetPropertyValue(KnownWaveProperties.WaterLevel) != null))
            {
                timepointData.HydroDataType = InputFieldDataType.TimeVarying;
            }

            if (timePointCategories.Any(c => c.GetPropertyValue(KnownWaveProperties.WindSpeed) != null))
            {
                timepointData.WindDataType = InputFieldDataType.TimeVarying;
            }

            DelftIniCategory generalCategory = mdwCategories.First(c => c.Name == KnownWaveCategories.GeneralCategory);
            List<string> meteoFiles = generalCategory.GetPropertyValues(KnownWaveProperties.MeteoFile).ToList();
            if (meteoFiles.Any())
            {
                timepointData.WindDataType = InputFieldDataType.FromInputFiles;
                timepointData.MeteoData = CreateMeteoDataFromFiles(meteoFiles);
            }

            if (timepointData.HydroDataType == InputFieldDataType.Constant)
            {
                double waterlevel = double.Parse(
                    generalCategory.GetPropertyValue(KnownWaveProperties.WaterLevel, "0.0"),
                    NumberStyles.Any, CultureInfo.InvariantCulture);
                double velocityX = double.Parse(
                    generalCategory.GetPropertyValue(KnownWaveProperties.WaterVelocityX, "0.0"),
                    NumberStyles.Any, CultureInfo.InvariantCulture);
                double velocityY = double.Parse(
                    generalCategory.GetPropertyValue(KnownWaveProperties.WaterVelocityY, "0.0"),
                    NumberStyles.Any, CultureInfo.InvariantCulture);
                timepointData.WaterLevelConstant = waterlevel;
                timepointData.VelocityXConstant = velocityX;
                timepointData.VelocityYConstant = velocityY;
            }

            if (timepointData.WindDataType == InputFieldDataType.Constant)
            {
                double windspeed = double.Parse(generalCategory.GetPropertyValue(KnownWaveProperties.WindSpeed, "0.0"),
                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                double winddir = double.Parse(
                    generalCategory.GetPropertyValue(KnownWaveProperties.WindDirection, "0.0"),
                    NumberStyles.Any, CultureInfo.InvariantCulture);
                timepointData.WindSpeedConstant = windspeed;
                timepointData.WindDirectionConstant = winddir;
            }

            foreach (DelftIniCategory timepoint in timePointCategories)
            {
                DateTime time = referenceDate.AddMinutes(double.Parse(
                                                             timepoint.GetPropertyValue(
                                                                 KnownWaveProperties.Time, "0.0"),
                                                             NumberStyles.Any,
                                                             CultureInfo.InvariantCulture));
                times.Add(time);

                double waterLevel;
                if (!double.TryParse(timepoint.GetPropertyValue(KnownWaveProperties.WaterLevel), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out waterLevel))
                {
                    waterLevel = (double) timepointData.InputFields.Components[0].DefaultValue;
                }

                double velocityX;
                if (!double.TryParse(timepoint.GetPropertyValue(KnownWaveProperties.WaterVelocityX), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out velocityX))
                {
                    velocityX = (double) timepointData.InputFields.Components[1].DefaultValue;
                }

                double velocityY;
                if (!double.TryParse(timepoint.GetPropertyValue(KnownWaveProperties.WaterVelocityY), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out velocityY))
                {
                    velocityY = (double) timepointData.InputFields.Components[2].DefaultValue;
                }

                double windSpeed;
                if (!double.TryParse(timepoint.GetPropertyValue(KnownWaveProperties.WindSpeed), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out windSpeed))
                {
                    windSpeed = (double) timepointData.InputFields.Components[3].DefaultValue;
                }

                double windDirection;
                if (!double.TryParse(timepoint.GetPropertyValue(KnownWaveProperties.WindDirection), NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out windDirection))
                {
                    windDirection = (double) timepointData.InputFields.Components[4].DefaultValue;
                }

                timepointData.InputFields[time] = new[]
                {
                    waterLevel,
                    velocityX,
                    velocityY,
                    windSpeed,
                    windDirection
                };
            }

            return timepointData;
        }

        private WaveMeteoData CreateMeteoDataFromFiles(IReadOnlyCollection<string> meteoFiles)
        {
            List<string> spwFiles = meteoFiles.Where(mf => mf.EndsWith(".spw")).ToList();
            List<string> otherFiles = meteoFiles.Where(mf => !mf.EndsWith(".spw")).ToList();

            if (spwFiles.Count > 1)
            {
                Log.Error("Multiple spider web files specified for single domain; meteo data set to default");
                return new WaveMeteoData();
            }

            if (spwFiles.Count == 1 && otherFiles.Count == 0)
            {
                return new WaveMeteoData
                {
                    FileType = WindDefinitionType.SpiderWebGrid,
                    SpiderWebFilePath = Path.Combine(MdwFilePath, spwFiles[0])
                };
            }

            WaveMeteoData data;
            if (otherFiles.Count == 1)
            {
                data = new WaveMeteoData
                {
                    FileType = WindDefinitionType.WindXY,
                    XYVectorFilePath = Path.Combine(MdwFilePath, otherFiles[0])
                };
            }
            else if (otherFiles.Count == 2)
            {
                data = new WaveMeteoData
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = Path.Combine(MdwFilePath, otherFiles[1]),
                    YComponentFilePath = Path.Combine(MdwFilePath, otherFiles[1])
                };
            }
            else
            {
                Log.Error("Invalid number of meteo files specified for single domain; meteo data set to default");
                return new WaveMeteoData();
            }

            if (spwFiles.Count == 1)
            {
                data.HasSpiderWeb = true;
                data.SpiderWebFilePath = Path.Combine(MdwFilePath, spwFiles[0]);
            }

            return data;
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
                Log.ErrorFormat("Obstacle file {0} does not exist", obstacleFilePath);
                yield break;
            }

            var delftIniReader = new DelftIniReader();
            IList<DelftIniCategory> obtCategories;
            using (var fileStream = new FileStream(obstacleFilePath, FileMode.Open, FileAccess.Read))
            {
                obtCategories = delftIniReader.ReadDelftIniFile(fileStream, obstacleFilePath);
            }

            DelftIniCategory fileInfo =
                obtCategories.First(c => c.Name == KnownWaveCategories.ObstacleFileInfoCategory);
            string polylineFileName = fileInfo.GetPropertyValue(PolyfileName);
            string geometryFilePath = Path.Combine(mdwDirectory, polylineFileName);
            if (!File.Exists(geometryFilePath))
            {
                Log.ErrorFormat("Obstacle polyline file {0} does not exist", geometryFilePath);
                yield break;
            }

            modelDefinition.ObstaclePolylineFile = polylineFileName;

            var pliFile = new PliFile<Feature2D>();
            Dictionary<string, Feature2D> features = pliFile.Read(geometryFilePath).ToDictionary(f => f.Name);

            foreach (DelftIniCategory obstacle in obtCategories.Where(
                o => o.Name == KnownWaveCategories.ObstacleCategory))
            {
                string name = obstacle.GetPropertyValue(WaveObstaclePropertyName, "default name");
                if (!features.ContainsKey(name))
                {
                    Log.ErrorFormat("Obstacle polyline file {0} does not contain geometry for obstacle {1}, skipping",
                                    geometryFilePath, name);
                    continue;
                }

                if (!features[name].Geometry.IsValid)
                {
                    Log.ErrorFormat("Obstacle polyline file {0} contain invalid geometry for obstacle {1}, skipping",
                                    geometryFilePath, name);
                    continue;
                }

                var obs = new WaveObstacle
                {
                    Name = name,
                    Geometry = features[name].Geometry
                };
                obs.Name = name;

                obs.Type = obstacle.GetPropertyValue(WaveObstaclePropertyType) == "dam"
                               ? ObstacleType.Dam
                               : ObstacleType.Sheet;

                string reflType = obstacle.GetPropertyValue(WaveObstaclePropertyReflections);
                obs.ReflectionType = ReflectionType.No;
                if (reflType == "specular")
                {
                    obs.ReflectionType = ReflectionType.Specular;
                }

                if (reflType == "diffuse")
                {
                    obs.ReflectionType = ReflectionType.Diffuse;
                }

                obs.TransmissionCoefficient = GetObstaclePropertyAndLogIfFails(
                    obstacle, obstacleFile, WaveObstaclePropertyTransmissionCoefficient,
                    WaveObstacleDefaultValueTransmissionCoefficient);
                obs.Height = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, WaveObstaclePropertyHeight,
                                                              WaveObstacleDefaultValueHeight);
                obs.Alpha = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, WaveObstaclePropertyAlpha,
                                                             WaveObstacleDefaultValueAlpha);
                obs.Beta = GetObstaclePropertyAndLogIfFails(obstacle, obstacleFile, WaveObstaclePropertyBeta,
                                                            WaveObstacleDefaultValueBeta);
                obs.ReflectionCoefficient = GetObstaclePropertyAndLogIfFails(
                    obstacle, obstacleFile, WaveObstaclePropertyReflectionCoefficient,
                    WaveObstacleDefaultValueReflectionCoefficient);

                yield return obs;
            }
        }

        private double GetObstaclePropertyAndLogIfFails(DelftIniCategory obstacle, string fileName, string property,
                                                        double defaultValue)
        {
            string input = obstacle.GetPropertyValue(property);
            if (input == null)
            {
                return defaultValue;
            }

            double result;
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            Log.WarnFormat(
                "Parsing error in file '{0}'. Can't convert '{1}' to a double. The property '{2}' has been given the default value '{3}'.",
                fileName, input, property, defaultValue);
            return defaultValue;
        }

        private WaveBoundaryImportDefinitionType GetImportDefinition(string value)
        {
            switch (value)
            {
                case "orientation":
                    return WaveBoundaryImportDefinitionType.Orientation;
                case "grid-coordinates":
                    return WaveBoundaryImportDefinitionType.GridIndexBased;
                case "fromsp2file":
                    return WaveBoundaryImportDefinitionType.FromSp2File;
                case "xy-coordinates":
                    return WaveBoundaryImportDefinitionType.CoordinateBased;
                case "fromWWfile":
                    return WaveBoundaryImportDefinitionType.FromWaveWatchFile;
                default:
                    throw new ArgumentException($"Invalid boundary definition: {value}");
            }
        }

        private WaveSpectrumShapeType GetSpectrumShapeType(string value)
        {
            switch (value)
            {
                case "jonswap":
                    return WaveSpectrumShapeType.Jonswap;
                case "pierson-moskowitz":
                    return WaveSpectrumShapeType.PiersonMoskowitz;
                case "gauss":
                    return WaveSpectrumShapeType.Gauss;
                default:
                    throw new ArgumentException($"Invalid spectral shape definition: {value}");
            }
        }

        private enum WaveBoundaryImportDefinitionType
        {
            Orientation,
            GridIndexBased,
            CoordinateBased,
            FromSp2File,
            FromWaveWatchFile
        }
    }
}