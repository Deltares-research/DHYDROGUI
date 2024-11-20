using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Common
{
    internal sealed class MockProperty : IModelProperty
    {
        public IModelProperty Mock { get; } = Substitute.For<IModelProperty>();

        public object Retrieve(IWaterFlowFMModel model) =>
            Mock.Retrieve(model);

        public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
            Mock.CreateLayer(creator, model);
    }

    [TestFixture]
    internal class InputPropertyLayerSubProviderTest
    {
        private IFlowFMLayerInstanceCreator instanceCreatorMock;
        private MockProperty propertyMock;


        [SetUp]
        public void BaseInit()
        {
            instanceCreatorMock = Substitute.For<IFlowFMLayerInstanceCreator>();
            propertyMock = new MockProperty();
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            void Construct() => new InputPropertyLayerSubProvider<MockProperty>(instanceCreatorMock);
            Assert.That(Construct, Throws.Nothing);
        }

        [Test]
        public void Constructor_InstanceCreatorNull_ThrowsArgumentNullException()
        {
            void Construct() => new InputPropertyLayerSubProvider<MockProperty>(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Construct);
            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        private static IEnumerable<TestCaseData> GetCanCreateLayerForData()
        {
            var model = Substitute.For<IWaterFlowFMModel>();
            var validSourceData = new object();

            void SetPropertyMock(IModelProperty property) =>
                property.Retrieve(model).Returns(validSourceData);

            var otherModel = Substitute.For<IWaterFlowFMModel>();

            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock, null, null, false);
            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock, new object(), null, false);
            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock, validSourceData, null, false);
            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock, validSourceData, new InputLayerData(otherModel, LayerDataDimension.Data1D), false);
            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock, new object(), new InputLayerData(model, LayerDataDimension.Data1D), false);
            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock, validSourceData, new InputLayerData(model, LayerDataDimension.Data1D), true);
        }

        [Test]
        [TestCaseSource(nameof(GetCanCreateLayerForData))]
        public void CanCreateLayerFor_Invalid_ExpectedResults(Action<IModelProperty> setPropertyMock, object sourceData, object parentData, bool expectedResult)
        {
            setPropertyMock(propertyMock.Mock);
            var provider = new InputPropertyLayerSubProvider<MockProperty>(instanceCreatorMock, propertyMock);
            bool canCreateLayerFor = provider.CanCreateLayerFor(sourceData, parentData);
            Assert.That(canCreateLayerFor, Is.EqualTo(expectedResult));
        }

        public static IEnumerable<TestCaseData> GetCreateLayerParams()
        {
            var model = Substitute.For<IWaterFlowFMModel>();

            var layer = Substitute.For<ILayer>();

            void SetPropertyMock(IModelProperty property) =>
                property.CreateLayer(Arg.Any<IFlowFMLayerInstanceCreator>(), Arg.Is(model))
                        .Returns(layer);

            void AssertValid(IModelProperty property, ILayer receivedLayer)
            {
                property.Received(1).CreateLayer(Arg.Any<IFlowFMLayerInstanceCreator>(), Arg.Is(model));
                Assert.That(receivedLayer, Is.SameAs(layer));
            }

            void AssertInvalid(IModelProperty property, ILayer receivedLayer)
            {
                property.DidNotReceiveWithAnyArgs().CreateLayer(null, null);
                Assert.That(receivedLayer, Is.Null);
            }

            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock,
                                          null,
                                          new InputLayerData(model, LayerDataDimension.Data1D),
                                          (Action<IModelProperty, ILayer>)AssertValid);

            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock,
                                          null,
                                          null,
                                          (Action<IModelProperty, ILayer>)AssertInvalid);
            yield return new TestCaseData((Action<IModelProperty>)SetPropertyMock,
                                          new object(),
                                          new object(),
                                          (Action<IModelProperty, ILayer>)AssertInvalid);
        }

        [Test]
        [TestCaseSource(nameof(GetCreateLayerParams))]
        public void CreateLayer_ReturnsExpectedResult(Action<IModelProperty> setProperty,
                                                      object sourceData,
                                                      object parentData,
                                                      Action<IModelProperty, ILayer> assertValid)
        {
            setProperty(propertyMock.Mock);
            var provider = new InputPropertyLayerSubProvider<MockProperty>(instanceCreatorMock, propertyMock);
            ILayer layer = provider.CreateLayer(sourceData, parentData);
            assertValid(propertyMock.Mock, layer);
        }

        public static IEnumerable<TestCaseData> GetGenerateChildLayerObjectsParams()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new object());
        }

        [Test]
        [TestCaseSource(nameof(GetGenerateChildLayerObjectsParams))]
        public void GenerateChildLayerObjects_ExpectedResult(object data)
        {
            var provider = new InputPropertyLayerSubProvider<MockProperty>(instanceCreatorMock);
            IList<object> children = provider.GenerateChildLayerObjects(data).ToList();
            Assert.That(children, Is.Empty);
        }
    }
}