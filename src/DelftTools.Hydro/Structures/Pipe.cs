using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    [Serializable]
    public class Pipe : SewerConnection, IPipe
    {
        public CrossSection CrossSectionShape { get; set; }
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

                branchFeatures = value;

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
            //Log exception // whatever
        }
    }
}