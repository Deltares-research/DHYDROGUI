using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
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
        }


        private static IEnumerable<TestCaseData> BeforeTagTestCaseData
        {
            get
            {
                var model = new HydroModel {Name = "Blastoise"};
                yield return new TestCaseData(null, model);
                yield return new TestCaseData(new HydroModel {Name = "Squirtle"}, model);
                yield return new TestCaseData(model, model);
            }
        }

        [Test]
        public void GetViewInfoForHydroModel_ReturnsExpectedViewInfoConfiguration()
        {
            // Setup
            using (var plugin = new HydroModelGuiPlugin())
            {
                // Call
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(HydroModel));

                // Assert
                Assert.That(viewInfo.ViewType, Is.EqualTo(typeof(HydroModelSettings)));
                Assert.That(viewInfo.Description, Is.EqualTo("Hydro Model Settings"));
                Assert.That(viewInfo.GetViewName, Is.Not.Null);
                Assert.That(viewInfo.CompositeViewType, Is.EqualTo(typeof(ProjectItemMapView)));
                Assert.That(viewInfo.GetCompositeViewData, Is.Not.Null);
                Assert.That(viewInfo.AfterCreate, Is.Not.Null);
            }
        }

        [Test]
        public void ViewInfoForHydroModelGetViewName_WithHydroModel_ReturnsExpectedValue()
        {
            // Setup
            using (var model = new HydroModel {Name = "Name of HydroModel"})
            using (var plugin = new HydroModelGuiPlugin())
            {
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(HydroModel));

                // Call
                string viewName = viewInfo.GetViewName(null, model);

                // Assert
                Assert.That(viewName, Is.EqualTo($"{model.Name} Settings"));
            }
        }

        [Test]
        public void ViewInfoForHydroModelGetCompositeViewData_WithHydroModel_ReturnsExpectedResult()
        {
            // Setup
            using (var model = new HydroModel())
            using (var plugin = new HydroModelGuiPlugin())
            {
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(HydroModel));

                // Call
                object compositeViewData = viewInfo.GetCompositeViewData(model);

                // Assert
                Assert.That(compositeViewData, Is.SameAs(model));
            }
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

        /// <summary>
        /// This test indirectly verifies that the map is forced to re-render after the view is created. This causes a double
        /// Render()
        /// call to the map.
        /// The test verifies the AfterCreate call by mimicking the situation. There is no need to fully configure the plugin,
        /// because:
        /// - MapView.GetLayerForData is set. If it returns any arbitrary MapLayer --> This ProjectItemMapView belongs to the
        /// argument
        /// (in this case a HydroModel for which the view is created)
        /// - The GUI and its corresponding ViewList can be substituted as desired. For the AfterCreate method to work, the view is
        /// retrieved from the ViewList and further operations are done based on the HydroModel argument that was used to retrieve
        /// (In this case, it only forces the corresponding ProjectItemMapView.MapView.Map to render twice to force the grid to
        /// become visible)
        /// This test does NOT verify additional properties being set on a HydroModelSettings.
        /// </summary>
        [Test]
        public void GivenPluginWithProjectItemMapViewForHydroModel_WhenViewInfoForHydroModelAfterCreateCalled_ThenMapViewDoubleRefreshed()
        {
            // Setup
            const int expectedRenderCalls = 2;

            var viewList = new ViewList(Substitute.For<IDockingManager>(), ViewLocation.Bottom);
            var gui = Substitute.For<IGui>();
            gui.DocumentViews.Returns(viewList);

            object modelArgument = null;
            IMap map = Substitute.For<IMap, INotifyPropertyChanged>();
            map.Layers.Returns(new EventedList<ILayer>());
            using (ProjectItemMapView mapView = CreateMapView(map, o =>
            {
                modelArgument = o;
                return Substitute.For<ILayer>();
            }))
            using (var model = new HydroModel())
            using (var view = new HydroModelSettings())
            using (var plugin = new HydroModelGuiPlugin {Gui = gui})
            {
                viewList.Add(mapView);
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(HydroModel));

                // Call
                viewInfo.AfterCreate(view, model);

                // Assert
                Assert.That(modelArgument, Is.SameAs(model));
                map.Received(expectedRenderCalls).Render();
            }
        }

        /// <summary>
        /// This test indirectly verifies that the map is not forced to re-render after the view is created. This causes a single
        /// Render()
        /// call to the map. (probably when the view is added to the view list)
        /// The test verifies the AfterCreate call by mimicking the situation. There is no need to fully configure the plugin,
        /// because:
        /// - MapView.GetLayerForData is set. If it returns NULL --> No ProjectItemMapView belongs to the argument
        /// - The GUI and its corresponding ViewList can be substituted as desired. For the verification of the AfterCreate method
        /// in this situation, it is only verified that no NullReferenceException is generated when there's no associated
        /// ProjectItemMapView found
        /// This test does NOT verify additional properties being set on a HydroModelSettings.
        /// </summary>
        [Test]
        public void GivenPluginWithNoProjectItemMapViewForHydroModel_WhenViewInfoForHydroModelAfterCreateCalled_ThenMapViewRefreshed()
        {
            // Setup
            const int expectedRenderCalls = 1;

            var viewList = new ViewList(Substitute.For<IDockingManager>(), ViewLocation.Bottom);
            var gui = Substitute.For<IGui>();
            gui.DocumentViews.Returns(viewList);

            object modelArgument = null;
            IMap map = Substitute.For<IMap, INotifyPropertyChanged>();
            map.Layers.Returns(new EventedList<ILayer>());
            using (ProjectItemMapView mapView = CreateMapView(map, o =>
            {
                modelArgument = o;
                return null;
            }))
            using (var model = new HydroModel())
            using (var view = new HydroModelSettings())
            using (var plugin = new HydroModelGuiPlugin {Gui = gui})
            {
                viewList.Add(mapView);

                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(HydroModel));

                // Call
                viewInfo.AfterCreate(view, model);

                // Assert
                Assert.That(modelArgument, Is.SameAs(model));
                map.Received(expectedRenderCalls).Render();
            }
        }

        /// <summary>
        /// GIVEN a ProjectExplorer with an existing ContextMenu with a Tag
        /// AND a HydroModelGuiPlugin using this ProjectExplorer
        /// AND some other model not equal to the Tag
        /// WHEN GetContextMenu is called with this model
        /// THEN The the ContextMenu Tag is updated with this model
        /// </summary>
        [TestCaseSource(nameof(BeforeTagTestCaseData))]
        public void GivenAHydroModelGuiPlugin_WhenGetContextMenuIsCalledWithAModel_ThenTheContextMenuTagIsUpdated(object beforeTag, HydroModel model)
        {
            var validateItem = new ClonableToolStripMenuItem
            {
                Text = Resources.HydroModelGuiPlugin_GetContextMenu_Validate___,
                Tag = beforeTag
            };

            using (HydroModelGuiPlugin plugin = GetConfiguredPlugin(validateItem))
            {
                // Given
                plugin.GetContextMenu(null, model);

                // Then
                Assert.That(validateItem.Tag, Is.EqualTo(model));
            }
        }

        private static ProjectItemMapView CreateMapView(IMap map, Func<object, ILayer> getLayerForDataFunc)
        {
            return new ProjectItemMapView
            {
                MapView =
                {
                    GetLayerForData = getLayerForDataFunc,
                    Map = map
                }
            };
        }

        private static HydroModelGuiPlugin GetConfiguredPlugin(ToolStripItem validateItem)
        {
            var gui = MockRepository.GenerateStub<IGui>();
            var application = MockRepository.GenerateStub<IApplication>();
            var activityRunner = MockRepository.GenerateStub<IActivityRunner>();

            application.Stub(a => a.ActivityRunner)
                       .Return(activityRunner);

            gui.Application = application;

            application.Stub(a => a.GetAllModelsInProject())
                       .Return(new List<IModel>());

            var mainWindow = MockRepository.GenerateStub<IMainWindow>();
            var projectExplorer = MockRepository.GenerateStub<IProjectExplorer>();

            var subMenu = new ContextMenuStrip();
            subMenu.Items.Add(validateItem);
            var contextMenuAdapter = new MenuItemContextMenuStripAdapter(subMenu);

            gui.Stub(g => g.MainWindow)
               .Return(mainWindow);
            mainWindow.Stub(mw => mw.ProjectExplorer)
                      .Return(projectExplorer);
            projectExplorer.Stub(pe => pe.GetContextMenu(null, null))
                           .IgnoreArguments()
                           .Return(contextMenuAdapter);

            var plugin = new HydroModelGuiPlugin {Gui = gui};
            return plugin;
        }
    }
}