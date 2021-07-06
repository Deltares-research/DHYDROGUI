using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WeirProperties_DisplayName")]
    public class WeirProperties : ObjectProperties<IWeir>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [DynamicVisible]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Description("Level of the weir above datum.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Crest level")]
        [PropertyOrder(3)]
        public double CrestLevel
        {
            get { return data.CrestLevel; }
           
            
            set { data.CrestLevel = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Crest width")]
        [PropertyOrder(3)]
        public double CrestWidth
        {
            get { return data.CrestWidth; }


            set { data.CrestWidth = value; }
        }

        [Description("Indicates whether the weir has a movable gate.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Has gate")]
        [PropertyOrder(6)]
        public bool Gate
        {
            get { return data.IsGated; }
        }

        [DynamicVisible]
        [Description("Shape of the crest of the shape along the river.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Crest shape")]
        [PropertyOrder(7)]
        public string CrestShape
        {
            get { return data.CrestShape.ToString(); }
        }

        [Description("Is flow in negative direction allowed.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Allow negative flow")]
        [PropertyOrder(8)]
        public bool AllowNegativeFlow
        {
            get { return data.AllowNegativeFlow; }
            set { data.AllowNegativeFlow = value; }
        }

        [Description("Is flow in positive direction allowed.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Allow positive flow")]
        [PropertyOrder(9)]
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

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(99)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [DynamicVisible]
        [Description("Channel in which the compound structure is located.")]
        [DisplayName("Branch")]
        [PropertyOrder(11)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [DynamicVisible]
        [Description("Channel in which the compound structure is located.")]
        [DisplayName("Compound structure")]
        [PropertyOrder(12)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [DynamicVisible]
        [Description("Chainage of the weir in the channel on the map.")]
        [PropertyOrder(13)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [DisplayFormat("0.00")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [DynamicVisible]
        [Description("Chainage of the weir in the channel as used in the simulation.")]
        [PropertyOrder(14)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [DynamicVisible]
        [DisplayName("Y Offset")]
        [Description("OffsetY of the weir in the cross section profile.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(30)]
        public string YOffSet
        {
            get { return string.Format("{0:0.##}", data.OffsetY); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Flow direction")]
        [Description("Direction of the flow.")]
        [PropertyOrder(16)]
        public FlowDirection FlowDirection
        {
            get { return data.FlowDirection; }
            set { data.FlowDirection = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Weir formula")]
        [Description("Formula used for the weir.")]
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