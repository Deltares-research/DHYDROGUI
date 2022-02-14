using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput
{
    /// <summary>
    /// <see cref="FunctionGroupingLayerSubProvider"/> provides the group layer for
    /// function groupings as well as the child layer objects contained in this group
    /// layer.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal class FunctionGroupingLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="FunctionGroupingLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public FunctionGroupingLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IGrouping<string, IFunction> functionGrouping &&
            functionGrouping.Any();

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IGrouping<string, IFunction> functionGrouping
                ? instanceCreator.CreateFunctionGroupingLayer(functionGrouping)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            data is IGrouping<string, IFunction> functionGrouping &&
            functionGrouping is IEnumerable<IFunction> functions
                ? functions
                : Enumerable.Empty<object>();
    }
}