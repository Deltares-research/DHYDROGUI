namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public static class ModelPropertySchemaExtensions
    {
        public static void AddPropertyDefinition<TDef>(this ModelPropertySchema<TDef> schema, TDef propertyDefinition)
            where TDef : ModelPropertyDefinition, new()
        {
            schema.PropertyDefinitions.Add(propertyDefinition.FilePropertyName.ToLower(), propertyDefinition);

            string fileCategoryName = propertyDefinition.FileCategoryName;
            schema.AddNewModelDefinitionCategoryIfNotExisting(fileCategoryName);
            schema.ModelDefinitionCategory[fileCategoryName].Add(propertyDefinition);
        }

        private static void AddNewModelDefinitionCategoryIfNotExisting<TDef>(this ModelPropertySchema<TDef> schema, string categoryName)
            where TDef : ModelPropertyDefinition, new()
        {
            if (!schema.ModelDefinitionCategory.ContainsKey(categoryName))
            {
                schema.ModelDefinitionCategory.Add(categoryName, new ModelPropertyGroup(categoryName));
            }
        }
    }
}