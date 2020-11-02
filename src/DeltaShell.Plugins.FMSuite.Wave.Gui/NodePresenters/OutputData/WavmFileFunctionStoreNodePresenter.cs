using System.Collections;
using System.Linq;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="WavmFileFunctionStore"/> implements the <see cref="WaveFileFunctionStoreNodePresenter{T}"/>
    /// for <see cref="WavmFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreNodePresenter{WavmFileFunctionStore}" />
    public sealed class WavmFileFunctionStoreNodePresenter : WaveFileFunctionStoreNodePresenter<WavmFileFunctionStore>
    {
        protected override bool IsContainedInModel(WavmFileFunctionStore nodeData, IWaveModel model) =>
            model.WaveOutputData.WavmFileFunctionStores.Contains(nodeData);

        public override IEnumerable GetChildNodeObjects(WavmFileFunctionStore parentNodeData, ITreeNode node)
        {
            foreach (object baseChild in base.GetChildNodeObjects(parentNodeData, node))
            {
                yield return baseChild;
            }

            if (IsStandAloneFunctionStore(parentNodeData))
            {
                yield return WrapIntoOutputItem(parentNodeData.Grid, parentNodeData, "grid");
            }

            foreach (object childCoverage in GetChildCoverages(parentNodeData))
            {
                yield return childCoverage;
            }
        }
    }
}