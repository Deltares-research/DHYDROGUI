using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity(FireOnCollectionChange = false)]
    public class Weir : BranchStructure, IWeir
    {
        private IWeirFormula weirFormula;
        private double crestLevel;
        private double crestWidth;
        private bool canBeTimedependent;
        private bool useCrestLevelTimeSeries;
        private bool useCrestWidthTimeSeries;

        public Weir() : this("Weir") {}

        public Weir(bool allowTimeVaryingData = false) : this("Weir", allowTimeVaryingData) {}

        public Weir(string name, bool allowTimeVaryingData = false)
        {
            // todo: move initialization to demo model only
            WeirFormula = new SimpleWeirFormula
            {
                DischargeCoefficient = 1.0,
                LateralContraction = 1.0
            };
            Name = name;
            CrestWidth = 0.0;
            CrestLevel = 0.0;
            OffsetY = 0;
            FlowDirection = FlowDirection.Both;
            CrestShape = CrestShape.Sharp;
            CanBeTimedependent = allowTimeVaryingData;
        }

        [ReadOnly(true)]
        [DisplayName("Formula")]
        [FeatureAttribute(Order = 5)]
        public virtual string FormulaName => weirFormula.Name;

        public virtual bool CanBeTimedependent
        {
            get => canBeTimedependent;
            protected set
            {
                canBeTimedependent = value;
                OnCanBeTimeDependentSet();
            }
        }

        [FeatureAttribute(Order = 6)]
        public virtual double CrestWidth
        {
            get
            {
                if (weirFormula is GeneralStructureWeirFormula)
                {
                    return (weirFormula as GeneralStructureWeirFormula).WidthStructureCentre;
                }

                return crestWidth;
            }
            set
            {
                crestWidth = value;
                OnCrestWidthChanged();
            }
        }

        public virtual bool UseCrestLevelTimeSeries
        {
            get => useCrestLevelTimeSeries;
            set
            {
                if (!canBeTimedependent && value)
                {
                    throw new InvalidOperationException(
                        "Cannot use time series for crest level when time varying data is not allowed.");
                }

                useCrestLevelTimeSeries = value;
            }
        }

        [FeatureAttribute(Order = 7)]
        public virtual double CrestLevel
        {
            get
            {
                if (weirFormula is GeneralStructureWeirFormula)
                {
                    return (weirFormula as GeneralStructureWeirFormula).BedLevelStructureCentre;
                }

                return crestLevel;
            }
            set
            {
                crestLevel = value;
                OnCrestLevelChanged();
            }
        }

        public virtual TimeSeries CrestLevelTimeSeries { get; protected set; }

        public virtual IWeirFormula WeirFormula
        {
            get => weirFormula;
            set => weirFormula = value;
        }

        /// <summary>
        /// Secondary property : Gated or Not
        /// </summary>
        public virtual bool IsGated => WeirFormula is IGatedWeirFormula;

        /// <summary>
        /// Secondary property : Rectangle/FreeForm
        /// </summary>
        public virtual bool IsRectangle => WeirFormula.IsRectangle;

        public virtual bool AllowNegativeFlow
        {
            get
            {
                if (WeirFormula.HasFlowDirection)
                {
                    return FlowDirection == FlowDirection.Both ||
                           FlowDirection == FlowDirection.Negative;
                }

                return false;
            }
            set
            {
                if (value != AllowNegativeFlow)
                {
                    UpdateFlowDirection(AllowPositiveFlow, value);
                }
            }
        }

        public virtual bool AllowPositiveFlow
        {
            get
            {
                if (WeirFormula.HasFlowDirection)
                {
                    return FlowDirection == FlowDirection.Both ||
                           FlowDirection == FlowDirection.Positive;
                }

                return false;
            }
            set
            {
                if (value != AllowPositiveFlow)
                {
                    UpdateFlowDirection(value, AllowNegativeFlow);
                }
            }
        }

        /// <summary>
        /// Shape along the branch
        /// </summary>
        public virtual CrestShape CrestShape { get; set; }

        [DisplayName("Flow direction")]
        [FeatureAttribute(Order = 8, ExportName = "FlowDir")]
        public virtual FlowDirection FlowDirection { get; set; }

        public virtual bool SpecifyCrestLevelAndWidthOnWeir =>
            !(weirFormula is GeneralStructureWeirFormula);

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var copyFrom = (Weir) source;
            CrestWidth = copyFrom.CrestWidth;
            CrestLevel = copyFrom.CrestLevel;
            CrestShape = copyFrom.CrestShape;
            FlowDirection = copyFrom.FlowDirection;
            Attributes = (IFeatureAttributeCollection) copyFrom.Attributes.Clone();
            WeirFormula = (IWeirFormula) copyFrom.WeirFormula.Clone();
            CanBeTimedependent = copyFrom.CanBeTimedependent;

            if (!copyFrom.CanBeTimedependent)
            {
                return;
            }

            // Set time varying properties:
            UseCrestLevelTimeSeries = copyFrom.UseCrestLevelTimeSeries;
            CrestLevelTimeSeries = (TimeSeries) copyFrom.CrestLevelTimeSeries.Clone(true);
        }

        public override StructureType GetStructureType()
        {
            if (weirFormula is GatedWeirFormula)
            {
                return StructureType.Orifice;
            }

            if (weirFormula is SimpleWeirFormula)
            {
                return StructureType.Weir;
            }

            if (weirFormula is GeneralStructureWeirFormula)
            {
                return StructureType.GeneralStructure;
            }

            return StructureType.Unknown;
        }

        [EditAction]
        private void OnCanBeTimeDependentSet()
        {
            if (canBeTimedependent)
            {
                // For performance: initialize lazy
                if (CrestLevelTimeSeries == null)
                {
                    CrestLevelTimeSeries =
                        HydroTimeSeriesFactory.CreateTimeSeries(GuiParameterNames.CrestLevel,
                                                                GuiParameterNames.CrestLevel, "m AD");
                }
            }
            else
            {
                UseCrestLevelTimeSeries = false;

                CrestLevelTimeSeries = null;
            }
        }

        [EditAction]
        private void OnCrestLevelChanged()
        {
            if (weirFormula is GeneralStructureWeirFormula)
            {
                (weirFormula as GeneralStructureWeirFormula).BedLevelStructureCentre = crestLevel;
            }
        }

        [EditAction]
        private void OnCrestWidthChanged()
        {
            if (weirFormula is GeneralStructureWeirFormula)
            {
                (weirFormula as GeneralStructureWeirFormula).WidthStructureCentre = crestWidth;
            }
        }

        [EditAction]
        private void UpdateFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            if (WeirFormula.HasFlowDirection)
            {
                FlowDirection = GetPossibleFlowDirection(allowPositiveFlow, allowNegativeFlow);
            }
        }

        private static FlowDirection GetPossibleFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            return allowPositiveFlow
                       ? allowNegativeFlow ? FlowDirection.Both : FlowDirection.Positive
                       : allowNegativeFlow
                           ? FlowDirection.Negative
                           : FlowDirection.None;
        }
    }
}