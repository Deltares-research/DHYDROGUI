using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    [Entity(FireOnCollectionChange=false)]
    public class GeneralStructureWeirFormula : Unique<long>, IGatedWeirFormula
    {
        private readonly Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureWeirFormula, double>> SetKnownGeneralStructureProperty = new Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureWeirFormula, double>>
        {
            { KnownGeneralStructureProperties.Upstream1Width, (f, v) => f.WidthLeftSideOfStructure = v },
            { KnownGeneralStructureProperties.Upstream2Width, (f, v) => f.WidthStructureLeftSide = v },
            { KnownGeneralStructureProperties.CrestWidth, (f, v) => f.WidthStructureCentre = v },
            { KnownGeneralStructureProperties.Downstream1Width, (f, v) => f.WidthStructureRightSide = v },
            { KnownGeneralStructureProperties.Downstream2Width, (f, v) => f.WidthRightSideOfStructure = v },
            { KnownGeneralStructureProperties.Upstream1Level, (f, v) => f.BedLevelLeftSideOfStructure = v },
            { KnownGeneralStructureProperties.Upstream2Level, (f, v) => f.BedLevelLeftSideStructure = v },
            { KnownGeneralStructureProperties.CrestLevel, (f, v) => f.BedLevelStructureCentre = v },
            { KnownGeneralStructureProperties.Downstream1Level, (f, v) => f.BedLevelRightSideStructure = v },
            { KnownGeneralStructureProperties.Downstream2Level, (f, v) => f.BedLevelRightSideOfStructure = v },
            { KnownGeneralStructureProperties.PosFreeGateFlowCoeff, (f, v) => f.PositiveFreeGateFlow = v },
            { KnownGeneralStructureProperties.PosDrownGateFlowCoeff, (f, v) => f.PositiveDrownedGateFlow = v },
            { KnownGeneralStructureProperties.PosFreeWeirFlowCoeff, (f, v) => f.PositiveFreeWeirFlow = v },
            { KnownGeneralStructureProperties.PosDrownWeirFlowCoeff, (f, v) => f.PositiveDrownedWeirFlow = v },
            { KnownGeneralStructureProperties.PosContrCoefFreeGate, (f, v) => f.PositiveContractionCoefficient = v },
            { KnownGeneralStructureProperties.NegFreeGateFlowCoeff, (f, v) => f.NegativeFreeGateFlow = v },
            { KnownGeneralStructureProperties.NegDrownGateFlowCoeff, (f, v) => f.NegativeDrownedGateFlow = v },
            { KnownGeneralStructureProperties.NegFreeWeirFlowCoeff, (f, v) => f.NegativeFreeWeirFlow = v },
            { KnownGeneralStructureProperties.NegDrownWeirFlowCoeff, (f, v) => f.NegativeDrownedWeirFlow = v },
            { KnownGeneralStructureProperties.NegContrCoefFreeGate, (f, v) => f.NegativeContractionCoefficient = v },
            { KnownGeneralStructureProperties.ExtraResistance, (f, v) => { f.ExtraResistance = v; if (v == 0.0) f.UseExtraResistance = false; }},
            { KnownGeneralStructureProperties.GateHeight, (f, v) => f.GateHeight = v },
            { KnownGeneralStructureProperties.GateLowerEdgeLevel, (f, v) => f.LowerEdgeLevel = v },
            { KnownGeneralStructureProperties.GateOpeningWidth, (f, v) => f.GateOpeningWidth = v },
            { KnownGeneralStructureProperties.CrestLength, (f, v) => f.CrestLength = v },
            { KnownGeneralStructureProperties.UseVelocityHeight, (f, v) => f.UseVelocityHeight = Convert.ToBoolean(v) },
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
            GateHeight = 1.0;
            LowerEdgeLevel = 11.0;
            GateOpeningWidth = 0.0;
            CrestLength = 0.0;
            GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric;
            UseVelocityHeight = true;
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
                    GateOpening = GateOpening,
                    GateOpeningWidth = GateOpeningWidth,
                    CrestLength = CrestLength,
                    GateOpeningHorizontalDirection = GateOpeningHorizontalDirection,
                    UseVelocityHeight = UseVelocityHeight,
                    LowerEdgeLevel = LowerEdgeLevel,
                    GateHeight = GateHeight
                };
            return clone;
        }

        /// <inheritdoc />
        public virtual string Name
        {
            get { return "General structure"; }
        }

        /// <inheritdoc />
        public virtual bool IsRectangle
        {
            get { return false; }
        }

        /// <inheritdoc />
        public virtual bool HasFlowDirection
        {
            get { return true; }
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

        /// <inheritdoc />
        public virtual double GateOpening { get; set; }

        /// <inheritdoc />
        public virtual double GateHeight { get; set; }

        /// <inheritdoc />
        public virtual double LowerEdgeLevel { get; set; }

        public virtual double GateOpeningWidth { get; set; }

        public virtual double CrestLength { get; set; }

        public virtual bool UseVelocityHeight { get; set; }

        public virtual GateOpeningDirection GateOpeningHorizontalDirection { get; set; }

        public virtual void SetPropertyValue(KnownGeneralStructureProperties propertyName, double value)
        {
            if (SetKnownGeneralStructureProperty.TryGetValue(propertyName, out var setProperty))
            {
                setProperty(this, value);
            }
            else
            {
                throw new Exception("property name : {0} cannot be set for general structure weir formula");
            }
        }
    }
}