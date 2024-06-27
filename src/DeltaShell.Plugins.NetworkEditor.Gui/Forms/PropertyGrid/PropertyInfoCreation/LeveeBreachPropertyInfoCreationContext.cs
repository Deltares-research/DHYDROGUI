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
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="ILeveeBreach"/> data.
    /// </summary>
    public sealed class LeveeBreachPropertyInfoCreationContext : IPropertyInfoCreationContext<ILeveeBreach, LeveeBreachProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(LeveeBreachProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            HydroArea hydroArea = GetHydroArea(guiContainer, properties.Data);
            properties.NameValidator.AddValidator(new UniqueNameValidator(hydroArea.LeveeBreaches));
        }

        private static HydroArea GetHydroArea(GuiContainer guiContainer, ILeveeBreach feature)
        {
            IEnumerable<object> projectItems = guiContainer.Gui.Application.Project.GetAllItemsRecursive();
            return projectItems.OfType<HydroArea>().FirstOrDefault(c => IsContainerData(c, feature));
        }

        private static bool IsContainerData(HydroArea container, ILeveeBreach feature)
        {
            return container.LeveeBreaches.OfType<ILeveeBreach>().Contains(feature);
        }
    }
}