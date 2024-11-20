using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors
{
    /// <summary>
    /// <see cref="FMPumpPropertiesRow"/> defines a single row within the
    /// multi-data editor of a single <see cref="IPump"/>.
    /// </summary>
    /// <seealso cref="IDisposable" />
    /// <seealso cref="INotifyPropertyChange" />
    /// <seealso cref="IFeatureRowObject" />
    public class FMPumpPropertiesRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        private IPump pump;

        /// <summary>
        /// Creates a new <see cref="FMPumpPropertiesRow"/>.
        /// </summary>
        /// <param name="pump">The pump.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="pump"/> is <c>null</c>.
        /// </exception>
        public FMPumpPropertiesRow(IPump pump)
        {
            Ensure.NotNull(pump, nameof(pump));
            Pump = pump;
        }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        [DisplayName("Group Name")]
        public string GroupName
        {
            get => Pump.GroupName;
            set => Pump.GroupName = value;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DisplayName("Name")]
        public string Name
        {
            get => Pump.Name;
            set => Pump.Name = value;
        }

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <c>Pump.UseCapacityTimeSeries</c> is <c>true</c>.
        /// </exception>
        [DisplayName(nameof(Capacity) + " [m3/s]")]
        [DynamicReadOnly]
        public virtual string Capacity
        {
            get => Pump.UseCapacityTimeSeries 
                       ? $"{Pump.Name}_{KnownStructureProperties.Capacity}.tim"
                       : Pump.Capacity.ToString(CultureInfo.CurrentCulture);
            set
            {
                if (Pump.UseCapacityTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent pump capacity.");
                }

                Pump.Capacity = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Determines whether the specified <paramref name="propertyName"/>
        /// is currently read-only.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="propertyName"/> is currently read-only;
        /// otherwise, <c>false</c>.
        /// </returns>
        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (Pump == null)
            {
                return false;
            }

            if (propertyName == nameof(Capacity))
            {
                return Pump.UseCapacityTimeSeries;
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from property changed.
                Pump = null;
            }
        }

        private IPump Pump
        {
            get => pump;
            set
            {
                if (pump != null)
                {
                    ((INotifyPropertyChanged) pump).PropertyChanged -= PumpPropertiesRowPropertyChanged;
                }

                pump = value;

                if (pump != null)
                {
                    ((INotifyPropertyChanged) pump).PropertyChanged += PumpPropertiesRowPropertyChanged;
                }
            }
        }

        private void PumpPropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        [Browsable(false)]
        public bool HasParent { get; set; }

        /// <summary>
        /// Gets the underlying feature.
        /// </summary>
        /// <returns>
        /// The <see cref="IPump"/> feature.
        /// </returns>
        public IFeature GetFeature() => Pump;
    }
}