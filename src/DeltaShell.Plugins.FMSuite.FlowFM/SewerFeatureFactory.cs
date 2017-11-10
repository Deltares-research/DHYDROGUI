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
        private static Dictionary<string, Func<GwswElement, INetworkFeature>> CreateSewerFeature = new Dictionary<string, Func<GwswElement, INetworkFeature>>
        {
            { "Node", CreateManhole },
            { "Pipe", CreatePipe }
        };

        public static INetworkFeature CreateInstance(GwswElement element)
        {
            return CreateSewerFeature[element.ElementTypeName](element);
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
