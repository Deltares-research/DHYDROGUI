using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using QuickGraph;

namespace DelftTools.Hydro
{
    [Serializable]
    public class Pipe : Branch, IPipe
    {
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IBranch other)
        {
            throw new NotImplementedException();
        }

        INode IEdge<INode>.Source
        {
            get { throw new NotImplementedException(); }
        }

        INode IBranch.Target { get; set; }
        public double Length
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IEventedList<IBranchFeature> BranchFeatures
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        INode IBranch.Source { get; set; }
        INode IEdge<INode>.Target
        {
            get { throw new NotImplementedException(); }
        }

        public ICrossSectionDefinition CrossSectionDefinition
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IEnumerable<IStructure> Structures
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IEnumerable<IPump> Pumps
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IGully> Gullies
        {
            get { throw new NotImplementedException(); }
        }

        public IHydroNetwork HydroNetwork
        {
            get { return (IHydroNetwork) Network; }
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string LongName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public double StartZ
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public double EndZ
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}