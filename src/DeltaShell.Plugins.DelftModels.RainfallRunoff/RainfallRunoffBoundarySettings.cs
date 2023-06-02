using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Represents the settings for a rainfall-runoff boundary.
    /// </summary>
    public class RainfallRunoffBoundarySettings : ICloneable, INotifyPropertyChanged
    {
        private RainfallRunoffBoundaryData boundaryData;
        private bool useLocalBoundaryData;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffBoundarySettings"/> class with the specified boundary data and local boundary
        /// data usage flag.
        /// </summary>
        /// <param name="data">The rainfall-runoff boundary data.</param>
        /// <param name="useLocalBoundaryData">A value indicating whether to use local boundary data.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> is <c>null</c>.</exception>
        public RainfallRunoffBoundarySettings(RainfallRunoffBoundaryData data, bool useLocalBoundaryData)
        {
            Ensure.NotNull(data, nameof(data));

            BoundaryData = data;
            UseLocalBoundaryData = useLocalBoundaryData;
        }

        /// <summary>
        /// Gets or sets the rainfall-runoff boundary data.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <c>value</c> is <c>null</c>.</exception>
        public RainfallRunoffBoundaryData BoundaryData
        {
            get => boundaryData;
            set
            {
                Ensure.NotNull(value, nameof(value));
                
                if (boundaryData == value)
                {
                    return;
                }

                boundaryData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use local boundary data.
        /// </summary>
        /// <value><c>true</c> to use local boundary data; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Local Water Level Data is given by the user.
        /// If this value is <c>true</c>: when the unpaved catchment is linked to a Lateral (1D) it's still using the local data (not the water level at the lateral)
        /// </remarks>
        public bool UseLocalBoundaryData
        {
            get => useLocalBoundaryData;
            set
            {
                if (value == useLocalBoundaryData)
                {
                    return;
                }

                useLocalBoundaryData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var oldBoundaryData = (RainfallRunoffBoundaryData)BoundaryData.Clone();

            var clone = new RainfallRunoffBoundarySettings(oldBoundaryData, useLocalBoundaryData);

            return clone;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}