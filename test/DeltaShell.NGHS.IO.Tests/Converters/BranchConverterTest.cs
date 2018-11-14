using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;
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

        [Test]
        public void GivenACorrectBranch_WhenConverting_ThenAListOfBranchesIsReturned()
        {
            var branchCategory = CreateBranchDelftIniCategory();
            var categories = new List<DelftIniCategory> {branchCategory};

            var nodes = GetTestHydroNodes();
            var branches = BranchConverter.Convert(categories, nodes, new List<string>());

            Assert.AreEqual(1, branches.Count);
            Assert.AreEqual(branches[0].Geometry.Coordinates[0], new Coordinate(0.0,0.0, double.NaN));
            Assert.AreEqual(branches[0].Geometry.Coordinates[1], new Coordinate(100.0,0.0, double.NaN));
        }


        [Test]
        public void GivenDuplicateBranches_WhenConverting_ThenAnErrorMessageIsProduced()
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
        public void GivenACorrectBranch_WhenConvertingWithANetworkThatIsNull_ThenAnExceptionIsThrown()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = CreateBranchDelftIniCategory();
            categories.Add(category1);

            var amountOfExceptions = new List<string>();
            BranchConverter.Convert(categories, null, amountOfExceptions);

            Assert.AreEqual(1, amountOfExceptions.Count);
        }

        private static DelftIniCategory CreateBranchDelftIniCategory()
        {
            var category1 = new DelftIniCategory("Branch");
            category1.AddProperty("id", "branch1");
            category1.AddProperty("fromNode", NodeName1);
            category1.AddProperty("toNode", NodeName2);
            category1.AddProperty("order", "0");
            category1.AddProperty("geometry", "LINESTRING (0 0, 100 0)");
            return category1;
        }

        private static List<INode> GetTestHydroNodes()
        {
            return new List<INode> {new HydroNode(NodeName1), new HydroNode(NodeName2)};
        }
    }
}

