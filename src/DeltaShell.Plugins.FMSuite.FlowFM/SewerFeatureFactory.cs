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
        private static Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>>
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

        public static INetworkFeature CreateInstance(GwswElement element)
        {
            if (Enum.TryParse(element.ElementTypeName, out SewerFeatureType elementType))
            {
                return CreateSewerFeature[elementType](element);
            }
            return null;
        }

        private static Manhole CreateManhole(GwswElement element)
        {
            // Create dictionary with all attributes
            var attributes = element.GwswAttributeList;
            var propertyValues = attributes.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            // Create manhole
            var coords = new Coordinate(double.Parse(propertyValues["X_COORDINATE"], CultureInfo.InvariantCulture), double.Parse(propertyValues["Y_COORDINATE"], CultureInfo.InvariantCulture));
            var manhole = new Manhole(propertyValues["MANHOLE_ID"], coords);
            var compartment = new ManholeCompartment(propertyValues["UNIQUE_ID"])
            {
                ManholeLength = int.Parse(propertyValues["NODE_LENGTH"]),
                ManholeWidth = int.Parse(propertyValues["NODE_WIDTH"]),
                Shape = (ManholeShape) EnumDescriptionAttributeTypeConverter.GetEnumValue<ManholeShape>(propertyValues["NODE_SHAPE"]),
                FloodableArea = double.Parse(propertyValues["FLOODABLE_AREA"]),
                BottomLevel = double.Parse(propertyValues["BOTTOM_LEVEL"], CultureInfo.InvariantCulture),
                SurfaceLevel = double.Parse(propertyValues["SURFACE_LEVEL"], CultureInfo.InvariantCulture)
            };
            manhole.Compartments.Add(compartment);
            
            return manhole;
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

        private static Pipe CreatePipe(GwswElement element)
        {
            var newPipe = new Pipe();

            var nodeIdStart = GetAttributeFromList(element, "NODE_UNIQUE_ID_START");
            if (nodeIdStart != null)
            {
                //Find node;
                //Nodes are needed first.
            }
            var nodeIdEnd = GetAttributeFromList(element, "NODE_UNIQUE_ID_END");
            if (nodeIdEnd != null)
            {
                //Find node;
                //Nodes are needed first.
            }
            var pipeTypeAttr = GetAttributeFromList(element, "PIPE_TYPE");
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
            var levelStart = GetAttributeFromList(element, "LEVEL_START");
            if (levelStart != null)
            {
                var valueType = levelStart.GwswAttributeType.AttributeType;
                if (valueType == newPipe.LevelSource.GetType()
                    && levelStart.ValueAsString != string.Empty)
                {
                    newPipe.LevelSource = double.Parse(levelStart.ValueAsString);
                }
            }
            var levelEnd = GetAttributeFromList(element, "LEVEL_END");
            if (levelEnd != null)
            {
                var valueType = levelEnd.GwswAttributeType.AttributeType;
                if (valueType == newPipe.LevelTarget.GetType()
                    && levelEnd.ValueAsString != string.Empty)
                {
                    newPipe.LevelTarget = double.Parse(levelEnd.ValueAsString);
                }
            }
            var length = GetAttributeFromList(element, "LENGTH");
            if (length != null)
            {
                var valueType = length.GwswAttributeType.AttributeType;
                if (valueType == newPipe.Length.GetType()
                    && length.ValueAsString != string.Empty)
                {
                    newPipe.Length = double.Parse(length.ValueAsString);
                }
            }
            var crossSectionDef = GetAttributeFromList(element, "CROSS_SECTION_DEF");
            if (crossSectionDef != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
            }

            return newPipe;
        }
    }
}
