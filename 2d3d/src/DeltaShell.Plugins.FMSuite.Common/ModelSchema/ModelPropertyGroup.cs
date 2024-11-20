using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public class ModelPropertyGroup
    {
        public ModelPropertyGroup(string groupName)
        {
            Name = groupName;
            PropertyDefinitions = new List<ModelPropertyDefinition>();
        }

        public string Name { get; private set; }

        public IList<ModelPropertyDefinition> PropertyDefinitions { get; private set; }

        public void Add(ModelPropertyDefinition propertyDefinition)
        {
            if (!PropertyDefinitions.Contains(propertyDefinition))
            {
                PropertyDefinitions.Add(propertyDefinition);
            }
        }
        
        /// <inheritdoc />
        public override string ToString() => Name;
    }
}