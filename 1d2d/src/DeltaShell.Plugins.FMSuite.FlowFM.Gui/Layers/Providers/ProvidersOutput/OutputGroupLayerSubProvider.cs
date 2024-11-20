using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput
{
    /// <summary>
    /// <see cref="OutputGroupLayerSubProvider"/> provides the output group layer
    /// containing the 2D components as well as the child layer objects that should be
    /// part of it.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class OutputGroupLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="OutputGroupLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public OutputGroupLayerSubProvider (IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is OutputLayerData inputData &&
            inputData.Model.Equals(parentData);

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData) 
                ? instanceCreator.CreateOutputGroupLayer() 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is OutputLayerData layerData))
            {
                yield break;
            }

            IWaterFlowFMModel model = layerData.Model;

            if (model.HasSnappedOutputFeatures())
            {
                yield return new OutputSnappedFeatureGroupData(model);
            }

            if (layerData.Model.Output1DFileStore != null)
            {
                yield return layerData.Model.Output1DFileStore;
            }

            if (model.OutputMapFileStore != null)
            {
                yield return model.OutputMapFileStore;
            }

            if (model.OutputHisFileStore != null)
            {
                yield return model.OutputHisFileStore;
            }
            
            if (model.OutputClassMapFileStore != null)
            {
                yield return model.OutputClassMapFileStore;
            }

            if (model.OutputFouFileStore!= null)
            {
                yield return model.OutputFouFileStore;
            }
        }
    }
}