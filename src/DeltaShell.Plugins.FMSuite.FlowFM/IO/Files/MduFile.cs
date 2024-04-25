using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DHYDRO.Common.Extensions;
using EnumerableExtensions = DHYDRO.Common.Extensions.EnumerableExtensions;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using SharpMap;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class MduFile : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MduFile));

        private static string FMSuiteFlowModelVersion;
        private static string FMDllVersion;

        // the following mdu-referenced files are written by the UI, or at least should not be copied along blindly 
        // (please keep this list up-to-date!):

        private static readonly string[] SupportedFiles =
        {
            KnownProperties.NetFile,
            KnownProperties.ExtForceFile,
            KnownProperties.MapFile,
            KnownProperties.HisFile,
            KnownProperties.ThinDamFile,
            KnownProperties.FixedWeirFile,
            KnownProperties.BridgePillarFile,
            KnownProperties.ObsFile,
            KnownProperties.ObsCrsFile,
            KnownProperties.LandBoundaryFile,
            KnownProperties.DryPointsFile,
            KnownProperties.RestartFile,
            KnownProperties.StructuresFile
        };

        private int propertyKeyAlignmentLength;
        private int propertyValueAlignmentLength;

        private LdbFile landBoundariesFile;
        private PliFile<ThinDam2D> thinDamFile;
        private PlizFile<FixedWeir> fixedWeirFile;
        private PlizFile<BridgePillar> bridgePillarFile;
        private StructuresFile structuresFile;
        private ObsFile<GroupableFeature2DPoint> obsFile;
        private PliFile<ObservationCrossSection2D> obsCrsFile;
        private PolFile<GroupableFeature2DPolygon> dryAreaFile;
        private PolFile<GroupableFeature2DPolygon> enclosureFile;

        private readonly IFileSystem fileSystem;
        private readonly InitialFieldFileReader initialFieldFileReader;
        private readonly InitialFieldFileWriter initialFieldFileWriter;

        public MduFile(IFlexibleMeshModelApi api = null)
        {
            fileSystem = new FileSystem();
            initialFieldFileReader = new InitialFieldFileReader(fileSystem);
            initialFieldFileWriter = new InitialFieldFileWriter(fileSystem, new SpatialDataFileWriter());

            if (FMDllVersion != null && FMDllVersion != Resources.MduFile_MduFile_Unknown)
            {
                return; // do it once
            }

            if (api == null) //not injected in constructor
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

                    FMDllVersion = Resources.MduFile_MduFile_Unknown;
                }
            }

            var waterFlowFMAssembly = typeof(WaterFlowFMModel).Assembly;
            FMSuiteFlowModelVersion = waterFlowFMAssembly.GetName().Version.ToString();
        }

        public ExtForceFile ExternalForcingsFile { get; private set; }

        public BndExtForceFile BoundaryExternalForcingsFile { get; private set; }

        internal string FilePath { get; set; }

        #region write logic

        /// <summary>
        /// Write this <see cref="MduFile"/> to the specified target mdu file path given the specified parameters.
        /// </summary>
        /// <param name="targetMduFilePath"> The target mdu file path. </param>
        /// <param name="modelDefinition"> The model definition. </param>
        /// <param name="hydroArea"> The hydro area. </param>
        /// <param name="allFixedWeirsAndCorrespondingProperties"> All fixed weirs and corresponding properties. </param>
        /// <param name="config"> The configuration. </param>
        /// <param name="switchTo"> if set to <c> true </c> [switch to]. </param>
        /// <param name="sedimentModelData"> The sediment model data. </param>
        /// <remarks>
        /// If <paramref name="config"/> is null, the default MduFileWriteConfig will be used.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="allFixedWeirsAndCorrespondingProperties"/> is <c>null</c>.
        /// </exception>
        public void Write(string targetMduFilePath,
                          WaterFlowFMModelDefinition modelDefinition,
                          HydroArea hydroArea,
                          IEnumerable<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties,
                          IMduFileWriteConfig config = null,
                          bool switchTo = true,
                          ISedimentModelData sedimentModelData = null)
        {
            Ensure.NotNull(allFixedWeirsAndCorrespondingProperties, nameof(allFixedWeirsAndCorrespondingProperties));

            if (config == null)
            {
                config = new MduFileWriteConfig();
            }

            VerifyTargetDirectory(targetMduFilePath);

            if (FilePath != null)
            {
                CopyNetFile(targetMduFilePath, modelDefinition);
                CopyUnsupportedFiles(targetMduFilePath, modelDefinition);
            }
            
            if (switchTo)
            {
                FilePath = targetMduFilePath;
            }

            if (config.WriteFeatures)
            {
                WriteAreaFeatures(targetMduFilePath, modelDefinition, hydroArea, allFixedWeirsAndCorrespondingProperties);
            }

            if (config.WriteExtForcings)
            {
                WriteExternalForcings(targetMduFilePath, modelDefinition, switchTo);
            }

            if (modelDefinition.UseMorphologySediment && config.WriteMorphologySediment)
            {
                WriteMorphologyFile(targetMduFilePath, modelDefinition);
                WriteSedimentFile(targetMduFilePath, modelDefinition, sedimentModelData);
            }

            modelDefinition.SetMduTimePropertiesFromGuiProperties();

            WriteInitialFieldFile(targetMduFilePath, modelDefinition, switchTo);

            // write at the end in case of updated file paths
            WriteProperties(targetMduFilePath, modelDefinition.Properties, config);
        }
        
        private void WriteInitialFieldFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, bool switchTo)
        {
            string initialFieldFilePath = GetInitialFieldFilePath(targetMduFilePath, modelDefinition);
            if (initialFieldFilePath == null)
            {
                return;
            }

            var pathsRelativeToParent =
                (bool)modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).Value;
            
            string initialFieldFileReferenceFilePath = pathsRelativeToParent ? initialFieldFilePath : targetMduFilePath;
            
            initialFieldFileWriter.Write(initialFieldFilePath, initialFieldFileReferenceFilePath, switchTo, modelDefinition);
        }

        private string GetInitialFieldFilePath(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty fileProperty = modelDefinition.GetModelProperty(KnownProperties.IniFieldFile);

            if (!initialFieldFileWriter.ShouldWrite(modelDefinition))
            {
                fileProperty.Value = string.Empty;
                return null;
            }

            if (!string.IsNullOrWhiteSpace(fileProperty.GetValueAsString()))
            {
                return MduFileHelper.GetSubfilePath(targetMduFilePath, fileProperty);
            }

            fileProperty.Value = InitialFieldFileConstants.DefaultFileName;
            string mduFolder = fileSystem.Path.GetDirectoryName(targetMduFilePath);
            return fileSystem.Path.Combine(mduFolder, InitialFieldFileConstants.DefaultFileName);
        }

        /// <summary>
        /// Write the properties to the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> The file path. </param>
        /// <param name="modelDefinition"> The model definition. </param>
        /// <param name="config"> The configuration. </param>
        /// <param name="writePartionFile"> if set to <c> true </c> [write partion file]. </param>
        /// <param name="useNetCDFMapFormat"> if set to <c> true </c> [use net CDF map format]. </param>
        /// <remarks>
        /// If <paramref name="config"/> is null, the default MduFileWriteConfig will be used.
        /// </remarks>
        public void WriteProperties(string filePath,
                                    IEnumerable<WaterFlowFMProperty> modelDefinition,
                                    IMduFileWriteConfig config = null,
                                    bool writePartionFile = true,
                                    bool useNetCDFMapFormat = false)
        {
            if (config == null)
            {
                config = new MduFileWriteConfig();
            }

            List<WaterFlowFMProperty> waterFlowFmProperties = modelDefinition.ToList();
            
            OpenOutputFile(filePath);
            try
            {
                propertyKeyAlignmentLength = GetLengthOfLongestPropertyName(waterFlowFmProperties);
                propertyValueAlignmentLength = GetLengthOfLongestPropertyValue(waterFlowFmProperties);

                IEnumerable<IGrouping<string, WaterFlowFMProperty>> propertiesByGroup = GetPropertiesByGroup(
                    waterFlowFmProperties,
                    config);

                foreach (IGrouping<string, WaterFlowFMProperty> propertyGroup in propertiesByGroup)
                {
                    WriteMduGroup(config,
                                  writePartionFile,
                                  useNetCDFMapFormat,
                                  propertyGroup);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private static int GetLengthOfLongestPropertyName(IEnumerable<WaterFlowFMProperty> waterFlowFmProperties)
        {
            IEnumerable<string> names = waterFlowFmProperties.Select(p => p.PropertyDefinition.MduPropertyName);
            return GetLengthOfLongestString(names);
        }

        private static int GetLengthOfLongestPropertyValue(IEnumerable<WaterFlowFMProperty> waterFlowFmProperties)
        {
            IEnumerable<string> values = waterFlowFmProperties.Select(p => p.GetValueAsString());
            return GetLengthOfLongestString(values);
        }

        private static int GetLengthOfLongestString(IEnumerable<string> strings)
        {
            return strings.Aggregate((max, current) => max.Length > current.Length ? max : current)
                          .Length;
        }

        public void WriteBathymetry(WaterFlowFMModelDefinition modelDefinition, string path)
        {
            WaterFlowFMProperty bedLevelTypeProperty = modelDefinition.GetModelProperty(KnownProperties.BedlevType);
            if (bedLevelTypeProperty == null)
            {
                Log.WarnFormat("Cannot determine Bed level location, z-values will not be exported");
                return;
            }

            var location = (UnstructuredGridFileHelper.BedLevelLocation)bedLevelTypeProperty.Value;
            double[] values = modelDefinition.Bathymetry.Components[0].GetValues<double>().ToArray();
            UnstructuredGridFileHelper.WriteZValues(path, location, values);
        }
        
        private static void VerifyTargetDirectory(string targetMduFilePath)
        {
            string targetDir = Path.GetDirectoryName(targetMduFilePath);
            if (targetDir != string.Empty && !Directory.Exists(targetDir))
            {
                throw new Exception("Non existing directory in file path: " + targetMduFilePath);
            }
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

                if (!sourcePath.EqualsCaseInsensitive(targetPath))
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
            if (File.Exists(targetPath) && !IsNetfileCoordinateSystemUpToDate(modelDefinition, targetPath))
            {
                UnstructuredGridFileHelper.SetCoordinateSystem(targetPath, modelDefinition.CoordinateSystem);
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
                       !Array.Exists(SupportedFiles, sf => sf.EqualsCaseInsensitive(p.PropertyDefinition.MduPropertyName));
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

        private void WriteExternalForcings(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, bool switchTo = true)
        {
            var pathsRelativeToParent =
                (bool)modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).Value;
            
            WaterFlowFMProperty extForceFileProperty = modelDefinition.GetModelProperty(KnownProperties.ExtForceFile);

            string extForceFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, extForceFileProperty);
            if (string.IsNullOrEmpty(extForceFilePath))
            {
                string extForceFileName = modelDefinition.ModelName + FileConstants.ExternalForcingFileExtension;

                extForceFilePath = Path.Combine(Path.GetDirectoryName(targetMduFilePath), extForceFileName);
                extForceFileProperty.SetValueFromString(extForceFileName);
            }
            
            string extSubFilesReferenceFilePath = pathsRelativeToParent ? extForceFilePath : targetMduFilePath;

            if (ExternalForcingsFile == null)
            {
                ExternalForcingsFile = new ExtForceFile();
            }

            bool newFormatBoundaryConditions =
                modelDefinition.BoundaryConditions.Except(ExternalForcingsFile.ExistingBoundaryConditions).Any();

            bool newBoundaries =
                modelDefinition.Boundaries.Except(
                    ExternalForcingsFile.ExistingBoundaryConditions.Where(bc => bc.Feature != null)
                                        .Select(bc => bc.Feature)).Any();

            bool laterals = modelDefinition.Laterals.Any();

            bool writeNewFormat = newFormatBoundaryConditions || newBoundaries || laterals;
            
            // will check if indeed the file is written)
            ExternalForcingsFile.Write(extForceFilePath, extSubFilesReferenceFilePath, modelDefinition, !writeNewFormat, switchTo);

            if (writeNewFormat)
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

                string bndExtSubFilesReferenceFilePath = pathsRelativeToParent ? bndExtForceFilePath : targetMduFilePath;

                BoundaryExternalForcingsFile.Write(bndExtForceFilePath, bndExtSubFilesReferenceFilePath, modelDefinition);
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
            WaterFlowFMProperty sedFileProperty = modelDefinition.GetModelProperty(KnownProperties.SedFile);

            if (sedimentModelData == null)
            {
                sedFileProperty.SetValueFromString(string.Empty);
                return;
            }
            
            string sedFilePath = MduFileHelper.GetSubfilePath(mduPath, sedFileProperty);
            
            if (string.IsNullOrEmpty(sedFilePath))
            {
                string sedFileName = modelDefinition.ModelName + FileConstants.SedimentFileExtension;

                sedFilePath = Path.Combine(Path.GetDirectoryName(mduPath), sedFileName);
                sedFileProperty.SetValueFromString(sedFileName);
            }
            
            SedimentFile.Save(sedFilePath, modelDefinition, sedimentModelData);
        }

        private bool IsNetfileCoordinateSystemUpToDate(WaterFlowFMModelDefinition modelDefinition, string targetFile)
        {
            if (modelDefinition.CoordinateSystem != null && File.Exists(targetFile))
            {
                ICoordinateSystem fileCoordinateSystem = NetFile.ReadCoordinateSystem(targetFile);
                string fileProjectedCSName = GetProjectedCoordinateSystemNameFromNetFile(targetFile);

                if (fileCoordinateSystem == null ||
                    fileCoordinateSystem.AuthorityCode != modelDefinition.CoordinateSystem.AuthorityCode ||
                    !string.IsNullOrEmpty(fileProjectedCSName) &&
                    fileProjectedCSName != modelDefinition.CoordinateSystem.Name)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetProjectedCoordinateSystemNameFromNetFile(string targetFile)
        {
            NetCdfFile netCdfFile = null;
            var nameProjectedCS = string.Empty;

            try
            {
                netCdfFile = NetCdfFile.OpenExisting(targetFile, true);
                NetCdfVariable projectedCSVariable = netCdfFile.GetVariableByName("projected_coordinate_system");
                if (projectedCSVariable != null)
                {
                    nameProjectedCS = netCdfFile.GetAttributeValue(projectedCSVariable, "name");
                }
            }
            finally
            {
                if (netCdfFile != null)
                {
                    netCdfFile.Close();
                }
            }

            return nameProjectedCS;
        }

        private void WriteMduGroup(IMduFileWriteConfig config,
                                   bool writePartionFile,
                                   bool useNetCDFMapFormat,
                                   IGrouping<string, WaterFlowFMProperty> propertyGroup)
        {
            WriteLine("");
            WriteLine("[" + propertyGroup.Key + "]");

            foreach (WaterFlowFMProperty prop in propertyGroup)
            {
                string mduPropertyName = prop.PropertyDefinition.MduPropertyName;

                if (!writePartionFile && mduPropertyName.Equals("PartitionFile"))
                {
                    continue;
                }

                if (useNetCDFMapFormat && mduPropertyName.Equals(KnownProperties.MapFormat))
                {
                    WriteMduLine(mduPropertyName, "1", "# For 1d2d coupling we should always write mapformat output in NetCDF format");
                }
                else if (config.DisableFlowNodeRenumbering && mduPropertyName.Equals("RenumberFlowNodes"))
                {
                    WriteMduLine(mduPropertyName, "0", "# For 1d2d coupling we should never renumber the flownodes");
                }
                else if (IsConditional3DLayerProperty(mduPropertyName)
                         && PropertyIsDisabled(prop, propertyGroup))
                {
                    string defaultValue = prop.PropertyDefinition.DefaultValueAsString;
                    string mduPropertyComment = prop.PropertyDefinition.Description;
                    WriteMduLine(mduPropertyName, defaultValue, mduPropertyComment);
                }
                else
                {
                    string mduPropertyValue = GetPropertyValue(prop, config);
                    string mduPropertyComment = prop.PropertyDefinition.Description;
                    WriteMduLine(mduPropertyName, mduPropertyValue, mduPropertyComment);
                }
            }
        }

        private static bool PropertyIsDisabled(WaterFlowFMProperty prop, IEnumerable<WaterFlowFMProperty> propertyGroup)
        {
            return !prop.PropertyDefinition.IsEnabled(propertyGroup);
        }

        private static bool IsConditional3DLayerProperty(string mduPropertyName)
        {
            return mduPropertyName.EqualsCaseInsensitive(KnownProperties.DzTop)
                   || mduPropertyName.EqualsCaseInsensitive(KnownProperties.FloorLevTopLay)
                   || mduPropertyName.EqualsCaseInsensitive(KnownProperties.DzTopUniAboveZ)
                   || mduPropertyName.EqualsCaseInsensitive(KnownProperties.SigmaGrowthFactor)
                   || mduPropertyName.EqualsCaseInsensitive(KnownProperties.NumTopSig)
                   || mduPropertyName.EqualsCaseInsensitive(KnownProperties.NumTopSigUniform);
        }

        private void WriteMduLine(string propertyName, string propertyValue, string propertyComment)
        {
            string formatString = "{0,-" + (propertyKeyAlignmentLength + 1) + "}= " +
                                  "{1,-" + (propertyValueAlignmentLength + 1) + "}";

            string line = string.Format(formatString, propertyName, propertyValue);

            if (!string.IsNullOrEmpty(propertyComment))
            {
                line += $"# {propertyComment}";
            }

            WriteLine(line.Trim());
        }

        private IEnumerable<IGrouping<string, WaterFlowFMProperty>> GetPropertiesByGroup(
            IList<WaterFlowFMProperty> properties, IMduFileWriteConfig config)
        {
            WriteLine("# Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLine($"# Deltares, Plugin D-FLOW FM Version {FMSuiteFlowModelVersion}, " +
                      $"D-Flow FM Version {FMDllVersion}");
            SetValueToPropertyIfExists(properties, KnownProperties.Version, FMDllVersion);
            SetValueToPropertyIfExists(properties, KnownProperties.GuiVersion, FMSuiteFlowModelVersion);

            IEnumerable<IGrouping<string, WaterFlowFMProperty>> propertiesByGroup = 
                properties.Where(IsMduFileProperty)
                          .OrderBy(GetPropertySortIndex)
                          .GroupBy(p => p.PropertyDefinition.FileSectionName);
            
            return RemoveMorAndSedPropertiesIfNeeded(propertiesByGroup, properties, config);
        }

        private static bool IsMduFileProperty(WaterFlowFMProperty property)
        {
            return property.PropertyDefinition.FileSectionName != GuiProperties.GUIonly
                   // remove unknown properties that should be located on the sed/mor files
                   && property.PropertyDefinition.UnknownPropertySource != PropertySource.MorphologyFile
                   && property.PropertyDefinition.UnknownPropertySource != PropertySource.SedimentFile;
        }

        private static int GetPropertySortIndex(WaterFlowFMProperty property)
        {
            int sortIndex = property.PropertyDefinition.SortIndex;
            return sortIndex != -1 
                       ? sortIndex 
                       : int.MaxValue;
        }

        private static IEnumerable<IGrouping<string, WaterFlowFMProperty>> RemoveMorAndSedPropertiesIfNeeded(
            IEnumerable<IGrouping<string, WaterFlowFMProperty>> propertiesByGroup,
            IEnumerable<WaterFlowFMProperty> properties,
            IMduFileWriteConfig config)
        {
            /* Not include Morphology / Sediment MDUs if UseMorSed has not been selected */
            propertiesByGroup = propertiesByGroup.Where(p => !p.Key.Equals(KnownProperties.morphology));
            WaterFlowFMProperty useMorSedProp = properties.FirstOrDefault(md => md.PropertyDefinition.MduPropertyName == "UseMorSed");
            if (useMorSedProp != null &&
                (!config.WriteMorphologySediment || int.TryParse(GetPropertyValue(useMorSedProp, config), out int useMorSed) && useMorSed != 1))
            {
                propertiesByGroup = propertiesByGroup.Where(p => !p.Key.Equals(KnownProperties.sediment));
            }

            return propertiesByGroup;
        }

        private static void SetValueToPropertyIfExists(IEnumerable<WaterFlowFMProperty> modelDefinition, string name,
                                                       string value)
        {
            WaterFlowFMProperty waterFlowFmProperty = modelDefinition?.FirstOrDefault(
                p => p.PropertyDefinition.MduPropertyName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            waterFlowFmProperty?.SetValueFromString(value);
        }

        private static string GetPropertyValue(WaterFlowFMProperty prop, IMduFileWriteConfig config)
        {
            string propertyName = prop.PropertyDefinition.MduPropertyName.ToLower();
            if (!config.WriteExtForcings &&
                (propertyName == KnownProperties.ExtForceFile || propertyName == KnownProperties.BndExtForceFile))
            {
                return string.Empty;
            }

            if (!config.WriteFeatures &&
                (propertyName == KnownProperties.DryPointsFile || propertyName == KnownProperties.LandBoundaryFile ||
                 propertyName == KnownProperties.ThinDamFile || propertyName == KnownProperties.FixedWeirFile ||
                 propertyName == KnownProperties.BridgePillarFile ||
                 propertyName == KnownProperties.ManholeFile || propertyName == KnownProperties.ObsFile ||
                 propertyName == KnownProperties.ObsCrsFile))
            {
                return string.Empty;
            }

            if (!config.WriteRestartStartTime && propertyName == KnownProperties.RestartDateTime)
            {
                return string.Empty;
            }

            return prop.GetValueAsString();
        }

        private static void WriteFeatures<TFeat, TFile>(string targetMduFilePath,
                                                        WaterFlowFMModelDefinition modelDefinition,
                                                        string propertyKey, IList<TFeat> features, ref TFile fileWriter,
                                                        string extension, params string[] alternativeExtensions)
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
                    if (fileWriter is StructuresFile structuresFileReader)
                    {
                        structuresFileReader.ReferencePath =
                            (bool)modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).Value
                                ? filePath
                                : targetMduFilePath;
                    }
                    
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

        private static void UpdateFixedWeirs(HydroArea hydroArea, Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>> fixedWeirPropertiesMapping)
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
        }

        private static void InitializeFeatureWriter<TFeat>(string targetMduFilePath, IList<TFeat> features,
                                                           string extension,
                                                           WaterFlowFMProperty waterFlowFMProperty,
                                                           out List<IGrouping<string, IGroupableFeature>> grouping,
                                                           params string[] alternativeExtensions)
        {
            string defaultGroup = Path.GetFileNameWithoutExtension(targetMduFilePath);
            MduFileHelper.UpdateFeatures(features, extension, defaultGroup);
            
            IEnumerable<string> writeToRelativeFilePaths = MduFileHelper.GetUniqueFilePathsForWindows(targetMduFilePath, features, extension, alternativeExtensions);

            grouping = features.OfType<IGroupableFeature>()
                               .GroupBy(f => f.GroupName, StringComparer.InvariantCultureIgnoreCase).ToList();

            waterFlowFMProperty.SetValueFromStrings(writeToRelativeFilePaths.Select(f => Path.HasExtension(f) ? f : string.Concat(f, extension)));
        }

        private static void AddDryAreasToWriter(string targetMduFilePath, IList<GroupableFeature2DPolygon> features,
                                                string extension,
                                                WaterFlowFMProperty waterFlowFMProperty,
                                                out List<IGrouping<string, IGroupableFeature>> grouping)
        {
            string defaultGroup = Path.GetFileNameWithoutExtension(targetMduFilePath);
            MduFileHelper.UpdateFeatures(features, extension, defaultGroup);
            
            List<string> writeToRelativeFilePaths = MduFileHelper.GetUniqueFilePathsForWindows(targetMduFilePath, features, extension).ToList();

            grouping = features.OfType<IGroupableFeature>()
                               .GroupBy(f => f.GroupName, StringComparer.InvariantCultureIgnoreCase).ToList();

            if (writeToRelativeFilePaths.Any())
            {
                string currentValue = waterFlowFMProperty.GetValueAsString();
                string newValue = currentValue + " " + string.Join(" ", writeToRelativeFilePaths.Select(f => Path.HasExtension(f) ? f : string.Concat(f, extension)));
                waterFlowFMProperty.SetValueFromString(newValue);
            }
        }

        private void WriteDryPointsAndDryAreas(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
                                               IList<GroupablePointFeature> dryPoints,
                                               IList<GroupableFeature2DPolygon> dryAreas)
        {
            WaterFlowFMProperty waterFlowFMProperty = modelDefinition.GetModelProperty(KnownProperties.DryPointsFile);
            if (dryPoints.Any() || dryAreas.Any())
            {
                List<IGrouping<string, IGroupableFeature>> dryPointsPerGroup;
                List<IGrouping<string, IGroupableFeature>> dryAreasPerGroup;
                
                InitializeFeatureWriter(targetMduFilePath, dryPoints, FileConstants.DryPointFileExtension, waterFlowFMProperty, out dryPointsPerGroup);
                AddDryAreasToWriter(targetMduFilePath, dryAreas, FileConstants.DryAreaFileExtension, waterFlowFMProperty, out dryAreasPerGroup);

                List<string> featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(targetMduFilePath, waterFlowFMProperty).ToList();
                featuresFilePaths.RemoveAllWhere(ffp => ffp == null);

                if (dryAreaFile == null)
                {
                    dryAreaFile = new PolFile<GroupableFeature2DPolygon>();
                }

                foreach (string featuresFilePath in featuresFilePaths)
                {
                    if (featuresFilePath.EndsWith(FileConstants.DryPointFileExtension))
                    {
                        // Create the directory to which the file is being written, because XyzFile class does not handle this.
                        string writeDirectory = Path.GetDirectoryName(featuresFilePath);
                        if (!string.IsNullOrEmpty(writeDirectory) && !Directory.Exists(writeDirectory))
                        {
                            Directory.CreateDirectory(writeDirectory);
                        }

                        string fileName = FileUtils.GetRelativePath(Path.GetDirectoryName(targetMduFilePath), featuresFilePath, true);
                        string fileNameWithoutExtension = fileName.Replace(FileConstants.DryPointFileExtension, string.Empty);
                        
                        IList<GroupablePointFeature> groupFeatures = GetAllGroupadFeaturesFromFile<GroupablePointFeature>(dryPointsPerGroup, fileName, fileNameWithoutExtension);
                        IList<GroupablePointFeature> featuresToWrite = dryPoints.Count > 0 && groupFeatures != null ? groupFeatures : dryPoints;
                        
                        XyzFile.Write(featuresFilePath, featuresToWrite.Select(p => new PointValue
                        {
                            X = p.X,
                            Y = p.Y,
                            Value = 0
                        }));
                    }
                    else if (featuresFilePath.EndsWith(FileConstants.DryAreaFileExtension))
                    {
                        string fileName = FileUtils.GetRelativePath(Path.GetDirectoryName(targetMduFilePath), featuresFilePath, true);
                        string fileNameWithoutExtension = fileName.Replace(FileConstants.DryAreaFileExtension, string.Empty);
                        
                        IList<GroupableFeature2DPolygon> groupFeatures = GetAllGroupadFeaturesFromFile<GroupableFeature2DPolygon>(dryAreasPerGroup, fileName, fileNameWithoutExtension);
                        IList<GroupableFeature2DPolygon> featuresToWrite = dryAreas.Count > 0 && groupFeatures != null ? groupFeatures : dryAreas;
                        
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
            
            if (fileWriter is PlizFile<FixedWeir> fixedWeirFileWriter)
            {
                fixedWeirFileWriter.CreateDelegate = delegate (List<Coordinate> points, string name)
                {
                    var feature = new FixedWeir
                    {
                        Name = name,
                        Geometry = LineStringCreator.CreateLineString(points)
                    };
                    return feature;
                };
            }

            if (fileWriter is PlizFile<BridgePillar> bridgePillarFileWriter)
            {
                bridgePillarFileWriter.CreateDelegate = (points, name) => CreateDelegateBridgePillar(name, points);
            }

            if (fileWriter is StructuresFile structuresFileWriter)
            {
                structuresFileWriter.StructureSchema = modelDefinition.StructureSchema;
                structuresFileWriter.ReferenceDate = modelDefinition.GetReferenceDateAsDateTime();
            }

            return fileWriter;
        }

        internal static BridgePillar CreateDelegateBridgePillar(string name, List<Coordinate> points)
        {
            var feature = new BridgePillar { Name = name };
            feature.Geometry = LineStringCreator.CreateLineString(points);
            return feature;
        }

        private void WriteAreaFeatures(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
                                       HydroArea hydroArea,
                                       IEnumerable<ModelFeatureCoordinateData<FixedWeir>>
                                           allFixedWeirsAndCorrespondingProperties)
        {
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.LandBoundaryFile,
                          hydroArea.LandBoundaries, ref landBoundariesFile, FileConstants.LandBoundaryFileExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams.ToList(),
                          ref thinDamFile, FileConstants.ThinDamPliFileExtension, FileConstants.ThinDamPlizFileExtension);

            UpdateFixedWeirs(hydroArea, allFixedWeirsAndCorrespondingProperties.ToDictionary(p => p.Feature));

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.FixedWeirFile,
                          hydroArea.FixedWeirs.ToList(),
                          ref fixedWeirFile, FileConstants.FixedWeirPlizFileExtension, FileConstants.FixedWeirPliFileExtension);

            foreach (FixedWeir fixedWeir in hydroArea.FixedWeirs)
            {
                fixedWeir.Attributes.Clear();
            }

            /*Bridge pillars*/
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.BridgePillarFile, hydroArea.BridgePillars,
                          ref bridgePillarFile, FileConstants.PlizFileExtension);
            /**/

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ObsFile, hydroArea.ObservationPoints,
                          ref obsFile, FileConstants.ObsPointFileExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ObsCrsFile,
                          hydroArea.ObservationCrossSections.ToList(),
                          ref obsCrsFile, FileConstants.ObsCrossSectionPliFileExtension, FileConstants.ObsCrossSectionPlizFileExtension);

            List<IStructureObject> structures = hydroArea.Pumps.Cast<IStructureObject>().Concat(hydroArea.Structures).ToList();

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.StructuresFile, structures,
                          ref structuresFile, FileConstants.StructuresFileExtension);

            WriteDryPointsAndDryAreas(targetMduFilePath, modelDefinition, hydroArea.DryPoints, hydroArea.DryAreas);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.EnclosureFile, hydroArea.Enclosures,
                          ref enclosureFile, FileConstants.EnclosureExtension);
        }

        public void WriteLandBoundaries(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
                                        HydroArea hydroArea)
        {
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.LandBoundaryFile,
                          hydroArea.LandBoundaries, ref landBoundariesFile, FileConstants.LandBoundaryFileExtension);
        }

        private static List<TFeat> GetAllGroupadFeaturesFromFile<TFeat>(
            IEnumerable<IGrouping<string, IGroupableFeature>> grouping, string fileName,
            string fileNameWithoutExtension)
            where TFeat : IGroupableFeature
        {
            IGrouping<string, IGroupableFeature> groupFeatures =
                grouping.FirstOrDefault(g => g.Key.ToLowerInvariant().Equals(fileName.ToLowerInvariant())
                                             || g.Key.ToLowerInvariant()
                                                 .Equals(fileNameWithoutExtension.ToLowerInvariant())
                                             || g.Key.ToLowerInvariant()
                                                 .Replace(Path.GetExtension(FileConstants.DryAreaFileExtension), string.Empty)
                                                 .Equals(fileNameWithoutExtension.ToLowerInvariant()));
            if (groupFeatures == null)
            {
                return null;
            }

            try
            {
                List<TFeat> groupFeaturesList = groupFeatures.Cast<TFeat>().ToList();
                return groupFeaturesList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region read logic

        public void Read(string filePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea,
                         IDictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>> fixedWeirProperties,
                         Action<string, int, int> reportProgress = null,
                         IList<ModelFeatureCoordinateData<BridgePillar>> allBridgePillarsAndCorrespondingProperties =
                             null)
        {
            FilePath = filePath;
            if (reportProgress == null)
            {
                reportProgress = (name, current, total) => { };
            }

            var totalSteps = 5;

            reportProgress("Reading properties", 2, totalSteps);
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                MduFileReader.Read(fileStream, filePath, modelDefinition);
            }

            var validator = new MduFileValidator(filePath, modelDefinition);
            validator.Validate();

            var pathsRelativeToParent =
                (bool)modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).Value;

            reportProgress("Reading morphology properties", 3, totalSteps);
            MorphologyFile.Read(filePath, modelDefinition);

            reportProgress("Reading area features", 4, totalSteps);
            ReadAreaFeatures(filePath, modelDefinition, hydroArea);

            //fix for fixed weirs
            fixedWeirProperties?.Clear();

            var logHandler = new LogHandler("reading the Fixed Weirs");

            foreach (FixedWeir fixedWeir in hydroArea.FixedWeirs)
            {
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir> { Feature = fixedWeir };
                string scheme = modelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();
                modelFeatureCoordinateData.UpdateDataColumns(scheme);

                bool locationKeyFound = fixedWeir.Attributes.ContainsKey(Feature2D.LocationKey);
                int indexKey = !locationKeyFound
                                   ? -1
                                   : fixedWeir.Attributes.Keys.ToList().IndexOf(Feature2D.LocationKey);

                int numberFixedWeirAttributes =
                    !locationKeyFound ? fixedWeir.Attributes.Count : fixedWeir.Attributes.Count - 1;

                int difference = Math.Abs(modelFeatureCoordinateData.DataColumns.Count - numberFixedWeirAttributes);

                if (scheme != "0" && modelFeatureCoordinateData.DataColumns.Count < fixedWeir.Attributes.Count)
                {
                    logHandler.ReportWarningFormat(Resources.MduFile_Read_Based_on_the_Fixed_Weir_Scheme__0___there_are_too_many_column_s__defined_for__1__in_the_imported_fixed_weir_file__The_last__2__column_s__have_been_ignored,
                                                   scheme, fixedWeir, difference);
                }

                if (scheme != "0" && modelFeatureCoordinateData.DataColumns.Count > fixedWeir.Attributes.Count)
                {
                    logHandler.ReportWarningFormat(Resources.MduFile_Read_Based_on_the_Fixed_Weir_Scheme__0___there_are_not_enough_column_s__defined_for__1__in_the_imported_fixed_weir_file__The_last__2__column_s__have_been_generated_using_default_values,
                                                   scheme, fixedWeir, difference);
                }

                for (var index = 0; index < modelFeatureCoordinateData.DataColumns.Count; index++)
                {
                    if (index < fixedWeir.Attributes.Count)
                    {
                        if (index == indexKey)
                        {
                            continue;
                        }

                        IDataColumn dataColumn = modelFeatureCoordinateData.DataColumns[index];
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

                fixedWeirProperties.Add(fixedWeir, modelFeatureCoordinateData);
            }

            logHandler.LogReport();

            foreach (FixedWeir fixedWeir in hydroArea.FixedWeirs)
            {
                fixedWeir.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
            }

            if (allBridgePillarsAndCorrespondingProperties != null)
            {
                foreach (BridgePillar bridgePillar in hydroArea.BridgePillars)
                {
                    var modelFeatureCoordinateData =
                        new ModelFeatureCoordinateData<BridgePillar>() { Feature = bridgePillar };

                    string bpFile = modelDefinition.GetModelProperty(KnownProperties.BridgePillarFile)
                                                   .GetValueAsString();
                    modelFeatureCoordinateData.UpdateDataColumns();

                    int difference =
                        Math.Abs(modelFeatureCoordinateData.DataColumns.Count - bridgePillar.Attributes.Count);

                    if (modelFeatureCoordinateData.DataColumns.Count < bridgePillar.Attributes.Count)
                    {
                        Log.Warn(
                            string.Format(
                                Resources
                                    .MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_too_many_column_s__defined_for__1___The_last__2__column_s__have_been_ignored,
                                bpFile, bridgePillar, difference));
                    }

                    if (modelFeatureCoordinateData.DataColumns.Count > bridgePillar.Attributes.Count)
                    {
                        Log.Warn(
                            string.Format(
                                Resources
                                    .MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_not_enough_column_s__defined_for__1___The_last__2__column_s__have_been_generated_using_default_values,
                                bpFile, bridgePillar?.Name, difference));
                    }

                    SetBridgePillarDataModel(allBridgePillarsAndCorrespondingProperties, modelFeatureCoordinateData,
                                             bridgePillar);
                }

                foreach (BridgePillar bridgePillar in hydroArea.BridgePillars)
                {
                    bridgePillar.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
                }
            }

            reportProgress("Reading external forcings file", 4, totalSteps);
            WaterFlowFMProperty extForceFileProperty = modelDefinition.GetModelProperty(KnownProperties.ExtForceFile);
            if (extForceFileProperty != null)
            {
                string forceFilePath = MduFileHelper.GetSubfilePath(filePath,
                                                                    modelDefinition.GetModelProperty(
                                                                        KnownProperties.ExtForceFile));

                if (forceFilePath != null && File.Exists(forceFilePath))
                {
                    ExternalForcingsFile = new ExtForceFile();

                    string extSubFilesReferenceFilePath = pathsRelativeToParent ? forceFilePath : filePath;

                    ExternalForcingsFile.Read(forceFilePath, extSubFilesReferenceFilePath, modelDefinition);
                }
            }

            reportProgress("Reading boundary external forcings file", 5, totalSteps);
            WaterFlowFMProperty bndExtForceFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile);
            if (bndExtForceFileProperty != null)
            {
                string forceFilePath = MduFileHelper.GetSubfilePath(filePath, bndExtForceFileProperty);

                if (forceFilePath != null && File.Exists(forceFilePath))
                {
                    BoundaryExternalForcingsFile = new BndExtForceFile();

                    string bndExtSubFilesReferenceFilePath = pathsRelativeToParent ? forceFilePath : filePath;

                    BoundaryExternalForcingsFile.Read(forceFilePath, bndExtSubFilesReferenceFilePath, modelDefinition);
                }
            }

            WaterFlowFMProperty initialFieldFileProperty = modelDefinition.GetModelProperty(KnownProperties.IniFieldFile);
            if (initialFieldFileProperty != null)
            {
                string initialFieldFilePath = MduFileHelper.GetSubfilePath(filePath, initialFieldFileProperty);
                if (initialFieldFilePath != null)
                {
                    string relativeParentPath = pathsRelativeToParent ? initialFieldFilePath : filePath;
                    initialFieldFileReader.Read(initialFieldFilePath, relativeParentPath, modelDefinition);
                }           

            }
        }

        private static void LoadAttributeIntoDataColumn(GeometryPointsSyncedList<double> loadedData,
                                                        IDataColumn dataColumn)
        {
            // Just a refactor of the setter.
            if (dataColumn == null || loadedData == null)
            {
                return;
            }

            dataColumn.ValueList = loadedData.ToList();
        }

        private void ReadFeatures<TFeat, TFile>(string mduFilePath, WaterFlowFMModelDefinition modelDefinition,
                                                string propertyKey, IList<TFeat> features, ref TFile fileReader, 
                                                string extension) where TFile : IFeature2DFileBase<TFeat>, new()
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyKey);
            IList<string> featuresFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelProperty);
            
            if (propertyKey == KnownProperties.StructuresFile)
            {
                RemoveAllStructuresFilesWithBadReferences(modelDefinition, modelProperty, featuresFilePaths);
            }
            
            if (featuresFilePaths == null || featuresFilePaths.Count == 0)
            {
                return;
            }

            fileReader = CreateFeatureFile<TFeat, TFile>(modelDefinition);

            var readFeatures = new List<TFeat>();
            foreach (string featuresFilePath in featuresFilePaths)
            {
                if (fileReader is StructuresFile structuresFileReader)
                {
                    structuresFileReader.ReferencePath =
                        (bool)modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).Value
                            ? featuresFilePath
                            : mduFilePath;
                }

                IList<TFeat> featuresToAdd = fileReader.Read(featuresFilePath);

                if (modelProperty.PropertyDefinition.IsMultipleFile)
                {
                    //make sure the features have the right group name.
                    List<IGroupableFeature> asGroupable = featuresToAdd.OfType<IGroupableFeature>().ToList();
                    string featurePathName =
                        FileUtils.GetRelativePath(Path.GetDirectoryName(mduFilePath), featuresFilePath, true);
                    string mduPathName = Path.GetFileNameWithoutExtension(mduFilePath);
                    asGroupable.ForEach(f =>
                    {
                        f.GroupName = featurePathName;
                        f.IsDefaultGroup = featurePathName != null &&
                                           featurePathName.Replace(extension, string.Empty).Trim().Equals(mduPathName);
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
                         ref landBoundariesFile, FileConstants.LandBoundaryFileExtension);

            ReadFeatures(filePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams,
                         ref thinDamFile, FileConstants.ThinDamPliFileExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.FixedWeirFile, hydroArea.FixedWeirs,
                         ref fixedWeirFile, FileConstants.FixedWeirPlizFileExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.ObsFile, hydroArea.ObservationPoints,
                         ref obsFile, FileConstants.ObsPointFileExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.ObsCrsFile, hydroArea.ObservationCrossSections,
                         ref obsCrsFile, FileConstants.ObsCrossSectionPliFileExtension);
            ReadFeatures(filePath, modelDefinition, KnownProperties.BridgePillarFile, hydroArea.BridgePillars,
                         ref bridgePillarFile, FileConstants.PlizFileExtension);

            var structures = new List<IStructureObject>();

            ReadFeatures(filePath, modelDefinition, KnownProperties.StructuresFile, structures,
                         ref structuresFile, FileConstants.StructuresFileExtension);

            foreach (IStructureObject structure in structures)
            {
                switch (structure)
                {
                    case Pump pump:
                        hydroArea.Pumps.Add(pump);
                        break;
                    case Structure weir:
                        hydroArea.Structures.Add(weir);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            ReadDryPointsAndDryAreas(filePath, modelDefinition, hydroArea);

            IList<string> enclosureMultipleFilePath =
                MduFileHelper.GetMultipleSubfilePath(
                    filePath, modelDefinition.GetModelProperty(KnownProperties.EnclosureFile));

            List<string> enclosuresToRemove = enclosureMultipleFilePath
                                              .Where(efp => !efp.EndsWith(PolFile<GroupableFeature2DPolygon>.Extension))
                                              .ToList();
            if (enclosuresToRemove.Count > 0)
            {
                Log.WarnFormat(
                    "The following enclosure files do not contain the correct extension and will not be read. {0}",
                    enclosuresToRemove);
                enclosureMultipleFilePath.RemoveAllWhere(efp => !efp.EndsWith(PolFile<GroupableFeature2DPolygon>.Extension));
            }

            if (enclosureMultipleFilePath.Count > 0)
            {
                ReadFeatures(filePath, modelDefinition, KnownProperties.EnclosureFile, hydroArea.Enclosures,
                             ref enclosureFile, FileConstants.EnclosureExtension);

                if (hydroArea.Enclosures.Count > 1)
                {
                    /*We do not support more than one enclosure, but the user should still be able to import everything. */
                    Log.WarnFormat(
                        Resources
                            .MduFile_ReadAreaFeatures_Multiple_enclosures_added_to_the_model__Validate_or_run_will_fail_if_more_than_one_enclosure_is_present_);
                }
            }
        }

        private void ReadDryPointsAndDryAreas(string mduFilePath, WaterFlowFMModelDefinition modelDefinition,
                                              HydroArea hydroArea)
        {
            const string dryPointsPropertyKey = KnownProperties.DryPointsFile;
            string mduPathName = Path.GetFileNameWithoutExtension(mduFilePath);

            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(dryPointsPropertyKey);
            IList<string> featureFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelProperty);
            if (!featureFilePaths.Any())
            {
                return;
            }

            if (dryAreaFile == null)
            {
                dryAreaFile = new PolFile<GroupableFeature2DPolygon>();
            }

            foreach (string featureFilePath in featureFilePaths)
            {
                string featureFilePathExtension = Path.GetExtension(featureFilePath);
                string groupName =
                    FileUtils.GetRelativePath(Path.GetDirectoryName(mduFilePath), featureFilePath, true);
                //By checking if the feature file path only ends with .xyz (without the _dry suffix),
                //we can assure that backwards compatibility is ensure i.e. some old mdu files still refer to the dry point file without the _dry suffix.
                if (featureFilePathExtension != null && featureFilePath.EndsWith(FileConstants.XyzFileExtension))
                {
                    IList<IPointValue> pointValues = XyzFile.Read(featureFilePath);
                    bool isDefaultGroup = groupName.Replace(featureFilePathExtension, string.Empty).Trim()
                                                   .Equals(mduPathName);
                    IEnumerable<GroupablePointFeature> dryPointsToAdd =
                        pointValues.Select(p => new GroupablePointFeature()
                        {
                            Geometry = p.Geometry,
                            GroupName = groupName,
                            IsDefaultGroup = isDefaultGroup
                        });
                    hydroArea.DryPoints.AddRange(dryPointsToAdd);
                }
                // D3DFMIQ-1037: We want to import all kind of dry areas as long as they come from a .pol file (regardless of their suffix).
                else if (featureFilePath.EndsWith(FileConstants.PolylineFileExtension))
                {
                    IList<GroupableFeature2DPolygon> dryAreasToAdd = dryAreaFile.Read(featureFilePath);
                    EnumerableExtensions.ForEach(dryAreasToAdd, f =>
                    {
                        f.GroupName = groupName;
                        f.IsDefaultGroup = groupName != null &&
                                           groupName.Replace(FileConstants.PolylineFileExtension, string.Empty).Trim().Equals(mduPathName);
                    });
                    hydroArea.DryAreas.AddRange(dryAreasToAdd);
                }
            }
        }

        /// <summary>
        /// Removes all structures files that contain references to feature files that do not exist.
        /// </summary>
        /// <param name="modelDefinition"> The model definition of the FM Model. </param>
        /// <param name="structureFileProperty"> The structures file property. </param>
        /// <param name="featureFilePaths"> The group names of all features, retrieved from the mdu file of the FM Model. </param>
        private void RemoveAllStructuresFilesWithBadReferences(WaterFlowFMModelDefinition modelDefinition, 
                                                               WaterFlowFMProperty structureFileProperty,
                                                               ICollection<string> featureFilePaths)
        {
            var pathsRelativeToParent =
                (bool)modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).Value;

            var structureFilesWithBadReferences = new List<string>();
            foreach (string filePath in featureFilePaths)
            {
                var logHandler = new LogHandler("import of the structure file: " + filePath);

                string structureFilePath = Path.GetFullPath(filePath);

                string structuresSubFilesReferenceFilePath = pathsRelativeToParent ? filePath : FilePath;

                var fileReader = new StructuresFile
                {
                    StructureSchema = modelDefinition.StructureSchema,
                    ReferenceDate = modelDefinition.GetReferenceDateAsDateTime()
                };

                List<StructureDAO> featuresToAdd = fileReader.ReadStructuresFromFile(structureFilePath).ToList();
                var hasBadFileReferences = false;

                foreach (StructureDAO f in featuresToAdd)
                {
                    string featureFileName = f.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString();
                    string featureFilePath =
                        Path.Combine(Path.GetDirectoryName(structuresSubFilesReferenceFilePath), featureFileName);

                    if (!File.Exists(featureFilePath))
                    {
                        hasBadFileReferences = true;
                        logHandler.ReportErrorFormat(Resources.MduFile_FilePath_0_referenced_in_StructuresFile_1_does_not_exist, featureFilePath,
                                                     structureFilePath);
                    }
                }

                if (hasBadFileReferences)
                {
                    structureFilesWithBadReferences.Add(structureFilePath);
                    logHandler.LogReport();
                }
            }

            featureFilePaths.RemoveAllWhere(gn => structureFilesWithBadReferences.Contains(gn));
            
            structureFileProperty.SetValueFromString(string.Join(" ", featureFilePaths));
        }

        #endregion

        #region bridge pillar helper

        /// <summary>
        /// Sets the bridge pillar data model.
        /// </summary>
        /// <param name="allBridgePillarsAndCorrespondingProperties"> All bridge pillars and corresponding properties. </param>
        /// <param name="modelFeatureCoordinateData"> The model feature coordinate data. </param>
        /// <param name="bridgePillar"> The bridge pillar. </param>
        internal static void SetBridgePillarDataModel(
            IList<ModelFeatureCoordinateData<BridgePillar>> allBridgePillarsAndCorrespondingProperties,
            ModelFeatureCoordinateData<BridgePillar> modelFeatureCoordinateData, BridgePillar bridgePillar)
        {
            if (allBridgePillarsAndCorrespondingProperties == null
                || modelFeatureCoordinateData == null
                || bridgePillar == null)
            {
                return;
            }

            /*Add the attributes from the Bridge pillar into the model data IF they exist.*/
            List<KeyValuePair<string, object>> attrList = bridgePillar.Attributes.ToList();
            attrList.ForEach(attr =>
                                 LoadAttributeIntoDataColumn(
                                     attr.Value as GeometryPointsSyncedList<double>,
                                     modelFeatureCoordinateData
                                         .DataColumns.ElementAtOrDefault(attrList.IndexOf(attr))));

            allBridgePillarsAndCorrespondingProperties.Add(modelFeatureCoordinateData);
        }

        /// <summary>
        /// Sets the bridge pillar extra attributes (besides the classic x,y).
        /// </summary>
        /// <param name="bridgePillars"> The bridge pillars. </param>
        /// <param name="modelFeatureCoordinateDatas"> The model feature coordinate datas. </param>
        internal static void SetBridgePillarAttributes(IEnumerable<BridgePillar> bridgePillars,
                                                       IList<ModelFeatureCoordinateData<BridgePillar>>
                                                           modelFeatureCoordinateDatas)
        {
            foreach (BridgePillar bridgePillar in bridgePillars)
            {
                bridgePillar.Attributes = new DictionaryFeatureAttributeCollection();

                ModelFeatureCoordinateData<BridgePillar> modelFeatureCoordinateData =
                    modelFeatureCoordinateDatas.FirstOrDefault(d => d.Feature == bridgePillar);
                if (modelFeatureCoordinateData == null)
                {
                    return;
                }

                for (var index = 0; index < modelFeatureCoordinateData.DataColumns.Count; index++)
                {
                    if (!modelFeatureCoordinateData.DataColumns[index].IsActive)
                    {
                        break;
                    }

                    IList dataColumnWithData = modelFeatureCoordinateData.DataColumns[index].ValueList;

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
        /// <param name="bridgePillars"> The bridge pillars. </param>
        internal static void CleanBridgePillarAttributes(IEnumerable<BridgePillar> bridgePillars)
        {
            if (bridgePillars != null)
            {
                EnumerableExtensions.ForEach(bridgePillars, bp => bp.Attributes.Clear());
            }
        }

        #endregion
    }
}