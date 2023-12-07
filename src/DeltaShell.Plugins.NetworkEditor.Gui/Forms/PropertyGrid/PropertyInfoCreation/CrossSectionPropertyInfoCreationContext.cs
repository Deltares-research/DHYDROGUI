using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="ICrossSection"/> data.
    /// </summary>
    public sealed class CrossSectionPropertyInfoCreationContext : IPropertyInfoCreationContext<ICrossSection, CrossSectionProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(CrossSectionProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            properties.NameValidator.AddValidator(new UniqueNameValidator(properties.Data.HydroNetwork.CrossSections));
        }
    }
}