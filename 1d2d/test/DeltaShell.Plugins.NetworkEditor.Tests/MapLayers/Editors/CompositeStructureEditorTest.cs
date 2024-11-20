using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Editors;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Forms;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    [TestFixture]
    public class CompositeStructureEditorTest
    {
        private static  HydroNetwork HydroNetwork;
        private static IChannel branch1;
        private static ICompositeBranchStructure CompositeBranchStructure;
        private static IPump pump1;
        private static IPump pump2;
        private static MapControl mapControl;

        [SetUp]
        public void NetworkSetup()
        {
            mapControl = new MapControl { Map = { Size = new Size(1000, 1000) } }; // enable coordinate conversions, default size is 100x100
            HydroNetwork = new HydroNetwork();

            branch1 = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(20, 0) }) };
            var node1 = new HydroNode { Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Geometry = new Point(20, 0) };
            HydroNetwork.Nodes.Add(node1);
            HydroNetwork.Nodes.Add(node2);

            pump1 = new Pump { Network = HydroNetwork, Geometry = new Point(5, 0) };
            pump2 = new Pump { Network = HydroNetwork, Geometry = new Point(5, 0) };
            
            CompositeBranchStructure = new CompositeBranchStructure { Network = HydroNetwork, Geometry = new Point(5, 0) };
            NetworkHelper.AddBranchFeatureToBranch(CompositeBranchStructure, branch1, CompositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, pump1);
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, pump2);

            branch1.Source = node1;
            branch1.Target = node2;

            HydroNetwork.Branches.Add(branch1);
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
        public void PropertyTest()
        {
            var compositeStructureEditor = new CompositeStructureInteractor(new VectorLayer { Map = mapControl.Map }, CompositeBranchStructure,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, null);
            Assert.AreEqual(true, compositeStructureEditor.AllowDeletion());
            Assert.AreEqual(true, compositeStructureEditor.AllowMove());
            Assert.AreEqual(true, compositeStructureEditor.AllowSingleClickAndMove());
        }

        [Test]
        public void SelectionTest()
        {
            var compositeStructureEditor = new CompositeStructureInteractor(new VectorLayer { Map = mapControl.Map }, CompositeBranchStructure,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, null);
            TrackerFeature trackerFeature = compositeStructureEditor.Trackers[0];
            Assert.AreEqual(true, trackerFeature.Selected);
            Bitmap selectedbitmap = trackerFeature.Bitmap;
            compositeStructureEditor.SetTrackerSelection(trackerFeature, false);
            Assert.AreEqual(false, trackerFeature.Selected);
            Assert.AreNotEqual(selectedbitmap, trackerFeature.Bitmap);
        }

        [Test]
        public void MoveCompositeStructure()
        {
            var compositeStructureEditor = new CompositeStructureInteractor(new VectorLayer { Map = mapControl.Map }, CompositeBranchStructure,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, null);
            const double deltaX = 5;
            const double deltaY = 0;

            compositeStructureEditor.Network = HydroNetwork;
            compositeStructureEditor.Start();
            compositeStructureEditor.MoveTracker(compositeStructureEditor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(5, pump1.Geometry.Coordinates[0].X);
            Assert.AreEqual(5, pump2.Geometry.Coordinates[0].X);
            Assert.AreEqual(5, CompositeBranchStructure.Geometry.Coordinates[0].X);

            compositeStructureEditor.Stop(new SnapResult(compositeStructureEditor.TargetFeature.Geometry.Coordinate, CompositeBranchStructure.Branch, compositeStructureEditor.Layer, CompositeBranchStructure.Branch.Geometry, 0, 0));

            Assert.AreEqual(10, pump1.Geometry.Coordinates[0].X);
            Assert.AreEqual(10, pump2.Geometry.Coordinates[0].X);
            Assert.AreEqual(10, CompositeBranchStructure.Geometry.Coordinates[0].X);
        }
    }
}
