using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class RetentionFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFile_WithRetentionWithOneLevel_AddCorrectRetentionToNetwork()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string content =
                    "[General]                                 " + Environment.NewLine +
                    "    fileVersion           = 2.00          " + Environment.NewLine +
                    "    fileType              = iniField      " + Environment.NewLine +
                    "    useStreetStorage      = 1             " + Environment.NewLine +
                    "                                          " + Environment.NewLine +
                    "[StorageNode]                             " + Environment.NewLine +
                    "    id                    = some_id       " + Environment.NewLine +
                    "    name                  = some_name     " + Environment.NewLine +
                    "    branchId              = some_branch   " + Environment.NewLine +
                    "    chainage              = 50.000000     " + Environment.NewLine +
                    "    useTable              = True          " + Environment.NewLine +
                    "    numLevels             = 1             " + Environment.NewLine +
                    "    levels                = 1.23          " + Environment.NewLine +
                    "    storageArea           = 4.56          " + Environment.NewLine +
                    "    interpolate           = block         ";

                string filePath = temp.CreateFile("nodeFile.ini", content);

                IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

                // Call
                RetentionFileReader.ReadFile(filePath, network);

                // Assert
                IRetention retention = network.Retentions.Single();
                Assert.That(retention.Name, Is.EqualTo("some_id"));
                Assert.That(retention.LongName, Is.EqualTo("some_name"));
                Assert.That(retention.Branch, Is.SameAs(network.Branches[0]));
                Assert.That(retention.Chainage, Is.EqualTo(50));
                Assert.That(retention.Geometry, Is.EqualTo(new Point(0, 50)));
                Assert.That(retention.UseTable, Is.False);
                Assert.That(retention.BedLevel, Is.EqualTo(1.23));
                Assert.That(retention.StorageArea, Is.EqualTo(4.56));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFile_UseTablePropertyNotInFile_LogsError()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string content =
                    "[General]                                 " + Environment.NewLine +
                    "    fileVersion           = 2.00          " + Environment.NewLine +
                    "    fileType              = iniField      " + Environment.NewLine +
                    "    useStreetStorage      = 1             " + Environment.NewLine +
                    "                                          " + Environment.NewLine +
                    "[StorageNode]                             " + Environment.NewLine +
                    "    id                    = some_id       " + Environment.NewLine +
                    "    name                  = some_name     " + Environment.NewLine +
                    "    branchId              = some_branch   " + Environment.NewLine +
                    "    chainage              = 50.000000     " + Environment.NewLine +
                    "    numLevels             = 1             " + Environment.NewLine +
                    "    levels                = 1.23          " + Environment.NewLine +
                    "    storageArea           = 4.56          " + Environment.NewLine +
                    "    interpolate           = block         ";

                string filePath = temp.CreateFile("nodeFile.ini", content);

                IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

                // Call
                void Call() => RetentionFileReader.ReadFile(filePath, network);

                // Assert
                string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
                Assert.That(error, Is.EqualTo("The category StorageNode on line 6 does not contain the useTable property."));
                Assert.That(network.Retentions, Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFile_WithRetentionWithMultipleLevels_AddCorrectRetentionToNetwork()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string content =
                    "[General]                                 " + Environment.NewLine +
                    "    fileVersion           = 2.00          " + Environment.NewLine +
                    "    fileType              = iniField      " + Environment.NewLine +
                    "    useStreetStorage      = 1             " + Environment.NewLine +
                    "                                          " + Environment.NewLine +
                    "[StorageNode]                             " + Environment.NewLine +
                    "    id                    = some_id       " + Environment.NewLine +
                    "    name                  = some_name     " + Environment.NewLine +
                    "    branchId              = some_branch   " + Environment.NewLine +
                    "    chainage              = 50.000000     " + Environment.NewLine +
                    "    useTable              = True          " + Environment.NewLine +
                    "    numLevels             = 3             " + Environment.NewLine +
                    "    levels                = 1.11 2.22 3.33" + Environment.NewLine +
                    "    storageArea           = 4.44 5.55 6.66" + Environment.NewLine +
                    "    interpolate           = linear        ";

                string filePath = temp.CreateFile("nodeFile.ini", content);

                IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

                // Call
                RetentionFileReader.ReadFile(filePath, network);

                // Assert
                IRetention retention = network.Retentions.Single();
                Assert.That(retention.Name, Is.EqualTo("some_id"));
                Assert.That(retention.LongName, Is.EqualTo("some_name"));
                Assert.That(retention.Branch, Is.SameAs(network.Branches[0]));
                Assert.That(retention.Chainage, Is.EqualTo(50));
                Assert.That(retention.Geometry, Is.EqualTo(new Point(0, 50)));
                Assert.That(retention.UseTable, Is.True);
                Assert.That(retention.Data.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
                Assert.That(retention.Data.Arguments[0].GetValues<double>().ToArray(), Is.EquivalentTo(new[]
                {
                    1.11,
                    2.22,
                    3.33
                }));
                Assert.That(retention.Data.Components[0].GetValues<double>().ToArray(), Is.EquivalentTo(new[]
                {
                    4.44,
                    5.55,
                    6.66
                }));
            }
        }

        private static IHydroNetwork GetNetworkWithChannel(double length, string branchName)
        {
            var node1 = new HydroNode("some_node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("some_node2") {Geometry = new Point(0, length)};

            var channel = new Channel(branchName, node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, length)
                })
            };

            var network = new HydroNetwork();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(channel);

            return network;
        }
    }
}