using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="Weir2D"/> data.
    /// </summary>
    public class FMWeirPropertyInfoCreationContext : IPropertyInfoCreationContext<Weir2D, FMWeirProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(FMWeirProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            HydroArea hydroArea = GetHydroArea(guiContainer, properties.Data);
            properties.NameValidator.AddValidator(new UniqueNameValidator(hydroArea.Weirs));
        }

        private static HydroArea GetHydroArea(GuiContainer guiContainer, Weir2D feature)
        {
            IEnumerable<object> projectItems = guiContainer.Gui.Application.ProjectService.Project.GetAllItemsRecursive();
            return projectItems.OfType<HydroArea>().FirstOrDefault(c => IsHydroAreaData(c, feature));
        }

        private static bool IsHydroAreaData(HydroArea container, Weir2D feature)
        {
            return container.Weirs.Contains(feature);
        }
    }
}