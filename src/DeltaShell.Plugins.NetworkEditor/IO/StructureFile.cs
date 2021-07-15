using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.IO
{
    public static class StructureFile
    {
        public static IEnumerable<DelftIniCategory> ExtractFunctionStructuresOfNetworkGenerator(IHydroNetwork network)
        {
            var compositeStructures = network.Structures.Where(s => s.GetStructureType() == StructureType.CompositeBranchStructure).Cast<ICompositeBranchStructure>().ToList();
            
            foreach (var structure in compositeStructures.SelectMany(composite => composite.Structures).Concat(network.Structures.Where(s => s.GetStructureType() != StructureType.CompositeBranchStructure)).Distinct())
            {
                var category = ExtractStructureCategory(structure);
                if(category != null)
                    yield return category;
            }

            foreach (var compositeStructure in compositeStructures.Where(cs => cs.Structures.Count >= 0 || cs.Branch is SewerConnection))
            {
                yield return new DefinitionGeneratorCompound().CreateStructureRegion(compositeStructure);
            }
        }

        private static DelftIniCategory ExtractStructureCategory(IStructure1D structure)
        {
            var structureType = structure.GetStructureType();
            var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structureType);
            if (definitionGeneratorStructure == null)
                return null;

            var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure);
            var structurefrictionData = structure as IFrictionData;
            if (structurefrictionData != null)
            {
                if (structure is IBridge)
                {
                    //key is friction
                    AddFrictionData(
                        structureCategory,
                        structurefrictionData.FrictionDataType,
                        structurefrictionData.Friction);
                }
                else
                {
                    //key is bedfriction
                    AddBedFrictionData(
                        structureCategory,
                        structurefrictionData.FrictionDataType,
                        structurefrictionData.Friction);
                }
            }

            return structureCategory;
        }

        private static void AddBedFrictionData(DelftIniCategory category, Friction frictionType, double friction)
        {
            category.AddProperty(StructureRegion.BedFrictionType.Key, frictionType.ToString().ToLower(), StructureRegion.BedFrictionType.Description);
            category.AddProperty(StructureRegion.BedFriction.Key, friction, StructureRegion.BedFriction.Description, StructureRegion.BedFriction.Format);
        }
        private static void AddFrictionData(DelftIniCategory category, Friction frictionType, double friction)
        {
            category.AddProperty(StructureRegion.FrictionType.Key, frictionType.ToString().ToLower(), StructureRegion.FrictionType.Description);
            category.AddProperty(StructureRegion.Friction.Key, friction, StructureRegion.Friction.Description, StructureRegion.Friction.Format);
        }
    }
}
