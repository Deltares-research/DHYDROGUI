using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity(FireOnCollectionChange=false)]
    public class Weir : BranchStructure, IWeir
    {
        private double crestLevel;
        private double crestWidth;
        private bool canBeTimedependent;
        private bool useCrestLevelTimeSeries;
        
        public Weir() : this(false)
        {
        }

        public Weir(bool allowTimeVaryingData) :this("Weir", allowTimeVaryingData) { }

        public Weir(string name, bool allowTimeVaryingData = false)
        {
            // todo: move initialization to demo model only
            WeirFormula = new SimpleWeirFormula
                {
                    CorrectionCoefficient = 1.0
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
        [DisplayName("Crest width")]
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
            set
            {
                crestWidth = value;
                OnCrestWidthChanged();
            }
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
        [DisplayName("Crest level")]
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
                if (WeirFormula is FreeFormWeirFormula)
                {
                    (WeirFormula as FreeFormWeirFormula).CrestLevel = value;
                }
                else
                {
                    crestLevel = value;
                }
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

        [EditAction]
        private void OnCrestWidthChanged()
        {
            if (WeirFormula is GeneralStructureWeirFormula)
            {
                (WeirFormula as GeneralStructureWeirFormula).WidthStructureCentre = crestWidth;
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
            // first copy weir formula, because get/set crest width/level depend on the type of formula
            WeirFormula = (IWeirFormula)copyFrom.WeirFormula.Clone();
            CrestWidth = copyFrom.CrestWidth;
            CrestLevel = copyFrom.CrestLevel;
            CrestShape = copyFrom.CrestShape;
            FlowDirection = copyFrom.FlowDirection;
            Attributes = (IFeatureAttributeCollection)copyFrom.Attributes.Clone();
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

        public virtual bool SpecifyCrestLevelOnWeir
        {
            get { return true; }//used to be disabled for GeneralStructureWeirFormula
        }
        public virtual bool SpecifyCrestWidthOnWeir
        {
            get { return !(WeirFormula is FreeFormWeirFormula); }//used to be disabled for GeneralStructureWeirFormula
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
            ISewerConnection sewerConnection = null;
            if (helper == null)
            {
                //read from network.
                sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(sc =>
                    sc.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            }

            if (sewerConnection == null && (helper == null || !helper.SewerConnectionsByName.TryGetValue(Name, out sewerConnection)))
            {
                sewerConnection = GetNewSewerConnectionWithWeirToNetwork(hydroNetwork, helper);
                sewerConnection.AddToHydroNetwork(hydroNetwork, helper);
                return;
            }

            var weirs = sewerConnection?.BranchFeatures.OfType<IWeir>().Where(bf =>
                bf.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (weirs != null && weirs.Any())
            {
                SetSewerConnectionProperties(sewerConnection, hydroNetwork, helper);
                weirs.ForEach(CopyPropertyValuesToExistingWeir);
            }
            else
            {
                //remove old bf and add this one
                lock (hydroNetwork.BranchFeatures)
                {
                    sewerConnection?.BranchFeatures.RemoveAllWhere(bf =>
                        bf.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                        || bf is ICompositeBranchStructure compositeBranchStructure &&
                        compositeBranchStructure.Structures.Any(s =>
                            s.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)));

                    var composite = sewerConnection.AddStructureToBranch(this, false);
                    composite.Name = "CompositeBranchStructure_1D_";
                    helper?.CompositeBranchStructures?.Enqueue(composite);
                }
            }
        }

        protected virtual void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
        }

        private ISewerConnection GetNewSewerConnectionWithWeirToNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            CopyPropertyValuesToExistingWeir(this);

            if (helper == null || !helper.SewerConnectionsByName.TryGetValue(Name, out var sewerConnection) )
            {
                sewerConnection = new SewerConnection(Name);
                SetSewerConnectionProperties(sewerConnection, hydroNetwork, helper);
            }
            
            var composite = sewerConnection.AddStructureToBranch(this, false);
            composite.Name = "CompositeBranchStructure_1D_";
            helper?.CompositeBranchStructures?.Enqueue(composite);
            return sewerConnection;
        }

        protected virtual void SetSewerConnectionProperties(ISewerConnection sewerConnection, IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
        }
    }
}