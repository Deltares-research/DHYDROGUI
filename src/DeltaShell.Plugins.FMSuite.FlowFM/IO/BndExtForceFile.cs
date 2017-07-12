using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class BndExtForceFile : FMSuiteFileBase
    {
        private const string BoundaryBlockKey = "[boundary]";
        private const string QuantityKey = "quantity";
        public const string LocationFileKey = "locationfile";
        public const string ForcingFileKey = "forcingfile";
        private const string AreaKey = "area";
        private const string ThatcherHarlemanTimeLagKey = "return_time";
        private const string OpenBoundaryToleranceKey = "OpenBoundaryTolerance";
        public static double OpenBoundaryTolerance = 0.5; // made public static while this value still needs to be tweaked *run away run away*

        private static DelftIniCategory CreateBoundaryBlock(string quantity, string locationFilePath, string forcingFilePath, TimeSpan thatcherHarlemanTimeLag, bool isEmbankment = false)
        {
            var block = new DelftIniCategory(BoundaryBlockKey);
            if (quantity != null)
            {
                block.AddProperty(QuantityKey, quantity);
            }
            if (locationFilePath != null)
            {
                block.AddProperty(LocationFileKey, locationFilePath);
            }
            if (forcingFilePath != null)
            {
                block.AddProperty(ForcingFileKey, forcingFilePath);
            }
            if (thatcherHarlemanTimeLag != TimeSpan.Zero)
            {
                block.AddProperty(ThatcherHarlemanTimeLagKey, thatcherHarlemanTimeLag.TotalSeconds);
            }
            if (isEmbankment)
            {
                block.AddProperty(OpenBoundaryToleranceKey, OpenBoundaryTolerance);
            }
            return block;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (BndExtForceFile));

        private const BcFile.WriteMode BcFileWriteMode = BcFile.WriteMode.FilePerQuantity;
        private const BcmFile.WriteMode BcmFileWriteMode = BcmFile.WriteMode.SingleFile; 

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolylineFiles; 
        private readonly IDictionary<IBoundaryCondition, DelftIniCategory> existingBndForceFileItems;

        public bool WriteToDisk { get; set; }

        private string FilePath { get; set; } 

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(FilePath), relativePath);
        }

        public BndExtForceFile()
        {
            existingPolylineFiles = new Dictionary<Feature2D, string>();
            existingBndForceFileItems = new Dictionary<IBoundaryCondition, DelftIniCategory>();
            WriteToDisk = true;
        }

        #region write logic

        public void Write(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var refDate = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            Write(filePath, modelDefinition.ModelName, modelDefinition.BoundaryConditionSets, modelDefinition.Embankments,
                modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile), refDate);
        }

        private void Write(string filePath, string modelDefinitionModelName, IList<BoundaryConditionSet> boundaryConditionSets, IList<Embankment> embankments, WaterFlowFMProperty modelProperty, DateTime refDate)
        {
            FilePath = filePath;
            var bndExtForceFileItems = WriteBndExtForceFileSubFiles(modelDefinitionModelName, boundaryConditionSets, refDate);
            var embankmentForceFileItems = WriteEmbankmentFiles(embankments);

            var allItems = bndExtForceFileItems.Concat(embankmentForceFileItems).ToList();
            if (allItems.Count > 0)
            {
                WriteBndExtForceFile(allItems);
                modelProperty.Value = Path.GetFileName(FilePath);
            }
            else
            {
                FileUtils.DeleteIfExists(FilePath);
                modelProperty.Value = "";
            }
        }

        private void WriteBndExtForceFile(IEnumerable<DelftIniCategory> bndExtForceFileItems)
        {
            OpenOutputFile(FilePath);
            try
            {
                foreach (var bndExtForceFileItem in bndExtForceFileItems)
                {
                    WriteLine("");
                    WriteLine(BoundaryBlockKey);
                    WriteLine(QuantityKey + "=" + bndExtForceFileItem.GetPropertyValue(QuantityKey));
                    WriteLine(LocationFileKey + "=" + bndExtForceFileItem.GetPropertyValue(LocationFileKey));

                    string openBoundaryTolerance = bndExtForceFileItem.GetPropertyValues(OpenBoundaryToleranceKey).FirstOrDefault();
                    if (openBoundaryTolerance != null)
                    {
                        WriteLine(OpenBoundaryToleranceKey + "=" + openBoundaryTolerance);
                    }

                    foreach (var propertyValue in bndExtForceFileItem.GetPropertyValues(ForcingFileKey))
                    {
                        WriteLine(ForcingFileKey + "=" + propertyValue);                        
                    }
                    var timelag = bndExtForceFileItem.GetPropertyValue(ThatcherHarlemanTimeLagKey);
                    if(timelag != null)
                    {
                        WriteLine(ThatcherHarlemanTimeLagKey + "=" + timelag);
                    }
                    var area = bndExtForceFileItem.GetPropertyValue(AreaKey);
                    if (area != null)
                    {
                        WriteLine(AreaKey + "=" + area);
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        // TODO: migrate sources & sinks to new format

        public IList<DelftIniCategory> WriteBndExtForceFileSubFiles(string modelDefinitionModelName, IList<BoundaryConditionSet> boundaryConditionSets, DateTime refDate)
        {
            WritePolyLines(boundaryConditionSets);

            var resultingItems =
                boundaryConditionSets.Where(bcs => !bcs.BoundaryConditions.Any())
                    .Select(boundaryConditionSet => existingPolylineFiles[boundaryConditionSet.Feature])
                    .Select(pliFileName => CreateBoundaryBlock(null, pliFileName, null, TimeSpan.Zero))
                    .ToList();

            /* Write all morphology boundaries in one file.*/
            var bcmFile = new BcmFile { MultiFileMode = BcmFileWriteMode };
            var morphologyGroupings = bcmFile.GroupBoundaryConditions(boundaryConditionSets);
            
            WriteBoundaryConditions(refDate, bcmFile, morphologyGroupings, new BcmFileFlowBoundaryDataBuilder(), modelDefinitionModelName + BcmFile.Extension);
            /* No longer return the morphology groupings since they will not be written to the .ext file (DELFT3DFM-1106) */
            
            var bcFile = new BcFile { MultiFileMode = BcFileWriteMode };
            var standardGroupings = bcFile.GroupBoundaryConditions(boundaryConditionSets);
            resultingItems.AddRange(WriteBoundaryConditions(refDate, bcFile, standardGroupings, new BcFileFlowBoundaryDataBuilder()).Distinct());

            return resultingItems;
        }

        private IList<DelftIniCategory> WriteEmbankmentFiles(IList<Embankment> embankments)
        {
            var categories = new List<DelftIniCategory>();

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

                categories.Add(CreateBoundaryBlock(ExtForceQuantNames.EmbankmentBnd, existingFile, ExtForceQuantNames.EmbankmentForcingFile, TimeSpan.Zero, true));
            }

            return categories;
        }

        private void WritePolyLines(IEnumerable<BoundaryConditionSet> boundaryConditionSets)
        {
            foreach (var boundaryConditionSet in boundaryConditionSets)
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue(boundaryConditionSet.Feature, out existingFile))
                {
                    existingFile = ExtForceFileHelper.GetPliFileName(boundaryConditionSet);
                    existingPolylineFiles[boundaryConditionSet.Feature] = existingFile;
                }
                if (WriteToDisk)
                {
                    new PliFile<Feature2D>().Write(GetFullPath(existingFile), new[] {boundaryConditionSet.Feature});
                }
            }
        }

        static string AddExtension(string fileName, string extension)
        {
            var cleanFileName=fileName.TrimEnd(new[] {'.'});
            var cleanExtension = extension.TrimStart(new[] {'.'});
            return string.Concat(cleanFileName, ".", cleanExtension);
        }

        private IEnumerable<DelftIniCategory> WriteBoundaryConditions(DateTime refDate, BcFile bcFile, 
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> grouping, BcFileFlowBoundaryDataBuilder boundaryDataBuilder, string bcFileName = null)
        {
            var resultingItems = new List<DelftIniCategory>();
            
            var fileNamesToBoundaryConditions =
                new Dictionary<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>>();
            
            foreach (var group in grouping)
            {
                foreach (var tuple in group.Where(t => t.Item1 is FlowBoundaryCondition))
                {
                    DelftIniCategory existingBlock;
                    existingBndForceFileItems.TryGetValue(tuple.Item1, out existingBlock);

                    var existingPaths = existingBlock != null
                        ? existingBlock.GetPropertyValues(ForcingFileKey).ToList()
                        : new List<string>();

                    var path = existingPaths.Any()
                        ? existingPaths.First()
                        : AddExtension(group.Key, bcFile is BcmFile ? BcmFile.Extension : BcFile.Extension);

                    if (existingBlock != null && !existingPaths.Contains(path))
                    {
                        existingBlock.AddProperty(ForcingFileKey, path);
                    }

                    var corrPath = existingPaths.Count > 1
                            ? existingPaths[1]
                            : AddExtension(group.Key + "_corr", BcFile.Extension);

                    if (existingBlock != null)
                    {
                        // set thatcher harlemann time lag once it is already existent in the ext force file but it has changed.
                        var condition = (FlowBoundaryCondition) tuple.Item1;
                        existingBlock.SetProperty(ThatcherHarlemanTimeLagKey, condition.ThatcherHarlemanTimeLag.TotalSeconds);

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType) && !existingPaths.Contains(corrPath))
                        {
                            existingBlock.AddProperty(ForcingFileKey, corrPath);
                        }
                        if (!BcFile.IsCorrectionType(tuple.Item1.DataType) && existingPaths.Contains(corrPath))
                        {
                            existingBlock.Properties.RemoveAllWhere(p => p.Value == corrPath);
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

                        var bndBlock = CreateBoundaryBlock(quantityName, pliFileName, path, ((FlowBoundaryCondition)tuple.Item1).ThatcherHarlemanTimeLag);

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType))
                        {
                            bndBlock.AddProperty(ForcingFileKey, corrPath);
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

        #endregion

        #region read logic

        public void Read(string bndExtForceFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            FilePath = bndExtForceFilePath;

            var bndBlocks = new DelftIniReader().ReadDelftIniFile(bndExtForceFilePath);

            ReadPolyLines(bndBlocks, modelDefinition);

            ReadBoundaryConditions(bndBlocks, modelDefinition);
        }

        
        private void ReadPolyLines(IEnumerable<DelftIniCategory> bndBlocks, WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (var delftIniCategory in bndBlocks)
            {
                var locationFile = delftIniCategory.GetPropertyValue(LocationFileKey);
                var isEmbankment = delftIniCategory.GetPropertyValue(QuantityKey) == ExtForceQuantNames.EmbankmentBnd;

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
                    if (!features.Any()) continue;
                    foreach (var feature in features)
                    {
                        existingPolylineFiles[feature] = locationFile;
                        modelDefinition.Boundaries.Add(feature);
                        modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature});
                    }
                }
            }
        }

        private void ReadBoundaryConditions(IList<DelftIniCategory> bndBlocks, WaterFlowFMModelDefinition modelDefinition)
        {
            var correctionFunctionTypes = BcFileFlowBoundaryDataBuilder.CorrectionFunctionTypes.ToList();

            var dataBlocks = new List<BcBlockData>();
            var bcFilePaths = new List<string>();

            // get file paths for each boundary
            foreach (var delftIniCategory in bndBlocks)
            {
                var bcFiles = delftIniCategory.GetPropertyValues(ForcingFileKey);
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
         
            foreach (var delftIniCategory in bndBlocks)
            {
                var quantityKey = delftIniCategory.GetPropertyValue(QuantityKey);
                
                var quantity = FlowBoundaryQuantityType.WaterLevel;

                var timelagString = delftIniCategory.GetPropertyValue(ThatcherHarlemanTimeLagKey);

                if (!string.IsNullOrEmpty(quantityKey) && !ExtForceQuantNames.TryParseBoundaryQuantityType(quantityKey, out quantity))
                {
                    if (quantityKey != ExtForceQuantNames.EmbankmentBnd)
                    {
                        Log.WarnFormat("Could not parse quantity {0} into a valid flow boundary condition", quantityKey);
                    }
                    continue;
                }

                var pliFile = delftIniCategory.GetPropertyValue(LocationFileKey);
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
                    existingBndForceFileItems[newBoundaryCondition] = delftIniCategory;
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
        }

        private static bool IsMorphologyRelatedProperty(FlowBoundaryQuantityType quantity)
        {
            return quantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelFixed
                   || quantity == FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint
                   || quantity == FlowBoundaryQuantityType.SedimentConcentration;
        }

        #endregion
    }
}
