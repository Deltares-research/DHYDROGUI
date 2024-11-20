namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekGeneralStructure: ISobekStructureDefinition
    {

        /// <summary>
        /// import from RE
        /// </summary>
        public bool ImportFromRE { get; set; }

        /// <summary>
        /// w1
        /// </summary>
        public float WidthLeftSideOfStructure{ get; set; }
        /// <summary>
        /// wl
        /// </summary>
        public float WidthStructureLeftSide { get; set; }
        /// <summary>
        /// ws
        /// </summary>
        public float WidthStructureCentre{ get; set; }
        /// <summary>
        /// wr
        /// </summary>
        public float WidthStructureRightSide{ get; set; }

        /// <summary>
        /// w2
        /// </summary>
        public float WidthRightSideOfStructure { get; set; }
        
        /// <summary>
        /// z1
        /// </summary>
        public float BedLevelLeftSideOfStructure{ get; set; }
        /// <summary>
        /// zl
        /// </summary>
        public float BedLevelLeftSideStructure { get; set; }
        /// <summary>
        /// zs
        /// </summary>
        public float BedLevelStructureCentre { get; set; }
        /// <summary>
        /// zr
        /// </summary>
        public float BedLevelRightSideStructure { get; set; }

        /// <summary>
        /// z2
        /// </summary>
        public float BedLevelRightSideOfStructure { get; set; }
        

        /// <summary>
        /// gh
        /// </summary>
        public float GateHeight{ get; set; }

        /// <summary>
        /// pg
        /// </summary>
        public float PositiveFreeGateFlow { get; set; }
        
        /// <summary>
        /// pd
        /// </summary>
        public float PositiveDrownedGateFlow { get; set; }
        
        /// <summary>
        /// pi
        /// </summary>
        public float PositiveFreeWeirFlow { get; set; }

        /// <summary>
        /// pr
        /// </summary>
        public float PositiveDrownedWeirFlow { get; set; }

        /// <summary>
        /// pc
        /// </summary>
        public float PositiveContractionCoefficient { get; set; }

        /// <summary>
        /// ng
        /// </summary>
        public float NegativeFreeGateFlow { get; set; }

        /// <summary>
        /// nd
        /// </summary>
        public float NegativeDrownedGateFlow { get; set; }

        /// <summary>
        /// nf
        /// </summary>
        public float NegativeFreeWeirFlow { get; set; }

        /// <summary>
        /// nr
        /// </summary>
        public float NegativeDrownedWeirFlow { get; set; }

        /// <summary>
        /// nc
        /// </summary>
        public float NegativeContractionCoefficient { get; set; }

        /// <summary>
        /// er
        /// </summary>
        public float? ExtraResistance { get; set; }
    }
}