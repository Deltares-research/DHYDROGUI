using System.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    internal class FouFileMetaData
    {
        private IList<INetworkLocation> mesh1dLocations;

        /// <summary>
        /// Path to the original fou file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Unstructured grid contained in the fou file
        /// </summary>
        public UnstructuredGrid Grid { get; set; }

        /// <summary>
        /// Network contained in the fou file
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Computational point of the 1d mesh contained in the fou file
        /// </summary>
        public IList<INetworkLocation> Mesh1dLocations
        {
            get
            {
                return mesh1dLocations;
            }
            set
            {
                mesh1dLocations = value;
                IndexByLocation = mesh1dLocations.ToIndexDictionary();
            }
        }

        /// <summary>
        /// Lookup for finding an index for a given location
        /// </summary>
        public IDictionary<INetworkLocation, int> IndexByLocation { get; private set; }

        /// <summary>
        /// Output NetCdfVariables based on the 1d mesh
        /// </summary>
        public IEnumerable<NetCdfVariable> Variables1D { get; set; }

        /// <summary>
        /// Output NetCdfVariables based on the 2d mesh
        /// </summary>
        public IEnumerable<NetCdfVariable> Variables2D { get; set; }
    }
}