using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor
{
    /// <summary>
    /// <see cref="HydrodynamicsConstantsViewModel"/> defines the view model for the
    /// <see cref="Views.TimeFrameEditor.HydrodynamicsConstantsView"/>.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public sealed class HydrodynamicsConstantsViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> propertyMapping = new Dictionary<string, string>
        {
            {nameof(HydrodynamicsConstantData.WaterLevel), nameof(WaterLevel)},
            {nameof(HydrodynamicsConstantData.VelocityX), nameof(VelocityX)},
            {nameof(HydrodynamicsConstantData.VelocityY), nameof(VelocityY)},
        };

        private readonly HydrodynamicsConstantData hydrodynamicsConstantData;

        // Note this object takes care of the event propagation for this class.
        private readonly NotifyPropertyChangedEventPropagator eventPropagator;

        /// <summary>
        /// Creates a new <see cref="HydrodynamicsConstantsViewModel"/>.
        /// </summary>
        /// <param name="hydrodynamicsConstantData">The hydrodynamics constant data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hydrodynamicsConstantData"/> is <c>null</c>.
        /// </exception>
        public HydrodynamicsConstantsViewModel(HydrodynamicsConstantData hydrodynamicsConstantData)
        {
            Ensure.NotNull(hydrodynamicsConstantData, nameof(hydrodynamicsConstantData));
            this.hydrodynamicsConstantData = hydrodynamicsConstantData;

            eventPropagator = new NotifyPropertyChangedEventPropagator((INotifyPropertyChanged)hydrodynamicsConstantData,
                                                                       OnPropertyChanged,
                                                                       propertyMapping);
        }

        /// <summary>
        /// Gets or sets the water level in meters.
        /// </summary>
        public double WaterLevel
        {
            get => hydrodynamicsConstantData.WaterLevel;
            set
            {
                // This value is purely a result of being set by a user, as
                // such we do direct comparison.
                if (value != WaterLevel)
                {
                    hydrodynamicsConstantData.WaterLevel = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the velocity in the x-axis in meter per second.
        /// </summary>
        public double VelocityX
        {
            get => hydrodynamicsConstantData.VelocityX;
            set
            {
                // This value is purely a result of being set by a user, as
                // such we do direct comparison.
                if (value != VelocityX)
                {
                    hydrodynamicsConstantData.VelocityX = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the velocity in the y-axis in meter per second.
        /// </summary>
        public double VelocityY
        {
            get => hydrodynamicsConstantData.VelocityY;
            set
            {
                // This value is purely a result of being set by a user, as
                // such we do direct comparison.
                if (value != VelocityY)
                {
                    hydrodynamicsConstantData.VelocityY = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }

            eventPropagator?.Dispose();
        }

        private bool hasDisposed = false;
    }
}