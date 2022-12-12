using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class YzFromGisImporterTest
    {
        [Test]
        [TestCaseSource(nameof(ValidListOfValuesForConverting))]
        public void GivenListOFValues_WhenConvertPropertyMappingToList_ThenReceivedListIsExpectedList(string listOfValues, IList<double> expectedListOfValues)
        {
            //Arrange
            var importer = new YzFromGisImporter();

            //Assert
            IList<double> receivedListOfValues = importer.ConvertPropertyMappingToList(listOfValues);

            //Act
            Assert.That(receivedListOfValues.Count, Is.EqualTo(expectedListOfValues.Count));
            Assert.That(receivedListOfValues, Is.EqualTo(expectedListOfValues));
        }

        [Test]
        public void GivenInvalidListOFValues_WhenConvertPropertyMappingToList_ThenFormatExceptionIsThrown()
        {
            //Arrange
            var importer = new YzFromGisImporter();

            //Act & Assert
            Assert.Throws<FormatException>(() => importer.ConvertPropertyMappingToList("1,0.-5,3.10,23.10"));
        }

        [Test]
        public void GivenYAndZCoordinates_WhenConvertYzProperties_ThenCoordinatesArePlacedInCrossSection()
        {
            var importer = new YzFromGisImporter();
            var cs = new CrossSectionDefinitionYZ();
            IList<double> yCoordinates = new List<double>()
            {
                0.0,
                1.1
            };
            IList<double> zCoordinates = new List<double>()
            {
                2.0,
                2.2
            };

            importer.ConvertYzProperties(cs, yCoordinates, zCoordinates);

            for (var i = 0; i < cs.YZDataTable.Count; i++)
            {
                Assert.That(cs.YZDataTable[i].Yq, Is.EqualTo(yCoordinates[i]));
                Assert.That(cs.YZDataTable[i].Z, Is.EqualTo(zCoordinates[i]));
            }
        }

        private static IEnumerable<TestCaseData> ValidListOfValuesForConverting()
        {
            yield return new TestCaseData("1.0,-5.3,10.23,10", new List<double>()
            {
                1.0,
                -5.3,
                10.23,
                10
            });
            yield return new TestCaseData("1.0 , -5.3 ,10.23,10", new List<double>()
            {
                1.0,
                -5.3,
                10.23,
                10
            });
            yield return new TestCaseData("1.0 , -5.3 , 10.23 , 10", new List<double>()
            {
                1.0,
                -5.3,
                10.23,
                10
            });
            yield return new TestCaseData("1.0 , -5.333242 , 10.23 , 10", new List<double>()
            {
                1.0,
                -5.333242,
                10.23,
                10
            });
        }
    }
}