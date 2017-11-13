using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));
        // For now, the types are: Node, Pipe, Structure, Surface, Runoff, Discharge, Distribution, Meta
        private static Dictionary<SewerFeatureType, Func<object, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<object, INetworkFeature>>
        {
            { SewerFeatureType.Node, CreateManhole },
            { SewerFeatureType.Pipe, CreatePipe }
        };

        public static IEnumerable<INetworkFeature> CreateMultipleInstances(List<GwswElement> listOfElements)
        {
            var networkFeatures = new List<INetworkFeature>();
            foreach (var element in listOfElements)
            {
                networkFeatures.Add(CreateInstance(element));
            }
            return networkFeatures;
        }

        public static INetworkFeature CreateInstance(object element)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement != null && Enum.TryParse(gwswElement.ElementTypeName, out SewerFeatureType elementType))
            {
                return CreateSewerFeature[elementType](gwswElement);
            }

            var gwswElementList = element as List<GwswElement>;
            if (gwswElementList != null && Enum.TryParse(gwswElementList.FirstOrDefault()?.ElementTypeName,
                    out SewerFeatureType elementListType))
            {
                return CreateSewerFeature[elementListType](gwswElementList);
            }

            return null;
        }

        private static CompositeManholeNode CreateManhole(object element)
        {
            switch (element)
            {
                case GwswElement gwswElement:
                    return CreateManholeNode(new List<GwswElement> {gwswElement});
                case List<GwswElement> gwswElementList:
                    return CreateManholeNode(gwswElementList);
                default:
                    return null;
            }
        }

        private static CompositeManholeNode CreateManholeNode(IEnumerable<GwswElement> gwswElementList)
        {
            // Create dictionary with all attributes
            var elementValuesDictionary = gwswElementList.Select(gwswElement => gwswElement.GwswAttributeList)
                .Select(attributes => attributes.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString))
                .ToList();

            var manholeIds = elementValuesDictionary.Select(v => v["MANHOLE_ID"]).Distinct().ToList();
            if (manholeIds.Count != 1) return null;

            var manholeNode = new CompositeManholeNode(manholeIds.FirstOrDefault());
            foreach (var elementValues in elementValuesDictionary)
            {
                var manholeCompartment = CreateManHoleCompartment(elementValues);
                manholeNode.Compartments.Add(manholeCompartment);
            }

            return manholeNode;
        }

        private static Manhole CreateManHoleCompartment(Dictionary<string, string> elementValues)
        {
            return new Manhole(elementValues["UNIQUE_ID"])
            {
                ManholeLength = int.Parse(elementValues["NODE_LENGTH"]),
                ManholeWidth = int.Parse(elementValues["NODE_WIDTH"]),
                Shape = (ManholeShape)EnumDescriptionAttributeTypeConverter.GetEnumValue<ManholeShape>(elementValues["NODE_SHAPE"]),
                FloodableArea = double.Parse(elementValues["FLOODABLE_AREA"]),
                BottomLevel = double.Parse(elementValues["BOTTOM_LEVEL"], CultureInfo.InvariantCulture),
                SurfaceLevel = double.Parse(elementValues["SURFACE_LEVEL"], CultureInfo.InvariantCulture),
                Coordinates = new Coordinate(double.Parse(elementValues["X_COORDINATE"], CultureInfo.InvariantCulture), double.Parse(elementValues["Y_COORDINATE"], CultureInfo.InvariantCulture))
            };
        }

        private static GwswAttribute GetAttributeFromList(GwswElement element, string attributeName)
        {
            var attribute = element.GwswAttributeList.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute == null)
            {
                Log.WarnFormat("Attribute {0} was not found for element {1}", attributeName, element.ElementTypeName);
                return null;
            }
            return attribute;
        }

        private static Pipe CreatePipe(object element)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement == null) return null;

            var newPipe = new Pipe();

            var nodeIdStart = GetAttributeFromList(gwswElement, "NODE_UNIQUE_ID_START");
            if (nodeIdStart != null)
            {
                //Find node;
                //Nodes are needed first.
            }
            var nodeIdEnd = GetAttributeFromList(gwswElement, "NODE_UNIQUE_ID_END");
            if (nodeIdEnd != null)
            {
                //Find node;
                //Nodes are needed first.
            }
            var pipeTypeAttr = GetAttributeFromList(gwswElement, "PIPE_TYPE");
            if (pipeTypeAttr != null
                && pipeTypeAttr.ValueAsString != string.Empty)
            {
                //Find type;
                PipeType pipeType;
                if (Enum.TryParse(pipeTypeAttr.ValueAsString, out pipeType))
                {
                    newPipe.PipeType = pipeType;
                }
            }
            var levelStart = GetAttributeFromList(gwswElement, "LEVEL_START");
            if (levelStart != null)
            {
                var valueType = levelStart.GwswAttributeType.AttributeType;
                if (valueType == newPipe.LevelSource.GetType()
                    && levelStart.ValueAsString != string.Empty)
                {
                    newPipe.LevelSource = double.Parse(levelStart.ValueAsString);
                }
            }
            var levelEnd = GetAttributeFromList(gwswElement, "LEVEL_END");
            if (levelEnd != null)
            {
                var valueType = levelEnd.GwswAttributeType.AttributeType;
                if (valueType == newPipe.LevelTarget.GetType()
                    && levelEnd.ValueAsString != string.Empty)
                {
                    newPipe.LevelTarget = double.Parse(levelEnd.ValueAsString);
                }
            }
            var length = GetAttributeFromList(gwswElement, "LENGTH");
            if (length != null)
            {
                var valueType = length.GwswAttributeType.AttributeType;
                if (valueType == newPipe.Length.GetType()
                    && length.ValueAsString != string.Empty)
                {
                    newPipe.Length = double.Parse(length.ValueAsString);
                }
            }
            var crossSectionDef = GetAttributeFromList(gwswElement, "CROSS_SECTION_DEF");
            if (crossSectionDef != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
            }

            return newPipe;
        }
    }
}
