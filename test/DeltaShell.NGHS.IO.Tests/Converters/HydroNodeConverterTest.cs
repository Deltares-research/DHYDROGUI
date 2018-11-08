using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Network;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class HydroNodeConverterTest
    {
        [Test]
        public void GivenAnIniFileWithCategories_WhenConverting_ThenAListOfNodesIsReturned()
        {

            var testFile = TestHelper.GetTestFilePath(@"HydroNodeConvertTest\NetworkDefinition.ini");
            var categories = DelftIniFileParser.ReadFile(testFile);
            Assert.IsNotNull(categories);

            HydroNodeConverter.Convert(categories, new List<FileReadingException>());
            Assert.AreEqual(4, categories.Count);
        }

        [Test]
        [TestCase(@"HydroNodeConvertTest\NetworkDefinitionWithMissingX.ini")]
        [TestCase(@"HydroNodeConvertTest\NetworkDefinitionWithDuplicateNode.ini")]
        public void GivenAnIniFileWithMissingXValuesOrDuplicateNodes_WhenConverting_ThenAnExceptionisThrown(string filePath)
        {
            var testFile = TestHelper.GetTestFilePath(filePath);
            var categories = DelftIniFileParser.ReadFile(testFile);
            Assert.IsNotNull(categories);

            var amountOfExceptions = new List<FileReadingException>();

            HydroNodeConverter.Convert(categories, amountOfExceptions);

            Assert.AreEqual(1, amountOfExceptions.Count);
        }
    }
}
