using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class BasicStructuresOperations
    {
        /// <summary>
        /// Sets the common structure properties that are available for every structure.
        /// </summary>
        /// <param name="structure">The structure to set the values on.</param>
        /// <param name="structureBranchCategory">The <see cref="IDelftIniCategory"/> object to extract the property values from.</param>
        /// <param name="branch">The branch that is associated with the structure.</param>
        /// <exception cref="ArgumentException">When one of the arguments is equal to null.</exception>
        public static void SetCommonRegionElementsFromCategory(this IStructure1D structure, IDelftIniCategory structureBranchCategory, IBranch branch)
        {
            if (structure == null || structureBranchCategory == null) throw new ArgumentException();

            if (branch == null)
            {
                var errorMessage =
                    string.Format(Resources.BasicStructuresOperations_SetCommonRegionElements_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_,
                        structureBranchCategory.Name, StructureRegion.BranchId.Key, Environment.NewLine);
                throw new ArgumentException(errorMessage);
            }

            var name = structureBranchCategory.ReadProperty<string>(StructureRegion.Id.Key);
            var longName = structureBranchCategory.ReadProperty<string>(StructureRegion.Name.Key, true) ?? string.Empty;
            var chainage = structureBranchCategory.ReadProperty<double>(StructureRegion.Chainage.Key);

            var resultingChainage = chainage / branch.Length * branch.Geometry.Length;
            var geometry = CalculateStructureGeometry(branch, resultingChainage);

            structure.Name = name;
            structure.Chainage = chainage;
            structure.Branch = branch;
            structure.Network = branch.Network;
            structure.Geometry = geometry;
            structure.LongName = longName;
        }

        private static Point CalculateStructureGeometry(IBranch branch, double resultingChainage)
        {
            var location = LengthLocationMap.GetLocation(branch.Geometry, resultingChainage);
            var geometry = new Point(location.GetCoordinate(branch.Geometry));
            return geometry;
        }

        /*If compound is 0 it means that there is only one structure at a certain location. 
        For users this means no composite structure, however in the code behind all structures (alone or groups) are placed in a composite structure.
        Therefore if 0 is given always a new composite structure should be created.
        For all other numbers, the number indicates the group and the compoundName is the corresponding group name. For these ones only 
        the first time a composite structure should be created
        */
        public static ICompositeBranchStructure CreateCompositeBranchStructuresIfNeeded
            (IDelftIniCategory structureBranchCategory, IStructure1D structure, IList<ICompositeBranchStructure> compositeBranchStructures)
        {
            ICompositeBranchStructure compositeBranchStructure;

            var compound = structureBranchCategory.ReadProperty<int>(StructureRegion.Compound.Key);
            if (compound == 0)
            {
                compositeBranchStructure = new CompositeBranchStructure
                {
                    Branch = structure.Branch,
                    Network = structure.Branch.Network,
                    Chainage = structure.Chainage,
                    Geometry = (IGeometry) structure.Geometry?.Clone()
                };

                // make new composite structure names unique
                compositeBranchStructure.Name =
                    HydroNetworkHelper.GetUniqueFeatureNameWithAdditionalNewNameCheck(compositeBranchStructure.Network as HydroNetwork,
                        compositeBranchStructure);

                compositeBranchStructures.Add(compositeBranchStructure);

                structure.ParentStructure = compositeBranchStructure;

                return compositeBranchStructure;
            }

            try
            {
                var compoundName = structureBranchCategory.ReadProperty<string>(StructureRegion.CompoundName.Key);
                return GetNewOrExistingCompositeBranchStructure(compoundName, structure, compositeBranchStructures);
            }
            catch (PropertyNotFoundInFileException)
            {
                var compoundProperty = structureBranchCategory.Properties.FirstOrDefault(p => p.Name == StructureRegion.Compound.Key);
                var type = structureBranchCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);
                var errorMessage = string.Format(Resources.BasicStructuresOperations_CreateCompositeBranchStructuresIfNeeded_Line__0___property___1___is_mandatory_when_property___2___is_defined_as_a_number_unequal_to_0,
                    compoundProperty?.LineNumber, StructureRegion.CompoundName.Key, StructureRegion.Compound.Key, structure.Name, type);

                throw new PropertyNotFoundInFileException(errorMessage);
            }
        }

        private static ICompositeBranchStructure GetNewOrExistingCompositeBranchStructure(string compositeStructureName, IStructure1D structure, ICollection<ICompositeBranchStructure> compositeBranchStructures)
        {
            var alreadyExistingCompositeBranchStructure = compositeBranchStructures.FirstOrDefault(bf => bf.Name == compositeStructureName);
            if (alreadyExistingCompositeBranchStructure != null)
            {
                return GetCompositeBranchStructureWithAddedStructure(structure, alreadyExistingCompositeBranchStructure);
            }

            var compositeBranchStructure = CreateNewCompositeBranchStructure(compositeStructureName, structure);
            compositeBranchStructures.Add(compositeBranchStructure);

            return GetCompositeBranchStructureWithAddedStructure(structure, compositeBranchStructure);
        }

        private static ICompositeBranchStructure GetCompositeBranchStructureWithAddedStructure(IStructure1D structure,
            ICompositeBranchStructure compositeBranchStructure)
        {
            structure.ParentStructure = compositeBranchStructure;
            return compositeBranchStructure;
        }

        private static ICompositeBranchStructure CreateNewCompositeBranchStructure(string compoundName, IStructure1D structure)
        {
            return new CompositeBranchStructure
            {
                Branch = structure.Branch,
                Network = structure.Branch.Network,
                Chainage = structure.Chainage,
                Geometry = (IGeometry) structure.Geometry?.Clone(),
                Name = compoundName
            };
        }
    }

}
