using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "PumpProperties_DisplayName")]
    public class PumpProperties : ObjectProperties<IPump>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [DynamicVisible]
        [DisplayName("Long name")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName= value; }
        }

        [DynamicVisible]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(3)]
        [Description("Use a time series for the pump capacity or use a time constant value.")]
        [DisplayName("Capacity input")]
        public TimeDependency UseCapacityTimeSeries
        {
            get { return data.UseCapacityTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant; }
            set { data.UseCapacityTimeSeries = value == TimeDependency.TimeDependent; }
        }

        [DynamicReadOnly]
        [Description("Capacity of the pump.")]
        [DisplayName("Capacity")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(4)]
        public string Capacity
        {
            get
            {
                if (data.CanBeTimedependent && data.UseCapacityTimeSeries)
                {
                    return "Time series";
                }
                return data.Capacity.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                if (data.CanBeTimedependent && data.UseCapacityTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value using time dependent pump capacity.");
                }
                data.Capacity = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DynamicVisible]
        [Description("Start level delivery.")]
        [DisplayName("Start delivery")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(5)]
        public double StartDelivery
        {
            get { return data.StartDelivery; }
            set { data.StartDelivery = value; }
        }

        [DynamicVisible]
        [Description("Stop level delivery.")]
        [DisplayName("Stop delivery")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(6)]
        public double StopDelivery
        {
            get { return data.StopDelivery; }
            set { data.StopDelivery = value; }
        }

        [DynamicVisible]
        [Description("Start level suction.")]
        [DisplayName("Start suction")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(7)]
        public double StartSuction
        {
            get { return data.StartSuction; }
            set { data.StartSuction = value; }
        }

        [DynamicVisible]
        [Description("Stop level suction.")]
        [DisplayName("Stop suction")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(8)]
        public double StopSuction
        {
            get { return data.StopSuction; }
            set { data.StopSuction = value; }
        }

        [DynamicVisible]
        [DisplayName("Attributes")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(99)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [DynamicVisible]
        [Description("Channel in which the compound structure is located.")]
        [PropertyOrder(10)]
        [DisplayName("Branch")]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        public string Channel
        {
            get { return data.Branch != null ? data.Branch.ToString() : ""; }
        }

        [DynamicVisible]
        [Description("Channel in which the compound structure is located.")]
        [PropertyOrder(11)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        public string CompositeStructure
        {
            get { return data.ParentStructure != null ? data.ParentStructure.ToString() : ""; }
        }

        [DynamicVisible]
        [Description("Chainage of the pump in the channel on the map.")]
        [PropertyOrder(12)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        public double Chainage
        {
            get { return data.ParentStructure != null ? NetworkHelper.MapChainage(data.ParentStructure) : double.NaN; }
        }

        [DynamicVisible]
        [Description("Chainage of the pump in the channel as used in the simulation.")]
        [PropertyOrder(13)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.ParentStructure != null ? data.ParentStructure.Chainage : double.NaN; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [DynamicVisible]
        [Description("Y offset of the pump in the cross section profile.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Y offset")]
        [PropertyOrder(30)]
        public string YOffSet
        {
            get { return string.Format("{0:0.##}", data.OffsetY); }
            set { data.OffsetY = double.Parse(value); }
        }

        [DynamicVisible]
        [Description("Z offset of the pump in the cross section profile.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Z offset")]
        [PropertyOrder(31)]
        public string ZOffSet
        {
            get { return string.Format("{0:0.##}", data.OffsetZ); }
        }

        [DynamicVisible]
        [Description("Direction in which the pump pumps. Positive is along the branch.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Pump direction")]
        [PropertyOrder(16)]
        public PumpDirection PumpDirection
        {
            get { return data.DirectionIsPositive ? PumpDirection.Positive : PumpDirection.Negative; }
            set { data.DirectionIsPositive = value == PumpDirection.Positive; }
        }

        [DynamicVisible]
        [Description("Direction in which the pump control acts.")]
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Control direction")]
        [PropertyOrder(17)]
        public PumpControlDirection ControlDirection
        {
            get { return data.ControlDirection; }
            set { data.ControlDirection = value; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            if (propertyName == "Channel" || propertyName == "CompositeStructure" ||
                propertyName == "Chainage" || propertyName == "CompuChainage" ||
                propertyName == "LongName" || propertyName == "YOffSet" || propertyName == "ZOffSet" ||
                propertyName == "PumpDirection" || propertyName == "Attributes")
            {
                return data.Branch != null; //exclude FM pumps
            }
            if (propertyName == "StartDelivery" || propertyName == "StopDelivery" ||
                propertyName == "StartSuction" || propertyName == "StopSuction" || propertyName == "ControlDirection")
            {
                return data.Branch != null; //exclude FM pumps until we support it...
            }
            if (propertyName == "UseCapacityTimeSeries")
            {
                return data.CanBeTimedependent;
            }
            return true;
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == "Capacity")
            {
                return data.CanBeTimedependent && data.UseCapacityTimeSeries;
            }
            return false;
        }
    }

    public enum PumpDirection
    {
        Positive,
        Negative
    }
}