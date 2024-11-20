using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class DuplicateCalculationPointsViewModel
    {
        private readonly Coordinate coordinate;
        private readonly INetworkLocation mainNetworkLocation;
        private readonly IEnumerable<INetworkLocation> duplicateLocations;
        
        public DuplicateCalculationPointsViewModel(Coordinate coordinate, INetworkLocation mainNetworkLocation, IEnumerable<INetworkLocation> duplicateLocations)
        {
            this.coordinate = coordinate;
            this.mainNetworkLocation = mainNetworkLocation;
            this.duplicateLocations = duplicateLocations;
        }
        
        public string Name
        {
            get { return mainNetworkLocation.Name; }
        }
        

        public string DuplicateNames
        {
            get { return string.Join(", " , duplicateLocations.Select(dl => dl.Name)); }
        }

        public IEnumerable<INetworkLocation> DuplicateNetworkLocations
        {
            get { return duplicateLocations; }
        }
        public INetworkLocation MainNetworkLocation
        {
            get { return mainNetworkLocation; }
        }
    }
}