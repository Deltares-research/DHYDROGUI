using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.CommonTools.Gui.Property.Functions;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "RetentionProperties_DisplayName")]
    public class RetentionProperties : ObjectProperties<Retention>
    {
        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category("General")]
        [PropertyOrder(2)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(3)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category("Administration")]
        [Description("Channel in which the source is located.")]
        [PropertyOrder(3)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Description("Chainage of the retention in the channel on the map.")]
        [PropertyOrder(4)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Description("Chainage of the retention in the channel as used in the simulation.")]
        [PropertyOrder(5)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }


        [Category("Retention")]
        [Description("Type")]
        [PropertyOrder(6)]
        public RetentionType Type
        {
            get { return data.Type; }
            set { data.Type = value; }
        }

        [Category("Retention")]
        [Description("Storage area (manhole)")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        public double Area
        {
            get { return data.StorageArea; }
            set{data.StorageArea = value;}
        }

        [Category("Retention")]
        [Description("StreetStorageArea")]
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public double StreetStorageArea
        {
            get { return data.StreetStorageArea; }
            set{data.StreetStorageArea = value;}
        }

        [Category("Retention")]
        [Description("Bed level storage reservoir (manhole)")]
        [PropertyOrder(9)]
        [DynamicReadOnly]
        public double BedLevel
        {
            get { return data.BedLevel; }
            set { data.BedLevel = value; }
        }

        [Category("Retention")]
        [Description("Street level")]
        [PropertyOrder(10)]
        [DynamicReadOnly]
        public double StreetLevel
        {
            get { return data.StreetLevel; }
            set { data.StreetLevel = value; }
        }
        
        [Category("Table")]
        [Description("Storage bed definition")]
        [PropertyOrder(11)]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        public Function Storage
        {
            get { return (Function)data.Data; }
            set { data.Data = value; }
        }

        [Category("Table")]
        [Description("Type")]
        [PropertyOrder(12)]
        [DynamicReadOnly]
        public InterpolationType InterpolationType
        {
            get { return data.Data.Arguments[0].InterpolationType; }
            set { data.Data.Arguments[0].InterpolationType = value; }
        }

        [Category("Table")]
        [Description("Use storage as function of level")]
        [PropertyOrder(13)]
        public bool UseTable
        {
            get { return data.UseTable; }
            set { data.UseTable = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsFieldReadOnly(string propertyName)
        {
            if (propertyName == "Area")
            {
                return GetUseTable();
            }

            if (propertyName == "StreetLevel")
            {
                return GetUseTable();
            }

            if (propertyName == "StreetStorageArea")
            {
                return GetUseTable();
            }

            if (propertyName == "BedLevel")
            {
                return GetUseTable();
            }

            if (propertyName == "Storage")
            {
                return !GetUseTable();
            }

            if (propertyName == "InterpolationType")
            {
                return !GetUseTable();
            }

            return true;
        }

        private bool GetUseTable()
        {
            if (!UseTable)
            {
                InterpolationType = InterpolationType.Constant;
            }

            return UseTable;
        }
    }
}