using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput
{
    /// <summary>
    /// <see cref="Links1D2DOutputLayerSubProvider"/> provides the layer for
    /// output 1D2D links.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class Links1D2DOutputLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="Links1D2DOutputLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public Links1D2DOutputLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IList<ILink1D2D> linkData && linkData.Any() &&
            (parentData is FMMapFileFunctionStore || 
             parentData is FMClassMapFileFunctionStore);

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IList<ILink1D2D> linkData &&
            TryGetCoordinateSystem(parentData, out ICoordinateSystem coordinateSystem)
                ? instanceCreator.CreateLinks1D2DLayer(linkData, coordinateSystem)
                : null;

        private static bool TryGetCoordinateSystem(object parentData, out ICoordinateSystem coordinateSystem)
        {
            coordinateSystem = null;
            switch (parentData)
            {
                case FMMapFileFunctionStore mapFileFunctionStore:
                    coordinateSystem = mapFileFunctionStore.Grid?.CoordinateSystem;
                    return true;
                case FMClassMapFileFunctionStore classMapFileFunctionStore:
                    coordinateSystem = classMapFileFunctionStore.Grid?.CoordinateSystem;
                    return true;
                default:
                    return false;
            }
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            Enumerable.Empty<object>();
    }
}
