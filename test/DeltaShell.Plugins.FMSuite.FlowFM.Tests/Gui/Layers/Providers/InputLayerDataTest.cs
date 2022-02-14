using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers
{
    [TestFixture]
    internal class InputLayerDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            const LayerDataDimension dimension = LayerDataDimension.Data2D;

            // Call
            var data = new InputLayerData(model, dimension);

            // Assert
            Assert.That(data, Is.InstanceOf<IEquatable<InputLayerData>>());
            Assert.That(data.Model, Is.SameAs(model));
            Assert.That(data.Dimension, Is.EqualTo(dimension));
        }

        private static IEnumerable<TestCaseData> EqualsParams()
        {
            var thisModel = Substitute.For<IWaterFlowFMModel>();
            var otherModel = Substitute.For<IWaterFlowFMModel>();

            var thisData1D = new InputLayerData(thisModel,
                                                LayerDataDimension.Data1D);
            var thisData2D = new InputLayerData(thisModel,
                                                LayerDataDimension.Data2D);
            var otherData1D = new InputLayerData(otherModel,
                                                 LayerDataDimension.Data1D);
            var otherData2D = new InputLayerData(otherModel,
                                                 LayerDataDimension.Data2D);

            yield return new TestCaseData(thisData1D, 
                                          thisData1D, 
                                          true);
            yield return new TestCaseData(thisData1D, 
                                          new InputLayerData(thisModel, LayerDataDimension.Data1D), 
                                          true);
            yield return new TestCaseData(thisData2D, 
                                          thisData2D, 
                                          true);
            yield return new TestCaseData(thisData2D, 
                                          new InputLayerData(thisModel, LayerDataDimension.Data2D), 
                                          true);
            yield return new TestCaseData(thisData1D, 
                                          thisData2D, 
                                          false);
            yield return new TestCaseData(thisData2D, 
                                          thisData1D, 
                                          false);
            yield return new TestCaseData(thisData1D, 
                                          otherData1D, 
                                          false);
            yield return new TestCaseData(thisData2D, 
                                          otherData2D, 
                                          false);
            yield return new TestCaseData(thisData1D, 
                                          otherData2D, 
                                          false);
            yield return new TestCaseData(thisData2D, 
                                          otherData1D, 
                                          false);
            yield return new TestCaseData(thisData2D, 
                                          null, 
                                          false);
            yield return new TestCaseData(thisData2D, 
                                          new object(), 
                                          false);
        }

        [Test]
        [TestCaseSource(nameof(EqualsParams))]
        public void Equals_ExpectedResults(InputLayerData a, 
                                           object b,
                                           bool expectedResult)
        {
            bool result = a.Equals(b);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}