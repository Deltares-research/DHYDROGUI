using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Area.Objects.StructureObjects
{
    /// <summary>
    /// <see cref="Structure"/> defines a single structure.
    /// </summary>
    /// <seealso cref="Unique{T}" />
    /// <seealso cref="IStructure" />
    [Entity(FireOnCollectionChange = false)]
    public sealed class Structure : Unique<long>, IStructure
    {
        private string groupName;
        private double crestWidth;
        private double crestLevel;

        /// <summary>
        /// Creates a new <see cref="Structure"/> with default values.
        /// </summary>
        public Structure() : this(HydroTimeSeriesFactory.CreateTimeSeries(GuiParameterNames.CrestLevel,
                                                                          GuiParameterNames.CrestLevel,
                                                                          "m AD"))
        {
            Formula = new SimpleWeirFormula()
            {
                DischargeCoefficient = 1.0,
                LateralContraction = 1.0,
            };

            CrestWidth = double.NaN;
        }

        private Structure(TimeSeries crestLevelTimeSeries)
        {
            Ensure.NotNull(crestLevelTimeSeries, nameof(crestLevelTimeSeries));
            CrestLevelTimeSeries = crestLevelTimeSeries;
        }

        public IGeometry Geometry { get; set; }

        public IFeatureAttributeCollection Attributes { get; set; }

        [FeatureAttribute]
        public string GroupName
        {
            get => groupName;
            set => groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value);
        }

        public bool IsDefaultGroup { get; set; } = false;

        [FeatureAttribute]
        public string Name { get; set; } = "Structure";

        /// <summary>
        /// Gets the name of the formula.
        /// </summary>
        [FeatureAttribute]
        public string FormulaName => Formula?.Name;

        public IStructureFormula Formula { get; set; }
        
        [FeatureAttribute]
        public double CrestWidth
        {
            get => Formula is GeneralStructureFormula formula 
                       ? formula.CrestWidth
                       : crestWidth;
            set
            {
                crestWidth = value;

                if (Formula is GeneralStructureFormula formula)
                {
                    formula.CrestWidth = value;
                }
            }
        }

        public bool UseCrestLevelTimeSeries { get; set; }

        [FeatureAttribute]
        public double CrestLevel
        {
            get => Formula is GeneralStructureFormula formula
                       ? formula.CrestLevel
                       : crestLevel;
            set
            {
                crestLevel = value;

                if (Formula is GeneralStructureFormula formula)
                {
                    formula.CrestLevel = value;
                }
            }
        }
        
        public TimeSeries CrestLevelTimeSeries { get; }

        public object Clone()
        {
            return new Structure((TimeSeries) CrestLevelTimeSeries.Clone())
            {
                GroupName = GroupName,
                Geometry = (IGeometry) Geometry?.Clone(),
                Attributes = (IFeatureAttributeCollection) Attributes?.Clone(),
                Name = Name,
                IsDefaultGroup = IsDefaultGroup,
                Formula = (IStructureFormula) Formula?.Clone(),
                CrestWidth = CrestWidth,
                CrestLevel = CrestLevel,
                UseCrestLevelTimeSeries = UseCrestLevelTimeSeries,
            };
        }

        // As part of WaterFlowFMModel.Eventing.GetDataItemListForFeature
        // uses the `feature.ToString()` method to generate a name. In order
        // to ensure the name does not get overwritten, we need to overwrite
        // the `ToString` method to return the Name. This is legacy behaviour 
        // from the previous Weir implementation unfortunately.
        public override string ToString() => 
            !string.IsNullOrEmpty(Name) ? Name : "Unnamed Structure";
    }
}