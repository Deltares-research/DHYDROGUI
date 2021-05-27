using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using SharpMap;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class MduFile : FMSuiteFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MduFile));

        private static string FMSuiteFlowModelVersion;
        private static string FMDllVersion;

        public const string MduExtension = ".mdu";
        public const string LandBoundariesExtension = ".ldb";
        public const string ThinDamExtension = "_thd.pli";
        public const string ThinDamAlternativeExtension = "_thd.pliz";
        public const string FixedWeirExtension = "_fxw.pliz";
        public const string BridgePillarExtension = ".pliz";
        public const string FixedWeirAlternativeExtension = "_fxw.pli";
        public const string StructuresExtension = "_structures.ini"; // TODO: Might not want to require a specific extension
        public const string ObsExtension = "_obs.xyn";
        public const string ObsCrossExtension = "_crs.pli";
        public const string ObsCrossAlternativeExtension = "_crs.pliz";
        public const string DryAreaExtension = "_dry.pol";
        public const string DryPointExtension = "_dry.xyz";
        public const string EnclosureExtension = "_enc.pol";
        public const string RoofAreaExtension = "_roofs.pol";
        public const string MorphologyExtension = ".mor";
        public const string SedimentExtension = ".sed";

        private readonly Dictionary<string, string> mduComments = new Dictionary<string, string>();

        private LdbFile landBoundariesFile;
        private PliFile<ThinDam2D> thinDamFile;
        private PlizFile<FixedWeir> fixedWeirFile;
        private PlizFile<BridgePillar> bridgePillarFile;
        private StructuresFile structuresFile;
        private ObsFile<GroupableFeature2DPoint> obsFile;
        private PliFile<ObservationCrossSection2D> obsCrsFile;
        private PolFile<GroupableFeature2DPolygon> dryAreaFile;
        private PolFile<GroupableFeature2DPolygon> enclosureFile;
        private PolFile<GroupableFeature2DPolygon> roofAreaFile;
        

        // the following mdu-referenced files are written by the UI, or at least should not be copied along blindly 
        // (please keep this list up-to-date!):

        private static readonly string[] SupportedFiles =
        {
            KnownProperties.NetFile, KnownProperties.ExtForceFile, KnownProperties.MapFile__Obsolete,
            KnownProperties.HisFile__Obsolete, KnownProperties.ThinDamFile, KnownProperties.FixedWeirFile,
            KnownProperties.BridgePillarFile,
            KnownProperties.ObsFile, KnownProperties.ObsCrsFile, KnownProperties.LandBoundaryFile,
            KnownProperties.DryPointsFile, KnownProperties.RestartFile, KnownProperties.StructuresFile, KnownProperties.RoofAreaFile
        };

        private static readonly Dictionary<string, string> MduFilePropertyDescriptionDictionary = new Dictionary<string, string>
        {
            { KnownProperties.DryPointsFile, "DryPointsFile" },
            { KnownProperties.EnclosureFile, "EnclosureFile" },
            { KnownProperties.LandBoundaryFile, "LandBoundaryFile" },
            { KnownProperties.ThinDamFile, "ThinDamFile" },
            { KnownProperties.FixedWeirFile, "FixedWeirFile" },
            { KnownProperties.BridgePillarFile, "PillarFile" },
            { KnownProperties.StructuresFile, "StructureFile" },
            { KnownProperties.ObsFile, "ObsFile" },
            { KnownProperties.ObsCrsFile, "CrsFile" },
        };

        public MduFile()
        {
            if (FMDllVersion != null)
                return; // do it once

            var api = FlexibleMeshModelApiFactory.CreateNew();
            if (api == null)
            {
                Log.ErrorFormat("Failed to initialise FlexibleMeshModelApi");
                return;
            }

            using (api)
            {
                try
                {
                    FMDllVersion = api.GetVersionString();
                }
                catch (Exception ex)
                {
                    var exception = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    Log.ErrorFormat("Error retrieving FM Dll version: {0}", exception);

                    FMDllVersion = "Unknown";
                }
            }

            var waterFlowFMAssembly = typeof(WaterFlowFMModel).Assembly;
            FMSuiteFlowModelVersion = waterFlowFMAssembly.GetName().Version.ToString();
        }

        internal string Path { get; set; }

        public ExtForceFile ExternalForcingsFile { get; private set; }

        public BndExtForceFile BoundaryExternalForcingsFile { get; private set; }

        public MeteoExtForceFile MeteoExtForceFile { get; private set; }

        #region write logic

        public void Write(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea, IHydroNetwork network, IEnumerable<RoughnessSection> roughnessSections, IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions, IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions, IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D, IEnumerable<Model1DLateralSourceData> lateralSourcesData, IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true, bool disableFlowNodeRenumbering = false, ISedimentModelData sedimentModelData = null, bool writeStructureFile = true, string workNetFilePath = null)
        {
            var targetDir = VerifyTargetDirectory(targetMduFilePath);
            var substitutedPaths = new Dictionary<string, System.Tuple<string, string>>();

            if (workNetFilePath != null)
            {
                var targetFile = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.NetFile));
                var sourceDir = VerifyTargetDirectory(workNetFilePath);

                CopyNetFile(modelDefinition, workNetFilePath, targetFile, substitutedPaths, sourceDir, targetDir);
            }
            else if (Path != null)
            {
                CopyNetFile(targetMduFilePath, modelDefinition, substitutedPaths, targetDir);
            }

            if (switchTo)
            {
                Path = targetMduFilePath;
            }

            if (writeFeatures)
            {
                WriteAreaFeatures(targetMduFilePath, modelDefinition, hydroArea, allFixedWeirsAndCorrespondingProperties, writeStructureFile);
            }

            if (writeExtForcings)
            {
                WriteExternalForcings(targetMduFilePath, modelDefinition, hydroArea, boundaryConditions1D, lateralSourcesData);
            }

            if (modelDefinition.UseMorphologySediment)
            {
                WriteMorSedFiles(targetMduFilePath, modelDefinition, sedimentModelData);
            }

            modelDefinition.SetMduTimePropertiesFromGuiProperties();
            if(targetMduFilePath != null &&
               network != null &&
               hydroArea != null &&
               roughnessSections != null)
                FeatureFile1D2DWriter.Write1D2DFeatures(targetMduFilePath, modelDefinition, network, hydroArea, roughnessSections, channelFrictionDefinitions, channelInitialConditionDefinitions);

            // write at the end in case of updated file paths
            WriteProperties(targetMduFilePath, modelDefinition.Properties, writeExtForcings, writeFeatures, useNetCDFMapFormat:false, disableFlowNodeRenumbering:disableFlowNodeRenumbering);

            if (!switchTo)
            {
                // Revert path substitutions
                foreach (var itemPair in substitutedPaths)
                {
                    modelDefinition.GetModelProperty(itemPair.Key).SetValueAsString(itemPair.Value.Item1);
                }
            }
        }

        public void WriteProperties(string filePath, IEnumerable<WaterFlowFMProperty> modelDefinition, bool writeExtForcings, bool writeFeatures, bool writePartionFile = true, bool useNetCDFMapFormat = false, bool disableFlowNodeRenumbering = false)
        {
            var waterFlowFmProperties = modelDefinition.ToList();
            WriteMorphologySediment(filePath, waterFlowFmProperties);

            OpenOutputFile(filePath);
            try
            {
                var propertiesByGroup = GetPropertiesByGroup(waterFlowFmProperties, writeExtForcings, writeFeatures);
                 
                foreach (var propertyGroup in propertiesByGroup)
                {
                    WriteMduLine(writeExtForcings, writeFeatures, writePartionFile, useNetCDFMapFormat, disableFlowNodeRenumbering, propertyGroup);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        public void WriteBathymetry(WaterFlowFMModelDefinition modelDefinition, string path)
        {
            var bedLevelTypeProperty = modelDefinition.Properties.FirstOrDefault(p =>
                p.PropertyDefinition != null &&
                p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);

            if (bedLevelTypeProperty == null)
            {
                Log.WarnFormat("Cannot determine Bed level location, z-values will not be exported");
                return;
            }

            var location = (UGridFileHelper.BedLevelLocation)bedLevelTypeProperty.Value;
            var values = modelDefinition.Bathymetry.Components[0].GetValues<double>().ToArray();
            UGridFileHelper.WriteZValues(path, location, values);
        }

        private static string VerifyTargetDirectory(string targetMduFilePath)
        {
            var targetDir = System.IO.Path.GetDirectoryName(targetMduFilePath);
            if (targetDir != string.Empty && !Directory.Exists(targetDir))
            {
                throw new Exception("Non existing directory in file path: " + targetMduFilePath);
            }

            return targetDir;
        }

        private void CopyNetFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            Dictionary<string, System.Tuple<string, string>> substitutedPaths, string targetDir)
        {
            var sourceFile = MduFileHelper.GetSubfilePath(Path,
                modelDefinition.GetModelProperty(KnownProperties.NetFile));

            var targetFile = MduFileHelper.GetSubfilePath(targetMduFilePath,
                modelDefinition.GetModelProperty(KnownProperties.NetFile));

            CopyNetFile(modelDefinition, sourceFile, targetFile, substitutedPaths, Path, targetDir);

        }
        private void CopyNetFile(WaterFlowFMModelDefinition modelDefinition, string sourceFile, string targetFile,
            Dictionary<string, System.Tuple<string, string>> substitutedPaths, string sourceDir, string targetDir)
            {
                if (sourceFile != null)
                {
                    if (File.Exists(sourceFile) && targetFile != null)
                    {
                        var targetNcDir = System.IO.Path.GetDirectoryName(targetFile);
                        Directory.CreateDirectory(targetNcDir);

                    var fullSourcePath =
                        string.IsNullOrEmpty(sourceFile) ? string.Empty : System.IO.Path.GetFullPath(sourceFile);
                    var fullTargetPath =
                        string.IsNullOrEmpty(targetFile) ? string.Empty : System.IO.Path.GetFullPath(targetFile);

                        if (fullSourcePath.ToLower() != fullTargetPath.ToLower())
                        {
                            File.Copy(fullSourcePath, fullTargetPath, true);
                        }
                    }

                    // write the bathymetry in the net file.
                    if (modelDefinition.SpatialOperations.TryGetValue(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, out var bathymetryOperations) && File.Exists(targetFile) && bathymetryOperations.Any(so => !(so is ISpatialOperationSet)))
                    {
                        WriteBathymetry(modelDefinition, targetFile);
                    }

                    // if needed, adjust coordinate system in netfile
                    if (modelDefinition.CoordinateSystem != null && File.Exists(targetFile) && !modelDefinition.CoordinateSystem.IsNetfileCoordinateSystemUpToDate(targetFile))
                        UGridFileHelper.WriteCoordinateSystem(targetFile, modelDefinition.CoordinateSystem);
                }

                // copy along any mdu-referenced files that are *not* yet supported/written in the UI:
                // (for example: partition file, manhole file, profloc/profdef files, etc..)
                // work with the assumption that all and only file entries end with 'file' in their name
                var fileBasedProperties =
                    modelDefinition.Properties.Where(p => MduFileHelper.IsFileValued(p) &&
                                                          !SupportedFiles.Any(
                                                              sf =>
                                                                  sf.Equals(p.PropertyDefinition.MduPropertyName,
                                                                      StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();

                foreach (var fileItem in fileBasedProperties)
                {
                    var relativeSourcePath =
                        modelDefinition.GetModelProperty(fileItem.PropertyDefinition.MduPropertyName).GetValueAsString();

                    if (relativeSourcePath == null) continue;

                    var relativeTargetPath = System.IO.Path.GetFileName(relativeSourcePath);

                    if (relativeSourcePath != relativeTargetPath)
                    {
                        fileItem.SetValueAsString(relativeTargetPath);
                        substitutedPaths[fileItem.PropertyDefinition.MduPropertyName] =
                            new System.Tuple<string, string>(relativeSourcePath,
                                relativeTargetPath);
                    }

                    var absoluteSourcePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sourceDir), relativeSourcePath);
                    var absoluteTargetPath = System.IO.Path.Combine(targetDir, relativeTargetPath);

                    if (File.Exists(absoluteSourcePath))
                    {
                        var fullAbsoluteSourcePath = string.IsNullOrEmpty(absoluteSourcePath)
                            ? string.Empty
                            : System.IO.Path.GetFullPath(absoluteSourcePath);
                        var fullAbsoluteTargetPath = string.IsNullOrEmpty(absoluteTargetPath)
                            ? string.Empty
                            : System.IO.Path.GetFullPath(absoluteTargetPath);
                        if (fullAbsoluteSourcePath != fullAbsoluteTargetPath)
                        {
                            File.Copy(absoluteSourcePath, absoluteTargetPath, true);
                        }
                    }
                }
            }

        private void WriteExternalForcings(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            HydroArea hydroArea, IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D, IEnumerable<Model1DLateralSourceData> lateralSourcesData)
        {
            var exportDirectory = System.IO.Path.GetDirectoryName(targetMduFilePath);

            var extFileName = modelDefinition.GetModelProperty(KnownProperties.ExtForceFile).GetValueAsString();
            if (string.IsNullOrEmpty(extFileName))
                extFileName = modelDefinition.ModelName + ExtForceFile.Extension;
            var extForceFilePath = System.IO.Path.Combine(exportDirectory, extFileName);

            if (ExternalForcingsFile == null)
            {
                ExternalForcingsFile = new ExtForceFile();
            }

            var newBoundaryConditions = modelDefinition.BoundaryConditions.Except(ExternalForcingsFile.ExistingBoundaryConditions);
            var newBoundaries = modelDefinition.Boundaries.Except(ExternalForcingsFile.ExistingBoundaryConditions.Where(bc => bc.Feature != null)
                .Select(bc => bc.Feature));

            var newFormatBoundaryConditions = newBoundaryConditions.Any();
            var hasNewBoundaries = newBoundaries.Any();

            var hasModel1dBoundaryConditions = boundaryConditions1D != null && boundaryConditions1D.Any();
            var hasLateralSourcesData = lateralSourcesData != null && lateralSourcesData.Any();
            // TODO: fix this, also, multiple FM models for a single integrated hydroregion to be expected?!
            var hasEmbankments = hydroArea.Embankments.Any();
            modelDefinition.Embankments = hydroArea.Embankments;

            // will check if indeed the file is written)
            ExternalForcingsFile.Write(extForceFilePath, modelDefinition,
                !(newFormatBoundaryConditions || hasNewBoundaries ));

            if (newFormatBoundaryConditions || hasNewBoundaries || hasEmbankments || modelDefinition.FmMeteoFields.Any() || hasModel1dBoundaryConditions || hasLateralSourcesData)
            {
                var bndExtFileName = modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).GetValueAsString();
                if (string.IsNullOrEmpty(bndExtFileName))
                    bndExtFileName = System.IO.Path.GetFileNameWithoutExtension(extFileName) + "_bnd" +
                                     ExtForceFile.Extension;
                var bndExtForceFilePath = System.IO.Path.Combine(exportDirectory, bndExtFileName);

                if (BoundaryExternalForcingsFile == null)
                {
                    BoundaryExternalForcingsFile = new BndExtForceFile();
                }

                BoundaryExternalForcingsFile.Write(bndExtForceFilePath, modelDefinition, boundaryConditions1D, lateralSourcesData);
            }
            else if (!modelDefinition.BoundaryConditions.Any())
            {
                modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueAsString(string.Empty);
            }
        }

        private void WriteMorSedFiles(string mduPath, WaterFlowFMModelDefinition modelDefinition, ISedimentModelData sedimentModelData)
        {
            var morPath = System.IO.Path.ChangeExtension(mduPath, "mor");
            MorphologyFile.Save(morPath, modelDefinition);

            var sedPath = System.IO.Path.ChangeExtension(mduPath, "sed");
            if (sedimentModelData != null) SedimentFile.Save(sedPath, modelDefinition, sedimentModelData);
        }
        
        private void WriteMduLine(bool writeExtForcings, bool writeFeatures, bool writePartionFile, bool useNetCDFMapFormat,
            bool disableFlowNodeRenumbering, IGrouping<string, WaterFlowFMProperty> propertyGroup)
            {
                    WriteLine("");
                    WriteLine("[" + propertyGroup.Key + "]");
                    foreach (var prop in propertyGroup)
                    {
                        if (!writePartionFile && prop.PropertyDefinition.MduPropertyName.Equals("PartitionFile"))
                            continue;

                        if (useNetCDFMapFormat && prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MapFormat))
                        {
                            var line = String.Format("{0,-18}= {1,-20}{2}", prop.PropertyDefinition.MduPropertyName, 1,
                                "# For 1d2d coupling we should always write mapformat output in NetCDF format");
                            WriteLine(line.Trim());
                        }
                        else if (disableFlowNodeRenumbering && prop.PropertyDefinition.MduPropertyName.Equals("RenumberFlowNodes"))
                        {
                            var line = String.Format("{0,-18}= {1,-20}{2}", prop.PropertyDefinition.MduPropertyName, 0,
                                "# For 1d2d coupling we should never renumber the flownodes");
                            WriteLine(line.Trim());
                        }
                        else
                        {
                            var mduPropertyValue = GetPropertyValue(prop, writeExtForcings, writeFeatures);
                            WriteMduLine(prop, mduPropertyValue);
                        }
                    }
                }

        private IEnumerable<IGrouping<string, WaterFlowFMProperty>> GetPropertiesByGroup(IEnumerable<WaterFlowFMProperty> modelDefinition, bool writeExtForcings, bool writeFeatures)
            {
            WriteLine("# Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLine("# Deltares,Delft3D FM 2018 Suite Version " + FMSuiteFlowModelVersion + ", D-Flow FM Version " +
                      FMDllVersion);
            SetValueToPropertyIfExists(modelDefinition, KnownProperties.Version, FMDllVersion);
            SetValueToPropertyIfExists(modelDefinition, KnownProperties.GuiVersion, FMSuiteFlowModelVersion);
            var propertiesByGroup = modelDefinition.Where(p => p.PropertyDefinition.FileCategoryName != "GUIOnly"
                                                               && p.PropertyDefinition.FileCategoryName !=
                                                               MorphologyFile
                                                                   .MorphologyUnknownProperty /*Remove morphology unknown properties*/
                                                               && p.PropertyDefinition.FileCategoryName !=
                                                               SedimentFile
                                                                   .SedimentUnknownProperty) /*Remove sediment unknown properties that should be located on the sediment file*/
                .GroupBy(p => p.PropertyDefinition.FileCategoryName);

            propertiesByGroup =
                RemoveMorAndSedPropertiesIfNeeded(propertiesByGroup, modelDefinition, writeExtForcings, writeFeatures);
            return propertiesByGroup;
            }

        private void WriteMduLine(WaterFlowFMProperty prop, string pathValue)
        {
            var mduLine = String.Format("{0,-18}= {1,-20}{2}", prop.PropertyDefinition.MduPropertyName,
                pathValue,
                mduComments.ContainsKey(prop.PropertyDefinition.MduPropertyName)
                    ? mduComments[prop.PropertyDefinition.MduPropertyName]
                    : string.Empty);
            WriteLine(mduLine.Trim());
        }

        private static IEnumerable<IGrouping<string, WaterFlowFMProperty>> RemoveMorAndSedPropertiesIfNeeded(IEnumerable<IGrouping<string, WaterFlowFMProperty>> propertiesByGroup, IEnumerable<WaterFlowFMProperty> modelDefinition, bool writeExtForcings, bool writeFeatures)
        {
            /* Not include Morphology / Sediment MDUs if UseMorSed has not been selected */
            propertiesByGroup = propertiesByGroup.Where(p => !p.Key.Equals(KnownProperties.morphology));
            var useMorSedProp = modelDefinition.FirstOrDefault(md => md.PropertyDefinition.MduPropertyName == "UseMorSed");
            if (useMorSedProp != null)
            {
                int useMorSed;
                if (int.TryParse(GetPropertyValue(useMorSedProp, writeExtForcings, writeFeatures), out useMorSed) && useMorSed != 1)
                {
                    propertiesByGroup = propertiesByGroup.Where(p => !p.Key.Equals(KnownProperties.sediment));
                }
            }
            return propertiesByGroup;
        }

        private void WriteMorphologySediment(string mduFilePath, IEnumerable<WaterFlowFMProperty> modelDefinition)
        {
            if (modelDefinition == null) return;

            var targetDirPath = System.IO.Path.GetDirectoryName(mduFilePath);
            var targetMorFilePath = ReplaceMduExtension(mduFilePath, MorphologyExtension);
            var targetSedFilePath = ReplaceMduExtension(mduFilePath, SedimentExtension);

            var morFileProperty = modelDefinition.FirstOrDefault(p =>
                p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MorFile,
                    StringComparison.InvariantCultureIgnoreCase));
            var morFilePropValue = morFileProperty != null ? morFileProperty.GetValueAsString() : string.Empty;

            var sedFileProperty = modelDefinition.FirstOrDefault(p =>
                p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile,
                    StringComparison.InvariantCultureIgnoreCase));
            var sedFilePropValue = sedFileProperty != null ? sedFileProperty.GetValueAsString() : string.Empty;

            var currentMorFilePath = System.IO.Path.Combine(targetDirPath, morFilePropValue);
            var currentSedFilePath = System.IO.Path.Combine(targetDirPath, sedFilePropValue);

            if (currentMorFilePath != targetMorFilePath)
            {
                if (morFilePropValue != String.Empty) FileUtils.DeleteIfExists(currentMorFilePath);
                SetValueToPropertyIfExists(modelDefinition, KnownProperties.MorFile, System.IO.Path.GetFileName(targetMorFilePath));
            }

            if (currentSedFilePath != targetSedFilePath)
            {
                if (sedFilePropValue != string.Empty) FileUtils.DeleteIfExists(currentSedFilePath);
                SetValueToPropertyIfExists(modelDefinition, KnownProperties.SedFile, System.IO.Path.GetFileName(targetSedFilePath));
            }
        }

        private static void SetValueToPropertyIfExists(IEnumerable<WaterFlowFMProperty> modelDefinition, string name, string value)
        {
            if (modelDefinition == null) return;
            var waterFlowFmProperty = modelDefinition.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (waterFlowFmProperty != null)
            {
                waterFlowFmProperty.SetValueAsString(value);
            }
        }

        private static string GetPropertyValue(WaterFlowFMProperty prop, bool writeExtForcings, bool writeFeatures)
        {
            var propertyName = prop.PropertyDefinition.MduPropertyName.ToLower();
            if (!writeExtForcings &&
                (propertyName == KnownProperties.ExtForceFile || propertyName == KnownProperties.BndExtForceFile))
            {
                return string.Empty;
            }
            if (!writeFeatures &&
                (propertyName == KnownProperties.DryPointsFile || propertyName == KnownProperties.LandBoundaryFile ||
                 propertyName == KnownProperties.ThinDamFile || propertyName == KnownProperties.FixedWeirFile || propertyName == KnownProperties.BridgePillarFile ||
                 propertyName == KnownProperties.ManholeFile || propertyName == KnownProperties.ObsFile || propertyName == KnownProperties.ObsCrsFile))
            {
                return string.Empty;
            }
            return prop.GetValueAsString();
        }

        private static void WriteFeatures<TFeat, TFile>(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            string propertyKey, IList<TFeat> features, ref TFile fileWriter, string extension, params string[] alternativeExtensions)
            where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var waterFlowFMProperty = modelDefinition.GetModelProperty(propertyKey);
            if (waterFlowFMProperty == null) return;
            if (features.Any())
            {
                List<IGrouping<string, IGroupableFeature>> grouping;
                InitializeFeatureWriter(targetMduFilePath, features, extension, waterFlowFMProperty, out grouping, alternativeExtensions);
                var featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(targetMduFilePath, waterFlowFMProperty).ToList();
                featuresFilePaths.RemoveAllWhere(ffp => ffp == null);

                if (fileWriter == null)
                {
                    fileWriter = CreateFeatureFile<TFeat, TFile>(modelDefinition);
                }

                foreach (var filePath in featuresFilePaths)
                {
                    var fileName = FileUtils.GetRelativePath(System.IO.Path.GetDirectoryName(targetMduFilePath), filePath, true);
                    var fileNameWithoutExtension = string.IsNullOrEmpty(extension) ? fileName : fileName.Replace(extension, string.Empty);
                    var groupFeatures = grouping.FirstOrDefault(g => g.Key.ToLowerInvariant().Equals(fileName.ToLowerInvariant())
                        || g.Key.ToLowerInvariant().Equals(fileNameWithoutExtension.ToLowerInvariant())
                        || !string.IsNullOrEmpty(extension)
                            && g.Key.ToLowerInvariant().Replace(System.IO.Path.GetExtension(extension), string.Empty).Equals(fileNameWithoutExtension.ToLowerInvariant()));
                    var featuresToWrite = grouping.Count > 0 && groupFeatures != null
                        ? groupFeatures.Cast<TFeat>().ToList()
                        : features;
                    fileWriter.Write(filePath, featuresToWrite);
                }
            }
            else
            {
                waterFlowFMProperty.SetValueAsString(string.Empty);
            }
        }

        private static void InitializeFeatureWriter<TFeat>(string targetMduFilePath, IList<TFeat> features, string extension,
            WaterFlowFMProperty waterFlowFMProperty, out List<IGrouping<string, IGroupableFeature>> grouping, params string[] alternativeExtensions)
        {
            var defaultGroup = System.IO.Path.GetFileNameWithoutExtension(targetMduFilePath);
            MduFileHelper.UpdateFeatures(features, extension, defaultGroup);
            var writeToRelativeFilePaths = MduFileHelper.GetUniqueFilePathsForWindows(targetMduFilePath, features, extension, alternativeExtensions).Where(fp => MduFileHelper.IsValidFilePath(fp, targetMduFilePath));

            grouping = features.OfType<IGroupableFeature>().GroupBy(f => f.GroupName, StringComparer.InvariantCultureIgnoreCase).ToList();

            waterFlowFMProperty.SetValueAsString(string.Join(" ", writeToRelativeFilePaths.Select(f => System.IO.Path.HasExtension(f) ? f : string.Concat(f, extension))));
        }

        private static void AddDryAreasToWriter(string targetMduFilePath, IList<GroupableFeature2DPolygon> features, string extension,
            WaterFlowFMProperty waterFlowFMProperty, out List<IGrouping<string, IGroupableFeature>> grouping)
        {
            var defaultGroup = System.IO.Path.GetFileNameWithoutExtension(targetMduFilePath);
            MduFileHelper.UpdateFeatures(features, extension, defaultGroup);
            var writeToRelativeFilePaths = MduFileHelper.GetUniqueFilePathsForWindows(targetMduFilePath, features, extension).Where(fp => MduFileHelper.IsValidFilePath(fp, targetMduFilePath)).ToList();

            grouping = features.OfType<IGroupableFeature>().GroupBy(f => f.GroupName, StringComparer.InvariantCultureIgnoreCase).ToList();

            if (writeToRelativeFilePaths.Any())
            {
                var currentStringValue = waterFlowFMProperty.GetValueAsString();
                waterFlowFMProperty.SetValueAsString(currentStringValue + " " + string.Join(" ", writeToRelativeFilePaths.Select(f => System.IO.Path.HasExtension(f)
                                                                 ? f
                                                                 : string.Concat(f, extension))));
            }
        }

        private void WriteDryPointsAndDryAreas(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IList<GroupablePointFeature> dryPoints, IList<GroupableFeature2DPolygon> dryAreas)
        {
            var waterFlowFMProperty = modelDefinition.GetModelProperty(KnownProperties.DryPointsFile);
            if (dryPoints.Any() || dryAreas.Any())
            {
                List<IGrouping<string, IGroupableFeature>> dryPointsPerGroup;
                List<IGrouping<string, IGroupableFeature>> dryAreasPerGroup;
                InitializeFeatureWriter(targetMduFilePath, dryPoints, DryPointExtension, waterFlowFMProperty, out dryPointsPerGroup);
                AddDryAreasToWriter(targetMduFilePath, dryAreas, DryAreaExtension, waterFlowFMProperty, out dryAreasPerGroup);

                var featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(targetMduFilePath, waterFlowFMProperty).ToList();
                featuresFilePaths.RemoveAllWhere(ffp => ffp == null);

                if (dryAreaFile == null) dryAreaFile = new PolFile<GroupableFeature2DPolygon>();
                foreach (var featuresFilePath in featuresFilePaths)
                {
                    if (featuresFilePath.EndsWith(DryPointExtension))
                    {
                        // Create the directory to which the file is being written, because XyzFile class does not handle this.
                        var writeDirectory = System.IO.Path.GetDirectoryName(featuresFilePath);
                        if (!string.IsNullOrEmpty(writeDirectory) && !Directory.Exists(writeDirectory))
                        {
                            Directory.CreateDirectory(writeDirectory);
                        }

                        var fileName = FileUtils.GetRelativePath(System.IO.Path.GetDirectoryName(targetMduFilePath), featuresFilePath, true);
                        var fileNameWithoutExtension = fileName.Replace(DryPointExtension, string.Empty);
                        var groupFeatures = GetAllGroupadFeaturesFromFile<GroupablePointFeature>(dryPointsPerGroup, fileName, fileNameWithoutExtension);
                        var featuresToWrite = dryPoints.Count > 0 && groupFeatures != null ? groupFeatures : dryPoints;
                        XyzFile.Write(featuresFilePath, featuresToWrite.Select(p => new PointValue { X = p.X, Y = p.Y, Value = 0 }));
                    }
                    else if (featuresFilePath.EndsWith(DryAreaExtension))
                    {
                        var fileName = FileUtils.GetRelativePath(System.IO.Path.GetDirectoryName(targetMduFilePath), featuresFilePath, true);
                        var fileNameWithoutExtension = fileName.Replace(DryAreaExtension, string.Empty);
                        var groupFeatures = GetAllGroupadFeaturesFromFile<GroupableFeature2DPolygon>(dryAreasPerGroup, fileName, fileNameWithoutExtension);
                        var featuresToWrite = dryAreas.Count > 0 && groupFeatures != null ? groupFeatures : dryAreas;
                        dryAreaFile.Write(featuresFilePath, featuresToWrite);
                    }
                }
            }
            else
            {
                waterFlowFMProperty.SetValueAsString(string.Empty);
            }
        }

        private static TFile CreateFeatureFile<TFeat, TFile>(WaterFlowFMModelDefinition modelDefinition)
            where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var fileWriter = new TFile();
            var fixedWeirFile = fileWriter as PlizFile<FixedWeir>;
            if (fixedWeirFile != null)
            {
                fixedWeirFile.CreateDelegate = delegate (List<Coordinate> points, string name)
                {
                    var feature = new FixedWeir { Name = name, Geometry = PlizFile<FixedWeir>.CreatePolyLineGeometry(points) };
                    feature.InitializeAttributes();
                    return feature;
                };
            }

            var bridgePillarFile = fileWriter as PlizFile<BridgePillar>;
            if (bridgePillarFile != null)
            {
                bridgePillarFile.CreateDelegate = delegate (List<Coordinate> points, string name)
                {
                    return CreateDelegateBridgePillar(name, points);
                };
            }

            var structuresFileWriter = fileWriter as StructuresFile;
            if (structuresFileWriter != null)
            {
                structuresFileWriter.StructureSchema = modelDefinition.StructureSchema;
                structuresFileWriter.ReferenceDate =
                    (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            }

            var roofAreaFileWriter = fileWriter as PolFile<GroupableFeature2DPolygon>;
            if (roofAreaFileWriter != null)
            {
                roofAreaFileWriter.IncludeClosingCoordinate = true;
            }
            return fileWriter;
        }

        internal static BridgePillar CreateDelegateBridgePillar(string name, List<Coordinate> points)
        {
            var feature = new BridgePillar { Name = name };
            feature.Geometry = PlizFile<BridgePillar>.CreatePolyLineGeometry(points);
            feature.InitializeAttributes();
            return feature;
        }

        private void WriteAreaFeatures(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea, 
            IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties, bool writeStructureFile = true)
        {
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.LandBoundaryFile,
                hydroArea.LandBoundaries, ref landBoundariesFile, LandBoundariesExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams.ToList(),
                ref thinDamFile, ThinDamExtension, ThinDamAlternativeExtension);

            //fix attributes for fixed weirs. Create attributes from modelfeaturecoordinatdata.
            foreach (var fixedWeir in hydroArea.FixedWeirs)
            {
                fixedWeir.Attributes = new DictionaryFeatureAttributeCollection();

                var correspondingModelFeatureCoordinateData =
                    allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir);

                if (correspondingModelFeatureCoordinateData == null) break;

                for (var index = 0; index < correspondingModelFeatureCoordinateData.DataColumns.Count; index++)
                {
                    if (!correspondingModelFeatureCoordinateData.DataColumns[index].IsActive) break;
                    var dataColumnWithData = correspondingModelFeatureCoordinateData.DataColumns[index].ValueList;

                    GeometryPointsSyncedList<double> syncedList;
                    syncedList = new GeometryPointsSyncedList<double>
                    {
                        CreationMethod = (f, i) => 0.0,
                        Feature = fixedWeir
                    };
                    fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[index]] = syncedList;

                    for (var i = 0; i < dataColumnWithData.Count; ++i)
                    {
                        syncedList[i] = (double)dataColumnWithData[i];
                    }
                }
            }

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.FixedWeirFile, hydroArea.FixedWeirs.ToList(),
                ref fixedWeirFile, FixedWeirExtension, FixedWeirAlternativeExtension);

            foreach (var fixedWeir in hydroArea.FixedWeirs)
            {
                fixedWeir.Attributes.Clear();
            }

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.BridgePillarFile, hydroArea.BridgePillars,
                ref bridgePillarFile, BridgePillarExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ObsFile, hydroArea.ObservationPoints,
                ref obsFile, ObsExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ObsCrsFile, hydroArea.ObservationCrossSections.ToList(),
                ref obsCrsFile, ObsCrossExtension, ObsCrossAlternativeExtension);
            
            WriteDryPointsAndDryAreas(targetMduFilePath, modelDefinition, hydroArea.DryPoints, hydroArea.DryAreas);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.EnclosureFile, hydroArea.Enclosures,
                ref enclosureFile, EnclosureExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.RoofAreaFile, hydroArea.RoofAreas, ref roofAreaFile, RoofAreaExtension);
        }

        public void WriteLandBoundaries(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea)
        {
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.LandBoundaryFile,
                hydroArea.LandBoundaries, ref landBoundariesFile, LandBoundariesExtension);
        }

        private static string ReplaceMduExtension(string mduFilePath, string newExtension)
        {
            return mduFilePath.Substring(0, mduFilePath.Length - MduExtension.Length) + newExtension;
        }

        private static List<TFeat> GetAllGroupadFeaturesFromFile<TFeat>(IEnumerable<IGrouping<string, IGroupableFeature>> grouping, string fileName, string fileNameWithoutExtension)
            where TFeat : IGroupableFeature
        {
            var groupFeatures =
                grouping.FirstOrDefault(g => g.Key.ToLowerInvariant().Equals(fileName.ToLowerInvariant())
                                             || g.Key.ToLowerInvariant().Equals(fileNameWithoutExtension.ToLowerInvariant())
                                             || g.Key.ToLowerInvariant().Replace(System.IO.Path.GetExtension(DryAreaExtension), string.Empty).Equals(fileNameWithoutExtension.ToLowerInvariant()));
            if (groupFeatures == null) return null;

            try
            {
                var groupFeaturesList = groupFeatures.Cast<TFeat>().ToList();
                return groupFeaturesList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region read logic

        public void Read(string filePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea, IHydroNetwork network, IDiscretization discretization, IEventedList<Model1DBoundaryNodeData> boundaryConditions1D, IEventedList<Model1DLateralSourceData> lateralSourcesData, IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties, Action<string, int, int> reportProgress = null, IList<ModelFeatureCoordinateData<BridgePillar>> allBridgePillarsAndCorrespondingProperties = null)
        {
            if (reportProgress == null) reportProgress = (name, current, total) => { };
            var totalSteps = 6;

            reportProgress("Reading properties", 1, totalSteps);
            ReadProperties(filePath, modelDefinition);

            reportProgress("Reading morphology properties", 2, totalSteps);
            MorphologyFile.Read(filePath, modelDefinition);

            reportProgress("Reading area features", 3, totalSteps);
            ReadAreaFeatures(filePath, modelDefinition, hydroArea);

            //fix for fixed weirs

            foreach (var fixedWeir in hydroArea.FixedWeirs)
            {
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir>() { Feature = fixedWeir };
                var scheme = modelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();
                modelFeatureCoordinateData.UpdateDataColumns(scheme);

                var locationKeyFound = fixedWeir.Attributes.ContainsKey(Feature2D.LocationKey);
                var indexKey = !locationKeyFound ? -1 : fixedWeir.Attributes.Keys.ToList().IndexOf(Feature2D.LocationKey);

                var numberFixedWeirAttributes = !locationKeyFound ? fixedWeir.Attributes.Count : (fixedWeir.Attributes.Count - 1);

                var difference = Math.Abs(modelFeatureCoordinateData.DataColumns.Count - numberFixedWeirAttributes);

                if (modelFeatureCoordinateData.DataColumns.Count < fixedWeir.Attributes.Count)
                {
                    Log.Warn($"Based on the Fixed Weir Scheme {scheme}, there are too many column(s) defined for {fixedWeir} in the imported fixed weir file. The last {difference} column(s) have been ignored");
                }

                if (modelFeatureCoordinateData.DataColumns.Count > fixedWeir.Attributes.Count)
                {
                    Log.Warn($"Based on the Fixed Weir Scheme {scheme}, there are not enough column(s) defined for {fixedWeir} in the imported fixed weir file. The last {difference} column(s) have been generated using default values");
                }

                for (var index = 0; index < modelFeatureCoordinateData.DataColumns.Count; index++)
                {
                    if (index < fixedWeir.Attributes.Count)
                    {
                        if (index == indexKey) continue;


                        var dataColumn = modelFeatureCoordinateData.DataColumns[index];
                        var attributeWithListOfLoadedData =
                            fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[index]] as
                                GeometryPointsSyncedList<double>;
                        LoadAttributeIntoDataColumn(attributeWithListOfLoadedData, dataColumn);
                    }
                    else
                    {
                        break;
                    }
                }
                allFixedWeirsAndCorrespondingProperties.Add(modelFeatureCoordinateData);
            }

            foreach (var fixedWeir in hydroArea.FixedWeirs)
            {
                fixedWeir.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
            }

            if (allBridgePillarsAndCorrespondingProperties != null)
            {
                foreach (var bridgePillar in hydroArea.BridgePillars)
                {
                    var modelFeatureCoordinateData =
                        new ModelFeatureCoordinateData<BridgePillar>() { Feature = bridgePillar };

                    var bpFile = modelDefinition.GetModelProperty(KnownProperties.BridgePillarFile).GetValueAsString();
                    modelFeatureCoordinateData.UpdateDataColumns();

                    var difference =
                        Math.Abs(modelFeatureCoordinateData.DataColumns.Count - bridgePillar.Attributes.Count);

                    if (modelFeatureCoordinateData.DataColumns.Count < bridgePillar.Attributes.Count)
                    {
                        Log.Warn(
                            string.Format(Resources.MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_too_many_column_s__defined_for__1___The_last__2__column_s__have_been_ignored, bpFile, bridgePillar, difference));
                    }

                    if (modelFeatureCoordinateData.DataColumns.Count > bridgePillar.Attributes.Count)
                    {
                        Log.Warn(
                            string.Format(Resources.MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_not_enough_column_s__defined_for__1___The_last__2__column_s__have_been_generated_using_default_values, bpFile, bridgePillar?.Name, difference));
                    }

                    SetBridgePillarDataModel(allBridgePillarsAndCorrespondingProperties, modelFeatureCoordinateData, bridgePillar);
                }

                foreach (var bridgePillar in hydroArea.BridgePillars)
                {
                    bridgePillar.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
                }
            }
            string netFilePath = MduFileHelper.GetSubfilePath(filePath, modelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!string.IsNullOrEmpty(netFilePath) && File.Exists(netFilePath))
                UGridFileHelper.ReadNetworkAndDiscretisation(netFilePath, discretization, network, NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netFilePath), NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netFilePath));

            reportProgress("Reading external forcings file", 4, totalSteps);
            var extForceFileProperty = modelDefinition.GetModelProperty(KnownProperties.ExtForceFile);
            if (extForceFileProperty != null)
            {
                var forceFilePath = MduFileHelper.GetSubfilePath(filePath,
                    modelDefinition.GetModelProperty(KnownProperties.ExtForceFile));

                if (forceFilePath != null && File.Exists(forceFilePath))
                {
                    ExternalForcingsFile = new ExtForceFile();
                    ExternalForcingsFile.Read(forceFilePath, modelDefinition);
                }
            }

            reportProgress("Reading boundary external forcings file", 5, totalSteps);
            var bndExtForceFileProperty = modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile);
            if (bndExtForceFileProperty != null)
            {
                var forceFilePath = MduFileHelper.GetSubfilePath(filePath, bndExtForceFileProperty);

                if (forceFilePath != null && File.Exists(forceFilePath))
                {
                    BoundaryExternalForcingsFile = new BndExtForceFile();
                    BoundaryExternalForcingsFile.Read(forceFilePath, modelDefinition, network, boundaryConditions1D, lateralSourcesData);
                }
            }

            reportProgress("Reading fm meteo external forcings file", 6, totalSteps);
            

            hydroArea.Embankments.AddRange(modelDefinition.Embankments);
        }
        
        private static void LoadAttributeIntoDataColumn(GeometryPointsSyncedList<double> loadedData, IDataColumn dataColumn)
        {
            // Just a refactor of the setter.
            if (dataColumn == null || loadedData == null) return;
            dataColumn.ValueList = loadedData.ToList();
        }

        private void ReadProperties(string filePath, WaterFlowFMModelDefinition definition)
        {
            Path = filePath;
            OpenInputFile(filePath);
            IgnoreCommentLines(new[] { "# Generated on", "# Generated by", "# Deltares, " });
            try
            {
                string currentGroupName = null;
                string line = GetNextLine();
                bool readNextLine = true;
                while (line != null)
                {
                    line = line.Trim().Replace("\0", " ");  // some mdu files contain null characters (generated by interactor GUI)
                    if (line.StartsWith("["))
                    {
                        // group line
                        int endIndex = line.LastIndexOf("]", StringComparison.Ordinal);
                        if (endIndex < 2)
                        {
                            throw new FormatException(String.Format("Invalid group on line {0} in file {1}", LineNumber, filePath));
                        }
                        currentGroupName = line.Substring(1, endIndex - 1).Trim();
                        if (currentGroupName.ToLower().Equals("structure"))
                        {
                            // put structure block
                            // 
                            StructuresFile.ParseStructure(this);
                            continue;
                        }
                    }
                    else
                    {
                        // property line
                        string mduPropertyName;
                        string mduPropertyLowerCase;
                        var fields = GetPropertyLine(line, out mduPropertyName, out mduPropertyLowerCase);

                        // some backwards compatibility issues (properties have been renamed
                        if (mduPropertyLowerCase.Equals("enclosurefile"))
                        {
                            mduPropertyName = "GridEnclosureFile";
                        }
                        if (mduPropertyLowerCase.Equals("trtdt"))
                        {
                            mduPropertyName = "DtTrt";
                        }
                        if (mduPropertyLowerCase.Equals("botlevuni"))
                        {
                            mduPropertyName = "BedLevUni";
                        }
                        if (mduPropertyLowerCase.Equals("botlevtype"))
                        {
                            mduPropertyName = "BedLevType";
                        }
                        if (mduPropertyLowerCase.Equals("hdam"))
                        {
                            line = GetNextLine();
                            continue;
                        }
                        mduPropertyLowerCase = mduPropertyName.ToLower();

                        string mduPropertyValue = fields[1].Trim();

                        if (mduPropertyLowerCase == KnownProperties.FixedWeirScheme && (mduPropertyValue != "0" && mduPropertyValue != "6" && mduPropertyValue != "8" && mduPropertyValue != "9"))
                        {
                            Log.Warn(string.Format("Obsolete Fixed Weir Scheme {0} detected and it will be corrected to the default Numerical Scheme.", mduPropertyValue));
                            mduPropertyValue = "6";
                        }
                        GetPropertyComment(line, mduPropertyName, fields.Length > 2, false);

                        if (!definition.ContainsProperty(mduPropertyLowerCase))
                        {
                            string mduComment = null;
                            if (mduComments.ContainsKey(mduPropertyName))
                            {
                                mduComment = mduComments[mduPropertyName];
                            }
                            // create definition for unknown property:
                            var propDef =
                                WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(currentGroupName,
                                    mduPropertyName, mduComment);

                            definition.AddProperty(new WaterFlowFMProperty(propDef, mduPropertyValue));
                        }
                        if (!string.IsNullOrEmpty(mduPropertyValue))
                        {
                            var property = definition.GetModelProperty(mduPropertyLowerCase);
                            if (mduPropertyValue.EndsWith(@"\"))
                            {
                                mduPropertyValue = GetMduPropertyMultipleLineValueRecursive(mduPropertyName, mduPropertyValue, ref line, ref readNextLine);
                            }

                            property.SetValueAsString(mduPropertyValue);
                        }
                    }

                    if (readNextLine)
                    {
                        line = GetNextLine();
                    }

                    readNextLine = true; //Reset it if needed.

                }
            }
            finally
            {
                CloseInputFile();
            }

            definition.GetModelProperty(KnownProperties.RefDate).Value =
                DataTypeValueParser.ParseFMDateTime(definition.GetModelProperty(KnownProperties.RefDate).GetValueAsString());

            definition.SetGuiTimePropertiesFromMduProperties();

            // update the heat flux model in the definition, because the event of KnownProperties.Temperature is not bubbled during loading of an mdu file.
            definition.UpdateHeatFluxModel();

            // update the write shape file properties as they are not visible in the gui.
            definition.UpdateWriteOutputSnappedFeatures();
        }

        private void GetPropertyComment(string line, string mduPropertyName, bool condition, bool isMultipleLine)
        {
            if (condition)
            {
                int commentStart = line.IndexOf('#');
                if (commentStart > 0)
                {

                    mduComments[mduPropertyName] = isMultipleLine
                        ? (mduComments.ContainsKey(mduPropertyName)
                            ? mduComments[mduPropertyName]
                            : "#")
                            + line.Substring(commentStart + 1)
                        : line.Substring(commentStart);
                }
            }
        }

        private static string[] GetPropertyLine(string line, out string mduPropertyName, out string mduPropertyLowerCase)
        {
            string[] fields = line.Split(new[] { '=', '#' });
            mduPropertyName = fields[0].Trim();

            mduPropertyLowerCase = mduPropertyName.ToLower();
            return fields;
        }

        private string GetMduPropertyMultipleLineValueRecursive(string mduPropertyName, string mduPropertyValue, ref string line, ref bool readNextLine)
        {
            line = GetNextLine();
            if (line == null || !mduPropertyValue.EndsWith(@"\"))
            {
                readNextLine = false;
                return mduPropertyValue;
            }

            mduPropertyValue = mduPropertyValue.Replace('\\', ' ');

            /* Check if it's a new property or a new line value */
            var lineValueWithComment = line.Split('#');
            var lineValue = lineValueWithComment[0];
            if (lineValue.Contains("="))
            {
                //It actually read a new property display a log warning / error and keep on reading the mdu file.
                Log.WarnFormat("Found a new property while the mdu expected a new multiple line value {0}", line);
                readNextLine = false;
                return mduPropertyValue;
            }

            GetPropertyComment(line, mduPropertyName, line.Split('#').Length == 2, true);

            if (!string.IsNullOrEmpty(lineValue))
            {
                var multipleLineValues = GetMduPropertyMultipleLineValueRecursive(mduPropertyName, lineValue.Trim(), ref line, ref readNextLine);
                mduPropertyValue += multipleLineValues;
            }

            return mduPropertyValue;
        }

        private static void ReadFeatures<TFeat, TFile>(string mduFilePath, WaterFlowFMModelDefinition modelDefinition,
            string propertyKey, IList<TFeat> features, ref TFile fileReader, string extension) where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var replacedFilePaths = new Dictionary<string, string>();
            var modelProperty = modelDefinition.GetModelProperty(propertyKey);
            var featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelProperty);
            RemoveBadFilePaths(ref featuresFilePaths, mduFilePath, modelDefinition, propertyKey);
            CopyFilesToProjectFolderIfNeeded(featuresFilePaths, mduFilePath, modelDefinition, propertyKey, ref replacedFilePaths);

            if (featuresFilePaths == null || featuresFilePaths.Count == 0) return;

            fileReader = CreateFeatureFile<TFeat, TFile>(modelDefinition);

            var readFeatures = new List<TFeat>();
            foreach (var featuresFilePath in featuresFilePaths)
            {
                IList<TFeat> featuresToAdd;
                if (fileReader is StructuresFile)
                {
                    var structuresFile = fileReader as StructuresFile;
                    featuresToAdd = (IList<TFeat>)structuresFile.CopyFileAndRead(featuresFilePath, replacedFilePaths[featuresFilePath]);
                }
                else
                {
                    featuresToAdd = fileReader.Read(featuresFilePath);
                }

                if (modelProperty.PropertyDefinition.IsMultipleFile)
                {
                    //make sure the features have the right group name.
                    var asGroupable = featuresToAdd.OfType<IGroupableFeature>().ToList();
                    var featurePathName = FileUtils.GetRelativePath(System.IO.Path.GetDirectoryName(mduFilePath), featuresFilePath, true);
                    var mduPathName = System.IO.Path.GetFileNameWithoutExtension(mduFilePath);
                    asGroupable.ForEach(f =>
                    {
                        f.GroupName = featurePathName;
                        f.IsDefaultGroup = featurePathName != null &&
                                           featurePathName.Replace(extension, String.Empty).Trim().Equals(mduPathName);
                    });
                }
                readFeatures.AddRange(featuresToAdd);
            }

            NamingHelper.MakeNamesUnique(readFeatures.Cast<INameable>().ToList());

            features.AddRange(readFeatures);
        }

        private void ReadAreaFeatures(string filePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea)
        {
            ReadFeatures(filePath, modelDefinition, KnownProperties.LandBoundaryFile, hydroArea.LandBoundaries,
                ref landBoundariesFile, LandBoundariesExtension);

            ReadFeatures(filePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams, ref thinDamFile, ThinDamExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.FixedWeirFile, hydroArea.FixedWeirs, ref fixedWeirFile, FixedWeirExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.ObsFile, hydroArea.ObservationPoints, ref obsFile, ObsExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.ObsCrsFile, hydroArea.ObservationCrossSections, ref obsCrsFile, ObsCrossExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.BridgePillarFile, hydroArea.BridgePillars, ref bridgePillarFile, BridgePillarExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.RoofAreaFile, hydroArea.RoofAreas, ref roofAreaFile, RoofAreaExtension);


            var structures = new List<IStructure>();

            ReadFeatures(filePath, modelDefinition, KnownProperties.StructuresFile, structures, ref structuresFile, StructuresExtension);

            foreach (var structure in structures)
            {
                if (structure is Pump2D)
                {
                    hydroArea.Pumps.Add((Pump2D)structure);
                }
                else if (structure is Weir2D)
                {
                    hydroArea.Weirs.Add((Weir2D)structure);
                }
                else if (structure is Gate2D)
                {
                    hydroArea.Gates.Add((Gate2D)structure);
                }
                else if(structure is LeveeBreach)
                {
                    hydroArea.LeveeBreaches.Add((LeveeBreach)structure);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            ReadDryPointsAndDryAreas(filePath, modelDefinition, hydroArea);

            var enclosureMultipleFilePath = MduFileHelper.GetMultipleSubfilePath(filePath,
                modelDefinition.GetModelProperty(KnownProperties.EnclosureFile));

            var enclosuresToRemove = enclosureMultipleFilePath.Where(efp => !efp.EndsWith(PolFile<GroupableFeature2DPolygon>.Extension)).ToList();
            if (enclosuresToRemove.Count > 0)
            {
                Log.WarnFormat("The following enclosure files do not contain the correct extension and will not be read. {0}", enclosuresToRemove);
                enclosureMultipleFilePath.RemoveAllWhere(efp => !efp.EndsWith(PolFile<GroupableFeature2DPolygon>.Extension));
            }

            if (enclosureMultipleFilePath.Any())
            {
                ReadFeatures(filePath, modelDefinition, KnownProperties.EnclosureFile, hydroArea.Enclosures, ref enclosureFile, EnclosureExtension);

                if (hydroArea.Enclosures.Count > 1)
                {
                    /*We do not support more than one enclosure, but the user should still be able to import everything. */
                    Log.WarnFormat(Resources.MduFile_ReadAreaFeatures_Multiple_enclosures_added_to_the_model__Validate_or_run_will_fail_if_more_than_one_enclosure_is_present_);
                }
            }
        }

        private void ReadDryPointsAndDryAreas(string mduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea)
        {
            var replacedFilePaths = new Dictionary<string, string>();
            var dryPointsPropertyKey = KnownProperties.DryPointsFile;
            var mduPathName = System.IO.Path.GetFileNameWithoutExtension(mduFilePath);

            var modelProperty = modelDefinition.GetModelProperty(dryPointsPropertyKey);
            var featureFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelProperty);
            RemoveBadFilePaths(ref featureFilePaths, mduFilePath, modelDefinition, dryPointsPropertyKey);
            CopyFilesToProjectFolderIfNeeded(featureFilePaths, mduFilePath, modelDefinition, dryPointsPropertyKey, ref replacedFilePaths);
            if (!featureFilePaths.Any()) return;

            if (dryAreaFile == null) dryAreaFile = new PolFile<GroupableFeature2DPolygon>();
            foreach (var featureFilePath in featureFilePaths)
            {
                var groupName = FileUtils.GetRelativePath(System.IO.Path.GetDirectoryName(mduFilePath), featureFilePath, true);
                if (featureFilePath.EndsWith(DryPointExtension))
                {
                    var dryPointsToAdd = XyzFile.Read(featureFilePath).Select(p => new GroupablePointFeature
                    {
                        Geometry = p.Geometry,
                        GroupName = groupName,
                        IsDefaultGroup = groupName != null && groupName.Replace(DryPointExtension, string.Empty).Trim().Equals(mduPathName)
                    });
                    hydroArea.DryPoints.AddRange(dryPointsToAdd);
                }
                else if (featureFilePath.EndsWith(DryAreaExtension))
                {
                    var dryAreasToAdd = dryAreaFile.Read(featureFilePath);
                    dryAreasToAdd.ForEach(f =>
                    {
                        f.GroupName = groupName;
                        f.IsDefaultGroup = groupName != null && groupName.Replace(DryAreaExtension, string.Empty).Trim().Equals(mduPathName);
                    });
                    hydroArea.DryAreas.AddRange(dryAreasToAdd);
                }
            }
        }

        /// <summary>
        /// Removes file paths from featureGroupNames List, whenever they do not exist or whenever the files contain references to files that do not exist. 
        /// Filtering out all these file paths is important for importing an mdu file, because it will just skip over the invalid file paths and will read
        /// the existing ones. The corresponding ModelDefinition property is updated for all changes.
        /// </summary>
        /// <param name="featureFilePaths">The group names of all features, retrieved from the mdu file of the FM Model.</param>
        /// <param name="mduFilePath">The file path of the mdu file.</param>
        /// <param name="modelDefinition">The model definition of the FM Model.</param>
        /// <param name="propertyKey">The key that corresponds to the type of file that is being read.</param>
        private static void RemoveBadFilePaths(ref IList<string> featureFilePaths, string mduFilePath, WaterFlowFMModelDefinition modelDefinition, string propertyKey)
        {
            if (featureFilePaths.Count == 0) return;
            featureFilePaths = featureFilePaths.Select(fp =>
            {
                while (fp.StartsWith(@"\") || fp.StartsWith("/")) fp = fp.Remove(0, 1);
                return fp;
            }).ToList();

            RemoveAllNonExistentFilePaths(featureFilePaths, mduFilePath, modelDefinition, propertyKey);
            RemoveAllStructuresFilesWithBadReferences(featureFilePaths, modelDefinition);
            modelDefinition.GetModelProperty(propertyKey).SetValueAsString(string.Join(" ", featureFilePaths));
        }

        /// <summary>
        /// Removes all file paths that point to a file location that does not exist.
        /// </summary>
        /// <param name="featureFilePaths">The group names of all features, retrieved from the mdu file of the FM Model.</param>
        /// <param name="mduFilePath">The file path of the mdu file.</param>
        /// <param name="modelDefinition">The model definition of the FM Model.</param>
        /// <param name="propertyKey">The key that corresponds to the type of file that is being read.</param>
        private static void RemoveAllNonExistentFilePaths(ICollection<string> featureFilePaths, string mduFilePath, WaterFlowFMModelDefinition modelDefinition, string propertyKey)
        {
            var nonExistentFilePaths = featureFilePaths.Where(fp => fp != null && !File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mduFilePath), fp)));
            foreach (var filePath in nonExistentFilePaths)
            {
                Log.WarnFormat(Resources.MduFile_RemoveNonExistentFilePaths_, filePath, mduFilePath,
                    MduFilePropertyDescriptionDictionary[propertyKey], modelDefinition.ModelName);
            }
            featureFilePaths.RemoveAllWhere(gn => gn == null || nonExistentFilePaths.Contains(gn));
        }

        /// <summary>
        /// Removes all structures files that contain references to feature files that do not exist.
        /// </summary>
        /// <param name="featureFilePaths">The group names of all features, retrieved from the mdu file of the FM Model.</param>
        /// <param name="modelDefinition">The model definition of the FM Model.</param>
        private static void RemoveAllStructuresFilesWithBadReferences(ICollection<string> featureFilePaths, WaterFlowFMModelDefinition modelDefinition)
        {
            var structureFilesWithBadReferences = new List<string>();
            foreach (var filePath in featureFilePaths.Where(fp => fp.EndsWith(StructuresExtension)))
            {
                var structureFilePath = System.IO.Path.GetFullPath(filePath);
                var fileReader = new StructuresFile
                {
                    StructureSchema = modelDefinition.StructureSchema,
                    ReferenceDate = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value
                };
                var featuresToAdd = fileReader.ReadStructures2D(structureFilePath).ToList();
                var referencesToNonExistentFilesExist = false;
                featuresToAdd.ForEach(f =>
                {
                    var featureFileName = f.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString();
                    var featureFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(structureFilePath), featureFileName);
                    if (!File.Exists(featureFilePath))
                    {
                        referencesToNonExistentFilesExist = true;
                        Log.ErrorFormat(Resources.MduFile_RemoveAllStructuresFilesWithBadReferences_, featureFilePath, structureFilePath);
                    }
                });
                if (referencesToNonExistentFilesExist) structureFilesWithBadReferences.Add(structureFilePath);
            }
            featureFilePaths.RemoveAllWhere(gn => structureFilesWithBadReferences.Contains(gn));
        }

        /// <summary>
        /// This method copies feature files at the given featureGroupNames to locations in the mdu folder if the file paths point to a location
        /// outside of the mdu folder. In case a file path has '../' in its path, the path is replaced by its absolute path. The corresponding ModelProperty
        /// is updated for every file path change.
        /// </summary>
        /// <param name="featureGroupNames">The group names of all features, retrieved from the mdu file of the FM Model.</param>
        /// <param name="mduFilePath">The file path of the mdu file.</param>
        /// <param name="modelDefinition">The model definition of the FM Model.</param>
        /// <param name="propertyKey">The key that corresponds to the type of file that is being read.</param>
        /// <param name="oldFilePaths">Dictionary that relates the resulting file paths to their original file paths.</param>
        private static void CopyFilesToProjectFolderIfNeeded(IList<string> featureGroupNames, string mduFilePath, WaterFlowFMModelDefinition modelDefinition, string propertyKey, ref Dictionary<string, string> oldFilePaths)
        {
            var mduDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(mduFilePath));
            for (var i = 0; i < featureGroupNames.Count; i++)
            {
                var filePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(mduDirectory, featureGroupNames[i]));
                var isOutsideMduFolderMatch = new Regex(@"\.{2,}").Match(FileUtils.GetRelativePath(mduDirectory, filePath, true));
                if (!isOutsideMduFolderMatch.Success) // File is situated inside mdu-folder or in a subfolder
                {
                    featureGroupNames[i] = filePath;
                    oldFilePaths.Add(filePath, filePath);
                }
                else // File is situated outside of mdu-folder
                {
                    var newFilePath = System.IO.Path.Combine(mduDirectory, System.IO.Path.GetFileName(filePath));
                    if (File.Exists(newFilePath))
                    {
                        Log.ErrorFormat(
                            Resources.MduFile_CopyFilesToProjectFolderIfNeeded_CopyingFileDidNotSucceedBecauseFileAlreadyExists,
                            filePath, newFilePath);
                        featureGroupNames[i] = null;
                        continue;
                    }

                    File.Copy(filePath, newFilePath);
                    oldFilePaths.Add(newFilePath, filePath);
                    Log.InfoFormat(
                        Resources.MduFile_CopyFilesToProjectFolderIfNeeded_CopiedFileFrom_0_to_1_BecauseTheFileExistedOutsideOfTheProjectFolder,
                        filePath, newFilePath, modelDefinition.ModelName);
                    featureGroupNames[i] = newFilePath;
                }
            }
            featureGroupNames.RemoveAllWhere(fp => fp == null);
            modelDefinition.GetModelProperty(propertyKey).SetValueAsString(string.Join(" ", featureGroupNames.Select(fp => FileUtils.GetRelativePath(mduDirectory, fp, true))));
        }

        #endregion

        #region bridge pillar helper

        /// <summary>
        /// Sets the bridge pillar data model.
        /// </summary>
        /// <param name="allBridgePillarsAndCorrespondingProperties">All bridge pillars and corresponding properties.</param>
        /// <param name="modelFeatureCoordinateData">The model feature coordinate data.</param>
        /// <param name="bridgePillar">The bridge pillar.</param>
        internal static void SetBridgePillarDataModel(IList<ModelFeatureCoordinateData<BridgePillar>> allBridgePillarsAndCorrespondingProperties,
            ModelFeatureCoordinateData<BridgePillar> modelFeatureCoordinateData, BridgePillar bridgePillar)
        {
            if (allBridgePillarsAndCorrespondingProperties == null
                || modelFeatureCoordinateData == null
                || bridgePillar == null)
                return;

            /*Add the attributes from the Bridge pillar into the model data IF they exist.*/
            var attrList = bridgePillar.Attributes.ToList();
            attrList.ForEach(attr =>
                LoadAttributeIntoDataColumn(
                    attr.Value as GeometryPointsSyncedList<double>,
                    modelFeatureCoordinateData.DataColumns.ElementAtOrDefault(attrList.IndexOf(attr))));

            allBridgePillarsAndCorrespondingProperties.Add(modelFeatureCoordinateData);
        }

        /// <summary>
        /// Sets the bridge pillar extra attributes (besides the classic x,y).
        /// </summary>
        /// <param name="bridgePillars">The bridge pillars.</param>
        /// <param name="modelFeatureCoordinateDatas">The model feature coordinate datas.</param>
        internal static void SetBridgePillarAttributes(IEnumerable<BridgePillar> bridgePillars, IList<ModelFeatureCoordinateData<BridgePillar>> modelFeatureCoordinateDatas)
        {
            foreach (var bridgePillar in bridgePillars)
            {
                bridgePillar.Attributes = new DictionaryFeatureAttributeCollection();

                var modelFeatureCoordinateData = modelFeatureCoordinateDatas.FirstOrDefault(d => d.Feature == bridgePillar);
                if (modelFeatureCoordinateData == null) return;
                for (var index = 0; index < modelFeatureCoordinateData.DataColumns.Count; index++)
                {
                    if (!modelFeatureCoordinateData.DataColumns[index].IsActive) break;
                    var dataColumnWithData = modelFeatureCoordinateData.DataColumns[index].ValueList;

                    var syncedList = new GeometryPointsSyncedList<double>
                    {
                        CreationMethod = (f, i) => 0.0,
                        Feature = bridgePillar
                    };
                    bridgePillar.Attributes[PlizFile<BridgePillar>.NumericColumnAttributesKeys[index]] = syncedList;

                    for (var i = 0; i < dataColumnWithData.Count; ++i)
                    {
                        syncedList[i] = (double)dataColumnWithData[i];
                    }
                }
            }
        }

        /// <summary>
        /// Cleans the bridge pillar attributes.
        /// </summary>
        /// <param name="bridgePillars">The bridge pillars.</param>
        internal static void CleanBridgePillarAttributes(IEnumerable<BridgePillar> bridgePillars)
        {
            if (bridgePillars == null) return;
            bridgePillars.ForEach(bp => bp.Attributes.Clear());
        }

        #endregion
    }
}
