using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    [TestFixture]
    internal class OutputSnappedFeatureGroupDataTest
    {

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();

            // Call
            var data = new OutputSnappedFeatureGroupData(model);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<OutputSnappedFeatureGroupData>>());
            Assert.That(data.Model, Is.SameAs(model));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            var thisGroupData = new OutputSnappedFeatureGroupData(
                thisModel);
            var otherGroupData = new OutputSnappedFeatureGroupData(
                otherModel);

            yield return new TestCaseData(thisGroupData, 
                                          thisGroupData, 
                                          true);
            yield return new TestCaseData(thisGroupData,
                                          new OutputSnappedFeatureGroupData(thisModel),
                                          true);
            yield return new TestCaseData(thisGroupData, 
                                          otherGroupData, 
                                          false);
            yield return new TestCaseData(thisGroupData, 
                                          null, 
                                          false);
            yield return new TestCaseData(thisGroupData, 
                                          new object(), 
                                          false);
        }

        [Test]
        [TestCaseSource(nameof(EqualsParams))]
        public void Equals_ExpectedResults(OutputSnappedFeatureGroupData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
 
    }
}