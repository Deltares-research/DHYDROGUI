using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));
        // For now, the types are: Node, Pipe, Structure, Surface, Runoff, Discharge, Distribution, Meta
        private static Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>>
        {
            {  SewerFeatureType.Node, CreateManhole },
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
            SewerFeatureType elementType;
            if (Enum.TryParse(element.ElementTypeName, out elementType))
            {
                return CreateSewerFeature[elementType](element);
            }
            return null;
        }

        private static Manhole CreateManhole(GwswElement element)
        {
            return new Manhole();
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
