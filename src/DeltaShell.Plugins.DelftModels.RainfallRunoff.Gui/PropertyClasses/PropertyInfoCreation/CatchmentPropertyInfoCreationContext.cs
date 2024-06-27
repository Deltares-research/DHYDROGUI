using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="Catchment"/> data.
    /// </summary>
    public sealed class CatchmentPropertyInfoCreationContext : IPropertyInfoCreationContext<Catchment, CatchmentProperties>
    {
        public void CustomizeProperties(CatchmentProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            properties.CatchmentData = GetCatchmentData(properties, guiContainer);
            properties.NameValidator.AddValidator(new UniqueNameValidator(properties.Data.Basin.Catchments));
        }

        private static CatchmentModelData GetCatchmentData(CatchmentProperties properties, GuiContainer guiContainer)
        {
            return guiContainer.Gui.Application.GetAllModelsInProject()
                               .OfType<IRainfallRunoffModel>()
                               .SelectMany(m => m.ModelData)
                               .FirstOrDefault(d => ReferenceEquals(d.Catchment, properties.Data));
        }
    }
}