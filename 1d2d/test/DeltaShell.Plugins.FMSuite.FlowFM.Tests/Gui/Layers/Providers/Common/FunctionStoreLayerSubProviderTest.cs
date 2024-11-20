using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Common
{
    public interface IMockFunctionStore : IFunctionStore { }

    public interface IFunctionStoreDescription<in T>
    {
        ILayer CreateLayer(IFlowFMLayerInstanceCreator creator);
        IEnumerable<object> GenerateChildren(T store);
    }

    internal class MockFunctionStoreDescription :
        BaseFunctionStoreDescription<IMockFunctionStore>
    {
        public IFunctionStoreDescription<IMockFunctionStore> Mock { get; } =
            Substitute.For<IFunctionStoreDescription<IMockFunctionStore>>();

        public override ILayer CreateLayer(IFlowFMLayerInstanceCreator creator) =>
            Mock.CreateLayer(creator);

        public override IEnumerable<object> GenerateChildren(IMockFunctionStore store) =>
            Mock.GenerateChildren(store);
    }

    [TestFixture]
    internal class FunctionStoreLayerSubProviderTest
    {
        
        private IFlowFMLayerInstanceCreator instanceCreatorMock;
        private MockFunctionStoreDescription descriptionMock;


        [SetUp]
        public void BaseInit()
        {
            instanceCreatorMock = Substitute.For<IFlowFMLayerInstanceCreator>();
            descriptionMock = new MockFunctionStoreDescription();
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            void Construct() => new FunctionStoreLayerSubProvider<MockFunctionStoreDescription, 
                                                                  IMockFunctionStore>(instanceCreatorMock);
            Assert.That(Construct, Throws.Nothing);
        }

        [Test]
        public void Constructor_InstanceCreatorNull_ThrowsArgumentNullException()
        {
            void Construct() => new FunctionStoreLayerSubProvider<MockFunctionStoreDescription, IMockFunctionStore>(null);

            var exception = Assert.Throws<ArgumentNullException>(Construct);
            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        private static IEnumerable<TestCaseData> GetCanCreateLayerForData()
        {
            yield return new TestCaseData(null, null, false);
            yield return new TestCaseData(new object(), null, false);
            yield return new TestCaseData(Substitute.For<IFunctionStore>(), null, false);
            yield return new TestCaseData(Substitute.For<IMockFunctionStore>(), null, true);
            yield return new TestCaseData(Substitute.For<IMockFunctionStore>(), new object(), true);
        }

        [Test]
        [TestCaseSource(nameof(GetCanCreateLayerForData))]
        public void CanCreateLayerFor_Invalid_ExpectedResults(object sourceData, object parentData, bool expectedResult)
        {
            var provider = 
                new FunctionStoreLayerSubProvider<MockFunctionStoreDescription, 
                                                  IMockFunctionStore>(instanceCreatorMock, descriptionMock);
            bool canCreateLayerFor = provider.CanCreateLayerFor(sourceData, parentData);
            Assert.That(canCreateLayerFor, Is.EqualTo(expectedResult));
        }

        public static IEnumerable<TestCaseData> GetCreateLayerParams()
        {
            var layer = Substitute.For<ILayer>();

            void SetDescriptionMock(IFunctionStoreDescription<IMockFunctionStore> description) =>
                description.CreateLayer(Arg.Any<IFlowFMLayerInstanceCreator>()).Returns(layer);

            void AssertValid(IFunctionStoreDescription<IMockFunctionStore> description, ILayer receivedLayer)
            {
                description.Received(1).CreateLayer(Arg.Any<IFlowFMLayerInstanceCreator>());
                Assert.That(receivedLayer, Is.SameAs(layer));
            }

            void AssertInvalid(IFunctionStoreDescription<IMockFunctionStore> property, ILayer receivedLayer)
            {
                property.DidNotReceiveWithAnyArgs().CreateLayer(null);
                Assert.That(receivedLayer, Is.Null);
            }

            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>)SetDescriptionMock, 
                                          Substitute.For<IMockFunctionStore>(), 
                                          null,
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, ILayer>)AssertValid);

            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>)SetDescriptionMock,
                                          null,
                                          null,
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, ILayer>)AssertInvalid);
            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>)SetDescriptionMock,
                                          new object(),
                                          new object(),
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, ILayer>)AssertInvalid);
        }

        [Test]
        [TestCaseSource(nameof(GetCreateLayerParams))]
        public void CreateLayer_ReturnsExpectedResult(Action<IFunctionStoreDescription<IMockFunctionStore>> setProperty,
                                                      object sourceData,
                                                      object parentData,
                                                      Action<IFunctionStoreDescription<IMockFunctionStore>, ILayer> assertValid)
        {
            setProperty(descriptionMock.Mock);
            var provider = 
                new FunctionStoreLayerSubProvider<MockFunctionStoreDescription, 
                                                  IMockFunctionStore>(instanceCreatorMock, descriptionMock);
            ILayer layer = provider.CreateLayer(sourceData, parentData);
            assertValid(descriptionMock.Mock, layer);
        }

        public static IEnumerable<TestCaseData> GetGenerateChildLayerObjectsParams()
        {
            var functionStore = Substitute.For<IMockFunctionStore>();
            var result = new List<object>()
            {
                new object(),
                new object(),
                new object(),
            };

            void SetDescriptionMock(IFunctionStoreDescription<IMockFunctionStore> description) =>
                description.GenerateChildren(functionStore).Returns(result);

            void ExpectedChildren(IFunctionStoreDescription<IMockFunctionStore> mock, IEnumerable<object> generatedChildren)
            {
                Assert.That(generatedChildren, Is.EqualTo(result));
                Assert.That(mock.ReceivedCalls().Count(), Is.EqualTo(1));
                mock.Received(1).GenerateChildren(functionStore);
            }

            void NoChildren(IFunctionStoreDescription<IMockFunctionStore> mock, IEnumerable<object> generatedChildren)
            {
                Assert.That(generatedChildren, Is.Empty);
                mock.DidNotReceiveWithAnyArgs().GenerateChildren(null);
            }

            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>) SetDescriptionMock, 
                                          null,
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, IEnumerable<object>>) NoChildren);
            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>) SetDescriptionMock, 
                                          new object(), 
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, IEnumerable<object>>) NoChildren);
            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>) SetDescriptionMock, 
                                          Substitute.For<IFunctionStore>(),
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, IEnumerable<object>>) NoChildren);
            yield return new TestCaseData((Action<IFunctionStoreDescription<IMockFunctionStore>>) SetDescriptionMock, 
                                          functionStore,
                                          (Action<IFunctionStoreDescription<IMockFunctionStore>, IEnumerable<object>>) ExpectedChildren);
        }

        [Test]
        [TestCaseSource(nameof(GetGenerateChildLayerObjectsParams))]
        public void GenerateChildLayerObjects_ExpectedResult(Action<IFunctionStoreDescription<IMockFunctionStore>> setProperty,
                                                             object data,
                                                             Action<IFunctionStoreDescription<IMockFunctionStore>, IEnumerable<object>> assertValid)
        {
            setProperty(descriptionMock.Mock);
            var provider = 
                new FunctionStoreLayerSubProvider<MockFunctionStoreDescription, 
                                                  IMockFunctionStore>(instanceCreatorMock, descriptionMock);
            IList<object> children = provider.GenerateChildLayerObjects(data).ToList();
            assertValid(descriptionMock.Mock, children);
        }
    }
}