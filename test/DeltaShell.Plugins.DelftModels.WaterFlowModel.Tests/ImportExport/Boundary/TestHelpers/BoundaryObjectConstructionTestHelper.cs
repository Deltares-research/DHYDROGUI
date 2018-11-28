using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.TestHelpers
{
    public enum HasComponent
    {
        None,
        Constant,
        Table,
        TimeDependent
    }

    #region BoundaryCondition
    public static class BoundaryObjectConstructionTestHelper
    {
        public static BoundaryConditionWater GetConstantWaterLevelBcComponent()
        {
            return new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, InterpolationType.Constant, false, 21.0);
        }

        public static BoundaryConditionWater GetConstantWaterDischargeComponent()
        {
            return new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, InterpolationType.Constant, false, 22.0);
        }

        public static BoundaryConditionWater GetLevelDischargeTableBcComponent()
        {
            var valuesWaterLevelTable = new List<double>() { 55.0, 520.0, 1150.0, 1530.0 };
            var valuesWaterDischargeTable = new List<double>() { 95.0, 120.0, 210.0, 430.0 };
            var tableFunction = new Function();

            tableFunction.Arguments.Add(new Variable<double>("Water Level")
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Periodic
            });

            tableFunction.Components.Add(new Variable<double>("Discharge", new Unit("", "")));


            tableFunction.Arguments[0].SetValues(valuesWaterLevelTable);
            tableFunction.Components[0].SetValues(valuesWaterDischargeTable);
            return new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, InterpolationType.Linear, true, tableFunction);
        }

        public static BoundaryConditionWater GetTimeDependentWaterLevelBcComponent()
        {
            var startTime = DateTime.Today;
            var valuesWaterLevel = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValuesWaterLevel = new List<DateTime>()
            {
                startTime.AddHours(2),
                startTime.AddHours(4),
                startTime.AddHours(6),
                startTime.AddHours(8),
            };

            var functionWaterLevel = BoundaryTestHelper.GetNewTimeFunction("Water Level", "", "");
            functionWaterLevel.Arguments[0].SetValues(timeValuesWaterLevel);
            functionWaterLevel.Components[0].SetValues(valuesWaterLevel);
            return new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries,
                                              InterpolationType.Linear,
                                              true,
                                              functionWaterLevel);
        }

        public static BoundaryConditionWater GetTimeDependentWaterDischargeBcComponent()
        {
            var startTime = DateTime.Today;
            var valuesWaterFlow = new List<double>() { 6.0, 21.0, 11.0, 31.0 };
            var timeValuesWaterFlow = new List<DateTime>()
            {
                startTime.AddHours(3),
                startTime.AddHours(5),
                startTime.AddHours(7),
                startTime.AddHours(9),
            };

            var functionWaterFlow = BoundaryTestHelper.GetNewTimeFunction("Discharge", "", "");
            functionWaterFlow.Arguments[0].SetValues(timeValuesWaterFlow);
            functionWaterFlow.Components[0].SetValues(valuesWaterFlow);
            return new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries,
                                              InterpolationType.Linear,
                                              true,
                                              functionWaterFlow);
        }

        public static BoundaryConditionSalt GetConstantSaltBcComponent()
        {
            return new BoundaryConditionSalt(SaltBoundaryConditionType.Constant, InterpolationType.Constant, false, 23.0);
        }

        public static BoundaryConditionSalt GetTimeDependentSaltBcComponent()
        {
            var startTime = DateTime.Today;
            var valuesSalt = new List<double>() { 7.0, 22.0, 12.0, 32.0 };
            var timeValuesSalt = new List<DateTime>()
            {
                startTime.AddHours(4),
                startTime.AddHours(6),
                startTime.AddHours(8),
                startTime.AddHours(10),
            };

            var functionSalt = BoundaryTestHelper.GetNewTimeFunction("Salinity", "", "");
            functionSalt.Arguments[0].SetValues(timeValuesSalt);
            functionSalt.Components[0].SetValues(valuesSalt);
            return new BoundaryConditionSalt(SaltBoundaryConditionType.TimeDependent,
                                             InterpolationType.Linear,
                                             true,
                                             functionSalt);
        }

        public static BoundaryConditionTemperature GetConstantTemperatureBcComponent()
        {
            return new BoundaryConditionTemperature(TemperatureBoundaryConditionType.Constant, InterpolationType.Constant, false, 24.0);
        }

        public static BoundaryConditionTemperature GetTimeDependentTemperatureBcComponent()
        {
            var startTime = DateTime.Today;
            var valuesTemperature = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValuesTemperature = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
            };
            var functionTemperature = BoundaryTestHelper.GetNewTimeFunction("Temperature", "", "");
            functionTemperature.Arguments[0].SetValues(timeValuesTemperature);
            functionTemperature.Components[0].SetValues(valuesTemperature);
            return new BoundaryConditionTemperature(TemperatureBoundaryConditionType.TimeDependent,
                                                    InterpolationType.Linear,
                                                    true,
                                                    functionTemperature);
        }
        #endregion

        #region LateralDischarge
        public static LateralDischargeWater GetConstantWaterLdComponent()
        {
            return new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowConstant, InterpolationType.Constant, false, 21.0);
        }

        public static LateralDischargeWater GetTableWaterLdComponent()
        {
            var valuesWaterLevelTable = new List<double>() { 55.0, 520.0, 1150.0, 1530.0 };
            var valuesWaterDischargeTable = new List<double>() { 95.0, 120.0, 210.0, 430.0 };
            var tableFunction = new Function();

            tableFunction.Arguments.Add(new Variable<double>(BoundaryRegion.QuantityStrings.WaterLevel)
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Periodic
            });

            tableFunction.Components.Add(new Variable<double>(BoundaryRegion.QuantityStrings.WaterDischarge, new Unit("", "")));
            tableFunction.Arguments[0].SetValues(valuesWaterLevelTable);
            tableFunction.Components[0].SetValues(valuesWaterDischargeTable);
            return new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowWaterLevelTable, InterpolationType.Linear, true, tableFunction);
        }

        public static LateralDischargeWater GetTimeDependentWaterLdComponent()
        {
            var startTime = DateTime.Today;
            var valuesWaterLevel = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValuesWaterLevel = new List<DateTime>()
            {
                startTime.AddHours(2),
                startTime.AddHours(4),
                startTime.AddHours(6),
                startTime.AddHours(8),
            };

            var functionWaterLevel = BoundaryTestHelper.GetNewTimeFunction("Water Level", "", "");
            functionWaterLevel.Arguments[0].SetValues(timeValuesWaterLevel);
            functionWaterLevel.Components[0].SetValues(valuesWaterLevel);
            return new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowTimeSeries,
                InterpolationType.Linear,
                true,
                functionWaterLevel);
        }

        public static LateralDischargeSalt GetConstantSaltMassLdComponent()
        {
            return new LateralDischargeSalt(SaltLateralDischargeType.MassConstant, InterpolationType.Constant, false, 23.0);

        }

        public static LateralDischargeSalt GetTimeDependentSaltMassLdComponent()
        {
            var startTime = DateTime.Today;
            var valuesSaltMass = new List<double>() { 7.0, 22.0, 12.0, 32.0 };
            var timeValuesSaltMass = new List<DateTime>()
            {
                startTime.AddHours(4),
                startTime.AddHours(6),
                startTime.AddHours(8),
                startTime.AddHours(10),
            };

            var functionSaltMass = BoundaryTestHelper.GetNewTimeFunction(BoundaryRegion.QuantityStrings.WaterSalinity, "", "");
            functionSaltMass.Arguments[0].SetValues(timeValuesSaltMass);
            functionSaltMass.Components[0].SetValues(valuesSaltMass);
            return new LateralDischargeSalt(SaltLateralDischargeType.MassTimeSeries,
                InterpolationType.Linear,
                true,
                functionSaltMass);
        }

        public static LateralDischargeSalt GetConstantSaltConcentrationLdComponent()
        {
            return new LateralDischargeSalt(SaltLateralDischargeType.ConcentrationConstant, InterpolationType.Constant, false, 25.0);
        }

        public static LateralDischargeSalt GetTimeDependentSaltConcentrationLdComponent()
        {
            var startTime = DateTime.Today;
            var valuesSaltConcentration = new List<double>() { 7.0, 22.0, 12.0, 32.0 };
            var timeValuesSaltConcentration = new List<DateTime>()
            {
                startTime.AddHours(8),
                startTime.AddHours(10),
                startTime.AddHours(12),
                startTime.AddHours(14),
            };

            var functionSaltConcentration = BoundaryTestHelper.GetNewTimeFunction(BoundaryRegion.QuantityStrings.WaterSalinity, "", "");
            functionSaltConcentration.Arguments[0].SetValues(timeValuesSaltConcentration);
            functionSaltConcentration.Components[0].SetValues(valuesSaltConcentration);
            return new LateralDischargeSalt(SaltLateralDischargeType.ConcentrationTimeSeries,
                InterpolationType.Linear,
                true,
                functionSaltConcentration);
        }

        public static LateralDischargeTemperature GetConstantTemperatureLdComponent()
        {
            return new LateralDischargeTemperature(TemperatureLateralDischargeType.Constant, InterpolationType.Constant, false, 24.0);
        }

        public static LateralDischargeTemperature timeDependentTemperatureLdComponent()
        {
            var startTime = DateTime.Today;
            var valuesTemperature = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValuesTemperature = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
            };
            var functionTemperature = BoundaryTestHelper.GetNewTimeFunction(BoundaryRegion.QuantityStrings.WaterTemperature, "", "");
            functionTemperature.Arguments[0].SetValues(timeValuesTemperature);
            functionTemperature.Components[0].SetValues(valuesTemperature);
            return new LateralDischargeTemperature(TemperatureLateralDischargeType.TimeDependent,
                InterpolationType.Linear,
                true,
                functionTemperature);
        }



        #endregion


    }
}
