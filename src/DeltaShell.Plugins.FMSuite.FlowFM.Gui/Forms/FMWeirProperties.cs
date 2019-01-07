using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;

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
            set { data.Name = value; }
        }

        [Category("General")]
        [DisplayName("Structure Type")]
        [Description("Structure Type")]
        [PropertyOrder(1)]
        public SelectableWeirFormulaType WeirFormula
        {
            get
            {
                if (data.WeirFormula is SimpleWeirFormula)
                {
                    return SelectableWeirFormulaType.SimpleWeir;
                }
                else if (data.WeirFormula is GatedWeirFormula)
                {
                    return SelectableWeirFormulaType.SimpleGate;
                }
                else 
                {
                    return SelectableWeirFormulaType.GeneralStructure;
                }
            }
            set
            {
                if (value == SelectableWeirFormulaType.SimpleWeir)
                {
                    data.WeirFormula = new SimpleWeirFormula();
                }
                else if (value == SelectableWeirFormulaType.SimpleGate)
                {
                    data.WeirFormula = new GatedWeirFormula(true);
                }
                else
                {
                    var generalStructureWeirFormula = new GeneralStructureWeirFormula()
                    {
                        BedLevelStructureCentre = data.CrestLevel,
                        WidthStructureCentre = data.CrestWidth,
                    };

                    data.WeirFormula = generalStructureWeirFormula;
                }
            }
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
                if (data.CanBeTimedependent && data.UseCrestLevelTimeSeries)
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
        [PropertyOrder(3)]
        public TimeDependency UseCrestLevelTimeSeries
        {
            get { return data.UseCrestLevelTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant; }
            set { data.UseCrestLevelTimeSeries = value == TimeDependency.TimeDependent; }
        }

        

        [Category("General")]
        [DisplayName("Crest width")]
        [PropertyOrder(4)]
        [Description("Width (in [m]) of the weir crest")]
        [DynamicReadOnly]
        public double CrestWidth
        {
            get { return data.CrestWidth; }
            set { data.CrestWidth = value; }
        }

        [Category("General")]
        [DisplayName("Use crest width")]
        [PropertyOrder(5)]
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