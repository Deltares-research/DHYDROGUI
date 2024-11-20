using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public enum StructureType
    {
        [Description(StructureRegion.StructureTypeName.Pump)]
        Pump,

        [Description(StructureRegion.StructureTypeName.Gate)]
        Gate,

        [Description(StructureRegion.StructureTypeName.Weir)]
        Weir,

        [Description(StructureRegion.StructureTypeName.GeneralStructure)]
        GeneralStructure,
        InvalidType
    }

    public class StructureDAO
    {
        public StructureDAO(string type)
        {
            SetStructureType(type);
            Properties = new List<ModelProperty>();
        }

        // Might be risky: assumes that KnownStructureProperties.Name is always available.
        public string Name => GetProperty(KnownStructureProperties.Name).GetValueAsString();

        public StructureType StructureType { get; private set; }
        public string InvalidStructureType { get; private set; }

        public IList<ModelProperty> Properties { get; private set; }

        public ModelProperty GetProperty(string name)
        {
            return Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyKey.ToLower() == name.ToLower());
        }

        public ModelProperty GetProperty(KnownGeneralStructureProperties property)
        {
            return GetProperty(property.GetDescription());
        }

        private void SetStructureType(string type)
        {
            try
            {
                StructureType = (StructureType)Enum.Parse(typeof(StructureType), type, true);
            }
            catch (ArgumentException)
            {
                StructureType = StructureType.InvalidType;
                InvalidStructureType = type;
            }
        }
    }
}