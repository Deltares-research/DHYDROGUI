using System.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class ZwFromGisImporterTest
    {
        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new ZwFromGisImporter(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void GivenLevel_WhenUpdateNumberOfLevels_ThenExpectNumberOfLevelsIsGivenLevel(int levels)
        {
            //Arrange
            var importer = new ZwFromGisImporter(new Dictionary<string, string>());
            var propertyMapping1 = new PropertyMapping("Test 1");
            var propertyMapping2 = new PropertyMapping("Test 2");

            var propertyMappings = new List<PropertyMapping>
            {
                propertyMapping1,
                propertyMapping2
            };

            for (var i = 1; i <= levels; i++)
            {
                propertyMappings.Add(new PropertyMapping($"Level {i}"));
            }

            //Act
            importer.UpdateNumberOfLevels(propertyMappings);

            //Assert
            Assert.That(importer.NumberOfLevels, Is.EqualTo(levels));
        }

        [Test]
        public void GivenPropertyMappingsList_WhenPropertiesAddedToImporterAndMakeNumberOfLevelPropertiesMapping_ThenPropertyMappingUpdatedWithLevelsAndAddedProperties()
        {
            //Arrange
            const int levels = 3;

            var propertyMapping1 = new PropertyMapping("Test 1");
            var propertyMapping2 = new PropertyMapping("Test 2");
            var propertyMapping3 = new PropertyMapping("Test 3");
            var propertyMapping4 = new PropertyMapping("Test 4");

            var propertyMappings = new List<PropertyMapping>
            {
                propertyMapping1,
                propertyMapping2
            };

            var propertyMapping = new Dictionary<string, string>
            {
                { propertyMapping3.PropertyName, "m" },
                { propertyMapping4.PropertyName, "m" }
            };

            var importer = new ZwFromGisImporter(propertyMapping);

            //Act
            importer.MakeNumberOfLevelPropertiesMapping(levels, propertyMappings);

            //Assert
            Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == propertyMapping1.PropertyName), Is.True);
            Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == propertyMapping2.PropertyName), Is.True);
            for (var i = 1; i <= levels; i++)
            {
                Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == $"Level {i}"), Is.True);
                Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == $"{propertyMapping3.PropertyName} {i}"), Is.True);
                Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == $"{propertyMapping4.PropertyName} {i}"), Is.True);
            }
        }

        [Test]
        public void GivenLevelAddedAndSetup_WhenLevelsDecreasedAndMakeNumberOfLevelPropertiesMappingThenPropertyMappingUpdatedWithLevelsAndAddedProperties()
        {
            //Arrange
            const int firstLevels = 3;
            const int secondLevels = 2;

            var propertyMapping1 = new PropertyMapping("Test 1");
            var propertyMapping2 = new PropertyMapping("Test 2");
            var propertyMapping3 = new PropertyMapping("Test 3");
            var propertyMapping4 = new PropertyMapping("Test 4");

            var propertyMappings = new List<PropertyMapping>
            {
                propertyMapping1,
                propertyMapping2
            };

            var propertyMapping = new Dictionary<string, string>
            {
                { propertyMapping3.PropertyName, "m" },
                { propertyMapping4.PropertyName, "m" }
            };

            var importer = new ZwFromGisImporter(propertyMapping);

            //Act
            importer.MakeNumberOfLevelPropertiesMapping(firstLevels, propertyMappings);
            importer.MakeNumberOfLevelPropertiesMapping(secondLevels, propertyMappings);

            //Assert
            Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == propertyMapping1.PropertyName), Is.True);
            Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == propertyMapping2.PropertyName), Is.True);
            for (var i = 1; i <= secondLevels; i++)
            {
                Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == $"Level {i}"), Is.True);
                Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == $"{propertyMapping3.PropertyName} {i}"), Is.True);
                Assert.That(propertyMappings.Exists(mapping => mapping.PropertyName == $"{propertyMapping4.PropertyName} {i}"), Is.True);
            }
        }
    }
}