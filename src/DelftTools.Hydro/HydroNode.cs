using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity]
    public class HydroNode : Node, IHydroNode
    {
        public HydroNode() : this("hydro node") {}

        public HydroNode(string name) : base(name)
        {
            Links = new EventedList<HydroLink>();
        }

        public virtual IHydroNetwork HydroNetwork => (IHydroNetwork) network;

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        [DisplayName("Y")]
        [FeatureAttribute(Order = 4)]
        public virtual double YCoordinate
        {
            get
            {
                var point = Geometry as IPoint;
                return point != null ? point.Y : 0;
            }
        }

        [DisplayName("X")]
        [FeatureAttribute(Order = 3)]
        public virtual double XCoordinate
        {
            get
            {
                var point = Geometry as IPoint;
                return point != null ? point.X : 0;
            }
        }

        [Aggregation]
        public override IEventedList<IBranch> IncomingBranches
        {
            get => base.IncomingBranches;
            set
            {
                if (base.IncomingBranches != null)
                {
                    base.IncomingBranches.CollectionChanged -= OnBranchesCollectionChanged;
                }

                base.IncomingBranches = value;

                if (base.IncomingBranches != null)
                {
                    base.IncomingBranches.CollectionChanged += OnBranchesCollectionChanged;
                }
            }
        }

        [Aggregation]
        public override IEventedList<IBranch> OutgoingBranches
        {
            get => base.OutgoingBranches;
            set
            {
                if (base.OutgoingBranches != null)
                {
                    base.OutgoingBranches.CollectionChanged -= OnBranchesCollectionChanged;
                }

                base.OutgoingBranches = value;

                if (base.OutgoingBranches != null)
                {
                    base.OutgoingBranches.CollectionChanged += OnBranchesCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Returns the features of the node (as objects)
        /// </summary>
        public virtual IEnumerable<object> GetDirectChildren()
        {
            return NodeFeatures.Cast<object>();
        }

        public override object Clone()
        {
            var hydroNode = (HydroNode) base.Clone();
            hydroNode.LongName = LongName;

            hydroNode.Links = new EventedList<HydroLink>(Links);

            return hydroNode;
        }

        [EditAction]
        private void RepairLinks()
        {
            if (IsConnectedToMultipleBranches)
            {
                // remove all links
                foreach (HydroLink link in Links.ToArray())
                {
                    link.Source.UnlinkFrom(link.Target);
                }

                Links.Clear();
            }
        }

        [EditAction]
        private void OnBranchesCollectionChanged(object sender,
                                                 NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (sender is IList<IBranch>)
            {
                RepairLinks();
            }
        }

        public virtual IHydroRegion Region => HydroNetwork;

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => false;

        public virtual bool CanBeLinkTarget => !IsConnectedToMultipleBranches;

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            return Region.AddNewLink(this, target);
        }

        public virtual void UnlinkFrom(IHydroObject target)
        {
            Region.RemoveLink(this, target);
        }

        public virtual bool CanLinkTo(IHydroObject target)
        {
            return Region.CanLinkTo(this, target);
        }
    }
}