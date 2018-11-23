using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class BranchConverterTest
    {
        private const string NodeName1 = "node1";
        private const string NodeName2 = "node2";
        private readonly string[] gridPointsX = { "0.0", "50.0", "100.0" };
        private readonly string[] gridPointsY = { "0.0", "0.0", "0.0" };
        private readonly string[] gridPointsOffSets = { "0.0", "75.0", "150.0" };
        private readonly string[] gridPointsNames = { "Channel1_0.000", "Channel1_50.000", "Channel1_100.000" };

        [Test]
        public void GivenCorrectBranchDataModel_WhenConverting_ThenAListOfBranchesIsReturned()
        {
            var branchCategory = CreateBranchDelftIniCategory();
            var categories = new List<DelftIniCategory> {branchCategory};

            var nodes = GetTestHydroNodes();
            var branches = BranchConverter.Convert(categories, nodes, new List<string>());

            Assert.AreEqual(1, branches.Count);

            var branch = branches.First();
            Assert.AreEqual(branch.Geometry.Coordinates[0], new Coordinate(0.0, 0.0, double.NaN));
            Assert.AreEqual(branch.Geometry.Coordinates[1], new Coordinate(100.0, 0.0, double.NaN));
        }


        [Test]
        public void GivenTwoBranchCategoriesWithDuplicateIds_WhenConverting_ThenAnErrorMessageIsProducedAndOneBranchIsReturned()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = CreateBranchDelftIniCategory();
            var category2 = CreateBranchDelftIniCategory();

            categories.Add(category1);
            categories.Add(category2);

            var errorMessages = new List<string>();
            var nodes = GetTestHydroNodes();
            var branches = BranchConverter.Convert(categories, nodes, errorMessages);

            Assert.AreEqual(1, branches.Count);
            Assert.That(errorMessages.Any(m => m.EndsWith("branch id's cannot be duplicates.")), "A message about duplicate branch id's should have been produced, but was not.");
        }

        [Test]
        public void GivenACorrectBranchDataModel_WhenConvertingWithANetworkThatIsNull_ThenAnExceptionIsThrown()
        {
            var categories = new List<DelftIniCategory> { CreateBranchDelftIniCategory() };
            var errorMessages = new List<string>();
            BranchConverter.Convert(categories, null, errorMessages);

            Assert.AreEqual(1, errorMessages.Count);
        }

        [TestCase("0.0", "75.0", "150.0")]
        [TestCase("0.0", "75.0", "100.001")]
        [TestCase("0.0", "25.0", "50.0")]
        [TestCase("0.0", "75.0", "99.999")]
        public void GivenCorrectBranchDataModelWithGridPointOffsetSignificantlyLargerThanBranchEuclideanLenght_WhenConvertingToBranch_ThenBranchHasCustomLengthEqualToTheLargestOffsetValue(string smallestOffset, string middleOffset, string largestOffset)
        {
            var branchCategory = CreateBranchDelftIniCategory();
            branchCategory.SetProperty(NetworkDefinitionRegion.GridPointOffsets.Key, string.Join(" ", smallestOffset, middleOffset, largestOffset));
            var categories = new List<DelftIniCategory> { branchCategory };
            var errorMessages = new List<string>();
            var branches = BranchConverter.Convert(categories, GetTestHydroNodes(), errorMessages);

            var branch = branches.FirstOrDefault();
            var expectedCustomBranchLength = double.Parse(largestOffset, CultureInfo.InvariantCulture);
            Assert.IsNotNull(branch);
            Assert.IsTrue(branch.IsLengthCustom);
            Assert.That(branch.Length, Is.EqualTo(expectedCustomBranchLength));
        }

        private DelftIniCategory CreateBranchDelftIniCategory()
        {
            var branchCategory = new DelftIniCategory(NetworkDefinitionRegion.IniBranchHeader);
            branchCategory.AddProperty(NetworkDefinitionRegion.Id.Key, "branch1");
            branchCategory.AddProperty(NetworkDefinitionRegion.FromNode.Key, NodeName1);
            branchCategory.AddProperty(NetworkDefinitionRegion.ToNode.Key, NodeName2);
            branchCategory.AddProperty(NetworkDefinitionRegion.BranchOrder.Key, "0");
            branchCategory.AddProperty(NetworkDefinitionRegion.Geometry.Key, "LINESTRING (0 0, 100 0)");
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointsCount.Key, "3");
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointX.Key, string.Join(" ", gridPointsX));
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointY.Key, string.Join(" ", gridPointsY));
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointOffsets.Key, string.Join(" ", gridPointsOffSets));
            branchCategory.AddProperty(NetworkDefinitionRegion.GridPointNames.Key, string.Join(";", gridPointsNames));
            return branchCategory;
        }

        private static List<INode> GetTestHydroNodes()
        {
            return new List<INode> {new HydroNode(NodeName1), new HydroNode(NodeName2)};
        }
    }
}

