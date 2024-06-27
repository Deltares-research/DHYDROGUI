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
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="IPump"/> data.
    /// </summary>
    public sealed class PumpPropertyInfoCreationContext : IPropertyInfoCreationContext<IPump, PumpProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(PumpProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            IEnumerable<INameable> pumps = GetPumps(properties.Data, guiContainer);
            properties.NameValidator.AddValidator(new UniqueNameValidator(pumps));
        }

        private static IEnumerable<INameable> GetPumps(IPump pump, GuiContainer guiContainer)
        {
            // 1D pump
            if (pump.HydroNetwork != null)
            {
                return pump.HydroNetwork.Pumps;
            }

            // 2D pump
            IEnumerable<HydroArea> hydroAreas = guiContainer.Gui.Application.Project.GetAllItemsRecursive().OfType<HydroArea>();
            return hydroAreas.First(region => region.Pumps.Contains(pump)).Pumps;
        }
    }
}