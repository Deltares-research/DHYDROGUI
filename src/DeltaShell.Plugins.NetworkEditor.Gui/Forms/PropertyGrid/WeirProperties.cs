using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
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
    [ResourcesDisplayName(typeof(Resources), "WeirProperties_DisplayName")]
    public class WeirProperties : ObjectProperties<IWeir>
    {
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [DynamicVisible]
        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Description("Level of the weir above datum.")]
        [Category("General")]
        [DisplayName("Crest Level")]
        [PropertyOrder(3)]
        public double CrestLevel
        {
            get { return data.CrestLevel; }
            set { data.CrestLevel = value; }
        }

        [Description("Indicates whether the weir has a movable gate.")]
        [Category("General")]
        [DisplayName("Has gate")]
        [PropertyOrder(6)]
        public bool Gate
        {
            get { return data.IsGated; }
        }

        [DynamicVisible]
        [Description("Shape of the crest of the shape along the river.")]
        [Category("General")]
        [DisplayName("Crest Shape")]
        [PropertyOrder(7)]
        public string CrestShape
        {
            get { return data.CrestShape.ToString(); }
        }

        [Description("Is flow in negative direction allowed.")]
        [Category("General")]
        [DisplayName("Allow Negative Flow")]
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public bool AllowNegativeFlow
        {
            get { return data.AllowNegativeFlow; }
            set { data.AllowNegativeFlow = value; }
        }

        [Description("Is flow in positive direction allowed.")]
        [Category("General")]
        [DisplayName("Allow Positive Flow")]
        [PropertyOrder(9)]
        [DynamicReadOnly]
        public bool AllowPositiveFlow
        {
            get { return data.AllowPositiveFlow; }
            set { data.AllowPositiveFlow = value; }
        }

        // This works only for the AllowPositive/AllowNegative/Flowdirection Flow properties;

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            return (data.WeirFormula is RiverWeirFormula || data.WeirFormula is GeneralStructureWeirFormula || data.WeirFormula is PierWeirFormula);
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(10)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [DynamicVisible]
        [Description("Channel in which the composite structure is located.")]
        [PropertyOrder(11)]
        [Category("Administration")]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [DynamicVisible]
        [Description("Channel in which the composite structure is located.")]
        [PropertyOrder(12)]
        [Category("Administration")]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [DynamicVisible]
        [Description("Chainage of the weir in the channel on the map.")]
        [PropertyOrder(13)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        [DisplayFormat("0.00")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [DynamicVisible]
        [Description("Chainage of the weir in the channel as used in the simulation.")]
        [PropertyOrder(14)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [DynamicVisible]
        [Description("OffsetY of the weir in the cross section profile.")]
        [Category("Designer")]
        [PropertyOrder(15)]
        public string YOffSet
        {
            get { return string.Format("{0:0.##}", data.OffsetY); }
            //set { weir.OffsetY = double.Parse(value); }
        }

        [Category("General")]
        [DisplayName("Flow Direction")]
        [Description("Direction of the flow.")]
        [PropertyOrder(16)]
        [DynamicReadOnly]
        public FlowDirection FlowDirection
        {
            get { return data.FlowDirection; }
            set { data.FlowDirection = value; }
        }

        [Category("General")]
        [DisplayName("Weir Formula")]
        [Description("Formula used for the weir")]
        [PropertyOrder(17)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WeirFormulaProperties WeirFormula
        {
            get
            {
                var formula = data.WeirFormula;

                if (formula is SimpleWeirFormula)
                {
                    return new SimpleWeirFormulaProperties(formula as SimpleWeirFormula, data);
                }

                if (formula is RiverWeirFormula)
                {
                    return new RiverWeirFormulaProperties(formula as RiverWeirFormula, data);
                }

                if (formula is PierWeirFormula)
                {
                    return new PierWeirFormulaProperties(formula as PierWeirFormula, data);
                }

                if (formula is GatedWeirFormula)
                {
                    return new GatedWeirFormulaProperties(formula as GatedWeirFormula, data);
                }

                if (formula is GeneralStructureWeirFormula)
                {
                    return new GeneralStructureWeirFormulaProperties(formula as GeneralStructureWeirFormula, data);
                }

                if (formula is FreeFormWeirFormula)
                {
                    return new FreeFormWeirFormulaProperties(formula as FreeFormWeirFormula, data);
                }

                return null;
            }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            if (propertyName == "Channel" || propertyName == "CompositeStructure" ||
                propertyName == "Chainage" || propertyName == "CompuChainage" ||
                propertyName == "LongName" || propertyName == "CrestShape" || propertyName == "YOffSet")
            {
                return data.Branch != null;
            }
            
            return true;
        }
    }
}