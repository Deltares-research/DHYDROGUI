using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    internal class WindItemNodePresenter : FMSuiteNodePresenterBase<IWindField>
    {
        private static readonly Bitmap UniformWindImage = Resources.TimeSeries;
        private static readonly Bitmap GriddedWindImage = Resources.FunctionGrid2D;
        private static readonly Bitmap SpiderWebWindImage = Resources.hurricane2;

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menu = base.GetContextMenu(sender, nodeData);
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

        protected override string GetNodeText(IWindField data)
        {
            return data.Name;
        }

        protected override Image GetNodeImage(IWindField data)
        {
            if (data is UniformWindField)
            {
                return UniformWindImage;
            }

            if (data is GriddedWindField)
            {
                return GriddedWindImage;
            }

            if (data is SpiderWebWindField)
            {
                return SpiderWebWindImage;
            }

            return null;
        }

        protected override bool RemoveNodeData(object parentNodeData, IWindField nodeData)
        {
            var windFields = parentNodeData as IEventedList<IWindField>;
            if (windFields != null)
            {
                windFields.Remove(nodeData);
                return true;
            }

            return false;
        }

        protected override bool CanRemove(IWindField nodeData)
        {
            return true;
        }

        private void DeleteItem(object sender, EventArgs e)
        {
            var data = ((ToolStripMenuItem) sender).Tag as IWindField;
            if (data != null)
            {
                IEventedList<IWindField> list =
                    GuiPlugin.Gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive()
                             .OfType<WaterFlowFMModel>()
                             .First(m => m.WindFields.Contains(data))
                             .WindFields;

                list.Remove(data);
            }
        }
    }
}