using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="IRetention"/> data.
    /// </summary>
    public class RetentionTableViewCreationContext : ITableViewCreationContext<IRetention, RetentionRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Retention table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IRetention> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Retentions, data);
        }

        /// <inheritdoc/>
        public RetentionRow CreateFeatureRowObject(IRetention feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new RetentionRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IRetention> data, GuiContainer guiContainer)
        {
            string storageName = typeof(RetentionRow).GetProperty(nameof(RetentionRow.Data))?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            ITableViewColumn column = view.TableView.Columns.FirstOrDefault(c => string.Equals(c.Caption, storageName, StringComparison.InvariantCultureIgnoreCase));
            if (column == null)
            {
                return;
            }

            column.Editor = new ButtonTypeEditor
            {
                Name = "ViewStorageTable",
                Caption = "...",
                HideOnReadOnly = true,
                Tooltip = storageName,
                ButtonClickAction = () =>
                {
                    if (view.TableView.CurrentFocusedRowObject is RetentionRow retention)
                    {
                        guiContainer.Gui.CommandHandler.OpenView(retention.Data);
                    }
                }
            };
        }
    }
}