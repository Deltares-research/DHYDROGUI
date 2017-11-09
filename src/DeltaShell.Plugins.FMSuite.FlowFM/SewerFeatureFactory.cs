using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        private static Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<GwswElement, INetworkFeature>>
        {
            { SewerFeatureType.Node, CreateManhole },
            { SewerFeatureType.Pipe, CreatePipe }
        };

        public static INetworkFeature CreateInstance(GwswElement element)
        {
            return CreateSewerFeature[element.ElementType](element);
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
