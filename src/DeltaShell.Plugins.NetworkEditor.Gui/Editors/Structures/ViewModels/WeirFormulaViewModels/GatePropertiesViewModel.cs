using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="GatePropertiesViewModel"/> defines the view model for the
    /// <see cref="Views.WeirFormulaViews.GatePropertiesView"/>.
    /// </summary>
    public sealed class GatePropertiesViewModel
    {
        private readonly IGatedWeirFormula formula;

        /// <summary>
        /// Creates a new <see cref="GatePropertiesViewModel"/>.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="weirPropertiesViewModel">The weir properties view model.</param>
        /// <param name="canChooseGateOpeningDirection">
        /// if set to <c>true</c> then the gate opening direction can be set.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="formula"/> or
        /// <paramref name="weirPropertiesViewModel"/> is <c>null</c>.
        /// </exception>
        public GatePropertiesViewModel(IGatedWeirFormula formula,
                                       WeirPropertiesViewModel weirPropertiesViewModel,
                                       bool canChooseGateOpeningDirection)
        {
            Ensure.NotNull(formula, nameof(formula));
            Ensure.NotNull(weirPropertiesViewModel, nameof(weirPropertiesViewModel));

            this.formula = formula;
            WeirPropertiesViewModel = weirPropertiesViewModel;
            CanChooseGateOpeningDirection = canChooseGateOpeningDirection;
        }

        /// <summary>
        /// Gets or sets the gate lower edge level.
        /// </summary>
        public double GateLowerEdgeLevel
        {
            get => formula.LowerEdgeLevel;
            set => formula.LowerEdgeLevel = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the gate lower edge level
        /// time series should be used.
        /// </summary>
        public bool UseGateLowerEdgeLevelTimeSeries
        {
            get => formula.UseLowerEdgeLevelTimeSeries;
            set => formula.UseLowerEdgeLevelTimeSeries = value;
        }

        /// <summary>
        /// Gets the gate lower edge level time series.
        /// </summary>
        public TimeSeries GateLowerEdgeLevelTimeSeries =>
            formula.LowerEdgeLevelTimeSeries;

        /// <summary>
        /// Gets or sets the height of the gate.
        /// </summary>
        public double GateHeight
        {
            get => formula.DoorHeight;
            set => formula.DoorHeight = value;
        }

        /// <summary>
        /// Gets or sets the height of the gate opening.
        /// </summary>
        public double GateOpeningHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the horizontal opening.
        /// </summary>
        public double HorizontalOpeningWidth
        {
            get => formula.HorizontalDoorOpeningWidth;
            set => formula.HorizontalDoorOpeningWidth = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the horizontal opening width
        /// time series should be used.
        /// </summary>
        public bool UseHorizontalOpeningWidthTimeSeries
        {
            get => formula.UseHorizontalDoorOpeningWidthTimeSeries;
            set => formula.UseHorizontalDoorOpeningWidthTimeSeries = value;
        }

        /// <summary>
        /// Gets the horizontal opening width time series.
        /// </summary>
        public TimeSeries HorizontalOpeningWidthTimeSeries =>
            formula.HorizontalDoorOpeningWidthTimeSeries;

        /// <summary>
        /// Gets or sets the gate opening direction.
        /// </summary>
        public GateOpeningDirection GateOpeningDirection
        {
            get => formula.HorizontalDoorOpeningDirection;
            set => formula.HorizontalDoorOpeningDirection = value;
        }

        /// <summary>
        /// Gets the weir properties view model.
        /// </summary>
        public WeirPropertiesViewModel WeirPropertiesViewModel { get; }

        /// <summary>
        /// Gets a value indicating whether the gate opening direction can be
        /// chosen.
        /// </summary>
        public bool CanChooseGateOpeningDirection { get; }
    }
}