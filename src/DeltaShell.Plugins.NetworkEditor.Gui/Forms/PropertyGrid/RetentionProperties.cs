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
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "RetentionProperties_DisplayName")]
    public class RetentionProperties : ObjectProperties<Retention>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(99)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Channel in which the source is located.")]
        [PropertyOrder(3)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Description("Chainage of the retention in the channel on the map.")]
        [PropertyOrder(4)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Description("Chainage of the retention in the channel as used in the simulation.")]
        [PropertyOrder(5)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }


        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("Type")]
        [PropertyOrder(6)]
        public RetentionType Type
        {
            get { return data.Type; }
            set { data.Type = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("Storage area (manhole).")]
        [DisplayName("Storage area")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        public double Area
        {
            get { return data.StorageArea; }
            set{data.StorageArea = value;}
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("Street storage area.")]
        [DisplayName("Street storage area")]
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public double StreetStorageArea
        {
            get { return data.StreetStorageArea; }
            set{data.StreetStorageArea = value;}
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("Bed level storage reservoir (manhole)")]
        [DisplayName("Bed level")]
        [PropertyOrder(9)]
        [DynamicReadOnly]
        public double BedLevel
        {
            get { return data.BedLevel; }
            set { data.BedLevel = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("Street level.")]
        [DisplayName("Street level")]
        [PropertyOrder(10)]
        [DynamicReadOnly]
        public double StreetLevel
        {
            get { return data.StreetLevel; }
            set { data.StreetLevel = value; }
        }

        [Category(PropertyWindowCategoryHelper.TableCategory)]
        [Description("Storage bed definition.")]
        [PropertyOrder(11)]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        public Function Storage
        {
            get { return (Function)data.Data; }
            set { data.Data = value; }
        }

        [Category(PropertyWindowCategoryHelper.TableCategory)]
        [Description("Type")]
        [PropertyOrder(12)]
        [DynamicReadOnly]
        public InterpolationType InterpolationType
        {
            get { return data.Data.Arguments[0].InterpolationType; }
            set { data.Data.Arguments[0].InterpolationType = value; }
        }

        [Category(PropertyWindowCategoryHelper.TableCategory)]
        [Description("Use storage as function of level.")]
        [DisplayName("Use table")]
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