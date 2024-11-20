using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.FeatureEditing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.FeatureEditing
{
    [TestFixture]
    public class WaterQualityFeatureEditorTest
    {
        [Test]
        public void TestDefaultConstructorExpectedValues()
        {
            // call
            var editor = new WaterQualityFeatureEditor();

            // assert
            CollectionAssert.IsEmpty(editor.SnapRules);
            Assert.IsNull(editor.CreateNewFeature);
        }

        [Test]
        public void TestAddNewFeatureByGeometryForLoadLayer()
        {
            // setup
            var model = new WaterQualityModel();
            ILayer loadLayer = new WaterQualityModelMapLayerProvider().CreateLayer(model.Loads, model);
            var editor = new WaterQualityFeatureEditor();

            var geometry = new Point(1.2, 3.4, 0.0); // Simulates map-clicked point

            // call
            IFeature loadFeature = editor.AddNewFeatureByGeometry(loadLayer, geometry);

            // assert
            Assert.IsInstanceOf<WaterQualityLoad>(loadFeature);
            var load = (WaterQualityLoad) loadFeature;
            Assert.AreEqual(geometry.X, load.X);
            Assert.AreEqual(geometry.Y, load.Y);
            Assert.IsNaN(load.Z);
            Assert.AreEqual(string.Empty, load.Name);
            Assert.AreEqual(string.Empty, load.LoadType);
        }

        [Test]
        public void TestAddNewFeatureByGeometryForObservationPointLayer()
        {
            // setup
            var model = new WaterQualityModel();
            ILayer observationPointLayer = new WaterQualityModelMapLayerProvider().CreateLayer(model.ObservationPoints, model);
            var editor = new WaterQualityFeatureEditor();

            var geometry = new Point(1.2, 3.4, 0.0); // Simulates map-clicked point

            // call
            IFeature observationPointFeature = editor.AddNewFeatureByGeometry(observationPointLayer, geometry);

            // assert
            Assert.IsInstanceOf<WaterQualityObservationPoint>(observationPointFeature);
            var observationPoint = (WaterQualityObservationPoint) observationPointFeature;
            Assert.AreEqual(geometry.X, observationPoint.X);
            Assert.AreEqual(geometry.Y, observationPoint.Y);
            Assert.IsNaN(observationPoint.Z);
            Assert.AreEqual(string.Empty, observationPoint.Name);
            Assert.AreEqual(ObservationPointType.SinglePoint, observationPoint.ObservationPointType);
        }

        [Test]
        public void TestCreateInteractorForPointGeometryVectorLayer()
        {
            // setup
            var mocks = new MockRepository();
            var layerStub = mocks.Stub<VectorLayer>();
            var featureStub = mocks.Stub<IFeature>();

            var coordinate = mocks.Stub<Coordinate>();
            var geometry = mocks.Stub<IPoint>();
            geometry.Stub(g => g.Coordinate).Return(coordinate);
            geometry.Stub(g => g.Coordinates).Return(new[]
            {
                coordinate
            });
            featureStub.Geometry = geometry;
            mocks.ReplayAll();

            var editor = new WaterQualityFeatureEditor();

            // call
            IFeatureInteractor interactor = editor.CreateInteractor(layerStub, featureStub);

            // assert
            Assert.IsInstanceOf<FeaturePointInteractor>(interactor);
            Assert.AreSame(layerStub, interactor.Layer);
            Assert.AreSame(featureStub, interactor.SourceFeature);
        }
    }
}