using System;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.DataAccessBuilders
{
    [TestFixture]
    public class DelftIniCategoryFactoryTest
    {
        [Test]
        public void CreateBoundaryBlock_CreatesCorrectDelftIniCategory()
        {
            const string quantity = "some_quantity";
            const string locationFilePath = "some_location_file_path";
            const string forcingFilePath = "some_forcing_file_path";
            var thatcherHarlemanTimeLag = new TimeSpan(1, 2, 3);
            const bool isEmbankment = true;

            // Call
            DelftIniCategory result = DelftIniCategoryFactory.CreateBoundaryBlock(quantity, locationFilePath, forcingFilePath, thatcherHarlemanTimeLag, isEmbankment);

            // Assert
            Assert.That(result.Name, Is.EqualTo("[boundary]"));
            Assert.That(result.Properties, Has.Count.EqualTo(5));
            CategoryContains(result, "quantity", quantity);
            CategoryContains(result, "locationfile", locationFilePath);
            CategoryContains(result, "forcingfile", forcingFilePath);
            CategoryContains(result, "return_time", "3.7230000e+003");
            CategoryContains(result, "OpenBoundaryTolerance", "5.0000000e-001");
        }

        [Test]
        public void CreateBoundaryBlock_InvalidValues_CreatesCorrectDelftIniCategory()
        {
            // Call
            DelftIniCategory result = DelftIniCategoryFactory.CreateBoundaryBlock(null, null, null, TimeSpan.Zero);

            // Assert
            Assert.That(result.Name, Is.EqualTo("[boundary]"));
            Assert.That(result.Properties, Has.Count.EqualTo(0));
            CategoryDoesNotContain(result, "quantity");
            CategoryDoesNotContain(result, "locationfile");
            CategoryDoesNotContain(result, "forcingfile");
            CategoryDoesNotContain(result, "return_time");
            CategoryDoesNotContain(result, "OpenBoundaryTolerance");
        }

        public static void CategoryContains(DelftIniCategory category, string propertyName, object propertyValue)
        {
            DelftIniProperty property = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            Assert.That(property, Is.Not.Null,
                        $"Category should contain property <{propertyName}>.");
            Assert.That(property.Value, Is.EqualTo(propertyValue),
                        $"Property '{propertyName}' has an incorrect value.");
        }

        public static void CategoryDoesNotContain(DelftIniCategory category, string propertyName)
        {
            DelftIniProperty property = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            Assert.That(property, Is.Null,
                        $"Category should not contain property <{propertyName}>.");
        }
    }
}