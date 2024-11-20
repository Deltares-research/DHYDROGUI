using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="ISewerConnection"/> data.
    /// </summary>
    public class SewerConnectionTableViewCreationContext : ITableViewCreationContext<ISewerConnection, SewerConnectionRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Sewer connection table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<ISewerConnection> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.SewerConnections, data);
        }

        /// <inheritdoc/>
        public SewerConnectionRow CreateFeatureRowObject(ISewerConnection feature, IEnumerable<ISewerConnection> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new SewerConnectionRow(feature, nameValidator);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<ISewerConnection> data, GuiContainer guiContainer)
        {
            IModelWithNetwork networkModel = guiContainer.Gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<IModelWithNetwork>().FirstOrDefault(m => m.Network.SewerConnections.Equals(data));
            SetSharedCrossSectionDefinitionsComboBoxTypeEditor(view, networkModel);

            view.TableView.FocusedRowChanged += (sender, args) => { SetSharedCrossSectionDefinitionsComboBoxTypeEditor(view, networkModel); };
        }

        private void SetSharedCrossSectionDefinitionsComboBoxTypeEditor(VectorLayerAttributeTableView view, IModelWithNetwork networkModel)
        {
            var sewerConnectionRow = view.TableView.CurrentFocusedRowObject as SewerConnectionRow;
            if (sewerConnectionRow == null)
            {
                return;
            }

            string columnDisplayName = typeof(SewerConnectionRow).GetProperty(nameof(SewerConnectionRow.DefinitionName))?.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault()?.DisplayName;
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