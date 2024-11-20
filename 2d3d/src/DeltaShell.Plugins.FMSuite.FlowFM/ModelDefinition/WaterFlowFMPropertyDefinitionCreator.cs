namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    /// <summary>
    /// Creator for instances of <see cref="WaterFlowFMPropertyDefinition"/>.
    /// </summary>
    public static class WaterFlowFMPropertyDefinitionCreator
    {
        /// <summary>
        /// Creates an instance of <see cref="WaterFlowFMPropertyDefinition"/> intended for custom properties
        /// that are defined in ini files.
        /// </summary>
        /// <param name="mduCategoryName"> The category name of the property. </param>
        /// <param name="mduPropertyName"> The property name. </param>
        /// <param name="description"> The description for this property. </param>
        /// <param name="propertySource"> The file source for this property. </param>
        /// <returns> An instance of <see cref="WaterFlowFMPropertyDefinition"/>. </returns>
        public static WaterFlowFMPropertyDefinition CreateForCustomProperty(
            string mduCategoryName,
            string mduPropertyName,
            string description,
            PropertySource propertySource = PropertySource.MduFile)
        {
            return new WaterFlowFMPropertyDefinition
            {
                Caption = mduPropertyName,
                MduPropertyName = mduPropertyName,
                FileSectionName = mduCategoryName,
                FilePropertyKey = mduPropertyName,
                Category = "Miscellaneous",
                SubCategory = null,
                DataType = typeof(string),
                DefaultValueAsString = string.Empty,
                EnabledDependencies = string.Empty,
                VisibleDependencies = string.Empty,
                Description = description,
                IsDefinedInSchema = false,
                IsFile = false,
                UnknownPropertySource = propertySource
            };
        }
    }
}