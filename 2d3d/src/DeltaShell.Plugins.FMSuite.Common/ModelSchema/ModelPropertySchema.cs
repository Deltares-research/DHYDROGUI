using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public class ModelPropertySchema<TDefinition> where TDefinition : ModelPropertyDefinition
    {
        public ModelPropertySchema()
        {
            PropertyDefinitions = new Dictionary<string, TDefinition>();
            GuiPropertyGroups = new Dictionary<string, ModelPropertyGroup>();
            ModelDefinitionCategory = new Dictionary<string, ModelPropertyGroup>();
        }

        public IDictionary<string, ModelPropertyGroup> GuiPropertyGroups { get; set; }
        public IDictionary<string, ModelPropertyGroup> ModelDefinitionCategory { get; set; }
        public IDictionary<string, TDefinition> PropertyDefinitions { get; set; }
    }
}