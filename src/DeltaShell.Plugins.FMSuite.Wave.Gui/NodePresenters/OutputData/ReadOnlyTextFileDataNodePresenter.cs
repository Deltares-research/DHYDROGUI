using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="ReadOnlyTextFileDataNodePresenter"/> implements the <see cref="TreeViewNodePresenterBaseForPluginGui{T}"/>
    /// for the <see cref="ReadOnlyTextFileData"/>.
    /// </summary>
    /// <seealso cref="TreeViewNodePresenterBaseForPluginGui{ReadOnlyTextFileData}" />
    public class ReadOnlyTextFileDataNodePresenter : TreeViewNodePresenterBaseForPluginGui<ReadOnlyTextFileData>
    {
        private static readonly Bitmap icon = Resources.DocumentHS;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ReadOnlyTextFileData nodeData)
        {
            node.Text = nodeData.DocumentName;
            node.Image = icon;
        }
    }
}