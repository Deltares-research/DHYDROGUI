using System.Threading;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.GraphicsProviders;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class HydroModelGuiPluginTest
    {
        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            var hydroModelGuiPlugin = new HydroModelGuiPlugin();
            
            StringAssert.AreEqualIgnoringCase("Hydro Model (UI)",hydroModelGuiPlugin.Name);
            StringAssert.AreEqualIgnoringCase("Hydro Model Plugin (UI)",hydroModelGuiPlugin.DisplayName);
            StringAssert.AreEqualIgnoringCase("Provides functionality to create and run integrated models.",hydroModelGuiPlugin.Description);
            StringAssert.AreEqualIgnoringCase("1.1.0.0",hydroModelGuiPlugin.FileFormatVersion);
            
            Assert.IsInstanceOf<HydroModelGuiGraphicsProvider>(hydroModelGuiPlugin.GraphicsProvider);
        }
        
        
        [Test]
        public void OnViewRemoved_ResetsFeatureCoverageLayers()
        {
            // Setup
            var coverage = new FeatureCoverage();
            var coverageLayer = new FeatureCoverageLayer {Coverage = coverage};
            var mapView = new MapView();
            mapView.Map.Layers.Add(coverageLayer);

            var plugin = new HydroModelGuiPlugin();

            // Call
            plugin.OnViewRemoved(mapView);

            // Assert
            Assert.That(coverageLayer.Coverage, Is.Not.SameAs(coverage));
            Assert.That(coverageLayer.Coverage.Components, Has.Count.EqualTo(1));
        }

        [Test]
        public void OnViewRemoved_ResetsNetworkCoverageGroupLayer()
        {
            // Setup
            var coverage = new NetworkCoverage();
            var coverageLayer = new NetworkCoverageGroupLayer {Coverage = coverage};
            var mapView = new MapView();
            mapView.Map.Layers.Add(coverageLayer);

            var plugin = new HydroModelGuiPlugin();

            // Call
            plugin.OnViewRemoved(mapView);

            // Assert
            Assert.That(coverageLayer.Coverage, Is.Not.SameAs(coverage));
            Assert.That(coverageLayer.Coverage.Components, Has.Count.EqualTo(1));
        }
    }
}