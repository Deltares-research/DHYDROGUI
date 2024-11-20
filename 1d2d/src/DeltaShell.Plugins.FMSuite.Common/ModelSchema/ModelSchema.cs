using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public class ModelSchema<TDefinition> where TDefinition : ModelPropertyDefinition
    {
        public ModelSchema()
        {
            PropertyDefinitions = new Dictionary<string, TDefinition>();
            GuiPropertyGroups = new Dictionary<string, ModelPropertyGroup>();
            ModelDefinitionCategory = new Dictionary<string, ModelPropertyGroup>();
        }
        
        public Dictionary<string, ModelPropertyGroup> GuiPropertyGroups { get; set; }
        public IDictionary<string, ModelPropertyGroup> ModelDefinitionCategory { get; set; }
        public IDictionary<string, TDefinition> PropertyDefinitions { get; set; }
    }
}