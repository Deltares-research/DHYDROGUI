using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="OutletCompartment"/> data.
    /// </summary>
    public class OutletCompartmentTableViewCreationContext : ITableViewCreationContext<OutletCompartment, OutletCompartmentRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Outlet compartment table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<OutletCompartment> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.OutletCompartments, data);
        }

        /// <inheritdoc/>
        public OutletCompartmentRow CreateFeatureRowObject(OutletCompartment feature, IEnumerable<OutletCompartment> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new OutletCompartmentRow(feature, nameValidator);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<OutletCompartment> data, GuiContainer guiContainer)
        {
            string storageName = typeof(OutletCompartmentRow).GetProperty(nameof(OutletCompartmentRow.Storage))?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
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
                    if (view.TableView.CurrentFocusedRowObject is OutletCompartmentRow compartment)
                    {
                        guiContainer.Gui.CommandHandler.OpenView(compartment.Storage);
                    }
                }
            };
        }
    }
}