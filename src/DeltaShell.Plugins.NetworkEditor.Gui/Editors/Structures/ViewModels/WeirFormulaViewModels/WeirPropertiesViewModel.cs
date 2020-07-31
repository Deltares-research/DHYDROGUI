using System;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="WeirPropertiesViewModel"/> provides the view model of the
    /// <see cref="Views.WeirFormulaViews.WeirPropertiesView"/>.
    /// </summary>
    public sealed class WeirPropertiesViewModel
    {
        private readonly IWeir weir;

        /// <summary>
        /// Creates a new <see cref="WeirPropertiesViewModel"/>.
        /// </summary>
        /// <param name="weir">The weir.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="weir"/> is <c>null</c>.
        /// </exception>
        public WeirPropertiesViewModel(IWeir weir)
        {
            Ensure.NotNull(weir, nameof(weir));
            this.weir = weir;
        }

        /// <summary>
        /// Gets or sets the crest level.
        /// </summary>
        public double CrestLevel
        {
            get => weir.CrestLevel;
            set => weir.CrestLevel = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the crest level time series
        /// should be used.
        /// </summary>
        public bool UseCrestLevelTimeSeries
        {
            get => weir.UseCrestLevelTimeSeries;
            set => weir.UseCrestLevelTimeSeries = value;
        }

        /// <summary>
        /// Gets the crest level time series.
        /// </summary>
        public TimeSeries CrestLevelTimeSeries => weir.CrestLevelTimeSeries;

        /// <summary>
        /// Gets or sets the width of the crest.
        /// </summary>
        public double? CrestWidth
        {
            get => !double.IsNaN(weir.CrestWidth) ? (double?) weir.CrestWidth : null;
            set => weir.CrestWidth = value ?? double.NaN;
        }
    }
}