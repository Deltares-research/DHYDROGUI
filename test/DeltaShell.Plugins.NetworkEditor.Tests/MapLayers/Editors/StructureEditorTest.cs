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
    public class StructureEditorTest
    {
        private static HydroNetwork HydroNetwork;
        private static IChannel branch1;
        private static IPump pump;
        private static MapControl mapControl;

        [SetUp]
        public void NetworkSetup()
        {
            mapControl = new MapControl {Map = {Size = new Size(1000, 1000)}}; // enable coordinate conversions, default size is 100x100
            HydroNetwork = new HydroNetwork();

            branch1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(20, 0)
                })
            };
            HydroNetwork.Branches.Add(branch1);
            var node1 = new HydroNode {Geometry = new Point(0, 0)};
            var node2 = new HydroNode {Geometry = new Point(20, 0)};
            HydroNetwork.Nodes.Add(node1);
            HydroNetwork.Nodes.Add(node2);

            pump = new Pump
            {
                Network = HydroNetwork,
                Geometry = new Point(5, 0)
            };

            var compositeBranchStructure = new CompositeBranchStructure
            {
                Network = HydroNetwork,
                Geometry = new Point(5, 0)
            };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, branch1, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            branch1.Source = node1;
            branch1.Target = node2;

            HydroNetwork.Branches.Add(branch1);
        }

        [TearDown]
        public void TearDown()
        {
            if (mapControl != null && !mapControl.IsDisposed)
            {
                mapControl.Dispose();
            }
        }

        [Test]
        public void PropertyTest()
        {
            var pumpEditor = new StructureInteractor<Pump>(new VectorLayer {Map = mapControl.Map}, pump,
                                                           new VectorStyle {Symbol = new Bitmap(16, 16)}, null);
            Assert.AreEqual(true, pumpEditor.AllowDeletion());
            Assert.AreEqual(true, pumpEditor.AllowMove());
            Assert.AreEqual(true, pumpEditor.AllowSingleClickAndMove());
        }

        [Test]
        public void MovePump()
        {
            var pumpEditor = new StructureInteractor<Pump>(new VectorLayer {Map = mapControl.Map}, pump,
                                                           new VectorStyle {Symbol = new Bitmap(16, 16)}, null);
            const double deltaX = 5;
            const double deltaY = 0;

            pumpEditor.Start();
            pumpEditor.MoveTracker(pumpEditor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(5, pump.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y);

            pumpEditor.Stop(new SnapResult(pumpEditor.TargetFeature.Geometry.Coordinate, pump.Channel, pumpEditor.Layer, pump.Channel.Geometry, 0, 0));

            Assert.AreEqual(10, pump.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y);
        }
    }
}