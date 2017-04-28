using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    //TODO : change the names to match the sobek instead of the VIEW..
    [Entity(FireOnCollectionChange=false)]
    public class GeneralStructureWeirFormula : Unique<long>, IGatedWeirFormula
    {
        public GeneralStructureWeirFormula()
        {
            Initialize();
        }

        private void Initialize()
        {
            PositiveFreeGateFlow = 1.0;
            PositiveContractionCoefficient = 1.0;
            PositiveDrownedGateFlow = 1.0;
            PositiveDrownedWeirFlow = 1.0;
            PositiveFreeWeirFlow = 1.0;

            NegativeContractionCoefficient = 1.0;
            NegativeDrownedGateFlow = 1.0;
            NegativeDrownedWeirFlow = 1.0;
            NegativeFreeGateFlow = 1.0;
            NegativeFreeWeirFlow = 1.0;

            UseExtraResistance = true;
            ExtraResistance = 0.0;
        }

        public virtual object Clone()
        {
            var clone = new GeneralStructureWeirFormula
                {
                    PositiveFreeGateFlow = PositiveFreeGateFlow,
                    PositiveContractionCoefficient = PositiveContractionCoefficient,
                    PositiveDrownedGateFlow = PositiveDrownedGateFlow,
                    PositiveDrownedWeirFlow = PositiveDrownedWeirFlow,
                    PositiveFreeWeirFlow = PositiveFreeWeirFlow,

                    NegativeContractionCoefficient = NegativeContractionCoefficient,
                    NegativeDrownedGateFlow = NegativeDrownedGateFlow,
                    NegativeDrownedWeirFlow = NegativeDrownedWeirFlow,
                    NegativeFreeGateFlow = NegativeFreeGateFlow,
                    NegativeFreeWeirFlow = NegativeFreeWeirFlow,

                    BedLevelLeftSideOfStructure = BedLevelLeftSideOfStructure,
                    BedLevelLeftSideStructure = BedLevelLeftSideStructure,
                    BedLevelStructureCentre = BedLevelStructureCentre,
                    BedLevelRightSideStructure = BedLevelRightSideStructure,
                    BedLevelRightSideOfStructure = BedLevelRightSideOfStructure,

                    WidthLeftSideOfStructure = WidthLeftSideOfStructure,
                    WidthStructureLeftSide = WidthStructureLeftSide,
                    WidthStructureCentre = WidthStructureCentre,
                    WidthStructureRightSide = WidthStructureRightSide,
                    WidthRightSideOfStructure = WidthRightSideOfStructure,

                    UseExtraResistance = UseExtraResistance,
                    ExtraResistance = ExtraResistance,
                    GateOpening = GateOpening
                };
            return clone;
        }

        public virtual string Name
        {
            get { return "General structure"; }
        }

        public virtual bool IsRectangle
        {
            get { return false; }
        }

        public virtual bool HasFlowDirection
        {
            get { return false; }
        }

        /// <summary>
        /// pg
        /// </summary>
        public virtual double PositiveFreeGateFlow { get; set; }

        /// <summary>
        /// pd
        /// </summary>
        public virtual double PositiveDrownedGateFlow { get; set; }

        /// <summary>
        /// pi
        /// </summary>
        public virtual double PositiveFreeWeirFlow { get; set; }

        /// <summary>
        /// pr
        /// </summary>
        public virtual double PositiveDrownedWeirFlow { get; set; }

        /// <summary>
        /// pc
        /// </summary>
        public virtual double PositiveContractionCoefficient { get; set; }

        /// <summary>
        /// ng
        /// </summary>
        public virtual double NegativeFreeGateFlow { get; set; }

        /// <summary>
        /// nd
        /// </summary>
        public virtual double NegativeDrownedGateFlow { get; set; }

        /// <summary>
        /// nf
        /// </summary>
        public virtual double NegativeFreeWeirFlow { get; set; }

        /// <summary>
        /// nr
        /// </summary>
        public virtual double NegativeDrownedWeirFlow { get; set; }

        /// <summary>
        /// nc
        /// </summary>
        public virtual double NegativeContractionCoefficient { get; set; }

        /// <summary>
        /// w1
        /// </summary>
        public virtual double WidthLeftSideOfStructure { get; set; }

        /// <summary>
        /// wl
        /// </summary>
        public virtual double WidthStructureLeftSide { get; set; }

        /// <summary>
        /// ws
        /// </summary>
        public virtual double WidthStructureCentre { get; set; }

        /// <summary>
        /// wr
        /// </summary>
        public virtual double WidthStructureRightSide { get; set; }

        /// <summary>
        /// w2
        /// </summary>
        public virtual double WidthRightSideOfStructure { get; set; }

        /// <summary>
        /// z1
        /// </summary>
        public virtual double BedLevelLeftSideOfStructure { get; set; }

        /// <summary>
        /// zl
        /// </summary>
        public virtual double BedLevelLeftSideStructure { get; set; }

        /// <summary>
        /// zs
        /// </summary>
        public virtual double BedLevelStructureCentre { get; set; }

        /// <summary>
        /// zr
        /// </summary>
        public virtual double BedLevelRightSideStructure { get; set; }

        /// <summary>
        /// z2
        /// </summary>
        public virtual double BedLevelRightSideOfStructure { get; set; }

        /// <summary>
        /// Is extra resistance used? Used to bind to the view.
        /// </summary>
        public virtual bool UseExtraResistance { get; set; }

        /// <summary>
        /// er
        /// </summary>
        public virtual double ExtraResistance { get; set; }

        /// <summary>
        /// Gateopening = GateHeight (gle) - level at crest
        /// </summary>
        public virtual double GateOpening { get; set; }
    }
}