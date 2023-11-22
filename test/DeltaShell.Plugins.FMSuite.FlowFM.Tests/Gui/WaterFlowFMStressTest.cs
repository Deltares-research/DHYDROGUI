using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Layers;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class WaterFlowFMStressTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        [Ignore("Big model")]
        public void ImportCsmExtendedModelAndShowAllLayersAndViews()
        {
            var mduPath = TestHelper.GetTestFilePath(@"csm_extended\csma82n4.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            
            // TODO: Fix this statement
            //Assert.AreEqual(2453301*2, ((SpatialOperationSetValueConverter)model.GetDataItemByValue(model.Roughness).ValueConverter).SpatialOperationSet.Operations[0]);
            //Assert.AreEqual(159473, ((SamplesOperationInfo)model.InitialWaterLevels.Operations[0]).Points.Count);
            Assert.AreEqual(3024001, model.BoundaryConditions.Last().GetDataAtPoint(0).Arguments[0].Values.Count);

            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    Assert.IsTrue(gui.DocumentViews.ActiveView is ProjectItemMapView);
                    
                    var mapView = FlowFMGuiPlugin.ActiveMapView;

                    // give view time to open
                    Application.DoEvents();
                    Application.DoEvents();

                    var map = mapView.Map;
                    map.ZoomToExtents();
                    // enable all layers (except snap layers)
                    ((GroupLayer) map.Layers[0]).Layers
                                                        .OfType<GroupLayer>()
                                                        .Where(l => !l.Name.Contains("snapped"))
                                                        .ForEach(l => l.Visible = true);
                    for (var i = 0; i < 5; i++)
                        map.Render();

                    Console.WriteLine("done");
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
    }
}