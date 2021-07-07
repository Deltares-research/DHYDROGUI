using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class BridgeBuilder : BranchStructureBuilderBase<Bridge>
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(BridgeBuilder));

        private readonly Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions;

        public BridgeBuilder(Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions)
        {
            this.sobekCrossSectionDefinitions = sobekCrossSectionDefinitions;
        }

        public override IEnumerable<Bridge> GetBranchStructures(SobekStructureDefinition structure)
        {
            if (structure == null || (!(structure.Definition is SobekBridge)))
            {
                yield break;
            }

            var sobekBridge = structure.Definition as SobekBridge;

            Bridge bridge = new Bridge((string.IsNullOrEmpty(structure.Name) ? "Bridge" : structure.Name))
                                {
                                    InletLossCoefficient = sobekBridge.InletLossCoefficient,
                                    OutletLossCoefficient = sobekBridge.OutletLossCoefficient,
                                    Shift = sobekBridge.BedLevel,
                                    Length = sobekBridge.Length,
                                    FlowDirection = GetFlowDirection(sobekBridge.Direction),
                                    PillarWidth = sobekBridge.TotalPillarWidth,
                                    ShapeFactor = sobekBridge.FormFactor,
                                    BridgeType = BridgeType.Rectangle
                                };

            if (sobekBridge.BridgeType != DeltaShell.Sobek.Readers.SobekDataObjects.BridgeType.PillarBridge)
            {

                if (sobekBridge.CrossSectionId != null &&
                    sobekCrossSectionDefinitions.ContainsKey(sobekBridge.CrossSectionId))
                {
                    SobekCrossSectionDefinition sobekCrossSectionDefinition =
                        sobekCrossSectionDefinitions[sobekBridge.CrossSectionId];

                    bridge.GroundLayerEnabled = sobekCrossSectionDefinition.UseGroundLayer;
                    bridge.GroundLayerThickness = sobekCrossSectionDefinition.GroundLayerDepth;

                    if (sobekCrossSectionDefinition.Type == SobekCrossSectionDefinitionType.Tabulated)
                    {
                        var hfswData =
                            sobekCrossSectionDefinition.TabulatedProfile.Select(
                                t =>
                                    new HeightFlowStorageWidth(t.Height + bridge.Shift, t.TotalWidth, t.FlowWidth));

                        bridge.TabulatedCrossSectionDefinition.SetWithHfswData(hfswData);
                        bridge.YZCrossSectionDefinition.SetWithHfswData(hfswData);

                        bridge.BridgeType = BridgeType.Tabulated;
                        if ((sobekCrossSectionDefinition.Name != null) &&
                            sobekCrossSectionDefinition.Name.StartsWith("r_"))
                        {
                            //set the type of the bridgegeometry and set width and height, bed level in the bridge
                            bridge.BridgeType = BridgeType.Rectangle;
                            bridge.Width = sobekCrossSectionDefinition.TabulatedProfile[1].TotalWidth;
                            bridge.Height = sobekCrossSectionDefinition.TabulatedProfile[1].Height -
                                            sobekCrossSectionDefinition.TabulatedProfile[0].Height;
                        }
                    }
                    else
                    {
                        Log.WarnFormat("Only bridge profiles of type tabular {0} supported.",
                            SobekCrossSectionDefinitionType.Tabulated);
                    }
                }

            }
            else
            {
                Log.WarnFormat("Bridge pillars are not yet supported in the kernel, skipping this bridge with id : {0}", (string.IsNullOrEmpty(structure.Name) ? "<No id is set>" : structure.Name));
                yield break; //not yet implemented in the kernel
            }


            yield return bridge;
        }
    }
}
 