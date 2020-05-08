using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class GeneralStructureWeirFormulaProperties : WeirFormulaProperties
    {
        public GeneralStructureWeirFormulaProperties(GeneralStructureWeirFormula generalStructureWeirFormula, IWeir weir) : base(generalStructureWeirFormula, weir) {}

        [DisplayName("Gate Opening")]
        public double GateOpening
        {
            get
            {
                return GeneralStructureWeirFormula.GateOpening;
            }
            set
            {
                GeneralStructureWeirFormula.GateOpening = value;
            }
        }

        [DisplayName("Positive Free Gate Flow")]
        public double PositiveFreeGateFlow
        {
            get
            {
                return GeneralStructureWeirFormula.PositiveFreeGateFlow;
            }
            set
            {
                GeneralStructureWeirFormula.PositiveFreeGateFlow = value;
            }
        }

        [DisplayName("Positive Drowned Gate Flow")]
        public double PositiveDrownedGateFlow
        {
            get
            {
                return GeneralStructureWeirFormula.PositiveDrownedGateFlow;
            }
            set
            {
                GeneralStructureWeirFormula.PositiveDrownedGateFlow = value;
            }
        }

        [DisplayName("Positive Free Weir Flow")]
        public double PositiveFreeWeirFlow
        {
            get
            {
                return GeneralStructureWeirFormula.PositiveFreeWeirFlow;
            }
            set
            {
                GeneralStructureWeirFormula.PositiveFreeWeirFlow = value;
            }
        }

        [DisplayName("Positive Drowned Weir Flow")]
        public double PositiveDrownedWeirFlow
        {
            get
            {
                return GeneralStructureWeirFormula.PositiveDrownedWeirFlow;
            }
            set
            {
                GeneralStructureWeirFormula.PositiveDrownedWeirFlow = value;
            }
        }

        [DisplayName("Positive Contraction Coefficient")]
        public double PositiveContractionCoefficient
        {
            get
            {
                return GeneralStructureWeirFormula.PositiveContractionCoefficient;
            }
            set
            {
                GeneralStructureWeirFormula.PositiveContractionCoefficient = value;
            }
        }

        [DisplayName("Negative Free Gate Flow")]
        public double NegativeFreeGateFlow
        {
            get
            {
                return GeneralStructureWeirFormula.NegativeFreeGateFlow;
            }
            set
            {
                GeneralStructureWeirFormula.NegativeFreeGateFlow = value;
            }
        }

        [DisplayName("Negative Drowned Gate Flow")]
        public double NegativeDrownedGateFlow
        {
            get
            {
                return GeneralStructureWeirFormula.NegativeDrownedGateFlow;
            }
            set
            {
                GeneralStructureWeirFormula.NegativeDrownedGateFlow = value;
            }
        }

        [DisplayName("Negative Free Weir Flow")]
        public double NegativeFreeWeirFlow
        {
            get
            {
                return GeneralStructureWeirFormula.NegativeFreeWeirFlow;
            }
            set
            {
                GeneralStructureWeirFormula.NegativeFreeWeirFlow = value;
            }
        }

        [DisplayName("Negative Drowned Weir Flow")]
        public double NegativeDrownedWeirFlow
        {
            get
            {
                return GeneralStructureWeirFormula.NegativeDrownedWeirFlow;
            }
            set
            {
                GeneralStructureWeirFormula.NegativeDrownedWeirFlow = value;
            }
        }

        [DisplayName("Negative Contraction Coefficient")]
        public double NegativeContractionCoefficient
        {
            get
            {
                return GeneralStructureWeirFormula.NegativeContractionCoefficient;
            }
            set
            {
                GeneralStructureWeirFormula.NegativeContractionCoefficient = value;
            }
        }

        [DisplayName("Width Left Side Of Structure")]
        public double WidthLeftSideOfStructure
        {
            get
            {
                return GeneralStructureWeirFormula.WidthLeftSideOfStructure;
            }
            set
            {
                GeneralStructureWeirFormula.WidthLeftSideOfStructure = value;
            }
        }

        [DisplayName("Width Structure Left Side")]
        public double WidthStructureLeftSide
        {
            get
            {
                return GeneralStructureWeirFormula.WidthStructureLeftSide;
            }
            set
            {
                GeneralStructureWeirFormula.WidthStructureLeftSide = value;
            }
        }

        [DisplayName("Width Structure Centre")]
        public double WidthStructureCentre
        {
            get
            {
                return GeneralStructureWeirFormula.WidthStructureCentre;
            }
            set
            {
                GeneralStructureWeirFormula.WidthStructureCentre = value;
            }
        }

        [DisplayName("Width Structure Right Side")]
        public double WidthStructureRightSide
        {
            get
            {
                return GeneralStructureWeirFormula.WidthStructureRightSide;
            }
            set
            {
                GeneralStructureWeirFormula.WidthStructureRightSide = value;
            }
        }

        [DisplayName("Width Right Side Of Structure")]
        public double WidthRightSideOfStructure
        {
            get
            {
                return GeneralStructureWeirFormula.WidthRightSideOfStructure;
            }
            set
            {
                GeneralStructureWeirFormula.WidthRightSideOfStructure = value;
            }
        }

        [DisplayName("Bed Level Left Side Of Structure")]
        public double BedLevelLeftSideOfStructure
        {
            get
            {
                return GeneralStructureWeirFormula.BedLevelLeftSideOfStructure;
            }
            set
            {
                GeneralStructureWeirFormula.BedLevelLeftSideOfStructure = value;
            }
        }

        [DisplayName("Bed Level Left Side Structure")]
        public double BedLevelLeftSideStructure
        {
            get
            {
                return GeneralStructureWeirFormula.BedLevelLeftSideStructure;
            }
            set
            {
                GeneralStructureWeirFormula.BedLevelLeftSideStructure = value;
            }
        }

        [DisplayName("Bed Level Structure Centre")]
        public double BedLevelStructureCentre
        {
            get
            {
                return GeneralStructureWeirFormula.BedLevelStructureCentre;
            }
            set
            {
                GeneralStructureWeirFormula.BedLevelStructureCentre = value;
            }
        }

        [DisplayName("Bed Level Right Side Structure")]
        public double BedLevelRightSideStructure
        {
            get
            {
                return GeneralStructureWeirFormula.BedLevelRightSideStructure;
            }
            set
            {
                GeneralStructureWeirFormula.BedLevelRightSideStructure = value;
            }
        }

        [DisplayName("Bed Level Right Side Of Structure")]
        public double BedLevelRightSideOfStructure
        {
            get
            {
                return GeneralStructureWeirFormula.BedLevelRightSideOfStructure;
            }
            set
            {
                GeneralStructureWeirFormula.BedLevelRightSideOfStructure = value;
            }
        }

        [DisplayName("Use Extra Resistance")]
        [Description("Is extra resistance used")]
        public bool UseExtraResistance
        {
            get
            {
                return GeneralStructureWeirFormula.UseExtraResistance;
            }
            set
            {
                GeneralStructureWeirFormula.UseExtraResistance = value;
            }
        }

        [DisplayName("Extra Resistance")]
        public double ExtraResistance
        {
            get
            {
                return GeneralStructureWeirFormula.ExtraResistance;
            }
            set
            {
                GeneralStructureWeirFormula.ExtraResistance = value;
            }
        }

        private GeneralStructureWeirFormula GeneralStructureWeirFormula
        {
            get
            {
                return (GeneralStructureWeirFormula) weirFormula;
            }
        }
    }
}