using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="IGate"/> data.
    /// </summary>
    public sealed class GatePropertyInfoCreationContext : IPropertyInfoCreationContext<IGate, GateProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(GateProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            IEnumerable<INameable> gates = GetGates(properties.Data, guiContainer);
            properties.NameValidator.AddValidator(new UniqueNameValidator(gates));
        }

        private static IEnumerable<INameable> GetGates(IGate gate, GuiContainer guiContainer)
        {
            // 1D gate
            if (gate.HydroNetwork != null)
            {
                return gate.HydroNetwork.Gates;
            }

            // 2D gate
            IEnumerable<HydroArea> hydroAreas = guiContainer.Gui.Application.ProjectService.Project.GetAllItemsRecursive().OfType<HydroArea>();
            return hydroAreas.First(region => region.Gates.Contains(gate)).Gates;
        }
    }
}