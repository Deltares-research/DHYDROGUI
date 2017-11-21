using System;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    public class SewerConnection : Branch, ISewerConnection
    {
        #region Constructors

        public SewerConnection() : this(null, null)
        {
        }
        public SewerConnection(string name)
            : this(name, null, null, 0)
        {
        }
        public SewerConnection(INode fromNode, INode toNode)
            : this("sewerConnection", fromNode, toNode, 0)
        {
        }

        public SewerConnection(INode fromNode, INode toNode, double length)
            : this("sewerConnection", fromNode, toNode, length)
        {
        }

        public SewerConnection(string name, INode fromNode, INode toNode, double length) :
            base(name, fromNode, toNode, length)
        {
            branchFeatures = new EventedList<IBranchFeature>();
        }

        #endregion

        #region SewerConnection specific

        public string ConnectionId { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        protected IEventedList<IBranchFeature> branchFeatures;
        private Compartment sourceCompartment;
        private Compartment targetCompartment;
        public SewerConnectionType SewerConnectionType { get; set; }
        public SewerConnectionWaterType WaterType { get; set; }

        public Compartment SourceCompartment
        {
            get { return sourceCompartment; }
            set
            {
                sourceCompartment = value;
                Source = null;
                if (sourceCompartment != null)
                {
                    Source = sourceCompartment.ParentManhole;
                }
            }
        }

        public Compartment TargetCompartment
        {
            get { return targetCompartment; }
            set
            {
                targetCompartment = value;
                Target = null;
                if (targetCompartment != null)
                {
                    Target = targetCompartment.ParentManhole;
                }
            }
        }

        #endregion

        public override bool IsLengthCustom
        {
            get { return true; }
        }


        public override IEventedList<IBranchFeature> BranchFeatures
        {
            get { return branchFeatures; }
            set
            {
                if (branchFeatures != null)
                {
                    branchFeatures.CollectionChanging -= BranchFeaturesOnCollectionChanging;
                }

                //For the sewer connection we only allow one branch feature per sewer connection
                if (value != null && value.Count == 1)
                {
                    branchFeatures = value;
                    branchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
                }
                else
                {
                    //exception ??
                }
            }
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs notifyCollectionChangingEventArgs)
        {
            if (notifyCollectionChangingEventArgs.Action != NotifyCollectionChangeAction.Add) return;

            if (branchFeatures.Count != 0)
            {
                //exception ??
                notifyCollectionChangingEventArgs.Cancel = true;
            }
        }
    }
}
