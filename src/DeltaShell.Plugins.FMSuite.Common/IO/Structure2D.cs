using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class Structure2D
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Structure2D));

        public Structure2D(string type)
        {
            SetStructureType(type);
            Properties = new List<ModelProperty>();
        }

        private void SetStructureType(string type)
        {
            try
            {
                Structure2DType =  (Structure2DType)typeof(Structure2DType).GetEnumValueFromDescription(type); // TODO: This is also a ModelProperty! Should this refer to the ModelProperty of should we remove that one from Properties?
            }
            catch(ArgumentException e)
            {
                Structure2DType = Structure2DType.InvalidType;
                InvalidStructureType = type;
            }
        }

        // Might be risky: assumes that KnownStructureProperties.Name is always available.
        public string Name
        {
            get
            {
                return GetProperty(KnownStructureProperties.Name).GetValueAsString();
            }
        }

        public Structure2DType Structure2DType { get; private set; }
        public string InvalidStructureType { get; private set; }
        
        public IList<ModelProperty> Properties { get; private set; }

        public ModelProperty GetProperty(string name)
        {
            return Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.ToLower() == name.ToLower());
        }

        public ModelProperty GetProperty(KnownGeneralStructureProperties property)
        {
            return Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.ToLower() ==  property.GetDescription().ToLower());
        }
    }
}