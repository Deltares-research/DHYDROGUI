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
                if (data.WeirFormula is GatedWeirFormula)
                {
                    return SelectableWeirFormulaType.SimpleGate;
                }

                if (data.WeirFormula is GeneralStructureWeirFormula)
                {
                    return SelectableWeirFormulaType.GeneralStructure;
                }

                return SelectableWeirFormulaType.SimpleWeir;
            }
            set
            {
                switch (value)
                {
                    case SelectableWeirFormulaType.SimpleWeir:
                        data.WeirFormula = new SimpleWeirFormula();
                        break;
                    case SelectableWeirFormulaType.SimpleGate:
                        data.WeirFormula = new GatedWeirFormula(true);
                        break;
                    case SelectableWeirFormulaType.GeneralStructure:
                        var generalStructureWeirFormula = new GeneralStructureWeirFormula()
                        {
                            BedLevelStructureCentre = data.CrestLevel,
                            WidthStructureCentre = data.CrestWidth,

                            WidthStructureLeftSide    = double.NaN,
                            WidthStructureRightSide   = double.NaN,
                            WidthLeftSideOfStructure  = double.NaN,
                            WidthRightSideOfStructure = double.NaN,
                        };
                        data.WeirFormula = generalStructureWeirFormula;
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
                if (double.TryParse(value, out var crestLevel))
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
            get
            {
                if (double.IsNaN(data.CrestWidth))
                    return null;
                return data.CrestWidth;
            }
            set { data.CrestWidth = value ?? double.NaN; }
        }

        [Category("General")]
        [DisplayName("Use crest width")]
        [PropertyOrder(5)]
        [Description("If true then use the crest width value else calculate with the weir geometry")]
        public bool UseCrestWidth
        {
            get { return data.CrestWidth > 0; }
            set { data.CrestWidth = (value ? data.Geometry.Length : double.NaN); }
        }

        /// <summary>
        /// Determines whether the specified property name is readonly.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        ///   <c>true</c> if the specified property name is readonly; otherwise, <c>false</c>.
        /// </returns>
        [DynamicReadOnlyValidationMethod]
        public bool IsReadonly(string propertyName)
        {
            if (propertyName == "CrestLevel")
            {
                return data.UseCrestLevelTimeSeries;
            }
            if (propertyName == "CrestWidth")
            {
                return double.IsNaN(data.CrestWidth) || data.CrestWidth <= 0.0;
            }
            return false;
        }
    }
}