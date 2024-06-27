using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas
{
    [Entity(FireOnCollectionChange = false)]
    public class GeneralStructureFormula : Unique<long>, IGatedStructureFormula
    {
        private readonly Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureFormula, double>>
            SetKnownGeneralStructureProperty =
                new Dictionary<KnownGeneralStructureProperties, Action<GeneralStructureFormula, double>>
                {
                    {KnownGeneralStructureProperties.Upstream1Width, (f, v) => f.Upstream1Width = v},
                    {KnownGeneralStructureProperties.Upstream2Width, (f, v) => f.Upstream2Width = v},
                    {KnownGeneralStructureProperties.Downstream1Width, (f, v) => f.Downstream1Width = v},
                    {KnownGeneralStructureProperties.Downstream2Width, (f, v) => f.Downstream2Width = v},
                    {KnownGeneralStructureProperties.Upstream1Level, (f, v) => f.Upstream1Level = v},
                    {KnownGeneralStructureProperties.Upstream2Level, (f, v) => f.Upstream2Level = v},
                    {KnownGeneralStructureProperties.Downstream1Level, (f, v) => f.Downstream1Level = v},
                    {KnownGeneralStructureProperties.Downstream2Level, (f, v) => f.Downstream2Level = v},
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
                    {KnownGeneralStructureProperties.GateHeight, (f, v) => f.GateHeight = v}
                };

        private bool useHorizontalGateOpeningWidthTimeSeries;

        private bool useGateLowerEdgeLevelTimeSeries;

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
        public virtual double Upstream1Width { get; set; }

        /// <summary>
        /// wl
        /// </summary>
        public virtual double Upstream2Width { get; set; }

        /// <summary>
        /// ws
        /// </summary>
        public virtual double CrestWidth { get; set; }

        /// <summary>
        /// wr
        /// </summary>
        public virtual double Downstream1Width { get; set; }

        /// <summary>
        /// w2
        /// </summary>
        public virtual double Downstream2Width { get; set; }

        /// <summary>
        /// z1
        /// </summary>
        public virtual double Upstream1Level { get; set; }

        /// <summary>
        /// zl
        /// </summary>
        public virtual double Upstream2Level { get; set; }

        /// <summary>
        /// zs
        /// </summary>
        public virtual double CrestLevel { get; set; }

        /// <summary>
        /// zr
        /// </summary>
        public virtual double Downstream1Level { get; set; }

        /// <summary>
        /// z2
        /// </summary>
        public virtual double Downstream2Level { get; set; }

        /// <summary>
        /// Is extra resistance used? Used to bind to the view.
        /// </summary>
        public virtual bool UseExtraResistance { get; set; }

        /// <summary>
        /// er
        /// </summary>
        public virtual double ExtraResistance { get; set; }

        public virtual string Name => "General Structure";

        public virtual double GateHeight { get; set; }

        public virtual GateOpeningDirection GateOpeningHorizontalDirection { get; set; }
        public virtual double HorizontalGateOpeningWidth { get; set; }

        public virtual bool UseHorizontalGateOpeningWidthTimeSeries
        {
            get => useHorizontalGateOpeningWidthTimeSeries;
            set
            {
                useHorizontalGateOpeningWidthTimeSeries = value;
                
                if (useHorizontalGateOpeningWidthTimeSeries && HorizontalGateOpeningWidthTimeSeries == null)
                {
                    HorizontalGateOpeningWidthTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.HorizontalGateOpeningWidth, GuiParameterNames.HorizontalGateOpeningWidth, "m AD");
                }
            }
        }

        public virtual TimeSeries HorizontalGateOpeningWidthTimeSeries { get; set; }

        public virtual double GateLowerEdgeLevel { get; set; }

        /// <summary>
        /// When true, use <see cref="GateLowerEdgeLevelTimeSeries"/>, else use <see cref="GateOpening"/>.
        /// </summary>
        public virtual bool UseGateLowerEdgeLevelTimeSeries
        {
            get => useGateLowerEdgeLevelTimeSeries;
            set
            {
                useGateLowerEdgeLevelTimeSeries = value;
                
                if (useGateLowerEdgeLevelTimeSeries && GateLowerEdgeLevelTimeSeries == null)
                {
                    GateLowerEdgeLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.GateLowerEdgeLevel, GuiParameterNames.GateLowerEdgeLevel, "m AD");
                }
            }
        }

        /// <summary>
        /// Time dependent Lower edge level
        /// </summary>
        public virtual TimeSeries GateLowerEdgeLevelTimeSeries { get; set; }

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
                Upstream1Level = Upstream1Level,
                Upstream2Level = Upstream2Level,
                CrestLevel = CrestLevel,
                Downstream1Level = Downstream1Level,
                Downstream2Level = Downstream2Level,
                Upstream1Width = Upstream1Width,
                Upstream2Width = Upstream2Width,
                CrestWidth = CrestWidth,
                Downstream1Width = Downstream1Width,
                Downstream2Width = Downstream2Width,
                UseExtraResistance = UseExtraResistance,
                ExtraResistance = ExtraResistance,
                GateOpening = GateOpening,
                GateHeight = GateHeight,
                GateOpeningHorizontalDirection = GateOpeningHorizontalDirection,
                HorizontalGateOpeningWidth = HorizontalGateOpeningWidth,
                UseHorizontalGateOpeningWidthTimeSeries = UseHorizontalGateOpeningWidthTimeSeries,
                GateLowerEdgeLevel = GateLowerEdgeLevel,
                UseGateLowerEdgeLevelTimeSeries = UseGateLowerEdgeLevelTimeSeries
            };

            if (clone.UseGateLowerEdgeLevelTimeSeries)
            {
                clone.GateLowerEdgeLevelTimeSeries = (TimeSeries) GateLowerEdgeLevelTimeSeries.Clone(true);
            }

            if (clone.UseHorizontalGateOpeningWidthTimeSeries)
            {
                clone.HorizontalGateOpeningWidthTimeSeries =
                    (TimeSeries) HorizontalGateOpeningWidthTimeSeries.Clone(true);
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

            GateHeight = 0.0;

            GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric;
            HorizontalGateOpeningWidth = 0.0;

            UseHorizontalGateOpeningWidthTimeSeries = false;
            HorizontalGateOpeningWidthTimeSeries = null;

            GateLowerEdgeLevel = 0.0;
            UseGateLowerEdgeLevelTimeSeries = false;
            GateLowerEdgeLevelTimeSeries = null;
        }
    }
}