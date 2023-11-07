using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="IPipe"/> data.
    /// </summary>
    public class PipeTableViewCreationContext : ITableViewCreationContext<IPipe, PipeRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription() => "Pipe table view";

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IPipe> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Pipes, data);
        }

        /// <inheritdoc/>
        public PipeRow CreateFeatureRowObject(IPipe feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new PipeRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IPipe> data, GuiContainer guiContainer)
        {
            IModelWithNetwork networkModel = guiContainer.Gui.Application.GetAllModelsInProject().OfType<IModelWithNetwork>().FirstOrDefault(m => m.Network.Pipes.Equals(data));
            SetSharedCrossSectionDefinitionsComboBoxTypeEditor(view, networkModel);

            view.TableView.FocusedRowChanged += (sender, args) => { SetSharedCrossSectionDefinitionsComboBoxTypeEditor(view, networkModel); };
        }

        private void SetSharedCrossSectionDefinitionsComboBoxTypeEditor(VectorLayerAttributeTableView view, IModelWithNetwork networkModel)
        {
            var pipe = view.TableView.CurrentFocusedRowObject as PipeRow;
            if (pipe == null)
            {
                return;
            }

            string columnDisplayName = typeof(PipeRow).GetProperty(nameof(PipeRow.DefinitionName))?.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault()?.DisplayName;
            if (columnDisplayName == null)
            {
                return;
            }

            ITableViewColumn column = view.TableView.Columns.FirstOrDefault(c => c.Caption.Equals(columnDisplayName, StringComparison.InvariantCultureIgnoreCase));
            if (column != null)
            {
                column.Editor = new ComboBoxTypeEditor
                {
                    Items = networkModel.Network.SharedCrossSectionDefinitions.Select(d => d.Name),
                    ItemsMandatory = false
                };
            }
        }
    }
}