using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        // For now, the types are: Node, Pipe, Structure, Surface, Runoff, Discharge, Distribution, Meta
        private static Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>>
        {
            {  SewerFeatureType.Node, CreateManhole },
            { SewerFeatureType.Pipe, CreatePipe }
        };

        public static Dictionary<string, SewerFeatureType> stringToSewerTypeConverter = new Dictionary<string, SewerFeatureType>
        {
            { "Node", SewerFeatureType.Node },
            { "Pipe", SewerFeatureType.Pipe },
            { "Structure", SewerFeatureType.Structure },
            { "Surface", SewerFeatureType.Surface },
            { "Runoff", SewerFeatureType.Runoff },
            { "Discharge", SewerFeatureType.Discharge },
            { "Distribution", SewerFeatureType.Distribution },
            { "Meta", SewerFeatureType.Meta }
        };

        public static INetworkFeature CreateInstance(GwswElement element)
        {
            SewerFeatureType elementType;
            if (stringToSewerTypeConverter.TryGetValue(element.ElementTypeName, out elementType))
            {
                return CreateSewerFeature[elementType](element);
            }
            return null;
        }

        private static Manhole CreateManhole(GwswElement element)
        {
            return new Manhole();
        }

        private static Pipe CreatePipe(GwswElement element)
        {
            return new Pipe();
        }
    }
}
