using System;
using DelftTools.Hydro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public class NetworkDefinitionFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;
        private IList<DelftIniCategory> categories;
        private string networkDefinitionFilePath;

        public NetworkDefinitionFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
            categories = new List<DelftIniCategory>();
        }

        /// <summary>
        /// Reads the content in the network definition file defined at <paramref name="filePath"/> and sets
        /// the appropriate data on the <paramref name="network"/>.
        /// Data that is being set on the <paramref name="network"/> are HydroNodes & Branches.
        /// </summary>
        /// <param name="filePath">The file path to the network definition file.</param>
        /// <param name="network">The network.</param>
        /// <returns><see cref="INetworkLocation"/> objects that describe the computational grid points that are read in the network definition file.</returns>
        /// <exception cref="FileNotFoundException">In case the file at location <paramref name="filePath"/> does not exist.</exception>
        public INetworkLocation[] ReadNetworkDefinitionFile(string filePath, IHydroNetwork network)
        {
            var errorMessages = new List<string>();
            networkDefinitionFilePath = filePath;
            categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);

            var nodes = ReadHydroNodes();
            network.Nodes.AddRange(nodes);

            var branches = ReadBranches(network.Nodes);
            network.Branches.AddRange(branches);

            return ReadNetworkLocations(network.Branches).ToArray();
        }
        
        private IEnumerable<IHydroNode> ReadHydroNodes()
        {
            var errorMessages = new List<string>();
            var nodes = HydroNodeConverter.Convert(categories, errorMessages);

            CreateErrorReport("network nodes", errorMessages);
            return nodes;
        }

        private IEnumerable<IChannel> ReadBranches(IList<INode> nodes)
        {
            var errorMessages = new List<string>();
            var branches = BranchConverter.Convert(categories, nodes, errorMessages);

            CreateErrorReport("network branches", errorMessages);
            return branches;
        }

        private IEnumerable<INetworkLocation> ReadNetworkLocations(IList<IBranch> networkBranches)
        {
            var errorMessages = new List<string>();
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, networkBranches, errorMessages);

            CreateErrorReport("network discretization", errorMessages);
            return networkLocations;
        }

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, ICollection<string> errorMessages)
        {
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            return categories;
        }

        private void CreateErrorReport(string objectName, IList<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the {objectName} from file '{networkDefinitionFilePath}', the following errors occured", errorMessages);
        }
    }
}
