using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    class FmMeteoItemNodePresenter : FMSuiteNodePresenterBase<IFmMeteoField>
    {
        private static readonly Bitmap PrecipitationImage = Resources.precipitation;

        protected override string GetNodeText(IFmMeteoField data)
        {
            return data.Name;
        }
        protected override Image GetNodeImage(IFmMeteoField data)
        {
            if (data is IFmMeteoField)
            {
                return PrecipitationImage;
            }

            return null;
        }

        protected override bool RemoveNodeData(object parentNodeData, IFmMeteoField nodeData)
        {
            var fmMeteoField = parentNodeData as IEventedList<IFmMeteoField>;
            if (fmMeteoField != null)
            {
                fmMeteoField.Remove(nodeData);
                return true;
            }
            return false;
        }

        protected override bool CanRemove(IFmMeteoField nodeData)
        {
            return true;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);
            var menuStrip = new ContextMenuStrip();
            var deleteItem = new ClonableToolStripMenuItem
            {
                Text = "Delete",
                Tag = nodeData,
                Image = Resources.DeleteHS
            };
            deleteItem.Click += DeleteItem;
            menuStrip.Items.Add(deleteItem);
            menu.Insert(1, new MenuItemContextMenuStripAdapter(menuStrip));
            return menu;
        }

        private void DeleteItem(object sender, EventArgs e)
        {
            var data = ((ToolStripMenuItem)sender).Tag as IFmMeteoField;
            if (data != null)
            {
                var list =
                    GuiPlugin.Gui.Application.GetAllModelsInProject()
                        .OfType<WaterFlowFMModel>()
                        .First(m => m.FmMeteoFields.Contains(data))
                        .FmMeteoFields;

                list.Remove(data);
            }
        }
    }
}
