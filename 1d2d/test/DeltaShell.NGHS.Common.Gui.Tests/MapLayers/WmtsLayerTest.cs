using System.Collections.Generic;
using BruTile;
using BruTile.Wmts;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using GeoAPI.CoordinateSystems.Transformations;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.NGHS.Common.Gui.Tests.MapLayers
{
    [TestFixture]
    public class WmtsLayerTest
    {
        [Test]
        public void Clone_ClonesInstanceCorrectly()
        {
            var originalTheme = Substitute.For<ITheme>();
            var clonedTheme = Substitute.For<ITheme>();
            originalTheme.Clone().Returns(clonedTheme);

            var layer = new WmtsLayer(new List<ITileSource>(), new ResourceUrl());
            layer.SelectedTileSource = Substitute.For<ITileSource>();
            layer.SelectedTileSource.Schema.Returns(Substitute.For<ITileSchema>());
            layer.DataSource = Substitute.For<IFeatureProvider>();
            layer.Theme = originalTheme;
            layer.ThemeGroup = "some_theme_group";
            layer.CustomRenderers = new List<IFeatureRenderer>();
            layer.CoordinateTransformation = Substitute.For<ICoordinateTransformation>();
            layer.Visible = true;
            layer.RenderOrder = 3;
            layer.Selectable = false;
            layer.ReadOnly = true;
            layer.CanBeRemovedByUser = false;
            layer.ShowAttributeTable = true;
            layer.ShowInLegend = false;
            layer.ShowInTreeView = true;
            layer.MinVisible = 1.23;
            layer.MaxVisible = 4.56;
            layer.NameIsReadOnly = false;
            layer.AutoUpdateThemeOnDataSourceChanged = true;
            layer.ExcludeFromMapExtent = false;

            // Call
            var clonedLayer = (WmtsLayer)layer.Clone();

            // Assert
            Assert.That(clonedLayer.TileSources, Is.SameAs(layer.TileSources));
            Assert.That(clonedLayer.SelectedTileSource, Is.SameAs(layer.SelectedTileSource));
            Assert.That(clonedLayer.DataSource, Is.SameAs(layer.DataSource));
            Assert.That(clonedLayer.Theme, Is.SameAs(clonedTheme));
            Assert.That(clonedLayer.ThemeGroup, Is.SameAs(layer.ThemeGroup));
            Assert.That(clonedLayer.CustomRenderers, Is.SameAs(layer.CustomRenderers));
            Assert.That(clonedLayer.CoordinateTransformation, Is.SameAs(layer.CoordinateTransformation));
            Assert.That(clonedLayer.Visible, Is.EqualTo(layer.Visible));
            Assert.That(clonedLayer.RenderOrder, Is.EqualTo(layer.RenderOrder));
            Assert.That(clonedLayer.Selectable, Is.EqualTo(layer.Selectable));
            Assert.That(clonedLayer.ReadOnly, Is.EqualTo(layer.ReadOnly));
            Assert.That(clonedLayer.CanBeRemovedByUser, Is.EqualTo(layer.CanBeRemovedByUser));
            Assert.That(clonedLayer.ShowAttributeTable, Is.EqualTo(layer.ShowAttributeTable));
            Assert.That(clonedLayer.ShowInLegend, Is.EqualTo(layer.ShowInLegend));
            Assert.That(clonedLayer.ShowInTreeView, Is.EqualTo(layer.ShowInTreeView));
            Assert.That(clonedLayer.MinVisible, Is.EqualTo(layer.MinVisible));
            Assert.That(clonedLayer.MaxVisible, Is.EqualTo(layer.MaxVisible));
            Assert.That(clonedLayer.NameIsReadOnly, Is.EqualTo(layer.NameIsReadOnly));
            Assert.That(clonedLayer.AutoUpdateThemeOnDataSourceChanged, Is.EqualTo(layer.AutoUpdateThemeOnDataSourceChanged));
        }

        [Test]
        public void GetName_GetsTileSourceName()
        {
            // Setup
            var tileSource = Substitute.For<ITileSource>();
            tileSource.Name.Returns("some_tile_source_name");
            var layer = new WmtsLayer(new List<ITileSource> { tileSource }, new ResourceUrl());
            layer.SelectedTileSource = tileSource;

            // Call
            string result = layer.Name;

            // Assert
            Assert.That(result, Is.EqualTo("some_tile_source_name"));
        }
    }
}