using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Network;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class BranchConverterTest
    {
        [Test]
        public void GivenAnIniFileWithCategories_WhenConverting_ThenAListOfBranchesIsReturned()
        {
            var testFile = TestHelper.GetTestFilePath(@"BranchConverterTest\NetworkDefinition.ini");
            var categories = DelftIniFileParser.ReadFile(testFile);
            Assert.IsNotNull(categories);

            var branches = BranchConverter.Convert(categories, new HydroNetwork(), new List<FileReadingException>());
            Assert.AreEqual(1, branches.Count);
        }


        [Test]
        public void GivenAnIniFileWithDuplicateBranches_WhenConverting_ThenAnExceptionisThrown()
        {
            var testFile = TestHelper.GetTestFilePath(@"BranchConverterTest\NetworkDefinitionWithDuplicateBranch.ini");
            var categories = DelftIniFileParser.ReadFile(testFile);
            Assert.IsNotNull(categories);

            var amountOfExceptions = new List<FileReadingException>();

            BranchConverter.Convert(categories, new HydroNetwork(), amountOfExceptions);

            Assert.AreEqual(1, amountOfExceptions.Count);
        }


        [Test]
        public void GivenAnIniFileWithCategories_WhenConverting_ThenAnExceptionIsThrown()
        {
            var testFile = TestHelper.GetTestFilePath(@"BranchConverterTest\NetworkDefinition.ini");
            var categories = DelftIniFileParser.ReadFile(testFile);
            Assert.IsNotNull(categories);

            var amountOfExceptions = new List<FileReadingException>();
            BranchConverter.Convert(categories, null, amountOfExceptions);
            Assert.AreEqual(1, amountOfExceptions.Count);
        }
    }
}

