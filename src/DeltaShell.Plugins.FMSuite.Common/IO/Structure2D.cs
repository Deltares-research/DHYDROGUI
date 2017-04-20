using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class Structure2D
    {
        public Structure2D(string type)
        {
            StructureType = type; // TODO: This is also a ModelProperty! Should this refer to the ModelProperty of should we remove that one from Properties?
            Properties = new List<ModelProperty>();
        }

        // Might be risky: assumes that KnownStructureProperties.Name is always available.
        public string Name
        {
            get
            {
                return GetProperty(KnownStructureProperties.Name).GetValueAsString();
            }
        }

        public string StructureType { get; private set; }
        
        public IList<ModelProperty> Properties { get; private set; }

        public ModelProperty GetProperty(string name)
        {
            return Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName == name.ToLower());
        }
    }
}