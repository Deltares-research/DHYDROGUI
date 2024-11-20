using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>View model for directional space settings</summary>
    public class DirectionalSpaceSettingsViewModel : INotifyPropertyChanged
    {
        private readonly SpectralDomainData spectralDomainData;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public DirectionalSpaceSettingsViewModel(SpectralDomainData spectralDomainData)
        {
            this.spectralDomainData = spectralDomainData;
        }

        /// <summary>
        /// Gets or sets the type of the directional space
        /// </summary>
        /// <value>
        /// The type of the directional space.
        /// </value>
        public DirectionalSpaceType Type
        {
            get => ConvertToDirectionalSpaceType(spectralDomainData.DirectionalSpaceType);
            set
            {
                if (ConvertToDirectionalSpaceType(spectralDomainData.DirectionalSpaceType) != value)
                {
                    spectralDomainData.DirectionalSpaceType = ConvertToWaveDirectionalSpaceType(value);

                    if (Type == DirectionalSpaceType.Circle)
                    {
                        StartDirection = 0;
                        EndDirection = 360;
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of directions.
        /// </summary>
        /// <value>
        /// The number of directions.
        /// </value>
        public int NrOfDirections
        {
            get => spectralDomainData.NDir;
            set
            {
                if (spectralDomainData.NDir != value)
                {
                    spectralDomainData.NDir = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the start direction.
        /// </summary>
        /// <value>
        /// The start direction.
        /// </value>
        public double StartDirection
        {
            get => spectralDomainData.StartDir;
            set
            {
                if (Math.Abs(spectralDomainData.StartDir - value) > double.Epsilon)
                {
                    spectralDomainData.StartDir = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the end direction.
        /// </summary>
        /// <value>
        /// The end direction.
        /// </value>
        public double EndDirection
        {
            get => spectralDomainData.EndDir;
            set
            {
                if (Math.Abs(spectralDomainData.EndDir - value) > double.Epsilon)
                {
                    spectralDomainData.EndDir = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static DirectionalSpaceType ConvertToDirectionalSpaceType(WaveDirectionalSpaceType directionalSpaceType)
        {
            switch (directionalSpaceType)
            {
                case WaveDirectionalSpaceType.Circle:
                    return DirectionalSpaceType.Circle;
                case WaveDirectionalSpaceType.Sector:
                    return DirectionalSpaceType.Sector;
                default:
                    throw new ArgumentOutOfRangeException(nameof(directionalSpaceType), directionalSpaceType, null);
            }
        }

        private static WaveDirectionalSpaceType ConvertToWaveDirectionalSpaceType(DirectionalSpaceType directionalSpaceType)
        {
            switch (directionalSpaceType)
            {
                case DirectionalSpaceType.Circle:
                    return WaveDirectionalSpaceType.Circle;
                case DirectionalSpaceType.Sector:
                    return WaveDirectionalSpaceType.Sector;
                default:
                    throw new ArgumentOutOfRangeException($"{directionalSpaceType.ToString()} is not a valid directional space type");
            }
        }
    }
}