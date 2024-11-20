using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="ObservationPoint"/> data.
    /// </summary>
    public sealed class ObservationPointPropertyInfoCreationContext : IPropertyInfoCreationContext<ObservationPoint, ObservationPointProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(ObservationPointProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            properties.NameValidator.AddValidator(new UniqueNameValidator(properties.Data.HydroNetwork.ObservationPoints));
        }
    }
}