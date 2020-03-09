using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using GeoAPI.Extensions.CoordinateSystems;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class WaterFlowFM1D2DLinkFeatureCollectionTest
    {
        [Test]
        [Category("Quarantine")]
        public void GivenWaterFlowFMModel_WhenChangingCoordinateSystem_ThenWaterFlowFM1D2DLinkFeatureCollectionCoordinateSystemShouldBeInSync()
        {
            var mocks = new MockRepository();
            var fmModel = new WaterFlowFMModel();
            var coordinateSystem1 = mocks.Stub<ICoordinateSystem>();
            var coordinateSystem2 = mocks.Stub<ICoordinateSystem>();

            fmModel.CoordinateSystem = coordinateSystem1;

            var featureCollection = new WaterFlowFM1D2DLinkFeatureCollection(fmModel);
            Assert.That(featureCollection.CoordinateSystem, Is.EqualTo(fmModel.CoordinateSystem));

            fmModel.CoordinateSystem = coordinateSystem2;
            Assert.That(featureCollection.CoordinateSystem, Is.EqualTo(fmModel.CoordinateSystem));
        }
    }
}
