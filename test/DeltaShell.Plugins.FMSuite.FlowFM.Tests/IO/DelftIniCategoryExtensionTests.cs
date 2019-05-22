using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class DelftIniCategoryExtensionTests
    {
        [Test]
        public void AddSedimentPropertyTest()
        {
            var category = new DelftIniCategory("category");
            category.AddSedimentProperty(SedimentFile.Name.Key,"MyValue","","");
            var addedProperty = category.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.That(addedProperty.Value, Is.Not.StringContaining("#")); // Don't automaticlly add hashes, responsibility of caller!
            Assert.AreEqual("MyValue", addedProperty.Value);
        }

        [Test]
        public void GivenADelftIniCategoryWithPropertiesInWrongCase_WhenReadingProperties_ThenCorrectPropertiesAreReturned()
        {
            // Given
            var category = new DelftIniCategory(LocationRegion.Name.Key);
            category.AddProperty("PROPERTY1", "VALUE1");
            category.AddProperty("property2", "value2");

            // When
            var readProperty1 = category.ReadProperty<string>("property1");
            var readProperty2 = category.ReadProperty<string>("PROPERTY2");

            // Then
            Assert.That(readProperty1, Is.EqualTo("VALUE1"));
            Assert.That(readProperty2, Is.EqualTo("value2"));
        }
    }
}