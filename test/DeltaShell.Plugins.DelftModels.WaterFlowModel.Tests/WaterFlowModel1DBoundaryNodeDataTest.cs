using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    class WaterFlowModel1DBoundaryNodeDataTest
    { 
        [Test]
        [ExpectedException(typeof(NotImplementedException), ExpectedMessage = "BoundaryNodeDataType not supported.")]
        public void GivenAnEmptyWaterFlowModel1DBoundaryNodeData_WhenGetValueIsCalledWithAnyValue_ThenANotImplementedExceptionIsRaised()
        {
            var nodeData = new WaterFlowModel1DBoundaryNodeData();
            nodeData.GetValue(Arg<DateTime>.Is.Anything);
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDateWithAFlowConstantDataType_WhenGetValueIsCalledWithAnyDateTimeValue_ThenTheConstantValueIsReturned()
        {
            const double expectedValue = 50.0;
            var nodeData = new WaterFlowModel1DBoundaryNodeData
            {
                DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant,
                Flow = expectedValue
            };

            Assert.That(nodeData.GetValue(Arg<DateTime>.Is.Anything), Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDateWithAWaterLevelConstantDataType_WhenGetValueIsCalledWithAnyDateTimeValue_ThenTheConstantValueIsReturned()
        {
            const double expectedValue = 50.0;
            var nodeData = new WaterFlowModel1DBoundaryNodeData
            {
                DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant,
                WaterLevel = expectedValue
            };

            Assert.That(nodeData.GetValue(Arg<DateTime>.Is.Anything), Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDataWithATimeSeriesDataTypeAndATimeSeries_WhenGetValueIsCalledWithADateTimValue_ThenSeriesDataItemIsEvaluatedWithTheGivenDateTimeAndTheValueReturned()
        {
            var mocks = new MockRepository();
            var seriesDataItemMock = mocks.DynamicMultiMock<IDataItem>(typeof(INotifyPropertyChanged), typeof(IFunction));
            const double expectedVal = 50.0;

            var dateTime = DateTime.Today;

            seriesDataItemMock.Expect(n => n.Value).Return(seriesDataItemMock).Repeat.Any();
            ((IFunction) seriesDataItemMock).Expect(n => n.Evaluate<Double>(Arg<DateTime>.Is.Anything)).Return(expectedVal).Repeat
                .Once();

            mocks.ReplayAll();

            var nodeData = new WaterFlowModel1DBoundaryNodeData
            {
                DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries,
                SeriesDataItem = seriesDataItemMock
            };

            // When
            var obtainedVal = nodeData.GetValue(dateTime);

            // Then
            Assert.That(obtainedVal, Is.EqualTo(expectedVal));
            mocks.VerifyAll();
        }

        [Test]     
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GivenAnEmptyWaterFlowModel1DBoundaryNodeData_WhenGetSaltValueIsCalledWithAnyValue_ThenAnArgumentOutOfRangeExceptionIsRaised()
        {
            var nodeData = new WaterFlowModel1DBoundaryNodeData();
            var result = nodeData.GetSaltValue(Arg<DateTime>.Is.Anything);
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDataWithAConstantSaltBoundaryConditionType_WhenGetSaltValueIsCalledWithAnyValue_ThenTheConstantValueIsReturned()
        {
            const double expectedVal = 50.0;

            var nodeData = new WaterFlowModel1DBoundaryNodeData()
            {
                SaltConditionType = SaltBoundaryConditionType.Constant,
                SaltConcentrationConstant = expectedVal
            };

            var obtainedVal = nodeData.GetSaltValue(Arg<DateTime>.Is.Anything);
            Assert.That(obtainedVal, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDataWithATimeDependantSaltBoundaryConditionType_WhenGetSaltValueIsCalledWithADateTimeValue_ThenSeriesDataItemIsEvaluatedWithTheGivenDateTimeAndTheValueReturned()
        {
            var mocks = new MockRepository();
            var timeSeriesMock = mocks.DynamicMock<ITimeSeries>();
            var variableMock = mocks.DynamicMock<IVariable>();

            const double expectedVal = 50.0;

            variableMock.Expect(v => v.ValueType).Return(typeof(DateTime)).Repeat.Any();
            timeSeriesMock.Expect(n => n.Arguments[0]).Return(variableMock).Repeat.Any();
            timeSeriesMock.Expect(n => n.Evaluate<double>(Arg<VariableValueFilter<DateTime>>.Is.Anything)).IgnoreArguments().Return(expectedVal).Repeat
                .Once();

            mocks.ReplayAll();

            var nodeData = new WaterFlowModel1DBoundaryNodeData()
            {
                SaltConditionType = SaltBoundaryConditionType.TimeDependent,
                SaltConcentrationTimeSeries = timeSeriesMock
            };

            var obtainedVal = nodeData.GetSaltValue(Arg<DateTime>.Is.NotNull);
            Assert.That(obtainedVal, Is.EqualTo(expectedVal));

            mocks.VerifyAll();
        }


        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GivenAnEmptyWaterFlowModel1DBoundaryNodeData_WhenGetTemperatureValueIsCalledWithAnyValue_ThenAnArgumentOutOfRangeExceptionIsRaised()
        {
            var nodeData = new WaterFlowModel1DBoundaryNodeData();
            var result = nodeData.GetTemperatureValue(Arg<DateTime>.Is.Anything);
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDataWithAConstantTemperatureBoundaryConditionType_WhenGetTemperatureValueIsCalledWithAnyValue_ThenTheConstantValueIsReturned()
        {
            const double expectedVal = 50.0;

            var nodeData = new WaterFlowModel1DBoundaryNodeData()
            {
                TemperatureConditionType = TemperatureBoundaryConditionType.Constant,
                TemperatureConstant = expectedVal
            };

            var obtainedVal = nodeData.GetTemperatureValue(Arg<DateTime>.Is.Anything);
            Assert.That(obtainedVal, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDataWithATimeDependantTemperatureBoundaryConditionType_WhenGetTemperatureValueIsCalledWithADateTimeValue_ThenSeriesDataItemIsEvaluatedWithTheGivenDateTimeAndTheValueReturned()
        {
            var mocks = new MockRepository();
            var timeSeriesMock = mocks.DynamicMock<ITimeSeries>();
            var variableMock = mocks.DynamicMock<IVariable>();

            const double expectedVal = 50.0;

            variableMock.Expect(v => v.ValueType).Return(typeof(DateTime)).Repeat.Any();
            timeSeriesMock.Expect(n => n.Arguments[0]).Return(variableMock).Repeat.Any();
            timeSeriesMock.Expect(n => n.Evaluate<double>(Arg<VariableValueFilter<DateTime>>.Is.Anything)).IgnoreArguments().Return(expectedVal).Repeat
                .Once();

            mocks.ReplayAll();

            var nodeData = new WaterFlowModel1DBoundaryNodeData()
            {
                TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent,
                TemperatureTimeSeries = timeSeriesMock
            };

            var obtainedVal = nodeData.GetTemperatureValue(Arg<DateTime>.Is.NotNull);
            Assert.That(obtainedVal, Is.EqualTo(expectedVal));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenAWaterFlowModel1DBoundaryNodeDataWhenToStringIsCalledThenTheNameOfTheDataIsReturned()
        {
            var nodeData = new WaterFlowModel1DBoundaryNodeData();
            Assert.That(nodeData.ToString(), Is.StringEnding(" - None"));
        }
    }
}
