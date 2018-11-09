using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public static class BranchConverter
    {
        public static IList<IChannel> Convert(IList<DelftIniCategory> categories, IList<INode> nodes, IList<string> errorMessages)
        {
            IList<IChannel> branches = new List<IChannel>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var generatedBranch = ConvertToBranch(branchCategory, nodes);
                    errorMessages.AddRange(ValidateConvertedBranch(generatedBranch, branches));
                    branches.Add(generatedBranch);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            return branches;
        }

        private static IChannel ConvertToBranch(IDelftIniCategory branchCategory, IList<INode> nodes)
        {
            var idValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var sourceNodeName = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.FromNode.Key);
            var targetNodeName = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.ToNode.Key);
            var branchOrderValue = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.BranchOrder.Key);

            var sourceNode = nodes.FirstOrDefault(n => n.Name == sourceNodeName);
            var targetNode = nodes.FirstOrDefault(n => n.Name == targetNodeName);
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
                yield return $"Unable to parse Branch property: {NetworkDefinitionRegion.FromNode.Key}, Node not found in Network.";

            if (readBranch.Target == null)
                yield return $"Unable to parse Branch property: {NetworkDefinitionRegion.ToNode.Key}, Node not found in Network.";

            if (readBranch.IsDuplicateIn(generatedBranches))
                yield return $"branch with id {readBranch.Name} is already read, branch id's cannot be duplicates.";
        }

        private static bool IsDuplicateIn(this IChannel readBranch, IList<IChannel> branches)
        {
            return branches.Contains(readBranch) || branches.Any(b => b.Name == readBranch.Name);
        }
    }
}
