using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    //TODO : change the names to match the sobek instead of the VIEW..
    [Entity(FireOnCollectionChange=false)]
    public class GeneralStructureWeirFormula : Unique<long>, IGatedWeirFormula
    {
        private readonly Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureWeirFormula, double>> SetKnownGeneralStructureProperty = new Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureWeirFormula, double>>
        {
            { KnownGeneralStructureProperties.WidthLeftW1, (f, v) => f.WidthLeftSideOfStructure = v },
            { KnownGeneralStructureProperties.WidthLeftWsdl, (f, v) => f.WidthStructureLeftSide = v },
            { KnownGeneralStructureProperties.WidthCenter, (f, v) => f.WidthStructureCentre = v },
            { KnownGeneralStructureProperties.WidthRightWsdr, (f, v) => f.WidthStructureRightSide = v },
            { KnownGeneralStructureProperties.WidthRightW2, (f, v) => f.WidthRightSideOfStructure = v },
            { KnownGeneralStructureProperties.LevelLeftZb1, (f, v) => f.BedLevelLeftSideOfStructure = v },
            { KnownGeneralStructureProperties.LevelLeftZbsl, (f, v) => f.BedLevelLeftSideStructure = v },
            { KnownGeneralStructureProperties.LevelCenter, (f, v) => f.BedLevelStructureCentre = v },
            { KnownGeneralStructureProperties.LevelRightZbsr, (f, v) => f.BedLevelRightSideStructure = v },
            { KnownGeneralStructureProperties.LevelRightZb2, (f, v) => f.BedLevelRightSideOfStructure = v },
            { KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient, (f, v) => f.PositiveFreeGateFlow = v },
            { KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient, (f, v) => f.PositiveDrownedGateFlow = v },
            { KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient, (f, v) => f.PositiveFreeWeirFlow = v },
            { KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient, (f, v) => f.PositiveDrownedWeirFlow = v },
            { KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate, (f, v) => f.PositiveContractionCoefficient = v },
            { KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient, (f, v) => f.NegativeFreeGateFlow = v },
            { KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient, (f, v) => f.NegativeDrownedGateFlow = v },
            { KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient, (f, v) => f.NegativeFreeWeirFlow = v },
            { KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient, (f, v) => f.NegativeDrownedWeirFlow = v },
            { KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate, (f, v) => f.NegativeContractionCoefficient = v },
            { KnownGeneralStructureProperties.ExtraResistance, (f, v) => { f.ExtraResistance = v; if (v == 0.0) f.UseExtraResistance = false; } },
            { KnownGeneralStructureProperties.GateDoorHeightGeneralStructure, (f, v) => f.GateOpening = v },
            { KnownGeneralStructureProperties.GateHeight, (f, v) => {/* do nothing */} }
        };

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

            GateOpening = 1.0;
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

        public virtual void SetPropertyValue(KnownGeneralStructureProperties propertyName, double value)
        {
            if (SetKnownGeneralStructureProperty.ContainsKey(propertyName))
            {
                SetKnownGeneralStructureProperty[propertyName](this, value);
            }
            else
            {
                throw new Exception("property name : {0} cannot be set for general structure weir formula");
            }
       }
    }
}