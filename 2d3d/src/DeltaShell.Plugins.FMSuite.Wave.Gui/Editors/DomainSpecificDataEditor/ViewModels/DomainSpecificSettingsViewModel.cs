using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// View model for the domain specific settings.
    /// </summary>
    public class DomainSpecificSettingsViewModel : INotifyPropertyChanged
    {
        private readonly IWaveDomainData domainData;
        private DirectionalSpaceSettingsViewModel directionalSpaceSettings;
        private FrequencySpaceSettingsViewModel frequencySpaceSettings;
        private HydroDynamicsSettingsViewModel hydroDynamicsSettings;
        private WindSettingsViewModel windSettings;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainSpecificSettingsViewModel"/> class.
        /// </summary>
        /// <param name="domainData">The domain data.</param>
        public DomainSpecificSettingsViewModel(IWaveDomainData domainData)
        {
            this.domainData = domainData;

            DirectionalSpaceSettings = new DirectionalSpaceSettingsViewModel(domainData.SpectralDomainData);
            FrequencySpaceSettings = new FrequencySpaceSettingsViewModel(domainData.SpectralDomainData);
            HydroDynamicsSettings = new HydroDynamicsSettingsViewModel(domainData.HydroFromFlowData);
            WindSettings = new WindSettingsViewModel(domainData.MeteoData);
        }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <value>
        /// The name of the domain.
        /// </value>
        public string DomainName => domainData.Name;

        /// <summary>
        /// Gets or sets a value indicating whether to use custom directional space settings or model defaults.
        /// </summary>
        /// <value>
        /// <c>true</c> if custom settings are used; otherwise, <c>false</c>.
        /// </value>
        public bool UseCustomDirectionalSpaceSettings
        {
            get => !domainData.SpectralDomainData.UseDefaultDirectionalSpace;
            set
            {
                if (UseCustomDirectionalSpaceSettings != value)
                {
                    domainData.SpectralDomainData.UseDefaultDirectionalSpace = !value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom frequency space settings or model defaults.
        /// </summary>
        /// <value>
        /// <c>true</c> if custom settings are used; otherwise, <c>false</c>.
        /// </value>
        public bool UseCustomFrequencySpaceSettings
        {
            get => !domainData.SpectralDomainData.UseDefaultFrequencySpace;
            set
            {
                if (UseCustomFrequencySpaceSettings != value)
                {
                    domainData.SpectralDomainData.UseDefaultFrequencySpace = !value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom hydrodynamic settings or model defaults.
        /// </summary>
        /// <value>
        /// <c>true</c> if custom settings are used; otherwise, <c>false</c>.
        /// </value>
        public bool UseCustomHydroDynamicsSettings
        {
            get => !domainData.HydroFromFlowData.UseDefaultHydroFromFlowSettings;
            set
            {
                if (UseCustomHydroDynamicsSettings != value)
                {
                    domainData.HydroFromFlowData.UseDefaultHydroFromFlowSettings = !value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom wind settings or model defaults.
        /// </summary>
        /// <value>
        /// <c>true</c> if custom settings are used; otherwise, <c>false</c>.
        /// </value>
        public bool UseCustomWindSettings
        {
            get => !domainData.UseGlobalMeteoData;
            set
            {
                if (UseCustomWindSettings != value)
                {
                    domainData.UseGlobalMeteoData = !value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the view model for the directional space settings.
        /// </summary>
        /// <value>
        /// The directional space settings.
        /// </value>
        public DirectionalSpaceSettingsViewModel DirectionalSpaceSettings
        {
            get => directionalSpaceSettings;
            set
            {
                if (directionalSpaceSettings != value)
                {
                    directionalSpaceSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the view model for the frequency space settings.
        /// </summary>
        /// <value>
        /// The frequency space settings.
        /// </value>
        public FrequencySpaceSettingsViewModel FrequencySpaceSettings
        {
            get => frequencySpaceSettings;
            set
            {
                if (frequencySpaceSettings != value)
                {
                    frequencySpaceSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the view model for the hydro dynamics settings.
        /// </summary>
        /// <value>
        /// The hydro dynamics settings.
        /// </value>
        public HydroDynamicsSettingsViewModel HydroDynamicsSettings
        {
            get => hydroDynamicsSettings;
            set
            {
                if (hydroDynamicsSettings != value)
                {
                    hydroDynamicsSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the view model for the wind settings.
        /// </summary>
        /// <value>
        /// The wind settings.
        /// </value>
        public WindSettingsViewModel WindSettings
        {
            get => windSettings;
            set
            {
                if (windSettings != value)
                {
                    windSettings = value;
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