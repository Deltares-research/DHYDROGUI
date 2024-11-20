using System;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    [Serializable]
    public class PropertyMapping
    {
        public PropertyMapping(): this("PropertyMapping")
        {
        }

        public PropertyMapping(string propertyName):this(propertyName,false,false)
        {
        }

        public PropertyMapping(string propertyName, bool isUnique, bool isRequired)
        {
            PropertyName = propertyName;
            IsUnique = isUnique;
            IsRequired = isRequired;
            MappingColumn = new MappingColumn();
        }

        public string PropertyName { get; set;}
        public string PropertyUnit { get; set; }
        public bool IsUnique { get; set; }
        public bool IsRequired { get; set; }
        public MappingColumn MappingColumn { get; set; }

        public override string ToString()
        {
            if (MappingColumn!=null)
            {
                return MappingColumn.ToString();
            }
            return "";
        }
    }
}