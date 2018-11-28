using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class BasicStructuresOperations
    {
        public static void ReadCommonRegionElements(IDelftIniCategory structureBranchCategory, IList<IChannel> channelList, IStructure1D structure)
        {
            var name = structureBranchCategory.ReadProperty<string>(StructureRegion.Id.Key);
            var longName = structureBranchCategory.ReadProperty<string>(StructureRegion.Name.Key, true) ?? string.Empty;
            var chainage = structureBranchCategory.ReadProperty<double>(StructureRegion.Chainage.Key);

            var branchName = structureBranchCategory.ReadProperty<string>(StructureRegion.BranchId.Key);
            var branch = channelList.FirstOrDefault(c => c.Name == branchName);

            if (branch == null)
            {
                var errorMessage =
                    string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}",
                        structureBranchCategory.Name, StructureRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }

            var resultingChainage = chainage / branch.Length * branch.Geometry.Length;
            var geometry = new Point(
                LengthLocationMap.GetLocation(branch.Geometry, resultingChainage).GetCoordinate(branch.Geometry));

            structure.Name = name;
            structure.Chainage = chainage;
            structure.Branch = branch;
            structure.Network = branch.Network;
            structure.Geometry = geometry;
            structure.LongName = longName;
        }

        public static ICompositeBranchStructure CreateCompositeBranchStructuresIfNeeded
            (DelftIniCategory structureBranchCategory, IStructure1D structure, IList<ICompositeBranchStructure> compositeBranchStructures)
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

                if (compositeBranchStructures.Any(cbs => cbs.Name == compoundName))
                {
                    compositeBranchStructure = compositeBranchStructures.FirstOrDefault(bf => bf.Name == compoundName);

                    structure.ParentStructure = compositeBranchStructure;

                    return compositeBranchStructure;
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
