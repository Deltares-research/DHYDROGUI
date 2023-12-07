using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="ICulvert"/> data.
    /// </summary>
    public sealed class CulvertPropertyInfoCreationContext : IPropertyInfoCreationContext<ICulvert, CulvertProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(CulvertProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            properties.NameValidator.AddValidator(new UniqueNameValidator(properties.Data.HydroNetwork.Culverts));
        }
    }
}