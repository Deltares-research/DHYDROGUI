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
    [TestFixture]
    internal abstract class BaseFunctionStoreDescriptionTestFixture<TDescription, TStore>
        where TDescription : BaseFunctionStoreDescription<TStore>, new()
        where TStore : IFunctionStore
    {
        public abstract ILayer GetLayer(IFlowFMLayerInstanceCreator creator);
        public abstract TStore GetStore();
        public abstract IEnumerable<object> GetChildren(TStore store);

        [Test]
        public void CreateLayer_ExpectedResult()
        {
            // Setup
            var creator = Substitute.For<IFlowFMLayerInstanceCreator>();
            var layer = Substitute.For<ILayer>();
            GetLayer(creator).Returns(layer);

            var description = new TDescription();

            // Call
            ILayer result = description.CreateLayer(creator);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            Assert.That(creator.ReceivedCalls().Count(), Is.EqualTo(1));
            GetLayer(creator.Received(1));
        }

        [Test]
        public void GenerateChildren_ExpectedResult()
        {
            // Setup
            TStore store = GetStore();
            var description = new TDescription();

            // Call
            IEnumerable<object> result = description.GenerateChildren(store);
            Assert.That(result, Is.EqualTo(GetChildren(store)));
        }
    }
}