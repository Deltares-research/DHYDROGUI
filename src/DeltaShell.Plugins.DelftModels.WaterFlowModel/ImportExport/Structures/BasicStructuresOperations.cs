using System;
using System.Collections;
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
    public class BasicStructuresOperations
    {
        public static void ReadCommonRegionElements(DelftIniCategory structureBranchCategory, IList<IChannel> channelList, IStructure1D weir)
        {
            var name = structureBranchCategory.ReadProperty<string>(StructureRegion.Id.Key);
            var longName = structureBranchCategory.ReadProperty<string>(StructureRegion.Name.Key, true);
            var chainage = structureBranchCategory.ReadProperty<double>(StructureRegion.Chainage.Key);

            var branchName = structureBranchCategory.ReadProperty<string>(StructureRegion.BranchId.Key);
            var branch = channelList.FirstOrDefault(c => c.Name == branchName);

            var geometry = new Point(
                LengthLocationMap.GetLocation(branch.Geometry, chainage).GetCoordinate(branch.Geometry));

            if (branch == null)
            {
                var errorMessage =
                    string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}",
                        structureBranchCategory.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }

            weir.Name = name;
            weir.Chainage = chainage;
            weir.Branch = branch;
            weir.Network = branch.Network;
            weir.Geometry = geometry;
            weir.LongName = longName;
        }

        public static void CreateCompositeBranchStructuresIfNeededAndAddStructure
            (DelftIniCategory structureBranchCategory, IStructure1D weir, IList<ICompositeBranchStructure> compositeBranchStructures)
        {
            ICompositeBranchStructure compositeBranchStructure;

            var compound = structureBranchCategory.ReadProperty<int>(StructureRegion.Compound.Key);
            if (compound == 0)
            {
                compositeBranchStructure = new CompositeBranchStructure
                {
                    Branch = weir.Branch,
                    Network = weir.Branch.Network,
                    Chainage = weir.Chainage,
                    Geometry = (IGeometry) weir.Geometry?.Clone()
                };

                // make new composite structure names unique
                compositeBranchStructure.Name =
                    HydroNetworkHelper.GetUniqueFeatureName(compositeBranchStructure.Network as HydroNetwork,
                        compositeBranchStructure);

                //weir.Branch.BranchFeatures.Add(compositeBranchStructure);

                compositeBranchStructures.Add(compositeBranchStructure);
            }
            else
            {
                var compoundName = structureBranchCategory.ReadProperty<string>(StructureRegion.CompoundName.Key);

                if (compositeBranchStructures.Any(cbs => cbs.Name == compoundName))
                {
                    compositeBranchStructure = compositeBranchStructures.FirstOrDefault(bf => bf.Name == compoundName);
                }
                else
                {
                    compositeBranchStructure = new CompositeBranchStructure
                    {
                        Branch = weir.Branch,
                        Network = weir.Branch.Network,
                        Chainage = weir.Chainage,
                        Geometry = (IGeometry) weir.Geometry?.Clone(),
                        Name = compoundName
                    };


                    //  weir.Branch.BranchFeatures.Add(compositeBranchStructure);

                    compositeBranchStructures.Add(compositeBranchStructure);
                }
            }

            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            weir.ParentStructure = compositeBranchStructure;
        }
    }

}
