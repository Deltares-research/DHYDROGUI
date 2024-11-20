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
    /// <summary>
    /// Class responsible for providing the Meteo items list node
    /// </summary>
    /// <seealso cref="IFmMeteoField" />
    class FmMeteoItemListNodePresenter : FMSuiteNodePresenterBase<IEventedList<IFmMeteoField>>
    {
        private static readonly Bitmap MeteoItemsIcon = Resources.weather_cloudy;

        protected override string GetNodeText(IEventedList<IFmMeteoField> data)
        {
            return "Meteo items";
        }

        protected override Image GetNodeImage(IEventedList<IFmMeteoField> data)
        {
            return MeteoItemsIcon;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<IFmMeteoField> parentNodeData, ITreeNode node)
        {
            return parentNodeData;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);
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

        private void AddItemOnClick(object sender, EventArgs eventArgs)
        {
            var fmMeteoItems = ((ToolStripMenuItem)sender).Tag as IEventedList<IFmMeteoField>;
            if (fmMeteoItems == null) return;
            var dialog = new FmMeteoSelectionDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fmMeteoField = dialog.FmMeteoField;
                if (fmMeteoField != null)
                {
                    fmMeteoItems.Add(fmMeteoField);
                }
            }
        }
    }
}
