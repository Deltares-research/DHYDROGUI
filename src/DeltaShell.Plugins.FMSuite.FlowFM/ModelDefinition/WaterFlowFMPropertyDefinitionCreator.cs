namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public static class WaterFlowFMPropertyDefinitionCreator
    {
        public static WaterFlowFMPropertyDefinition CreateForUnknownProperty(
            string mduGroupName,
            string mduPropertyName,
            string comment,
            PropertySource propertySource = PropertySource.MduFile)
        {
            return new WaterFlowFMPropertyDefinition
            {
                Caption = mduPropertyName,
                MduPropertyName = mduPropertyName,
                FileCategoryName = mduGroupName,
                FilePropertyName = mduPropertyName,
                Category = "Miscellaneous",
                SubCategory = null,
                DataType = typeof(string),
                DefaultValueAsString = string.Empty,
                EnabledDependencies = string.Empty,
                VisibleDependencies = string.Empty,
                Description = comment,
                IsDefinedInSchema = false,
                IsFile = false,
                UnknownPropertySource = propertySource
            };
        }
    }
}