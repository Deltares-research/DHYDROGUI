using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.Common.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public interface IRealTimeControlModel : IRestartModel<RealTimeControlRestartFile>, ITimeDependentModel, IEditableObject
    {
        IEventedList<ControlGroup> ControlGroups { get; set; }

        ICoordinateSystem CoordinateSystem { get; set; }

        IEnumerable<IModel> ControlledModels { get; }

        DateTime SaveStateStartTime { get; set; }
        TimeSpan SaveStateTimeStep { get; set; }
        DateTime SaveStateStopTime { get; set; }

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