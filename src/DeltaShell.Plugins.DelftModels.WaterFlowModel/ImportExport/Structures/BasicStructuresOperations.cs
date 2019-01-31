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
        public static void SetCommonRegionElements(this IStructure1D structure, IDelftIniCategory structureBranchCategory, IBranch branch)
        {
            if (structure == null || structureBranchCategory == null) throw new ArgumentException();

            if (branch == null)
            {
                var errorMessage =
                    string.Format(Resources.BasicStructuresOperations_SetCommonRegionElements_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_,
                        structureBranchCategory.Name, StructureRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
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
                    HydroNetworkHelper.GetUniqueFeatureName(compositeBranchStructure.Network as HydroNetwork,
                        compositeBranchStructure);

                compositeBranchStructures.Add(compositeBranchStructure);

                structure.ParentStructure = compositeBranchStructure;

                return compositeBranchStructure;
            }
            else
            {
                var compoundName = structureBranchCategory.ReadProperty<string>(StructureRegion.CompoundName.Key);

                var alreadyExistingCompositeBranchStructure =
                    compositeBranchStructures.FirstOrDefault(bf => bf.Name == compoundName);

                if (alreadyExistingCompositeBranchStructure != null)
                {
                    structure.ParentStructure = alreadyExistingCompositeBranchStructure;
                    return alreadyExistingCompositeBranchStructure;
                }
                else
                {
                    compositeBranchStructure = new CompositeBranchStructure
                    {
                        Branch = structure.Branch,
                        Network = structure.Branch.Network,
                        Chainage = structure.Chainage,
                        Geometry = (IGeometry) structure.Geometry?.Clone(),
                        Name = compoundName
                    };

                    compositeBranchStructures.Add(compositeBranchStructure);

                    structure.ParentStructure = compositeBranchStructure;

                    return compositeBranchStructure;
                }
            }
        }
    }

}
