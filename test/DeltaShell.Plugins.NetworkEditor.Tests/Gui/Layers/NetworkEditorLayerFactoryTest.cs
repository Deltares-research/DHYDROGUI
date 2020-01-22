using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers
{
    [TestFixture]
    public class NetworkEditorLayerFactoryTest
    {
        [Test]
        public void CreateAreaLayer_ValidHydroArea_ReturnsExpectedHydroAreaLayer()
        {
            // Arrange
            var hydroArea = new HydroArea();

            // Act
            var areaLayer = NetworkEditorLayerFactory.CreateAreaLayer(hydroArea) as AreaLayer;

            // Assert
            Assert.IsNotNull(areaLayer);
            Assert.That(areaLayer.HydroArea, Is.SameAs(hydroArea));
            Assert.IsTrue(areaLayer.NameIsReadOnly);
        }

        [Test]
        public void CreateThinDamsLayer_ValidHydroArea_ReturnsExpectedVectorLayer()
        {
            // Arrange
            var hydroArea = new HydroArea();
            hydroArea.ThinDams.Add(new ThinDam2D());
            hydroArea.ThinDams.Add(new ThinDam2D());

            // Act
            var vectorLayer = NetworkEditorLayerFactory.CreateThinDamsLayer(hydroArea) as VectorLayer;

            // Assert
            Assert.IsNotNull(vectorLayer);
            Assert.That(vectorLayer.FeatureEditor is Feature2DEditor);

            Assert.That(vectorLayer.Style.Line.Color, Is.EqualTo(Color.Red));
            Assert.That(vectorLayer.Style.Line.Width, Is.EqualTo(3f));
            Assert.That(vectorLayer.Style.GeometryType, Is.EqualTo(typeof(ILineString)));

            var hydroAreaFeature2DCollection = vectorLayer.DataSource as HydroAreaFeature2DCollection;
            Assert.IsNotNull(hydroAreaFeature2DCollection);
            Assert.That(hydroAreaFeature2DCollection.Features, Is.EqualTo(hydroArea.ThinDams));
            Assert.That(hydroAreaFeature2DCollection.ModelName, Is.EqualTo("NetworkEditorModelName"));
            Assert.That(hydroAreaFeature2DCollection.CoordinateSystem, Is.SameAs(hydroArea.CoordinateSystem));

            Assert.IsTrue(vectorLayer.NameIsReadOnly);

        }

        [Test]
        [TestCaseSource(nameof(GetNetworkEditorLayerFactoryCallsWithNullArgument))]
        public void CreateNetworkEditorLayer_HydroAreaNull_ThrowsArgumentNullException(TestDelegate call)
        {
            // Act
            var exception = Assert.Throws<ArgumentNullException>(call);

            // Assert
            Assert.That(exception, Has.Property("ParamName").EqualTo("hydroArea"));
        }

        private static IEnumerable<TestDelegate> GetNetworkEditorLayerFactoryCallsWithNullArgument()
        {
            yield return delegate { NetworkEditorLayerFactory.CreateAreaLayer(null); };
            yield return delegate { NetworkEditorLayerFactory.CreateThinDamsLayer(null); };
        }
    }
}