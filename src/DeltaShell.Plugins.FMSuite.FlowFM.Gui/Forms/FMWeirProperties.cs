using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    [ResourcesDisplayName(typeof(Resources), "WeirProperties_DisplayName")]
    public class FMWeirProperties : ObjectProperties<IWeir>
    {
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.SetNameIfValid(value); }
        }

        [Category("General")]
        [DisplayName("Crest Level")]
        [Description("Level of the weir above datum.")]
        [PropertyOrder(2)]
        [DynamicReadOnly]
        public string CrestLevel
        {
            get
            {
                if (data.IsUsingTimeSeriesForCrestLevel())
                {
                    return "Time series";
                }
                return data.CrestLevel.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                double crestLevel;
                if (double.TryParse(value, out crestLevel))
                {
                    data.CrestLevel = crestLevel;
                }
            }
        }

        [Category("General")]
        [DisplayName("Crest level input")]
        [Description("Use a time series for the crest level or use a time constant value")]
        [PropertyOrder(1)]
        public TimeDependency UseCrestLevelTimeSeries
        {
            get { return data.UseCrestLevelTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant; }
            set { data.UseCrestLevelTimeSeries = value == TimeDependency.TimeDependent; }
        }

        [Category("General")]        
        [DisplayName("Correction coefficient")]
        [Description("Correction coefficient (0-1)")]
        [PropertyOrder(5)]
        public double CorrectionCoefficient
        {
            get
            {
                var formula = data.WeirFormula as SimpleWeirFormula;
                return formula == null ? 0 : formula.CorrectionCoefficient;
            }
            set
            {
                var formula = data.WeirFormula as SimpleWeirFormula;
                if (formula != null)
                {
                    formula.CorrectionCoefficient = value;
                }
            }
        }

        [Category("General")]
        [DisplayName("Crest width")]
        [PropertyOrder(3)]
        [Description("Width (in [m]) of the weir crest")]
        [DynamicReadOnly]
        public double CrestWidth
        {
            get { return data.CrestWidth; }
            set { data.CrestWidth = value; }
        }

        [Category("General")]
        [DisplayName("Use crest width")]
        [PropertyOrder(4)]
        [Description("Use crest width or use weir geometry")]
        public bool UseCrestWidth
        {
            get { return data.CrestWidth > 0; }
            set { data.CrestWidth = (value ? data.Geometry.Length : 0.0); }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadonly(string propertyName)
        {
            if (propertyName == "CrestLevel")
            {
                return data.UseCrestLevelTimeSeries;
            }
            if (propertyName == "CrestWidth")
            {
                return data.CrestWidth <= 0.0;
            }
            return false;
        }
    }
}