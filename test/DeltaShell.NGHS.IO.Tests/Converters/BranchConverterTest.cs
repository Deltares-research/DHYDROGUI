using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class BranchConverterTest
    {

        [Test]
        public void GivenAnCorrectBranch_WhenConverting_ThenAListOfBranchesIsReturned()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = CreateBranchDelftIniCategory();

            categories.Add(category1);

            var branches = BranchConverter.Convert(categories, new HydroNetwork(), new List<FileReadingException>());
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
            category2.AddProperty("gridPointsCount", "10");
            category2.AddProperty("gridPointX", "11.000 13.278 15.556 17.833 20.111 22.389 24.667 26.944 29.222 31.500");
            category2.AddProperty("gridPointY", "13.500 16.111 18.722 21.333 23.944 26.556 29.167 31.778 34.389 37.000");
            category2.AddProperty("gridPointOffsets", "0.000 11.111 22.222 33.333 44.444 55.556 66.667 77.778 88.889 100.000");
            category2.AddProperty("gridPointIds", "branch_0.000;branch_11.111;branch_22.222;branch_33.333;branch_44.444;branch_55.556;branch_66.667;branch_77.778;branch_88.889;branch_100.000");

            categories.Add(category1);
            categories.Add(category2);

            var amountOfExceptions = new List<FileReadingException>();

            BranchConverter.Convert(categories, new HydroNetwork(), amountOfExceptions);

            Assert.AreEqual(1, amountOfExceptions.Count);
        }

        [Test]
        public void GivenACorrectBranch_WhenConvertingWithANetworkThatIsNull_ThenAnExceptionIsThrown()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = CreateBranchDelftIniCategory();

            categories.Add(category1);

            var amountOfExceptions = new List<FileReadingException>();

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
            category1.AddProperty("gridPointsCount", "10");
            category1.AddProperty("gridPointX", "11.000 13.278 15.556 17.833 20.111 22.389 24.667 26.944 29.222 31.500");
            category1.AddProperty("gridPointY", "13.500 16.111 18.722 21.333 23.944 26.556 29.167 31.778 34.389 37.000");
            category1.AddProperty("gridPointOffsets", "0.000 11.111 22.222 33.333 44.444 55.556 66.667 77.778 88.889 100.000");
            category1.AddProperty("gridPointIds",
                "branch_0.000;branch_11.111;branch_22.222;branch_33.333;branch_44.444;branch_55.556;branch_66.667;branch_77.778;branch_88.889;branch_100.000");
            return category1;
        }
    }
}

