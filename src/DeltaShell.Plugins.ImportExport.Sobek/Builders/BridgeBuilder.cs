using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders
{
    public class BridgeBuilder : BranchStructureBuilderBase<Bridge>
    {
        private readonly BridgeInitializerFactory bridgeInitializerFactory;
        public BridgeBuilder(Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions)
        {
            IBridgeInitializer defaultBridgeInitializer = new DefaultBridgeInitializer(sobekCrossSectionDefinitions);
            bridgeInitializerFactory = new BridgeInitializerFactory(defaultBridgeInitializer);
        }

        public override IEnumerable<Bridge> GetBranchStructures(SobekStructureDefinition structure)
        {
            if (!(structure?.Definition is SobekBridge sobekBridge))
            {
                yield break;
            }

            Bridge bridge = GenerateStandardBridgeFromSobek2(structure, sobekBridge);

            IBridgeInitializer bridgeInitializer = bridgeInitializerFactory.GetBridgeInitializer(sobekBridge.BridgeType);
            bridgeInitializer.Initialize(sobekBridge, bridge);

            yield return bridge;
        }
        private Bridge GenerateStandardBridgeFromSobek2(SobekStructureDefinition structure, SobekBridge sobekBridge)
        {
            return new Bridge((string.IsNullOrEmpty(structure.Name) ? "Bridge" : structure.Name))
            {
                InletLossCoefficient = sobekBridge.InletLossCoefficient,
                OutletLossCoefficient = sobekBridge.OutletLossCoefficient,
                Shift = sobekBridge.BedLevel,
                Length = sobekBridge.Length,
                FlowDirection = GetFlowDirection(sobekBridge.Direction)
            };
        }
    }
}
 