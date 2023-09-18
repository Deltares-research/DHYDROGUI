using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureSchemaTest
    {
        [Test]
        public void GetDefinitionTest()
        {
            var testSchema = CreateTestStructureSchema();

            var definition = testSchema.GetDefinition("weir", "crestlevel");
            Assert.IsNotNull(definition);
            Assert.AreEqual(definition, testSchema.StructurePropertyGroups["weir"].PropertyDefinitions.First());

            definition = testSchema.GetDefinition("weir", "id");
            Assert.IsNotNull(definition);
            Assert.AreEqual(definition, testSchema.StructurePropertyGroups["structure"].PropertyDefinitions.First(),
                "Should also match to properties in 'structure' group.");

            Assert.IsNull(testSchema.GetDefinition("non-existend structure type", "id"));
            Assert.IsNull(testSchema.GetDefinition("weir", "non-existent property name"));
        }

        private static StructureSchema<ModelPropertyDefinition> CreateTestStructureSchema()
        {
            var schema = new StructureSchema<ModelPropertyDefinition>();
            schema.StructurePropertyGroups.Add("weir", new ModelPropertyGroup("weir"));
            schema.StructurePropertyGroups.Add("structure", new ModelPropertyGroup("structure"));

            schema.StructurePropertyGroups["weir"].AddPropertyDefinition(new StructurePropertyDefinition
                {
                    FilePropertyKey = "crestlevel"
                });

            schema.StructurePropertyGroups["structure"].AddPropertyDefinition(new StructurePropertyDefinition
            {
                FilePropertyKey = "id"
            });

            return schema;
        }
    }
}