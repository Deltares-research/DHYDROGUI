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
            const string sectionName = "mySection";
            const string filePropertyKey = "myPropertyKey";

            var schema = new ModelPropertySchema<WaterFlowFMPropertyDefinition>();
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                FileSectionName = sectionName,
                FilePropertyKey = filePropertyKey
            };

            // Call
            schema.AddPropertyDefinition(propertyDefinition);

            // Assert
            schema.PropertyDefinitions.TryGetValue(filePropertyKey.ToLower(), out WaterFlowFMPropertyDefinition resultingPropertyDefinition);
            Assert.That(resultingPropertyDefinition, Is.Not.Null);
            Assert.That(resultingPropertyDefinition, Is.SameAs(propertyDefinition));

            schema.ModelDefinitionCategory.TryGetValue(sectionName, out ModelPropertyGroup resultingModelPropertyGroup);
            Assert.That(resultingModelPropertyGroup, Is.Not.Null);
            Assert.That(resultingModelPropertyGroup.Name, Is.EqualTo(sectionName));
        }

        [Test]
        public void AddPropertyDefinition_WithExistingCategory_ThenModelDefinitionCategoryRemainsUnchanged()
        {
            // Setup
            const string sectionName = "mySection";

            var schema = new ModelPropertySchema<WaterFlowFMPropertyDefinition>();
            var initialModelPropertyGroup = new ModelPropertyGroup("myModelPropertyGroupName");
            schema.ModelDefinitionCategory.Add(sectionName, initialModelPropertyGroup);

            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                FileSectionName = sectionName,
                FilePropertyKey = "myPropertyName"
            };

            // Call
            schema.AddPropertyDefinition(propertyDefinition);

            // Assert
            schema.ModelDefinitionCategory.TryGetValue(sectionName, out ModelPropertyGroup resultingModelPropertyGroup);
            Assert.That(resultingModelPropertyGroup, Is.Not.Null);
            Assert.That(resultingModelPropertyGroup, Is.SameAs(initialModelPropertyGroup));
        }
    }
}