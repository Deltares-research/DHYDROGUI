﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WaterFlowFMMapIntegrationTest
    {
        private static string GetBendProfPath()
        {
            return TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
        }

        
        [Test]
        [Category(TestCategory.Performance)]
        public void TestRunningSmallModelWithManyTimeSteps()
        {
            var mduPath = TestHelper.GetTestFilePath(@"smallModelWithManyTimeSteps\r01.mdu");

            using (var gui = CreateGui())
            {
                gui.Run();

                gui.MainWindow.Show();

                Project project = gui.Application.ProjectService.CreateProject();
                
                var model = new WaterFlowFMModel(mduPath);

                project.RootFolder.Add(model);

                var sw = new Stopwatch();
                sw.Start();

                gui.Application.ActivityRunner.Enqueue(model);

                while (gui.Application.IsActivityRunningOrWaiting(model))
                {
                    Application.DoEvents();
                }

                sw.Stop();
                Assert.Less(sw.ElapsedMilliseconds, 30000);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCentralMapForFMModel()
        {
            var mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = CreateGui())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(model);

                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void OpenCloseCentralMapForFMModelCheckEventLeaks()
        {
            var mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = CreateGui())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(model);

                    // check subscribers
                    var eventsBefore = TestReferenceHelper.FindEventSubscriptions(model.Bathymetry, true);
                    
                    // open & close central map
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    gui.CommandHandler.RemoveAllViewsForItem(model); // close central map
                    Assert.IsNull(gui.DocumentViews.ActiveView);

                    // check event subscribers
                    var eventsAfter = TestReferenceHelper.FindEventSubscriptions(model.Bathymetry, true);

                    Assert.AreEqual(eventsBefore, eventsAfter, "#events bathymetry coverage");
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Integration)]
        public void ReloadCentralMapAfterModelWithOutputSaved()
        {
            var mduPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"harlingen\har.mdu"));
            
            using (var gui = CreateGui())
            {
                gui.Run();

                Action mainWindowShown = () =>
                {
                    var projectService = gui.Application.ProjectService;
                    Project project = projectService.CreateProject();
                    var model = new WaterFlowFMModel(mduPath);
                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof (ProjectItemMapView));
                    var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    projectService.SaveProjectAs("test.dsproj");
                    gui.CommandHandler.OpenView(model, typeof (ValidationView));
                    gui.DocumentViews.ActiveView = mapView;

                    Assert.AreEqual(mapView, gui.DocumentViews.ActiveView);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void OpenAttributeTableViewForArea2DFeatureListShouldNotThrowException()
        {
            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;
                app.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        Project project = app.ProjectService.CreateProject();
                        project.RootFolder.Add(model);

                        gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                        HydroArea area = model.Area;
                        var featuresToVerify = new List<IEnumerable<IFeature>>
                        {
                            area.Pumps,
                            area.Weirs,
                            area.Gates,
                            area.LeveeBreaches,
                            area.DryPoints,
                            area.RoofAreas,
                            area.Gullies,
                            area.ThinDams,
                            area.FixedWeirs,
                            area.LandBoundaries,
                            area.DryAreas,
                            area.ObservationPoints,
                            area.ObservationCrossSections,
                            area.Embankments,
                            area.Enclosures,
                            area.BridgePillars
                        };
                        Assert.Multiple(() =>
                        {
                            foreach (IEnumerable<IFeature> features2D in featuresToVerify)
                            {
                                void OpenViewCall() => gui.CommandHandler.OpenView(features2D);
                                string typeNameOfEnumerable = GetTypeNameOfEnumerable(features2D);
                                Assert.That(OpenViewCall, Throws.Nothing, $"Something went wrong when opening the layer of the feature list of type {typeNameOfEnumerable} in an Attribute Table view (MDE) in the FM Model Area of the Map file explorer" );
                            }
                        });
                    }
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        /// <summary>
        /// Retrieves the type name of the elements in a generic IEnumerable.
        /// </summary>
        /// <param name="enumerable">An IEnumerable of objects, typically casted from the original generic IEnumerable&lt;T&gt;.</param>
        /// <returns>The name of the element type T in the IEnumerable&lt;T&gt;.</returns>
        /// <remarks>
        /// This method uses reflection to determine the element type of the given IEnumerable, even if it is empty.
        /// It is important to note that the provided IEnumerable must be a generic type (IEnumerable&lt;T&gt;).
        /// </remarks>
        /// <example>
        /// <code>
        /// IEnumerable&lt;Pump2D&gt; emptyEnumerableOfPumps = Enumerable.Empty&lt;Pump2D&gt;();
        /// string typeName = GetTypeNameOfEnumerable(emptyEnumerableOfPumps.Cast&lt;object&gt;());
        /// Console.WriteLine(typeName); // Output: "Pump2D"
        /// </code>
        /// </example>
        private static string GetTypeNameOfEnumerable(IEnumerable<object> enumerable)
        {
            Type elementType = enumerable.GetType().GetGenericArguments()[0];
            return elementType.Name;
        }

        private static IGui CreateGui()
        {
            return new DHYDROGuiBuilder().WithFlowFM().Build();
        }
    }
}