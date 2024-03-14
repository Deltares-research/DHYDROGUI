using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class BndExtForceFile : FMSuiteFileBase
    {
        private readonly PolFile<GroupableFeature2DPolygon> roofPolFile = new PolFile<GroupableFeature2DPolygon> {IncludeClosingCoordinate = true};
        public const string MeteoBlockKey = "[meteo]";
        public const string LocationTypeKey = "locationType";

        public const string BoundaryBlockKey = "[Boundary]";
        public const string LateralBlockKey = "[Lateral]";
        public const string BoundaryHeaderKey = "Boundary";
        public const string LateralHeaderKey = "Lateral";
        public const string QuantityKey = "quantity";
        public const string BranchIdKey = "branchId";
        public const string IdKey = "id";
        public const string NameKey = "name";
        public const string TypeKey = "type";
        public const string ChainageKey = "chainage";
        public const string NodeIdKey = "nodeId";
        public const string DischargeKey = "discharge";
        
        public const string LocationFileKey = "locationfile";
        public const string ForcingFileKey = "forcingfile";
        public const string ForcingFileTypeKey = "forcingFileType";
        private const string TargetMaskFileKey = "targetMaskFile";
        private const string TargetMaskInvertKey = "targetMaskInvert";
        private const string InterpolationMethodKey = "interpolationMethod";
        private const string AreaKey = "area";
        private const string ThatcherHarlemanTimeLagKey = "return_time";
        private const string OpenBoundaryToleranceKey = "OpenBoundaryTolerance";
        public static double OpenBoundaryTolerance = 0.5; // made public static while this value still needs to be tweaked *run away run away*
        private const string BoundaryWidth = "bndWidth1D";
        private const string BoundaryDepth = "bndBLDepth";

        private static IniSection CreateBoundaryBlock(string quantity, string locationFilePath, string nodeid, string forcingFilePath, TimeSpan thatcherHarlemanTimeLag, bool isOnOutletCompartment = false, bool isEmbankment = false, LateralSourceForcingDefinition lateralSourceForcingDefinition = null, 
                                                            double? boundaryWidth = null, double? boundaryDepth = null)
        {
            var block = new IniSection(BoundaryBlockKey);
            if (quantity != null)
            {
                block.AddPropertyWithOptionalComment(QuantityKey, quantity);
            }
            if (locationFilePath != null)
            {
                block.AddPropertyWithOptionalComment(LocationFileKey, locationFilePath);
            }

            if (nodeid != null)
            {
                block.AddPropertyWithOptionalComment(NodeIdKey, nodeid);
            }
            if (forcingFilePath != null)
            {
                block.AddPropertyWithOptionalComment(ForcingFileKey, forcingFilePath);
            }
            if (thatcherHarlemanTimeLag != TimeSpan.Zero)
            {
                block.AddPropertyWithOptionalCommentAndFormat(ThatcherHarlemanTimeLagKey, thatcherHarlemanTimeLag.TotalSeconds);
            }
            if (isEmbankment)
            {
                block.AddPropertyWithOptionalCommentAndFormat(OpenBoundaryToleranceKey, OpenBoundaryTolerance);
            }

            if (isOnOutletCompartment)
            {
                block.AddPropertyWithOptionalComment("isOnOutletCompartment", "true");
            }

            if (isOnOutletCompartment && boundaryWidth != null && boundaryDepth != null)
            {
                block.AddPropertyWithOptionalCommentAndFormat(BoundaryWidth, (double) boundaryWidth);
                block.AddPropertyWithOptionalCommentAndFormat(BoundaryDepth, (double) boundaryDepth);
            }

            if (lateralSourceForcingDefinition != null)
            {
                block.AddPropertyWithOptionalComment(IdKey, lateralSourceForcingDefinition.Name);
                block.AddPropertyWithOptionalComment(NameKey, lateralSourceForcingDefinition.LongName);
                if (lateralSourceForcingDefinition.Type != null)
                {
                    block.AddPropertyWithOptionalComment(TypeKey, lateralSourceForcingDefinition.Type);
                }
                if (lateralSourceForcingDefinition.NumCoordinates >= 3)
                {
                    //x,y,locationtype, numcors GEEEEEN IDEE WAAR JE DIT VANDAAN TOVERT
                }
                else if (!string.IsNullOrEmpty(lateralSourceForcingDefinition.BranchId))
                {
                    block.AddPropertyWithOptionalComment(BranchIdKey, lateralSourceForcingDefinition.BranchId);
                    block.AddPropertyWithOptionalCommentAndFormat(ChainageKey, lateralSourceForcingDefinition.Chainage);
                }else if (!string.IsNullOrEmpty(lateralSourceForcingDefinition.NodeId))
                {
                    block.AddPropertyWithOptionalComment(NodeIdKey, lateralSourceForcingDefinition.NodeId);
                }

                if (lateralSourceForcingDefinition.RealTime)
                {
                    block.AddPropertyWithOptionalComment(DischargeKey, "realtime");
                }
                else if (!string.IsNullOrEmpty(lateralSourceForcingDefinition.DischargeForcingFile))
                {
                    block.AddPropertyWithOptionalComment(DischargeKey, lateralSourceForcingDefinition.DischargeForcingFile);
                }
                else
                {
                    block.AddPropertyWithOptionalCommentAndFormat(DischargeKey, lateralSourceForcingDefinition.Discharge);
                }
            }
            return block;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (BndExtForceFile));

        private const BcFile.WriteMode BcFileWriteMode = BcFile.WriteMode.FilePerQuantity;
        private const BcFile.WriteMode BcmFileWriteMode = BcFile.WriteMode.SingleFile; 

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolylineFiles; 
        private readonly IDictionary<IBoundaryCondition, IniSection> existingBndForceFileItems;
        private string filePath;

        public bool WriteToDisk { get; set; }

        private string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(FilePath), relativePath);
        }

        public BndExtForceFile()
        {
            existingPolylineFiles = new Dictionary<Feature2D, string>();
            existingBndForceFileItems = new Dictionary<IBoundaryCondition, IniSection>();
            WriteToDisk = true;
        }

        #region write logic

        public void Write(string filePath, WaterFlowFMModelDefinition modelDefinition, IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D = null, IEnumerable<Model1DLateralSourceData> lateralSourcesData = null, ICollection<GroupableFeature2DPolygon> roofAreas = null)
        {
            var refDate = modelDefinition.GetReferenceDateAsDateTime();

            Write(filePath, modelDefinition.ModelName, modelDefinition.BoundaryConditionSets, boundaryConditions1D, lateralSourcesData, roofAreas?? new GroupableFeature2DPolygon[0] , modelDefinition.Embankments, modelDefinition.FmMeteoFields,
                modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile), refDate);
        }

        private void Write(string filePath, string modelDefinitionModelName,
            IList<BoundaryConditionSet> boundaryConditionSets,
            IEnumerable<Model1DBoundaryNodeData> modelDefinitionBoundaryConditions1D,
            IEnumerable<Model1DLateralSourceData> modelDefinitionLateralSourcesData, ICollection<GroupableFeature2DPolygon> roofAreas, IList<Embankment> embankments,
            IEventedList<IFmMeteoField> fmMeteoFields, WaterFlowFMProperty modelProperty, DateTime refDate)
        {
            FilePath = filePath;
            var bndExtForceFileItems = WriteBndExtForceFileSubFiles(modelDefinitionModelName, boundaryConditionSets, refDate);
            var bnd1DExtForceFileItems = Write1DBndExtForceFileSubFiles(modelDefinitionModelName, modelDefinitionBoundaryConditions1D, refDate).ToList();
            var lateralSourcesDataExtForceFileItems = WriteLateralSourcesDataExtForceFileSubFiles(modelDefinitionModelName, modelDefinitionLateralSourcesData, refDate).ToList();
            var embankmentForceFileItems = WriteEmbankmentFiles(embankments);
            var meteoExtForceFileItems = WriteMeteoExtForceFileSubFiles(modelDefinitionModelName, fmMeteoFields, refDate, roofAreas.Any());
            WriteRoofAreasPolFiles(modelDefinitionModelName, roofAreas);

            var allItems = bndExtForceFileItems.Concat(embankmentForceFileItems).Concat(bnd1DExtForceFileItems).Concat(lateralSourcesDataExtForceFileItems).ToList();
            FileUtils.DeleteIfExists(FilePath);
            if (allItems.Any())
            {
                WriteBndExtForceFile(allItems);
            }
            if (meteoExtForceFileItems.Any())
            {
                WriteMeteoExtForceFile(meteoExtForceFileItems);
            }
            if (allItems.Any() || meteoExtForceFileItems.Any())
            {
                modelProperty.SetValueAsString(Path.GetFileName(FilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(FilePath);
                modelProperty.SetValueAsString(string.Empty);
            }
        }

        private IEnumerable<IniSection> WriteLateralSourcesDataExtForceFileSubFiles(string modelDefinitionModelName, IEnumerable<Model1DLateralSourceData> modelDefinitionLateralSourcesData, DateTime refDate)
        {
            if (modelDefinitionLateralSourcesData == null)
            {
                yield break;
            }

            var model1DLateralSourceDatas = modelDefinitionLateralSourcesData as Model1DLateralSourceData[] ?? modelDefinitionLateralSourcesData.ToArray();
            var lateralSourceDataLookup = model1DLateralSourceDatas
                                      .Where(d => d?.Feature != null)
                                      .ToDictionary(d => d.Feature.Name, StringComparer.InvariantCultureIgnoreCase);
            var generateModel1DLateralSourceDataBcSections = new Model1DBoundaryFileWriter().GenerateModel1DLateralSourceDataBcSections(refDate, model1DLateralSourceDatas, false, false, BoundaryRegion.BcForcingHeader);
            var filename = AddExtension(modelDefinitionModelName + "_lateral_sources", BcFile.Extension);

            // now generate bc files with the data
            var model1DLateralSourceDataBcSections = generateModel1DLateralSourceDataBcSections as BcIniSection[] ?? generateModel1DLateralSourceDataBcSections.ToArray();
            foreach (var model1DNodeBoundaryBcSection in model1DLateralSourceDataBcSections.OfType<BcIniSection>())
            {
                var lateralName = model1DNodeBoundaryBcSection.Section.GetPropertyValueWithOptionalDefaultValue(BoundaryRegion.Name.Key);
                if (!lateralSourceDataLookup.TryGetValue(lateralName, out var lateralData))
                {
                    continue;
                }
                
                var lateral = lateralData.Feature;
                if (lateral?.Branch == null)
                {
                    // we don't support 2d lateral sources types yet
                    continue;
                }

                var lateralDef = new LateralSourceForcingDefinition {Name = lateral.Name, LongName = lateral.LongName ?? lateral.Name};
                if (Math.Abs(lateral.Chainage) < double.Epsilon)
                {
                    lateralDef.NodeId = lateral.Branch.Source.Name;
                }
                else if (Math.Abs(lateral.Chainage - lateral.Branch.Length) < double.Epsilon)
                {
                    lateralDef.NodeId = lateral.Branch.Target.Name;
                }
                else
                {
                    lateralDef.BranchId = lateral.Branch.Name;
                    lateralDef.Chainage = lateral.Chainage;
                }

                lateralDef.RealTime = lateralData.DataType == Model1DLateralDataType.FlowRealTime;
                if ((lateralData.DataType == Model1DLateralDataType.FlowRealTime || lateralData.DataType == Model1DLateralDataType.FlowConstant) && lateralData.Compartment is ICompartment lateralCompartment)
                {
                    lateralDef.NodeId = lateralCompartment.Name;
                }

                lateralDef.DischargeForcingFile = filename;
                yield return CreateBoundaryBlock(null, null, null, null, TimeSpan.Zero, lateralSourceForcingDefinition:lateralDef);
            }
            var bcFile = new BcFile() { MultiFileMode = BcFile.WriteMode.SingleFile }; //single file want ff niet anders
            bcFile.Write(model1DLateralSourceDataBcSections, filename, Path.GetDirectoryName(FilePath));
        }

        private void WriteMeteoExtForceFile(IEnumerable<IniSection> meteoExtForceFileItems)
        {
            var generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(
                GeneralRegion.BoundaryConditionsExternalForcingMajorVersion, GeneralRegion.BoundaryConditionsExternalForcingMinorVersion,
                GeneralRegion.FileTypeName.BoundaryConditionExternalForcing);

            if (!File.Exists(FilePath) || ReadIniFile(FilePath).Sections.All(s => !s.IsNameEqualTo(GeneralRegion.IniHeader)))
            {
                WriteIniFile(FilePath, new[] { generalRegion });
            }
            OpenOutputFile(FilePath, true);
            try
            {
                foreach (var bndExtForceFileItem in meteoExtForceFileItems)
                {
                    WriteLine("");
                    WriteLine(MeteoBlockKey);

                    foreach (IniProperty property in bndExtForceFileItem.Properties)
                    {
                        WriteLine(property.Key + "=" + property.Value);
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteBndExtForceFile(IEnumerable<IniSection> bndExtForceFileItems)
        {
            var generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(
                GeneralRegion.BoundaryConditionsExternalForcingMajorVersion, GeneralRegion.BoundaryConditionsExternalForcingMinorVersion,
                GeneralRegion.FileTypeName.BoundaryConditionExternalForcing);
            WriteIniFile(FilePath, new[] { generalRegion });
            OpenOutputFile(FilePath,true);
            try
            {
                foreach (var bndExtForceFileItem in bndExtForceFileItems)
                {
                    WriteLine("");
                    string quantity = bndExtForceFileItem.GetPropertyValueWithOptionalDefaultValue(QuantityKey);
                    if (quantity != null)
                    {
                        WriteLine(BoundaryBlockKey);
                        WriteLine(QuantityKey + "=" + quantity);
                        WriteProperty(bndExtForceFileItem, LocationFileKey);
                        WriteProperty(bndExtForceFileItem, NodeIdKey);
                        string openBoundaryTolerance = bndExtForceFileItem.GetPropertyValuesByName(OpenBoundaryToleranceKey)
                            .FirstOrDefault();
                        if (openBoundaryTolerance != null)
                        {
                            WriteLine(OpenBoundaryToleranceKey + "=" + openBoundaryTolerance);
                        }

                        foreach (var propertyValue in bndExtForceFileItem.GetPropertyValuesByName(ForcingFileKey))
                        {
                            WriteLine(ForcingFileKey + "=" + propertyValue);
                        }

                        WriteProperty(bndExtForceFileItem, ThatcherHarlemanTimeLagKey);
                        WriteProperty(bndExtForceFileItem, AreaKey);
                        WriteProperty(bndExtForceFileItem, "isOnOutletCompartment");
                        WriteProperty(bndExtForceFileItem, BoundaryWidth);
                        WriteProperty(bndExtForceFileItem, BoundaryDepth);
                    }

                    string id = bndExtForceFileItem.GetPropertyValueWithOptionalDefaultValue(IdKey);
                    if (id != null)
                    {
                        WriteLine(LateralBlockKey);
                        WriteLine(IdKey + "=" + id);
                        WriteLine(NameKey + "=" + bndExtForceFileItem.GetPropertyValueWithOptionalDefaultValue(NameKey));
                        var branchId = bndExtForceFileItem.GetPropertyValueWithOptionalDefaultValue(BranchIdKey);
                        if (branchId != null)
                        {
                            WriteLine(BranchIdKey + "=" + branchId);
                            WriteLine(ChainageKey + "=" + bndExtForceFileItem.GetPropertyValueWithOptionalDefaultValue(ChainageKey));
                        }
                        
                        WriteProperty(bndExtForceFileItem, NodeIdKey);
                        WriteProperty(bndExtForceFileItem, TypeKey);
                        WriteProperty(bndExtForceFileItem, LocationTypeKey);
                        WriteProperty(bndExtForceFileItem, DischargeKey);
                    }
                }
            }
            finally

            {
                CloseOutputFile();
            }
        }

        private void WriteProperty(IniSection iniSection, string key)
        {
            string value = iniSection.GetPropertyValueWithOptionalDefaultValue(key);
            if (value != null)
            {
                WriteLine(key + "=" + value);
            }
        }

        public IList<IniSection> WriteBndExtForceFileSubFiles(string modelDefinitionModelName, IList<BoundaryConditionSet> boundaryConditionSets, DateTime refDate)
        {
            WritePolyLines(boundaryConditionSets);

            var resultingItems =
                boundaryConditionSets.Where(bcs => !bcs.BoundaryConditions.Any())
                    .Select(boundaryConditionSet =>
                    {
                        string pliFileName;
                        return existingPolylineFiles.TryGetValue(boundaryConditionSet.Feature, out pliFileName) ? CreateBoundaryBlock(null, pliFileName, null, null, TimeSpan.Zero) : null;
                    }).Where( it => it != null)
                    .ToList();

            /* Write all morphology boundaries in one file.*/
            var bcmFile = new BcmFile { MultiFileMode = BcmFileWriteMode };
            var morphologyGroupings = bcmFile.GroupBoundaryConditions(boundaryConditionSets);
            
            WriteBoundaryConditions(refDate, bcmFile, morphologyGroupings, new BcmFileFlowBoundaryDataBuilder(), modelDefinitionModelName);
            /* No longer return the morphology groupings since they will not be written to the .ext file (DELFT3DFM-1106) */
            
            var bcFile = new BcFile { MultiFileMode = BcFileWriteMode };
            var standardGroupings = bcFile.GroupBoundaryConditions(boundaryConditionSets);
            resultingItems.AddRange(WriteBoundaryConditions(refDate, bcFile, standardGroupings, new BcFileFlowBoundaryDataBuilder(), modelDefinitionModelName).Distinct());

            return resultingItems;
        }
        public IEnumerable<IniSection> Write1DBndExtForceFileSubFiles(string modelDefinitionModelName, IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D, DateTime refDate)
        {
            var generateModel1DNodeBoundaryBcSections = new Model1DBoundaryFileWriter().GenerateModel1DNodeBoundaryBcSections(refDate, boundaryConditions1D, false, false, BoundaryRegion.BcForcingHeader);
            var filename = AddExtension(modelDefinitionModelName+"_boundaryconditions1d", BcFile.Extension);
            // now generate bc files with the data
            var model1DNodeBoundaryBcSections = generateModel1DNodeBoundaryBcSections as BcIniSection[] ?? generateModel1DNodeBoundaryBcSections.ToArray();
            foreach (var generateModel1DNodeBoundaryIniSection in model1DNodeBoundaryBcSections.OfType<BcIniSection>())
            {
                var quantityName = string.Empty;
                var function = generateModel1DNodeBoundaryIniSection.Section.GetPropertyValueWithOptionalDefaultValue(BoundaryRegion.Function.Key);
                if (function == BoundaryRegion.FunctionStrings.QhTable)
                    quantityName = BoundaryRegion.QuantityStrings.QHDischargeWaterLevelDependency;
                else
                {
                    var bcQuantityData = generateModel1DNodeBoundaryIniSection.Table.LastOrDefault();
                    quantityName = bcQuantityData?.Quantity?.Value;
                }
                var nodeId = generateModel1DNodeBoundaryIniSection.Section.GetPropertyValueWithOptionalDefaultValue(BoundaryRegion.Name.Key);
                var manHoleName = generateModel1DNodeBoundaryIniSection.Section.ReadProperty<string>("manHoleName", true);

                var m1dbnd = boundaryConditions1D.FirstOrDefault(bc => bc.Feature.Name.EqualsCaseInsensitive(manHoleName ?? nodeId));

                if (m1dbnd?.OutletCompartment != null)
                {
                    nodeId = m1dbnd.OutletCompartment.Name;
                }
                var thatcherHarlemanTimeLag =  m1dbnd != null && m1dbnd.UseSalt ? new TimeSpan(0,0, (int)m1dbnd.ThatcherHarlemannCoefficient) : TimeSpan.Zero;
                yield return CreateBoundaryBlock(quantityName, null, nodeId, filename, thatcherHarlemanTimeLag, isOnOutletCompartment: m1dbnd?.OutletCompartment != null, 
                                                 boundaryWidth: m1dbnd?.BoundaryWidth, boundaryDepth: m1dbnd?.BoundaryDepth);
            }
            var bcFile = new BcFile() { MultiFileMode = BcFile.WriteMode.SingleFile };//single file want ff niet anders
            bcFile.Write(model1DNodeBoundaryBcSections, filename, Path.GetDirectoryName(FilePath));
        }

        private IList<IniSection> WriteEmbankmentFiles(IList<Embankment> embankments)
        {
            var iniSections = new List<IniSection>();

            foreach (var embankment in embankments)
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue(embankment, out existingFile))
                {
                    existingFile = embankment.Name + "_bnk.pliz"; 
                    existingPolylineFiles[embankment] = existingFile;
                }
                if (WriteToDisk)
                {
                    new PlizFile<Embankment>().Write(GetFullPath(existingFile), new[] { embankment });
                }

                iniSections.Add(CreateBoundaryBlock(ExtForceQuantNames.EmbankmentBnd, existingFile, null, ExtForceQuantNames.EmbankmentForcingFile, TimeSpan.Zero, true));
            }

            return iniSections;
        }

        private IEnumerable<IniSection> WriteMeteoExtForceFileSubFiles(string modelDefinitionModelName, IList<IFmMeteoField> fmMeteoFields, DateTime refDate, bool hasRoofs)
        {
            WritePolyLinesMeteo(fmMeteoFields);
            var bcFile = new BcFile { MultiFileMode = BcFile.WriteMode.SingleFile };
            
            string bcFileName = AddExtension(modelDefinitionModelName + "_meteo", BcFile.Extension);
            
            if (WriteToDisk)
            {
                string bcFilePath = GetFullPath(bcFileName);
                bcFile.Write(fmMeteoFields, bcFilePath, new BcMeteoFileDataBuilder(), refDate);
            }

            return CreateMeteoIniSections(fmMeteoFields, bcFileName, hasRoofs, modelDefinitionModelName);
        }

        private static IEnumerable<IniSection> CreateMeteoIniSections(IList<IFmMeteoField> fmMeteoFields, string bcFileName, bool hasRoofs, string modelName)
        {
            IniSection roofIniSection = null;
            if (hasRoofs)
            {
                roofIniSection = CreateRoofIniSection(modelName);
                
                if (!fmMeteoFields.Any())
                {
                    yield return roofIniSection;
                }
            }
            
            foreach (IFmMeteoField fmMeteoField in fmMeteoFields)
            {
                string quantityName = ExtForceQuantNames.MeteoQuantityNames[fmMeteoField.Quantity];
                yield return CreateMeteoIniSection(quantityName, bcFileName, roofIniSection);
            }
        }

        private static IniSection CreateMeteoIniSection(string quantity,
                                                            string forcingFilePath, IniSection roofIniSection)
        {
            var iniSection = new IniSection(MeteoBlockKey);
            iniSection.AddPropertyWithOptionalComment(QuantityKey, quantity);
            iniSection.AddPropertyWithOptionalComment(ForcingFileKey, forcingFilePath);
            iniSection.AddPropertyWithOptionalComment(ForcingFileTypeKey, "bcAscii");

            if (roofIniSection != null)
            {
                iniSection.AddMultipleProperties(roofIniSection.Properties);
            }
            
            return iniSection;
        }

        private static IniSection CreateRoofIniSection(string modelName)
        {
            var iniSection = new IniSection(MeteoBlockKey);
            iniSection.AddPropertyWithOptionalComment(TargetMaskFileKey, modelName + FileConstants.RoofAreaFileExtension);
            iniSection.AddPropertyWithOptionalComment(TargetMaskInvertKey, "true");
            iniSection.AddPropertyWithOptionalComment(InterpolationMethodKey, "nearestNb");

            return iniSection;
        }

        private void WriteRoofAreasPolFiles(string modelName, ICollection<GroupableFeature2DPolygon> roofAreas)
        {
            if (!roofAreas.Any())
            {
                return;
            }
            
            string roofFilePath = GetFullPath(modelName + FileConstants.RoofAreaFileExtension);
            roofPolFile.Write(roofFilePath, roofAreas);
        }

        private void WritePolyLines(IEnumerable<BoundaryConditionSet> boundaryConditionSets)
        {
            foreach (var boundaryConditionSet in boundaryConditionSets)
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue(boundaryConditionSet.Feature, out existingFile))
                {
                    existingFile = ExtForceFileHelper.GetPliFileName(boundaryConditionSet);
                    if (string.IsNullOrEmpty(existingFile)) return;
                    existingPolylineFiles[boundaryConditionSet.Feature] = existingFile;
                }
                if (WriteToDisk)
                {
                    new PliFile<Feature2D>().Write(GetFullPath(existingFile), new[] {boundaryConditionSet.Feature});
                }
            }
        }
        private void WritePolyLinesMeteo(IEnumerable<IFmMeteoField> fmMeteoFields)
        {
            foreach (var fmMeteoField in fmMeteoFields.Where(fmMeteoField => fmMeteoField.FeatureData?.Feature is Feature2D && fmMeteoField.FmMeteoLocationType == FmMeteoLocationType.Polygon))
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue((Feature2D)fmMeteoField.FeatureData.Feature, out existingFile))
                {
                    existingFile = ExtForceFileHelper.GetPliFileName(fmMeteoField.FeatureData);
                    if (string.IsNullOrEmpty(existingFile)) return;
                    existingPolylineFiles[(Feature2D)fmMeteoField.FeatureData.Feature] = existingFile;
                }
                if (WriteToDisk)
                {
                    new PliFile<Feature2D>().Write(GetFullPath(existingFile), new[] { (Feature2D)fmMeteoField.FeatureData.Feature });
                }
            }
        }

        static string AddExtension(string fileName, string extension)
        {
            var cleanFileName=fileName.TrimEnd(new[] {'.'});
            var cleanExtension = extension.TrimStart(new[] {'.'});
            return string.Concat(cleanFileName, ".", cleanExtension);
        }

        private IEnumerable<IniSection> WriteBoundaryConditions(DateTime refDate, BcFile bcFile, 
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> grouping, BcFileFlowBoundaryDataBuilder boundaryDataBuilder, string modelDefinitionName)
        {
            var resultingItems = new List<IniSection>();
            
            var fileNamesToBoundaryConditions =
                new Dictionary<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>>();
            
            foreach (var group in grouping)
            {
                foreach (var tuple in group.Where(t => t.Item1 is FlowBoundaryCondition))
                {
                    IniSection existingBlock;
                    existingBndForceFileItems.TryGetValue(tuple.Item1, out existingBlock);

                    var existingPaths = existingBlock != null
                        ? existingBlock.GetPropertyValuesByName(ForcingFileKey).ToList()
                        : new List<string>();

                    string fileName = group.Key;
                    if (string.IsNullOrEmpty(fileName) && bcFile.MultiFileMode == BcFile.WriteMode.SingleFile)
                    {
                        fileName = modelDefinitionName;
                    }
                    string path = existingPaths.Any() ? existingPaths.First() : AddExtension(fileName, bcFile is BcmFile ? BcmFile.Extension : BcFile.Extension);

                    if (existingBlock != null && !existingPaths.Contains(path))
                    {
                        existingBlock.AddPropertyWithOptionalComment(ForcingFileKey, path);
                    }

                    var corrPath = existingPaths.Count > 1
                            ? existingPaths[1]
                            : AddExtension(fileName + "_corr", BcFile.Extension);

                    if (existingBlock != null)
                    {
                        // set thatcher harlemann time lag once it is already existent in the ext force file but it has changed.
                        var condition = (FlowBoundaryCondition) tuple.Item1;
                        existingBlock.SetPropertyWithOptionalCommentAndFormat(ThatcherHarlemanTimeLagKey, condition.ThatcherHarlemanTimeLag.TotalSeconds);

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType) && !existingPaths.Contains(corrPath))
                        {
                            existingBlock.AddPropertyWithOptionalComment(ForcingFileKey, corrPath);
                        }
                        if (!BcFile.IsCorrectionType(tuple.Item1.DataType) && existingPaths.Contains(corrPath))
                        {
                            existingBlock.RemoveAllProperties(p => p.Value == corrPath);
                        }
                    }

                    IList<Tuple<IBoundaryCondition, BoundaryConditionSet>> tuples;
                    
                    if (fileNamesToBoundaryConditions.TryGetValue(path, out tuples))
                    {
                        tuples.Add(tuple);
                    }
                    else
                    {
                        tuples = new List<Tuple<IBoundaryCondition, BoundaryConditionSet>> {tuple};
                        fileNamesToBoundaryConditions.Add(path, tuples);
                    }
                    if (BcFile.IsCorrectionType(tuple.Item1.DataType))
                    {
                        if (fileNamesToBoundaryConditions.TryGetValue(corrPath, out tuples))
                        {
                            tuples.Add(tuple);
                        }
                        else
                        {
                            tuples = new List<Tuple<IBoundaryCondition, BoundaryConditionSet>> { tuple };
                            fileNamesToBoundaryConditions.Add(corrPath, tuples);
                        }
                    }

                    if (existingBlock == null)
                    {
                        var quantityName =
                            ExtForceQuantNames.GetQuantityString((FlowBoundaryCondition) tuple.Item1);

                        var pliFileName = existingPolylineFiles[tuple.Item2.Feature];

                        var bndBlock = CreateBoundaryBlock(quantityName, pliFileName, null, path, ((FlowBoundaryCondition)tuple.Item1).ThatcherHarlemanTimeLag);

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType))
                        {
                            bndBlock.AddPropertyWithOptionalComment(ForcingFileKey, corrPath);
                        }

                        resultingItems.Add(bndBlock);
                    }
                    else
                    {
                        resultingItems.Add(existingBlock);
                    }
                }
            }

            if (WriteToDisk)
            {
                foreach (var fileNamesToBoundaryCondition in fileNamesToBoundaryConditions)
                {
                    var fullPath = GetFullPath(fileNamesToBoundaryCondition.Key);

                    bcFile.CorrectionFile = fullPath.EndsWith("_corr.bc");

                    bcFile.Write(fileNamesToBoundaryCondition.Value.ToDictionary(t => t.Item1, t => t.Item2),
                        fullPath, boundaryDataBuilder, refDate);

                    bcFile.CorrectionFile = false;
                }
            }

            return resultingItems;
        }

        private static void WriteIniFile(string targetFile, IEnumerable<IniSection> iniSections)
        {
            var iniFormatter = new IniFormatter { Configuration = { WriteComments = false } };

            var iniData = new IniData();
            iniData.AddMultipleSections(iniSections);

            Log.InfoFormat(Resources.BndExtForceFile_WriteIniFile_Writing_external_forcings_to__0__, targetFile);
            using (Stream iniStream = File.Open(targetFile, FileMode.Create))
            {
                iniFormatter.Format(iniData, iniStream);
            }
        }
        #endregion

        #region read logic

        public void Read(string bndExtForceFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, HydroArea area = null, IEventedList<Model1DBoundaryNodeData> boundaryConditions1D=null, IEventedList<Model1DLateralSourceData> lateralSourcesData=null)
        {
            FilePath = bndExtForceFilePath;

            IEnumerable<IniSection> bndBlocks = ReadIniFile(bndExtForceFilePath).Sections;

            ReadPolyLines(bndBlocks, modelDefinition, area);

            ReadBoundaryConditions(bndBlocks, modelDefinition, network, boundaryConditions1D, lateralSourcesData);
        }

        
        private void ReadPolyLines(IEnumerable<IniSection> bndBlocks, WaterFlowFMModelDefinition modelDefinition, HydroArea area)
        {
            modelDefinition.Boundaries.ForEach(b => { existingPolylineFiles[b] = b.Name + ".pli"; });

            foreach (var iniSection in bndBlocks)
            {
                string roofFile = iniSection.GetPropertyValueWithOptionalDefaultValue(TargetMaskFileKey);
                if (area != null && roofFile != null)
                {
                    ReadRoofAreas(area, roofFile);
                    continue;
                }
                
                var locationFile = iniSection.GetPropertyValueWithOptionalDefaultValue(LocationFileKey);
                var isEmbankment = iniSection.GetPropertyValueWithOptionalDefaultValue(QuantityKey) == ExtForceQuantNames.EmbankmentBnd;
          
                if (existingPolylineFiles.Values.Contains(locationFile)) continue;

                if (locationFile == null) continue;

                if (string.IsNullOrEmpty(locationFile))
                {
                    Log.WarnFormat("Empty location file encountered in boundary ext-force file {0}", FilePath);
                    continue;
                }

                var pliFilePath = GetFullPath(locationFile);

                if (!File.Exists(pliFilePath))
                {
                    Log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                }


                if (isEmbankment)
                {
                    var reader = new PlizFile<Embankment>();
                    var features = reader.Read(pliFilePath);
                    if (!features.Any()) continue;
                    modelDefinition.Embankments.Add(features.First());
                }
                else
                {                
                    var reader = new PliFile<Feature2D>();
                    var features = reader.Read(pliFilePath);
                    foreach (var feature in features)
                    {
                        existingPolylineFiles[feature] = locationFile;
                        modelDefinition.Boundaries.Add(feature);
                        modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature});
                    }
                }
            }
        }

        private void ReadRoofAreas(HydroArea area, string relativeFilePath)
        {
            string roofFilePath = GetFullPath(relativeFilePath);
            if (!File.Exists(roofFilePath))
            {
                Log.Warn($"File does not exist: {roofFilePath}");
                return;
            }

            IList<GroupableFeature2DPolygon> roofAreas = roofPolFile.Read(roofFilePath);
            area.RoofAreas.AddRange(roofAreas);
        }

        private Dictionary<FmMeteoQuantity, Func<FmMeteoLocationType, IFmMeteoField>> IFMMeteoFieldGenerator = new Dictionary<FmMeteoQuantity, Func<FmMeteoLocationType, IFmMeteoField>>()
        {
            {FmMeteoQuantity.Precipitation, FmMeteoField.CreateMeteoPrecipitationSeries }
        };
        private void ReadBoundaryConditions(IEnumerable<IniSection> bndBlocks, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, IEventedList<Model1DBoundaryNodeData> boundaryConditions1D, IEventedList<Model1DLateralSourceData> lateralSourcesData)
        {
            var correctionFunctionTypes = BcFileFlowBoundaryDataBuilder.CorrectionFunctionTypes.ToList();

            var dataBlocks = new List<BcBlockData>();
            var bcFilePaths = new List<string>();

            // get file paths for each boundary
            foreach (var iniSection in bndBlocks)
            {
                var bcFiles = iniSection.GetPropertyValuesByName(ForcingFileKey);
                bcFilePaths.AddRange(bcFiles.Select(GetFullPath));
            }

            // read each file path (once)
            foreach (var bcFilePath in bcFilePaths.Distinct())
            {
                if (!File.Exists(bcFilePath))
                {
                    if (Path.GetFileName(bcFilePath) != ExtForceQuantNames.EmbankmentForcingFile)
                    {
                        Log.WarnFormat("Boundary condition data file {0} not found", bcFilePath);
                    }
                    continue;
                }

                dataBlocks.AddRange(bcFilePath.EndsWith(".bcm")
                    ? new BcmFile().Read(bcFilePath)
                    : new BcFile().Read(bcFilePath));
            }

            var correctionBlocks = dataBlocks.Where(db => correctionFunctionTypes.Contains(db.FunctionType)).ToList();

            var signalBlocks = dataBlocks.Except(correctionBlocks).ToList();
            var containsAndParsedModel1DBoundaryExtForceFileDefinitions = false;

            var useSalt = (bool) modelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value;
            bool useTemperature = (HeatFluxModelType) modelDefinition.GetModelProperty(KnownProperties.Temperature).Value != HeatFluxModelType.None;
            var logHandler = new LogHandler("reading the lateral sources");
            var lateralSourceParser = new BndExtForceLateralSourceParser(FilePath, network, useSalt, useTemperature, new BoundaryFileReader(), logHandler);

            foreach (var iniSection in bndBlocks)
            {
                var quantityKey = iniSection.GetPropertyValueWithOptionalDefaultValue(QuantityKey);
                if (string.IsNullOrEmpty(quantityKey))
                {
                    if (iniSection.Name.EqualsCaseInsensitive(LateralHeaderKey) && Is1DLateral(iniSection, logHandler))
                    {
                        lateralSourcesData.Add(lateralSourceParser.Parse(new LateralSourceExtSection(iniSection)));
                    }
                    
                    continue;
                }

                if (quantityKey == ExtForceQuantNames.Precipitation)
                {
                    List<BcBlockData> meteoDataBlocks = dataBlocks.Where(b => IsCorrespondingMeteoBlock(b, iniSection)).ToList();
                    ParseMeteoRainFallBoundaryExtForceSection(modelDefinition, quantityKey, meteoDataBlocks);
                    continue;
                }

                if (!containsAndParsedModel1DBoundaryExtForceFileDefinitions && iniSection.ReadProperty<string>(NodeIdKey, true) != null)
                {
                    containsAndParsedModel1DBoundaryExtForceFileDefinitions = CheckAndParseModel1DBoundaryOnNodeInBoundaryExtForceFile(network, boundaryConditions1D, bndBlocks.Where(b => b.Name.EqualsCaseInsensitive(BoundaryHeaderKey)));
                }
                if (iniSection.ReadProperty<string>(NodeIdKey, true) != null)
                    continue; // this INI section is 1d boundary, continue to check if you want to read a new INI section 2d boundary

                var quantity = FlowBoundaryQuantityType.WaterLevel;

                var timelagString = iniSection.GetPropertyValueWithOptionalDefaultValue(ThatcherHarlemanTimeLagKey);

                if (!string.IsNullOrEmpty(quantityKey) &&
                    !ExtForceQuantNames.TryParseBoundaryQuantityType(quantityKey, out quantity))
                {
                    if (quantityKey != ExtForceQuantNames.EmbankmentBnd)
                    {
                        Log.WarnFormat("Could not parse quantity {0} into a valid flow boundary condition",
                            quantityKey);
                    }
                    continue;
                }

                

                var pliFile = iniSection.GetPropertyValueWithOptionalDefaultValue(LocationFileKey);
                var feature = existingPolylineFiles.FirstOrDefault(kvp => kvp.Value == pliFile).Key;
                if (feature == null) continue;

                BcFileFlowBoundaryDataBuilder builder;
                if (IsMorphologyRelatedProperty(quantity))
                {
                    builder = new BcmFileFlowBoundaryDataBuilder
                    {
                        ExcludedQuantities =
                            Enum.GetValues(typeof(FlowBoundaryQuantityType))
                                .Cast<FlowBoundaryQuantityType>()
                                .Except(new[] { quantity })
                                .ToList(),
                        OverwriteExistingData = true,
                        CanCreateNewBoundaryCondition = true,
                        LocationFilter = feature,
                    };
                }
                else
                {
                    builder = new BcFileFlowBoundaryDataBuilder
                    {
                        ExcludedQuantities =
                            Enum.GetValues(typeof(FlowBoundaryQuantityType))
                                .Cast<FlowBoundaryQuantityType>()
                                .Except(new[] {quantity})
                                .ToList(),
                        OverwriteExistingData = true,
                        CanCreateNewBoundaryCondition = true,
                        LocationFilter = feature,
                    };
                }
                var bcSets =
                    modelDefinition.BoundaryConditionSets.Select(bcs => new BoundaryConditionSet {Feature = bcs.Feature})
                        .ToList();

                // first loading signals, then corrections
                var usedDataBlocks =
                    signalBlocks.Where(
                        dataBlock => builder.InsertBoundaryData(bcSets, dataBlock, timelagString))
                        .ToList();

                usedDataBlocks.AddRange(
                    correctionBlocks.Where(
                        dataBlock => builder.InsertBoundaryData(bcSets, dataBlock, timelagString)));

                var newBoundaryCondition = bcSets.SelectMany(bcs => bcs.BoundaryConditions).FirstOrDefault();

                if (newBoundaryCondition != null)
                {
                    existingBndForceFileItems[newBoundaryCondition] = iniSection;
                }

                usedDataBlocks.ForEach(b =>
                {
                    signalBlocks.Remove(b);
                    correctionBlocks.Remove(b);
                });

                for (var i = 0; i < bcSets.Count(); ++i)
                {
                    modelDefinition.BoundaryConditionSets[i].BoundaryConditions.AddRange(bcSets[i].BoundaryConditions);
                }
            }
            
            logHandler.LogReport();
        }

        private static bool Is1DLateral(IniSection iniSection, ILogHandler logHandler)
        {
            var locationType = iniSection.ReadProperty(InitialConditionRegion.LocationType.Key, true, "1d");
            if (locationType != "1d")
            {
                var name = iniSection.ReadProperty<string>("id");
                var longName = iniSection.ReadProperty<string>(BoundaryRegion.Name.Key);
                logHandler.ReportError($"We do not support {locationType} lateral source types, cannot import {name} ({longName})");
                return false;
            }
            return true;
        }

        private static bool IsCorrespondingMeteoBlock(BcBlockData b, IniSection iniSection)
        {
            string fileName = Path.GetFileName(iniSection.GetPropertyValueWithOptionalDefaultValue(ForcingFileKey));
            string quantity = iniSection.GetPropertyValueWithOptionalDefaultValue(QuantityKey);

            return Path.GetFileName(b.FilePath) == fileName &&
                   b.Quantities.Any(q => q.Quantity == quantity);
        }

        private bool CheckAndParseModel1DBoundaryOnNodeInBoundaryExtForceFile(IHydroNetwork network, IEventedList<Model1DBoundaryNodeData> boundaryConditions1D,  IEnumerable<IniSection> modelBoundary1DBlocks)
        {
            network.Nodes.Except(boundaryConditions1D.Select(bc1d => bc1d.Node)).ForEach(node => boundaryConditions1D.Add(Helper1D.CreateDefaultBoundaryCondition(node, false, false)));
            var forcingFiles = new HashSet<string>();

            Dictionary<string, Model1DBoundaryNodeData> bndByNodeName = ToNodeNameDictionary(boundaryConditions1D);

            foreach (var iniSection in modelBoundary1DBlocks)
            {
                var nodeId = iniSection.GetPropertyValueWithOptionalDefaultValue(NodeIdKey);
                if (nodeId == null) continue;
                
                if (!bndByNodeName.TryGetValue(nodeId, out Model1DBoundaryNodeData boundaryData))
                {
                    continue; 
                }
                
                double boundaryWidth = iniSection.ReadProperty(BoundaryWidth, true, double.NaN);
                if (!double.IsNaN(boundaryWidth))
                {
                    boundaryData.BoundaryWidth = boundaryWidth;
                }
                
                double boundaryDepth = iniSection.ReadProperty(BoundaryDepth, true, double.NaN);
                if (!double.IsNaN(boundaryDepth))
                {
                    boundaryData.BoundaryDepth = boundaryDepth;
                }
                
                var id = iniSection.GetPropertyValueWithOptionalDefaultValue(IdKey);
                if (id != null) continue; // make sure we are nog reading a lateral
                var forcingFile = iniSection.GetPropertyValueWithOptionalDefaultValue(ForcingFileKey);
                if (forcingFile == null) continue;
                forcingFiles.Add(forcingFile);
            }

            foreach (var fullPath in forcingFiles.Distinct().Select(GetFullPath))
            {
                if (!File.Exists(fullPath))
                {
                    Log.Error($"Reading boundary source data failed because path to data does not exist {fullPath}");
                    continue;
                }

                var isValidBcFile = IoHelper.IsValidTextFile(fullPath) &&
                                        ReadIniFile(fullPath).Sections
                                        .Any(c =>
                                            c.ValidGeneralRegion(
                                                GeneralRegion.BoundaryConditionsMajorVersion,
                                                GeneralRegion.BoundaryConditionsMinorVersion,
                                                GeneralRegion.FileTypeName.BoundaryConditions));
                if (isValidBcFile)
                    BoundaryFileReader.ReadFile(fullPath, boundaryConditions1D);
                else
                    Log.Warn($"Can not read boundary model data from : {fullPath}");
            }
            return true;
        }

        private static Dictionary<string, Model1DBoundaryNodeData> ToNodeNameDictionary(IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D)
        {
            var bndByNodeName = new Dictionary<string, Model1DBoundaryNodeData>();
            foreach (Model1DBoundaryNodeData boundary in boundaryConditions1D)
            {
                if (boundary.Node is IManhole manhole)
                {
                    foreach (ICompartment compartment in manhole.Compartments)
                    {
                        bndByNodeName[compartment.Name] = boundary;
                    }
                }

                else
                {
                    bndByNodeName[boundary.Node.Name] = boundary;
                }
            }

            return bndByNodeName;
        }

        private void ParseMeteoRainFallBoundaryExtForceSection(WaterFlowFMModelDefinition modelDefinition, string quantityKey, List<BcBlockData> dataBlocks)
        {
            FmMeteoQuantity meteoQuantity = ExtForceQuantNames.MeteoQuantityNames.FirstOrDefault(pair => pair.Value == quantityKey).Key;

            // NU ALTIJD GLOBAL!!!
            var fmMeteoField = IFMMeteoFieldGenerator[meteoQuantity](FmMeteoLocationType.Global);

            BcMeteoFileDataBuilder meteobuilder;
            meteobuilder = new BcMeteoFileDataBuilder
            {
                OverwriteExistingData = true,
                CanCreateNewBoundaryCondition = true,
            };

            
            meteobuilder.InsertBoundaryData(fmMeteoField, dataBlocks);

            if (modelDefinition.FmMeteoFields.Contains(fmMeteoField))
            {
                Log.WarnFormat(
                    "Could parse fm meteo data {0} into a valid meteo location data, but this type already exists in the model. We have overwritten it's data",
                    fmMeteoField.Name);
                modelDefinition.FmMeteoFields.Remove(fmMeteoField);
            }

            modelDefinition.FmMeteoFields.Add(fmMeteoField);
        }

        private static bool IsMorphologyRelatedProperty(FlowBoundaryQuantityType quantity)
        {
            return quantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelFixed
                   || quantity == FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint;
        }

        private static IniData ReadIniFile(string filepath)
        {
            var iniParser = new IniParser()
            {
                Configuration =
                {
                    AllowMultiLineValues = true
                }
            };

            Log.InfoFormat(Resources.BndExtForceFile_ReadIniFile_Reading_external_forcings_from__0__, filepath);
            using (FileStream iniStream = File.OpenRead(filepath))
            {
                return iniParser.Parse(iniStream);
            }
        }

        #endregion
    }

    internal class LateralSourceForcingDefinition
    {
        public string Type { get; set; }
        public string NodeId { get; set; }
        public string BranchId { get; set; }
        public double Chainage { get; set; }
        public int NumCoordinates { get; set; }
        public double Discharge { get; set; }
        public bool RealTime { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public string DischargeForcingFile { get; set; }
    }
}
