using System.Collections;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Layers
{
    [TestFixture]
    public class SnappedFeatureCollectionTest
    {
        [Test]
        public void AddToOriginalFeatures_AddsANewSnappedFeature()
        {
            var gridOperationApi = Substitute.For<IGridOperationApi>();
            var hydroArea = new HydroArea();
            var originalFeatures = new EventedList<IFeature>();
            var vectorStyle = new VectorStyle();
            var layerName = "";
            var snapApiFeatureType = "";

            var snappedFeatureCollection = new SnappedFeatureCollection(gridOperationApi, hydroArea, originalFeatures, vectorStyle, layerName, snapApiFeatureType);

            IFeature feature = Substitute.For<IFeature, INameable>();
            ((INameable)feature).Name = "some_feature_name";

            // Sets dirty to false
            var layer = Substitute.For<ILayer>();
            var map = Substitute.For<IMap>();
            layer.Map = map;
            map.GetAllVisibleLayers(false).Returns(new[]
            {
                layer
            });
            snappedFeatureCollection.Layer = layer;
            IList _ = snappedFeatureCollection.Features;

            // Call
            originalFeatures.Add(feature);

            // Assert
            Assert.That(snappedFeatureCollection.Features, Has.Count.EqualTo(1));
            var snappedFeature = (Feature2D)snappedFeatureCollection.Features[0];
            Assert.That(snappedFeature, Is.Not.SameAs(feature));
            Assert.That(snappedFeature.Name, Is.EqualTo("some_feature_name"));
        }
    }
}