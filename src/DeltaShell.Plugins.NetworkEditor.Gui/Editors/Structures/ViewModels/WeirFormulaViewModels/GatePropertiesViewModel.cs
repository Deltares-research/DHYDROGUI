using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    public sealed class GatePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly IGatedWeirFormula formula;

        public event PropertyChangedEventHandler PropertyChanged;

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
            set
            {
                // The floating point values are provided by the user in an entry
                // As such no error can be introduced, and either values or the same
                // or they should be updated.
                if (value == GateLowerEdgeLevel)
                {
                    return;
                }

                formula.LowerEdgeLevel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the gate lower edge level
        /// time series should be used.
        /// </summary>
        public bool UseGateLowerEdgeLevelTimeSeries
        {
            get => formula.UseLowerEdgeLevelTimeSeries;
            set
            {
                if (value == UseGateLowerEdgeLevelTimeSeries)
                {
                    return;
                }

                formula.UseLowerEdgeLevelTimeSeries = value;
                OnPropertyChanged();
            }
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
            set
            {
                if (value == GateHeight)
                {
                    return;
                }

                formula.DoorHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the width of the horizontal opening.
        /// </summary>
        public double HorizontalOpeningWidth
        {
            get => formula.HorizontalDoorOpeningWidth;
            set
            {
                if (value == HorizontalOpeningWidth)
                {
                    return;
                }

                formula.HorizontalDoorOpeningWidth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the horizontal opening width
        /// time series should be used.
        /// </summary>
        public bool UseHorizontalOpeningWidthTimeSeries
        {
            get => formula.UseHorizontalDoorOpeningWidthTimeSeries;
            set
            {
                if (value == UseHorizontalOpeningWidthTimeSeries)
                {
                    return;
                }

                formula.UseHorizontalDoorOpeningWidthTimeSeries = value;
                OnPropertyChanged();
            }
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
            set
            {
                if (value == GateOpeningDirection)
                {
                    return;
                }

                formula.HorizontalDoorOpeningDirection = value;
                OnPropertyChanged();
            }
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}