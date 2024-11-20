using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy
{
    /// <summary>
    /// Exists for backward compatibility with 3.0. Remove (+mappings) when no longer required.
    /// </summary>
    public class RtcLegacyLoader30 : LegacyLoader
    {
        private readonly LegacyLoader nextLegacyLoader = new RtcLegacyLoader36();
        private readonly IList<RealTimeControlModel> rtcModels = new List<RealTimeControlModel>();

        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            rtcModels.Add((RealTimeControlModel) entity);

            nextLegacyLoader.OnAfterInitialize(entity, dbConnection);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            foreach (RealTimeControlModel rtcModel in rtcModels)
            {
                //trigger some lazy loading
                project.RootFolder.GetDirectChildren();

                var oldOwner = rtcModel.Owner as Folder;

                if (oldOwner == null)
                {
                    throw new NotSupportedException("Model not in folder?!: " + rtcModel.Owner);
                }

                ITimeDependentModel flowModel = null;

                IDataItem linkedChildDataItem = rtcModel.AllDataItems.Where(di => di.LinkedBy.Count > 0 || di.LinkedTo != null)
                                                        .FirstOrDefault(di => di.Parent is DataItem);

                if (linkedChildDataItem != null)
                {
                    IDataItem otherSide = linkedChildDataItem.LinkedBy.FirstOrDefault() ?? linkedChildDataItem.LinkedTo;
                    flowModel = (ITimeDependentModel) otherSide.Parent.Owner;
                }
                else
                {
                    //rtc not linked to flow; no way to get the flow model (unless we directly access the db / session)
                    throw new InvalidOperationException(
                        "The RTC model in the legacy model has no items linked to the flow model; cannot continue." +
                        " Please work around this issue by creating a control group in the original model with one " +
                        " item (input or output) linked to flow. Then load the model again in this version of Delta Shell.");
                }

                // these are overwritten by hydro model :-(
                string name = flowModel.Name;
                DateTime startTime = flowModel.StartTime;
                DateTime stopTime = flowModel.StopTime;
                TimeSpan timestep = flowModel.TimeStep;

                // instantiate an empty hydro model
                var hydroModelType =
                    Type.GetType(
                        "DeltaShell.Plugins.DelftModels.HydroModel.HydroModel, DeltaShell.Plugins.DelftModels.HydroModel",
                        true);
                var hydroModel = (ICompositeActivity) Activator.CreateInstance(hydroModelType);

                // add rtc & flow as activities
                hydroModel.Activities.Add(rtcModel);
                hydroModel.Activities.Add(flowModel);

                // restore flow settings
                flowModel.Name = name;
                flowModel.StartTime = startTime;
                flowModel.StopTime = stopTime;
                flowModel.TimeStep = timestep;

                // remove orphaned flow data items
                List<IDataItem> orphanedDataItems = flowModel.AllDataItems.Where(
                                                                 di => di.LinkedTo == null &&
                                                                       !di.LinkedBy.Any() &&
                                                                       di.ValueConverter != null &&
                                                                       di.ValueConverter.GetEntityType().Name == "WaterFlowModelBranchFeatureValueConverter")
                                                             .ToList();

                foreach (IDataItem orphanedDataItem in orphanedDataItems)
                {
                    orphanedDataItem.Parent?.Children.Remove(orphanedDataItem);
                }

                var hydroModelAsTimeDependent = (ITimeDependentModel) hydroModel;
                hydroModelAsTimeDependent.StartTime = startTime;
                hydroModelAsTimeDependent.StopTime = stopTime;
                hydroModelAsTimeDependent.TimeStep = timestep;

                // put hydro model in folder instead of rtc
                oldOwner.Items.Remove(rtcModel);
                oldOwner.Items.Add(hydroModel);
            }

            nextLegacyLoader.OnAfterProjectMigrated(project);
        }
    }
}