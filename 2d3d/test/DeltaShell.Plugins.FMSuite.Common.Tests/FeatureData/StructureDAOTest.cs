using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    [TestFixture]
    public class StructureDAOTest
    {
        [Test]
        [TestCase("Pump", StructureType.Pump)]
        [TestCase("Gate", StructureType.Gate)]
        [TestCase("GeneralStructure", StructureType.GeneralStructure)]
        [TestCase("Weir", StructureType.Weir)]
        public void GivenAStructureDAOWithACorrectName_WhenCallingConstructor_StructureIsCreated(string structureName, StructureType type)
        {
            var structure = new StructureDAO(structureName);
            Assert.That(structure.StructureType, Is.EqualTo(type));
        }

        [Test]
        public void GetPropertyTest()
        {
            var structure = new StructureDAO("test");
            Assert.IsNull(structure.GetProperty("name"));

            structure.AddProperty("name", typeof(string), "value");

            ModelProperty property = structure.GetProperty("NaMe");
            Assert.IsNotNull(property, "Case insensitivity expected.");
            Assert.AreEqual(structure.Properties.ElementAt(0), property);
        }

        [Test]
        public void GivenStructureDAOWhenInstantiatingWithEmptyStringThenStructureHasInvalidType()
        {
            var structure = new StructureDAO("");
            Assert.That(structure.StructureType, Is.EqualTo(StructureType.InvalidType));
        }

        [Test]
        public void WhenInstantiatingStructureDAOWithNullValueThenStructureHasInvalidType()
        {
            var structure = new StructureDAO(null);
            Assert.That(structure.StructureType, Is.EqualTo(StructureType.InvalidType));
        }
    }
}