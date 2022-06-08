using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// <see cref="Weir"/> defines the weir structure which can be placed on branches.
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class Weir : BranchStructure, IWeir, IHasSteerableProperties
    {
        private double crestWidth;
        
        /// <summary>
        /// Creates a new <see cref="Weir"/> without time-varying data and a default name.
        /// </summary>
        public Weir() : this(false) { }

        /// <summary>
        /// Creates a new <see cref="Weir"/> with a default name.
        /// </summary>
        /// <param name="allowTimeVaryingData">Whether to allow time-varying data.</param>
        public Weir(bool allowTimeVaryingData) :this("Weir", allowTimeVaryingData) { }

        /// <summary>
        /// Creates a new <see cref="Weir"/>.
        /// </summary>
        /// <param name="name">The name of the weir.</param>
        /// <param name="allowTimeVaryingData">Whether to allow time-varying data</param>
        public Weir(string name, bool allowTimeVaryingData = false)
        {
            // todo: move initialization to demo model only
            WeirFormula = new SimpleWeirFormula
            {
                CorrectionCoefficient = 1.0
            };

            CanBeTimedependent = allowTimeVaryingData;
            crestLevel = ConstructCrestLevelSteerableProperty();

            Name = name;
            CrestWidth = 5;
            OffsetY = 0;
            FlowDirection = FlowDirection.Both;
            CrestShape = CrestShape.Sharp;
            UseVelocityHeight = true;
        }

        private SteerableProperty ConstructCrestLevelSteerableProperty()
        {
            const double defaultValue = 1.0;
            return CanBeTimedependent
                       ? new SteerableProperty(defaultValue,
                                               "Crest level",
                                               "Crest level",
                                               "m AD") 
                       : new SteerableProperty(defaultValue);
        }

        [FeatureAttribute(Order = 6)]
        [DisplayName("Crest width")]
        public virtual double CrestWidth
        {
            get
            {
                switch (WeirFormula)
                {
                    case GeneralStructureWeirFormula generalStructureWeirFormula:
                        return generalStructureWeirFormula.WidthStructureCentre;
                    case FreeFormWeirFormula freeFormWeirFormula:
                        return freeFormWeirFormula.CrestWidth;
                    default:
                        return crestWidth;
                }
            }
            set
            {
                crestWidth = value;
                OnCrestWidthChanged();
            }
        }

        public virtual bool CanBeTimedependent { get; protected set; }

        public virtual bool UseCrestLevelTimeSeries
        {
            get => crestLevel.CurrentDriver == SteerablePropertyDriver.TimeSeries;
            set => crestLevel.CurrentDriver = value ? SteerablePropertyDriver.TimeSeries : SteerablePropertyDriver.Constant;
        }

        // Should be immutable, however due to the CopyFrom behaviour it cannot be.
        private SteerableProperty crestLevel;

        [FeatureAttribute(Order = 7)]
        [DisplayName("Crest level")]
        public virtual double CrestLevel
        {
            get
            {
                switch (WeirFormula)
                {
                    case GeneralStructureWeirFormula generalStructureWeirFormula:
                        return generalStructureWeirFormula.BedLevelStructureCentre;
                    case FreeFormWeirFormula freeFormWeirFormula:
                        return freeFormWeirFormula.CrestLevel;
                    default:
                        return crestLevel.Constant;
                }
            }
            set
            {
                if (WeirFormula is FreeFormWeirFormula freeFormWeirFormula)
                {
                    freeFormWeirFormula.CrestLevel = value;
                }
                else
                {
                    crestLevel.Constant = value;
                }
                OnCrestLevelChanged();
            }
        }

        public virtual TimeSeries CrestLevelTimeSeries
        {
            get => crestLevel.TimeSeries;
            protected set => crestLevel.TimeSeries = value;
        }

        [EditAction]
        private void OnCrestLevelChanged()
        {
            if (WeirFormula is GeneralStructureWeirFormula generalStructureWeirFormula)
            {
                generalStructureWeirFormula.BedLevelStructureCentre = crestLevel.Constant;
            }
        }

        [EditAction]
        private void OnCrestWidthChanged()
        {
            if (WeirFormula is GeneralStructureWeirFormula generalStructureWeirFormula)
            {
                generalStructureWeirFormula.WidthStructureCentre = crestWidth;
            }
        }

        public virtual IWeirFormula WeirFormula { get; set; }
        
        [ReadOnly(true)]
        [DisplayName("Formula")]
        [FeatureAttribute(Order = 5)]
        public virtual string FormulaName => WeirFormula.Name;

        public virtual bool IsGated => WeirFormula is IGatedWeirFormula;

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

        [EditAction]
        private void UpdateFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            if (WeirFormula.HasFlowDirection)
            {
                FlowDirection = GetPossibleFlowDirection(allowPositiveFlow, allowNegativeFlow);
            }
        }

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
            crestLevel = new SteerableProperty(copyFrom.crestLevel);
            CrestShape = copyFrom.CrestShape;
            FlowDirection = copyFrom.FlowDirection;
            Attributes = (IFeatureAttributeCollection)copyFrom.Attributes.Clone();
            CanBeTimedependent = copyFrom.CanBeTimedependent;
        }

        private static FlowDirection GetPossibleFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            return allowPositiveFlow
                       ? (allowNegativeFlow ? FlowDirection.Both : FlowDirection.Positive)
                       : (allowNegativeFlow ? FlowDirection.Negative : FlowDirection.None);
        }

        public virtual bool SpecifyCrestLevelOnWeir => 
            true; //used to be disabled for GeneralStructureWeirFormula

        public virtual bool SpecifyCrestWidthOnWeir => 
            !(WeirFormula is FreeFormWeirFormula); //used to be disabled for GeneralStructureWeirFormula

        public virtual bool UseVelocityHeight { get; set; }

        public override StructureType GetStructureType()
        {
            switch (WeirFormula)
            {
                case FreeFormWeirFormula _:
                    return StructureType.UniversalWeir;
                case GatedWeirFormula _:
                    return StructureType.Orifice;
                case PierWeirFormula _:
                    return StructureType.AdvancedWeir;
                case RiverWeirFormula _:
                    return StructureType.RiverWeir;
                case SimpleWeirFormula _:
                    return StructureType.Weir;
                case GeneralStructureWeirFormula _:
                    return StructureType.GeneralStructure;
                default:
                    return StructureType.Unknown;
            }
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
            // No-op
        }

        private ISewerConnection GetNewSewerConnectionWithWeirToNetwork(IHydroNetwork hydroNetwork, 
                                                                        SewerImporterHelper helper)
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

        protected virtual void SetSewerConnectionProperties(ISewerConnection sewerConnection, 
                                                            IHydroNetwork hydroNetwork, 
                                                            SewerImporterHelper helper)
        {
            // No-op
        }

        public virtual IEnumerable<SteerableProperty> RetrieveSteerableProperties()
        {
            yield return crestLevel;
        }
    }
}