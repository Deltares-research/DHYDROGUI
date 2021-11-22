using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.ComponentModel;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.IO;

namespace DelftTools.Hydro
{
    [Entity]
    public class Channel : Branch, IChannel
    {
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

        [DisplayName("Long name")]
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
        public virtual IEventedList<HydroLink> Links { get; set; } = new EventedList<HydroLink>();

        public virtual bool CanBeLinkSource { get { return false; } }

        public virtual bool CanBeLinkTarget { get { return false; } }
        public virtual Coordinate LinkingCoordinate => Geometry?.Coordinate;

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

        [DynamicReadOnlyValidationMethod]
        public virtual bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            return propertyName != nameof(Length) || !IsLengthCustom;
        }

        #region MDE properties
        [DisplayName("From node")]
        [FeatureAttribute(Order = 5)]
        [ReadOnly(true)]
        public override INode Source { get => base.Source; set => base.Source = value; }

        [DisplayName("To node")]
        [FeatureAttribute(Order = 6)]
        [ReadOnly(true)]
        public override INode Target { get => base.Target; set => base.Target = value; }

        [DisplayName("Geometry length")]
        [FeatureAttribute(Order = 12)]
        public override double GeometryLength { get => base.GeometryLength; }

        [DisplayName("Has custom length")]
        [FeatureAttribute(Order = 10)]
        public override bool IsLengthCustom { get => base.IsLengthCustom; set => base.IsLengthCustom = value; }

        [DisplayName("Length")]
        [FeatureAttribute(Order = 11)]
        [DynamicReadOnly]
        public override double Length { get => base.Length; set => base.Length = value; }

        [DisplayName("Order number")]
        [FeatureAttribute(Order = 13)]
        public override int OrderNumber { get => base.OrderNumber; set => base.OrderNumber = value; }

        [DisplayName("Cross sections")]
        [FeatureAttribute(Order = 30)]
        public virtual int CrossSectionCount
        {
            get => CrossSections.Count();
        }

        [DisplayName("Structures")]
        [FeatureAttribute(Order = 31)]
        public virtual int StructureCount
        {
            get => Structures.Count();
        }

        [DisplayName("Pumps")]
        [FeatureAttribute(Order = 32)]
        public virtual int PumpCount
        {
            get => Pumps.Count();
        }

        [DisplayName("Culverts")]
        [FeatureAttribute(Order = 33)]
        public virtual int CulvertCount
        {
            get => Culverts.Count();
        }

        [DisplayName("Bridges")]
        [FeatureAttribute(Order = 34)]
        public virtual int BridgeCount
        {
            get => Bridges.Count();
        }

        [DisplayName("Weirs")]
        [FeatureAttribute(Order = 35)]
        public virtual int WeirCount
        {
            get => Weirs.Count();
        }

        [DisplayName("Gates")]
        [FeatureAttribute(Order = 36)]
        public virtual int GateCount
        {
            get => Gates.Count();
        }

        [DisplayName("Lateral sources")]
        [FeatureAttribute(Order = 37)]
        public virtual int LateralSourcesCount
        {
            get => BranchSources.Count();
        }
        #endregion
    }
}