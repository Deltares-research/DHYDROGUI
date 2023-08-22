using System;
using System.Drawing;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    /// <summary>
    /// Class to provide an image based on the name of <see cref="Samples"/>.
    /// </summary>
    public class SamplesImageProvider
    {
        /// <summary>
        /// Gets the image for the provided samples based on the name of the samples.
        /// </summary>
        /// <param name="samples">The samples to provide an image for.</param>
        /// <returns>The image corresponding to the provided samples.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="samples"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when no image is available for the provided samples.</exception>
        public Image GetImage(Samples samples)
        {
            Ensure.NotNull(samples, nameof(samples));

            switch (samples.Name)
            {
                case WaterFlowFMModelDefinition.InitialVelocityXName:
                    return Resources.velocity_x;
                case WaterFlowFMModelDefinition.InitialVelocityYName:
                    return Resources.velocity_y;
                default:
                    throw new ArgumentException($"No image could be provided for `{samples.Name}`.");
            }
        }
    }
}