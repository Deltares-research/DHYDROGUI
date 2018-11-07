using System;
using System.Linq;
using DelftTools.Hydro;
using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public class NetworkDefinitionFileReader
    {
        public IList<IHydroNode> ReadHydroNodes(string filePath)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            var categories = NetworkDefinitionFileParser.ReadFile(filePath);
            var nodes = HydroNodeConverter.Convert(categories, fileReadingExceptions);

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException?.Message + Environment.NewLine);
                throw new FileReadingException($"While reading the network nodes from file, an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

            return nodes;
        }

        public IList<IChannel> ReadBranches(string filePath, IHydroNetwork network)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            var categories = NetworkDefinitionFileParser.ReadFile(filePath);
            var branches = BranchConverter.Convert(categories, network, fileReadingExceptions);

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException?.Message + Environment.NewLine);
                throw new FileReadingException($"While reading the network branches from file, an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

            return branches;
        }

        public IList<INetworkLocation> ReadNetworkLocations(string filePath, IList<IBranch> networkBranches)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            var categories = NetworkDefinitionFileParser.ReadFile(filePath);
            var networkLocations = NetworkDiscretizationConverter.Convert(categories, networkBranches, fileReadingExceptions);

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException?.Message + Environment.NewLine);
                throw new FileReadingException($"While reading the network discretization, an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

            return networkLocations;
        }
    }
}
