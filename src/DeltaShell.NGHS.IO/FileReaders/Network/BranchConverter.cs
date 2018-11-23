using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
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
                    ValidateConvertedBranch(generatedBranch, branches);
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

            var channel =  new Channel
            {
                Name = idValue,
                Geometry = GeometryFromWKT.Parse(geometryString),
                LongName = name,
                Source = sourceNode,
                Target = targetNode,
                OrderNumber = branchOrderValue
            };
            AdjustCustomLengthProperty(branchCategory, channel);

            return channel;
        }

        private static void AdjustCustomLengthProperty(IDelftIniCategory branchCategory, IBranch channel)
        {
            var gridPointsOffsets = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointOffsets.Key, true);
            if (Math.Abs(gridPointsOffsets.Max() - channel.Length) < 1e-3) return;

            channel.IsLengthCustom = true;
            channel.Length = gridPointsOffsets.Max();
        }

        private static void ValidateConvertedBranch(IChannel readBranch, IList<IChannel> generatedBranches)
        {
            var errorMessages = new List<string>();
            if (readBranch.Source == null)
                errorMessages.Add($"Unable to parse Branch property: {NetworkDefinitionRegion.FromNode.Key} for branch {readBranch.Name}, Node not found in Network.");

            if (readBranch.Target == null)
                errorMessages.Add($"Unable to parse Branch property: {NetworkDefinitionRegion.ToNode.Key} for branch {readBranch.Name}, Node not found in Network.");

            if (readBranch.IsDuplicateIn(generatedBranches))
                errorMessages.Add($"branch with id {readBranch.Name} is already read, branch id's cannot be duplicates.");

            if (errorMessages.Any())
            {
                throw new Exception(string.Join(Environment.NewLine, errorMessages));
            }
        }

        private static bool IsDuplicateIn(this IChannel readBranch, IList<IChannel> branches)
        {
            return branches.Contains(readBranch) || branches.Any(b => b.Name == readBranch.Name);
        }
    }
}
