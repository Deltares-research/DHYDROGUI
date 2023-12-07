using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="HydroLink"/> data.
    /// </summary>
    public sealed class HydroLinkPropertyInfoCreationContext : IPropertyInfoCreationContext<HydroLink, HydroLinkProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(HydroLinkProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            IEnumerable<IHydroRegion> hydroRegions = guiContainer.Gui.Application.Project.GetAllItemsRecursive().OfType<IHydroRegion>();
            IHydroRegion hydroRegionWithLink = hydroRegions.First(region => region.Links.Contains(properties.Data));
            properties.NameValidator.AddValidator(new UniqueNameValidator(hydroRegionWithLink.Links));
        }
    }
}