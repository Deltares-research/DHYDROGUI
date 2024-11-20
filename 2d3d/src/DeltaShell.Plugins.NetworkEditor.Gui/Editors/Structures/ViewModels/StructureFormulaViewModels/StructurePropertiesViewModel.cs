using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    /// <summary>
    /// <see cref="StructurePropertiesViewModel"/> provides the view model of the
    /// <see cref="Views.WeirFormulaViews.WeirPropertiesView"/>.
    /// </summary>
    public sealed class StructurePropertiesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> PropertyMapping =
            new Dictionary<string, string>()
            {
                {nameof(IStructure.CrestLevel), nameof(CrestLevel)},
                {nameof(IStructure.UseCrestLevelTimeSeries), nameof(UseCrestLevelTimeSeries)},
                {nameof(IStructure.CrestLevelTimeSeries), nameof(CrestLevelTimeSeries)},
                {nameof(IStructure.CrestWidth), nameof(CrestWidth)},
                {nameof(IStructure.Name), nameof(StructureName)},
            };

        private readonly IStructure structure;

        private bool hasDisposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="StructurePropertiesViewModel"/>.
        /// </summary>
        /// <param name="structure">The structure.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="structure"/> is <c>null</c>.
        /// </exception>
        public StructurePropertiesViewModel(IStructure structure)
        {
            Ensure.NotNull(structure, nameof(structure));
            this.structure = structure;

            Subscribe();
        }

        /// <summary>
        /// Gets or sets the crest level.
        /// </summary>
        public double CrestLevel
        {
            get => structure.CrestLevel;
            set => structure.CrestLevel = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the crest level time series
        /// should be used.
        /// </summary>
        public bool UseCrestLevelTimeSeries
        {
            get => structure.UseCrestLevelTimeSeries;
            set => structure.UseCrestLevelTimeSeries = value;
        }

        /// <summary>
        /// Gets the crest level time series.
        /// </summary>
        public TimeSeries CrestLevelTimeSeries => structure.CrestLevelTimeSeries;

        /// <summary>
        /// Gets or sets the width of the crest.
        /// </summary>
        public double? CrestWidth
        {
            get => !double.IsNaN(structure.CrestWidth) ? (double?) structure.CrestWidth : null;
            set => structure.CrestWidth = value ?? double.NaN;
        }

        /// <summary>
        /// Gets or sets the name of the structure.
        /// </summary>
        public string StructureName
        {
            get => structure.Name;
            set => structure.Name = value;
        }

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

        private void Subscribe()
        {
            ((INotifyPropertyChanged) structure).PropertyChanged += PropagatePropertyChanged;
        }

        /// <summary>
        /// Propagates the property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This leverages the PostSharp Entity properties of the <see cref="Weir2D"/>. Unfortunately,
        /// their exist no proper way to pass messages around within DeltaShell / D-HYDRO. Instead we
        /// abuse callbacks. This means that the only way to determine whether the underlying domain is
        /// updated, is by adding callbacks to the property changed events. Instead of creating a significant
        /// amount of boilerplate, we propagate the property changed events here. Ideally however, this type
        /// of synchronisation would not be necessary, and instead we would use messages to achieve a cleaner
        /// architecture.
        /// </remarks>
        private void PropagatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, structure) || !IsObservedProperty(e.PropertyName))
            {
                return;
            }

            OnPropertyChanged(PropertyMapping[e.PropertyName]);
        }

        private bool IsObservedProperty(string propertyName) =>
            PropertyMapping.ContainsKey(propertyName);

        private void Unsubscribe()
        {
            ((INotifyPropertyChanged) structure).PropertyChanged -= PropagatePropertyChanged;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}