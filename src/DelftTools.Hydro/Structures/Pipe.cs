using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Properties;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DelftTools.Hydro.Structures
{
    [Serializable]
    public class Pipe : SewerConnection, IPipe
    {
        private static ILog Log = LogManager.GetLogger(typeof(Pipe));
        public CrossSection SewerProfile { get; set; }
        public string PipeId { get; set; }

        public override IEventedList<IBranchFeature> BranchFeatures
        {
            get { return branchFeatures; }
            set
            {
                if (branchFeatures != null)
                {
                    branchFeatures.CollectionChanging -= BranchFeaturesOnCollectionChanging;
                }

                if (value != null)
                {
                    if (value.Count > 0)
                    {
                        Log.ErrorFormat(Resources.Pipe_BranchFeaturesOnCollectionChanging_Pipe__0__does_not_allow_any_branch_feature_on_it_, Name);
                    }
                    else
                    {
                        branchFeatures = value;
                    }
                }

                if (branchFeatures != null)
                {
                    branchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
                }

            }
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs notifyCollectionChangingEventArgs)
        {
            if (notifyCollectionChangingEventArgs.Action != NotifyCollectionChangeAction.Add) return;

            notifyCollectionChangingEventArgs.Cancel = true;
            Log.ErrorFormat(Resources.Pipe_BranchFeaturesOnCollectionChanging_Pipe__0__does_not_allow_any_branch_feature_on_it_, Name);
        }
    }
}