using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    public abstract class GroupableFeatureTableViewCreationContext<TGroupableFeature, TFeatureRow> : ITableViewCreationContext<TGroupableFeature, TFeatureRow, HydroArea> where TGroupableFeature : IGroupableFeature
                                                                                                                                                                          where TFeatureRow : class, IFeatureRowObject
    {
        /// <inheritdoc/>
        public abstract string GetDescription();

        /// <inheritdoc/>
        public abstract bool IsRegionData(HydroArea region, IEnumerable<TGroupableFeature> data);

        /// <inheritdoc/>
        public abstract TFeatureRow CreateFeatureRowObject(TGroupableFeature feature);

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<TGroupableFeature> data, GuiContainer guiContainer)
        {
            Ensure.NotNull(view, nameof(view));
            Ensure.NotNull(data, nameof(data));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            CustomizeContextMenu((IEventedList<TGroupableFeature>)data, view.TableView.RowContextMenu, guiContainer);
        }

        private void CustomizeContextMenu(IEventedList<TGroupableFeature> data, ContextMenuStrip contextMenu, GuiContainer guiContainer)
        {
            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem addGroupContextMenuItem = GetAddGroupContextMenuItem(guiContainer, data);
            contextMenu.Items.Add(addGroupContextMenuItem);
            contextMenu.Opening += (f, v) => UpdateVisibilityAddGroupContextMenuItem(addGroupContextMenuItem, data, guiContainer);

            ToolStripMenuItem removeGroupContextMenuItem = GetRemoveGroupContextMenuItem(data);
            contextMenu.Items.Add(removeGroupContextMenuItem);
            contextMenu.Opening += (f, v) => UpdateVisibilityRemoveGroupContextMenuItem(removeGroupContextMenuItem, data);

            ToolStripMenuItem removeUngroupedItemsContextMenuItem = GetRemoveUngroupedItemsContextMenuItem(data);
            contextMenu.Items.Add(removeUngroupedItemsContextMenuItem);
            contextMenu.Opening += (f, v) => UpdateVisibilityRemoveUngroupedItemsContextMenuItem(removeUngroupedItemsContextMenuItem, data);
        }

        private void UpdateVisibilityAddGroupContextMenuItem(ToolStripMenuItem contextMenuItem, IEventedList<TGroupableFeature> data, GuiContainer guiContainer)
        {
            contextMenuItem.Enabled = guiContainer.Gui.CommandHandler.CanImportOn(data);
        }

        private void UpdateVisibilityRemoveGroupContextMenuItem(ToolStripMenuItem contextMenuItem, IEventedList<TGroupableFeature> data)
        {
            contextMenuItem.Visible = data.Any(g => !string.IsNullOrWhiteSpace(g.GroupName));
        }

        private void UpdateVisibilityRemoveUngroupedItemsContextMenuItem(ToolStripMenuItem contextMenuItem, IEventedList<TGroupableFeature> data)
        {
            contextMenuItem.Visible = data.Any(f => string.IsNullOrWhiteSpace(f.GroupName));
        }

        private static ToolStripMenuItem GetRemoveUngroupedItemsContextMenuItem(IEventedList<TGroupableFeature> data)
        {
            var contextMenuItem = new ToolStripMenuItem
            {
                Text = Resources.NetworkEditorGuiPlugin_CreateAddRemoveContextMenu_Remove_ungrouped,
            };
            contextMenuItem.Click += (s, e) => data.RemoveUngroupedItems();
            return contextMenuItem;
        }

        private static ToolStripMenuItem GetRemoveGroupContextMenuItem(IEventedList<TGroupableFeature> data)
        {
            var contextMenuItem = new ToolStripMenuItem
            {
                Text = Resources.NetworkEditorGuiPlugin_CreateAreaStructureCollectionViewInfo_Remove_group,
            };

            contextMenuItem.DropDownOpening += (sender, args) =>
            {
                contextMenuItem.DropDownItems.Clear();

                IEnumerable<string> groupNames = data.Select(g => g.GroupName).Distinct().Where(name => !string.IsNullOrWhiteSpace(name));
                foreach (string groupName in groupNames)
                {
                    var groupMenuItem = new ToolStripMenuItem
                    {
                        Text = groupName,
                    };

                    groupMenuItem.Click += (s, e) => data.RemoveGroup(groupName);
                    contextMenuItem.DropDownItems.Add(groupMenuItem);
                }
            };
            return contextMenuItem;
        }

        private static ToolStripMenuItem GetAddGroupContextMenuItem(GuiContainer guiContainer, IEventedList<TGroupableFeature> data)
        {
            var contextMenuItem = new ToolStripMenuItem
            {
                Text = Resources.NetworkEditorGuiPlugin_GetViewInfoForHydroAreaFeatureCollection_Add_group,
            };
            contextMenuItem.Click += (s, e) => guiContainer.Gui.CommandHandler.ImportOn(data);
            return contextMenuItem;
        }
    }
}