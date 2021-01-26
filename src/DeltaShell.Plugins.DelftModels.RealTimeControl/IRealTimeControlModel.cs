using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public interface IRealTimeControlModel : ITimeDependentModel, IWorkDirectoryModel, IEditableObject
    {
        IEventedList<ControlGroup> ControlGroups { get; set; }

        ICoordinateSystem CoordinateSystem { get; set; }

        IEnumerable<IModel> ControlledModels { get; } // TODO: rename to ControlledModels once RTC model will not be ICompositeModel
        /// <summary>
        /// Query connectable locations from controlled models.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        IEnumerable<IFeature> GetChildDataItemLocationsFromControlledModels(DataItemRole role);

        IEnumerable<IDataItem> GetChildDataItemsFromControlledModelsForLocation(IFeature feature);

        void ResetOrphanedControlGroupInputsAndOutputs(IControlGroup controlGroup);
    }
}