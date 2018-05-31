using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public enum StructureType
    {
        [Description(StructureRegion.StructureTypeName.Pump)] Pump,
        [Description(StructureRegion.StructureTypeName.Gate)] Gate,
        [Description(StructureRegion.StructureTypeName.Weir)] Weir,
        [Description(StructureRegion.StructureTypeName.GeneralStructure)] GeneralStructure,
        [Description(StructureRegion.StructureTypeName.LeveeBreach)] LeveeBreach,
        InvalidType
    }

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
                StructureType = EnumerableExtensions.GetValueFromDescription<StructureType>(type); // TODO: This is also a ModelProperty! Should this refer to the ModelProperty of should we remove that one from Properties?
            }
            catch(ArgumentException e)
            {
                StructureType = StructureType.InvalidType;
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

        public StructureType StructureType { get; private set; }
        public string InvalidStructureType { get; private set; }
        
        public IList<ModelProperty> Properties { get; private set; }

        public ModelProperty GetProperty(string name)
        {
            return Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.ToLower() == name.ToLower());
        }

        public ModelProperty GetProperty(KnownGeneralStructureProperties property)
        {
            return Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.ToLower() == EnumDescriptionAttributeTypeConverter.GetEnumDescription(property).ToLower());
        }
    }
}