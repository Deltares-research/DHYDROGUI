using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class MduFile : FMSuiteFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MduFile));

        private static string FMSuiteFlowModelVersion;
        private static string FMDllVersion;

        public const string MduExtension = ".mdu";
        public const string LandBoundariesExtension = ".ldb";
        public const string ThinDamExtension = "_thd.pli";
        public const string FixedWeirExtension = "_fxw.pli";
        public const string StructuresExtension = "_structures.ini"; // TODO: Might not want to require a specific extension
        public const string ObsExtension = "_obs.xyn";
        public const string ObsCrossExtension = "crs.pli";
        public const string DryAreaExtension = "_dry.pol";
        public const string DryPointExtension = "_dry.xyz";
        public const string MorphologyExtension = ".mor";
        public const string SedimentExtension = ".sed";

        private readonly Dictionary<string, string> mduComments = new Dictionary<string, string>();

        private LdbFile landBoundariesFile;
        private PliFile<ThinDam2D> thinDamFile;
        private PliFile<FixedWeir> fixedWeirFile;
        private StructuresFile structuresFile;
        private ObsFile obsFile;
        private PliFile<ObservationCrossSection2D> obsCrsFile;
        private PolFile dryAreaFile;
        private XyzFile dryPointFile;

        // the following mdu-referenced files are written by the UI, or at least should not be copied along blindly 
        // (please keep this list up-to-date!):

        private static readonly string[] SupportedFiles =
        {
            KnownProperties.NetFile, KnownProperties.ExtForceFile, KnownProperties.MapFile__Obsolete,
            KnownProperties.HisFile__Obsolete, KnownProperties.ThinDamFile, KnownProperties.FixedWeirFile,
            KnownProperties.ObsFile, KnownProperties.ObsCrsFile, KnownProperties.LandBoundaryFile,
            KnownProperties.DryPointsFile, KnownProperties.RestartFile, KnownProperties.StructuresFile
        };

        public MduFile()
        {
            if (FMDllVersion != null) 
                return; // do it once

            using (var api = new RemoteFlexibleMeshModelApi())
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

        #region write logic

        public void Write(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea,
            bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true, bool useNetCDFMapFormat = false, bool disableFlowNodeRenumbering = false)
        {
            var targetDir = System.IO.Path.GetDirectoryName(targetMduFilePath);
            if (targetDir != string.Empty && !Directory.Exists(targetDir))
            {
                throw new Exception("Non existing directory in file path: " + targetMduFilePath);
            }

            var substitutedPaths = new Dictionary<string, System.Tuple<string, string>>();

            // copy netfile
            if (Path != null)
            {
                var sourceFile = MduFileHelper.GetSubfilePath(Path,
                    modelDefinition.GetModelProperty(KnownProperties.NetFile));
                if (sourceFile != null)
                {
                    var targetFile = System.IO.Path.Combine(targetDir, System.IO.Path.GetFileName(sourceFile));

                    if (File.Exists(sourceFile))
                    {
                        var fullSourcePath = string.IsNullOrEmpty(sourceFile) ? string.Empty : System.IO.Path.GetFullPath(sourceFile);
                        var fullTargetPath = string.IsNullOrEmpty(targetFile) ? string.Empty : System.IO.Path.GetFullPath(targetFile);

                        if (fullSourcePath.ToLower() != fullTargetPath.ToLower())
                        {
                            File.Copy(fullSourcePath, fullTargetPath, true);
                        }
                    }

                    // write the bathymetry in the net file.
                    IList<ISpatialOperation> bathymetryOperations;
                    if (modelDefinition.SpatialOperations.TryGetValue(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, out bathymetryOperations))
                    {
                        if (bathymetryOperations.Any(so => !(so is ISpatialOperationSet)))
                        {
                            UnstructuredGridFileHelper.WriteZValues(targetFile, modelDefinition.Bathymetry.Components[0].GetValues<double>().ToArray());
                        }
                    }

                    // if needed, adjust coordinate system in netfile
                    if (modelDefinition.CoordinateSystem != null && File.Exists(targetFile))
                    {
                        var fileCoordinateSystem = NetFile.ReadCoordinateSystem(targetFile);
                        if (fileCoordinateSystem == null ||
                            fileCoordinateSystem.AuthorityCode != modelDefinition.CoordinateSystem.AuthorityCode)
                        {
                            UnstructuredGridFileHelper.SetCoordinateSystem(targetFile, modelDefinition.CoordinateSystem);
                        }
                    }
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

                    var absoluteSourcePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), relativeSourcePath);
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

            if (switchTo)
                Path = targetMduFilePath;

            if (writeFeatures)
            {
                WriteAreaFeatures(targetMduFilePath, modelDefinition, hydroArea);
            }

            // write external forcings (ExtForceFile.Write() will check if indeed the file is written)
            if (writeExtForcings)
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

                var newFormatBoundaryConditions =
                    modelDefinition.BoundaryConditions.Except(ExternalForcingsFile.ExistingBoundaryConditions).Any();

                var newBoundaries =
                    modelDefinition.Boundaries.Except(
                        ExternalForcingsFile.ExistingBoundaryConditions.Where(bc => bc.Feature != null)
                            .Select(bc => bc.Feature)).Any();

                // TODO: fix this, also, multiple FM models for a single integrated hydroregion to be expected?!
                var hasEmbankments = hydroArea.Embankments.Any();
                modelDefinition.Embankments = hydroArea.Embankments;

                ExternalForcingsFile.Write(extForceFilePath, modelDefinition, !(newFormatBoundaryConditions || newBoundaries));

                if (newFormatBoundaryConditions || newBoundaries || hasEmbankments)
                {
                    var bndExtFileName =
                        modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).GetValueAsString();
                    if (string.IsNullOrEmpty(bndExtFileName))
                        bndExtFileName = System.IO.Path.GetFileNameWithoutExtension(extFileName) + "_bnd" + ExtForceFile.Extension;
                    var bndExtForceFilePath = System.IO.Path.Combine(exportDirectory, bndExtFileName);

                    if (BoundaryExternalForcingsFile == null)
                    {
                        BoundaryExternalForcingsFile = new BndExtForceFile();
                    }
                    BoundaryExternalForcingsFile.Write(bndExtForceFilePath, modelDefinition);
                }
                else if (!modelDefinition.BoundaryConditions.Any())
                {
                    modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueAsString("");
                }
            }

            modelDefinition.SetMduTimePropertiesFromGuiProperties();
            // write at the end in case of updated file paths
            WriteProperties(targetMduFilePath, modelDefinition.Properties, writeExtForcings, writeFeatures, useNetCDFMapFormat:useNetCDFMapFormat, disableFlowNodeRenumbering:disableFlowNodeRenumbering);

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
            WriteMorphologySediment(filePath, modelDefinition);

            OpenOutputFile(filePath);
            try
            {
                WriteLine("# Generated on " + DateTime.Now);
                WriteLine("# Deltares, FM-Suite DFlowFM Model Version " + FMSuiteFlowModelVersion + ", DFlow FM Version " + FMDllVersion);
                modelDefinition.First(p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Version, StringComparison.InvariantCultureIgnoreCase)).Value = FMDllVersion;
                modelDefinition.First(p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.GuiVersion, StringComparison.InvariantCultureIgnoreCase)).Value = FMSuiteFlowModelVersion;
                var propertiesByGroup = modelDefinition.Where(p => p.PropertyDefinition.FileCategoryName != "GUIOnly"
                                                                    && p.PropertyDefinition.FileCategoryName != MorphologyFile.MorphologyUnknownProperty /*Remove morphology unknown properties*/
                                                                    && p.PropertyDefinition.FileCategoryName != SedimentFile.SedimentUnknownProperty)/*Remove sediment unknown properties that should be located on the sediment file*/
                                                        .GroupBy(p => p.PropertyDefinition.FileCategoryName);

                propertiesByGroup = RemoveMorAndSedPropertiesIfNeeded(propertiesByGroup, modelDefinition, writeExtForcings, writeFeatures);
                
                foreach (var propertyGroup in propertiesByGroup)
                {
                    WriteLine("");
                    WriteLine("[" + propertyGroup.Key + "]");
                    foreach (var prop in propertyGroup)
                    {
                        if (!writePartionFile && prop.PropertyDefinition.MduPropertyName.Equals("PartitionFile"))
                            continue;

                        if (useNetCDFMapFormat && prop.PropertyDefinition.MduPropertyName.Equals("MapFormat"))
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

                            var mduLine = String.Format("{0,-18}= {1,-20}{2}", prop.PropertyDefinition.MduPropertyName,
                                mduPropertyValue,
                                mduComments.ContainsKey(prop.PropertyDefinition.MduPropertyName)
                                    ? mduComments[prop.PropertyDefinition.MduPropertyName]
                                    : "");
                            WriteLine(mduLine.Trim());
                        }
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private static IEnumerable<IGrouping<string, WaterFlowFMProperty>> RemoveMorAndSedPropertiesIfNeeded(IEnumerable<IGrouping<string, WaterFlowFMProperty>> propertiesByGroup, IEnumerable<WaterFlowFMProperty> modelDefinition, bool writeExtForcings, bool writeFeatures)
        {
            /* Not include Morphology / Sediment MDUs if UseMorSed has not been selected */

            propertiesByGroup = propertiesByGroup.Where(p => !p.Key.Equals(KnownProperties.morphology));
            var useMorSedProp = modelDefinition.FirstOrDefault(md => md.PropertyDefinition.MduPropertyName == "UseMorSed");
            if (useMorSedProp != null)
            {
                int useMorSed;
                if ( int.TryParse(GetPropertyValue(useMorSedProp, writeExtForcings, writeFeatures), out useMorSed) && useMorSed != 1)
                {
                    propertiesByGroup = propertiesByGroup.Where(p => !p.Key.Equals(KnownProperties.sediment));
                }
            }
            return propertiesByGroup;
        }

        

        private void WriteMorphologySediment(string mduFilePath, IEnumerable<WaterFlowFMProperty> modelDefinition)
        {
            var morFilePath = ReplaceMduExtension(mduFilePath, MorphologyExtension);
            var sedFilePath = ReplaceMduExtension(mduFilePath, SedimentExtension);
            
            modelDefinition.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MorFile)).Value = System.IO.Path.GetFileName(morFilePath);
            modelDefinition.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile)).Value = System.IO.Path.GetFileName(sedFilePath);
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
                 propertyName == KnownProperties.ThinDamFile || propertyName == KnownProperties.FixedWeirFile ||
                 propertyName == KnownProperties.ManholeFile || propertyName == KnownProperties.ObsFile ||
                 propertyName == KnownProperties.ObsCrsFile))
            {
                return string.Empty;
            }
            return prop.GetValueAsString();
        }

        private static void WriteFeatures<TFeat, TFile>(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            string propertyKey,IList<TFeat> features, ref TFile fileWriter, string extension)
            where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var waterFlowFMProperty = modelDefinition.GetModelProperty(propertyKey);
            if (features.Any())
            {
                string filePath;
                if (fileWriter == null || string.IsNullOrEmpty(waterFlowFMProperty.GetValueAsString()))
                {
                    filePath = ReplaceMduExtension(targetMduFilePath, extension);
                    waterFlowFMProperty.Value = System.IO.Path.GetFileName(filePath);
                }
                else
                {
                    filePath = MduFileHelper.GetSubfilePath(targetMduFilePath, waterFlowFMProperty);
                }
                var path = filePath;

                if (fileWriter == null)
                {
                    fileWriter = CreateFeatureFile<TFeat, TFile>(modelDefinition);
                }
                fileWriter.Write(path, features);
            }
            else
            {
                waterFlowFMProperty.Value = "";
            }
        }

        private void WriteDryPoints(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IList<PointFeature> dryPoints)
        {
            var waterFlowFMProperty = modelDefinition.GetModelProperty(KnownProperties.DryPointsFile);
            if (dryPoints.Any())
            {
                string filePath;
                if (dryPointFile == null || string.IsNullOrEmpty(waterFlowFMProperty.GetValueAsString()) ||
                    !waterFlowFMProperty.GetValueAsString().EndsWith(DryPointExtension))
                {
                    filePath = ReplaceMduExtension(targetMduFilePath, DryPointExtension);
                    waterFlowFMProperty.Value = System.IO.Path.GetFileName(filePath);
                }
                else
                {
                    filePath = MduFileHelper.GetSubfilePath(targetMduFilePath, waterFlowFMProperty);
                }
                var path = filePath;
                if (dryPointFile == null)
                {
                    dryPointFile = new XyzFile();
                }
                dryPointFile.Write(path, dryPoints.Select(p => new PointValue {X = p.X, Y = p.Y, Value = 0}));
            }
            else
            {
                waterFlowFMProperty.Value = "";
            }
        }

        private static TFile CreateFeatureFile<TFeat, TFile>(WaterFlowFMModelDefinition modelDefinition)
            where TFile : IFeature2DFileBase<TFeat>, new()
        {
            var fileWriter = new TFile();
            var fixedWeirFile = fileWriter as PliFile<FixedWeir>;
            if (fixedWeirFile != null)
            {
                fixedWeirFile.CreateDelegate = delegate(List<Coordinate> points, string name)
                {
                    var feature = new FixedWeir {Name = name, Geometry = PliFile<FixedWeir>.CreatePolyLine(points)};
                    feature.InitializeAttributes();
                    return feature;
                };
            }

            var structuresFileWriter = fileWriter as StructuresFile;
            if (structuresFileWriter != null)
            {
                structuresFileWriter.StructureSchema = modelDefinition.StructureSchema;
                structuresFileWriter.ReferenceDate =
                    (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            }
            return fileWriter;
        }

        private void WriteAreaFeatures(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea)
        {
            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.LandBoundaryFile,
                hydroArea.LandBoundaries, ref landBoundariesFile, LandBoundariesExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams.ToList(),
                ref thinDamFile, ThinDamExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.FixedWeirFile, hydroArea.FixedWeirs.ToList(),
                ref fixedWeirFile, FixedWeirExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ObsFile, hydroArea.ObservationPoints,
                ref obsFile, ObsExtension);

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.ObsCrsFile, hydroArea.ObservationCrossSections.ToList(),
                ref obsCrsFile, ObsCrossExtension);

            var structures = hydroArea.Pumps.Cast<IStructure>().Concat(hydroArea.Weirs).Concat(hydroArea.Gates).ToList();

            WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.StructuresFile, structures,
                ref structuresFile, StructuresExtension);

            if (hydroArea.DryAreas.Any())
            {
                WriteFeatures(targetMduFilePath, modelDefinition, KnownProperties.DryPointsFile, hydroArea.DryAreas,
                ref dryAreaFile, DryAreaExtension);

                if (hydroArea.DryPoints.Any())
                {
                    Log.WarnFormat("Cannot serialize both dry points and dry polygons to mdu. Discarded dry points");
                }
            }
            else
            {
                WriteDryPoints(targetMduFilePath, modelDefinition, hydroArea.DryPoints);
            }
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

        #endregion

        #region read logic

        public void Read(string filePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea, Action<string,int,int> reportProgress = null)
        {
            if (reportProgress == null) reportProgress = (name, current, total) => { };
            var totalSteps = 5;

            reportProgress("Reading properties", 1, totalSteps);
            ReadProperties(filePath, modelDefinition);

            reportProgress("Reading morphology properties", 2, totalSteps);
            ReadMorphologyFile(filePath, modelDefinition);

            reportProgress("Reading area features", 3, totalSteps);
            ReadAreaFeatures(filePath, modelDefinition, hydroArea);

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
                    BoundaryExternalForcingsFile.Read(forceFilePath, modelDefinition);
                }
            }

            hydroArea.Embankments.AddRange(modelDefinition.Embankments);
        }

        private void ReadMorphologyProperties(string mduFilePath, string propertyKey, WaterFlowFMModelDefinition definition)
        {
            var filePath = MduFileHelper.GetSubfilePath(mduFilePath, definition.GetModelProperty(propertyKey));
            if (!File.Exists(filePath)) return;

            var propertiesCategories = new SedMorDelftIniReader().ReadDelftIniFile(filePath);
            foreach (var category in propertiesCategories)
            {
                var currentGroupName = category.Name;
                if(currentGroupName == MorphologyFile.GeneralHeader) continue; // don't store MorphologyFileInformation in model definition
                foreach (var readProp in category.Properties)
                {
                    if (!definition.ContainsProperty(readProp.Name))
                    {
                        // create definition for unknown property:
                        var propDef = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(MorphologyFile.MorphologyUnknownProperty,
                                readProp.Name, readProp.Comment);
                        propDef.Category = currentGroupName;
                        var newProp = new WaterFlowFMProperty(propDef, readProp.Value);
                        /*  We set the value now to avoid catching a 'used custom value' in the SedimentFile, or elsewhere */
                        if (!string.IsNullOrEmpty(readProp.Value))
                            newProp.SetValueAsString(readProp.Value);
                        definition.AddProperty(newProp);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(readProp.Value))
                    {
                        definition.GetModelProperty(readProp.Name).SetValueAsString(readProp.Value);
                    }
                }
            }
        }

        private void ReadMorphologyFile(string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            if ( ! modelDefinition.GetModelProperty(KnownProperties.MorFile).Value.Equals(string.Empty) )
            {
                ReadMorphologyProperties(mduFilePath, KnownProperties.MorFile, modelDefinition);
                modelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            }
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
                        currentGroupName = line.Substring(1, endIndex-1).Trim();
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
                        string[] fields = line.Split(new[] {'=', '#'});
                        string mduPropertyName = fields[0].Trim();

                        string mduPropertyLowerCase = mduPropertyName.ToLower();

                        // some backwards compatibility issues (properties have been renamed
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
                        if (fields.Length > 2)
                        {
                            int commentStart = line.IndexOf('#');
                            if (commentStart > 0)
                            {
                                mduComments[mduPropertyName] = line.Substring(commentStart);
                            }
                        }

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
                            definition.GetModelProperty(mduPropertyLowerCase).SetValueAsString(mduPropertyValue);
                        }
                    }
                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }

            definition.GetModelProperty(KnownProperties.RefDate).Value =
                FMParser.ParseFMDateTime(definition.GetModelProperty(KnownProperties.RefDate).GetValueAsString());

            definition.SetGuiTimePropertiesFromMduProperties();

            // update the heat flux model in the definition, because the event of KnownProperties.Temperature is not bubbled during loading of an mdu file.
            definition.UpdateHeatFluxModel();
        }

        private static void ReadFeatures<TFeat, TFile>(string mduFilePath, WaterFlowFMModelDefinition modelDefinition,
            string propertyKey, IList<TFeat> features, ref TFile fileReader) where TFile: IFeature2DFileBase<TFeat>, new()
        {
            var featuresFilePath = MduFileHelper.GetSubfilePath(mduFilePath, modelDefinition.GetModelProperty(propertyKey));

            if (featuresFilePath == null) return;

            fileReader = CreateFeatureFile<TFeat, TFile>(modelDefinition);

            var readFeatures = fileReader.Read(featuresFilePath);

            NamingHelper.MakeNamesUnique(readFeatures.Cast<INameable>().ToList());

            features.AddRange(readFeatures);
        }

        private void ReadDryPoints(HydroArea hydroArea, string dryPointsFilePath)
        {
            if (dryPointsFilePath == null || !dryPointsFilePath.EndsWith(XyzFile.Extension)) return;

            dryPointFile = new XyzFile();
            var points = dryPointFile.Read(dryPointsFilePath);
            hydroArea.DryPoints.Clear();
            hydroArea.DryPoints.AddRange(points.Select(p => new PointFeature() { Geometry = p.Geometry }));
        }

        private void ReadAreaFeatures(string filePath, WaterFlowFMModelDefinition modelDefinition, HydroArea hydroArea)
        {
            ReadFeatures(filePath, modelDefinition, KnownProperties.LandBoundaryFile, hydroArea.LandBoundaries,
                ref landBoundariesFile);

            ReadFeatures(filePath, modelDefinition, KnownProperties.ThinDamFile, hydroArea.ThinDams, ref thinDamFile);
            ReadFeatures(filePath, modelDefinition, KnownProperties.FixedWeirFile, hydroArea.FixedWeirs, ref fixedWeirFile);
            ReadFeatures(filePath, modelDefinition, KnownProperties.ObsFile, hydroArea.ObservationPoints, ref obsFile);
            ReadFeatures(filePath, modelDefinition, KnownProperties.ObsCrsFile, hydroArea.ObservationCrossSections, ref obsCrsFile);

            var structures = new List<IStructure>();

            ReadFeatures(filePath, modelDefinition, KnownProperties.StructuresFile, structures, ref structuresFile);

            foreach (var structure in structures)
            {
                if (structure is IPump)
                {
                    hydroArea.Pumps.Add((IPump) structure);
                }
                else if (structure is IWeir)
                {
                    hydroArea.Weirs.Add((IWeir) structure);
                }
                else if (structure is IGate)
                {
                    hydroArea.Gates.Add((IGate) structure);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            var dryPointsFilePath = MduFileHelper.GetSubfilePath(filePath,
                modelDefinition.GetModelProperty(KnownProperties.DryPointsFile));

            if (dryPointsFilePath != null && dryPointsFilePath.EndsWith(PolFile.Extension))
            {
                ReadFeatures(filePath, modelDefinition, KnownProperties.DryPointsFile, hydroArea.DryAreas, ref dryAreaFile);
            }
            else
            {
                ReadDryPoints(hydroArea, dryPointsFilePath);                
            }
        }

        #endregion
    }
}
