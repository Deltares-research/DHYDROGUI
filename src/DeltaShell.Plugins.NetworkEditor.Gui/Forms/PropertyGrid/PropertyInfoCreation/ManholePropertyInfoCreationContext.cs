using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="Manhole"/> data.
    /// </summary>
    public sealed class ManholePropertyInfoCreationContext : IPropertyInfoCreationContext<Manhole, ManholeProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(ManholeProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            IHydroNetwork hydroNetwork = properties.Data.HydroNetwork;
            properties.ManholeNameValidator.AddValidator(new UniqueNameValidator(hydroNetwork.Manholes));
            properties.CompartmentNameValidator.AddValidator(new UniqueNameValidator(hydroNetwork.Compartments));
        }
    }
}