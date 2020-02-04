using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DelftTools.Hydro.Structures
{
    [Entity(FireOnCollectionChange=false)]
    public class Weir : BranchStructure, IWeir
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Weir));

        private double crestLevel;
        private double crestWidth;
        private bool canBeTimedependent;
        private bool useCrestLevelTimeSeries;
        private bool useCrestWidthTimeSeries;
        private bool useVelocityHeight;

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
            UseVelocityHeight = true;
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
                if (WeirFormula is GeneralStructureWeirFormula)
                {
                    return (WeirFormula as GeneralStructureWeirFormula).WidthStructureCentre;
                }
                if (WeirFormula is FreeFormWeirFormula)
                {
                    return (WeirFormula as FreeFormWeirFormula).CrestWidth;
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
                if (WeirFormula is GeneralStructureWeirFormula)
                {
                    return (WeirFormula as GeneralStructureWeirFormula).BedLevelStructureCentre;
                }
                if (WeirFormula is FreeFormWeirFormula)
                {
                    return (WeirFormula as FreeFormWeirFormula).CrestLevel;
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
            if (WeirFormula is GeneralStructureWeirFormula)
            {
                (WeirFormula as GeneralStructureWeirFormula).BedLevelStructureCentre = crestLevel;
            }
        }

        public virtual IWeirFormula WeirFormula { get; set; }
        
        [ReadOnly(true)]
        [DisplayName("Formula")]
        [FeatureAttribute(Order = 5)]
        public virtual string FormulaName
        {
            get { return WeirFormula.Name; }
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
            get { return !(WeirFormula is GeneralStructureWeirFormula || WeirFormula is FreeFormWeirFormula); }
        }

        public virtual bool UseVelocityHeight { get; set; }

        public override StructureType GetStructureType()
        {
            if (WeirFormula is FreeFormWeirFormula) return StructureType.UniversalWeir;
            if (WeirFormula is GatedWeirFormula) return StructureType.Orifice;
            if (WeirFormula is PierWeirFormula) return StructureType.AdvancedWeir;
            if (WeirFormula is RiverWeirFormula) return StructureType.RiverWeir;
            if (WeirFormula is SimpleWeirFormula) return StructureType.Weir;
            if (WeirFormula is GeneralStructureWeirFormula) return StructureType.GeneralStructure;
            return StructureType.Unknown;
        }
        public virtual void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            var sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(
                sc => sc.BranchFeatures.Count >= 2
                      && sc.BranchFeatures[1].Name == Name
                      && sc.BranchFeatures[1] is IWeir);
            var weir = hydroNetwork.Weirs.FirstOrDefault(o => o.Name == Name);

            if (weir != null)
            {
                hydroNetwork.Branches.Remove(sewerConnection);
                CopyPropertyValuesToExistingWeir(weir);
                SetSewerConnectionProperties(sewerConnection);
                sewerConnection?.UpdateBranchFeatureGeometries();
                sewerConnection?.AddToHydroNetwork(hydroNetwork, helper);
                return;
            }

            sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(sc => sc.Name == Name);
            if (sewerConnection == null)
            {
                AddNewSewerConnectionWithWeirToNetwork(hydroNetwork, helper);
            }
            else
            {
                AddWeirToSewerConnection(sewerConnection);
            }
        }

        protected virtual void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
        }

        private void AddNewSewerConnectionWithWeirToNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            var sewerConnection = new SewerConnection(Name);
            SetSewerConnectionProperties(sewerConnection);

            var composite = sewerConnection.AddStructureToBranch(this);
            composite.Name = HydroNetworkHelper.GetUniqueFeatureName(hydroNetwork, composite);
            sewerConnection.AddToHydroNetwork(hydroNetwork, helper);
        }

        protected virtual void SetSewerConnectionProperties(ISewerConnection sewerConnection)
        {
        }

        private void AddWeirToSewerConnection(ISewerConnection sewerConnection)
        {
            if (sewerConnection.BranchFeatures.Count > 0)
            {
                RemoveExistingBranchFeatures(sewerConnection);
            }
            sewerConnection.AddStructureToBranch(this);
        }

        private void RemoveExistingBranchFeatures(ISewerConnection sewerConnection)
        {
            var branchFeature = sewerConnection.BranchFeatures[0];
            Log.Warn($"Overwriting branchfeature with name '{branchFeature.Name}' and type '{branchFeature.GetType()}' in sewer connection '{sewerConnection.Name}' with weir '{Name}'");
            sewerConnection.BranchFeatures.Clear();
        }
    }
}