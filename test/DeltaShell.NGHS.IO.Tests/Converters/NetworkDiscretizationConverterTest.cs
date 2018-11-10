using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class NetworkDiscretizationConverterTest
    {
        private readonly string channelName = "Channel1";
        private readonly string[] gridPointsX = { "22.0", "22.0", "22.0" };
        private readonly string[] gridPointsY = { "0.0", "50.0", "100.0" };
        private readonly string[] gridPointsOffSets = { "0.0", "50.0", "100.0" };
        private readonly string[] gridPointsNames = { "Channel1_0.000", "Channel1_50.000", "Channel1_100.000" };
        private readonly Channel channel1 = new Channel("Channel1", new HydroNode("Node001") { Geometry = new Point(22.0, 0.0) }, new HydroNode("Node002") { Geometry = new Point(22.0, 100.0) });
        private readonly Channel channel2 = new Channel("Channel2", new HydroNode("Node001") { Geometry = new Point(22.0, 0.0) }, new HydroNode("Node002") { Geometry = new Point(22.0, 100.0) });

        [Test]
        public void GivenValidNetworkDiscretisationDataModel_WhenConverting_ThenCorrectNetworkLocationsAreReturned()
        {
            var categories = new List<DelftIniCategory>();
            var branchCategory = CreateCorrectBranchCategory();
            categories.Add(branchCategory);

            var branches = new List<IBranch> { channel1 };
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, branches, new List<string>());

            Assert.That(networkLocations.Count, Is.EqualTo(3));
            for (var n = 0; n < networkLocations.Count; n++)
            {
                var networkLocation = networkLocations[n];
                Assert.That(networkLocation.Branch, Is.EqualTo(channel1));
                Assert.That(networkLocation.Chainage, Is.EqualTo(double.Parse(gridPointsOffSets[n], CultureInfo.InvariantCulture)));
                Assert.That(networkLocation.Geometry, Is.EqualTo(new Point(double.Parse(gridPointsX[n], CultureInfo.InvariantCulture), double.Parse(gridPointsY[n], CultureInfo.InvariantCulture))));
                Assert.That(networkLocation.Name, Is.EqualTo(gridPointsNames[n]));
            }
        }

        [Test]
        public void GivenNetworkDiscretisationDataModel_WhenConvertingWithoutReferencedBranches_ThenErrorMessageIsReturnedAndNoNetworkLocations()
        {
            var categories = new List<DelftIniCategory>();
            var branchCategory = CreateCorrectBranchCategory();
            categories.Add(branchCategory);

            var branches = new List<IBranch> {channel2};
            var errorMessages = new List<string>();
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, branches, errorMessages);

            Assert.That(networkLocations.Count, Is.EqualTo(0));
            Assert.That(errorMessages.Any(m => m.Contains($"Could not add discretization points for branch with id '{channelName}', because it was not found in the network.")));
        }

        [Test]
        public void GivenNetworkDiscretisationDataModelWithZeroGridPointCount_WhenConverting_ThenErrorMessageIsReturnedAndNoNetworkLocations()
        {
            var categories = new List<DelftIniCategory>();
            var branchCategory = CreateCorrectBranchCategory();
            branchCategory.SetProperty(NetworkDefinitionRegion.GridPointsCount.Key, "0"); // Change gridPointCount value to zero
            categories.Add(branchCategory);

            var branches = new List<IBranch> { channel1 };
            var errorMessages = new List<string>();
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, branches, errorMessages);

            Assert.That(networkLocations.Count, Is.EqualTo(0));
            Assert.That(errorMessages.Any(m => m.Contains($"There are zero discretization points defined for branch with id '{channelName}")));
        }

        [Test]
        public void GivenNetworkDiscretisationDataModelWithWrongGridPointsCount_WhenConverting_ThenErrorMessagesAreReturnedAndNoNetworkLocations()
        {
            var categories = new List<DelftIniCategory>();
            var branchCategory = CreateCorrectBranchCategory();
            branchCategory.SetProperty(NetworkDefinitionRegion.GridPointsCount.Key, "4"); // Change gridPointCount value to four
            categories.Add(branchCategory);

            var branches = new List<IBranch> { channel1 };
            var errorMessages = new List<string>();
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, branches, errorMessages);

            Assert.That(networkLocations.Count, Is.EqualTo(0));
            Assert.That(errorMessages.Any(m => m.Contains("The amount of x-coordinates defined for discretization points")));
            Assert.That(errorMessages.Any(m => m.Contains("The amount of y-coordinates defined for discretization points")));
            Assert.That(errorMessages.Any(m => m.Contains("The amount of offsets defined for discretization points")));
            Assert.That(errorMessages.Any(m => m.Contains("The amount of names defined for discretization points")));
        }

        private DelftIniCategory CreateCorrectBranchCategory()
        {
            var branchCategory = new DelftIniCategory(NetworkDefinitionRegion.IniBranchHeader);

            branchCategory.AddProperty(NetworkDefinitionRegion.Id.Key, channelName);
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointsCount.Key, "3");
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointX.Key, string.Join(" ", gridPointsX));
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointY.Key, string.Join(" ", gridPointsY));
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointOffsets.Key, string.Join(" ", gridPointsOffSets));
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointNames.Key, string.Join(";", gridPointsNames));
            return branchCategory;
        }
    }
}