using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Editors;
using SharpMap.Data.Providers;
using SharpMap.Editors.FallOff;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Forms;
using Control = System.Windows.Controls.Control;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    [TestFixture]
    public class CrossSectionEditorTest
    {
        private INetwork network;
        private IChannel branch1;
        private IHydroNode node1;
        private IHydroNode node2;
        private ICrossSection crossSection;
        private MapControl mapControl;

        public void NetworkWithYZCrossSectionSetup()
        {
            mapControl = new MapControl { Map = { Size = new Size(1000, 1000) } }; // enable coordinate conversions, default size is 100x100

            network = new HydroNetwork();

            branch1 = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(20, 0) }) };
            node1 = new HydroNode { Geometry = new Point(0, 0) };
            node2 = new HydroNode { Geometry = new Point(20, 0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;

            network.Branches.Add(branch1);

            crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, CrossSectionDefinitionYZ.CreateDefault(), 10);
            crossSection.Definition.Thalweg = 0;
        }

        /// <summary>
        /// Create a network with a geomatry based cross section. Hit testing depends on the size of the bitmap tracker.
        /// Thus use a realistic size for cross section and map.
        /// </summary>
        public void NetworkWithGeometryBasedCrossSectionSetup()
        {
            mapControl = new MapControl { Map = { Size = new Size(1000, 1000) } }; // enable coordinate conversions, default size is 100x100
            network = new HydroNetwork();

            branch1 = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(200, 0) }) };
            node1 = new HydroNode { Geometry = new Point(0, 0) };
            node2 = new HydroNode { Geometry = new Point(200, 0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;
            var crossSectionDef = new CrossSectionDefinitionXYZ
                               {
                                   Geometry = new LineString(new[]
                                                          {
                                                              new Coordinate(100, -40, 0.0), new Coordinate(100, -20, 0.0),
                                                              new Coordinate(100, 0, 0.0), new Coordinate(100, 20, 0.0),
                                                              new Coordinate(100, 40, 0.0)
                                                          })
                               };

            network.Branches.Add(branch1);
            crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSectionDef, 10.0);
        }

        [TearDown]
        public void TearDown()
        {
            if(mapControl != null && !mapControl.IsDisposed)
            {
                mapControl.Dispose();
            }
        }

        [Test]
        public void PropertyTestNonGeometryBased()
        {
            NetworkWithYZCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network);
            Assert.AreEqual(true, crossSectionEditor.AllowDeletion());
            Assert.AreEqual(true, crossSectionEditor.AllowMove());
            Assert.AreEqual(true, crossSectionEditor.AllowSingleClickAndMove());
        }

        [Test]
        public void GetTrackersYZ()
        {
            NetworkWithYZCrossSectionSetup();   
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network);
            Assert.AreEqual(2, crossSectionEditor.Trackers.Count());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveCrossSectionNonGeometryBased()
        {
            NetworkWithYZCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network)
                                         {Network = network};
            const double deltaX = 5;
            const double deltaY = 0;

            crossSectionEditor.Start();
            var tracker = crossSectionEditor.Trackers[0];
            tracker.Selected = true;
            crossSectionEditor.MoveTracker(tracker, deltaX, deltaY);
            ((IBranchFeature) crossSectionEditor.TargetFeature).Chainage = 15.0;

            // result are not yet stored
            Assert.AreEqual(10, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y);

            // todo use MockRepository; problem with VectorLayer
            VectorLayer branchLayer = new VectorLayer("");
            FeatureCollection featureCollection = new FeatureCollection {Features = ((IList) network.Branches)};
            branchLayer.DataSource = featureCollection;

            crossSectionEditor.Stop();

            Assert.AreEqual(15, crossSection.Geometry.Coordinates[0].X, 0.000001);
            Assert.AreEqual(15, crossSection.Geometry.Coordinates[1].X, 0.000001);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y, 0.000001);
            Assert.AreEqual(-100, crossSection.Geometry.Coordinates[1].Y, 0.000001);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveCrossSectionNonGeometryBasedOnCustomLengthBranch()
        {
            NetworkWithYZCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network) { Network = network };
            Assert.AreEqual(10.0, crossSection.Chainage);
            branch1.IsLengthCustom = true;
            branch1.Length = 400;
            Assert.AreEqual(200.0, crossSection.Chainage);
            
            const double deltaX = 5;
            const double deltaY = 0;

            crossSectionEditor.Start();
            var tracker = crossSectionEditor.Trackers[0];
            tracker.Selected = true;
            crossSectionEditor.MoveTracker(tracker, deltaX, deltaY);
            ((IBranchFeature)crossSectionEditor.TargetFeature).Chainage = 205.0;

            // result are not yet stored
            Assert.AreEqual(200.0, crossSection.Chainage);
            Assert.AreEqual(10, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y);

            // todo use MockRepository; problem with VectorLayer
            VectorLayer branchLayer = new VectorLayer("");
            FeatureCollection featureCollection = new FeatureCollection { Features = ((IList)network.Branches) };
            branchLayer.DataSource = featureCollection;

            crossSectionEditor.Stop();

            Assert.AreEqual(205.0, crossSection.Chainage, 0.000001);
            Assert.AreEqual(10.25, crossSection.Geometry.Coordinates[0].X, 0.000001);
            Assert.AreEqual(10.25, crossSection.Geometry.Coordinates[1].X, 0.000001);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y, 0.000001);
            Assert.AreEqual(-100, crossSection.Geometry.Coordinates[1].Y, 0.000001);
        }

        [Test]
        public void PropertyTestGeometryBased()
        {
            NetworkWithGeometryBasedCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor(
                                            new VectorLayer { Map = mapControl.Map },
                                            crossSection,
                                            new VectorStyle { Symbol = new Bitmap(16, 16) }, 
                                            network
                                            );
            Assert.AreEqual(true, crossSectionEditor.AllowDeletion());
            Assert.AreEqual(true, crossSectionEditor.AllowMove());
            Assert.AreEqual(false, crossSectionEditor.AllowSingleClickAndMove());
        }

        [Test]
        public void GetTrackerTestGeometryBased()
        {
            NetworkWithGeometryBasedCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                 new VectorStyle { Symbol = new Bitmap(16, 16) }, network);
            // Expect 3 Trackers for the coordinates
            Assert.AreEqual(5, crossSectionEditor.Trackers.Count());

            Assert.AreEqual(crossSectionEditor.Trackers[0],
                            crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, -40)));
            Assert.AreEqual(crossSectionEditor.Trackers[1],
                            crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, -20)));
            Assert.AreEqual(crossSectionEditor.Trackers[2],
                            crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, 0)));
            Assert.AreEqual(crossSectionEditor.Trackers[3],
                            crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, 20)));
            Assert.AreEqual(crossSectionEditor.Trackers[4],
                            crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, 40)));

            TrackerFeature trackerFeature = crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, 10));
            Assert.IsNotNull(trackerFeature);
            Assert.AreEqual(false, crossSectionEditor.Trackers.Contains(trackerFeature));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveSingleCoordinateCrossSectionGeometryBased()
        {
            NetworkWithGeometryBasedCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network)
                                         {Network = network};
            const double deltaX = 5;
            const double deltaY = 5;

            crossSectionEditor.Start();
            crossSectionEditor.Trackers[0].Selected = true;
            crossSectionEditor.MoveTracker(crossSectionEditor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-40, crossSection.Geometry.Coordinates[0].Y);

            crossSectionEditor.Stop();

            // only the first coordinate is moved
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-35, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[1].X);
            Assert.AreEqual(-20, crossSection.Geometry.Coordinates[1].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[2].X);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[2].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[3].X);
            Assert.AreEqual(20, crossSection.Geometry.Coordinates[3].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[4].X);
            Assert.AreEqual(40, crossSection.Geometry.Coordinates[4].Y);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveAllCoordinatesCrossSectionGeometryBased()
        {
            NetworkWithGeometryBasedCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer {Map = mapControl.Map}, crossSection,
                 new VectorStyle {Symbol = new Bitmap(16, 16)}, network)
                {
                    Network = network
                };
            const double deltaX = 5;
            const double deltaY = 5;

            crossSectionEditor.Start();
            TrackerFeature trackerFeature = crossSectionEditor.GetTrackerAtCoordinate(new Coordinate(100, 10));
            crossSectionEditor.MoveTracker(trackerFeature, deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-40, crossSection.Geometry.Coordinates[0].Y);

            crossSectionEditor.Stop();

            // all coordinates are moved
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-35, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[1].X);
            Assert.AreEqual(-15, crossSection.Geometry.Coordinates[1].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[2].X);
            Assert.AreEqual(5, crossSection.Geometry.Coordinates[2].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[3].X);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[3].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[4].X);
            Assert.AreEqual(45, crossSection.Geometry.Coordinates[4].Y);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveMultiCoordinatesNoFallOffPolicyCrossSectionGeometryBased()
        {
            NetworkWithGeometryBasedCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network)
                                         {Network = network};
            const double deltaX = 5;
            const double deltaY = 5;

            crossSectionEditor.Start();
            crossSectionEditor.Trackers[1].Selected = true;
            crossSectionEditor.Trackers[3].Selected = true;
            Assert.AreEqual(2, crossSectionEditor.Trackers.Count(t => t.Selected));

            crossSectionEditor.FallOffPolicy = new NoFallOffPolicy();
            crossSectionEditor.MoveTracker(crossSectionEditor.Trackers[1], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-40, crossSection.Geometry.Coordinates[0].Y);

            crossSectionEditor.Stop();

            // only the first coordinate is moved
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-40, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[1].X);
            Assert.AreEqual(-15, crossSection.Geometry.Coordinates[1].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[2].X);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[2].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[3].X);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[3].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[4].X);
            Assert.AreEqual(40, crossSection.Geometry.Coordinates[4].Y);
        }
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveMultiCoordinatesLinearFallOffPolicyCrossSectionGeometryBased()
        {
            NetworkWithGeometryBasedCrossSectionSetup();
            var crossSectionEditor = new CrossSectionInteractor
                (new VectorLayer { Map = mapControl.Map }, crossSection,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network)
                                         {Network = network};
            const double deltaX = 5;
            const double deltaY = 5;

            crossSectionEditor.Start();
            crossSectionEditor.Trackers[1].Selected = true;
            crossSectionEditor.Trackers[3].Selected = true;
            Assert.AreEqual(2, crossSectionEditor.Trackers.Count(t => t.Selected));

            crossSectionEditor.FallOffPolicy = new LinearFallOffPolicy();
            crossSectionEditor.MoveTracker(crossSectionEditor.Trackers[1], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(-40, crossSection.Geometry.Coordinates[0].Y);

            crossSectionEditor.Stop();

            Assert.AreEqual(100, crossSection.Geometry.Coordinates[0].X); // not modified by LinearFallOffPolicy
            Assert.AreEqual(-40, crossSection.Geometry.Coordinates[0].Y); 
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[1].X);
            Assert.AreEqual(-15, crossSection.Geometry.Coordinates[1].Y);
            Assert.AreNotEqual(100, crossSection.Geometry.Coordinates[2].X);  // modified by LinearFallOffPolicy
            Assert.AreNotEqual(0, crossSection.Geometry.Coordinates[2].Y);
            Assert.AreEqual(105, crossSection.Geometry.Coordinates[3].X);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[3].Y);
            Assert.AreEqual(100, crossSection.Geometry.Coordinates[4].X);  // not modified by LinearFallOffPolicy
            Assert.AreEqual(40, crossSection.Geometry.Coordinates[4].Y);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)] //TOOLS-7472
        public void CrossSectionYZWithSinglePointGeometryShouldNotCrash()
        {
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Run();

                Action onMainWindowShown =
                    () =>
                        {
                            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
                            app.Project.RootFolder.Add(network);

                            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(
                                        network.Branches[0], CrossSectionDefinitionYZ.CreateDefault("csdef"), 15);

                            gui.CommandHandler.OpenView(network);
                            var networkEditor = gui.DocumentViews.ActiveView;

                            gui.Selection = cs;

                            while(cs.Definition.RawData.Rows.Count > 0)
                            {
                                cs.Definition.RawData.Rows.RemoveAt(0);
                            }

                            Application.DoEvents();
                        };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, onMainWindowShown);
            }
        }
    }
}
