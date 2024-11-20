using System.Collections;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="WavhFileFunctionStoreNodePresenter"/> implements the
    /// <see cref="WaveFileFunctionStoreNodePresenterBase{T}"/> for
    /// <see cref="WavhFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreNodePresenterBase{WavhFileFunctionStore}" />
    public sealed class WavhFileFunctionStoreNodePresenter : WaveFileFunctionStoreNodePresenterBase<IWavhFileFunctionStore>
    {
        protected override bool IsContainedInModel(IWavhFileFunctionStore nodeData, IWaveModel model) =>
            model.WaveOutputData.WavhFileFunctionStores.Contains(nodeData);

        public override IEnumerable GetChildNodeObjects(IWavhFileFunctionStore parentNodeData, ITreeNode node)
        {
            foreach (object baseChild in base.GetChildNodeObjects(parentNodeData, node))
            {
                yield return baseChild;
            }

            foreach (object childCoverage in GetChildCoverages(parentNodeData))
            {
                yield return childCoverage;
            }
        }
    }
}