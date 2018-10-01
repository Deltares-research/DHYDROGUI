using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public static class StructureFileWriter
    {
        public static void WriteFile(string targetIniFile, IHydroNetwork network)
        {
            WriteFile(targetIniFile, network, null, DateTime.MinValue);
        }

        public static void WriteFile(string targetIniFile, IHydroNetwork network, HydroArea hydroArea, DateTime referenceDateTime)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.StructureDefinitionsMajorVersion, 
                                                             GeneralRegion.StructureDefinitionsMinorVersion, 
                                                             GeneralRegion.FileTypeName.StructureDefinition)
            };

            var lastCompositeStructureId = 0;
            var compositeStructures = network.Structures.Where(s => s.GetStructureType() == StructureType.CompositeBranchStructure).Cast<ICompositeBranchStructure>();
            foreach (var composite in compositeStructures) // Note: In DeltaShell all Structures belong to a CompositeBranchStructure, even if they are alone
            {
                var currentCompositeStructureId = composite.Structures.Count > 1 ? ++lastCompositeStructureId : 0;

                foreach (var structure in composite.Structures)
                {
                    var structureType = structure.GetStructureType();
                    var compositeStructureInfo = new CompoundStructureInfo(currentCompositeStructureId, composite.Name);
                    var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structureType, compositeStructureInfo);

                    var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure);
                    var structurefrictionData = structure as IFrictionData;
                    var structureGroundLayerData = structure as IGroundLayer;
                    if (structurefrictionData != null && structureGroundLayerData != null)
                    {
                        AddFrictionData(
                            structureCategory,
                            structurefrictionData.FrictionDataType,
                            structurefrictionData.Friction,
                            structureGroundLayerData.GroundLayerEnabled ? structureGroundLayerData.GroundLayerRoughness : structurefrictionData.Friction);
                    }

                    categories.Add(structureCategory);
                }
            }

            if (hydroArea != null)
            {
                foreach (var structure2D in hydroArea.AllHydroObjects.Cast<IStructure2D>())
                {
                    var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structure2D.Structure2DType);
                    var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure2D);
                    categories.Add(structureCategory);

                    WriteHydroObjectSpecificFiles(targetIniFile, referenceDateTime, structureCategory, structure2D);
                }
            }
            if (File.Exists(targetIniFile)) File.Delete(targetIniFile);
            new IniFileWriter().WriteIniFile(categories, targetIniFile);
        }

        private static void WriteHydroObjectSpecificFiles(string targetFile, DateTime referenceDateTime, DelftIniCategory structureCategory, IStructure2D structure2D)
        {
            var polylineFileProperty = structureCategory.Properties.FirstOrDefault(p => p.Name == StructureRegion.PolylineFile.Key);
            if (polylineFileProperty != null)
            {
                var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(targetFile, polylineFileProperty.Value);
                WritePolyLineFile(pliFilePath, structure2D);
            }

            var timFileCategory = structureCategory.Properties.FirstOrDefault(p => p.Name == StructureRegion.Capacity.Key);
            if (timFileCategory != null)
            {
                var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(targetFile, timFileCategory.Value);
                WriteTimeSeriesFile(timFilePath, structure2D, referenceDateTime);
            }
        }

        private static void WritePolyLineFile(string pliFilePath, IStructure2D structure2D)
        {
            var geometryObjectsToBeWritten = new[]
            {
                new Feature2D
                {
                    Name = structure2D.Name,
                    Geometry = structure2D.Geometry
                }
            };
            new PliFile<Feature2D>().Write(pliFilePath, geometryObjectsToBeWritten);
        }

        private static void WriteTimeSeriesFile(string timFilePath, IStructure2D structure2D, DateTime referenceDateTime)
        {
            var pump = structure2D as IPump;
            if (pump != null && pump.HasCapacityTimeSeries())
            {
                new TimFile().Write(timFilePath, pump.CapacityTimeSeries, referenceDateTime);
            }
        }

        private static bool HasCapacityTimeSeries(this IPump pump)
        {
            return pump.CanBeTimedependent 
                   && pump.UseCapacityTimeSeries 
                   && pump.CapacityTimeSeries != null;
        }

        private static void AddFrictionData(DelftIniCategory category, Friction frictionType, double friction, double groundLayerRoughness)
        {
            category.AddProperty(StructureRegion.BedFrictionType.Key, (int)frictionType, StructureRegion.BedFrictionType.Description);
            category.AddProperty(StructureRegion.BedFriction.Key, friction, StructureRegion.BedFriction.Description, StructureRegion.BedFriction.Format);
            category.AddProperty(StructureRegion.GroundFrictionType.Key, (int)frictionType, StructureRegion.GroundFrictionType.Description); // This may be removed, but for now just duplicate
            category.AddProperty(StructureRegion.GroundFriction.Key, groundLayerRoughness, StructureRegion.GroundFriction.Description, StructureRegion.GroundFriction.Format);
        }
    }
}
