using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class DataTableManagerNodePresenter : TreeViewNodePresenterBase<DataTableManager>
    {
        private static readonly Bitmap DataTableManagerImage = Resources.DataTableManager;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, DataTableManager nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = DataTableManagerImage;
        }
    }
}