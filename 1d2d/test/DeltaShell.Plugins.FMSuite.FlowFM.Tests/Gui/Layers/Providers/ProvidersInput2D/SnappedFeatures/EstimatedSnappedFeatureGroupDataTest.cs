using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    [TestFixture]
    internal class EstimatedSnappedFeatureGroupDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();

            const int snapVersion = 13;
            model.SnapVersion.Returns(snapVersion);

            // Call
            var data = new EstimatedSnappedFeatureGroupData(model);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<EstimatedSnappedFeatureGroupData>>());
            Assert.That(data.Model, Is.SameAs(model));
            Assert.That(data.SnapVersion, Is.EqualTo(snapVersion));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            const int snapVersion = 13;
            thisModel.SnapVersion.Returns(snapVersion);
            otherModel.SnapVersion.Returns(snapVersion);

            var thisGroupData = new EstimatedSnappedFeatureGroupData(
                thisModel);
            var otherGroupData = new EstimatedSnappedFeatureGroupData(
                otherModel);

            yield return new TestCaseData(thisGroupData, 
                                          thisGroupData, 
                                          true);
            yield return new TestCaseData(thisGroupData,
                                          new EstimatedSnappedFeatureGroupData(thisModel),
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
        public void Equals_ExpectedResults(EstimatedSnappedFeatureGroupData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void Equals_SameModelButDifferentSnapVersion_IsFalse()
        {
            var model = Substitute.For<IWaterFlowFMModel>();

            const int initialSnapVersion = 13;
            model.SnapVersion.Returns(initialSnapVersion);
            var initialGroupData = new EstimatedSnappedFeatureGroupData(model);

            const int nextSnapVersion = 15;
            model.SnapVersion.Returns(nextSnapVersion);
            var nextGroupData = new EstimatedSnappedFeatureGroupData(model);

            Assert.That(initialGroupData.SnapVersion, Is.EqualTo(initialSnapVersion));
            Assert.That(nextGroupData.SnapVersion, Is.EqualTo(nextSnapVersion));
            Assert.That(initialGroupData.Equals(nextGroupData), Is.False);
        }
    }
}