using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.IniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class BndExtForceFile
    {
        private static readonly ILogHandler parserLogHandler = new LogHandler(Resources.The_parsing_of_the_boundary_external_forcing_file);
        private readonly BndExtForceFileParser bndExtForceFileParser = new BndExtForceFileParser(parserLogHandler);
        private readonly LateralFactory lateralFactory = new LateralFactory();

        public void Read(string filePath, string referenceFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            bndExtFilePath = filePath;
            bndExtSubFilesReferenceFilePath = referenceFilePath;

            IniData iniData;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                iniData = new MduIniReader().ReadIniFile(fileStream, filePath);
                RemoveRedundantProperties(iniData, modelDefinition);
                UpdateLegacyNames(iniData);
            }

            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            ReadPolyLines(bndExtForceFileDTO, modelDefinition);
            List<BcBlockData> dataBlocks = GetBcBlockDataFromForcingFiles(bndExtForceFileDTO);
            ReadBoundaryConditions(bndExtForceFileDTO, modelDefinition, dataBlocks);
            ReadLaterals(bndExtForceFileDTO, modelDefinition, dataBlocks);
            
            parserLogHandler.LogReport();
        }

        private void ReadLaterals(BndExtForceFileDTO bndExtForceFileDTO, WaterFlowFMModelDefinition modelDefinition, IEnumerable<BcBlockData> dataBlocks)
        {
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(parserLogHandler, dataBlocks);
            
            foreach (LateralDTO lateralDTO in bndExtForceFileDTO.Laterals)
            {
                Lateral lateral = lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

                existingLateralItems[lateral] = lateralDTO;
                
                modelDefinition.Laterals.Add(lateral);
                modelDefinition.LateralFeatures.Add(lateral.Feature);
            }
        }

        private static void RemoveRedundantProperties(IniData iniData, WaterFlowFMModelDefinition definition)
        {
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());
            iniData.Sections.ForEach(section =>
            {
                section.RemoveAllProperties(p => definition.ContainsProperty(p.Key) && p.Value == string.Empty);
                section.RemoveAllProperties(p => backwardsCompatibilityHelper.IsObsoletePropertyKey(p.Key));
            });
        }

        private static void UpdateLegacyNames(IniData iniData)
        {
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());

            foreach (IniSection section in iniData.Sections)
            {
                foreach (string propertyKey in section.Properties.Select(x => x.Key).ToList())
                {
                    string updatedPropertyKey = backwardsCompatibilityHelper.GetUpdatedPropertyKey(propertyKey);
                    if (updatedPropertyKey != null)
                    {
                        section.RenameProperties(propertyKey, updatedPropertyKey);
                    }
                }
            }
        }

        private string GetFullPathForReading(string relativePath)
        {
            return GetOtherFilePathInSameDirectory(bndExtSubFilesReferenceFilePath, relativePath);
        }

        private void ReadPolyLines(BndExtForceFileDTO bndExtForceFileDTO, WaterFlowFMModelDefinition modelDefinition)
        {
            modelDefinition.Boundaries.ForEach(b => { existingPolyLineFiles[b] = b.Name + FileConstants.PliFileExtension; });

            foreach (var locationFile in bndExtForceFileDTO.LocationFiles)
            {
                bool locationFileHasAlreadyBeenRead = existingPolyLineFiles.Values.Contains(locationFile);
                if (locationFile == null || locationFileHasAlreadyBeenRead)
                {
                    continue;
                }

                if (locationFile == string.Empty)
                {
                    log.WarnFormat("Empty location file encountered in boundary ext-force file {0}", bndExtFilePath);
                    continue;
                }

                string pliFilePath = GetFullPathForReading(locationFile);
                CheckFilePath(pliFilePath, $"Boundary location file {pliFilePath} not found");

                ReadPliFile(modelDefinition, pliFilePath, locationFile);
            }
        }

        private void ReadPliFile(WaterFlowFMModelDefinition modelDefinition, string pliFilePath, string locationFile)
        {
            var pliFile = new PliFile<Feature2D>();
            IList<Feature2D> features = pliFile.Read(pliFilePath);

            foreach (Feature2D feature in features)
            {
                existingPolyLineFiles[feature] = locationFile;
                modelDefinition.Boundaries.Add(feature);
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet { Feature = feature });
            }
        }

        private static void CheckFilePath(string filePath, string warningMessage)
        {
            if (!File.Exists(filePath))
            {
                log.Warn(warningMessage);
            }
        }

        private void ReadBoundaryConditions(BndExtForceFileDTO bndExtForceFileDTO,
                                            WaterFlowFMModelDefinition modelDefinition,
                                            List<BcBlockData> dataBlocks)
        {
            List<string> correctionFunctionTypes = BcFileFlowBoundaryDataBuilder.CorrectionFunctionTypes.ToList();

            List<BcBlockData> correctionBlocks =
                dataBlocks.Where(db => correctionFunctionTypes.Contains(db.FunctionType)).ToList();

            List<BcBlockData> signalBlocks = dataBlocks.Except(correctionBlocks).ToList();

            foreach (BoundaryDTO boundaryDTO in bndExtForceFileDTO.Boundaries)
            {
                if (TryGetQuantityValue(boundaryDTO.Quantity, out FlowBoundaryQuantityType quantity))
                {
                    continue;
                }

                string pliFile = boundaryDTO.LocationFile;

                Feature2D feature = existingPolyLineFiles.FirstOrDefault(kvp => kvp.Value == pliFile).Key;

                if (feature == null)
                {
                    continue;
                }

                BcFileFlowBoundaryDataBuilder builder = CreateFlowBoundaryDataBuilder(quantity, feature);

                List<BoundaryConditionSet> bcSets = CreateBoundaryConditionSetsWithFeature(modelDefinition);

                // first loading signals, then corrections

                var usedDataBlocks = new List<BcBlockData>();
                usedDataBlocks.AddRange(signalBlocks
                                            .Where(dataBlock =>
                                                       builder.InsertBoundaryData(bcSets, dataBlock, boundaryDTO.ReturnTime)));
                usedDataBlocks.AddRange(correctionBlocks
                                            .Where(dataBlock =>
                                                       builder.InsertBoundaryData(bcSets, dataBlock, boundaryDTO.ReturnTime)));

                IBoundaryCondition newBoundaryCondition =
                    bcSets.SelectMany(bcs => bcs.BoundaryConditions).FirstOrDefault();
                if (newBoundaryCondition != null)
                {
                    existingBoundaryItems[newBoundaryCondition] = boundaryDTO;
                }

                RemoveUsedDataBlocks(usedDataBlocks, signalBlocks, correctionBlocks);

                AddBoundaryConditionsToModelDefinition(modelDefinition, bcSets);
            }
        }

        private List<BcBlockData> GetBcBlockDataFromForcingFiles(BndExtForceFileDTO bndExtForceFileDTO)
        {
            IEnumerable<string> bcFilePaths = GetForcingFilePathsFromIniCategories(bndExtForceFileDTO);
            return ReadBoundaryConditionBlocks(bcFilePaths);
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
                                  .Select(bcs => new BoundaryConditionSet { Feature = bcs.Feature })
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
                                                                    .Except(quantity)
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

        private static bool TryGetQuantityValue(string quantityValue,
                                                out FlowBoundaryQuantityType quantity)
        {
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
            foreach (string bcFilePath in bcFilePaths)
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

        private IEnumerable<string> GetForcingFilePathsFromIniCategories(BndExtForceFileDTO bndExtForceFileDTO)
        {
            return bndExtForceFileDTO.ForcingFiles.Select(GetFullPathForReading);
        }

        private static bool IsMorphologyRelatedProperty(FlowBoundaryQuantityType quantity)
        {
            return quantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                   || quantity == FlowBoundaryQuantityType.MorphologyBedLevelFixed
                   || quantity == FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint;
        }
    }
}