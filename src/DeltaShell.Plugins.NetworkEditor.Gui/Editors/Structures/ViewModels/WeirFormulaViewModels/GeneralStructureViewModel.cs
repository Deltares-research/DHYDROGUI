using System.ComponentModel;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="GeneralStructureViewModel"/> provides the view model for the
    /// <see cref="Views.WeirFormulaViews.GeneralStructureView"/>.
    /// </summary>
    /// <seealso cref="WeirViewModel" />
    [Description("General Structure")]
    public sealed class GeneralStructureViewModel : WeirViewModel
    {
        private readonly GeneralStructureWeirFormula formula;

        /// <summary>
        /// Creates a new <see cref="GeneralStructureViewModel"/>
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="weirPropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public GeneralStructureViewModel(GeneralStructureWeirFormula formula,
                                         WeirPropertiesViewModel weirPropertiesViewModel) :
            base(weirPropertiesViewModel)
        {
            Ensure.NotNull(formula, nameof(formula));

            this.formula = formula;
            GatePropertiesViewModel = new GatePropertiesViewModel(formula, weirPropertiesViewModel, false);
        }

        private static double? ToNullableValue(double value) =>
            double.IsNaN(value) ? null : (double?) value;

        /// <summary>
        /// Gets or sets the Upstream1 width.
        /// </summary>
        public double? Upstream1Width
        {
            get => ToNullableValue(formula.WidthStructureLeftSide); 
            set => formula.WidthStructureLeftSide = value ?? double.NaN;
        }

        /// <summary>
        /// Gets or sets the Upstream1 level.
        /// </summary>
        public double Upstream1Level
        {
            get => formula.BedLevelLeftSideStructure; 
            set => formula.BedLevelLeftSideStructure = value;
        }

        /// <summary>
        /// Gets or sets the Upstream2 width.
        /// </summary>
        public double? Upstream2Width
        {
            get => ToNullableValue(formula.WidthLeftSideOfStructure); 
            set => formula.WidthLeftSideOfStructure = value ?? double.NaN;
        }

        /// <summary>
        /// Gets or sets the Upstream2 level.
        /// </summary>
        public double Upstream2Level
        {
            get => formula.BedLevelLeftSideOfStructure; 
            set => formula.BedLevelLeftSideOfStructure= value;
        }

        /// <summary>
        /// Gets or sets the Downstream1 width.
        /// </summary>
        public double? Downstream1Width
        {
            get => ToNullableValue(formula.WidthStructureRightSide); 
            set => formula.WidthStructureRightSide = value ?? double.NaN;
        }

        /// <summary>
        /// Gets or sets the Downstream1 level.
        /// </summary>
        public double Downstream1Level
        {
            get => formula.BedLevelRightSideStructure; 
            set => formula.BedLevelRightSideStructure = value;
        }

        /// <summary>
        /// Gets or sets the Downstream2 width.
        /// </summary>
        public double? Downstream2Width
        {
            get => ToNullableValue(formula.WidthRightSideOfStructure); 
            set => formula.WidthRightSideOfStructure = value ?? double.NaN;
        }

        /// <summary>
        /// Gets or sets the Downstream2 level.
        /// </summary>
        public double Downstream2Level
        {
            get => formula.BedLevelRightSideOfStructure; 
            set => formula.BedLevelRightSideOfStructure= value;
        }

        /// <summary>
        /// Gets the gate properties view model.
        /// </summary>
        public GatePropertiesViewModel GatePropertiesViewModel { get; }

        /// <summary>
        /// Gets or sets the positive free gate flow.
        /// </summary>
        public double FreeGateFlowPositive
        {
            get => formula.PositiveFreeGateFlow;
            set => formula.PositiveFreeGateFlow = value;
        }

        /// <summary>
        /// Gets or sets the negative free gate flow.
        /// </summary>
        public double FreeGateFlowNegative
        {
            get => formula.NegativeFreeGateFlow;
            set => formula.NegativeFreeGateFlow = value;
        }

        /// <summary>
        /// Gets or sets the positive drowned gate flow.
        /// </summary>
        public double DrownedGateFlowPositive
        {
            get => formula.PositiveDrownedGateFlow;
            set => formula.PositiveDrownedGateFlow = value;
        }

        /// <summary>
        /// Gets or sets the negative drowned gate flow.
        /// </summary>
        public double DrownedGateFlowNegative
        {
            get => formula.NegativeDrownedGateFlow;
            set => formula.NegativeDrownedGateFlow = value;
        }

        /// <summary>
        /// Gets or sets the positive free weir flow.
        /// </summary>
        public double FreeWeirFlowPositive
        {
            get => formula.PositiveFreeWeirFlow;
            set => formula.PositiveFreeWeirFlow = value;
        }

        /// <summary>
        /// Gets or sets the negative free weir flow.
        /// </summary>
        public double FreeWeirFlowNegative
        {
            get => formula.NegativeFreeWeirFlow;
            set => formula.NegativeFreeWeirFlow = value;
        }

        /// <summary>
        /// Gets or sets the positive drowned weir flow.
        /// </summary>
        public double DrownedWeirFlowPositive
        {
            get => formula.PositiveDrownedWeirFlow;
            set => formula.PositiveDrownedWeirFlow = value;
        }

        /// <summary>
        /// Gets or sets the negative drowned weir flow.
        /// </summary>
        public double DrownedWeirFlowNegative
        {
            get => formula.NegativeDrownedWeirFlow;
            set => formula.NegativeDrownedWeirFlow = value;
        }

        /// <summary>
        /// Gets or sets the positive contraction coefficient.
        /// </summary>
        public double ContractionCoefficientPositive
        {
            get => formula.PositiveContractionCoefficient;
            set => formula.PositiveContractionCoefficient = value;
        }

        /// <summary>
        /// Gets or sets the negative contraction coefficient.
        /// </summary>
        public double ContractionCoefficientNegative
        {
            get => formula.NegativeContractionCoefficient;
            set => formula.NegativeContractionCoefficient = value;
        }

        /// <summary>
        /// Gets or sets the extra resistance.
        /// </summary>
        public double ExtraResistance
        {
            get => formula.ExtraResistance;
            set => formula.ExtraResistance = value;
        }
    }
}