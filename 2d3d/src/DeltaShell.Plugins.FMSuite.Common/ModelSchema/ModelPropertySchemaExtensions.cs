namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public static class ModelPropertySchemaExtensions
    {
        public static void AddPropertyDefinition<TDef>(this ModelPropertySchema<TDef> schema, TDef propertyDefinition)
            where TDef : ModelPropertyDefinition, new()
        {
            schema.PropertyDefinitions.Add(propertyDefinition.FilePropertyKey.ToLower(), propertyDefinition);

            string fileSectionName = propertyDefinition.FileSectionName;
            schema.AddNewModelDefinitionCategoryIfNotExisting(fileSectionName);
            schema.ModelDefinitionCategory[fileSectionName].Add(propertyDefinition);
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