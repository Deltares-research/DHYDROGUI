using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataNodePresenter"/> implements the node presenter
    /// for the <see cref="IWaveOutputData"/>.
    /// </summary>
    public class WaveOutputDataNodePresenter : TreeViewNodePresenterBaseForPluginGui<IWaveOutputData>
    {
        private static readonly Bitmap outputFolderImage = Resources.folder_output;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IWaveOutputData nodeData)
        {
            node.Text = Properties.Resources.WaveOutputDataNodePresenter_Output;
            node.Image = outputFolderImage;
        }

        public override IEnumerable GetChildNodeObjects(IWaveOutputData parentNodeData, ITreeNode node)
        {
            // TODO: Child objects should go in here
            // note that the creation of sub folders should be dependent on
            // whether there exists actual data in these folders.
            foreach (ReadOnlyTextFileData readOnlyTextFileData in parentNodeData.DiagnosticFiles)
            {
                yield return readOnlyTextFileData;
            }

            if (parentNodeData.SpectraFiles.Count > 0)
            {
                yield return new TreeFolder(parentNodeData,
                                            parentNodeData.SpectraFiles,
                                            Properties.Resources.WaveOutputDataNodePresenter_Spectra,
                                            FolderImageType.None);
            }
        }
    }
}