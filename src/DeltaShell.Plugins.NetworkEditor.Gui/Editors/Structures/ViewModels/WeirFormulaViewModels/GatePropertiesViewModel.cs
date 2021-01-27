using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructuresObjects.StructureFormulas;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="GatePropertiesViewModel"/> defines the view model for the
    /// <see cref="Views.WeirFormulaViews.GatePropertiesView"/>.
    /// </summary>
    public sealed class GatePropertiesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IGatedStructureFormula formula;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool hasDisposed = false;

        private readonly IReadOnlyDictionary<string, string> propertyMapping =
            new Dictionary<string, string>()
            {
                {nameof(SimpleGateFormula.DoorHeight), nameof(GateHeight)},
                {nameof(SimpleGateFormula.HorizontalDoorOpeningDirection), nameof(GateOpeningDirection)},
                {nameof(SimpleGateFormula.HorizontalDoorOpeningWidth), nameof(HorizontalOpeningWidth)},
                {nameof(SimpleGateFormula.UseHorizontalDoorOpeningWidthTimeSeries), nameof(UseHorizontalOpeningWidthTimeSeries)},
                {nameof(SimpleGateFormula.HorizontalDoorOpeningWidthTimeSeries), nameof(HorizontalOpeningWidthTimeSeries)},
                {nameof(SimpleGateFormula.LowerEdgeLevel), nameof(GateLowerEdgeLevel)},
                {nameof(SimpleGateFormula.UseLowerEdgeLevelTimeSeries), nameof(UseGateLowerEdgeLevelTimeSeries)},
                {nameof(SimpleGateFormula.LowerEdgeLevelTimeSeries), nameof(GateLowerEdgeLevelTimeSeries)}
            };

        /// <summary>
        /// Creates a new <see cref="GatePropertiesViewModel"/>.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="weirPropertiesViewModel">The weir properties view model.</param>
        /// <param name="canChooseGateOpeningDirection">
        /// if set to <c>true</c> then the gate opening direction can be set.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="formula"/> or
        /// <paramref name="weirPropertiesViewModel"/> is <c>null</c>.
        /// </exception>
        public GatePropertiesViewModel(IGatedStructureFormula formula,
                                       WeirPropertiesViewModel weirPropertiesViewModel,
                                       bool canChooseGateOpeningDirection)
        {
            Ensure.NotNull(formula, nameof(formula));
            Ensure.NotNull(weirPropertiesViewModel, nameof(weirPropertiesViewModel));

            this.formula = formula;
            WeirPropertiesViewModel = weirPropertiesViewModel;
            CanChooseGateOpeningDirection = canChooseGateOpeningDirection;

            Subscribe();
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

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Subscribe()
        {
            ((INotifyPropertyChanged)formula).PropertyChanged += PropagatePropertyChanged;
        }

        private void Unsubscribe()
        {
            ((INotifyPropertyChanged)formula).PropertyChanged -= PropagatePropertyChanged;
        }

        /// <summary>
        /// Propagates the property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This leverages the PostSharp Entity properties of the <see cref="IGatedStructureFormula"/>. Unfortunately,
        /// their exist no proper way to pass messages around within DeltaShell / D-HYDRO. Instead we
        /// abuse callbacks. This means that the only way to determine whether the underlying domain is
        /// updated, is by adding callbacks to the property changed events. Instead of creating a significant
        /// amount of boilerplate, we propagate the property changed events here. Ideally however, this type
        /// of synchronisation would not be necessary, and instead we would use messages to achieve a cleaner
        /// architecture.
        /// </remarks>
        private void PropagatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, formula) || !IsObservedProperty(e.PropertyName))
            {
                return;
            }

            OnPropertyChanged(propertyMapping[e.PropertyName]);
        }

        private bool IsObservedProperty(string propertyName) =>
            propertyMapping.ContainsKey(propertyName);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Note that this class is sealed, and no un-managed resources need to be
        /// released, as such we do not need to use an isDisposing approach.
        /// </remarks>
        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }
            Unsubscribe();
            hasDisposed = true;
            
        }
    }
}