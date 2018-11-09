using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class HydroNodeConverterTest
    {
        [Test]
        public void GivenNetworkDefinitionDataModel_WhenConvertingToHydroNodes_ThenAListOfNodesIsReturned()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = new DelftIniCategory("Node");
            category1.AddProperty("id", "node1");
            category1.AddProperty("x", 11.0);
            category1.AddProperty("y", 13.5);

            var category2 = new DelftIniCategory("Node");
            category2.AddProperty("id", "node2");
            category2.AddProperty("x", 31.5);
            category2.AddProperty("y", 37.0);

            categories.Add(category1);
            categories.Add(category2);

            var nodes = HydroNodeConverter.Convert(categories, new List<FileReadingException>());
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void GivenAnIniFileWithMissingXValues_WhenConverting_ThenAnExceptionisThrown()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = new DelftIniCategory("Node");
            category1.AddProperty("id", "node1");
            category1.AddProperty("y", 13.5);

            var category2 = new DelftIniCategory("Node");
            category2.AddProperty("id", "node2");
            category2.AddProperty("x", 31.5);
            category2.AddProperty("y", 37.0);

            categories.Add(category1);
            categories.Add(category2);

            var amountOfExceptions = new List<FileReadingException>();

            HydroNodeConverter.Convert(categories, amountOfExceptions);

            Assert.AreEqual(1, amountOfExceptions.Count);
        }

        [Test]
        public void GivenAnIniFileWithDuplicateNodes_WhenConverting_ThenAnExceptionisThrown()
        {
            var categories = new List<DelftIniCategory>();
            var category1 = new DelftIniCategory("Node");
            category1.AddProperty("id", "node1");
            category1.AddProperty("x", 11.0);
            category1.AddProperty("y", 13.5);

            var category2 = new DelftIniCategory("Node");
            category2.AddProperty("id", "node1");
            category2.AddProperty("x", 31.5);
            category2.AddProperty("y", 37.0);

            categories.Add(category1);
            categories.Add(category2);

            var amountOfExceptions = new List<FileReadingException>();

            HydroNodeConverter.Convert(categories, amountOfExceptions);

            Assert.AreEqual(1, amountOfExceptions.Count);
        }
    }
}
