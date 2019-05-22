using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    static class BoundaryFileReaderTestHelper
    {
        public static WaterFlowModel1D GetSimpleModel()
        {
            var model = new WaterFlowModel1D
            {
                Network = new HydroNetwork()
                {
                    Nodes =
                    {
                        new HydroNode("Node000"){ Geometry = new Point(0.0, 0.0) },
                        new HydroNode("Node001"){ Geometry = new Point(1.0, 1.0) },
                        new HydroNode("Node002"){ Geometry = new Point(2.0, 2.0) },
                        new HydroNode("Node003"){ Geometry = new Point(3.0, 3.0) },
                        new HydroNode("Node004"){ Geometry = new Point(4.0, 4.0) },
                        new HydroNode("Node005"){ Geometry = new Point(5.0, 5.0) }
                    }
                }
            };

            model.Network.Branches.AddRange(new[]
            {
                new Channel("Branch001", model.Network.Nodes[0], model.Network.Nodes[1], 0.0), 
                new Channel("Branch002", model.Network.Nodes[1], model.Network.Nodes[2], 0.0), 
                new Channel("Branch003", model.Network.Nodes[2], model.Network.Nodes[3], 0.0), 
                new Channel("Branch004", model.Network.Nodes[3], model.Network.Nodes[4], 0.0), 
                new Channel("Branch005", model.Network.Nodes[4], model.Network.Nodes[5], 0.0), 
            });

            model.Network.Branches[0].BranchFeatures.Add(new LateralSource(){ Name = "Lateral001", Geometry = new Point(0.5, 0.5) });
            model.Network.Branches[1].BranchFeatures.Add(new LateralSource(){ Name = "Lateral002", Geometry = new Point(1.5, 1.5) });
            model.Network.Branches[2].BranchFeatures.Add(new LateralSource(){ Name = "Lateral003", Geometry = new Point(2.5, 2.5) });

            return model;
        }

        public static bool CompareBoundaryNodeData(WaterFlowModel1DBoundaryNodeData boundaryNodeData1, WaterFlowModel1DBoundaryNodeData boundaryNodeData2, bool verifyGeometry = true)
        {
            var areEqual = true;

            areEqual &= boundaryNodeData1.Name == boundaryNodeData2.Name;
            areEqual &= boundaryNodeData1.DataType == boundaryNodeData2.DataType;
            areEqual &= boundaryNodeData1.Feature.Name == boundaryNodeData2.Feature.Name;
            if (boundaryNodeData1.Data != null && boundaryNodeData2.Data != null)
            {
                areEqual &= boundaryNodeData1.Data.Arguments.Count == boundaryNodeData2.Data.Arguments.Count;
                for (var i = 0; i < boundaryNodeData1.Data.Arguments.Count; i++)
                {
                    areEqual &= CompareDataValues(boundaryNodeData1.Data.Arguments[i].Values,
                        boundaryNodeData2.Data.Arguments[i].Values);
                }

                areEqual &= boundaryNodeData1.Data.Components.Count == boundaryNodeData2.Data.Components.Count;
                for (var i = 0; i < boundaryNodeData1.Data.Components.Count; i++)
                {
                    areEqual &= CompareDataValues(boundaryNodeData1.Data.Components[i].Values,
                        boundaryNodeData2.Data.Components[i].Values);
                }
            }
            areEqual &= boundaryNodeData1.InterpolationType == boundaryNodeData2.InterpolationType;
            areEqual &= boundaryNodeData1.Flow.Equals(boundaryNodeData2.Flow);
            areEqual &= boundaryNodeData1.WaterLevel.Equals(boundaryNodeData2.WaterLevel);
            if (verifyGeometry)
                areEqual &= boundaryNodeData1.Geometry.EqualsExact(boundaryNodeData2.Geometry);

            return areEqual;
        }

        public static bool CompareLateralSourceData(WaterFlowModel1DLateralSourceData lateralSourceData1, WaterFlowModel1DLateralSourceData lateralSourceData2, bool verifyGeometry = true)
        {
            var areEqual = true;

            areEqual &= lateralSourceData1.Name == lateralSourceData2.Name;
            areEqual &= lateralSourceData1.DataType == lateralSourceData2.DataType;
            areEqual &= lateralSourceData1.Feature.Name == lateralSourceData2.Feature.Name;

            if (lateralSourceData1.Data != null && lateralSourceData2.Data != null)
            {
                areEqual &= lateralSourceData1.Data.Arguments.Count == lateralSourceData2.Data.Arguments.Count;
                for (var i = 0; i < lateralSourceData1.Data.Arguments.Count; i++)
                {
                    areEqual &= CompareDataValues(lateralSourceData1.Data.Arguments[i].Values,
                        lateralSourceData2.Data.Arguments[i].Values);
                }

                areEqual &= lateralSourceData1.Data.Components.Count == lateralSourceData2.Data.Components.Count;
                for (var i = 0; i < lateralSourceData1.Data.Components.Count; i++)
                {
                    areEqual &= CompareDataValues(lateralSourceData1.Data.Components[i].Values,
                        lateralSourceData2.Data.Components[i].Values);
                }
            }

            areEqual &= lateralSourceData1.Flow.Equals(lateralSourceData2.Flow);

            if (verifyGeometry)
                areEqual &= lateralSourceData1.Geometry.EqualsExact(lateralSourceData2.Geometry);

            return areEqual;
        }

        public static bool CompareWindSourceData(WindFunction wind1, WindFunction wind2)
        {
            var areEqual = true;

            areEqual &= wind1.Name == wind2.Name;
            if (wind1.Velocity != null && wind2.Velocity != null
                && wind1.Arguments != null && wind2.Arguments != null)
            {
                areEqual &= wind1.Arguments.Count == wind2.Arguments.Count;
                for (var i = 0; i < wind1.Arguments.Count; i++)
                {
                    areEqual &= CompareDataValues(wind1.Arguments[i].Values, wind2.Arguments[i].Values);
                }

                areEqual &= CompareDataValues(wind1.Velocity.Values, wind2.Velocity.Values);
                areEqual &= CompareDataValues(wind1.Direction.Values, wind2.Direction.Values);
            }

            return areEqual;
        }

        public static bool CompareMeteoSourceData(MeteoFunction meteo1, MeteoFunction meteo2)
        {
            var areEqual = true;

            areEqual &= meteo1.Name == meteo2.Name;

            areEqual &= ((meteo1.Arguments[0] == null && meteo2.Arguments[0] == null) ||
                         (meteo1.Arguments[0] != null && meteo2.Arguments[0] != null));
            if (meteo1.Arguments[0] != null)
                areEqual &= CompareDataValues(meteo1.Arguments[0].Values, meteo2.Arguments[0].Values);

            areEqual &= ((meteo1.AirTemperature == null && meteo2.AirTemperature == null) ||
                         (meteo1.AirTemperature != null && meteo2.AirTemperature != null));
            if (meteo1.AirTemperature != null)
                areEqual &= CompareDataValues(meteo1.AirTemperature.Values, meteo2.AirTemperature.Values);

            areEqual &= ((meteo1.Cloudiness == null && meteo2.Cloudiness == null) ||
                         (meteo1.Cloudiness != null && meteo2.Cloudiness != null));
            if (meteo1.Cloudiness != null)
                areEqual &= CompareDataValues(meteo1.Cloudiness.Values, meteo2.Cloudiness.Values);

            areEqual &= ((meteo1.RelativeHumidity == null && meteo2.RelativeHumidity == null) ||
                         (meteo1.RelativeHumidity != null && meteo2.RelativeHumidity != null));
            if (meteo1.Cloudiness != null)
                areEqual &= CompareDataValues(meteo1.RelativeHumidity.Values, meteo2.RelativeHumidity.Values);

            return areEqual;
        }

        private static bool CompareDataValues(IMultiDimensionalArray data1, IMultiDimensionalArray data2)
        {
            var areEqual = true;
            areEqual &= data1.Count == data2.Count;
            for (var i = 0; i < data1.Count; i++)
            {
                areEqual &= data1[i].ToString() == data2[i].ToString();
            }
            return areEqual;
        }

        
    }
}
