using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    internal class EmbankmentGeneratorTest
    {
        // An overwiew of the tests are shown in a document added to issue TOOLS-21993
        // It is a mixed map, but with that you can trace back the coordinates used in
        // the tests with Cross-Sections

        // Distances calculated like this:
        // var distance = Math.Sqrt(200.0);
        // are used to get round figures for resulting (and to be checked) coordinates.

        [Test]
        public void EmbankmentDownLeftUpRight()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 20},
                new Coordinate {X = 70, Y = 70},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            var distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentUpLeftDownRight()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 70},
                new Coordinate {X = 70, Y = 20},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            var distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentDownRightUpLeft()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 70, Y = 20},
                new Coordinate {X = 20, Y = 70},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            var distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentUpRightDownLeft()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 70, Y = 70},
                new Coordinate {X = 20, Y = 20},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            var distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentHorizontal2Left()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 40},
                new Coordinate {X = 80, Y = 40},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentHorizontal2Right()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 80, Y = 40},
                new Coordinate {X = 20, Y = 40},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentVerticalUp()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 50, Y = 20},
                new Coordinate {X = 50, Y = 70},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(40.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentVerticalDown()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 50, Y = 70},
                new Coordinate {X = 50, Y = 20},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentUpAndDown()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0)*2.0;

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 20},
                new Coordinate {X = 70, Y = 70},
                new Coordinate {X = 120, Y = 20},
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            var distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(3, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(3, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);

            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentUpAndShortRight()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((70.0*70.0)*2.0) + 10;

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 50, Y = 50},
                new Coordinate {X = 120, Y = 120},
                new Coordinate {X = 130, Y = 120},
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            var distance = Math.Sqrt(1800.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(3, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(102.42640687119285, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(162.42640687119285, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(162.42640687119285, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);

            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(137.57359312880715, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(77.573593128807147, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

        }

        [Test]
        public void
            EmbankmentUpAndShortLeft()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((70.0*70.0)*2.0) + 10;

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 100, Y = 50},
                new Coordinate {X = 30, Y = 120},
                new Coordinate {X = 20, Y = 120},
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            var distance = Math.Sqrt(1800.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(3, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(12.426406871192825, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(77.573593128807147, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(130.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(47.573593128807154, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(162.42640687119285, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(162.42640687119285, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentAroundOrigin()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);
            branlen = branlen + Math.Sqrt((70.0*70.0)*2.0);
            branlen = branlen + Math.Sqrt((140.0*140.0)*2.0);
            branlen = branlen + 140.0;
            branlen = branlen + Math.Sqrt((40.0*40.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = -20, Y = 50},
                new Coordinate {X = 30, Y = 100},
                new Coordinate {X = 100, Y = 30},
                new Coordinate {X = -40, Y = -110},
                new Coordinate {X = -40, Y = 30},
                new Coordinate {X = -80, Y = 70},
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            var distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(6, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(6, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(-30.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(120.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(120.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(-54.142135623730937, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(-144.14213562373095, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(-54.142135623730937, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(24.142135623730958, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(-90.0, embankmentDefinitions[0].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[5].Y, 0.00000000001);

            Assert.AreEqual(-10.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(-25.857864376269063, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(-75.85786437626905, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(-25.857864376269063, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(35.857864376269042, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(-70.0, embankmentDefinitions[1].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[5].Y, 0.00000000001);

        }

        [Test]
        public void EmbankmentAtNegativeDistance()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 20},
                new Coordinate {X = 70, Y = 70},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            var distance = -Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsFalse(result);
        }

        [Test]
        public void EmbankmentAtTooBigDistance()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", Network = network};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branlen = Math.Sqrt((50.0*50.0)*2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 20},
                new Coordinate {X = 70, Y = 70},
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 100000.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, distance, true, true);

            Assert.IsFalse(result);
        }

        [Test]
        public void EmbankmentBasedOnTwoCrossSections()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(10, 30)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(120, 140)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 30, Y = 50},
                    new Coordinate {X = 60, Y = 80},
                    new Coordinate {X = 100, Y = 120},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side2, 8),
                new Coordinate(-side2 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            var yzCoordinates2 = new List<Coordinate>
            {
                new Coordinate(-side1, 10.0),
                new Coordinate(-side1 + 5, 0.0),
                new Coordinate(side2 - 5, 0.0),
                new Coordinate(side2, 8.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, Math.Sqrt(1800.0), yzCoordinates1,
                "CrossSection1");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, Math.Sqrt(9800.0), yzCoordinates2,
                "CrossSection2");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(7, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(7, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(-10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(45.0, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(95.0, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[0].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(150.0, embankmentDefinitions[0].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(75.0, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(65.0, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(120, embankmentDefinitions[1].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[1].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(140.0, embankmentDefinitions[1].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(120.0, embankmentDefinitions[1].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[6].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[6].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentSimpleBranchTwoCrossSectionsAtEnds()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(40, 60)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(80, 100)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side2, 8),
                new Coordinate(-side2 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            var yzCoordinates2 = new List<Coordinate>
            {
                new Coordinate(-side1, 10.0),
                new Coordinate(-side1 + 5, 0.0),
                new Coordinate(side2 - 5, 0.0),
                new Coordinate(side2, 8.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 0.0, yzCoordinates1, "CrossSection1");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, channel1.Length, yzCoordinates2,
                "CrossSection2");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(2, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(2, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentSimpleBranchOneCrossSectionInMiddle()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(40, 60)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(80, 100)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side2, 8),
                new Coordinate(-side2 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, channel1.Length/2.0, yzCoordinates1,
                "CrossSection1");


            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(3, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(3, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(120.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);

            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentSimpleBranchOneCrossSectionAndPointInMiddle()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(40, 60)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(80, 100)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 60, Y = 80},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side2, 8),
                new Coordinate(-side2 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, channel1.Length/2.0, yzCoordinates1,
                "CrossSection1");


            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(3, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(3, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(120.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);

            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentPointsAtTwoCrossSections()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(10, 30)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(120, 140)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 30, Y = 50},
                    new Coordinate {X = 40, Y = 60},
                    new Coordinate {X = 60, Y = 80},
                    new Coordinate {X = 80, Y = 100},
                    new Coordinate {X = 100, Y = 120},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side2, 8),
                new Coordinate(-side2 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            var yzCoordinates2 = new List<Coordinate>
            {
                new Coordinate(-side1, 10.0),
                new Coordinate(-side1 + 5, 0.0),
                new Coordinate(side2 - 5, 0.0),
                new Coordinate(side2, 8.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, Math.Sqrt(1800.0), yzCoordinates1,
                "CrossSection1");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, Math.Sqrt(9800.0), yzCoordinates2,
                "CrossSection2");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(7, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(7, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(-10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(45.0, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(95.0, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[0].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(150.0, embankmentDefinitions[0].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(75.0, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(65.0, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(120, embankmentDefinitions[1].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(100.0, embankmentDefinitions[1].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(140.0, embankmentDefinitions[1].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(120.0, embankmentDefinitions[1].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[6].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[6].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentPointsAtTwoCrossSectionsLeftOnly()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(10, 30)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(120, 140)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 30, Y = 50},
                    new Coordinate {X = 40, Y = 60},
                    new Coordinate {X = 60, Y = 80},
                    new Coordinate {X = 80, Y = 100},
                    new Coordinate {X = 100, Y = 120},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side2, 8),
                new Coordinate(-side2 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            var yzCoordinates2 = new List<Coordinate>
            {
                new Coordinate(-side1, 10.0),
                new Coordinate(-side1 + 5, 0.0),
                new Coordinate(side2 - 5, 0.0),
                new Coordinate(side2, 8.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, Math.Sqrt(1800.0), yzCoordinates1,
                "CrossSection1");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, Math.Sqrt(9800.0), yzCoordinates2,
                "CrossSection2");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, false);

            Assert.IsTrue(result);

            Assert.AreEqual(1, embankmentDefinitions.Count);
            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);

            Assert.AreEqual(7, embankmentDefinitions[0].Geometry.Coordinates.Count());

            Assert.AreEqual(-10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(45.0, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(95.0, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[0].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(150.0, embankmentDefinitions[0].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[6].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentPointsAtTwoZWCrossSections()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(10, 30)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(120, 140)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 30, Y = 50},
                    new Coordinate {X = 40, Y = 60},
                    new Coordinate {X = 60, Y = 80},
                    new Coordinate {X = 80, Y = 100},
                    new Coordinate {X = 100, Y = 120},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var width1 = Math.Sqrt(800.0)*2.0;
            var width2 = Math.Sqrt(200.0)*2.0;

            var heightWidthFlowData1 = new List<HeightFlowStorageWidth>
            {
                new HeightFlowStorageWidth(0.0, width1 - 5.0, width1 - 5.0),
                new HeightFlowStorageWidth(10.0, width1, width1)
            };

            var heightWidthFlowData2 = new List<HeightFlowStorageWidth>
            {
                new HeightFlowStorageWidth(0.0, width2 - 5.0, width2 - 5.0),
                new HeightFlowStorageWidth(8.0, width2, width2)
            };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(channel1, Math.Sqrt(1800.0), heightWidthFlowData1,
                "CrossSection1");

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(channel1, Math.Sqrt(9800.0), heightWidthFlowData2,
                "CrossSection2");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(7, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(7, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(-10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(45.0, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(95.0, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[0].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(150.0, embankmentDefinitions[0].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(50.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(40.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(75.0, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(65.0, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[1].Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[1].Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[1].Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[1].Geometry.Coordinates[6].Y, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[6].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(9.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[5].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[1].Geometry.Coordinates[6].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentHorizontalThreeCrossSections()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(10, 50)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(110, 50)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 35, Y = 50},
                    new Coordinate {X = 85, Y = 50},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-30, 10),
                new Coordinate(-25, 0.0),
                new Coordinate(25 - 5, 0.0),
                new Coordinate(30, 9.0),
            };

            var yzCoordinates2 = new List<Coordinate>
            {
                new Coordinate(-40, 12.0),
                new Coordinate(-35, 0.0),
                new Coordinate(15, 0.0),
                new Coordinate(20, 13.0),
            };

            var yzCoordinates3 = new List<Coordinate>
            {
                new Coordinate(-20, 8.0),
                new Coordinate(-15, 0.0),
                new Coordinate(25, 0.0),
                new Coordinate(30, 11.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 0.0, yzCoordinates1, "CrossSection1");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 50.0, yzCoordinates2, "CrossSection2");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 100.0, yzCoordinates3, "CrossSection3");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(5, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(5, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(35.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(85.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(90.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(85.0, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(35.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(25.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(85.0, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(25.0, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(11.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(12.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);

            Assert.AreEqual(9.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(11.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(13.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(12.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(11.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentVerticalThreeCrossSections()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(50, 10)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(50, 110)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 50, Y = 35},
                    new Coordinate {X = 50, Y = 85},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);
            var side2 = Math.Sqrt(800.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-30, 10),
                new Coordinate(-25, 0.0),
                new Coordinate(25 - 5, 0.0),
                new Coordinate(30, 9.0),
            };

            var yzCoordinates2 = new List<Coordinate>
            {
                new Coordinate(-40, 12.0),
                new Coordinate(-35, 0.0),
                new Coordinate(15, 0.0),
                new Coordinate(20, 13.0),
            };

            var yzCoordinates3 = new List<Coordinate>
            {
                new Coordinate(-20, 8.0),
                new Coordinate(-15, 0.0),
                new Coordinate(25, 0.0),
                new Coordinate(30, 11.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 0.0, yzCoordinates1, "CrossSection1");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 50.0, yzCoordinates2, "CrossSection2");

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, 100.0, yzCoordinates3, "CrossSection3");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(5, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(5, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(15.0, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(35.0, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(20.0, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(85.0, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(75.0, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(35.0, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(70.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(60.0, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(75.0, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(85.0, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(80.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(11.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(12.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);

            Assert.AreEqual(9.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(11.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(13.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(12.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(11.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentUpHorUpCrossSectionInMiddle()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(20, 20)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(190, 120)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 70, Y = 70},
                    new Coordinate {X = 140, Y = 70},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side1, 8),
                new Coordinate(-side1 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, channel1.Length/2.0, yzCoordinates1,
                "CrossSection1");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual(2, embankmentDefinitions.Count);
            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(5, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(5, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(64.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(84.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(105.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(84.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(134.14213562373095, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(84.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(180.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(130.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(75.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(55.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(105.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(55.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(145.85786437626905, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(55.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(200.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(110.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
        }

        [Test]
        public void EmbankmentUpHorDownCrossSectionInMiddle()
        {
            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(20, 20)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(190, 20)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    new Coordinate {X = 70, Y = 70},
                    new Coordinate {X = 140, Y = 70},
                    node2.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1});
            network.Nodes.AddRange(new[] {node1, node2});

            // add cross-sections
            var side1 = Math.Sqrt(200.0);

            var yzCoordinates1 = new List<Coordinate>
            {
                new Coordinate(-side1, 8),
                new Coordinate(-side1 + 5, 0.0),
                new Coordinate(side1 - 5, 0.0),
                new Coordinate(side1, 10.0),
            };

            CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(channel1, channel1.Length/2.0, yzCoordinates1,
                "CrossSection1");

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            var result = EmbankmentGenerator.GenerateEmbankmentsBasedOnCrossSection(network.Branches.Cast<Channel>().ToList(),
                embankmentDefinitions, true, true);

            Assert.IsTrue(result);

            Assert.AreEqual(2, embankmentDefinitions.Count);
            Assert.AreEqual("Embankment01", embankmentDefinitions[0].Name);
            Assert.AreEqual("Embankment02", embankmentDefinitions[1].Name);

            Assert.AreEqual(5, embankmentDefinitions[0].Geometry.Coordinates.Count());
            Assert.AreEqual(5, embankmentDefinitions[1].Geometry.Coordinates.Count());

            Assert.AreEqual(10.0, embankmentDefinitions[0].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(64.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(84.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(105.0, embankmentDefinitions[0].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(84.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(145.85786437626905, embankmentDefinitions[0].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(84.142135623730951, embankmentDefinitions[0].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(200.0, embankmentDefinitions[0].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(30.0, embankmentDefinitions[0].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(30.0, embankmentDefinitions[1].Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(75.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(55.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(105.0, embankmentDefinitions[1].Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(55.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(134.14213562373095, embankmentDefinitions[1].Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(55.857864376269049, embankmentDefinitions[1].Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(180.0, embankmentDefinitions[1].Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[4].Y, 0.00000000001);

            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(8.0, embankmentDefinitions[0].Geometry.Coordinates[4].Z, 0.00000000001);

            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[0].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[1].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[2].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[3].Z, 0.00000000001);
            Assert.AreEqual(10.0, embankmentDefinitions[1].Geometry.Coordinates[4].Z, 0.00000000001);
        }

        [Test]
        public void AutomaticMerge()
        {
            // test automatic merge for three types of networks

            // network 1: 3 branches with similar direction for each branch
            #region network 1

            // Create Waterflow Network
            var node1 = new HydroNode("Node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("Node2") {Geometry = new Point(100, 0)};
            var node3 = new HydroNode("Node3") {Geometry = new Point(100, 100)};
            var node4 = new HydroNode("Node4") {Geometry = new Point(200, 100)};

            var channel1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry = new LineString(new[] {node1.Geometry.Coordinate, node2.Geometry.Coordinate})
            };
            var channel2 = new Channel(node2, node3)
            {
                Name = "Channel2",
                Geometry = new LineString(new[] {node2.Geometry.Coordinate, node3.Geometry.Coordinate})
            };
            var channel3 = new Channel(node3, node4)
            {
                Name = "Channel3",
                Geometry = new LineString(new[] {node3.Geometry.Coordinate, node4.Geometry.Coordinate})
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[] {channel1, channel2, channel3});
            network.Nodes.AddRange(new[] {node1, node2, node3, node4});

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            // generate and merge left embankments
            var result = EmbankmentGenerator.GenerateEmbankments(network.Branches.Cast<Channel>().ToList(), embankmentDefinitions, false,
                10.0d, true, false, true);
            Assert.IsTrue(result);
            Assert.AreEqual(1, embankmentDefinitions.Count); // automatic merging should result in 1 embankment

            // generate and merge right embankments
            embankmentDefinitions.Clear();
            result = EmbankmentGenerator.GenerateEmbankments(network.Branches.Cast<Channel>().ToList(), embankmentDefinitions, false,
                10.0d, false, true, true);
            Assert.IsTrue(result);
            Assert.AreEqual(1, embankmentDefinitions.Count); // automatic merging should result in 1 embankment

            // generate and merge left and right embankments
            embankmentDefinitions.Clear();
            result = EmbankmentGenerator.GenerateEmbankments(network.Branches.Cast<Channel>().ToList(), embankmentDefinitions, false,
                10.0d, true, true, true);
            Assert.IsTrue(result);
            Assert.AreEqual(2, embankmentDefinitions.Count); // automatic merging should result in 2 embankments

            #endregion

            // network 2: 2 branches with opposite direction for each branch
            #region network 2

            var node5 = new HydroNode("Node5") { Geometry = new Point(0, 0) };
            var node6 = new HydroNode("Node6") { Geometry = new Point(100, 0) };
            var node7 = new HydroNode("Node7") { Geometry = new Point(100, 100) };

            var channel4 = new Channel(node5, node6)
            {
                Name = "Channel4",
                Geometry = new LineString(new[] { node5.Geometry.Coordinate, node6.Geometry.Coordinate })
            };
            var channel5 = new Channel(node7, node6)
            {
                Name = "Channel5",
                Geometry = new LineString(new[] { node7.Geometry.Coordinate, node6.Geometry.Coordinate })
            };

            var network2 = new HydroNetwork();
            network2.Branches.AddRange(new[] {channel4, channel5});
            network2.Nodes.AddRange(new[] { node5, node6, node7 });

            // generate and merge left and right embankments
            embankmentDefinitions.Clear();
            result = EmbankmentGenerator.GenerateEmbankments(network2.Branches.Cast<Channel>().ToList(), embankmentDefinitions, false,
                10.0d, true, true, true);
            Assert.IsTrue(result);
            Assert.AreEqual(2, embankmentDefinitions.Count); // automatic merging should result in 2 embankments

            #endregion

            // network 3: 3 branches that all share a node, non-uniform directions
            // exception case that can not (yet) be dealt with at this moment
            #region network 3

            var node8 = new HydroNode("Node8") { Geometry = new Point(0, 0) };
            var node9 = new HydroNode("Node9") { Geometry = new Point(100, 0) };
            var node10 = new HydroNode("Node10") { Geometry = new Point(100, 100) };
            var node11 = new HydroNode("Node11") { Geometry = new Point(200, 0) };

            var channel6 = new Channel(node8, node9)
            {
                Name = "Channel6",
                Geometry = new LineString(new[] { node8.Geometry.Coordinate, node9.Geometry.Coordinate })
            };
            var channel7 = new Channel(node9, node10)
            {
                Name = "Channel7",
                Geometry = new LineString(new[] { node9.Geometry.Coordinate, node10.Geometry.Coordinate })
            };
            var channel8 = new Channel(node11, node9)
            {
                Name = "Channel8",
                Geometry = new LineString(new[] { node11.Geometry.Coordinate, node9.Geometry.Coordinate })
            };

            var network3 = new HydroNetwork();
            network3.Branches.AddRange(new[] { channel6, channel7, channel8 });
            network3.Nodes.AddRange(new[] { node8, node9, node10, node11 });

            // generate and merge left and right embankments
            embankmentDefinitions.Clear();
            result = EmbankmentGenerator.GenerateEmbankments(network3.Branches.Cast<Channel>().ToList(), embankmentDefinitions, false,
                10.0d, true, true, true);
            Assert.IsTrue(result);
            Assert.AreEqual(6, embankmentDefinitions.Count); // no merge attempted

            #endregion
        }
    }
}

