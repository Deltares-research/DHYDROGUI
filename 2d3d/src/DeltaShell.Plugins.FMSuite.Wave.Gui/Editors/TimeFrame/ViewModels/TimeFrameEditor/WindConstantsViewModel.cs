using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor
{
    /// <summary>
    /// <see cref="WindConstantsViewModel"/> defines the view model for the
    /// <see cref="Views.TimeFrameEditor.WindConstantsView"/>.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="IDisposable" />
    public sealed class WindConstantsViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> propertyMapping = new Dictionary<string, string>
        {
            {nameof(WindConstantData.Speed), nameof(Speed)},
            {nameof(WindConstantData.Direction), nameof(Directions)},
        };

        private readonly WindConstantData windConstantData;

        // Note this object takes care of the event propagation for this class.
        private readonly NotifyPropertyChangedEventPropagator eventPropagator;

        public WindConstantsViewModel(WindConstantData windConstantData)
        {
            Ensure.NotNull(windConstantData, nameof(windConstantData));
            this.windConstantData = windConstantData;

            eventPropagator = new NotifyPropertyChangedEventPropagator((INotifyPropertyChanged)windConstantData,
                                                                       OnPropertyChanged,
                                                                       propertyMapping);
        }

        /// <summary>
        /// Gets or sets the in meters.
        /// </summary>
        public double Speed
        {
            get => windConstantData.Speed;
            set
            {
                // This value is purely a result of being set by a user, as
                // such we do direct comparison.
                if (value != Speed)
                {
                    windConstantData.Speed = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the velocity in the x-axis in meter per second.
        /// </summary>
        public double Directions
        {
            get => windConstantData.Direction;
            set
            {
                // This value is purely a result of being set by a user, as
                // such we do direct comparison.
                if (value != Directions)
                {
                    windConstantData.Direction = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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