using System;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity]
    public class SewerConnection : Branch, ISewerConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SewerConnection));

        protected IEventedList<IBranchFeature> branchFeatures;
        private Compartment sourceCompartment;
        private Compartment targetCompartment;

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
            if (fromNode?.Geometry == null || toNode?.Geometry == null) return;

            if (fromNode.Geometry.IsValid && toNode.Geometry.IsValid)
            {
                Geometry = new LineString(new[] { fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate });
            }
        }

        public double LevelSource { get; set; }

        public double LevelTarget { get; set; }
        
        public SewerConnectionWaterType WaterType { get; set; }

        // This property is used in the NetworkLayerStyleFactory, do not remove :)
        public SewerConnectionSpecialConnectionType SpecialConnectionType { get { return GetConnectionType(); } }

        public Compartment SourceCompartment
        {
            get { return sourceCompartment; }
            set
            {
                sourceCompartment = value;
                UpdateSource(sourceCompartment);
            }
        }

        public Compartment TargetCompartment
        {
            get { return targetCompartment; }
            set
            {
                targetCompartment = value;
                UpdateTarget(targetCompartment);
            }
        }

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
                if (value != null && value.Count <= 1)
                {
                    branchFeatures = value;
                    branchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
                }
                else
                {
                    Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
                }
            }
        }

        private void UpdateTarget(Compartment compartment)
        {
            var parent = compartment?.ParentManhole;
            if (this.IsInternalConnection()) return;
            if ((Manhole) Target != parent)
            {
                Target = parent;
            }
        }

        private void UpdateSource(Compartment compartment)
        {
            var parent = compartment?.ParentManhole;
            if (this.IsInternalConnection()) return;
            if ((Manhole) Source != parent)
            {
                Source = parent;
            }
        }

        private SewerConnectionSpecialConnectionType GetConnectionType()
        {
            if(!this.IsSpecialConnection()) return SewerConnectionSpecialConnectionType.None;

            if (!BranchFeatures.Any()) return SewerConnectionSpecialConnectionType.None;

            if(BranchFeatures.Any(bf => bf.GetType() == typeof(Pump))) return SewerConnectionSpecialConnectionType.Pump;
            if(BranchFeatures.Any(bf => bf.GetType() == typeof(Weir))) return SewerConnectionSpecialConnectionType.Weir;

            return SewerConnectionSpecialConnectionType.None;
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action != NotifyCollectionChangeAction.Add) return;
            if (!branchFeatures.Any()) return;

            var compositeStructures = branchFeatures.OfType<CompositeBranchStructure>().ToList();
            if (!compositeStructures.Any() ||
                (compositeStructures.First().Structures.Any() &&
                 !compositeStructures.First().Structures.Contains(e.Item)))
            {
                Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
                e.Cancel = true;
            }
        }

        #region IHydroNetworkFeature

        public virtual IHydroRegion Region { get { return HydroNetwork; } }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource { get { return false; } }

        public virtual bool CanBeLinkTarget { get { return false; } }

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            throw new NotSupportedException();
        }

        public virtual void UnlinkFrom(IHydroObject target)
        {
            throw new NotSupportedException();
        }

        public virtual bool CanLinkTo(IHydroObject target)
        {
            return false; // no linking to/from pipe yet
        }

        public virtual IHydroNetwork HydroNetwork { get { return (IHydroNetwork) Network; } }
        public virtual string LongName { get; set; }

        #endregion
    }
}
