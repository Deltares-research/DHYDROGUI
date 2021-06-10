using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// <see cref="MdwFileDTO"/> defines the data-transfer object (DTO) for
    /// loading and saving .mdw files.
    /// </summary>
    /// <remarks>
    /// The .mdw file defines both the properties of the
    /// <see cref="ModelDefinition.WaveModelDefinition"/> as well as the
    /// <see cref="ITimeFrameData"/>. Within the architecture
    /// of D-Waves however, these are separate objects, as such
    /// we need a class to contain this data.
    /// </remarks>
    public sealed class MdwFileDTO
    {
        /// <summary>
        /// Creates a new <see cref="MdwFileDTO"/>.
        /// </summary>
        /// <param name="waveModelDefinition">The wave model definition.</param>
        /// <param name="timeFrameData">The time frame data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public MdwFileDTO(WaveModelDefinition waveModelDefinition,
                          ITimeFrameData timeFrameData)
        {
            Ensure.NotNull(waveModelDefinition, nameof(waveModelDefinition));
            Ensure.NotNull(timeFrameData, nameof(timeFrameData));

            WaveModelDefinition = waveModelDefinition;
            TimeFrameData = timeFrameData;
        }

        /// <summary>
        /// Gets the <see cref="ModelDefinition.WaveModelDefinition"/>
        /// </summary>
        public WaveModelDefinition WaveModelDefinition { get; }

        /// <summary>
        /// Gets the <see cref="ITimeFrameData"/>
        /// </summary>
        public ITimeFrameData TimeFrameData { get; }
    }
}