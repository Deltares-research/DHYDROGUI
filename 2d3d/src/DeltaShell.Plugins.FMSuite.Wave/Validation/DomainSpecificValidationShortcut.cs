using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// This class describes a shortcut that can be used to link a domain specific validation issue
    /// to the domain specific tab on the wave settings view.
    /// </summary>
    public class DomainSpecificValidationShortcut
    {
        /// <summary>
        /// Creates a new instance of <see cref="DomainSpecificValidationShortcut"/>.
        /// </summary>
        /// <param name="waveModel">The wave model that is used as data for the view that is to be opened.</param>
        /// <param name="selectedDomainData">The domain data that has to be selected in the view.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> or <paramref name="selectedDomainData"/> is <c>null</c>.
        /// </exception>
        public DomainSpecificValidationShortcut(WaveModel waveModel, IWaveDomainData selectedDomainData)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));
            Ensure.NotNull(selectedDomainData, nameof(selectedDomainData));
            
            WaveModel = waveModel;
            SelectedDomainData = selectedDomainData;
        }
        
        /// <summary>
        /// Gets the wave model that is used as data for the view that is to be opened.
        /// </summary>
        public WaveModel WaveModel { get; }
        
        /// <summary>
        /// Gets the domain data that has to be selected in the view.
        /// </summary>
        public IWaveDomainData SelectedDomainData { get; }

        /// <summary>
        /// Gets the tab name of the domain specific wave settings view that needs to be opened.
        /// </summary>
        public string TabName
        {
            get => Resources.Wave_Domain_specific_settings;
        }
    }
}