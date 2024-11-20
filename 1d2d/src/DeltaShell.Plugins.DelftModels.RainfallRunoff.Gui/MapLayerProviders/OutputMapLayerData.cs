
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders
{
    /// <summary>
    /// A data transfer object used to create the output map layer for the Rainfall Runoff model.
    /// </summary>
    public class OutputMapLayerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputMapLayerData"/> class.
        /// </summary>
        /// <param name="model"> The Rainfall Runoff model. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public OutputMapLayerData(RainfallRunoffModel model)
        {
            Ensure.NotNull(model, nameof(model));

            Name = "Output";
            Model = model;
        }

        /// <summary>
        /// The name of the output layer.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The Rainfall Runoff model.
        /// </summary>
        public RainfallRunoffModel Model { get; }
    }
}