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

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the network nodes from file '{filePath}', an error occured", errorMessages);

            return nodes;
        }

        public IList<IChannel> ReadBranches(string filePath, IList<INode> nodes)
        {
            var errorMessages = new List<string>();
            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            var branches = BranchConverter.Convert(categories, nodes, errorMessages);

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the network branches from file '{filePath}', an error occured", errorMessages);

            return branches;
        }

        public IList<INetworkLocation> ReadNetworkLocations(string filePath, IList<IBranch> networkBranches)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            var categories = DelftIniFileParser.ReadFile(filePath);
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, networkBranches, fileReadingExceptions);

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException?.Message + Environment.NewLine);
                throw new FileReadingException($"While reading the network discretization, an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

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
    }
}
