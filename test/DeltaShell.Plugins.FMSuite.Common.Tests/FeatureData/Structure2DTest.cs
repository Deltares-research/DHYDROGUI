using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    [TestFixture]
    public class Structure2DTest
    {
        [Test]
        [TestCase("Pump", StructureType.Pump)]
        [TestCase("Gate", StructureType.Gate)]
        [TestCase("GeneralStructure", StructureType.GeneralStructure)]
        [TestCase("Weir", StructureType.Weir)]
        public void GivenAStructure2DWithACorrectName_WhenCallingConstructor_StructureIsCreated(string structureName, StructureType type)
        {
            var structure = new Structure2D(structureName);
            Assert.That(structure.StructureType, Is.EqualTo(type));
        }

        [Test]
        public void GetPropertyTest()
        {
            var structure = new Structure2D("test");
            Assert.IsNull(structure.GetProperty("name"));

            structure.AddProperty("name", typeof(string), "value");

            var property = structure.GetProperty("NaMe");
            Assert.IsNotNull(property, "Case insensitivity expected.");
            Assert.AreEqual(structure.Properties.ElementAt(0), property);
        }

        [Test]
        public void GivenStructure2DWhenInstantiatingWithEmptyStringThenStructureHasInvalidType()
        {
            var structure = new Structure2D("");
            Assert.That(structure.StructureType, Is.EqualTo(StructureType.InvalidType));
        }

        [Test]
        public void WhenInstantiatingStructure2DWithNullValueThenStructureHasInvalidType()
        {
            var structure = new Structure2D(null);
            Assert.That(structure.StructureType, Is.EqualTo(StructureType.InvalidType));
        }
    }
}