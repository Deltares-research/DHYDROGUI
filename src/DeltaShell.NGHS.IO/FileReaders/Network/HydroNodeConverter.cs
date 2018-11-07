using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public static class HydroNodeConverter
    {
        public static IList<IHydroNode> Convert(IList<DelftIniCategory> categories, IList<FileReadingException> fileReadingExceptions)
        {
            IList<IHydroNode> nodes = new List<IHydroNode>();
            IList<string> errorMessages = new List<string>();
            foreach (var nodeCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniNodeHeader))
            {
                try
                {
                    var generatedNode = ConvertToHydroNode(nodeCategory);
                    errorMessages.AddRange(ValidateConvertedNode(generatedNode, nodes));
                    nodes.Add(generatedNode);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            if (errorMessages.Count > 0)
            {
                var fileReadingException = FileReadingException.GetReportAsException("nodes", errorMessages);
                fileReadingExceptions.Add(fileReadingException);
            }

            return nodes;
        }

        private static IHydroNode ConvertToHydroNode(IDelftIniCategory nodeCategory)
        {
            var idProperty = nodeCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var xCoordinate = nodeCategory.ReadProperty<double>(NetworkDefinitionRegion.X.Key);
            var yCoordinate = nodeCategory.ReadProperty<double>(NetworkDefinitionRegion.Y.Key);

            // Optional Node Properties - don't throw exception if not available
            var nameProperty = nodeCategory.ReadProperty<string>(NetworkDefinitionRegion.Name.Key, true) ?? string.Empty;

            return new HydroNode
            {
                Name = idProperty,
                LongName = nameProperty,
                Geometry = new Point(xCoordinate, yCoordinate)
            };
        }

        private static IEnumerable<string> ValidateConvertedNode(IHydroNode readNode, IList<IHydroNode> generatedNodes)
        {
            if (readNode.IsDuplicateIn(generatedNodes))
                yield return $"Node with id {readNode.Name} already exists, there cannot be any duplicate Node ids";
        }

        private static bool IsDuplicateIn(this IHydroNode readNode, IList<IHydroNode> nodes)
        {
            return nodes.Contains(readNode) || nodes.Any(n => n.Name == readNode.Name);
        }
    }
}
