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
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccess;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class BndExtForceFile : FMSuiteFileBase
    {
        public const string BoundaryBlockKey = "[boundary]";
        public const string QuantityKey = "quantity";
        public const string LocationFileKey = "locationfile";
        public const string ForcingFileKey = "forcingfile";
        private const string AreaKey = "area";
        private const string ThatcherHarlemanTimeLagKey = "return_time";
        private const string OpenBoundaryToleranceKey = "OpenBoundaryTolerance";

        public static double
            OpenBoundaryTolerance =
                0.5; // made public static while this value still needs to be tweaked *run away run away*

        private static IDelftIniCategory CreateBoundaryBlock(string quantity, string locationFilePath,
                                                            string forcingFilePath, TimeSpan thatcherHarlemanTimeLag,
                                                            bool isEmbankment = false)
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(BndExtForceFile));

        private const BcFile.WriteMode BcFileWriteMode = BcFile.WriteMode.FilePerQuantity;
        private const BcFile.WriteMode BcmFileWriteMode = BcFile.WriteMode.SingleFile;

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolylineFiles;
        private readonly IDictionary<IBoundaryCondition, IDelftIniCategory> existingBndForceFileItems;

        public bool WriteToDisk { get; set; }

        private string bndExtFilePath;

        private string BndExtFilePath
        {
            get => bndExtFilePath;
            set => bndExtFilePath = value;
        }
        
        private string bndExtSubFilesReferenceFilePath;

        private string BndExtSubFilesReferenceFilePath
        {
            get => bndExtSubFilesReferenceFilePath;
            set => bndExtSubFilesReferenceFilePath = value;
        }

        private string GetFullPathForReading(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(BndExtSubFilesReferenceFilePath), relativePath);
        }

        private string GetFullPathForWriting(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(BndExtFilePath), relativePath);
        }

        public BndExtForceFile()
        {
            existingPolylineFiles = new Dictionary<Feature2D, string>();
            existingBndForceFileItems = new Dictionary<IBoundaryCondition, IDelftIniCategory>();
            WriteToDisk = true;
        }

        #region write logic

        public void Write(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var refDate = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            Write(
                filePath,
                modelDefinition.ModelName,
                modelDefinition.BoundaryConditionSets,
                modelDefinition.Embankments,
                modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile),
                refDate);
        }

        private void Write(string filePath, string modelDefinitionModelName,
                           IList<BoundaryConditionSet> boundaryConditionSets, IList<Embankment> embankments,
                           WaterFlowFMProperty modelProperty, DateTime refDate)
        {
            BndExtFilePath = filePath;
            IList<IDelftIniCategory> bndExtForceFileItems =
                WriteBndExtForceFileSubFiles(modelDefinitionModelName, boundaryConditionSets, refDate);
            IList<IDelftIniCategory> embankmentForceFileItems = WriteEmbankmentFiles(embankments);

            List<IDelftIniCategory> allItems = bndExtForceFileItems.Concat(embankmentForceFileItems).ToList();
            if (allItems.Count > 0)
            {
                WriteBndExtForceFile(allItems);
                modelProperty.SetValueAsString(Path.GetFileName(BndExtFilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(BndExtFilePath);
                modelProperty.SetValueAsString(string.Empty);
            }
        }

        private void WriteBndExtForceFile(IEnumerable<IDelftIniCategory> bndExtForceFileItems)
        {
            OpenOutputFile(BndExtFilePath);
            try
            {
                foreach (IDelftIniCategory bndExtForceFileItem in bndExtForceFileItems)
                {
                    WriteLine("");
                    WriteLine(BoundaryBlockKey);
                    WritePropertyValue(QuantityKey, bndExtForceFileItem);
                    WritePropertyValue(LocationFileKey, bndExtForceFileItem);

                    string openBoundaryTolerance = bndExtForceFileItem.GetPropertyValues(OpenBoundaryToleranceKey)
                                                                      .FirstOrDefault();
                    if (openBoundaryTolerance != null)
                    {
                        WritePropertyValue(OpenBoundaryToleranceKey, openBoundaryTolerance);
                    }

                    WritePropertyValues(ForcingFileKey, bndExtForceFileItem);
                    WritePropertyValueIfNotNull(ThatcherHarlemanTimeLagKey, bndExtForceFileItem);
                    WritePropertyValueIfNotNull(AreaKey, bndExtForceFileItem);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WritePropertyValues(string propertyName, IDelftIniCategory bndExtForceFileItem)
        {
            foreach (string propertyValue in bndExtForceFileItem.GetPropertyValues(propertyName))
            {
                WritePropertyValue(propertyName, propertyValue);
            }
        }

        private void WritePropertyValue(string propertyName, IDelftIniCategory bndExtForceFileItem)
        {
            WritePropertyValue(propertyName, bndExtForceFileItem.GetPropertyValue(propertyName));
        }

        private void WritePropertyValueIfNotNull(string propertyName, IDelftIniCategory bndExtForceFileItem)
        {
            string propertyValue = bndExtForceFileItem.GetPropertyValue(propertyName);
            if (propertyValue != null)
            {
                WritePropertyValue(propertyName, propertyValue);
            }
        }

        private void WritePropertyValue(string propertyName, string propertyValue)
        {
            WriteLine(propertyName + "=" + propertyValue);
        }

        // TODO: migrate sources & sinks to new format

        public IList<IDelftIniCategory> WriteBndExtForceFileSubFiles(string modelDefinitionModelName,
                                                                    IList<BoundaryConditionSet> boundaryConditionSets,
                                                                    DateTime refDate)
        {
            WritePolyLines(boundaryConditionSets);

            List<IDelftIniCategory> resultingItems =
                boundaryConditionSets.Where(bcs => !bcs.BoundaryConditions.Any())
                                     .Select(boundaryConditionSet =>
                                     {
                                         string pliFileName;
                                         return existingPolylineFiles.TryGetValue(
                                                    boundaryConditionSet.Feature, out pliFileName)
                                                    ? CreateBoundaryBlock(null, pliFileName, null, TimeSpan.Zero)
                                                    : null;
                                     }).Where(it => it != null)
                                     .ToList();

            /* Write all morphology boundaries in one file.*/
            var bcmFile = new BcmFile {MultiFileMode = BcmFileWriteMode};
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> morphologyGroupings =
                bcmFile.GroupBoundaryConditions(boundaryConditionSets);

            WriteBoundaryConditions(refDate, bcmFile, morphologyGroupings, new BcmFileFlowBoundaryDataBuilder(),
                                    modelDefinitionModelName);
            /* No longer return the morphology groupings since they will not be written to the .ext file (DELFT3DFM-1106) */

            var bcFile = new BcFile {MultiFileMode = BcFileWriteMode};
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> standardGroupings =
                bcFile.GroupBoundaryConditions(boundaryConditionSets);
            resultingItems.AddRange(WriteBoundaryConditions(refDate, bcFile, standardGroupings,
                                                            new BcFileFlowBoundaryDataBuilder(),
                                                            modelDefinitionModelName).Distinct());

            return resultingItems;
        }

        private IList<IDelftIniCategory> WriteEmbankmentFiles(IList<Embankment> embankments)
        {
            var categories = new List<IDelftIniCategory>();

            foreach (Embankment embankment in embankments)
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue(embankment, out existingFile))
                {
                    existingFile = embankment.Name + "_bnk.pliz";
                    existingPolylineFiles[embankment] = existingFile;
                }

                if (WriteToDisk)
                {
                    new PlizFile<Embankment>().Write(GetFullPathForWriting(existingFile), new[]
                    {
                        embankment
                    });
                }

                categories.Add(CreateBoundaryBlock(ExtForceQuantNames.EmbankmentBnd, existingFile,
                                                   ExtForceQuantNames.EmbankmentForcingFile, TimeSpan.Zero, true));
            }

            return categories;
        }

        private void WritePolyLines(IEnumerable<BoundaryConditionSet> boundaryConditionSets)
        {
            foreach (BoundaryConditionSet boundaryConditionSet in boundaryConditionSets)
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue(boundaryConditionSet.Feature, out existingFile))
                {
                    existingFile = ExtForceFileHelper.GetPliFileName(boundaryConditionSet);
                    if (string.IsNullOrEmpty(existingFile))
                    {
                        return;
                    }

                    existingPolylineFiles[boundaryConditionSet.Feature] = existingFile;
                }

                if (WriteToDisk)
                {
                    new PliFile<Feature2D>().Write(GetFullPathForWriting(existingFile), new[]
                    {
                        boundaryConditionSet.Feature
                    });
                }
            }
        }

        private static string AddExtension(string fileName, string extension)
        {
            string cleanFileName = fileName.TrimEnd(new[]
            {
                '.'
            });
            string cleanExtension = extension.TrimStart(new[]
            {
                '.'
            });
            return string.Concat(cleanFileName, ".", cleanExtension);
        }

        private IEnumerable<IDelftIniCategory> WriteBoundaryConditions(DateTime refDate, BcFile bcFile,
                                                                      IEnumerable<IGrouping<string,
                                                                          Tuple<IBoundaryCondition, BoundaryConditionSet
                                                                          >>> grouping,
                                                                      BcFileFlowBoundaryDataBuilder boundaryDataBuilder,
                                                                      string modelDefinitionName)
        {
            var resultingItems = new List<IDelftIniCategory>();

            var fileNamesToBoundaryConditions =
                new Dictionary<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>>();

            foreach (IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>> group in grouping)
            {
                foreach (Tuple<IBoundaryCondition, BoundaryConditionSet> tuple in group.Where(
                    t => t.Item1 is FlowBoundaryCondition))
                {
                    IDelftIniCategory existingBlock;
                    existingBndForceFileItems.TryGetValue(tuple.Item1, out existingBlock);

                    List<string> existingPaths = existingBlock != null
                                                     ? existingBlock.GetPropertyValues(ForcingFileKey).ToList()
                                                     : new List<string>();

                    string fileName = group.Key;
                    if (string.IsNullOrEmpty(fileName) && bcFile.MultiFileMode == BcFile.WriteMode.SingleFile)
                    {
                        fileName = modelDefinitionName;
                    }

                    string path = existingPaths.Any()
                                      ? existingPaths.First()
                                      : AddExtension(
                                          fileName, bcFile is BcmFile ? BcmFile.Extension : BcFile.Extension);

                    if (existingBlock != null && !existingPaths.Contains(path))
                    {
                        existingBlock.AddProperty(ForcingFileKey, path);
                    }

                    string corrPath = existingPaths.Count > 1
                                          ? existingPaths[1]
                                          : AddExtension(fileName + "_corr", BcFile.Extension);

                    if (existingBlock != null)
                    {
                        // set thatcher harlemann time lag once it is already existent in the ext force file but it has changed.
                        var condition = (FlowBoundaryCondition) tuple.Item1;
                        existingBlock.SetProperty(ThatcherHarlemanTimeLagKey,
                                                  condition.ThatcherHarlemanTimeLag.TotalSeconds);

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
                            tuples = new List<Tuple<IBoundaryCondition, BoundaryConditionSet>> {tuple};
                            fileNamesToBoundaryConditions.Add(corrPath, tuples);
                        }
                    }

                    if (existingBlock == null)
                    {
                        string quantityName =
                            ExtForceQuantNames.GetQuantityString((FlowBoundaryCondition) tuple.Item1);

                        string pliFileName = existingPolylineFiles[tuple.Item2.Feature];

                        IDelftIniCategory bndBlock = CreateBoundaryBlock(quantityName, pliFileName, path,
                                                                        ((FlowBoundaryCondition) tuple.Item1)
                                                                        .ThatcherHarlemanTimeLag);

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
                foreach (KeyValuePair<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>>
                             fileNamesToBoundaryCondition in fileNamesToBoundaryConditions)
                {
                    string fullPath = GetFullPathForWriting(fileNamesToBoundaryCondition.Key);

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

        public void Read(string bndExtForceFilePath, WaterFlowFMModelDefinition modelDefinition, string bndExtSubFilesReferenceFilePath)
        {
            BndExtFilePath = bndExtForceFilePath;
            BndExtSubFilesReferenceFilePath = bndExtSubFilesReferenceFilePath;

            IList<IDelftIniCategory> bndBlocks = new DelftIniReader().ReadDelftIniFile(bndExtForceFilePath);

            ReadPolyLines(bndBlocks, modelDefinition);

            ReadBoundaryConditions(bndBlocks, modelDefinition);
        }

        private void ReadPolyLines(IEnumerable<IDelftIniCategory> bndBlocks, WaterFlowFMModelDefinition modelDefinition)
        {
            modelDefinition.Boundaries.ForEach(b => { existingPolylineFiles[b] = b.Name + ".pli"; });

            foreach (IDelftIniCategory delftIniCategory in bndBlocks)
            {
                string locationFile = delftIniCategory.GetPropertyValue(LocationFileKey);
                bool locationFileHasAlreadyBeenRead = existingPolylineFiles.Values.Contains(locationFile);
                if (locationFile == null || locationFileHasAlreadyBeenRead)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(locationFile))
                {
                    Log.WarnFormat("Empty location file encountered in boundary ext-force file {0}", BndExtFilePath);
                    continue;
                }

                string pliFilePath = GetFullPathForReading(locationFile);

                if (!File.Exists(pliFilePath))
                {
                    Log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                }

                if (IsEmbankmentCategory(delftIniCategory))
                {
                    var plizFileReader = new PlizFile<Embankment>();
                    IList<Embankment> embankments = plizFileReader.Read(pliFilePath);
                    LogWarningMessagesForOnePointGeometryEmbankments(embankments, pliFilePath);

                    Embankment[] validEmbankments = embankments.Where(e => e.Geometry.Coordinates.Length > 1).ToArray();
                    if (!validEmbankments.Any())
                    {
                        continue;
                    }

                    modelDefinition.Embankments.Add(validEmbankments.First());
                }
                else
                {
                    var reader = new PliFile<Feature2D>();
                    IList<Feature2D> features = reader.Read(pliFilePath);
                    if (!features.Any())
                    {
                        continue;
                    }

                    foreach (Feature2D feature in features)
                    {
                        existingPolylineFiles[feature] = locationFile;
                        modelDefinition.Boundaries.Add(feature);
                        modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature});
                    }
                }
            }
        }

        private static void LogWarningMessagesForOnePointGeometryEmbankments(
            IList<Embankment> embankments, string pliFilePath)
        {
            IEnumerable<Embankment> onePointEmbankments = embankments.Where(e => e.Geometry.Coordinates.Length == 1);
            string directory = Directory.GetParent(pliFilePath).FullName;
            onePointEmbankments.ForEach(e =>
            {
                string embankmentFilePath = Path.Combine(directory, $"{e.Name}_bnk.pliz");
                Log.Warn(
                    $"Embankment file '{embankmentFilePath}' with only 1 point detected and it will not be imported.");
            });
        }

        private static bool IsEmbankmentCategory(IDelftIniCategory delftIniCategory)
        {
            return delftIniCategory.GetPropertyValue(QuantityKey) == ExtForceQuantNames.EmbankmentBnd;
        }

        private void ReadBoundaryConditions(IList<IDelftIniCategory> delftIniCategories,
                                            WaterFlowFMModelDefinition modelDefinition)
        {
            List<string> correctionFunctionTypes = BcFileFlowBoundaryDataBuilder.CorrectionFunctionTypes.ToList();

            IEnumerable<string> bcFilePaths = GetForcingFilePathsFromIniCategories(delftIniCategories);

            List<BcBlockData> dataBlocks = ReadBoundaryConditionBlocks(bcFilePaths);

            List<BcBlockData> correctionBlocks =
                dataBlocks.Where(db => correctionFunctionTypes.Contains(db.FunctionType)).ToList();

            List<BcBlockData> signalBlocks = dataBlocks.Except(correctionBlocks).ToList();

            foreach (IDelftIniCategory delftIniCategory in delftIniCategories)
            {
                if (TryGetQuantityValue(delftIniCategory, out FlowBoundaryQuantityType quantity))
                {
                    continue;
                }

                string pliFile = delftIniCategory.GetPropertyValue(LocationFileKey);

                Feature2D feature = existingPolylineFiles.FirstOrDefault(kvp => kvp.Value == pliFile).Key;

                if (feature == null)
                {
                    continue;
                }

                BcFileFlowBoundaryDataBuilder builder = CreateFlowBoundaryDataBuilder(quantity, feature);

                List<BoundaryConditionSet> bcSets = CreateBoundaryConditionSetsWithFeature(modelDefinition);

                // first loading signals, then corrections

                string timeLagString = delftIniCategory.GetPropertyValue(ThatcherHarlemanTimeLagKey);

                var usedDataBlocks = new List<BcBlockData>();
                usedDataBlocks.AddRange(signalBlocks
                                            .Where(dataBlock =>
                                                       builder.InsertBoundaryData(bcSets, dataBlock, timeLagString)));
                usedDataBlocks.AddRange(correctionBlocks
                                            .Where(dataBlock =>
                                                       builder.InsertBoundaryData(bcSets, dataBlock, timeLagString)));

                IBoundaryCondition newBoundaryCondition =
                    bcSets.SelectMany(bcs => bcs.BoundaryConditions).FirstOrDefault();
                if (newBoundaryCondition != null)
                {
                    existingBndForceFileItems[newBoundaryCondition] = delftIniCategory;
                }

                RemoveUsedDataBlocks(usedDataBlocks, signalBlocks, correctionBlocks);

                AddBoundaryConditionsToModelDefinition(modelDefinition, bcSets);
            }
        }

        private static void AddBoundaryConditionsToModelDefinition(WaterFlowFMModelDefinition modelDefinition,
                                                                   List<BoundaryConditionSet> bcSets)
        {
            for (var i = 0; i < bcSets.Count; ++i)
            {
                modelDefinition.BoundaryConditionSets[i].BoundaryConditions.AddRange(bcSets[i].BoundaryConditions);
            }
        }

        private static List<BoundaryConditionSet> CreateBoundaryConditionSetsWithFeature(
            WaterFlowFMModelDefinition modelDefinition)
        {
            return modelDefinition.BoundaryConditionSets
                                  .Select(bcs => new BoundaryConditionSet {Feature = bcs.Feature})
                                  .ToList();
        }

        private static void RemoveUsedDataBlocks(List<BcBlockData> usedDataBlocks, List<BcBlockData> signalBlocks,
                                                 List<BcBlockData> correctionBlocks)
        {
            usedDataBlocks.ForEach(b =>
            {
                signalBlocks.Remove(b);
                correctionBlocks.Remove(b);
            });
        }

        private static BcFileFlowBoundaryDataBuilder CreateFlowBoundaryDataBuilder(FlowBoundaryQuantityType quantity,
                                                                                   Feature2D feature)
        {
            BcFileFlowBoundaryDataBuilder builder;

            List<FlowBoundaryQuantityType> excludedQuantities = Enum.GetValues(typeof(FlowBoundaryQuantityType))
                                                                    .Cast<FlowBoundaryQuantityType>()
                                                                    .Except(new[]
                                                                    {
                                                                        quantity
                                                                    })
                                                                    .ToList();

            if (IsMorphologyRelatedProperty(quantity))
            {
                builder = new BcmFileFlowBoundaryDataBuilder
                {
                    ExcludedQuantities = excludedQuantities,
                    OverwriteExistingData = true,
                    CanCreateNewBoundaryCondition = true,
                    LocationFilter = feature,
                };
            }
            else
            {
                builder = new BcFileFlowBoundaryDataBuilder
                {
                    ExcludedQuantities = excludedQuantities,
                    OverwriteExistingData = true,
                    CanCreateNewBoundaryCondition = true,
                    LocationFilter = feature,
                };
            }

            return builder;
        }

        private static bool TryGetQuantityValue(IDelftIniCategory delftIniCategory,
                                                out FlowBoundaryQuantityType quantity)
        {
            string quantityValue = delftIniCategory.GetPropertyValue(QuantityKey);
            quantity = FlowBoundaryQuantityType.WaterLevel;

            if (string.IsNullOrEmpty(quantityValue)
                || ExtForceQuantNames.TryParseBoundaryQuantityType(quantityValue, out quantity))
            {
                return false;
            }

            if (quantityValue != ExtForceQuantNames.EmbankmentBnd)
            {
                Log.WarnFormat("Could not parse quantity {0} into a valid flow boundary condition", quantityValue);
            }

            return true;
        }

        private static List<BcBlockData> ReadBoundaryConditionBlocks(IEnumerable<string> bcFilePaths)
        {
            var dataBlocks = new List<BcBlockData>();
            foreach (string bcFilePath in bcFilePaths.Distinct())
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

            return dataBlocks;
        }

        private IEnumerable<string> GetForcingFilePathsFromIniCategories(IList<IDelftIniCategory> bndBlocks)
        {
            var bcFilePaths = new List<string>();

            foreach (IDelftIniCategory delftIniCategory in bndBlocks)
            {
                IEnumerable<string> bcFiles = delftIniCategory.GetPropertyValues(ForcingFileKey);
                bcFilePaths.AddRange(bcFiles.Select(GetFullPathForReading));
            }

            return bcFilePaths;
        }

        private static bool IsMorphologyRelatedProperty(FlowBoundaryQuantityType quantity)
        {
            return quantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelFixed
                   || quantity == FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint;
        }

        #endregion
    }
}