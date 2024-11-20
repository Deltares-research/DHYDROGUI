using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.Model
{
    [TestFixture]
    public class WaterQualityObservationVariableOutputTest
    {
        [Test]
        public void TestCreateWaterQualityObservationVariableOutputWithObservationVariable()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l"),
                new DelftTools.Utils.Tuple<string, string>("Output parameter", "")
            }) {ObservationVariable = new WaterQualityObservationPoint {Name = "O1"}};

            Assert.AreEqual("O1", waterQualityObservationVariableOutput.Name);
            Assert.AreEqual("O1", waterQualityObservationVariableOutput.ToString());
            Assert.AreEqual(2, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            TimeSeries timeSeries1 = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0);
            Assert.AreEqual("Substance", timeSeries1.Name);
            Assert.AreEqual(1, timeSeries1.Components.Count);
            Assert.AreEqual("mg/l", timeSeries1.Components[0].Unit.Name);

            TimeSeries timeSeries2 = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(1);
            Assert.AreEqual("Output parameter", timeSeries2.Name);
            Assert.AreEqual(1, timeSeries2.Components.Count);
            Assert.AreEqual("", timeSeries2.Components[0].Unit.Name);
        }

        [Test]
        public void TestCreateWaterQualityObservationVariableOutputWithoutObservationVariable()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l"),
                new DelftTools.Utils.Tuple<string, string>("Output parameter", "")
            }) {Name = "O1"};

            Assert.AreEqual("O1", waterQualityObservationVariableOutput.Name);
            Assert.AreEqual("O1", waterQualityObservationVariableOutput.ToString());
            Assert.AreEqual(2, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            TimeSeries timeSeries1 = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0);
            Assert.AreEqual("Substance", timeSeries1.Name);
            Assert.AreEqual(1, timeSeries1.Components.Count);
            Assert.AreEqual("mg/l", timeSeries1.Components[0].Unit.Name);

            TimeSeries timeSeries2 = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(1);
            Assert.AreEqual("Output parameter", timeSeries2.Name);
            Assert.AreEqual(1, timeSeries2.Components.Count);
            Assert.AreEqual("", timeSeries2.Components[0].Unit.Name);
        }

        [Test]
        public void TestCreateWaterQualityObservationVariableOutputThrowsWhenOutputVariableTuplesParameterIsNull()
        {
            Assert.That(() => new WaterQualityObservationVariableOutput(null),
                        Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("outputVariableTuples"));
        }

        [Test]
        public void TestAddTimeSeries()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>>());

            // Add a time series
            waterQualityObservationVariableOutput.AddTimeSeries(new DelftTools.Utils.Tuple<string, string>("Substance 1", "mg/l"));

            Assert.AreEqual(1, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            TimeSeries timeSeries = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0);
            Assert.AreEqual("Substance 1", timeSeries.Name);
            Assert.AreEqual(1, timeSeries.Components.Count);
            Assert.AreEqual("mg/l", timeSeries.Components[0].Unit.Name);

            // Add another time series with the same name => addition should be skipped
            waterQualityObservationVariableOutput.AddTimeSeries(new DelftTools.Utils.Tuple<string, string>("Substance 1", "kg/l"));

            Assert.AreEqual(1, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            timeSeries = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0);
            Assert.AreEqual("Substance 1", timeSeries.Name);
            Assert.AreEqual(1, timeSeries.Components.Count);
            Assert.AreEqual("mg/l", timeSeries.Components[0].Unit.Name);

            // Insert a time series at the start
            waterQualityObservationVariableOutput.AddTimeSeries(new DelftTools.Utils.Tuple<string, string>("Substance 2", "mg/l"), 0);

            Assert.AreEqual(2, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            timeSeries = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0);
            Assert.AreEqual("Substance 2", timeSeries.Name);
            Assert.AreEqual(1, timeSeries.Components.Count);
            Assert.AreEqual("mg/l", timeSeries.Components[0].Unit.Name);

            // Insert a time series at the end
            waterQualityObservationVariableOutput.AddTimeSeries(new DelftTools.Utils.Tuple<string, string>("Substance 3", "mg/l"), 10);

            Assert.AreEqual(3, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            timeSeries = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(2);
            Assert.AreEqual("Substance 3", timeSeries.Name);
            Assert.AreEqual(1, timeSeries.Components.Count);
            Assert.AreEqual("mg/l", timeSeries.Components[0].Unit.Name);

            // Insert a time series in the middle
            waterQualityObservationVariableOutput.AddTimeSeries(new DelftTools.Utils.Tuple<string, string>("Substance 4", "mg/l"), 1);

            Assert.AreEqual(4, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            timeSeries = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(1);
            Assert.AreEqual("Substance 4", timeSeries.Name);
            Assert.AreEqual(1, timeSeries.Components.Count);
            Assert.AreEqual("mg/l", timeSeries.Components[0].Unit.Name);
        }

        [Test]
        public void TestRemoveTimeSeries()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>> {new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l")});

            // Remove a non existing time series => no exception should be thrown
            waterQualityObservationVariableOutput.RemoveTimeSeries("Test");

            Assert.AreEqual(1, waterQualityObservationVariableOutput.TimeSeriesList.Count());

            // Remove an existing time series
            waterQualityObservationVariableOutput.RemoveTimeSeries("Substance");

            Assert.AreEqual(0, waterQualityObservationVariableOutput.TimeSeriesList.Count());
        }

        [Test]
        public void TestCloneWaterQualityObservationVariableOutputWithObservationVariable()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l"),
                new DelftTools.Utils.Tuple<string, string>("Output parameter", "")
            }) {ObservationVariable = new WaterQualityObservationPoint {Name = "O1"}};

            var clone = waterQualityObservationVariableOutput.Clone() as WaterQualityObservationVariableOutput;

            Assert.IsNotNull(clone);
            Assert.AreEqual(waterQualityObservationVariableOutput.Name, clone.Name);
            Assert.AreEqual(waterQualityObservationVariableOutput.ObservationVariable, clone.ObservationVariable);
            Assert.AreNotSame(waterQualityObservationVariableOutput.TimeSeriesList, clone.TimeSeriesList);
            Assert.AreEqual(2, clone.TimeSeriesList.Count());
            Assert.AreNotSame(waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0), clone.TimeSeriesList.ElementAt(0));
            Assert.AreNotSame(waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(1), clone.TimeSeriesList.ElementAt(1));
        }

        [Test]
        public void TestCloneWaterQualityObservationVariableOutputWithoutObservationVariable()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l"),
                new DelftTools.Utils.Tuple<string, string>("Output parameter", "")
            }) {Name = "O1"};

            var clone = waterQualityObservationVariableOutput.Clone() as WaterQualityObservationVariableOutput;

            Assert.IsNotNull(clone);
            Assert.AreEqual(waterQualityObservationVariableOutput.Name, clone.Name);
            Assert.AreEqual(waterQualityObservationVariableOutput.ObservationVariable, clone.ObservationVariable);
            Assert.AreNotSame(waterQualityObservationVariableOutput.TimeSeriesList, clone.TimeSeriesList);
            Assert.AreEqual(2, clone.TimeSeriesList.Count());
            Assert.AreNotSame(waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0), clone.TimeSeriesList.ElementAt(0));
            Assert.AreNotSame(waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(1), clone.TimeSeriesList.ElementAt(1));
        }
    }
}