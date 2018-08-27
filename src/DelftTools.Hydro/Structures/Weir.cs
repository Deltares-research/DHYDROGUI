using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity(FireOnCollectionChange=false)]
    public class Weir : BranchStructure, IWeir
    {
        private IWeirFormula weirFormula;
        private double crestLevel;
        private double crestWidth;
        private bool canBeTimedependent;
        private bool useCrestLevelTimeSeries;
        private bool useCrestWidthTimeSeries;

        public Weir() : this("Weir")
        {
        }

        public Weir(bool allowTimeVaryingData = false) :this("Weir", allowTimeVaryingData) { }

        public Weir(string name, bool allowTimeVaryingData = false)
        {
            // todo: move initialization to demo model only
            WeirFormula = new SimpleWeirFormula
                {
                    DischargeCoefficient = 1.0,
                    LateralContraction = 1.0
                };
            Name = name;
            CrestWidth = 5;
            CrestLevel = 1;
            OffsetY = 0;
            FlowDirection = FlowDirection.Both;
            CrestShape = CrestShape.Sharp;
            CanBeTimedependent = allowTimeVaryingData;
        }

        public virtual bool CanBeTimedependent
        {
            get { return canBeTimedependent; }
            protected set
            {
                canBeTimedependent = value;
                OnCanBeTimeDependentSet();
            }
        }

        [EditAction]
        private void OnCanBeTimeDependentSet()
        {
            if (canBeTimedependent)
            {
                // For performance: initialize lazy
                if (CrestLevelTimeSeries == null)
                    CrestLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Crest level", "Crest level", "m AD");
            }
            else
            {
                UseCrestLevelTimeSeries = false;

                CrestLevelTimeSeries = null;
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
                if (weirFormula is FreeFormWeirFormula)
                {
                    return (weirFormula as FreeFormWeirFormula).CrestWidth;
                }
                return crestWidth;
            }
            set { crestWidth = value; }
        }

        public virtual bool UseCrestLevelTimeSeries
        {
            get { return useCrestLevelTimeSeries; }
            set
            {
                if (!canBeTimedependent && value)
                    throw new InvalidOperationException("Cannot use time series for crest level when time varying data is not allowed.");

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
                if (weirFormula is FreeFormWeirFormula)
                {
                    return (weirFormula as FreeFormWeirFormula).CrestLevel;
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

        [EditAction]
        private void OnCrestLevelChanged()
        {
            if (weirFormula is GeneralStructureWeirFormula)
            {
                (weirFormula as GeneralStructureWeirFormula).BedLevelStructureCentre = crestLevel;
            }
        }

        public virtual IWeirFormula WeirFormula
        {
            get { return weirFormula; }
            set { weirFormula = value; }
        }
        
        [ReadOnly(true)]
        [DisplayName("Formula")]
        [FeatureAttribute(Order = 5)]
        public virtual string FormulaName
        {
            get { return weirFormula.Name; }
        }
        
        /// <summary>
        /// Secondary property : Gated or Not
        /// </summary>
        public virtual bool IsGated
        {
            get { return WeirFormula is IGatedWeirFormula; }
        }

        /// <summary>
        /// Secondary property : Rectangle/FreeForm
        /// </summary>
        public virtual bool IsRectangle
        {
            get { return WeirFormula.IsRectangle; }
        }

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

        [EditAction]
        private void UpdateFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            if (WeirFormula.HasFlowDirection)
            {
                FlowDirection = GetPossibleFlowDirection(allowPositiveFlow, allowNegativeFlow);
            }
        }

        /// <summary>
        /// Shape along the branch
        /// </summary>
        public virtual CrestShape CrestShape { get; set; }

        [DisplayName("Flow direction")]
        [FeatureAttribute(Order = 8, ExportName = "FlowDir")]
        public virtual FlowDirection FlowDirection { get; set; }
        
        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var copyFrom = (Weir) source;
            CrestWidth = copyFrom.CrestWidth;
            CrestLevel = copyFrom.CrestLevel;
            CrestShape = copyFrom.CrestShape;
            FlowDirection = copyFrom.FlowDirection;
            Attributes = (IFeatureAttributeCollection)copyFrom.Attributes.Clone();
            WeirFormula = (IWeirFormula)copyFrom.WeirFormula.Clone();
            CanBeTimedependent = copyFrom.CanBeTimedependent;

            if (!copyFrom.CanBeTimedependent) return;

            // Set time varying properties:
            UseCrestLevelTimeSeries = copyFrom.UseCrestLevelTimeSeries;
            CrestLevelTimeSeries = (TimeSeries)copyFrom.CrestLevelTimeSeries.Clone(true);
        }

        private static FlowDirection GetPossibleFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            return allowPositiveFlow
                       ? (allowNegativeFlow ? FlowDirection.Both : FlowDirection.Positive)
                       : (allowNegativeFlow ? FlowDirection.Negative : FlowDirection.None);
        }

        public virtual bool SpecifyCrestLevelAndWidthOnWeir
        {
            get { return !(weirFormula is GeneralStructureWeirFormula || weirFormula is FreeFormWeirFormula); }
        }

        public override StructureType GetStructureType()
        {
            if (weirFormula is FreeFormWeirFormula) return StructureType.UniversalWeir;
            if (weirFormula is GatedWeirFormula) return StructureType.Orifice;
            if (weirFormula is PierWeirFormula) return StructureType.AdvancedWeir;
            if (weirFormula is RiverWeirFormula) return StructureType.RiverWeir;
            if (weirFormula is SimpleWeirFormula) return StructureType.Weir;
            if (weirFormula is GeneralStructureWeirFormula) return StructureType.GeneralStructure;
            return StructureType.Unknown;
        }

        public virtual void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(
                sc => sc.BranchFeatures.Any()
                      && sc.BranchFeatures[1].Name == Name
                      && sc.BranchFeatures[1] is IWeir);
            var weir = sewerConnection?.BranchFeatures.FirstOrDefault(bf => bf is IWeir) as IWeir;

            if (weir != null)
            {
                CopyPropertyValuesToExistingWeir(weir);
            }
            else
            {
                sewerConnection = GetNewSewerConnectionWithWeir();
                sewerConnection.AddToHydroNetwork(hydroNetwork);
            }

            sewerConnection.UpdateBranchFeatureGeometries();
        }

        protected virtual ISewerConnection GetNewSewerConnectionWithWeir()
        {
            return null;
        }

        protected virtual void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
        }
    }
}