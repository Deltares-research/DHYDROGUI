using System;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    public static class BoundaryFileWriterTestHelper
    {
        #region ConstTestData

        // Node1: Constant Flow
        public const string NodeConstantFlowName = "Node001";
        public const WaterFlowModel1DBoundaryNodeDataType NodeConstantFlowType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
        public const double NodeConstantFlowValue = 1.0;

        // Node2: Constant WaterLevel
        public const string NodeConstantWaterLevelName = "Node002";
        public const WaterFlowModel1DBoundaryNodeDataType NodeConstantWaterLevelType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
        public const double NodeConstantWaterLevelValue = 3.0;

        // Node3: Flow TimeSeries
        public const string NodeFlowTimeSeriesName = "Node003";
        public const WaterFlowModel1DBoundaryNodeDataType NodeFlowTimeSeriesType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
        public const int NodeFlowTimeSeriesArgument1 = 7;
        public const int NodeFlowTimeSeriesArgument2 = 11;
        public const double NodeFlowTimeSeriesComponent1 = 13.0;
        public const double NodeFlowTimeSeriesComponent2 = 17.0;

        // Node4: WaterLevel TimeSeries
        public const string NodeWaterLevelTimeSeriesName = "Node004";
        public const WaterFlowModel1DBoundaryNodeDataType NodeWaterLevelTimeSeriesType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
        public const int NodeWaterLevelTimeSeriesArgument1 = 19;
        public const int NodeWaterLevelTimeSeriesArgument2 = 23;
        public const double NodeWaterLevelTimeSeriesComponent1 = 29.0;
        public const double NodeWaterLevelTimeSeriesComponent2 = 31.0;

        // Node5: Flow WaterLevel
        public const string NodeFlowWaterLevelName = "Node005";
        public const WaterFlowModel1DBoundaryNodeDataType NodeFlowWaterLevelType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;
        public const double NodeFlowWaterLevelArgument1 = 37.0;
        public const double NodeFlowWaterLevelArgument2 = 41.0;
        public const double NodeFlowWaterLevelComponent1 = 43.0;
        public const double NodeFlowWaterLevelComponent2 = 47.0;

        // Lateral1: Constant Flow
        public const string LateralConstantFlowName = "LateralSource1";
        public const WaterFlowModel1DLateralDataType LateralConstantFlowType = WaterFlowModel1DLateralDataType.FlowConstant;
        public const double LateralConstantFlowValue = 53.0;

        // Lateral2: Flow WaterLevel
        public const string LateralFlowWaterLevelName = "LateralSource2";
        public const WaterFlowModel1DLateralDataType LateralFlowWaterLevelType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
        public const double LateralFlowWaterLevelArgument1 = 59.0;
        public const double LateralFlowWaterLevelArgument2 = 61.0;
        public const double LateralFlowWaterLevelComponent1 = 67.0;
        public const double LateralFlowWaterLevelComponent2 = 71.0;

        // Lateral3: Flow TimeSeries
        public const string LateralFlowTimeSeriesName = "LateralSource3";
        public const WaterFlowModel1DLateralDataType LateralFlowTimeSeriesType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
        public const int LateralFlowTimeSeriesArgument1 = 73;
        public const int LateralFlowTimeSeriesArgument2 = 79;
        public const double LateralFlowTimeSeriesComponent1 = 83.0;
        public const double LateralFlowTimeSeriesComponent2 = 89.0;

        // Wind
        public const int WindTimeSeriesArgument1 = 19;
        public const int WindTimeSeriesArgument2 = 23;
        // Wind: velocity
        public const double WindVelocityTimeSeriesComponent1 = 29.0;
        public const double WindVelocityTimeSeriesComponent2 = 31.0;
        // Wind: direction
        public const double WindDirectionTimeSeriesComponent1 = 30.0;
        public const double WindDirectionTimeSeriesComponent2 = 41.0;

        // MeteoData
        public const int MeteoDataTimeSeriesArgument1 = 89;
        public const int MeteoDataTimeSeriesArgument2 = 93;
        // MeteoData: AirTemperature
        public const double MeteoDataAirTemperatureTimeSeriesComponent1 = 19.0;
        public const double MeteoDataAirTemperatureTimeSeriesComponent2 = 20.5;
        // MeteoData: Humidity
        public const double MeteoDataHumidityTimeSeriesComponent1 = 10.5;
        public const double MeteoDataHumidityTimeSeriesComponent2 = 8.0;
        // MeteoData: Cloudiness
        public const double MeteoDataCloudinessTimeSeriesComponent1 = 26.5;
        public const double MeteoDataCloudinessTimeSeriesComponent2 = 25.0;
        
        #endregion

        public static WaterFlowModel1DBoundaryNodeData GetBoundaryNodeDataWithConstantType(
            string nodeName, WaterFlowModel1DBoundaryNodeDataType boundaryNodeDataType, double value)
        {
            var boundaryNodeData = new WaterFlowModel1DBoundaryNodeData() 
            {
                Feature = new HydroNode(nodeName),
                DataType = boundaryNodeDataType,
                Flow = value,
                WaterLevel = value
            };
            return boundaryNodeData;
        }

        public static WaterFlowModel1DBoundaryNodeData GetBoundaryNodeDataWithTimeSeriesType(
            string nodeName, WaterFlowModel1DBoundaryNodeDataType boundaryNodeDataType, DateTime[] argumentValues, double[] componentValues)
        {
            var boundaryNodeData = new WaterFlowModel1DBoundaryNodeData()
            {
                Feature = new HydroNode(nodeName),
                DataType = boundaryNodeDataType
            };

            var argument = new Variable<DateTime>();
            argument.Values.AddRange(argumentValues);
            boundaryNodeData.Data.Arguments.Clear();
            boundaryNodeData.Data.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(componentValues);
            boundaryNodeData.Data.Components.Clear();
            boundaryNodeData.Data.Components.Add(component);

            return boundaryNodeData;
        }

        public static WaterFlowModel1DBoundaryNodeData GetBoundaryNodeDataWithFlowWaterLevelData(
            string nodeName, WaterFlowModel1DBoundaryNodeDataType boundaryNodeDataType, double[] argumentValues, double[] componentValues)
        {
            var boundaryNodeData = new WaterFlowModel1DBoundaryNodeData()
            {
                Feature = new HydroNode(nodeName),
                DataType = boundaryNodeDataType
            };

            var argument = new Variable<double>();
            argument.Values.AddRange(argumentValues);
            boundaryNodeData.Data.Arguments.Clear();
            boundaryNodeData.Data.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(componentValues);
            boundaryNodeData.Data.Components.Clear();
            boundaryNodeData.Data.Components.Add(component);

            return boundaryNodeData;
        }

        public static WaterFlowModel1DLateralSourceData GetLateralSourceDataWithFlowData(
            string lateralName, WaterFlowModel1DLateralDataType lateralDataType, double value)
        {
            var lateralSourceData = new WaterFlowModel1DLateralSourceData()
            {
                Feature = new LateralSource(){Name = lateralName},
                DataType = lateralDataType,
                Flow = value
            };

            return lateralSourceData;
        }

        public static WaterFlowModel1DLateralSourceData GetLateralSourceDataWithFlowWaterLevelData(
            string lateralName, WaterFlowModel1DLateralDataType lateralDataType, double[] argumentValues, double[] componentValues)
        {
            var lateralSourceData = new WaterFlowModel1DLateralSourceData()
            {
                Feature = new LateralSource() { Name = lateralName },
                DataType = lateralDataType
            };

            var argument = new Variable<double>();
            argument.Values.AddRange(argumentValues);
            lateralSourceData.Data.Arguments.Clear();
            lateralSourceData.Data.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(componentValues);
            lateralSourceData.Data.Components.Clear();
            lateralSourceData.Data.Components.Add(component);

            return lateralSourceData;
        }

        public static WaterFlowModel1DLateralSourceData GetLateralSourceDataWithFlowTimeSeriesData(
            string lateralName, WaterFlowModel1DLateralDataType lateralDataType, DateTime[] argumentValues, double[] componentValues)
        {
            var lateralSourceData = new WaterFlowModel1DLateralSourceData()
            {
                Feature = new LateralSource() { Name = lateralName },
                DataType = lateralDataType
            };

            var argument = new Variable<DateTime>();
            argument.Values.AddRange(argumentValues);
            lateralSourceData.Data.Arguments.Clear();
            lateralSourceData.Data.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(componentValues);
            lateralSourceData.Data.Components.Clear();
            lateralSourceData.Data.Components.Add(component);

            return lateralSourceData;
        }

    }
}
