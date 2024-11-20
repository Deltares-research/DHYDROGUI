using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    internal class WindItemListNodePresenter : FMSuiteNodePresenterBase<IEventedList<IWindField>>
    {
        private static readonly Bitmap WindIcon = Resources.Wind1;

        public override IEnumerable GetChildNodeObjects(IEventedList<IWindField> parentNodeData, ITreeNode node)
        {
            return parentNodeData;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menu = base.GetContextMenu(sender, nodeData);
            var menuStrip = new ContextMenuStrip();
            var addItem = new ClonableToolStripMenuItem
            {
                Text = "Add...",
                Tag = nodeData,
                Image = Resources.Plus
            };
            addItem.Click += AddItemOnClick;
            menuStrip.Items.Add(addItem);
            menu.Insert(1, new MenuItemContextMenuStripAdapter(menuStrip));
            return menu;
        }

        protected override string GetNodeText(IEventedList<IWindField> data)
        {
            return "Wind";
        }

        protected override Image GetNodeImage(IEventedList<IWindField> data)
        {
            return WindIcon;
        }

        private void AddItemOnClick(object sender, EventArgs eventArgs)
        {
            var windItems = ((ToolStripMenuItem) sender).Tag as IEventedList<IWindField>;
            if (windItems == null)
            {
                return;
            }

            var dialog = new WindSelectionDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                IWindField windItem = dialog.WindField;
                if (windItem != null)
                {
                    windItems.Add(windItem);
                }
            }
        }
    }
}