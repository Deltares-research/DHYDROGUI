using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public interface IHydroNetwork : INetwork, IHydroRegion
    {
        IEventedList<Route> Routes { get; }
        
        IEnumerable<IHydroNode> HydroNodes { get; }
        
        IEnumerable<IPipe> Pipes { get; }
        
        IEnumerable<IManhole> Manholes { get; }
        
        IEnumerable<OutletCompartment> OutletCompartments { get; }
        
        IEnumerable<Compartment> Compartments { get; }
        
        IEnumerable<IOrifice> Orifices { get; }
        
        IEnumerable<ISewerConnection> SewerConnections { get; }
        
        IEnumerable<IChannel> Channels { get; }
        
        IEnumerable<ICrossSection> CrossSections { get; }
        
        IEnumerable<IStructure1D> Structures { get; }
        
        IEnumerable<ICompositeBranchStructure> CompositeBranchStructures { get; }
        
        IEnumerable<IPump> Pumps { get; }
        
        IEnumerable<ICulvert> Culverts { get; }
        
        IEnumerable<IBridge> Bridges { get; }
        
        IEnumerable<IWeir> Weirs { get; }
        
        IEnumerable<IGate> Gates { get; }
        
        IEnumerable<ILateralSource> LateralSources { get; }
        
        IEnumerable<IRetention> Retentions { get; }
        
        IEnumerable<IObservationPoint> ObservationPoints { get; }

        IEventedList<CrossSectionSectionType> CrossSectionSectionTypes { get; }
        
        IEventedList<ICrossSectionDefinition> SharedCrossSectionDefinitions { get; }

        ICrossSectionDefinition DefaultCrossSectionDefinition { get; set; }

        /// <summary>
        /// Use cached lookup to get a <see cref="IBranchFeature"/> by name
        /// </summary>
        /// <typeparam name="T">Type of feature to search for (use interface type)</typeparam>
        /// <param name="featureName">Name of the feature to search for (case insensitive)</param>
        /// <returns>if <see cref="IBranchFeature"/> is found it returns the feature else it returns the default(T)</returns>
        T GetBranchFeatureByName<T>(string featureName) where T : IBranchFeature;
    }
}