using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IChannel : IBranch, IHydroNetworkFeature, IItemContainer
    {
        //reintroducing members for databinding.
        string Name { get; set; }

        /// <summary>
        /// Name as defined in sobek under name. Since Name is taken by ID name is stored here
        /// </summary>
        string LongName { get; set; }

        double Length { get; set; }

        INode Source { get; set; }
        INode Target { get; set; }
        IEnumerable<IStructure1D> Structures { get; }
        IEnumerable<IPump> Pumps { get; }
        IEnumerable<IWeir> Weirs { get; }
        IEnumerable<IGate> Gates { get; }
        IEnumerable<LateralSource> BranchSources { get; }
        IEnumerable<ObservationPoint> ObservationPoints { get; }
    }
}