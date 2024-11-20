using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput1D
{
    [TestFixture]
    internal class InputFeatureGroupLayerDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            const FeatureType featureType = FeatureType.Friction;

            // Call
            var data = new InputFeatureGroupLayerData(model, featureType);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<InputFeatureGroupLayerData>>());
            Assert.That(data.Model, Is.SameAs(model));
            Assert.That(data.FeatureGroupType, Is.EqualTo(featureType));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            var thisDataFriction = new InputFeatureGroupLayerData(
                thisModel,
                FeatureType.Friction);
            var thisDataInitialConditions = new InputFeatureGroupLayerData(
                thisModel,
                FeatureType.InitialConditions);
            var otherDataFriction = new InputFeatureGroupLayerData(
                otherModel,
                FeatureType.Friction);
            var otherDataInitialConditions = new InputFeatureGroupLayerData(
                otherModel,
                FeatureType.InitialConditions);

            yield return new TestCaseData(thisDataFriction, 
                                          thisDataFriction, 
                                          true);
            yield return new TestCaseData(thisDataFriction, 
                                          new InputFeatureGroupLayerData(thisModel, FeatureType.Friction), 
                                          true);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          thisDataInitialConditions, 
                                          true);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          new InputFeatureGroupLayerData(thisModel, FeatureType.InitialConditions), 
                                          true);
            yield return new TestCaseData(thisDataFriction, 
                                          thisDataInitialConditions, 
                                          false);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          thisDataFriction,
                                          false);
            yield return new TestCaseData(thisDataFriction, 
                                          otherDataFriction, 
                                          false);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          otherDataInitialConditions, 
                                          false);
            yield return new TestCaseData(thisDataFriction, 
                                          otherDataInitialConditions, 
                                          false);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          otherDataFriction, 
                                          false);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          null, 
                                          false);
            yield return new TestCaseData(thisDataInitialConditions, 
                                          new object(), 
                                          false);
        }

        [Test]
        [TestCaseSource(nameof(EqualsParams))]
        public void Equals_ExpectedResults(InputFeatureGroupLayerData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}