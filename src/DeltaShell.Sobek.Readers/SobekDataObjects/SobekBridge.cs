namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum BridgeType
    {
        PillarBridge = 2,
        Abutment = 3,
        FixedBed = 4,
        SoilBed = 5
    }

    public class SobekBridge : ISobekStructureDefinition
    {
        /// <summary>
        /// tb
        /// tb = type of bridge
        /// 2 = pillar bridge
        /// 3 = abutment bridge
        /// 4 = fixed bed bridge
        /// 5 = soil bed bridge
        /// </summary>
        public BridgeType BridgeType { get; set; }

        /// <summary>
        /// si
        /// id of cross section definition 
        /// </summary>
        public string CrossSectionId { get; set; }

        /// <summary>
        /// pw
        /// total width of pillars in direction of flow (if tb=2)
        /// </summary>
        public float TotalPillarWidth { get; set; }

        /// <summary>
        /// vf
        /// form factor (if tb=2)
        /// </summary>
        public float FormFactor { get; set; }

        /// <summary>
        /// li = 
        /// inlet loss coefficient
        /// </summary>
        public float InletLossCoefficient { get; set; }

        /// <summary>
        /// lo = 
        /// outlet loss coefficient
        /// </summary>
        public float OutletLossCoefficient { get; set; }

        /// <summary>
        /// dl = 
        /// length of bridge in flow direction.
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// rl = 
        /// bed level 
        /// </summary>
        public float BedLevel { get; set; }

        /// <summary>
        /// rt= possible flow 
        /// 0 : flow in both directions
        /// 1 : flow from begin node to end node (positive)
        /// 2 : flow from end node to begin node (negative)
        /// 3 : no flow
        /// </summary>
        public int Direction { get; set; }
    }
}
