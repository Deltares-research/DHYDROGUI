using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class StructureFileWriter
    {
        public static void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.StructureDefinitionsMajorVersion, 
                                                             GeneralRegion.StructureDefinitionsMinorVersion, 
                                                             GeneralRegion.FileTypeName.StructureDefinition)
            };

            var lastCompositeStructureId = 0;
            var compositeStructures = waterFlowModel1D.Network.Structures.Where(s => s.GetStructureType() == StructureType.CompositeBranchStructure).Cast<ICompositeBranchStructure>();
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

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
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
