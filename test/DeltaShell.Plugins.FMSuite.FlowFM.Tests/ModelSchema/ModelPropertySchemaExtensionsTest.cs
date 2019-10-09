using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelSchema
{
    [TestFixture]
    public class ModelPropertySchemaExtensionsTest
    {
        [Test]
        public void AddNewModelDefinitionCategoryIfNotExisting_WithNonExistingCategory_ThenCategoryIsAddedToModelPropertySchema()
        {
            // Setup
            const string categoryName = "myCategory";
            var schema = new ModelPropertySchema<WaterFlowFMPropertyDefinition>();

            // Call
            schema.AddNewModelDefinitionCategoryIfNotExisting(categoryName);

            // Assert
            schema.ModelDefinitionCategory.TryGetValue(categoryName, out ModelPropertyGroup resultingPropertyGroup);
            Assert.That(resultingPropertyGroup, Is.Not.Null);
            Assert.That(resultingPropertyGroup.Name, Is.EqualTo(categoryName));
        }

        [Test]
        public void AddNewModelDefinitionCategoryIfNotExisting_WithExistingCategory_ThenModelDefinitionCategoryRemainsUnchanged()
        {
            // Setup
            const string categoryName = "myCategory";
            const string groupName = "myGroupName";

            var schema = new ModelPropertySchema<WaterFlowFMPropertyDefinition>();
            var initialPropertyGroup = new ModelPropertyGroup(groupName);
            schema.ModelDefinitionCategory.Add(categoryName, initialPropertyGroup);

            // Call
            schema.AddNewModelDefinitionCategoryIfNotExisting(categoryName);

            // Assert
            schema.ModelDefinitionCategory.TryGetValue(categoryName, out ModelPropertyGroup resultingPropertyGroup);
            Assert.That(resultingPropertyGroup, Is.Not.Null);
            Assert.That(resultingPropertyGroup, Is.SameAs(initialPropertyGroup));
        }
    }
}