using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 20,
                    Y = 20
                },
                new Coordinate
                {
                    X = 70,
                    Y = 70
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            double distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 20,
                    Y = 70
                },
                new Coordinate
                {
                    X = 70,
                    Y = 20
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            double distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 70,
                    Y = 20
                },
                new Coordinate
                {
                    X = 20,
                    Y = 70
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            double distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 70,
                    Y = 70
                },
                new Coordinate
                {
                    X = 20,
                    Y = 20
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            double distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 20,
                    Y = 40
                },
                new Coordinate
                {
                    X = 80,
                    Y = 40
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 80,
                    Y = 40
                },
                new Coordinate
                {
                    X = 20,
                    Y = 40
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 50,
                    Y = 20
                },
                new Coordinate
                {
                    X = 50,
                    Y = 70
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 50,
                    Y = 70
                },
                new Coordinate
                {
                    X = 50,
                    Y = 20
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 10.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0) * 2.0;

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 20,
                    Y = 20
                },
                new Coordinate
                {
                    X = 70,
                    Y = 70
                },
                new Coordinate
                {
                    X = 120,
                    Y = 20
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            double distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(70.0 * 70.0 * 2.0) + 10;

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 50,
                    Y = 50
                },
                new Coordinate
                {
                    X = 120,
                    Y = 120
                },
                new Coordinate
                {
                    X = 130,
                    Y = 120
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            double distance = Math.Sqrt(1800.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(70.0 * 70.0 * 2.0) + 10;

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 100,
                    Y = 50
                },
                new Coordinate
                {
                    X = 30,
                    Y = 120
                },
                new Coordinate
                {
                    X = 20,
                    Y = 120
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            double distance = Math.Sqrt(1800.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);
            branlen = branlen + Math.Sqrt(70.0 * 70.0 * 2.0);
            branlen = branlen + Math.Sqrt(140.0 * 140.0 * 2.0);
            branlen = branlen + 140.0;
            branlen = branlen + Math.Sqrt(40.0 * 40.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = -20,
                    Y = 50
                },
                new Coordinate
                {
                    X = 30,
                    Y = 100
                },
                new Coordinate
                {
                    X = 100,
                    Y = 30
                },
                new Coordinate
                {
                    X = -40,
                    Y = -110
                },
                new Coordinate
                {
                    X = -40,
                    Y = 30
                },
                new Coordinate
                {
                    X = -80,
                    Y = 70
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());
            network.Branches.Add(branch1);

            double distance = Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
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
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 20,
                    Y = 20
                },
                new Coordinate
                {
                    X = 70,
                    Y = 70
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            double distance = -Math.Sqrt(200.0);
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                                                                                    embankmentDefinitions, distance, true, true);

            Assert.IsFalse(result);
        }

        [Test]
        public void EmbankmentAtTooBigDistance()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode
            {
                Name = "node1",
                Network = network
            };
            IHydroNode node2 = new HydroNode
            {
                Name = "node2",
                Network = network
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            double branlen = Math.Sqrt(50.0 * 50.0 * 2.0);

            var branch1 = new Channel("branch1", node1, node2, branlen);
            var vertices = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 20,
                    Y = 20
                },
                new Coordinate
                {
                    X = 70,
                    Y = 70
                }
            };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            const double distance = 100000.0;
            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            bool result = EmbankmentGenerator.GenerateEmbankmentsAtConstantDistance(network.Branches.Cast<Channel>().ToList(),
                                                                                    embankmentDefinitions, distance, true, true);

            Assert.IsFalse(result);
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
                Geometry = new LineString(new[]
                {
                    node1.Geometry.Coordinate,
                    node2.Geometry.Coordinate
                })
            };
            var channel2 = new Channel(node2, node3)
            {
                Name = "Channel2",
                Geometry = new LineString(new[]
                {
                    node2.Geometry.Coordinate,
                    node3.Geometry.Coordinate
                })
            };
            var channel3 = new Channel(node3, node4)
            {
                Name = "Channel3",
                Geometry = new LineString(new[]
                {
                    node3.Geometry.Coordinate,
                    node4.Geometry.Coordinate
                })
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(new[]
            {
                channel1,
                channel2,
                channel3
            });
            network.Nodes.AddRange(new[]
            {
                node1,
                node2,
                node3,
                node4
            });

            IList<Embankment> embankmentDefinitions = new List<Embankment>();

            // generate and merge left embankments
            bool result = EmbankmentGenerator.GenerateEmbankments(network.Branches.Cast<Channel>().ToList(), embankmentDefinitions, false,
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

            var node5 = new HydroNode("Node5") {Geometry = new Point(0, 0)};
            var node6 = new HydroNode("Node6") {Geometry = new Point(100, 0)};
            var node7 = new HydroNode("Node7") {Geometry = new Point(100, 100)};

            var channel4 = new Channel(node5, node6)
            {
                Name = "Channel4",
                Geometry = new LineString(new[]
                {
                    node5.Geometry.Coordinate,
                    node6.Geometry.Coordinate
                })
            };
            var channel5 = new Channel(node7, node6)
            {
                Name = "Channel5",
                Geometry = new LineString(new[]
                {
                    node7.Geometry.Coordinate,
                    node6.Geometry.Coordinate
                })
            };

            var network2 = new HydroNetwork();
            network2.Branches.AddRange(new[]
            {
                channel4,
                channel5
            });
            network2.Nodes.AddRange(new[]
            {
                node5,
                node6,
                node7
            });

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

            var node8 = new HydroNode("Node8") {Geometry = new Point(0, 0)};
            var node9 = new HydroNode("Node9") {Geometry = new Point(100, 0)};
            var node10 = new HydroNode("Node10") {Geometry = new Point(100, 100)};
            var node11 = new HydroNode("Node11") {Geometry = new Point(200, 0)};

            var channel6 = new Channel(node8, node9)
            {
                Name = "Channel6",
                Geometry = new LineString(new[]
                {
                    node8.Geometry.Coordinate,
                    node9.Geometry.Coordinate
                })
            };
            var channel7 = new Channel(node9, node10)
            {
                Name = "Channel7",
                Geometry = new LineString(new[]
                {
                    node9.Geometry.Coordinate,
                    node10.Geometry.Coordinate
                })
            };
            var channel8 = new Channel(node11, node9)
            {
                Name = "Channel8",
                Geometry = new LineString(new[]
                {
                    node11.Geometry.Coordinate,
                    node9.Geometry.Coordinate
                })
            };

            var network3 = new HydroNetwork();
            network3.Branches.AddRange(new[]
            {
                channel6,
                channel7,
                channel8
            });
            network3.Nodes.AddRange(new[]
            {
                node8,
                node9,
                node10,
                node11
            });

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