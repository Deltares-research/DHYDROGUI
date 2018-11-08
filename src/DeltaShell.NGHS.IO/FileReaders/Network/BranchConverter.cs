using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public static class BranchConverter
    {
        public static IList<IChannel> Convert(IList<DelftIniCategory> categories, IHydroNetwork network, IList<FileReadingException> fileReadingExceptions)
        {
            IList<IChannel> branches = new List<IChannel>();
            IList<string> errorMessages = new List<string>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var generatedBranch = ConvertToBranch(branchCategory, network);
                    errorMessages.AddRange(ValidateConvertedBranch(generatedBranch, branches));
                    branches.Add(generatedBranch);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            if (errorMessages.Count > 0)
            {
                var fileReadingException = FileReadingException.GetReportAsException("branches", errorMessages);
                fileReadingExceptions.Add(fileReadingException);
            }

            return branches;
        }

        private static IChannel ConvertToBranch(IDelftIniCategory branchCategory, IHydroNetwork network)
        {
            var idValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var sourceNodeName = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.FromNode.Key);
            var targetNodeName = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.ToNode.Key);
            var branchOrderValue = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.BranchOrder.Key);

            var sourceNode = network.GetNodeByName(sourceNodeName);
            var targetNode = network.GetNodeByName(targetNodeName);
            var geometryString = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Geometry.Key);


            // Optional Branch Properties
            var name = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Name.Key, true) ?? string.Empty;

            return new Channel
            {
                Name = idValue,
                Geometry = GeometryFromWKT.Parse(geometryString),
                LongName = name,
                Source = sourceNode,
                Target = targetNode,
                OrderNumber = branchOrderValue
            };
        }

        private static IEnumerable<string> ValidateConvertedBranch(IChannel readBranch, IList<IChannel> generatedBranches)
        {
            if (readBranch.Source == null)
                yield return $"Unable to parse Branch property: {NetworkDefinitionRegion.FromNode.Key}, Node not found in Network.{Environment.NewLine}";

            if (readBranch.Target == null)
                yield return $"Unable to parse Branch property: {NetworkDefinitionRegion.ToNode.Key}, Node not found in Network.{Environment.NewLine}";

            if (readBranch.IsDuplicateIn(generatedBranches))
                yield return $"branch with id {readBranch.Name} is already read, branch id's cannot be duplicates.";
        }

        private static bool IsDuplicateIn(this IChannel readBranch, IList<IChannel> branches)
        {
            return branches.Contains(readBranch) || branches.Any(b => b.Name == readBranch.Name);
        }
    }
}
