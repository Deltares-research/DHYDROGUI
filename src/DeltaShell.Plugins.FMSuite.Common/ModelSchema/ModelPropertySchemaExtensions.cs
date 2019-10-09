namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public static class ModelPropertySchemaExtensions
    {
        public static void AddNewModelDefinitionCategoryIfNotExisting<TDef>(this ModelPropertySchema<TDef> schema, string categoryName)
            where TDef : ModelPropertyDefinition, new()
        {
            if (!schema.ModelDefinitionCategory.ContainsKey(categoryName))
            {
                schema.ModelDefinitionCategory.Add(categoryName, new ModelPropertyGroup(categoryName));
            }
        }
    }
}