using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataNodePresenter"/> implements the node presenter
    /// for the <see cref="IWaveOutputData"/>.
    /// </summary>
    /// <seealso cref="TreeViewNodePresenterBaseForPluginGui{IWaveOutputData}" />
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
            foreach (ReadOnlyTextFileData readOnlyTextFileData in parentNodeData.DiagnosticFiles)
            {
                yield return readOnlyTextFileData;
            }

            if (parentNodeData.SpectraFiles.Any())
            {
                yield return new TreeFolder(parentNodeData,
                                            parentNodeData.SpectraFiles,
                                            Properties.Resources.WaveOutputDataNodePresenter_Spectra,
                                            FolderImageType.None);
            }

            if (parentNodeData.WavmFileFunctionStores.Any(x => x.Functions.Any()))
            {
                yield return new TreeFolder(parentNodeData, 
                                            parentNodeData.WavmFileFunctionStores.Where(x => x.Functions.Any()),
                                            Properties.Resources.WaveOutputDataNodePresenter_Map_Files,
                                            FolderImageType.None);
            }

            if (parentNodeData.WavhFileFunctionStores.Any(x => x.Functions.Any()))
            {
                yield return new TreeFolder(parentNodeData, 
                                            parentNodeData.WavhFileFunctionStores.Where(x => x.Functions.Any()),
                                            Properties.Resources.WaveOutputDataNodePresenter_His_Files,
                                            FolderImageType.None);
            }
            
            if (parentNodeData.SwanFiles.Any())
            {
                yield return new TreeFolder(parentNodeData,
                                            parentNodeData.SwanFiles,
                                            Properties.Resources.WaveOutputDataNodePresenter_Swan_Input_Files,
                                            FolderImageType.None);
            }
        }
    }
}