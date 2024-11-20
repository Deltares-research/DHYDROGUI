using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common
{
    /// <summary>
    /// <see cref="IModelProperty"/> describes how to retrieve and create a layer
    /// for properties of the <see cref="IWaterFlowFMModel"/> in the
    /// <see cref="InputPropertyLayerSubProvider{TModelProperty}"/>.
    /// </summary>
    internal interface IModelProperty
    {
        /// <summary>
        /// Retrieve the relevant property from the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="model">The model to retrieve the property from.</param>
        /// <returns>The relevant property.</returns>
        object Retrieve(IWaterFlowFMModel model);

        /// <summary>
        /// Create the relevant layer with the specified <paramref name="creator"/> given the
        /// specified <paramref name="model"/>.
        /// </summary>
        /// <param name="creator">The <see cref="IFlowFMLayerInstanceCreator"/> to create the layer width.</param>
        /// <param name="model">The model containing the data.</param>
        /// <returns>The created <see cref="ILayer"/>.</returns>
        ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model);
    }
}