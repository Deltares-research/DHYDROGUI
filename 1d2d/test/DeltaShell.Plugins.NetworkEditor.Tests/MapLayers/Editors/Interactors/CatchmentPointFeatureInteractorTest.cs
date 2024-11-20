using DelftTools.Hydro;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors.Interactors
{
    [TestFixture]
    public class CatchmentPointFeatureInteractorTest
    {
        private ILayer layer;
        private VectorStyle vectorStyle;
        private IEditableObject editableObject;
        
        [SetUp]
        public void Setup()
        {
            layer = Substitute.For<ILayer>();
            vectorStyle = new VectorStyle();
            editableObject = Substitute.For<IEditableObject>();;
        }
        
        [Test]
        public void SetupCatchmentPointFeatureInteractorAsGreenHouseAndCheckIfAddedToTrackers()
        {
            
            IFeature feature = new Catchment() {CatchmentType = CatchmentType.GreenHouse};
            feature.Geometry = Substitute.For<IGeometry>();

            var catchment = new CatchmentPointFeatureInteractor(layer, feature, vectorStyle, editableObject);
            
            Assert.That(catchment.Trackers.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void SetupCatchmentPointFeatureInteractorAsSubstituteWithNoGeometryAndCheckIfAddedToTrackers()
        {
            IFeature feature = Substitute.For<IFeature>();

            var catchment = new CatchmentPointFeatureInteractor(layer, feature, vectorStyle, editableObject);
            
            Assert.That(catchment.Trackers.Count, Is.EqualTo(1));
        }
    }
}