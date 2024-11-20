using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="LayerSubProviderBaseFixture{TProvider,TCanCreateLayerForSource,TCreateLayerSource,TGenerateChildLayerObjectsSource}"/>
    /// provides the implementation for the tests of implementations of the <see cref="ILayerSubProvider"/>.
    /// The test fixture should be defined with the exact type of provider, and 3 enumerator classes, which
    /// provide the test cases for each of three methods of the <see cref="ILayerSubProvider"/>.
    ///
    /// Common asserts are provided in the <see cref="CommonAsserts"/> class.
    /// provides 
    /// </summary>
    /// <typeparam name="TProvider">The exact type of provider being tested.</typeparam>
    /// <typeparam name="TCanCreateLayerForSource">The source of CanCreateLayerFor parameters.</typeparam>
    /// <typeparam name="TCreateLayerSource">The source of CreateLayer parameters.</typeparam>
    /// <typeparam name="TGenerateChildLayerObjectsSource">The source of GenerateChildLayerObjects parameters.</typeparam>
    [TestFixture]
    public abstract class LayerSubProviderBaseFixture<TProvider, 
                                                      TCanCreateLayerForSource,
                                                      TCreateLayerSource, 
                                                      TGenerateChildLayerObjectsSource> 
        where TProvider : ILayerSubProvider
        where TCanCreateLayerForSource : IEnumerable<TestCaseData>, new()
        where TCreateLayerSource : IEnumerable<TestCaseData>, new()
        where TGenerateChildLayerObjectsSource : IEnumerable<TestCaseData>, new()
    {
        protected IFlowFMLayerInstanceCreator instanceCreatorMock;

        protected abstract TProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator);

        [SetUp]
        public void BaseInit()
        {
            instanceCreatorMock = Substitute.For<IFlowFMLayerInstanceCreator>();
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            void Construct() => CreateDefault(instanceCreatorMock);
            Assert.That(Construct, Throws.Nothing);
        }

        [Test]
        public void Constructor_InstanceCreatorNull_ThrowsArgumentNullException()
        {
            void Construct() => CreateDefault(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Construct);
            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        public static IEnumerable<TestCaseData> GetCanCreateLayerForParams() => 
            new TCanCreateLayerForSource();

        [Test]
        [TestCaseSource(nameof(GetCanCreateLayerForParams))]
        public void CanCreateLayerFor_ReturnsExpectedResult(object sourceData, object parentData, bool expectedResult)
        {
            TProvider provider = CreateDefault(instanceCreatorMock);
            bool canCreateLayerFor = provider.CanCreateLayerFor(sourceData, parentData);
            Assert.That(canCreateLayerFor, Is.EqualTo(expectedResult));
        }

        public static IEnumerable<TestCaseData> GetCreateLayerParams() => 
            new TCreateLayerSource();

        [Test]
        [TestCaseSource(nameof(GetCreateLayerParams))]
        public void CreateLayer_ReturnsExpectedResult(object sourceData, 
                                                      object parentData, 
                                                      Action<IFlowFMLayerInstanceCreator> configureMock,
                                                      Action<IFlowFMLayerInstanceCreator, ILayer> assertValid)
        {
            configureMock(instanceCreatorMock);
            TProvider provider = CreateDefault(instanceCreatorMock);
            ILayer layer = provider.CreateLayer(sourceData, parentData);
            assertValid(instanceCreatorMock, layer);
        }

        public static IEnumerable<TestCaseData> GetGenerateChildLayerObjectsParams() => 
            new TGenerateChildLayerObjectsSource();

        [Test]
        [TestCaseSource(nameof(GetGenerateChildLayerObjectsParams))]
        public void GenerateChildLayerObjects_ExpectedResult(object data, Action<IList<object>> assertValid)
        {
            TProvider provider = CreateDefault(instanceCreatorMock);
            IList<object> children = provider.GenerateChildLayerObjects(data).ToArray();
            assertValid(children);
        }

        public static class CommonAsserts
        {
            public static Action<IList<object>> NoChildren() =>
                returnedChildren => Assert.That(returnedChildren, Is.Empty);

            public static Action<IList<object>> ChildrenEqualTo(IList<object> expected) =>
                returnedChildren => Assert.That(returnedChildren, Is.EqualTo(expected));

            public static Action<IFlowFMLayerInstanceCreator, ILayer> NoLayerCreated() =>
                (creator, layer) =>
                {
                    Assert.That(layer, Is.Null);
                    Assert.That(creator.ReceivedCalls(), Is.Empty);
                };

            public static Action<IFlowFMLayerInstanceCreator, ILayer> CreatedLayer(ILayer expectedLayer,
                                                                                   Action<IFlowFMLayerInstanceCreator> call) =>
                (creator, layer) =>
                {
                    Assert.That(layer, Is.SameAs(expectedLayer));
                    Assert.That(creator.ReceivedCalls().Count(), Is.EqualTo(1));
                    call(creator.Received());
                };
        }
    }
}