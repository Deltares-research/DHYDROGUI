using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    [TestFixture]
    internal class EstimatedSnappedFeatureDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            const EstimatedSnappedFeatureType featureType = 
                EstimatedSnappedFeatureType.DryAreas;

            // Call
            var data = new EstimatedSnappedFeatureData(model, featureType);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<EstimatedSnappedFeatureData>>());
            Assert.That(data.Model, Is.SameAs(model));
            Assert.That(data.FeatureType, Is.EqualTo(featureType));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            var thisDataDryAreas = new EstimatedSnappedFeatureData(
                thisModel,
                EstimatedSnappedFeatureType.DryAreas);
            var thisDataFixedWeirs = new EstimatedSnappedFeatureData(
                thisModel,
                EstimatedSnappedFeatureType.FixedWeirs);
            var otherDataDryAreas = new EstimatedSnappedFeatureData(
                otherModel,
                EstimatedSnappedFeatureType.DryAreas);
            var otherDataFixedWeirs = new EstimatedSnappedFeatureData(
                otherModel,
                EstimatedSnappedFeatureType.FixedWeirs);

            yield return new TestCaseData(thisDataDryAreas, 
                                          thisDataDryAreas, 
                                          true);
            yield return new TestCaseData(thisDataDryAreas, 
                                          new EstimatedSnappedFeatureData(
                                              thisModel, 
                                              EstimatedSnappedFeatureType.DryAreas), 
                                          true);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          thisDataFixedWeirs, 
                                          true);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          new EstimatedSnappedFeatureData(
                                              thisModel, 
                                              EstimatedSnappedFeatureType.FixedWeirs), 
                                          true);
            yield return new TestCaseData(thisDataDryAreas, 
                                          thisDataFixedWeirs, 
                                          false);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          thisDataDryAreas,
                                          false);
            yield return new TestCaseData(thisDataDryAreas, 
                                          otherDataDryAreas, 
                                          false);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          otherDataFixedWeirs, 
                                          false);
            yield return new TestCaseData(thisDataDryAreas, 
                                          otherDataFixedWeirs, 
                                          false);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          otherDataDryAreas, 
                                          false);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          null, 
                                          false);
            yield return new TestCaseData(thisDataFixedWeirs, 
                                          new object(), 
                                          false);
        }

        [Test]
        [TestCaseSource(nameof(EqualsParams))]
        public void Equals_ExpectedResults(EstimatedSnappedFeatureData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
    }
}