using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid
{
    [ExcludeFromCodeCoverage]
    [ResourcesDisplayName(typeof(Resources), "TimeRuleProperties_DisplayName")]
    public class TimeRuleProperties : ObjectProperties<TimeRule>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_Name_Description")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_LongName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_LongName_Description")]
        [PropertyOrder(2)]
        public string LongName
        {
            get => data.LongName;
            set => data.LongName = value;
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Timeseries_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_TimeSeries_Description")]
        [PropertyOrder(3)]
        public TimeSeries TimeSeries
        {
            get => data.TimeSeries;
            set => data.TimeSeries = value;
        }

        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        [PropertyOrder(4)]
        public InterpolationHydraulicType Interpolation
        {
            get => (InterpolationHydraulicType)data.InterpolationOptionsTime;
            set => data.InterpolationOptionsTime = (InterpolationType)value;
        }

        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Time_Extrapolation_Description")]
        [PropertyOrder(5)]
        public ExtrapolationTimeSeriesType Periodicity
        {
            get => (ExtrapolationTimeSeriesType) data.Periodicity;
            set => data.Periodicity = (ExtrapolationType) value;
        }
        
    }
}
