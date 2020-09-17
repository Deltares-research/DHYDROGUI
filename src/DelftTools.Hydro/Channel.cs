using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity]
    public class Channel : Branch, IChannel
    {
        public Channel() : this(null, null) {}

        public Channel(INode fromNode, INode toNode)
            : this("channel", fromNode, toNode, double.NaN) {}

        public Channel(string name, INode fromNode, INode toNode, double length) :
            base(name, fromNode, toNode, length) {}

        public virtual IHydroRegion Region => throw new NotImplementedException();

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => throw new NotImplementedException();

        public virtual bool CanBeLinkTarget => throw new NotImplementedException();

        public virtual IEnumerable<object> GetDirectChildren()
        {
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }

        #region IChannel Members

        public virtual IEnumerable<IStructure1D> Structures => throw new NotImplementedException();

        public virtual IEnumerable<IPump> Pumps => throw new NotImplementedException();

        public virtual IEnumerable<IWeir> Weirs => throw new NotImplementedException();

        public virtual IEnumerable<IGate> Gates => throw new NotImplementedException();

        public virtual IEnumerable<LateralSource> BranchSources => throw new NotImplementedException();

        public virtual IEnumerable<ObservationPoint> ObservationPoints => throw new NotImplementedException();

        [DisplayName("LongName")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public virtual IHydroNetwork HydroNetwork => throw new NotImplementedException();

        #endregion
    }
}