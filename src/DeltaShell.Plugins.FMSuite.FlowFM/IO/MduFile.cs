using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO;
using Deltares.Infrastructure.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
        public const string StructuresExtension = "_structures.ini";
        public const string ObsExtension = "_obs.xyn";
        public const string ObsCrossExtension = "_crs.pli";
        public const string ObsCrossAlternativeExtension = "_crs.pliz";
        public const string DryAreaExtension = "_dry.pol";
        public const string DryPointExtension = "_dry.xyz";
        public const string EnclosureExtension = "_enc.pol";
        public const string MorphologyExtension = ".mor";
        public const string SedimentExtension = ".sed";
        public const string GullyFileName = "gullies.gui";
        
        private readonly Dictionary<string, string> mduComments = new Dictionary<string, string>();

        private LdbFile landBoundariesFile;
        private PliFile<ThinDam2D> thinDamFile;
        private PlizFile<FixedWeir> fixedWeirFile;
        private PlizFile<BridgePillar> bridgePillarFile;
        private StructuresFile structuresFile;
        private Feature2DPointFile<ObservationPoint2D> obsFile;
        private PliFile<ObservationCrossSection2D> obsCrsFile;
        private PolFile<GroupableFeature2DPolygon> dryAreaFile;
        private PolFile<GroupableFeature2DPolygon> enclosureFile;
        
        private readonly Feature2DPointFile<Gully> gullyFile;
        private readonly FeatureFile1D2DReader featureFileReader;
        private readonly FeatureFile1D2DWriter featureFileWriter;

        private readonly IFileSystem fileSystem;

        // the following mdu-referenced files are written by the UI, or at least should not be copied along blindly 
        // (please keep this list up-to-date!):

        private static readonly string[] supportedFiles =
        {
            KnownProperties.BndExtForceFile,
            KnownProperties.BranchFile,
            KnownProperties.BridgePillarFile,
            KnownProperties.CrossDefFile,
            KnownProperties.CrossLocFile,
            KnownProperties.DryPointsFile,
            KnownProperties.ExtForceFile,
            KnownProperties.FixedWeirFile,
            KnownProperties.FrictFile,
            KnownProperties.HisFile,
            KnownProperties.IniFieldFile,
            KnownProperties.LandBoundaryFile,
            KnownProperties.MapFile,
            KnownProperties.NetFile,
            KnownProperties.ObsCrsFile,
            KnownProperties.ObsFile,
            KnownProperties.RestartFile,
            KnownProperties.StorageNodeFile,
            KnownProperties.StructuresFile,
            KnownProperties.ThinDamFile
        };

        private static ISet<string> ObsoleteProperties { get; } = new HashSet<string>
        {
            "hdam",
            "writebalancefile",
            "transportmethod",
            "transporttimestepping"
        };

        public MduFile(IFlexibleMeshModelApi api  =  null)
        {
            var initialFieldFile = new InitialFieldFile();
            featureFileReader = new FeatureFile1D2DReader(initialFieldFile);
            featureFileWriter = new FeatureFile1D2DWriter(initialFieldFile);
            gullyFile = new Feature2DPointFile<Gully>();
            fileSystem = new FileSystem();

            if (FMDllVersion != null)
                return; // do it once
            
            if (api == null)//not injected in constructor
            {
                api = FlexibleMeshModelApiFactory.CreateNew();
                if (api == null) //not created by factory
                {
                    Log.ErrorFormat("Failed to initialise FlexibleMeshModelApi");
                    return;
                }
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
                    Log.DebugFormat(Resources.MduFile_MduFile_Error_retrieving_FM_Dll_version___0_, exception);

                    FMDllVersion = Resources.MduFile_MduFile_Unknown_DFlowFMDll_version;
                }
            }

            var waterFlowFMAssembly = typeof(WaterFlowFMModel).Assembly;
            FMSuiteFlowModelVersion = waterFlowFMAssembly.GetName().Version.ToString();
        }

        internal string FilePath { get; set; }

        public ExtForceFile ExternalForcingsFile { get; private set; }

        public BndExtForceFile BoundaryExternalForcingsFile { get; private set; }

        #region write logic

        /// <summary>
        /// Write this <see cref="MduFile"/> to the specified target mdu file path given the specified parameters.
        /// </summary>
        /// <param name="targetMduFilePath"> The target mdu file path. </param>
        /// <param name="modelDefinition"> The model definition. </param>
        /// <param name="hydroArea"> The hydro area. </param>
        /// <param name="network">The model network.</param>
        /// <param name="roughnessSections">The roughness sections of the model.</param>
        /// <param name="channelFrictionDefinitions">The channel friction definitions.</param>
        /// <param name="channelInitialConditionDefinitions">The channel initial condition definitions.</param>
        /// <param name="boundaryConditions1D">The 1D boundary conditions.</param>
        /// <param name="lateralSourcesData">The lateral sources data.</param>
        /// <param name="allFixedWeirsAndCorrespondingProperties"> All fixed weirs and corresponding properties. </param>
        /// <param name="switchTo"> if set to <c> true </c> [switch to]. Defaults to <c>true</c>. </param>
        /// <param name="writeExtForcings">Whether to write the external forcings. Defaults to <c>true</c>.</param>
        /// <param name="writeFeatures">Whether to write features. Defaults to <c>true</c>.</param>
        /// <param name="disableFlowNodeRenumbering">Whether to disable flow node renumbering. Defaults to <c>true</c>.</param>
        /// <param name="sedimentModelData"> The sediment model data. Defaults to <c>null</c>.</param>
        /// <param name="copyNetFile">Whether the net file should be copied to the output folder.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="allFixedWeirsAndCorrespondingProperties"/> is <c>null</c>.
        /// </exception>
        public void Write(string targetMduFilePath, 
                          WaterFlowFMModelDefinition modelDefinition, 
                          HydroArea hydroArea, 
                          IHydroNetwork network, 
                          IEnumerable<RoughnessSection> roughnessSections, 
                          IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions, 
                          IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions, 
                          IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D, 
                          IEnumerable<Model1DLateralSourceData> lateralSourcesData, 
                          IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties, 
                          bool switchTo = true, 
                          bool writeExtForcings = true, 
                          bool writeFeatures = true, 
                          bool disableFlowNodeRenumbering = false, 
                          ISedimentModelData sedimentModelData = null, 
                          bool copyNetFile = true)
        {
            Ensure.NotNull(allFixedWeirsAndCorrespondingProperties, nameof(allFixedWeirsAndCorrespondingProperties));
            
            var targetDir = VerifyTargetDirectory(targetMduFilePath);
            
            if (FilePath != null)
            {
                if (copyNetFile)
                {
                    CopyNetFile(targetMduFilePath, modelDefinition);
                }
                CopyUnsupportedFiles(targetMduFilePath, modelDefinition);
            }

            if (switchTo)
            {
                FilePath = targetMduFilePath;
            }

            if (writeFeatures)
            {
                WriteAreaFeatures(targetMduFilePath, modelDefinition, hydroArea, allFixedWeirsAndCorrespondingProperties);
            }

            if (writeExtForcings)
            {
                WriteExternalForcings(targetMduFilePath, modelDefinition, hydroArea, boundaryConditions1D, lateralSourcesData);
            }

            if (modelDefinition.UseMorphologySediment)
            {
                WriteMorphologyFile(targetMduFilePath, modelDefinition);
                WriteSedimentFile(targetMduFilePath, modelDefinition, sedimentModelData);
            }

            modelDefinition.SetMduTimePropertiesFromGuiProperties();

            var fouFileWriter = new FouFileWriter(modelDefinition);
            
            if (fouFileWriter.CanWrite())
            {
                fouFileWriter.WriteToDirectory(targetDir);
            }

            if (targetMduFilePath != null && network != null && hydroArea != null && roughnessSections != null)
            {
                featureFileWriter.Write1D2DFeatures(targetMduFilePath, modelDefinition, network, hydroArea, roughnessSections, channelFrictionDefinitions, channelInitialConditionDefinitions, switchTo);
            }

            // write at the end in case of updated file paths
            WriteProperties(targetMduFilePath, modelDefinition.Properties, writeExtForcings, writeFeatures, useNetCDFMapFormat:false, disableFlowNodeRenumbering:disableFlowNodeRenumbering);
        }

        public void WriteProperties(string filePath, IEnumerable<WaterFlowFMProperty> modelDefinition, bool writeExtForcings, bool writeFeatures, bool writePartionFile = true, bool useNetCDFMapFormat = false, bool disableFlowNodeRenumbering = false)
        {
            var waterFlowFmProperties = modelDefinition.ToList();

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
            var targetDir = Path.GetDirectoryName(targetMduFilePath);
            if (targetDir != string.Empty && !Directory.Exists(targetDir))
            {
                throw new Exception("Non existing directory in file path: " + targetMduFilePath);
            }

            return targetDir;
        }

        private void CopyNetFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty netFileProperty = modelDefinition.GetModelProperty(KnownProperties.NetFile);
            
            string sourcePath = MduFileHelper.GetSubfilePath(FilePath, netFileProperty);
            string targetPath = MduFileHelper.GetSubfilePath(targetMduFilePath, netFileProperty);

            if (sourcePath == null)
            {
                return;
            }

            if (File.Exists(sourcePath) && targetPath != null)
            {
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(targetPath));

                if (!fileSystem.ArePathsEqual(sourcePath, targetPath))
                {
                    File.Copy(sourcePath, targetPath, true);
                }
            }

            // write the bathymetry in the net file.
            if (modelDefinition.SpatialOperations.TryGetValue(WaterFlowFMModelDefinition.BathymetryDataItemName, out IList<ISpatialOperation> bathymetryOperations) && 
                File.Exists(targetPath) && bathymetryOperations.Any(so => !(so is ISpatialOperationSet)))
            {
                WriteBathymetry(modelDefinition, targetPath);
            }

            // if needed, adjust coordinate system in netfile
            if (modelDefinition.CoordinateSystem != null && File.Exists(targetPath) && !modelDefinition.CoordinateSystem.IsNetfileCoordinateSystemUpToDate(targetPath))
            {
                UGridFileHelper.WriteCoordinateSystem(targetPath, modelDefinition.CoordinateSystem);
            }
        }

        /// copy along any mdu-referenced files that are *not* yet supported/written in the UI:
        /// (for example: partition file, manhole file, profloc/profdef files, etc..)
        /// work with the assumption that all and only file entries end with 'file' in their name
        private void CopyUnsupportedFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            IEnumerable<WaterFlowFMProperty> fileBasedProperties = modelDefinition.Properties.Where(p =>
            {
                return MduFileHelper.IsFileValued(p) && 
                       !Array.Exists(supportedFiles, sf => sf.EqualsCaseInsensitive(p.PropertyDefinition.MduPropertyName));
            });

            foreach (WaterFlowFMProperty fileItem in fileBasedProperties)
            {
                string sourcePath = MduFileHelper.GetSubfilePath(FilePath, fileItem);
                string targetPath = MduFileHelper.GetSubfilePath(targetMduFilePath, fileItem);

                if (sourcePath == null || targetPath == null || !File.Exists(sourcePath) || sourcePath.EqualsCaseInsensitive(targetPath))
                {
                    continue;
                }

                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(targetPath));

                if (!sourcePath.EqualsCaseInsensitive(targetPath))
                {
                    File.Copy(sourcePath, targetPath, true);
                }
            }
        }

        private void WriteExternalForcings(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            HydroArea hydroArea, IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D, IEnumerable<Model1DLateralSourceData> lateralSourcesData)
        {
            WaterFlowFMProperty extForceFileProperty = modelDefinition.GetModelProperty(KnownProperties.ExtForceFile);

            string extForceFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, extForceFileProperty);
            if (string.IsNullOrEmpty(extForceFilePath))
            {
                string extForceFileName = modelDefinition.ModelName + FileConstants.ExternalForcingFileExtension;

                extForceFilePath = Path.Combine(Path.GetDirectoryName(targetMduFilePath), extForceFileName);
                extForceFileProperty.SetValueFromString(extForceFileName);
            }

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

            var hasEmbankments = hydroArea.Embankments.Any();
            modelDefinition.Embankments = hydroArea.Embankments;

            IEventedList<GroupableFeature2DPolygon> roofAreas = hydroArea.RoofAreas;
            bool hasRoofAreas = roofAreas.Any();

            // will check if indeed the file is written)
            ExternalForcingsFile.Write(extForceFilePath, modelDefinition, !(newFormatBoundaryConditions || hasNewBoundaries ));

            if (newFormatBoundaryConditions || hasNewBoundaries || hasEmbankments || modelDefinition.FmMeteoFields.Any() || hasModel1dBoundaryConditions || hasLateralSourcesData || hasRoofAreas)
            {
                WaterFlowFMProperty bndExtForceFileProperty = modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile);

                string bndExtForceFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, bndExtForceFileProperty);
                if (string.IsNullOrEmpty(bndExtForceFilePath))
                {
                    string bndExtForceFileName = modelDefinition.ModelName + FileConstants.BoundaryExternalForcingFileExtension;
                    
                    bndExtForceFilePath = Path.Combine(Path.GetDirectoryName(targetMduFilePath), bndExtForceFileName);
                    bndExtForceFileProperty.SetValueFromString(bndExtForceFileName);
                }

                if (BoundaryExternalForcingsFile == null)
                {
                    BoundaryExternalForcingsFile = new BndExtForceFile();
                }

                BoundaryExternalForcingsFile.Write(bndExtForceFilePath, modelDefinition, boundaryConditions1D, lateralSourcesData, roofAreas);
            }
            else if (!modelDefinition.BoundaryConditions.Any())
            {
                modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueFromString(string.Empty);
            }
        }
        
        private static void WriteMorphologyFile(string mduPath, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty morFileProperty = modelDefinition.GetModelProperty(KnownProperties.MorFile);
            string morFilePath = MduFileHelper.GetSubfilePath(mduPath, morFileProperty);

            if (string.IsNullOrEmpty(morFilePath))
            {
                string morFileName = modelDefinition.ModelName + FileConstants.MorphologyFileExtension;

                morFilePath = Path.Combine(Path.GetDirectoryName(mduPath), morFileName);
                morFileProperty.SetValueFromString(morFileName);
            }
            
            MorphologyFile.Save(morFilePath, modelDefinition);
        }
        
        private static void WriteSedimentFile(string mduPath, WaterFlowFMModelDefinition modelDefinition, ISedimentModelData sedimentModelData)
        {
            if (sedimentModelData == null)
            {
                return;
            }
            
            WaterFlowFMProperty sedFileProperty = modelDefinition.GetModelProperty(KnownProperties.SedFile);
            string sedFilePath = MduFileHelper.GetSubfilePath(mduPath, sedFileProperty);
            
            if (string.IsNullOrEmpty(sedFilePath))
            {
                string sedFileName = modelDefinition.ModelName + FileConstants.SedimentFileExtension;

                sedFilePath = Path.Combine(Path.GetDirectoryName(mduPath), sedFileName);
                sedFileProperty.SetValueFromString(sedFileName);
            }
            
            SedimentFile.Save(sedFilePath, modelDefinition, sedimentModelData);
        }

        private void WriteMduLine(bool writeExtForcings, bool writeFeatures, bool writePartionFile, bool useNetCDFMapFormat,
                                  bool disableFlowNodeRenumbering, IGrouping<string, WaterFlowFMProperty> propertyGroup)
        {
            WriteLine("");
            WriteLine("[" + propertyGroup.Key + "]");
            foreach (WaterFlowFMProperty prop in propertyGroup)
            {
                if (!writePartionFile && prop.PropertyDefinition.MduPropertyName.Equals("PartitionFile"))
                {
                    continue;
                }

                if (useNetCDFMapFormat && prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MapFormat))
                {
                    string line = string.Format("{0,-18}= {1,-20}{2}", prop.PropertyDefinition.MduPropertyName, 1,
                                                "# For 1d2d coupling we should always write mapformat output in NetCDF format");
                    WriteLine(line.Trim());
                }
                else if (disableFlowNodeRenumbering && prop.PropertyDefinition.MduPropertyName.Equals("RenumberFlowNodes"))
                {
                    string line = string.Format("{0,-18}= {1,-20}{2}", prop.PropertyDefinition.MduPropertyName, 0,
                                                "# For 1d2d coupling we should never renumber the flownodes");
                    WriteLine(line.Trim());
                }
                else
                {
                    string mduPropertyValue = GetPropertyValue(prop, writeExtForcings, writeFeatures);
                    WriteMduLine(prop, mduPropertyValue);
                }
            }
        }

        private IEnumerable<IGrouping<string, WaterFlowFMProperty>> GetPropertiesByGroup(
            IList<WaterFlowFMProperty> properties, bool writeExtForcings, bool writeFeatures)
        {
            WriteLine("# Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLine("# Deltares,Delft3D FM 2018 Suite Version " + FMSuiteFlowModelVersion + 
                      ", D-Flow FM Version " + FMDllVersion);
            SetValueToPropertyIfExists(properties, KnownProperties.Version, FMDllVersion);
            SetValueToPropertyIfExists(properties, KnownProperties.GuiVersion, FMSuiteFlowModelVersion);
            
            IEnumerable<IGrouping<string, WaterFlowFMProperty>> propertiesByGroup = 
                properties.Where(IsMduFileProperty)
                          .OrderBy(GetPropertySortIndex)
                          .GroupBy(p => p.PropertyDefinition.FileSectionName);

            return RemoveMorAndSedPropertiesIfNeeded(propertiesByGroup, properties, writeExtForcings, writeFeatures);
        }
        
        private static bool IsMduFileProperty(WaterFlowFMProperty property)
        {
            return property.PropertyDefinition.FileSectionName != "GUIOnly"
                   // remove unknown properties that should be located on the sed/mor files
                   && property.PropertyDefinition.FileSectionName != MorphologyFile.MorphologyUnknownProperty
                   && property.PropertyDefinition.FileSectionName != SedimentFile.SedimentUnknownProperty;
        }
        
        private static int GetPropertySortIndex(WaterFlowFMProperty property)
        {
            int sortIndex = property.PropertyDefinition.SortIndex;
            return sortIndex != -1 
                       ? sortIndex 
                       : int.MaxValue;
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

        private static void SetValueToPropertyIfExists(IEnumerable<WaterFlowFMProperty> modelDefinition, string name, string value)
        {
            if (modelDefinition == null) return;
            var waterFlowFmProperty = modelDefinition.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (waterFlowFmProperty != null)
            {
                waterFlowFmProperty.SetValueFromString(value);
            }
        }

        private static string GetPropertyValue(WaterFlowFMProperty prop, bool writeExtForcings, bool writeFeatures)
        {
            var propertyName = prop.PropertyDefinition.MduPropertyName.ToLower();
            if (!writeExtForcings &&
                (propertyName == KnownProperties.ExtForceFile || 
                 propertyName == KnownProperties.BndExtForceFile))
            {
                return string.Empty;
            }
            if (!writeFeatures &&
                (propertyName == KnownProperties.DryPointsFile || 
                 propertyName == KnownProperties.LandBoundaryFile ||
                 propertyName == KnownProperties.ThinDamFile || 
                 propertyName == KnownProperties.FixedWeirFile || 
                 propertyName == KnownProperties.BridgePillarFile ||
                 propertyName == KnownProperties.ManholeFile || 
                 propertyName == KnownProperties.ObsFile || 
                 propertyName == KnownProperties.ObsCrsFile))
            {
                return string.Empty;
            }
            return prop.GetValueAsString();
        }

        private static void WriteFeatures<TFeat, TFile>(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
                                                        string propertyKey, IList<TFeat> features, ref TFile fileWriter, string extension, params string[] alternativeExtensions)
            where TFile : IFeature2DFileBase<TFeat>, new()
        {
            WaterFlowFMProperty waterFlowFMProperty = modelDefinition.GetModelProperty(propertyKey);
            if (waterFlowFMProperty == null)
            {
                return;
            }

            if (features.Any())
            {
                InitializeFeatureWriter(targetMduFilePath, features, extension, waterFlowFMProperty, out List<IGrouping<string, IGroupableFeature>> grouping, alternativeExtensions);

                List<string> featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(targetMduFilePath, waterFlowFMProperty).ToList();
                featuresFilePaths.RemoveAllWhere(ffp => ffp == null);

                if (fileWriter == null)
                {
                    fileWriter = CreateFeatureFile<TFeat, TFile>(modelDefinition);
                }

                foreach (string filePath in featuresFilePaths)
                {
                    string fileName = FileUtils.GetRelativePath(Path.GetDirectoryName(targetMduFilePath), filePath, true);
                    string fileNameWithoutExtension = string.IsNullOrEmpty(extension) ? fileName : fileName.Replace(extension, string.Empty);

                    IGrouping<string, IGroupableFeature> groupFeatures = grouping.Find(g =>
                    {
                        string key = g.Key.ToLowerInvariant();
                        
                        return key.Equals(fileName.ToLowerInvariant()) || 
                               key.Equals(fileNameWithoutExtension.ToLowerInvariant()) || 
                               !string.IsNullOrEmpty(extension) && 
                               key.Replace(Path.GetExtension(extension), string.Empty).Equals(fileNameWithoutExtension.ToLowerInvariant());
                    });
                    
                    IList<TFeat> featuresToWrite = grouping.Count > 0 && groupFeatures != null
                                                       ? groupFeatures.Cast<TFeat>().ToList()
                                                       : features;
                    
                    fileWriter.Write(filePath, featuresToWrite);
                }
            }
            else
            {
                waterFlowFMProperty.SetValueFromString(string.Empty);
            }
        }

        private static void InitializeFeatureWriter<TFeat>(string targetMduFilePath, IList<TFeat> features, string extension,
            WaterFlowFMProperty waterFlowFMProperty, out List<IGrouping<string, IGroupableFeature>> grouping, params string[] alternativeExtensions)
        {
            string defaultGroup = Path.GetFileNameWithoutExtension(targetMduFilePath);
            MduFileHelper.UpdateFeatures(features, extension, defaultGroup);
            
            string[] writeToRelativeFilePaths = MduFileHelper.GetUniqueFilePathsForWindows(targetMduFilePath, features, extension, alternativeExtensions);

            grouping = features.OfType<IGroupableFeature>().GroupBy(f => f.GroupName, StringComparer.InvariantCultureIgnoreCase).ToList();

            waterFlowFMProperty.SetValueFromStrings(writeToRelativeFilePaths.Select(f => Path.HasExtension(f) ? f : string.Concat(f, extension)));
        }

        private static void AddDryAreasToWriter(string targetMduFilePath, IList<GroupableFeature2DPolygon> features, string extension,
                                                WaterFlowFMProperty waterFlowFMProperty, out List<IGrouping<string, IGroupableFeature>> grouping)
        {
            string defaultGroup = Path.GetFileNameWithoutExtension(targetMduFilePath);
            MduFileHelper.UpdateFeatures(features, extension, defaultGroup);

            List<string> writeToRelativeFilePaths = MduFileHelper.GetUniqueFilePathsForWindows(targetMduFilePath, features, extension).ToList();

            grouping = features.OfType<IGroupableFeature>().GroupBy(f => f.GroupName, StringComparer.InvariantCultureIgnoreCase).ToList();

            if (writeToRelativeFilePaths.Any())
            {
                string currentValue = waterFlowFMProperty.GetValueAsString();
                string newValue = currentValue + " " + string.Join(" ", writeToRelativeFilePaths.Select(f => Path.HasExtension(f) ? f : string.Concat(f, extension)));

                waterFlowFMProperty.SetValueFromString(newValue);
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
                        var writeDirectory = Path.GetDirectoryName(featuresFilePath);
                        if (!string.IsNullOrEmpty(writeDirectory) && !Directory.Exists(writeDirectory))
                        {
                            Directory.CreateDirectory(writeDirectory);
                        }

                        var fileName = FileUtils.GetRelativePath(Path.GetDirectoryName(targetMduFilePath), featuresFilePath, true);
                        var fileNameWithoutExtension = fileName.Replace(DryPointExtension, string.Empty);
                        var groupFeatures = GetAllGroupedFeaturesFromFile<GroupablePointFeature>(dryPointsPerGroup, fileName, fileNameWithoutExtension);
                        var featuresToWrite = dryPoints.Count > 0 && groupFeatures != null ? groupFeatures : dryPoints;
                        XyzFile.Write(featuresFilePath, featuresToWrite.Select(p => new PointValue { X = p.X, Y = p.Y, Value = 0 }));
                    }
                    else if (featuresFilePath.EndsWith(DryAreaExtension))
                    {
                        var fileName = FileUtils.GetRelativePath(Path.GetDirectoryName(targetMduFilePath), featuresFilePath, true);
                        var fileNameWithoutExtension = fileName.Replace(DryAreaExtension, string.Empty);
                        var groupFeatures = GetAllGroupedFeaturesFromFile<GroupableFeature2DPolygon>(dryAreasPerGroup, fileName, fileNameWithoutExtension);
                        var featuresToWrite = dryAreas.Count > 0 && groupFeatures != null ? groupFeatures : dryAreas;
                        dryAreaFile.Write(featuresFilePath, featuresToWrite);
                    }
                }
            }
            else
            {
                waterFlowFMProperty.SetValueFromString(string.Empty);
            }
        }

        private static TFile CreateFeatureFile<TFeat, TFile>(WaterFlowFMModelDefinition modelDefinition)
            where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var fileWriter = new TFile();
            
            if (fileWriter is PlizFile<FixedWeir> fixedWeirFile)
            {
                fixedWeirFile.CreateDelegate = (points, name) => new FixedWeir
                {
                    Name = name,
                    Geometry = PlizFile<FixedWeir>.CreatePolyLineGeometry(points)
                };
            }

            if (fileWriter is PlizFile<BridgePillar> bridgePillarFile)
            {
                bridgePillarFile.CreateDelegate = (points, name) => CreateDelegateBridgePillar(name, points);
            }

            if (fileWriter is StructuresFile structuresFileWriter)
            {
                structuresFileWriter.StructureSchema = modelDefinition.StructureSchema;
                structuresFileWriter.ReferenceDate =
                    modelDefinition.GetReferenceDateAsDateTime();
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
            IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties)
        {
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.LandBoundaryFile,
                hydroArea.LandBoundaries, ref landBoundariesFile, LandBoundariesExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams.ToList(),
                ref thinDamFile, ThinDamExtension, ThinDamAlternativeExtension);

            UpdateFixedWeirs(hydroArea, allFixedWeirsAndCorrespondingProperties.ToDictionary(p => p.Feature));
            
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

            WriteGullies(targetMduFilePath, hydroArea.Gullies);
        }

        private static void UpdateFixedWeirs(HydroArea hydroArea, 
                                             IReadOnlyDictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>> fixedWeirPropertiesMapping)
        {
            //fix attributes for fixed weirs. Create attributes from modelfeaturecoordinatdata.
            foreach (FixedWeir fixedWeir in hydroArea.FixedWeirs)
            {
                fixedWeir.Attributes = new DictionaryFeatureAttributeCollection();

                ModelFeatureCoordinateData<FixedWeir> correspondingModelFeatureCoordinateData = fixedWeirPropertiesMapping[fixedWeir];

                if (correspondingModelFeatureCoordinateData == null)
                {
                    break;
                }

                for (var index = 0; index < correspondingModelFeatureCoordinateData.DataColumns.Count; index++)
                {
                    if (!correspondingModelFeatureCoordinateData.DataColumns[index].IsActive)
                    {
                        break;
                    }

                    IList dataColumnWithData = correspondingModelFeatureCoordinateData.DataColumns[index].ValueList;

                    var syncedList = new GeometryPointsSyncedList<double>
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
        }

        private void WriteGullies(string mduFilePath, IEventedList<Gully> gullies)
        {
            if (!gullies.Any())
            {
                return;
            }

            string filePath = Path.Combine(Path.GetDirectoryName(mduFilePath), GullyFileName);
            gullyFile.Write(filePath, gullies);
        }

        private static List<TFeat> GetAllGroupedFeaturesFromFile<TFeat>(IEnumerable<IGrouping<string, IGroupableFeature>> grouping, string fileName, string fileNameWithoutExtension)
            where TFeat : IGroupableFeature
        {
            var groupFeatures =
                grouping.FirstOrDefault(g => g.Key.ToLowerInvariant().Equals(fileName.ToLowerInvariant())
                                             || g.Key.ToLowerInvariant().Equals(fileNameWithoutExtension.ToLowerInvariant())
                                             || g.Key.ToLowerInvariant().Replace(Path.GetExtension(DryAreaExtension), string.Empty).Equals(fileNameWithoutExtension.ToLowerInvariant()));
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

        public void Read(string mduFilePath, IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel, Action<string> reportProgress = null)
        {
            reportProgress?.Invoke(Resources.MduFile_Read_Reading_properties);
            ReadProperties(mduFilePath, convertedFileObjectsForFMModel.ModelDefinition);
            
            reportProgress?.Invoke(Resources.MduFile_Read_Validating_morphology_properties);
            ValidateProperties(mduFilePath, convertedFileObjectsForFMModel.ModelDefinition);

            reportProgress?.Invoke(Resources.MduFile_Read_Reading_morphology_properties);
            MorphologyFile.Read(mduFilePath, convertedFileObjectsForFMModel.ModelDefinition);

            reportProgress?.Invoke(Resources.MduFile_Read_Reading_area_features);
            ReadAreaFeatures(mduFilePath, convertedFileObjectsForFMModel.ModelDefinition, convertedFileObjectsForFMModel.HydroArea);

            FixFixedWeirs(convertedFileObjectsForFMModel);

            FixBridgePillars(convertedFileObjectsForFMModel);

            reportProgress?.Invoke(Resources.MduFile_Read_Reading_grid);
            ReadNetFile(mduFilePath, convertedFileObjectsForFMModel, reportProgress);

            reportProgress?.Invoke(Resources.MduFile_Read_Reading_external_forcings_file);
            ReadExternalForcings(mduFilePath, convertedFileObjectsForFMModel);

            reportProgress?.Invoke(Resources.MduFile_Read_Reading_boundary_external_forcings_file);
            ReadBoundaryExternalForcings(mduFilePath, convertedFileObjectsForFMModel);

            reportProgress?.Invoke(Resources.MduFile_Read_Reading_FouFile_if_used);
            ReadFouFileIfUsed(mduFilePath, convertedFileObjectsForFMModel);

            convertedFileObjectsForFMModel.HydroArea.Embankments.AddRange(convertedFileObjectsForFMModel.ModelDefinition.Embankments);
            
            reportProgress?.Invoke(Resources.MduFile_Read_Reading_1d2d_features);
            featureFileReader.Read1D2DFeatures(mduFilePath, convertedFileObjectsForFMModel.ModelDefinition, convertedFileObjectsForFMModel.HydroNetwork, convertedFileObjectsForFMModel.RoughnessSections, convertedFileObjectsForFMModel.ChannelFrictionDefinitions, convertedFileObjectsForFMModel.ChannelInitialConditionDefinitions, text => reportProgress?.Invoke(text + Environment.NewLine + Resources.WaterFlowFMModel_OnLoad_Reading_1D2D_features));
            reportProgress?.Invoke(Resources.MduFile_Read_done_reading_1d2d_features);
        }

        private static void FixFixedWeirs(IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel)
        {
            var logHandler = new LogHandler("updating Fixed Weirs", 100);

            foreach (var fixedWeir in convertedFileObjectsForFMModel.HydroArea.FixedWeirs)
            {
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir>() { Feature = fixedWeir };
                var scheme = convertedFileObjectsForFMModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();
                modelFeatureCoordinateData.UpdateDataColumns(scheme);

                var locationKeyFound = fixedWeir.Attributes.ContainsKey(Feature2D.LocationKey);
                var indexKey = !locationKeyFound ? -1 : fixedWeir.Attributes.Keys.ToList().IndexOf(Feature2D.LocationKey);

                var numberFixedWeirAttributes = !locationKeyFound ? fixedWeir.Attributes.Count : (fixedWeir.Attributes.Count - 1);

                var difference = Math.Abs(modelFeatureCoordinateData.DataColumns.Count - numberFixedWeirAttributes);

                if (modelFeatureCoordinateData.DataColumns.Count < fixedWeir.Attributes.Count)
                {
                    logHandler.ReportWarningFormat(Resources.MduFile_Read_Based_on_the_Fixed_Weir_Scheme__0___there_are_too_many_column_s__defined_for__1__in_the_imported_fixed_weir_file__The_last__2__column_s__have_been_ignored,
                                                   scheme, fixedWeir, difference);
                }

                if (modelFeatureCoordinateData.DataColumns.Count > fixedWeir.Attributes.Count)
                {
                    logHandler.ReportWarningFormat(Resources.MduFile_Read_Based_on_the_Fixed_Weir_Scheme__0___there_are_not_enough_column_s__defined_for__1__in_the_imported_fixed_weir_file__The_last__2__column_s__have_been_generated_using_default_values,
                                                   scheme, fixedWeir, difference);
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
                convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties.Add(modelFeatureCoordinateData);
            }

            foreach (var fixedWeir in convertedFileObjectsForFMModel.HydroArea.FixedWeirs)
            {
                fixedWeir.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
            }

            logHandler.LogReport();
        }

        private static void FixBridgePillars(IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel)
        {
            if (convertedFileObjectsForFMModel.AllBridgePillarsAndCorrespondingProperties != null)
            {
                foreach (var bridgePillar in convertedFileObjectsForFMModel.HydroArea.BridgePillars)
                {
                    var modelFeatureCoordinateData =
                        new ModelFeatureCoordinateData<BridgePillar>() { Feature = bridgePillar };

                    var bpFile = convertedFileObjectsForFMModel.ModelDefinition.GetModelProperty(KnownProperties.BridgePillarFile).GetValueAsString();
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

                    SetBridgePillarDataModel(convertedFileObjectsForFMModel.AllBridgePillarsAndCorrespondingProperties, modelFeatureCoordinateData, bridgePillar);
                }

                foreach (var bridgePillar in convertedFileObjectsForFMModel.HydroArea.BridgePillars)
                {
                    bridgePillar.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
                }
            }
        }

        private static void ReadNetFile(string filePath, IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel, Action<string> reportProgress)
        {
            string netFilePath = MduFileHelper.GetSubfilePath(filePath, convertedFileObjectsForFMModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!string.IsNullOrEmpty(netFilePath) && File.Exists(netFilePath))
            {
                IEnumerable<CompartmentProperties> compartmentData = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netFilePath);
                IEnumerable<BranchProperties> branchData = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netFilePath);
                convertedFileObjectsForFMModel.CompartmentProperties = compartmentData;
                convertedFileObjectsForFMModel.BranchProperties = branchData;
                UGridFileHelper.ReadNetFileDataIntoModel(netFilePath, convertedFileObjectsForFMModel, reportProgress: (progressText) =>  reportProgress?.Invoke(Resources.MduFile_Read_Reading_netFile + Environment.NewLine + progressText));
            }
        }

        private void ReadExternalForcings(string filePath, IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel)
        {
            var extForceFileProperty = convertedFileObjectsForFMModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile);
            if (extForceFileProperty != null)
            {
                var forceFilePath = MduFileHelper.GetSubfilePath(filePath,
                                                                 convertedFileObjectsForFMModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile));

                if (forceFilePath != null && File.Exists(forceFilePath))
                {
                    ExternalForcingsFile = new ExtForceFile();
                    ExternalForcingsFile.Read(forceFilePath, convertedFileObjectsForFMModel.ModelDefinition);
                }
            }
        }

        private void ReadBoundaryExternalForcings(string filePath, IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel)
        {
            var bndExtForceFileProperty = convertedFileObjectsForFMModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile);
            if (bndExtForceFileProperty != null)
            {
                var forceFilePath = MduFileHelper.GetSubfilePath(filePath, bndExtForceFileProperty);

                if (forceFilePath != null && File.Exists(forceFilePath))
                {
                    BoundaryExternalForcingsFile = new BndExtForceFile();
                    BoundaryExternalForcingsFile.Read(forceFilePath, convertedFileObjectsForFMModel.ModelDefinition, convertedFileObjectsForFMModel.HydroNetwork, convertedFileObjectsForFMModel.HydroArea, convertedFileObjectsForFMModel.BoundaryConditions1D, convertedFileObjectsForFMModel.LateralSourcesData);
                }
            }
        }

        private static void ReadFouFileIfUsed(string filePath, IConvertedFileObjectsForFMModel convertedFileObjectsForFMModel)
        {
            string targetDir = Path.GetDirectoryName(filePath);

            var fouFileReader = new FouFileReader(convertedFileObjectsForFMModel.ModelDefinition);

            if (fouFileReader.CanReadFromDirectory(targetDir))
            {
                fouFileReader.ReadFromDirectory(targetDir);
            }
        }

        private static void LoadAttributeIntoDataColumn(GeometryPointsSyncedList<double> loadedData, IDataColumn dataColumn)
        {
            // Just a refactor of the setter.
            if (dataColumn == null || loadedData == null) return;
            dataColumn.ValueList = loadedData.ToList();
        }

        private void ReadProperties(string filePath, WaterFlowFMModelDefinition definition)
        {
            FilePath = filePath;
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
                        if (ObsoleteProperties.Contains(mduPropertyLowerCase))
                        {
                            Log.Warn(string.Format(Resources.Key_0_in_1_is_deprecated_and_automatically_removed_from_model, mduPropertyName, Path.GetFileName(filePath)));
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

                            var waterFlowFmProperty = new WaterFlowFMProperty(propDef, mduPropertyValue);
                            definition.AddProperty(waterFlowFmProperty);
                            
                            Log.InfoFormat(Resources.MduFile_ReadProperties_An_unrecognized_keyword___0___has_been_detected, propDef.Caption);
                        }
                        
                        WaterFlowFMProperty property = definition.GetModelProperty(mduPropertyLowerCase);
                        property.PropertyDefinition.SortIndex = LineNumber;
                        
                        if (!string.IsNullOrEmpty(mduPropertyValue))
                        {
                            if (mduPropertyValue.EndsWith(@"\"))
                            {
                                mduPropertyValue = GetMduPropertyMultipleLineValueRecursive(mduPropertyName, mduPropertyValue, ref line, ref readNextLine);
                            }

                            property.SetValueFromString(mduPropertyValue);
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
                DataTypeValueParser.FromString<DateOnly>(definition.GetModelProperty(KnownProperties.RefDate).GetValueAsString());

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

        private static void ValidateProperties(string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var validator = new MduFileValidator(mduFilePath, modelDefinition);
            validator.Validate();
        }

        private static void ReadFeatures<TFeat, TFile>(string mduFilePath, WaterFlowFMModelDefinition modelDefinition,
            string propertyKey, IList<TFeat> features, ref TFile fileReader, string extension) where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var modelProperty = modelDefinition.GetModelProperty(propertyKey);

            if (propertyKey == KnownProperties.StructuresFile)
            {
                RemoveAllStructuresFilesWithBadReferences(mduFilePath, modelDefinition, modelProperty);
            }
            
            var featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelProperty);
            if (featuresFilePaths == null || featuresFilePaths.Count == 0) return;

            fileReader = CreateFeatureFile<TFeat, TFile>(modelDefinition);

            var readFeatures = new List<TFeat>();
            foreach (var featuresFilePath in featuresFilePaths)
            {
                IList<TFeat> featuresToAdd;
                if (fileReader is StructuresFile structuresFile)
                {
                    featuresToAdd = (IList<TFeat>)structuresFile.CopyFileAndRead(featuresFilePath, featuresFilePath);
                }
                else
                {
                    featuresToAdd = fileReader.Read(featuresFilePath);
                }

                if (modelProperty.PropertyDefinition.IsMultipleFile)
                {
                    //make sure the features have the right group name.
                    var asGroupable = featuresToAdd.OfType<IGroupableFeature>().ToList();
                    var featurePathName = FileUtils.GetRelativePath(Path.GetDirectoryName(mduFilePath), featuresFilePath, true);
                    var mduPathName = Path.GetFileNameWithoutExtension(mduFilePath);
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
            
            var structures = new List<IStructure>();

            ReadFeatures(filePath, modelDefinition, KnownProperties.StructuresFile, structures, ref structuresFile, StructuresExtension);

            foreach (var structure in structures)
            {
                switch (structure)
                {
                    case Pump2D pump2D:
                        hydroArea.Pumps.Add(pump2D);
                        break;
                    case Weir2D weir2D:
                        hydroArea.Weirs.Add(weir2D);
                        break;
                    case Gate2D gate2D:
                        hydroArea.Gates.Add(gate2D);
                        break;
                    case LeveeBreach breach:
                        hydroArea.LeveeBreaches.Add(breach);
                        break;
                    default:
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
            
            ReadGullies(filePath, hydroArea.Gullies);
        }

        private void ReadGullies(string mduFilePath, IEventedList<Gully> gullies)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(mduFilePath), GullyFileName);
            if (!File.Exists(filePath))
            {
                return;
            }
            
            IList<Gully> pointFeatures = gullyFile.Read(filePath);
            gullies.AddRange(pointFeatures);
        }

        private void ReadDryPointsAndDryAreas(string mduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea)
        {
            var dryPointsPropertyKey = KnownProperties.DryPointsFile;
            var mduPathName = Path.GetFileNameWithoutExtension(mduFilePath);

            var modelProperty = modelDefinition.GetModelProperty(dryPointsPropertyKey);
            var featureFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelProperty);
            if (!featureFilePaths.Any()) return;

            if (dryAreaFile == null) dryAreaFile = new PolFile<GroupableFeature2DPolygon>();
            foreach (var featureFilePath in featureFilePaths)
            {
                var groupName = FileUtils.GetRelativePath(Path.GetDirectoryName(mduFilePath), featureFilePath, true);
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
        /// Removes all structures files that contain references to feature files that do not exist.
        /// </summary>
        /// <param name="mduFilePath">The path of the MDU file to write to.</param>
        /// <param name="structureFileProperty"> The structures file property. </param>
        /// <param name="modelDefinition">The model definition of the FM Model.</param>
        private static void RemoveAllStructuresFilesWithBadReferences(string mduFilePath,
                                                                      WaterFlowFMModelDefinition modelDefinition,
                                                                      WaterFlowFMProperty structureFileProperty)
        {
            var structureFilesWithBadReferences = new List<string>();
            var featureFilePaths = structureFileProperty.GetFileLocationValues().ToList();
            
            foreach (var filePath in featureFilePaths.Where(fp => fp.EndsWith(StructuresExtension)))
            {
                var structureFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, filePath);
                var fileReader = new StructuresFile
                {
                    StructureSchema = modelDefinition.StructureSchema,
                    ReferenceDate = modelDefinition.GetReferenceDateAsDateTime()
                };
                var featuresToAdd = fileReader.ReadStructures2D(structureFilePath).ToList();
                var referencesToNonExistentFilesExist = false;
                featuresToAdd.ForEach(f =>
                {
                    var featureFileName = f.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString();
                    var featureFilePath = Path.Combine(Path.GetDirectoryName(structureFilePath), featureFileName);
                    if (!File.Exists(featureFilePath))
                    {
                        referencesToNonExistentFilesExist = true;
                        Log.ErrorFormat(Resources.MduFile_RemoveAllStructuresFilesWithBadReferences_, featureFilePath, structureFilePath);
                    }
                });
                if (referencesToNonExistentFilesExist) structureFilesWithBadReferences.Add(structureFilePath);
            }
            featureFilePaths.RemoveAllWhere(gn => structureFilesWithBadReferences.Contains(gn));
            structureFileProperty.SetValueFromStrings(featureFilePaths);
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
