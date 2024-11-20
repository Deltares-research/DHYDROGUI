using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Providers
{
    [TestFixture]
    public class HydroAreaFeature2DCollectionTest
    {
        [Test]
        public void IsFeature2DCollectionCoordinateSystemUpdatedWhenAreaCoordinateSystemIsUpdated()
        {
            var hydroArea = new HydroArea();
            hydroArea.ObservationPoints.Add(new GroupableFeature2DPoint {Name = "ob1"});
            Feature2DCollection feature2DCollection = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.ObservationPoints, "ObservationPoint", "Not important", hydroArea.CoordinateSystem);
            Assert.That(hydroArea.CoordinateSystem, Is.Null);
            Assert.That(feature2DCollection.CoordinateSystem, Is.Null);
            hydroArea.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //rd new

            Assert.That(hydroArea.CoordinateSystem, Is.Not.Null);
            Assert.That(hydroArea.CoordinateSystem.Name, Is.EqualTo("Amersfoort / RD New"));
            Assert.That(hydroArea.CoordinateSystem.AuthorityCode, Is.EqualTo(28992));

            Assert.That(feature2DCollection.CoordinateSystem, Is.Not.Null);
            Assert.That(feature2DCollection.CoordinateSystem.Name, Is.EqualTo("Amersfoort / RD New"));
            Assert.That(feature2DCollection.CoordinateSystem.AuthorityCode, Is.EqualTo(28992));
        }
    }
}