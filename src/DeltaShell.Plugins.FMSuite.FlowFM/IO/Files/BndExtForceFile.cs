using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class BndExtForceFile : NGHSFileBase
    {
        public const string BoundaryBlockKey = "[boundary]";
        public const string QuantityKey = "quantity";
        public const string LocationFileKey = "locationfile";
        public const string ForcingFileKey = "forcingfile";

        private const string areaKey = "area";
        private const string thatcherHarlemanTimeLagKey = "return_time";
        private const string openBoundaryToleranceKey = "OpenBoundaryTolerance";

        private const double openBoundaryTolerance = 0.5;

        private const BcFile.WriteMode bcFileWriteMode = BcFile.WriteMode.FilePerQuantity;
        private const BcFile.WriteMode bcmFileWriteMode = BcFile.WriteMode.SingleFile;

        private static readonly ILog log = LogManager.GetLogger(typeof(BndExtForceFile));

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolyLineFiles;
        private readonly IDictionary<IBoundaryCondition, DelftIniCategory> existingBndForceFileItems;

        private string bndExtFilePath;

        private string bndExtSubFilesReferenceFilePath;

        public BndExtForceFile()
        {
            existingPolyLineFiles = new Dictionary<Feature2D, string>();
            existingBndForceFileItems = new Dictionary<IBoundaryCondition, DelftIniCategory>();
            WriteToDisk = true;
        }

        public bool WriteToDisk { get; set; }

        private static DelftIniCategory CreateBoundaryBlock(string quantity, string locationFilePath,
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
                block.AddProperty(thatcherHarlemanTimeLagKey, thatcherHarlemanTimeLag.TotalSeconds);
            }

            if (isEmbankment)
            {
                block.AddProperty(openBoundaryToleranceKey, openBoundaryTolerance);
            }

            return block;
        }

        private string GetFullPathForReading(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(bndExtSubFilesReferenceFilePath), relativePath);
        }

        private string GetFullPathForWriting(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(bndExtFilePath), relativePath);
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

        private void Write(string filePath, string modelDefinitionModelName, IList<BoundaryConditionSet> boundaryConditionSets,
                           IEnumerable<Embankment> embankments, ModelProperty modelProperty, DateTime refDate)
        {
            bndExtFilePath = filePath;
            IList<DelftIniCategory> bndExtForceFileItems =
                WriteBndExtForceFileSubFiles(modelDefinitionModelName, boundaryConditionSets, refDate);
            IEnumerable<DelftIniCategory> embankmentForceFileItems = WriteEmbankmentFiles(embankments);

            List<DelftIniCategory> allItems = bndExtForceFileItems.Concat(embankmentForceFileItems).ToList();
            if (allItems.Count > 0)
            {
                WriteBndExtForceFile(allItems);
                modelProperty.SetValueAsString(Path.GetFileName(bndExtFilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(bndExtFilePath);
                modelProperty.SetValueAsString(string.Empty);
            }
        }

        private void WriteBndExtForceFile(IEnumerable<DelftIniCategory> bndExtForceFileItems)
        {
            OpenOutputFile(bndExtFilePath);
            try
            {
                foreach (DelftIniCategory bndExtForceFileItem in bndExtForceFileItems)
                {
                    WriteLine("");
                    WriteLine(BoundaryBlockKey);
                    WritePropertyValue(QuantityKey, bndExtForceFileItem);
                    WritePropertyValue(LocationFileKey, bndExtForceFileItem);

                    string openBoundaryToleranceProperty = bndExtForceFileItem.GetPropertyValues(openBoundaryToleranceKey)
                                                                              .FirstOrDefault();
                    if (openBoundaryToleranceProperty != null)
                    {
                        WritePropertyValue(openBoundaryToleranceKey, openBoundaryToleranceProperty);
                    }

                    WritePropertyValues(ForcingFileKey, bndExtForceFileItem);
                    WritePropertyValueIfNotNull(thatcherHarlemanTimeLagKey, bndExtForceFileItem);
                    WritePropertyValueIfNotNull(areaKey, bndExtForceFileItem);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WritePropertyValues(string propertyName, DelftIniCategory bndExtForceFileItem)
        {
            foreach (string propertyValue in bndExtForceFileItem.GetPropertyValues(propertyName))
            {
                WritePropertyValue(propertyName, propertyValue);
            }
        }

        private void WritePropertyValueIfNotNull(string propertyName, DelftIniCategory bndExtForceFileItem)
        {
            string propertyValue = bndExtForceFileItem.GetPropertyValue(propertyName);
            if (propertyValue != null)
            {
                WritePropertyValue(propertyName, propertyValue);
            }
        }

        private void WritePropertyValue(string propertyName, DelftIniCategory bndExtForceFileItem)
        {
            WritePropertyValue(propertyName, bndExtForceFileItem.GetPropertyValue(propertyName));
        }

        private void WritePropertyValue(string propertyName, string propertyValue)
        {
            WriteLine($"{propertyName}={propertyValue}");
        }

        // TODO: migrate sources & sinks to new format

        public IList<DelftIniCategory> WriteBndExtForceFileSubFiles(string modelDefinitionModelName,
                                                                    IList<BoundaryConditionSet> boundaryConditionSets,
                                                                    DateTime refDate)
        {
            WritePolyLines(boundaryConditionSets);

            List<DelftIniCategory> resultingItems =
                boundaryConditionSets.Where(bcs => !bcs.BoundaryConditions.Any())
                                     .Select(boundaryConditionSet => existingPolyLineFiles.TryGetValue(
                                                                         boundaryConditionSet.Feature, out string pliFileName)
                                                                         ? CreateBoundaryBlock(
                                                                             null, pliFileName, null, TimeSpan.Zero)
                                                                         : null).Where(it => it != null)
                                     .ToList();

            /* Write all morphology boundaries in one file.*/
            var bcmFile = new BcmFile
            {
                MultiFileMode = bcmFileWriteMode
            };
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> morphologyGroupings =
                bcmFile.GroupBoundaryConditions(boundaryConditionSets);

            WriteBoundaryConditions(refDate, bcmFile, morphologyGroupings, new BcmFileFlowBoundaryDataBuilder(),
                                    modelDefinitionModelName);
            /* No longer return the morphology groupings since they will not be written to the .ext file (DELFT3DFM-1106) */

            var bcFile = new BcFile
            {
                MultiFileMode = bcFileWriteMode
            };
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> standardGroupings =
                bcFile.GroupBoundaryConditions(boundaryConditionSets);
            resultingItems.AddRange(WriteBoundaryConditions(refDate, bcFile, standardGroupings,
                                                            new BcFileFlowBoundaryDataBuilder(),
                                                            modelDefinitionModelName).Distinct());

            return resultingItems;
        }

        private IEnumerable<DelftIniCategory> WriteEmbankmentFiles(IEnumerable<Embankment> embankments)
        {
            var categories = new List<DelftIniCategory>();

            foreach (Embankment embankment in embankments)
            {
                if (!existingPolyLineFiles.TryGetValue(embankment, out string existingFile))
                {
                    existingFile = embankment.Name + FileConstants.EmbankmentFileExtension;
                    existingPolyLineFiles[embankment] = existingFile;
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
                if (!existingPolyLineFiles.TryGetValue(boundaryConditionSet.Feature, out string existingFile))
                {
                    existingFile = ExtForceFileHelper.GetPliFileName(boundaryConditionSet);
                    if (string.IsNullOrEmpty(existingFile))
                    {
                        return;
                    }

                    existingPolyLineFiles[boundaryConditionSet.Feature] = existingFile;
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
            string cleanFileName = fileName.TrimEnd('.');
            string cleanExtension = extension.TrimStart('.');
            return string.Concat(cleanFileName, ".", cleanExtension);
        }

        private IEnumerable<DelftIniCategory> WriteBoundaryConditions(DateTime refDate, BcFile bcFile,
                                                                      IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> grouping,
                                                                      BcFileFlowBoundaryDataBuilder boundaryDataBuilder,
                                                                      string modelDefinitionName)
        {
            var resultingItems = new List<DelftIniCategory>();

            var fileNamesToBoundaryConditions =
                new Dictionary<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>>();

            foreach (IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>> group in grouping)
            {
                foreach (Tuple<IBoundaryCondition, BoundaryConditionSet> tuple in group.Where(
                    t => t.Item1 is FlowBoundaryCondition))
                {
                    existingBndForceFileItems.TryGetValue(tuple.Item1, out DelftIniCategory existingBlock);

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
                        existingBlock.SetProperty(thatcherHarlemanTimeLagKey,
                                                  condition.ThatcherHarlemanTimeLag.TotalSeconds);

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType) && !existingPaths.Contains(corrPath))
                        {
                            existingBlock.AddProperty(ForcingFileKey, corrPath);
                        }

                        if (!BcFile.IsCorrectionType(tuple.Item1.DataType) && existingPaths.Contains(corrPath))
                        {
                            existingBlock.RemoveAllPropertiesWhere(p => p.Value == corrPath);
                        }
                    }

                    if (fileNamesToBoundaryConditions.TryGetValue(
                        path, out IList<Tuple<IBoundaryCondition, BoundaryConditionSet>> tuples))
                    {
                        tuples.Add(tuple);
                    }
                    else
                    {
                        tuples = new List<Tuple<IBoundaryCondition, BoundaryConditionSet>>
                        {
                            tuple
                        };
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
                            tuples = new List<Tuple<IBoundaryCondition, BoundaryConditionSet>>
                            {
                                tuple
                            };
                            fileNamesToBoundaryConditions.Add(corrPath, tuples);
                        }
                    }

                    if (existingBlock == null)
                    {
                        string quantityName =
                            ExtForceQuantNames.GetQuantityString((FlowBoundaryCondition) tuple.Item1);

                        string pliFileName = existingPolyLineFiles[tuple.Item2.Feature];

                        DelftIniCategory bndBlock = CreateBoundaryBlock(quantityName, pliFileName, path,
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

        public void Read(string bndExtForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                         string bndExtForceSubFilesReferenceFilePath)
        {
            bndExtFilePath = bndExtForceFilePath;
            bndExtSubFilesReferenceFilePath = bndExtForceSubFilesReferenceFilePath;

            IList<DelftIniCategory> bndBlocks;
            using (var fileStream = new FileStream(bndExtForceFilePath, FileMode.Open, FileAccess.Read))
            {
                bndBlocks = new DelftIniReader().ReadDelftIniFile(fileStream, bndExtForceFilePath);
            }

            ReadPolyLines(bndBlocks, modelDefinition);

            ReadBoundaryConditions(bndBlocks, modelDefinition);
        }

        private void ReadPolyLines(IEnumerable<DelftIniCategory> bndBlocks, WaterFlowFMModelDefinition modelDefinition)
        {
            modelDefinition.Boundaries.ForEach(b =>
            {
                existingPolyLineFiles[b] = b.Name + FileConstants.PliFileExtension;
            });

            foreach (DelftIniCategory delftIniCategory in bndBlocks)
            {
                string locationFile = delftIniCategory.GetPropertyValue(LocationFileKey);
                bool locationFileHasAlreadyBeenRead = existingPolyLineFiles.Values.Contains(locationFile);
                if (locationFile == null || locationFileHasAlreadyBeenRead)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(locationFile))
                {
                    log.WarnFormat("Empty location file encountered in boundary ext-force file {0}", bndExtFilePath);
                    continue;
                }

                string pliFilePath = GetFullPathForReading(locationFile);

                if (!File.Exists(pliFilePath))
                {
                    log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                }

                if (IsEmbankmentCategory(delftIniCategory))
                {
                    var plizFile = new PlizFile<Embankment>();
                    IList<Embankment> embankments = plizFile.Read(pliFilePath);
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
                    var pliFile = new PliFile<Feature2D>();
                    IList<Feature2D> features = pliFile.Read(pliFilePath);
                    if (!features.Any())
                    {
                        continue;
                    }

                    foreach (Feature2D feature in features)
                    {
                        existingPolyLineFiles[feature] = locationFile;
                        modelDefinition.Boundaries.Add(feature);
                        modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet
                        {
                            Feature = feature
                        });
                    }
                }
            }
        }

        private static void LogWarningMessagesForOnePointGeometryEmbankments(
            IEnumerable<Embankment> embankments, string pliFilePath)
        {
            IEnumerable<Embankment> onePointEmbankments = embankments.Where(e => e.Geometry.Coordinates.Length == 1);
            string directory = Directory.GetParent(pliFilePath).FullName;
            onePointEmbankments.ForEach(e =>
            {
                string embankmentFilePath = Path.Combine(directory, $"{e.Name}{FileConstants.EmbankmentFileExtension}");
                log.Warn(
                    $"Embankment file '{embankmentFilePath}' with only 1 point detected and it will not be imported.");
            });
        }

        private static bool IsEmbankmentCategory(DelftIniCategory delftIniCategory)
        {
            return delftIniCategory.GetPropertyValue(QuantityKey) == ExtForceQuantNames.EmbankmentBnd;
        }

        private void ReadBoundaryConditions(IList<DelftIniCategory> delftIniCategories,
                                            WaterFlowFMModelDefinition modelDefinition)
        {
            List<string> correctionFunctionTypes = BcFileFlowBoundaryDataBuilder.CorrectionFunctionTypes.ToList();

            IEnumerable<string> bcFilePaths = GetForcingFilePathsFromIniCategories(delftIniCategories);

            List<BcBlockData> dataBlocks = ReadBoundaryConditionBlocks(bcFilePaths);

            List<BcBlockData> correctionBlocks =
                dataBlocks.Where(db => correctionFunctionTypes.Contains(db.FunctionType)).ToList();

            List<BcBlockData> signalBlocks = dataBlocks.Except(correctionBlocks).ToList();

            foreach (DelftIniCategory delftIniCategory in delftIniCategories)
            {
                if (TryGetQuantityValue(delftIniCategory, out FlowBoundaryQuantityType quantity))
                {
                    continue;
                }

                string pliFile = delftIniCategory.GetPropertyValue(LocationFileKey);

                Feature2D feature = existingPolyLineFiles.FirstOrDefault(kvp => kvp.Value == pliFile).Key;

                if (feature == null)
                {
                    continue;
                }

                BcFileFlowBoundaryDataBuilder builder = CreateFlowBoundaryDataBuilder(quantity, feature);

                List<BoundaryConditionSet> bcSets = CreateBoundaryConditionSetsWithFeature(modelDefinition);

                // first loading signals, then corrections

                string timeLagString = delftIniCategory.GetPropertyValue(thatcherHarlemanTimeLagKey);

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
                                  .Select(bcs => new BoundaryConditionSet
                                  {
                                      Feature = bcs.Feature
                                  })
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
                                                                                   IFeature feature)
        {
            List<FlowBoundaryQuantityType> excludedQuantities = Enum.GetValues(typeof(FlowBoundaryQuantityType))
                                                                    .Cast<FlowBoundaryQuantityType>()
                                                                    .Except(new[]
                                                                    {
                                                                        quantity
                                                                    })
                                                                    .ToList();

            BcFileFlowBoundaryDataBuilder builder = IsMorphologyRelatedProperty(quantity)
                                                        ? new BcmFileFlowBoundaryDataBuilder()
                                                        : new BcFileFlowBoundaryDataBuilder();

            builder.ExcludedQuantities = excludedQuantities;
            builder.OverwriteExistingData = true;
            builder.CanCreateNewBoundaryCondition = true;
            builder.LocationFilter = feature;

            return builder;
        }

        private static bool TryGetQuantityValue(DelftIniCategory delftIniCategory,
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
                log.WarnFormat("Could not parse quantity {0} into a valid flow boundary condition", quantityValue);
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
                        log.WarnFormat("Boundary condition data file {0} not found", bcFilePath);
                    }

                    continue;
                }

                dataBlocks.AddRange(bcFilePath.EndsWith(".bcm")
                                        ? new BcmFile().Read(bcFilePath)
                                        : new BcFile().Read(bcFilePath));
            }

            return dataBlocks;
        }

        private IEnumerable<string> GetForcingFilePathsFromIniCategories(IList<DelftIniCategory> bndBlocks)
        {
            var bcFilePaths = new List<string>();

            foreach (DelftIniCategory delftIniCategory in bndBlocks)
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