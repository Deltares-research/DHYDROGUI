using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="ICompositeBranchStructure"/> data.
    /// </summary>
    public sealed class CompositeStructurePropertyInfoCreationContext : IPropertyInfoCreationContext<ICompositeBranchStructure, CompositeStructureProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(CompositeStructureProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            properties.NameValidator.AddValidator(new UniqueNameValidator(properties.Data.HydroNetwork.CompositeBranchStructures));
        }
    }
}