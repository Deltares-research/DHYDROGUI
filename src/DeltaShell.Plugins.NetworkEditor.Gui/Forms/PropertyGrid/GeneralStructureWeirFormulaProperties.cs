using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class GeneralStructureWeirFormulaProperties : WeirFormulaProperties
    {
        public GeneralStructureWeirFormulaProperties(GeneralStructureWeirFormula generalStructureWeirFormula, IWeir weir):base(generalStructureWeirFormula, weir)
        {
        }

        private GeneralStructureWeirFormula GeneralStructureWeirFormula
        {
            get { return (GeneralStructureWeirFormula)weirFormula; }
        }

        [DisplayName("Gate opening")]
        [ReadOnly(true)]
        public double GateOpening
        {
            get { return GeneralStructureWeirFormula.LowerEdgeLevel - weir.CrestLevel; }
        }

        [DisplayName("Gate height")]
        public double GateHeight
        {
            get { return GeneralStructureWeirFormula.GateHeight; }
            set { GeneralStructureWeirFormula.GateHeight = value; }
        }

        [DisplayName("Gate lower edge level")]
        public double LowerEdgeLevel
        {
            get { return GeneralStructureWeirFormula.LowerEdgeLevel; }
            set { GeneralStructureWeirFormula.LowerEdgeLevel = value; }
        }

        [DisplayName("Positive free gate flow")]
        public double PositiveFreeGateFlow
        {
            get { return GeneralStructureWeirFormula.PositiveFreeGateFlow; }
            set { GeneralStructureWeirFormula.PositiveFreeGateFlow = value; }
        }

        [DisplayName("Positive Drowned Gate Flow")]
        public double PositiveDrownedGateFlow
        {
            get { return GeneralStructureWeirFormula.PositiveDrownedGateFlow; }
            set { GeneralStructureWeirFormula.PositiveDrownedGateFlow = value; }
        }

        [DisplayName("Positive free weir flow")]
        public double PositiveFreeWeirFlow
        {
            get { return GeneralStructureWeirFormula.PositiveFreeWeirFlow; }
            set { GeneralStructureWeirFormula.PositiveFreeWeirFlow = value; }
        }

        [DisplayName("Positive drowned weir flow")]
        public double PositiveDrownedWeirFlow
        {
            get { return GeneralStructureWeirFormula.PositiveDrownedWeirFlow; }
            set { GeneralStructureWeirFormula.PositiveDrownedWeirFlow = value; }
        }

        [DisplayName("Positive contraction coefficient")]
        public double PositiveContractionCoefficient
        {
            get { return GeneralStructureWeirFormula.PositiveContractionCoefficient; }
            set { GeneralStructureWeirFormula.PositiveContractionCoefficient = value; }
        }

        [DisplayName("Negative free gate flow")]
        public double NegativeFreeGateFlow
        {
            get { return GeneralStructureWeirFormula.NegativeFreeGateFlow; }
            set { GeneralStructureWeirFormula.NegativeFreeGateFlow = value; }
        }

        [DisplayName("Negative drowned gate flow")]
        public double NegativeDrownedGateFlow
        {
            get { return GeneralStructureWeirFormula.NegativeDrownedGateFlow; }
            set { GeneralStructureWeirFormula.NegativeDrownedGateFlow = value; }
        }

        [DisplayName("Negative free weir flow")]
        public double NegativeFreeWeirFlow
        {
            get { return GeneralStructureWeirFormula.NegativeFreeWeirFlow; }
            set { GeneralStructureWeirFormula.NegativeFreeWeirFlow = value; }
        }

        [DisplayName("Negative drowned weir flow")]
        public double NegativeDrownedWeirFlow
        {
            get { return GeneralStructureWeirFormula.NegativeDrownedWeirFlow; }
            set { GeneralStructureWeirFormula.NegativeDrownedWeirFlow = value; }
        }

        [DisplayName("Negative contraction coefficient")]
        public double NegativeContractionCoefficient
        {
            get { return GeneralStructureWeirFormula.NegativeContractionCoefficient; }
            set { GeneralStructureWeirFormula.NegativeContractionCoefficient = value; }
        }

        [DisplayName("Width left side of structure")]
        public double WidthLeftSideOfStructure
        {
            get { return GeneralStructureWeirFormula.WidthLeftSideOfStructure; }
            set { GeneralStructureWeirFormula.WidthLeftSideOfStructure = value; }
        }

        [DisplayName("Width structure left side")]
        public double WidthStructureLeftSide
        {
            get { return GeneralStructureWeirFormula.WidthStructureLeftSide; }
            set { GeneralStructureWeirFormula.WidthStructureLeftSide = value; }
        }

        [DisplayName("Width structure centre")]
        public double WidthStructureCentre
        {
            get { return GeneralStructureWeirFormula.WidthStructureCentre; }
            set { GeneralStructureWeirFormula.WidthStructureCentre = value; }
        }

        [DisplayName("Width structure right side")]
        public double WidthStructureRightSide
        {
            get { return GeneralStructureWeirFormula.WidthStructureRightSide; }
            set { GeneralStructureWeirFormula.WidthStructureRightSide = value; }
        }

        [DisplayName("Width right side of structure")]
        public double WidthRightSideOfStructure
        {
            get { return GeneralStructureWeirFormula.WidthRightSideOfStructure; }
            set { GeneralStructureWeirFormula.WidthRightSideOfStructure = value; }
        }

        [DisplayName("Bed level left side of structure")]
        public double BedLevelLeftSideOfStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelLeftSideOfStructure; }
            set { GeneralStructureWeirFormula.BedLevelLeftSideOfStructure = value; }
        }

        [DisplayName("Bed level left side structure")]
        public double BedLevelLeftSideStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelLeftSideStructure; }
            set { GeneralStructureWeirFormula.BedLevelLeftSideStructure = value; }
        }

        [DisplayName("Bed level structure centre")]
        public double BedLevelStructureCentre
        {
            get { return GeneralStructureWeirFormula.BedLevelStructureCentre; }
            set { GeneralStructureWeirFormula.BedLevelStructureCentre = value; }
        }

        [DisplayName("Bed level right side structure")]
        public double BedLevelRightSideStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelRightSideStructure; }
            set { GeneralStructureWeirFormula.BedLevelRightSideStructure = value; }
        }

        [DisplayName("Bed level right side of structure")]
        public double BedLevelRightSideOfStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelRightSideOfStructure; }
            set { GeneralStructureWeirFormula.BedLevelRightSideOfStructure = value; }
        }

        [DisplayName("Use extra resistance")]
        [Description("Is extra resistance used.")]
        public bool UseExtraResistance
        {
            get { return GeneralStructureWeirFormula.UseExtraResistance; }
            set { GeneralStructureWeirFormula.UseExtraResistance = value; }
        }

        [DisplayName("Extra resistance")]
        public double ExtraResistance
        {
            get { return GeneralStructureWeirFormula.ExtraResistance; }
            set { GeneralStructureWeirFormula.ExtraResistance = value; }
        }
    }
}