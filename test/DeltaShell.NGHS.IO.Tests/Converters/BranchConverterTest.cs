using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class BranchConverterTest
    {

        [Test]
        public void GivenACorrectBranch_WhenConverting_ThenAListOfBranchesIsReturned()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = CreateBranchDelftIniCategory();

            categories.Add(category1);

            var branches = BranchConverter.Convert(categories, new HydroNetwork().Nodes, new List<string>());
            Assert.AreEqual(1, branches.Count);
        }
        

        [Test]
        public void GivenDuplicateBranches_WhenConverting_ThenAnExceptionisThrown()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = CreateBranchDelftIniCategory();

            var category2 = new DelftIniCategory("Branch");
            category2.AddProperty("id", "branch1");
            category2.AddProperty("fromNode", "node1");
            category2.AddProperty("toNode", "node2");
            category2.AddProperty("order", "0");
            category2.AddProperty("geometry", "LINESTRING (0 0, 100 0)");

            categories.Add(category1);
            categories.Add(category2);

            var errorMessages = new List<string>();
            BranchConverter.Convert(categories, new HydroNetwork().Nodes, errorMessages);
            
            Assert.That(errorMessages.Any(m => m.EndsWith("branch id's cannot be duplicates.")));
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
            category1.AddProperty("fromNode", "node1");
            category1.AddProperty("toNode", "node2");
            category1.AddProperty("order", "0");
            category1.AddProperty("geometry", "LINESTRING (0 0, 100 0)");
            return category1;
        }
    }
}

