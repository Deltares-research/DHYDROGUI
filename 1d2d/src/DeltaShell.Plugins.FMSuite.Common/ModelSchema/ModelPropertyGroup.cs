using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public class ModelPropertyGroup
    {
        public string Name { get; private set; }

        public IList<ModelPropertyDefinition> PropertyDefinitions { get; private set; }

        public ModelPropertyGroup(string groupName)
        {
            Name = groupName;
            PropertyDefinitions = new List<ModelPropertyDefinition>();
        }

        public void AddPropertyDefinition(ModelPropertyDefinition propertyDefinition)
        {
            if (!PropertyDefinitions.Contains(propertyDefinition))
            {
                PropertyDefinitions.Add(propertyDefinition);
            }
        }
    }
}