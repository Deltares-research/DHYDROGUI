using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers
{
    [TestFixture]
    internal class OutputLayerDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();

            // Call
            var data = new OutputLayerData(model);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<OutputLayerData>>());
            Assert.That(data.Model, Is.SameAs(model));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            var thisData = new OutputLayerData(thisModel);
            var otherData = new OutputLayerData(otherModel);

            yield return new TestCaseData(thisData, 
                                          thisData, 
                                          true);
            yield return new TestCaseData(thisData, 
                                          new OutputLayerData(thisModel),
                                          true);
            yield return new TestCaseData(thisData, 
                                          otherData, 
                                          false);
            yield return new TestCaseData(thisData, 
                                          otherData, 
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
        public void Equals_ExpectedResults(OutputLayerData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}