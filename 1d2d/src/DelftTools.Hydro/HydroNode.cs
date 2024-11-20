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
        public HydroNode() : this("hydro node")
        {
        }

        public HydroNode(string name) : base(name)
        {
            Links = new EventedList<HydroLink>();
        }

        public virtual IHydroNetwork HydroNetwork { get { return (IHydroNetwork)network; } }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        [DisplayName("Y coordinate")]
        [FeatureAttribute(Order = 10)]
        public virtual double YCoordinate
        {
            get
            {
                var point = Geometry as IPoint;
                return point != null ? point.Y : 0;
            }
        }


        [DisplayName("X coordinate")]
        [FeatureAttribute(Order = 9)]
        public virtual double XCoordinate
        {
            get
            {
                var point = Geometry as IPoint;
                return point != null ? point.X : 0;
            }
        }

        [DisplayName("Incoming branches")]
        [FeatureAttribute(Order = 30)]
        public double IncomingBranchesCount
        {
            get => IncomingBranches.Count;
        }

        [DisplayName("Outgoing branches")]
        [FeatureAttribute(Order = 31)]
        public double OutgoingBranchesCount
        {
            get => OutgoingBranches.Count;
        }


        [Aggregation]
        public override IEventedList<IBranch> IncomingBranches
        {
            get { return base.IncomingBranches; }
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
            get { return base.OutgoingBranches; }
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

        private void RepairLinks()
        {
            if (IsConnectedToMultipleBranches)
            {
                // remove all links
                foreach (var link in Links.ToArray())
                {
                    link.Source.UnlinkFrom(link.Target);
                }

                Links.Clear();
            }
        }

        private void OnBranchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs NotifyCollectionChangedEventArgs)
        {
            if (sender is IList<IBranch>)
            {
                RepairLinks();
            }
        }

        public virtual IHydroRegion Region { get { return HydroNetwork; } }

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource { get { return false; } }

        public virtual bool CanBeLinkTarget { get { return !IsConnectedToMultipleBranches; } }
        public virtual Coordinate LinkingCoordinate => Geometry?.Coordinate;

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