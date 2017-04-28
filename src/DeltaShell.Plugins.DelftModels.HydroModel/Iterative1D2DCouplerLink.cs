using System;
using System.ComponentModel;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class Iterative1D2DCouplerLink : Feature, INameable, IComparable
    {
        [FeatureAttribute]
        [ReadOnly(true)]
        public string Name { get; set; }

        public Edge LinkEdge { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo(((Iterative1D2DCouplerLink)obj).Name);
        }
    }
}