using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelSchema
{
    [TestFixture]
    public class ModelPropertySchemaExtensionsTest
    {
        [Test]
        public void AddPropertyDefinition_WithNonExistingCategory_ThenPropertyDefinitionIsAddedToModelPropertySchemaAndNewModelPropertyGroupIsAdded()
        {
            // Setup
            const string categoryName = "myCategory";
            const string filePropertyName = "myPropertyName";

            var schema = new ModelPropertySchema<WaterFlowFMPropertyDefinition>();
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                FileCategoryName = categoryName,
                FilePropertyName = filePropertyName
            };

            // Call
            schema.AddPropertyDefinition(propertyDefinition);

            // Assert
            schema.PropertyDefinitions.TryGetValue(filePropertyName.ToLower(), out WaterFlowFMPropertyDefinition resultingPropertyDefinition);
            Assert.That(resultingPropertyDefinition, Is.Not.Null);
            Assert.That(resultingPropertyDefinition, Is.SameAs(propertyDefinition));

            schema.ModelDefinitionCategory.TryGetValue(categoryName, out ModelPropertyGroup resultingModelPropertyGroup);
            Assert.That(resultingModelPropertyGroup, Is.Not.Null);
            Assert.That(resultingModelPropertyGroup.Name, Is.EqualTo(categoryName));
        }

        [Test]
        public void AddPropertyDefinition_WithExistingCategory_ThenModelDefinitionCategoryRemainsUnchanged()
        {
            // Setup
            const string categoryName = "myCategory";

            var schema = new ModelPropertySchema<WaterFlowFMPropertyDefinition>();
            var initialModelPropertyGroup = new ModelPropertyGroup("myModelPropertyGroupName");
            schema.ModelDefinitionCategory.Add(categoryName, initialModelPropertyGroup);

            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                FileCategoryName = categoryName,
                FilePropertyName = "myPropertyName"
            };

            // Call
            schema.AddPropertyDefinition(propertyDefinition);

            // Assert
            schema.ModelDefinitionCategory.TryGetValue(categoryName, out ModelPropertyGroup resultingModelPropertyGroup);
            Assert.That(resultingModelPropertyGroup, Is.Not.Null);
            Assert.That(resultingModelPropertyGroup, Is.SameAs(initialModelPropertyGroup));
        }
    }
}