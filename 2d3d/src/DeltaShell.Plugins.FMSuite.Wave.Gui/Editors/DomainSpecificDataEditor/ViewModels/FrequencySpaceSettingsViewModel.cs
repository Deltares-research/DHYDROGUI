using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// View model for the frequency space settings.
    /// </summary>
    public class FrequencySpaceSettingsViewModel : INotifyPropertyChanged
    {
        private readonly SpectralDomainData spectralDomainData;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySpaceSettingsViewModel"/> class.
        /// </summary>
        /// <param name="spectralDomainData">The spectral domain data.</param>
        public FrequencySpaceSettingsViewModel(SpectralDomainData spectralDomainData)
        {
            this.spectralDomainData = spectralDomainData;
        }

        /// <summary>
        /// Gets or sets the number of frequencies.
        /// </summary>
        /// <value>
        /// The number of frequencies.
        /// </value>
        public int NrOfFrequencies
        {
            get => spectralDomainData.NFreq;
            set
            {
                if (spectralDomainData.NFreq != value)
                {
                    spectralDomainData.NFreq = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the start frequency.
        /// </summary>
        /// <value>
        /// The start frequency.
        /// </value>
        public double StartFrequency
        {
            get => spectralDomainData.FreqMin;
            set
            {
                if (Math.Abs(spectralDomainData.FreqMin - value) > double.Epsilon)
                {
                    spectralDomainData.FreqMin = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the end frequency.
        /// </summary>
        /// <value>
        /// The end frequency.
        /// </value>
        public double EndFrequency
        {
            get => spectralDomainData.FreqMax;
            set
            {
                if (Math.Abs(spectralDomainData.FreqMax - value) > double.Epsilon)
                {
                    spectralDomainData.FreqMax = value;
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
    }
}