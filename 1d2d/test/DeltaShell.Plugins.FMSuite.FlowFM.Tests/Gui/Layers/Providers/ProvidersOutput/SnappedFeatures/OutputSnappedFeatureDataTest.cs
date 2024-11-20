using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    [TestFixture]
    internal class OutputSnappedFeatureDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            const string layerName = "someLayerName";
            const string snappedFeatureDataPath = "someDataPath";

            // Call
            var data = new OutputSnappedFeatureData(model, layerName, snappedFeatureDataPath);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<OutputSnappedFeatureData>>());
            Assert.That(data.Model, Is.SameAs(model));
            Assert.That(data.LayerName, Is.EqualTo(layerName));
            Assert.That(data.SnappedFeatureDataPath, Is.EqualTo(snappedFeatureDataPath));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            const string thisLayerName = "this";
            const string otherLayerName = "other";

            const string thisPath = "thisPath";
            const string otherPath = "otherPath";

            var thisData = new OutputSnappedFeatureData(
                thisModel,
                thisLayerName, 
                thisPath);

            var otherData = new OutputSnappedFeatureData(
                otherModel,
                otherLayerName,
                otherPath);

            yield return new TestCaseData(thisData, 
                                          thisData, 
                                          true);
            yield return new TestCaseData(thisData, 
                                          new OutputSnappedFeatureData(
                                              thisModel, 
                                              thisLayerName, 
                                              thisPath),
                                          true);
            yield return new TestCaseData(otherData, 
                                          otherData, 
                                          true);
            yield return new TestCaseData(otherData, 
                                          new OutputSnappedFeatureData(
                                              otherModel, 
                                              otherLayerName,
                                              otherPath), 
                                          true);
            yield return new TestCaseData(thisData, 
                                          otherData, 
                                          false);
            yield return new TestCaseData(thisData, 
                                          new OutputSnappedFeatureData(
                                              otherModel,
                                              thisLayerName,
                                              thisPath),
                                          false);
            yield return new TestCaseData(thisData, 
                                          new OutputSnappedFeatureData(
                                              thisModel,
                                              otherLayerName,
                                              thisPath),
                                          false);
            yield return new TestCaseData(thisData, 
                                          new OutputSnappedFeatureData(
                                              thisModel,
                                              thisLayerName,
                                              otherPath),
                                          false);
            yield return new TestCaseData(thisData, 
                                          null, 
                                          false);
            yield return new TestCaseData(thisData, 
                                          new object(), 
                                          false);
        }

        [Test]
        [TestCaseSource(nameof(EqualsParams))]
        public void Equals_ExpectedResults(OutputSnappedFeatureData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}