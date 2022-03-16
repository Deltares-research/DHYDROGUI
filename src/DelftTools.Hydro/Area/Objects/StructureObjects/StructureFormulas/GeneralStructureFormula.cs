using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas
{
    [Entity(FireOnCollectionChange = false)]
    public class GeneralStructureFormula : Unique<long>, IGatedStructureFormula
    {
        private readonly Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureFormula, double>>
            SetKnownGeneralStructureProperty =
                new Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureFormula, double>>
                {
                    {KnownGeneralStructureProperties.Upstream2Width, (f, v) => f.WidthLeftSideOfStructure = v},
                    {KnownGeneralStructureProperties.Upstream1Width, (f, v) => f.WidthStructureLeftSide = v},
                    {KnownGeneralStructureProperties.Downstream1Width, (f, v) => f.WidthStructureRightSide = v},
                    {KnownGeneralStructureProperties.Downstream2Width, (f, v) => f.WidthRightSideOfStructure = v},
                    {KnownGeneralStructureProperties.Upstream2Level, (f, v) => f.BedLevelLeftSideOfStructure = v},
                    {KnownGeneralStructureProperties.Upstream1Level, (f, v) => f.BedLevelLeftSideStructure = v},
                    {KnownGeneralStructureProperties.Downstream1Level, (f, v) => f.BedLevelRightSideStructure = v},
                    {KnownGeneralStructureProperties.Downstream2Level, (f, v) => f.BedLevelRightSideOfStructure = v},
                    {KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient, (f, v) => f.PositiveFreeGateFlow = v},
                    {KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient, (f, v) => f.PositiveDrownedGateFlow = v},
                    {KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient, (f, v) => f.PositiveFreeWeirFlow = v},
                    {KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient, (f, v) => f.PositiveDrownedWeirFlow = v},
                    {KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate, (f, v) => f.PositiveContractionCoefficient = v},
                    {KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient, (f, v) => f.NegativeFreeGateFlow = v},
                    {KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient, (f, v) => f.NegativeDrownedGateFlow = v},
                    {KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient, (f, v) => f.NegativeFreeWeirFlow = v},
                    {KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient, (f, v) => f.NegativeDrownedWeirFlow = v},
                    {KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate, (f, v) => f.NegativeContractionCoefficient = v},
                    {
                        KnownGeneralStructureProperties.ExtraResistance, (f, v) =>
                        {
                            f.ExtraResistance = v;
                            if (v == 0.0)
                            {
                                f.UseExtraResistance = false;
                            }
                        }
                    },
                    {KnownGeneralStructureProperties.GateHeight, (f, v) => f.DoorHeight = v}
                };

        private bool useHorizontalDoorOpeningWidthTimeSeries;

        private bool useLowerEdgeLevelTimeSeries;

        public GeneralStructureFormula()
        {
            Initialize();
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

        public virtual string Name => "General Structure";

        public virtual double DoorHeight { get; set; }

        public virtual GateOpeningDirection HorizontalDoorOpeningDirection { get; set; }
        public virtual double HorizontalDoorOpeningWidth { get; set; }

        public virtual bool UseHorizontalDoorOpeningWidthTimeSeries
        {
            get => useHorizontalDoorOpeningWidthTimeSeries;
            set
            {
                useHorizontalDoorOpeningWidthTimeSeries = value;
                
                if (useHorizontalDoorOpeningWidthTimeSeries && HorizontalDoorOpeningWidthTimeSeries == null)
                {
                    HorizontalDoorOpeningWidthTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.HorizontalOpeningWidth, GuiParameterNames.HorizontalOpeningWidth, "m AD");
                }
            }
        }

        public virtual TimeSeries HorizontalDoorOpeningWidthTimeSeries { get; set; }

        public virtual double LowerEdgeLevel { get; set; }

        /// <summary>
        /// When true, use <see cref="LowerEdgeLevelTimeSeries"/>, else use <see cref="GateOpening"/>.
        /// </summary>
        public virtual bool UseLowerEdgeLevelTimeSeries
        {
            get => useLowerEdgeLevelTimeSeries;
            set
            {
                useLowerEdgeLevelTimeSeries = value;
                
                if (useLowerEdgeLevelTimeSeries && LowerEdgeLevelTimeSeries == null)
                {
                    LowerEdgeLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.GateLowerEdgeLevel, GuiParameterNames.GateLowerEdgeLevel, "m AD");
                }
            }
        }

        /// <summary>
        /// Time dependent Lower edge level
        /// </summary>
        public virtual TimeSeries LowerEdgeLevelTimeSeries { get; set; }

        /// <summary>
        /// Gateopening = GateHeight (gle) - level at crest
        /// </summary>
        public virtual double GateOpening { get; set; }

        /// <summary>
        /// Sets a general structure weir formula property.
        /// </summary>
        /// <param name="property"> The property to be set. </param>
        /// <param name="value"> The new value of the property. </param>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="property"/> is not a defined <see cref="KnownGeneralStructureProperties"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="property"/> is not a valid property.
        /// This includes:
        /// - <see cref="KnownGeneralStructureProperties.CrestWidth"/>
        /// - <see cref="KnownGeneralStructureProperties.CrestLevel"/>
        /// - <see cref="KnownGeneralStructureProperties.GateLowerEdgeLevel"/>
        /// - <see cref="KnownGeneralStructureProperties.GateOpeningHorizontalDirection"/>
        /// - <see cref="KnownGeneralStructureProperties.GateOpeningWidth"/>
        /// </exception>
        public virtual void SetPropertyValue(KnownGeneralStructureProperties property, double value)
        {
            Ensure.IsDefined(property, nameof(property));

            if (SetKnownGeneralStructureProperty.ContainsKey(property))
            {
                SetKnownGeneralStructureProperty[property](this, value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(property), $"Property {property} is not a valid property of a general structure weir formula.");
            }
        }

        public virtual object Clone()
        {
            var clone = new GeneralStructureFormula
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
                DoorHeight = DoorHeight,
                HorizontalDoorOpeningDirection = HorizontalDoorOpeningDirection,
                HorizontalDoorOpeningWidth = HorizontalDoorOpeningWidth,
                UseHorizontalDoorOpeningWidthTimeSeries = UseHorizontalDoorOpeningWidthTimeSeries,
                LowerEdgeLevel = LowerEdgeLevel,
                UseLowerEdgeLevelTimeSeries = UseLowerEdgeLevelTimeSeries
            };

            if (clone.UseLowerEdgeLevelTimeSeries)
            {
                clone.LowerEdgeLevelTimeSeries = (TimeSeries) LowerEdgeLevelTimeSeries.Clone(true);
            }

            if (clone.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                clone.HorizontalDoorOpeningWidthTimeSeries =
                    (TimeSeries) HorizontalDoorOpeningWidthTimeSeries.Clone(true);
            }

            return clone;
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

            DoorHeight = 0.0;

            HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric;
            HorizontalDoorOpeningWidth = 0.0;

            UseHorizontalDoorOpeningWidthTimeSeries = false;
            HorizontalDoorOpeningWidthTimeSeries = null;

            LowerEdgeLevel = 0.0;
            UseLowerEdgeLevelTimeSeries = false;
            LowerEdgeLevelTimeSeries = null;
        }
    }
}