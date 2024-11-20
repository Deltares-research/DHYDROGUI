using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Common
{
    [TestFixture]
    internal abstract class ModelPropertyBaseTestFixture<TProperty, TData> 
        where TProperty : IModelProperty, new()
    {
        protected abstract TData Data { get; }
        protected abstract TData GetValue(IWaterFlowFMModel model);

        [Test]
        public void Retrieve_ReturnsExpectedData()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            GetValue(model).Returns(Data);
            var property = new TProperty();

            // Call
            object result = property.Retrieve(model);

            // Assert
            Assert.That(result, Is.SameAs(Data));
        }

        protected abstract ILayer GetLayer(
            IFlowFMLayerInstanceCreator creator,
            IWaterFlowFMModel model);

        [Test]
        public void CreateLayer_ExpectedLayer()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var creator = Substitute.For<IFlowFMLayerInstanceCreator>();
            var layer = Substitute.For<ILayer>();

            GetLayer(creator, model).Returns(layer);

            var property = new TProperty();

            // Call
            ILayer result = property.CreateLayer(creator, model);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            GetLayer(creator.Received(1), model);
        }
    }
}