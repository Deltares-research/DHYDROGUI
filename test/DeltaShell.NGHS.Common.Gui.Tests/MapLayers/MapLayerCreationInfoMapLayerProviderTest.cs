using DeltaShell.NGHS.Common.Gui.MapLayers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.Tests.MapLayers
{
    [TestFixture]
    public class MapLayerCreationInfoMapLayerProviderTest
    {
        [Test]
        public void GivenMapLayerCreationInfoMapLayerProvider_Calls_ShouldBeSendToMatchingMapLayerCreationInfoObject()
        {
            //Arrange
            var data = "test";
            var parentData = "parentData";

            var mapLayerCreationInfo = Substitute.For<IMapLayerCreationInfo>();
            mapLayerCreationInfo.SupportedType.Returns(typeof(string));
            mapLayerCreationInfo.CanBuildWithParent(parentData).Returns(true);

            var provider = new MapLayerCreationInfoMapLayerProvider(new [] { mapLayerCreationInfo });

            // Act & Assert
            provider.CanCreateLayerFor(data, parentData);
            mapLayerCreationInfo.Received(1).CanBuildWithParent(parentData);
            
            provider.ChildLayerObjects(data);
            mapLayerCreationInfo.Received().ChildLayerObjects(data);

            provider.CreateLayer(data, parentData);
            mapLayerCreationInfo.Received().CreateLayer(data, parentData);

            var layer = Substitute.For<ILayer>();
            provider.AfterCreate(layer,data, parentData, null);
            mapLayerCreationInfo.Received().AfterCreate(layer, data, parentData, null);
        }

        [Test]
        public void GivenMapLayerCreationInfoMapLayerProvider_ResolvingMapLayerCreationInfo_ShouldCheckTypeAndCanBuildWithParent()
        {
            //Arrange
            var stringData = "test";
            var doubleData = 1.0;
            var lowParent = 2.0;
            var highParent = 10.0;
            var parentData = "parentData";

            var stringLayerInfo = Substitute.For<IMapLayerCreationInfo>();
            stringLayerInfo.SupportedType.Returns(typeof(string));
            stringLayerInfo.CanBuildWithParent(parentData).Returns(true);

            var doubleBelow6LayerInfo = Substitute.For<IMapLayerCreationInfo>();
            doubleBelow6LayerInfo.SupportedType.Returns(typeof(double));
            doubleBelow6LayerInfo.CanBuildWithParent(lowParent).Returns(d => d.Arg<double>() <= 6.0);

            var doubleAbove6LayerInfo = Substitute.For<IMapLayerCreationInfo>();
            doubleAbove6LayerInfo.SupportedType.Returns(typeof(double));
            doubleAbove6LayerInfo.CanBuildWithParent(highParent).Returns(d => d.Arg<double>() > 6.0);

            var provider = new MapLayerCreationInfoMapLayerProvider(new[] { stringLayerInfo, doubleBelow6LayerInfo, doubleAbove6LayerInfo });

            // Act & Assert
            provider.CreateLayer(stringData, parentData);
            
            stringLayerInfo.Received(1).CreateLayer(stringData, parentData);
            doubleBelow6LayerInfo.Received(0).CreateLayer(stringData, parentData);
            doubleAbove6LayerInfo.Received(0).CreateLayer(stringData, parentData);

            provider.CreateLayer(doubleData, highParent);

            stringLayerInfo.Received(0).CreateLayer(doubleData, highParent);
            doubleBelow6LayerInfo.Received(0).CreateLayer(doubleData, highParent);
            doubleAbove6LayerInfo.Received(1).CreateLayer(doubleData, highParent);

            provider.CreateLayer(doubleData, lowParent);

            stringLayerInfo.Received(0).CreateLayer(doubleData, lowParent);
            doubleBelow6LayerInfo.Received(1).CreateLayer(doubleData, lowParent);
            doubleAbove6LayerInfo.Received(0).CreateLayer(doubleData, lowParent);
        }
    }
}