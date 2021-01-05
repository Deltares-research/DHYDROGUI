using System.Collections;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="WavmFileFunctionStoreNodePresenter"/> implements the
    /// <see cref="WaveFileFunctionStoreNodePresenterBase{T}"/> for
    /// <see cref="WavmFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreNodePresenterBase{T}" />
    public sealed class WavmFileFunctionStoreNodePresenter : WaveFileFunctionStoreNodePresenterBase<IWavmFileFunctionStore>
    {
        protected override bool IsContainedInModel(IWavmFileFunctionStore nodeData, IWaveModel model) =>
            model.WaveOutputData.WavmFileFunctionStores.Contains(nodeData);

        public override IEnumerable GetChildNodeObjects(IWavmFileFunctionStore parentNodeData, ITreeNode node)
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