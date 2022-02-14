using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ImportedFMNetFileLayerSubProvider"/> is responsible for creating
    /// the layers out of <see cref="ImportedFMNetFile"/> objects.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class ImportedFMNetFileLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="ImportedFMNetFileLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public ImportedFMNetFileLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is ImportedFMNetFile;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is ImportedFMNetFile netFile ? instanceCreator.CreateImportedFMNetFileLayer(netFile) : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) => Enumerable.Empty<object>();
    }
}