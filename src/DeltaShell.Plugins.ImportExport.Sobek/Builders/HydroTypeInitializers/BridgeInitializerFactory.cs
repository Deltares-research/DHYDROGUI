using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using log4net;
using SobekBridgeType = DeltaShell.Sobek.Readers.SobekDataObjects.BridgeType;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers
{
    public class BridgeInitializerFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeInitializerFactory));

        private readonly Dictionary<SobekBridgeType, IBridgeInitializer> sobekBridgeInitializerByType = new Dictionary<SobekBridgeType, IBridgeInitializer>();
        private readonly IBridgeInitializer defaultBridgeInitializer;

        public BridgeInitializerFactory(IBridgeInitializer defaultBridgeInitializer)
        {
            this.defaultBridgeInitializer = defaultBridgeInitializer;
            RegisterBridgeInitializer(SobekBridgeType.PillarBridge, new PillarBridgeInitializer());
        }

        public void RegisterBridgeInitializer(SobekBridgeType sobekBridgeType, IBridgeInitializer bridgeInitializer)
        {
            Ensure.NotNull(bridgeInitializer, nameof(bridgeInitializer));
            if (sobekBridgeInitializerByType.ContainsKey(sobekBridgeType))
            {
                log.WarnFormat(Resources.BridgeInitializerFactory_RegisterBridgeInitializer_Already_registered_sobek2_bridge_initializer_for_type__0___Overwriting_with_new_initializer, sobekBridgeType);
                sobekBridgeInitializerByType[sobekBridgeType] = bridgeInitializer;
            }
            else
            {
                sobekBridgeInitializerByType.Add(sobekBridgeType, bridgeInitializer);
            }
        }

        public IBridgeInitializer GetBridgeInitializer(SobekBridgeType sobekBridgeType)
        {
            return sobekBridgeInitializerByType.TryGetValue(sobekBridgeType, out IBridgeInitializer bridgeInitializer)
                       ? bridgeInitializer
                       : defaultBridgeInitializer;
        }
    }
}