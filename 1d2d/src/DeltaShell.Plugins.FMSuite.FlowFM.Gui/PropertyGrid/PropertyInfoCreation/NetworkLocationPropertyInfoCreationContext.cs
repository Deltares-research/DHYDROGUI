using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/> that should create the
    /// <see cref="DelftTools.Shell.Gui.PropertyInfo"/> for <see cref="INetworkLocation"/> data.
    /// </summary>
    public sealed class NetworkLocationPropertyInfoCreationContext : IPropertyInfoCreationContext<INetworkLocation, NetworkLocationProperties>
    {
        /// <inheritdoc/>
        public void CustomizeProperties(NetworkLocationProperties properties, GuiContainer guiContainer)
        {
            Ensure.NotNull(properties, nameof(properties));
            Ensure.NotNull(guiContainer, nameof(guiContainer));

            WaterFlowFMModel waterFlowFMModel = GetWaterFlowFMModel(guiContainer, properties.Data);
            properties.NameValidator.AddValidator(new UniqueNameValidator(waterFlowFMModel.NetworkDiscretization.Locations.Values));
        }

        private static WaterFlowFMModel GetWaterFlowFMModel(GuiContainer guiContainer, INetworkLocation feature)
        {
            IEnumerable<WaterFlowFMModel> models = guiContainer.Gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>();
            return models.FirstOrDefault(m => IsWaterFlowFMModelData(m, feature));
        }

        private static bool IsWaterFlowFMModelData(WaterFlowFMModel container, INetworkLocation feature)
        {
            return container.NetworkDiscretization.Locations.Values.Contains(feature);
        }
    }
}