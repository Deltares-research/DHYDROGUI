using DelftTools.Hydro;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    public class StructureConverterTest
    {
        protected static IBranch GetSimpleBranchWith2Nodes()
        {
            IHydroNode node1 = new HydroNode { Name = "node1" };
            IHydroNode node2 = new HydroNode { Name = "node2" };

            return new Channel("branch", node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(100, 0)
                })
            };
        }
    }
}
