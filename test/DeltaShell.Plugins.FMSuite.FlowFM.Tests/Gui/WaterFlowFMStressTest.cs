using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Layers;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class WaterFlowFMStressTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        [Ignore("Big model")]
        public void ImportCsmExtendedModelAndShowAllLayersAndViews()
        {
            string mduPath = TestHelper.GetTestFilePath(@"csm_extended\csma82n4.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.LoadMdu(mduPath);

            // TODO: Fix this statement
            //Assert.AreEqual(2453301*2, ((SpatialOperationSetValueConverter)model.GetDataItemByValue(model.Roughness).ValueConverter).SpatialOperationSet.Operations[0]);
            //Assert.AreEqual(159473, ((SamplesOperationInfo)model.InitialWaterLevels.Operations[0]).Points.Count);
            Assert.AreEqual(3024001, model.BoundaryConditions.Last().GetDataAtPoint(0).Arguments[0].Values.Count);

            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = app.Project;
                    project.RootFolder.Add(model);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    Assert.IsTrue(gui.DocumentViews.ActiveView is ProjectItemMapView);

                    MapView mapView = FlowFMGuiPlugin.ActiveMapView;

                    // give view time to open
                    Application.DoEvents();
                    Application.DoEvents();

                    IMap map = mapView.Map;
                    map.ZoomToExtents();
                    // enable all layers (except snap layers)
                    ((GroupLayer) map.Layers[0]).Layers
                                                .OfType<GroupLayer>()
                                                .Where(l => !l.Name.Contains("snapped"))
                                                .ForEach(l => l.Visible = true);
                    for (var i = 0; i < 5; i++)
                    {
                        map.Render();
                    }

                    Console.WriteLine("done");
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
    }
}