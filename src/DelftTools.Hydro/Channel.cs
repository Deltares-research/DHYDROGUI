using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.IO;

namespace DelftTools.Hydro
{
    [Entity]
    public class Channel : Branch, IChannel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Channel));

        public Channel() : this(null, null)
        {
        }

        public Channel(INode fromNode, INode toNode)
            : this("channel", fromNode, toNode, double.NaN)
        {
        }

        public Channel(string name, INode fromNode, INode toNode)
            : this(name, fromNode, toNode, double.NaN)
        {
        }

        public Channel(INode fromNode, INode toNode, double length)
            : this("channel", fromNode, toNode, length)
        {
        }

        public Channel(string name, INode fromNode, INode toNode, double length) :
            base(name, fromNode, toNode, length)
        {
        }

        [FeatureAttribute(Order = 4)]
        public override double Length
        {
            get => base.Length;
            set
            {
                if (value > 0)
                {
                    base.Length = value;
                }
                else
                {
                    Log.ErrorFormat(Resources.Channel_Length_Channel_length_must_be_positive__Length_of_channel___0___remains__1__, Name, Length);
                }
            }
        }

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
                crossSections = BranchFeatures.OfType<ICrossSection>();
                structures = BranchFeatures.OfType<IStructure1D>();
                pumps = BranchFeatures.OfType<IPump>();
                culverts = BranchFeatures.OfType<ICulvert>();
                bridges = BranchFeatures.OfType<IBridge>();
                weirs = BranchFeatures.OfType<IWeir>();
                gates = BranchFeatures.OfType<IGate>();
                branchSources = BranchFeatures.OfType<LateralSource>();
                observationPoints = BranchFeatures.OfType<ObservationPoint>();
            }
        }

        #region IChannel Members

        private IEnumerable<ICrossSection> crossSections;
        private IEnumerable<IStructure1D> structures;
        private IEnumerable<IPump> pumps;
        private IEnumerable<ICulvert> culverts;
        private IEnumerable<IBridge> bridges;
        private IEnumerable<IWeir> weirs;
        private IEnumerable<IGate> gates; 
        private IEnumerable<LateralSource> branchSources;
        private IEnumerable<ObservationPoint> observationPoints;

        public virtual IEnumerable<ICrossSection> CrossSections { get { return crossSections; } }

        public virtual IEnumerable<IStructure1D> Structures { get { return structures; } }

        public virtual IEnumerable<IPump> Pumps { get { return pumps; } }

        public virtual IEnumerable<ICulvert> Culverts { get { return culverts; } }

        public virtual IEnumerable<IBridge> Bridges { get { return bridges; } }

        public virtual IEnumerable<IWeir> Weirs { get { return weirs; } }

        public virtual IEnumerable<IGate> Gates { get { return gates; } } 

        public virtual IEnumerable<LateralSource> BranchSources { get { return branchSources; } }
        
        public virtual IEnumerable<ObservationPoint> ObservationPoints { get { return observationPoints; } }

        [DisplayName("LongName")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public override object Clone()
        {
            Channel clone = (Channel) base.Clone();

            // TODO: remove structures from BranchFeatures if they are part of CompositeBranchStructure, clone child structures in CompositeBranchStructure and then remove this foreach!
            foreach (var compositeBranchStructure in Structures.OfType<ICompositeBranchStructure>())
            {
                var compositeBranchStructureClone = (ICompositeBranchStructure)clone.BranchFeatures[BranchFeatures.IndexOf(compositeBranchStructure)];
                foreach (var structure in compositeBranchStructure.Structures)
                {
                    var structureClone = (IStructure1D)clone.BranchFeatures[BranchFeatures.IndexOf(structure)];
                    structureClone.ParentStructure = compositeBranchStructureClone;
                    compositeBranchStructureClone.Structures.Add(structureClone);
                }
            }
            clone.LongName = LongName;

            return clone;
        }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return (IHydroNetwork) Network; }
        }

        //public string Description { get; set; }
        
        #endregion

        public virtual int CompareTo(IChannel other)
        {
            return Network.Branches.IndexOf(this).CompareTo(Network.Branches.IndexOf(other));
        }

        public static Channel CreateDefault(IHydroNetwork network)
        {
            var channel = new Channel();
            channel.Network = network;
            channel.Geometry = new WKTReader().Read("LINESTRING(0 0,100 100)");
            return channel;
        }

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1)]
        [NoNotifyPropertyChange] //handled by baseclass
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
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
            return false; // no linking to / from channel yet
        }
    }
}