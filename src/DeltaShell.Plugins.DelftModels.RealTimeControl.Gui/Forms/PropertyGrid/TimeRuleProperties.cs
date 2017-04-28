using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "TimeRuleProperties_DisplayName")]
    public class TimeRuleProperties : ObjectProperties<TimeRule>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_Name_Description")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_LongName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_LongName_Description")]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        public InterpolationType Interpolation
        {
            get { return data.InterpolationOptionsTime; }
            set { data.InterpolationOptionsTime = value; }
        }

        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "TimeRuleProperties_Periodicity_DisplayName")]
        [ResourcesDescription(typeof(Resources), "TimeRuleProperties_Periodicity_Description")]
        public ExtrapolationTimeSeriesType Periodicity
        {
            get { return (ExtrapolationTimeSeriesType) data.Periodicity; }
            set { data.Periodicity = (ExtrapolationType) value; }
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Timeseries_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_TimeSeries_Description")]
        public TimeSeries TimeSeries
        {
            get { return data.TimeSeries; }
            set { data.TimeSeries = value; }
        }
    }
}
