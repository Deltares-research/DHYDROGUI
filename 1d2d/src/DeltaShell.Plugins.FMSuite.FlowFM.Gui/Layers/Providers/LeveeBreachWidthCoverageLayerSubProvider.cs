using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="LeveeBreachWidthCoverageLayerSubProvider"/> is responsible for creating
    /// the levee breach width coverage layers.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class LeveeBreachWidthCoverageLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="LeveeBreachWidthCoverageLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public LeveeBreachWidthCoverageLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is FeatureCoverage coverage &&
            IsCoverageLeveeBreachWidth(coverage);

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is FeatureCoverage coverage && IsCoverageLeveeBreachWidth(coverage)
                ? instanceCreator.CreateLeveeBreachWidthCoverageLayer(coverage)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            Enumerable.Empty<object>();

        private static bool IsCoverageLeveeBreachWidth(INameable featureCoverage) =>
            featureCoverage.Name == "dambreak breach width (dambreak_breach_width)";
    }
}