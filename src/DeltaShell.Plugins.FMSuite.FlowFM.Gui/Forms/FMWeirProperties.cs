using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    [ResourcesDisplayName(typeof(Resources), "WeirProperties_DisplayName")]
    public class FMWeirProperties : ObjectProperties<IStructure>
    {
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
            }
        }

        /// <summary>
        /// Property for changing/showing the weir formula by using the properties box.
        /// </summary>
        [Category("General")]
        [DisplayName("Structure Type")]
        [Description("Structure Type")]
        [PropertyOrder(1)]
        public SelectableWeirFormulaType WeirFormula
        {
            get
            {
                switch (data.Formula)
                {
                    case SimpleGateFormula _:
                        return SelectableWeirFormulaType.SimpleGate;
                    case GeneralStructureFormula _:
                        return SelectableWeirFormulaType.GeneralStructure;
                    default:
                        return SelectableWeirFormulaType.SimpleWeir;
                }
            }
            set
            {
                switch (value)
                {
                    case SelectableWeirFormulaType.SimpleWeir:
                        data.Formula = new SimpleWeirFormula();
                        break;
                    case SelectableWeirFormulaType.SimpleGate:
                        data.Formula = new SimpleGateFormula(true);
                        break;
                    case SelectableWeirFormulaType.GeneralStructure:
                        var generalStructureWeirFormula = new GeneralStructureFormula()
                        {
                            CrestLevel = data.CrestLevel,
                            CrestWidth = data.CrestWidth,
                            Upstream2Width = double.NaN,
                            Downstream1Width = double.NaN,
                            Upstream1Width = double.NaN,
                            Downstream2Width = double.NaN
                        };
                        data.Formula = generalStructureWeirFormula;
                        break;
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
            get => data.UseCrestLevelTimeSeries 
                       ? "Time series" 
                       : data.CrestLevel.ToString(CultureInfo.CurrentCulture);
            set
            {
                if (double.TryParse(value, out double crestLevel))
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
            get => data.UseCrestLevelTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant;
            set => data.UseCrestLevelTimeSeries = value == TimeDependency.TimeDependent;
        }

        /// <summary>
        /// Gets or sets the crest width to display in the FMWeirProperties.
        /// </summary>
        /// <value>
        /// The crest width.
        /// </value>
        /// <remarks>
        /// data.CrestWidth is not nullable, as such, null is mapped to NaN
        /// in order to obtain an empty field.
        /// </remarks>
        [Category("General")]
        [DisplayName(GuiParameterNames.CrestWidth)]
        [PropertyOrder(4)]
        [Description("Width (in [m]) of the weir crest")]
        [DynamicReadOnly]
        public double? CrestWidth
        {
            get => (!double.IsNaN(data.CrestWidth)) ? (double?) data.CrestWidth : null;
            set => data.CrestWidth = value ?? double.NaN;
        }

        [Category("General")]
        [DisplayName("Use crest width")]
        [PropertyOrder(5)]
        [Description("If true then use the crest width value else calculate with the weir geometry")]
        public bool UseCrestWidth
        {
            get => data.CrestWidth > 0;
            set => data.CrestWidth = value ? data.Geometry.Length : double.NaN;
        }

        /// <summary>
        /// Determines whether the specified property name is readonly.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// <c>true</c> if the specified property name is readonly; otherwise, <c>false</c>.
        /// </returns>
        [DynamicReadOnlyValidationMethod]
        public bool IsReadonly(string propertyName)
        {
            switch (propertyName)
            {
                case "CrestLevel":
                    return data.UseCrestLevelTimeSeries;
                case "CrestWidth":
                    return double.IsNaN(data.CrestWidth) || data.CrestWidth <= 0.0;
                default:
                    return false;
            }
        }
    }
}