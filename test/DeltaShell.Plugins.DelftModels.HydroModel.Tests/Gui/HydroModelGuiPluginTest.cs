using System;
using System.Linq;
using System.Threading;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.GraphicsProviders;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
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

        [Test]
        public void GivenHydroModelGuiPlugin_HydroModelFails_ShouldShowValidationView()
        {
            //Arrange
            var plugin = new HydroModelGuiPlugin();
            var gui = Substitute.For<IGui>();
            var guiCommandHandler = Substitute.For<IGuiCommandHandler>();
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var hydroModel = new HydroModel();

            gui.Application.Returns(app);
            gui.CommandHandler.Returns(guiCommandHandler);
            app.ActivityRunner.Returns(activityRunner);

            plugin.Gui = gui;

            // Act
            activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(hydroModel, new ActivityStatusChangedEventArgs(ActivityStatus.Cleaning, ActivityStatus.Failed));

            // Assert
            guiCommandHandler.Received().OpenView(hydroModel, Arg.Is<Type>(t => t.Implements(typeof(IView))));
        }
        
        [Test]
        public void GetViewInfoOBject_ForDHydroConfigXmlExporter_IsCorrectlyConfiguredWhenAfterCreateIsInvoked()
        {
            using (var plugin = new HydroModelGuiPlugin())
            {
                plugin.Gui = Substitute.For<IGui>();
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .Single(vi => vi.DataType == typeof(DHydroConfigXmlExporter));
                var exporter = new DHydroConfigXmlExporter();
                var exportedDialog = new DHydroExporterDialog();

                Assert.That(exportedDialog.Gui, Is.Null);
                Assert.That(exportedDialog.FolderDialogService, Is.Null);

                viewInfo.AfterCreate.Invoke(exportedDialog, exporter);

                Assert.That(exportedDialog.Gui, Is.SameAs(plugin.Gui));
                Assert.That(exportedDialog.FolderDialogService, Is.Not.Null);
            }
        }
    }
}