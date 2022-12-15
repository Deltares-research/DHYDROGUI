using System.Drawing;
using DeltaShell.Plugins.NetworkEditor.Gui;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    class NetworkLayerFactoryTest
    {
        [Test]
        public void GivenAnBitmapWhenGeneratingPointStyleThenExpectSameBitmapForStyle()
        {
            var bitmap = new Bitmap(10, 10);
            var style = NetworkLayerFactory.CreatePointStyle(bitmap);
            Assert.That(bitmap, Is.EqualTo(style.Symbol));
        }
        [Test]
        public void GivenNeedingAVectorPointStyleWhenGeneratingPointStyleThenExpectPointStyle()
        {
            var bitmap = new Bitmap(10, 10);
            var style = NetworkLayerFactory.CreatePointStyle(bitmap);
            Assert.That(style.GeometryType, Is.EqualTo(typeof(IPoint)));
        }
    }
}