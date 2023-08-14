using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class BndExtForceFile
    {
        private const BcFile.WriteMode bcFileWriteMode = BcFile.WriteMode.FilePerQuantity;
        private const BcFile.WriteMode bcmFileWriteMode = BcFile.WriteMode.SingleFile;
        private BoundarySerializer boundarySerializer = new BoundarySerializer();
        private readonly LateralSerializer lateralSerializer = new LateralSerializer();


        private string bndExtFilePath;

        public bool WriteToDisk { get; set; }

        public void Write(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            ModelProperty modelProperty = modelDefinition.GetModelProperty(KnownProperties.BndExtForceFile);
            bndExtFilePath = filePath;
            IList<DelftIniCategory> bndExtForceFileItems = WriteBndExtForceFileSubFiles(modelDefinition);

            IList<DelftIniCategory> lateralCategories = new List<DelftIniCategory>();
            foreach (var lateral in modelDefinition.Laterals)
            {
                DelftIniCategory lateralCategory = lateralSerializer.Serialize(lateral);
                lateralCategories.Add(lateralCategory);
                
            }
            if (bndExtForceFileItems.Any() || lateralCategories.Any())
            {
                WriteBndExtForceFile(bndExtForceFileItems, lateralCategories);
                modelProperty.SetValueAsString(Path.GetFileName(bndExtFilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(bndExtFilePath);
                modelProperty.SetValueAsString(string.Empty);
            }
        }

        public IList<DelftIniCategory> WriteBndExtForceFileSubFiles(WaterFlowFMModelDefinition modelDefinition)
        {
            DateTime refDate = modelDefinition.GetReferenceDateAsDateTime();
            
            WritePolyLines(modelDefinition.BoundaryConditionSets);
            WriteLateralBcFiles(modelDefinition.Laterals, refDate);
            return WriteBoundaryBcFiles(modelDefinition.ModelName, modelDefinition.BoundaryConditionSets, refDate);
        }

        private IList<DelftIniCategory> WriteBoundaryBcFiles(string modelDefinitionModelName, IList<BoundaryConditionSet> boundaryConditionSets, DateTime refDate)
        {
            List<DelftIniCategory> resultingItems =
                boundaryConditionSets.Where(bcs => !bcs.BoundaryConditions.Any())
                                     .Select(boundaryConditionSet => existingPolyLineFiles.TryGetValue(boundaryConditionSet.Feature, out string pliFileName)
                                                                         ? DelftIniCategoryFactory.CreateBoundaryBlock(null, pliFileName, null, TimeSpan.Zero)
                                                                         : null).Where(it => it != null)
                                     .ToList();

            // Write all morphology boundaries in one file.
            var bcmFile = new BcmFile { MultiFileMode = bcmFileWriteMode };
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> morphologyGroupings =
                bcmFile.GroupBoundaryConditions(boundaryConditionSets);

            WriteBoundaryConditions(refDate, bcmFile, morphologyGroupings, new BcmFileFlowBoundaryDataBuilder(),
                                    modelDefinitionModelName);
            // No longer return the morphology groupings since they will not be written to the .ext file (DELFT3DFM-1106)

            var bcFile = new BcFile { MultiFileMode = bcFileWriteMode };
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> standardGroupings =
                bcFile.GroupBoundaryConditions(boundaryConditionSets);
            resultingItems.AddRange(WriteBoundaryConditions(refDate, bcFile, standardGroupings,
                                                            new BcFileFlowBoundaryDataBuilder(),
                                                            modelDefinitionModelName).Distinct());

            return resultingItems;
        }
        
        private void WriteLateralBcFiles(IEnumerable<Lateral> laterals, DateTime refDate)
        {
            if (!laterals.Any())
            {
                return;
            }

            IEnumerable<Lateral> timeDependentDischargeLaterals = laterals.Where(HasTimeSeriesDischarge);
            WriteLateralDischargeBcFiles(refDate, timeDependentDischargeLaterals);
        }

        private static bool HasTimeSeriesDischarge(Lateral lateral) => lateral.Data.Discharge.Type == LateralDischargeType.TimeSeries;

        private void WriteLateralDischargeBcFiles(DateTime refDate, IEnumerable<Lateral> timeDependentDischargeLaterals)
        {
            var fileName = $"{BcFileConstants.LateralDischargeQuantityName}.bc";
            string filePath = GetFullPathForWriting(fileName);

            var bcFile = new BcFile { MultiFileMode = bcFileWriteMode };
            bcFile.WriteLateralData(timeDependentDischargeLaterals, filePath, new BcFileFlowBoundaryDataBuilder(), refDate);
        }

        private string GetFullPathForWriting(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(bndExtFilePath), relativePath);
        }

        private void WriteBndExtForceFile(IEnumerable<DelftIniCategory> bndExtForceFileItems, IEnumerable<DelftIniCategory> lateralCategories)
        {
            OpenOutputFile(bndExtFilePath);
            try
            {
                WriteBoundaryCategories(bndExtForceFileItems);
                WriteLateralCategories(lateralCategories);

            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteLateralCategories(IEnumerable<DelftIniCategory> lateralCategories)
        {
            foreach (DelftIniCategory lateralCategory in lateralCategories)
            {
                WriteLine("");
                WriteLine($"[{lateralCategory.Name}]");
                foreach (DelftIniProperty property in lateralCategory.Properties)
                {
                    WritePropertyValue(property.Name, property.Value);
                }
            }
        }

        private void WriteBoundaryCategories(IEnumerable<DelftIniCategory> bndExtForceFileItems)
        {
            foreach (DelftIniCategory bndExtForceFileItem in bndExtForceFileItems)
            {
                WriteLine("");
                WriteLine($"[{bndExtForceFileItem.Name}]");
                WritePropertyValue(BndExtForceFileConstants.QuantityKey, bndExtForceFileItem);
                WritePropertyValue(BndExtForceFileConstants.LocationFileKey, bndExtForceFileItem);

                string openBoundaryToleranceProperty = bndExtForceFileItem.GetPropertyValues(BndExtForceFileConstants.OpenBoundaryToleranceKey)
                                                                          .FirstOrDefault();
                if (openBoundaryToleranceProperty != null)
                {
                    WritePropertyValue(BndExtForceFileConstants.OpenBoundaryToleranceKey, openBoundaryToleranceProperty);
                }

                WritePropertyValues(BndExtForceFileConstants.ForcingFileKey, bndExtForceFileItem);
                WritePropertyValueIfNotNull(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey, bndExtForceFileItem);
                WritePropertyValueIfNotNull(areaKey, bndExtForceFileItem);
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

        private static string AddExtension(string fileName, BcFile bcFile)
        {
            string extension = bcFile is BcmFile
                                   ? BcmFile.Extension
                                   : BcFile.Extension;

            return AddExtension(fileName, extension);
        }

        private static string AddExtension(string fileName, string extension)
        {
            string cleanFileName = fileName.TrimEnd('.');
            string cleanExtension = extension.TrimStart('.');
            return string.Concat(cleanFileName, ".", cleanExtension);
        }

        private IEnumerable<DelftIniCategory> WriteBoundaryConditions(
            DateTime refDate, BcFile bcFile,
            IEnumerable<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> grouping,
            BcFileFlowBoundaryDataBuilder boundaryDataBuilder, string modelDefinitionName)
        {
            var resultingItems = new List<BoundaryDTO>();

            var fileNamesToBoundaryConditions =
                new Dictionary<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>>();

            foreach (IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>> group in grouping)
            {
                string fileName = group.Key;

                foreach (Tuple<IBoundaryCondition, BoundaryConditionSet> tuple in group.Where(t => t.Item1 is FlowBoundaryCondition))
                {
                    existingBndForceFileItems.TryGetValue(tuple.Item1, out BoundaryDTO existingBlock);

                    List<string> existingPaths = existingBlock != null
                                                     ? existingBlock.ForcingFiles.ToList()
                                                     : new List<string>();

                    if (string.IsNullOrEmpty(fileName) && bcFile.MultiFileMode == BcFile.WriteMode.SingleFile)
                    {
                        fileName = modelDefinitionName;
                    }

                    string path = existingPaths.Any()
                                      ? existingPaths.First()
                                      : AddExtension(fileName, bcFile);

                    if (existingBlock != null && !existingPaths.Contains(path))
                    {
                        existingBlock.AddForcingFile(path);
                    }

                    string corrPath = existingPaths.Count > 1
                                          ? existingPaths[1]
                                          : AddExtension(fileName + "_corr", BcFile.Extension);

                    if (existingBlock != null)
                    {
                        // set thatcher harlemann time lag once it is already existent in the ext force file but it has changed.
                        var condition = (FlowBoundaryCondition)tuple.Item1;
                        existingBlock.ReturnTime = condition.ThatcherHarlemanTimeLag.TotalSeconds;

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType) && !existingPaths.Contains(corrPath))
                        {
                            existingBlock.AddForcingFile(corrPath);
                        }

                        if (!BcFile.IsCorrectionType(tuple.Item1.DataType) && existingPaths.Contains(corrPath))
                        {
                            existingBlock.RemoveForcingFile(corrPath);
                        }
                    }

                    AddFileNamesToBoundaryConditions(fileNamesToBoundaryConditions, path, tuple);

                    if (BcFile.IsCorrectionType(tuple.Item1.DataType))
                    {
                        AddFileNamesToBoundaryConditions(fileNamesToBoundaryConditions, corrPath, tuple);
                    }

                    if (existingBlock == null)
                    {
                        string quantityName = ExtForceQuantNames.GetQuantityString((FlowBoundaryCondition)tuple.Item1);

                        string pliFileName = existingPolyLineFiles[tuple.Item2.Feature];


                        var thatcherHarlemanTimeLag = ((FlowBoundaryCondition)tuple.Item1).ThatcherHarlemanTimeLag;
                        double? returnTime = thatcherHarlemanTimeLag != TimeSpan.Zero ? thatcherHarlemanTimeLag.TotalSeconds : (double?)null;
                        var boundaryDTO = new BoundaryDTO(quantityName, pliFileName, new[] {path}, returnTime);

                        if (BcFile.IsCorrectionType(tuple.Item1.DataType))
                        {
                            boundaryDTO.AddForcingFile(corrPath);
                        }

                        resultingItems.Add(boundaryDTO);
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

            return resultingItems.Select(boundarySerializer.Serialize);
        }

        private static void AddFileNamesToBoundaryConditions(
            IDictionary<string, IList<Tuple<IBoundaryCondition, BoundaryConditionSet>>> fileNamesToBoundaryConditions,
            string path, Tuple<IBoundaryCondition, BoundaryConditionSet> tuple)
        {
            if (fileNamesToBoundaryConditions.TryGetValue(path, out IList<Tuple<IBoundaryCondition, BoundaryConditionSet>> tuples))
            {
                tuples.Add(tuple);
            }
            else
            {
                tuples = new List<Tuple<IBoundaryCondition, BoundaryConditionSet>> { tuple };
                fileNamesToBoundaryConditions.Add(path, tuples);
            }
        }
    }
}