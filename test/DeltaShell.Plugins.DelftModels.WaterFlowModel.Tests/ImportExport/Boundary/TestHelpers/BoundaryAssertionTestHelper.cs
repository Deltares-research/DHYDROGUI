using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.TestHelpers
{
    public static class BoundaryAssertionTestHelper
    {
        public static void AssertThatTimeDependentFunctionIsEqualTo(IFunction actual, IFunction expected)
        {
            AssertThatTimeDependentFunctionBaseIsEqualTo(actual, expected);

            Assert.That(actual.GetInterpolationType(), Is.EqualTo(expected.GetInterpolationType()));
            Assert.That(actual.GetExtrapolationType(), Is.EqualTo(expected.GetExtrapolationType()));
            Assert.That(actual.HasPeriodicity(), Is.EqualTo(expected.HasPeriodicity()));
        }

        public static void AssertThatTimeDependentFunctionWithoutExtensionIsEqualTo(
            IFunction actual, IFunction expected)
        {
            AssertThatTimeDependentFunctionBaseIsEqualTo(actual, expected);

            Assert.That(actual.Arguments[0].InterpolationType,
                Is.EqualTo(expected.Arguments[0].InterpolationType));
        }

        private static void AssertThatTimeDependentFunctionBaseIsEqualTo(IFunction actual, IFunction expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(expected, Is.Not.Null);

            var nValues = expected.Arguments[0].Values.Count;
            Assert.That(expected.Arguments[0].Values.Count,
                        Is.EqualTo(nValues));
            Assert.That(actual.Components[0].Values.Count,
                        Is.EqualTo(nValues));

            for (var i = 0; i < nValues; i++)
            {
                Assert.That(actual.Arguments[0].Values[i],
                            Is.EqualTo(expected.Arguments[0].Values[i]));
                Assert.That(actual.Components[0].Values[i],
                            Is.EqualTo(actual.Components[0].Values[i]));
            }
        }

        public static void AssertThatBoundaryConditionIsEqualTo(WaterFlowModel1DBoundaryNodeData node,
                                                                BoundaryCondition expected)
        {
            if (expected.WaterComponent != null)
            {
                Assert.That(node.DataType, Is.EqualTo(expected.WaterComponent.BoundaryType));

                switch (node.DataType)
                {
                    case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                        Assert.That(node.Flow, Is.EqualTo(expected.WaterComponent.ConstantBoundaryValue));
                        break;
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                        Assert.That(node.WaterLevel, Is.EqualTo(expected.WaterComponent.ConstantBoundaryValue));
                        break;
                    case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                    case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                        AssertThatTimeDependentFunctionIsEqualTo(node.Data, expected.WaterComponent.TimeDependentBoundaryValue);
                        break;
                }
            }

            if (expected.SaltComponent != null)
            {
                Assert.That(node.SaltConditionType, Is.EqualTo(expected.SaltComponent.BoundaryType));

                switch (node.SaltConditionType)
                {
                    case SaltBoundaryConditionType.Constant:
                        Assert.That(node.SaltConcentrationConstant, Is.EqualTo(expected.SaltComponent.ConstantBoundaryValue));
                        break;
                    case SaltBoundaryConditionType.TimeDependent:
                        AssertThatTimeDependentFunctionWithoutExtensionIsEqualTo(node.SaltConcentrationTimeSeries, expected.SaltComponent.TimeDependentBoundaryValue);
                        break;
                    case SaltBoundaryConditionType.None:
                        break;
                }
            }

            if (expected.TemperatureComponent != null)
            {
                Assert.That(node.TemperatureConditionType, Is.EqualTo(expected.TemperatureComponent.BoundaryType));

                switch (node.TemperatureConditionType)
                {
                    case TemperatureBoundaryConditionType.Constant:
                        Assert.That(node.TemperatureConstant, Is.EqualTo(expected.TemperatureComponent.ConstantBoundaryValue));
                        break;
                    case TemperatureBoundaryConditionType.TimeDependent:
                        AssertThatTimeDependentFunctionWithoutExtensionIsEqualTo(node.TemperatureTimeSeries, expected.TemperatureComponent.TimeDependentBoundaryValue);
                        break;
                    case TemperatureBoundaryConditionType.None:
                        break;
                }
            }
        }

        public static void AssertThatLateralDischargeIsEqualTo(WaterFlowModel1DLateralSourceData node,
                                                               LateralDischarge expected)
        {
            if (expected.WaterComponent != null)
            {
                Assert.That(node.DataType, Is.EqualTo(expected.WaterComponent.BoundaryType));

                switch (node.DataType)
                {
                    case WaterFlowModel1DLateralDataType.FlowConstant:
                        Assert.That(node.Flow, Is.EqualTo(expected.WaterComponent.ConstantBoundaryValue));
                        break;
                    case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                    case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                        AssertThatTimeDependentFunctionIsEqualTo(node.Data, expected.WaterComponent.TimeDependentBoundaryValue);
                        break;
                }
            }

            if (expected.SaltComponent != null)
            {
                Assert.That(node.SaltLateralDischargeType, Is.EqualTo(expected.SaltComponent.BoundaryType));

                switch (node.SaltLateralDischargeType)
                {
                    case SaltLateralDischargeType.ConcentrationConstant:
                        Assert.That(node.SaltConcentrationDischargeConstant, Is.EqualTo(expected.SaltComponent.ConstantBoundaryValue));
                        break;
                    case SaltLateralDischargeType.ConcentrationTimeSeries:
                        AssertThatTimeDependentFunctionWithoutExtensionIsEqualTo(node.SaltConcentrationTimeSeries, expected.SaltComponent.TimeDependentBoundaryValue);
                        break;
                    case SaltLateralDischargeType.MassConstant:
                        Assert.That(node.SaltMassDischargeConstant, Is.EqualTo(expected.SaltComponent.ConstantBoundaryValue));
                        break;
                    case SaltLateralDischargeType.MassTimeSeries:
                        AssertThatTimeDependentFunctionWithoutExtensionIsEqualTo(node.SaltMassTimeSeries, expected.SaltComponent.TimeDependentBoundaryValue);
                        break;
                    case SaltLateralDischargeType.Default:
                        break;
                }
            }

            if (expected.TemperatureComponent != null)
            {
                Assert.That(node.TemperatureLateralDischargeType, Is.EqualTo(expected.TemperatureComponent.BoundaryType));

                switch (node.TemperatureLateralDischargeType)
                {
                    case TemperatureLateralDischargeType.Constant:
                        Assert.That(node.TemperatureConstant, Is.EqualTo(expected.TemperatureComponent.ConstantBoundaryValue));
                        break;
                    case TemperatureLateralDischargeType.TimeDependent:
                        AssertThatTimeDependentFunctionWithoutExtensionIsEqualTo(node.TemperatureTimeSeries, expected.TemperatureComponent.TimeDependentBoundaryValue);
                        break;
                    case TemperatureLateralDischargeType.None:
                        break;
                }
            }
        }
    }
}
