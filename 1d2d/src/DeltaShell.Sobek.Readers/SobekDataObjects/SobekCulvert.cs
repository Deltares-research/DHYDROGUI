namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum CulvertType
    {
        Culvert = 1,
        Siphon= 2,
        InvertedSiphon = 3,
    }
    public class SobekCulvert : ISobekStructureDefinition
    {
        /// <summary>
        /// tc = type of culvert
        /// </summary> 
        public CulvertType CulvertType { get; set; }
        
        /// <summary>
        ///rl = bed level (right)
        /// </summary>
        public float BedLevelRight { get; set; }
        
        /// <summary>
        /// ll = bed level (left)
        ///</summary>
        public float BedLevelLeft { get; set; }

        
        /// <summary>
        /// si = id of cross section definition (profile.def), only closed profiles/// 
        /// </summary>
        public string CrossSectionId { get; set; }

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
        ///lb = bend loss coefficient
        /// </summary>
        public float BendLossCoefficient { get; set; }
        
        /// <summary>
        ///ov = initial opening level of valve
        /// </summary>
        public float ValveInitialOpeningLevel { get; set; }
        
        ///<summary>
        ///tv = table of loss coefficient
        ///0 no table, no valve
        ///1 valve present, reference to table in file valve.tab. See detailed decription of this file below.
        /// </summary>
        public int UseTableOffLossCoefficient { get; set; }

        public string TableOfLossCoefficientId { get; set; }

        /// <summary>
        /// rt= possible flow 
        /// 0 : flow in both directions
        /// 1 : flow from begin node to end node (positive)
        /// 2 : flow from end node to begin node (negative)
        /// 3 : no flow
        /// </summary>
        public int Direction { get; set; }
        
        /// <summary>
        ///dl = length of culvert, siphon or inverted siphon
        /// </summary>
        public float Length { get; set; }
    }
}