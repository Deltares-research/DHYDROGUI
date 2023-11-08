using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.Network;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Network
{
    [TestFixture]
    public class RoutesFileTest
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Write_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Call
            void Call() => RoutesFile.Write(filePath, Enumerable.Empty<Route>());

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Write_RoutesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => RoutesFile.Write("random string", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GivenNetworkWithRoutes_WhenWritingRoutesFileToTempFolder_ThenFileAsExpected()
        {
            // Given
            var emptyRoute = new Route { Name = "route 1" };

            var routeWithOneLocation = new Route { Name = "route 2" };

            var routeWithMultipleLocationsOnSingleBranch = new Route { Name = "route 3" };

            var routeWithMultipleLocationsOnMultipleBranches = new Route { Name = "route 4" };

            var channel = new Channel { Name = "Channel 1" };

            var pipe = new Pipe { Name = "Pipe 1" };

            var networkLocation1 = new NetworkLocation(channel, 100.0);
            var networkLocation2 = new NetworkLocation(channel, 200.0);
            var networkLocation3 = new NetworkLocation(pipe, 300.0);
            var networkLocation4 = new NetworkLocation(pipe, 400.0);

            routeWithOneLocation.Locations.AddValues(new[] { networkLocation1 });

            routeWithMultipleLocationsOnSingleBranch.Locations.AddValues(new[] { networkLocation1, networkLocation2 });

            routeWithMultipleLocationsOnMultipleBranches.Locations.AddValues(new[] { networkLocation1, networkLocation2, networkLocation3, networkLocation4 });

            Route[] routes = { emptyRoute, routeWithOneLocation, routeWithMultipleLocationsOnSingleBranch, routeWithMultipleLocationsOnMultipleBranches };

            using (var tempFolder = new TemporaryDirectory())
            {
                string expectedFilePath = TestHelper.GetTestFilePath(@"FileWriters\Routes_expected.txt");
                string actualFilePath = Path.Combine(tempFolder.Path, RoutesFile.RoutesFileName);

                // When
                RoutesFile.Write(actualFilePath, routes);

                // Then
                Assert.That(File.Exists(actualFilePath), Is.True);
                
                string expected = File.ReadAllText(expectedFilePath);
                string actual = File.ReadAllText(actualFilePath);

                Assert.That(expected, Is.EqualTo(actual));
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Read_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Call
            void Call() => RoutesFile.Read(filePath, Substitute.For<IHydroNetwork>());

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Read_NetworkNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => RoutesFile.Read("random string", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GivenNetworkWithBranches_WhenReadingRoutesFile_ThenRoutesAddedAsExpected()
        {
            // Given
            var channel = new Channel { Name = "Channel 1" };

            var pipe = new Pipe { Name = "Pipe 1" };

            var hydroNetwork = new HydroNetwork
            {
                Branches =
                {
                    channel,
                    pipe
                }
            };

            var expectedNetworkLocation1 = new NetworkLocation(channel, 100.0);
            var expectedNetworkLocation2 = new NetworkLocation(channel, 200.0);
            var expectedNetworkLocation3 = new NetworkLocation(pipe, 300.0);
            var expectedNetworkLocation4 = new NetworkLocation(pipe, 400.0);

            // When
            RoutesFile.Read(TestHelper.GetTestFilePath(@"FileWriters\Routes_expected.txt"), hydroNetwork);

            // Then
            Assert.AreEqual(4, hydroNetwork.Routes.Count);

            Route route1 = hydroNetwork.Routes[0];
            Assert.AreEqual("route 1", route1.Name);
            Assert.IsEmpty(route1.Locations.AllValues);

            Route route2 = hydroNetwork.Routes[1];
            Assert.AreEqual("route 2", route2.Name);
            Assert.AreEqual(route2.Locations.AllValues,
                            new[] { expectedNetworkLocation1 });

            Route route3 = hydroNetwork.Routes[2];
            Assert.AreEqual("route 3", route3.Name);
            Assert.AreEqual(route3.Locations.AllValues,
                            new[] { expectedNetworkLocation1, expectedNetworkLocation2 });

            Route route4 = hydroNetwork.Routes[3];
            Assert.AreEqual("route 4", route4.Name);
            Assert.AreEqual(route4.Locations.AllValues,
                            new[] { expectedNetworkLocation1, expectedNetworkLocation2, expectedNetworkLocation3, expectedNetworkLocation4 });
        }
    }
}