using System;
using System.Linq;
using DelftTools.Hydro;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public class NetworkDefinitionFileReader
    {
        private readonly Action<string, List<string>> createAndAddErrorReport;

        public NetworkDefinitionFileReader(Action<string, List<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }
        
        public IList<IHydroNode> ReadHydroNodes(string filePath)
        {
            var errorMessages = new List<string>();
            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            var nodes = HydroNodeConverter.Convert(categories, errorMessages);

            CreateErrorReport("network nodes", filePath, errorMessages);
            return nodes;
        }

        public IList<IChannel> ReadBranches(string filePath, IList<INode> nodes)
        {
            var errorMessages = new List<string>();
            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            var branches = BranchConverter.Convert(categories, nodes, errorMessages);

            CreateErrorReport("network branches", filePath, errorMessages);
            return branches;
        }

        public IList<INetworkLocation> ReadNetworkLocations(string filePath, IList<IBranch> networkBranches)
        {
            var errorMessages = new List<string>();
            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, networkBranches, errorMessages);

            CreateErrorReport("network discretization", filePath, errorMessages);
            return networkLocations;
        }

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, List<string> errorMessages)
        {
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            return categories;
        }

        private void CreateErrorReport(string objectName, string filePath, List<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the {objectName} from file '{filePath}', the following errors occured", errorMessages);
        }
    }
}
