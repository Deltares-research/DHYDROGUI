using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
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
        }

        #endregion

        #region SewerConnection specific

        public string ConnectionId { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        public SewerConnectionType SewerConnectionType { get; set; }

        private Compartment sourceCompartment;
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

        private Compartment targetCompartment;
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
        public SewerConnectionWaterType WaterType { get; set; }
        
        #endregion

        public override IEventedList<IBranchFeature> BranchFeatures
        {
            get { return base.BranchFeatures; }
            set
            {
                base.BranchFeatures = value;

                // Set the filtered properties. Use backing fields ( private setters for 
                // properties e.g. public virtual IEnumerable<IPump> Pumps { get; private set; }
                // will have performance impact. Possible issue in implementation of propertychanged aspect
                // check with performance tests in HydroNetworkTest
                structures = BranchFeatures.OfType<IStructure>();
                pumps = BranchFeatures.OfType<IPump>();
            }
        }

        #region SewerConnection members

        private IEnumerable<IStructure> structures;
        private IEnumerable<IPump> pumps;
        
        public virtual IEnumerable<IStructure> Structures { get { return structures; } }

        public virtual IEnumerable<IPump> Pumps { get { return pumps; } }

        public override object Clone()
        {
            Channel clone = (Channel)base.Clone();

            // TODO: remove structures from BranchFeatures if they are part of CompositeBranchStructure, clone child structures in CompositeBranchStructure and then remove this foreach!
            foreach (var compositeBranchStructure in Structures.OfType<ICompositeBranchStructure>())
            {
                var compositeBranchStructureClone = (ICompositeBranchStructure)clone.BranchFeatures[BranchFeatures.IndexOf(compositeBranchStructure)];
                foreach (var structure in compositeBranchStructure.Structures)
                {
                    var structureClone = (IStructure)clone.BranchFeatures[BranchFeatures.IndexOf(structure)];
                    structureClone.ParentStructure = compositeBranchStructureClone;
                    compositeBranchStructureClone.Structures.Add(structureClone);
                }
            }
            clone.LongName = LongName;

            return clone;
        }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return (IHydroNetwork)Network; }
        }


        #endregion

        public override bool IsLengthCustom
        {
            get { return true; }
        }

        public virtual string LongName { get; set; }

        public virtual int CompareTo(IChannel other)
        {
            return Network.Branches.IndexOf(this).CompareTo(Network.Branches.IndexOf(other));
        }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            return BranchFeatures.Cast<object>();
        }

        public virtual IHydroRegion Region { get { return HydroNetwork; } }

        [Aggregation]
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
            return false; // no linking to / from sewer connection yet
        }
    
    }
}
