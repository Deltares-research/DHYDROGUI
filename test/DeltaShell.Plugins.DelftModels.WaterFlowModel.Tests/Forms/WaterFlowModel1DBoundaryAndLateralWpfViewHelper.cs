using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using System;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    public static class WaterFlowModel1DBoundaryAndLateralWpfViewHelper
    {
        public static WaterFlowModel1DLateralSourceData GetStandardLateralSourceData()
        {
            var t0 = DateTime.Now;
            var t1 = t0.AddMinutes(30);
            var t2 = t1.AddMinutes(30);

            var flowTimeSeries = new TimeSeries();
            flowTimeSeries.Components.Add(new Variable<double>("Flow [m³/s]"));
            flowTimeSeries[t0] = 0.567;
            flowTimeSeries[t1] = 0.234;
            flowTimeSeries[t2] = 0.012;

            var saltConcentrationTimeSeries = new TimeSeries();
            saltConcentrationTimeSeries.Components.Add(new Variable<double>("Salinity concentration [ppt]"));
            saltConcentrationTimeSeries[t0] = 1.47;
            saltConcentrationTimeSeries[t1] = 2.58;
            saltConcentrationTimeSeries[t2] = 3.69;

            var temperatureTimeSeries = new TimeSeries();
            temperatureTimeSeries.Components.Add(new Variable<double>("Temperature [°C]"));
            temperatureTimeSeries[t0] = 20.58;
            temperatureTimeSeries[t1] = 21.47;
            temperatureTimeSeries[t2] = 23.69;

            var lateralSourceData = new WaterFlowModel1DLateralSourceData
            {
                DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries,
                Data = flowTimeSeries,
                Flow = 0.567,
                UseSalt = true, // be sure to enable salt before changing type
                SaltLateralDischargeType = SaltLateralDischargeType.ConcentrationTimeSeries,
                SaltConcentrationDischargeConstant = 1.47,
                SaltConcentrationTimeSeries = saltConcentrationTimeSeries,
                UseTemperature = true, // be sure to enable temperature before changing type
                TemperatureLateralDischargeType = TemperatureLateralDischargeType.TimeDependent,
                TemperatureConstant = 20.58,
                TemperatureTimeSeries = temperatureTimeSeries,
            };

            return lateralSourceData;
        }

        public static WaterFlowModel1DBoundaryNodeData GetStandardBoundaryNodeData()
        {
            var t0 = DateTime.Now;
            var t1 = t0.AddMinutes(30);
            var t2 = t1.AddMinutes(30);

            var flowTimeSeries = new TimeSeries();
            flowTimeSeries.Components.Add(new Variable<double>("Flow [m³/s]"));
            flowTimeSeries[t0] = 0.567;
            flowTimeSeries[t1] = 0.234;
            flowTimeSeries[t2] = 0.012;

            var saltConcentrationTimeSeries = new TimeSeries();
            saltConcentrationTimeSeries.Components.Add(new Variable<double>("Salinity concentration [ppt]"));
            saltConcentrationTimeSeries[t0] = 1.47;
            saltConcentrationTimeSeries[t1] = 2.58;
            saltConcentrationTimeSeries[t2] = 3.69;

            var temperatureTimeSeries = new TimeSeries();
            temperatureTimeSeries.Components.Add(new Variable<double>("Temperature [°C]"));
            temperatureTimeSeries[t0] = 20.58;
            temperatureTimeSeries[t1] = 21.47;
            temperatureTimeSeries[t2] = 23.69;

            var boundaryNodeData = new WaterFlowModel1DBoundaryNodeData
            {
                DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, // be sure to change type before setting data
                Data = flowTimeSeries,
                Flow = 0.567,
                UseSalt = true, // be sure to enable salt before changing type
                SaltConditionType = SaltBoundaryConditionType.TimeDependent,
                SaltConcentrationConstant = 1.47,
                SaltConcentrationTimeSeries = saltConcentrationTimeSeries,
                UseTemperature = true, // be sure to enable temperature before changing type
                TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent,
                TemperatureConstant = 20.58,
                TemperatureTimeSeries = temperatureTimeSeries,
            };

            return boundaryNodeData;
        }
    }
}
