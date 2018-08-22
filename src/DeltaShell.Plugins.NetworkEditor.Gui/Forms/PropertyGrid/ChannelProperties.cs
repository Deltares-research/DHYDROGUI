using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.FromType;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.CommonTools.Gui.Property.Functions;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ChannelProperties_DisplayName")]
    public class ChannelProperties : ObjectProperties<IChannel>
    {
        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(7)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category("General")]
        [PropertyOrder(2)]
        [DisplayName("From node")]
        public string FromNode
        {
            get { return data.Source.ToString(); }
        }

        [Category("General")]
        [PropertyOrder(3)]
        [DisplayName("To node")]
        public string ToNode
        {
            get { return data.Target.ToString(); }
        }

        [Category("General")]
        [PropertyOrder(4)]
        [Description("Length used for simulation when IsLengthCustom is true")]
        [DynamicReadOnly]
        public string Length
        {
            get { return string.Format("{0:0.##}", data.Length); }
            set
            {
                double result;
                if (double.TryParse(value, out result))
                {
                    data.Length = result;
                }
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "Length")
            {
                return !IsLengthCustom;
            }

            return true;
        }

        [Category("General")]
        [PropertyOrder(5)]
        [Description("Length of the channel on the map.")]
        [DisplayName("Geometry length")]
        public string GeometryLength
        {
            get { return string.Format("{0:0.##}", data.Geometry.Length); }
        }

        [Category("General")]
        [PropertyOrder(12)]
        [Description("Length of an ellopsoid channel on the map.")]
        [DisplayName("Geodetic length")]
        [DynamicVisible]
        public string GeodeticLength
        {
            get { return string.Format("{0:0.##}", data.GeodeticLength); }
        }
        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            if (propertyName == nameof(data.GeodeticLength) )
            {
                return !double.IsNaN(data.GeodeticLength); 
            }
            return true;
        }

        [Category("General")]
        [PropertyOrder(6)]
        [Description("Length of the channel on the map is ignored for simulation.")]
        [DisplayName("Is custom length")]
        public bool IsLengthCustom
        {
            get { return data.IsLengthCustom; }
            set { data.IsLengthCustom = value; }
        }

        [Category("General")]
        [PropertyOrder(7)]
        [DisplayName("Order number")]
        [Description("Order number will be used for interpolation over branches. A chain of branches with the same order number will be treated as one.")]
        public int OrderNumber
        {
            get { return data.OrderNumber; }
            set { data.OrderNumber = value; }
        }

        [Category("Branch Features")]
        [PropertyOrder(1)]
        [DisplayName("Number of cross-sections")]
        public int CrossSections
        {
            get { return data.CrossSections.Count(); }
        }

        [Category("Branch Features")]
        [PropertyOrder(2)]
        [DisplayName("Number of structures")]
        public int Structures
        {
            get { return data.Structures.Count(s => s.ParentStructure == null); }
        }

        [Category("Branch Features")]
        [PropertyOrder(3)]
        [DisplayName("Number of pumps")]
        public int Pumps
        {
            get { return data.Pumps.Count(); }
        }

        [Category("Branch Features")]
        [PropertyOrder(4)]
        [DisplayName("Number of culverts")]
        public int Culverts
        {
            get { return data.Culverts.Count(); }
        }

        [Category("Branch Features")]
        [PropertyOrder(5)]
        [DisplayName("Number of bridges")]
        public int Bridges
        {
            get { return data.Bridges.Count(); }
        }

        [Category("Branch Features")]
        [PropertyOrder(6)]
        [DisplayName("Number of weirs")]
        public int Weirs
        {
            get { return data.Weirs.Count(); }
        }

        [Category("Branch Features")]
        [PropertyOrder(7)]
        [DisplayName("Number of gates")]
        public int Gates
        {
            get { return data.Gates.Count(); }
        }

        [Category("Branch Features")]
        [PropertyOrder(8)]
        [DisplayName("Number of lateral sources")]
        public int BranchSources
        {
            get { return data.BranchSources.Count(); }
        }
    }
}