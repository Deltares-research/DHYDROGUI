using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Data;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class CompositeHydroModelWorkFlowData : Unique<long>, IHydroModelWorkFlowData
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompositeHydroModelWorkFlowData));

        // used for nHibernate persistence -> int list is saved as string (the sequence of levelIndices is always unique).
        private IDictionary<IHydroModelWorkFlowData, string> hydroModelWorkFlowDataLookUp;

        // this is needed because nhibernate does not support cascade saving of dictionary keys
        private IList<IHydroModelWorkFlowData> workFlowDatas;

        public IDictionary<IHydroModelWorkFlowData, IList<int>> HydroModelWorkFlowDataLookUp
        {
            get
            {
                return hydroModelWorkFlowDataLookUp.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IList<int>)
                            kvp.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => Convert.ToInt32(s))
                                .ToList());
            }
            set
            {
                hydroModelWorkFlowDataLookUp = value != null
                    ? value.ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value) )
                    : null;

                workFlowDatas = value?.Keys.ToList();
            }
        }

        public IEnumerable<IDataItem> OutputDataItems
        {
            get { return hydroModelWorkFlowDataLookUp.Keys.SelectMany(d => d.OutputDataItems); }
        }

        public void TryRestoreData(ICompositeActivity currentWorkflow)
        {
            if (HydroModelWorkFlowDataLookUp == null) return;

            foreach (var workFlowDataKvp in HydroModelWorkFlowDataLookUp)
            {
                var activity = GetActivityForIndices(currentWorkflow, workFlowDataKvp.Value);
                if (activity != null)
                {
                    activity.Data = workFlowDataKvp.Key;
                }
            }
        }

        private IHydroModelWorkFlow GetActivityForIndices(IActivity currentWorkflow, IList<int> indices)
        {
            var currentActivity = currentWorkflow;

            foreach (var index in indices)
            {
                var compositeActivity = currentActivity as ICompositeActivity;
                if (compositeActivity == null)
                {
                    log.WarnFormat("Can not find sub activity {0} for {1}", index, currentActivity.Name);
                    return null;
                }

                currentActivity = compositeActivity.Activities[index];
            }

            return currentActivity as IHydroModelWorkFlow;
        }
    }
}