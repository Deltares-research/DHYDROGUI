using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Area.Objects
{
    /// <summary>
    /// <see cref="Structure"/> defines a single structure.
    /// </summary>
    /// <seealso cref="Utils.Data.Unique{long}" />
    /// <seealso cref="IStructure" />
    [Entity]
    public sealed class Structure : Unique<long>, IStructure
    {
        private string groupName;
        private double crestWidth;
        private double crestLevel;

        /// <summary>
        /// Creates a new <see cref="Structure"/>.
        /// </summary>
        public Structure() : this(HydroTimeSeriesFactory.CreateTimeSeries(GuiParameterNames.CrestLevel,
                                                                          GuiParameterNames.CrestLevel, 
                                                                          "m AD")) { }

        private Structure(TimeSeries crestLevelTimeSeries)
        {
            Ensure.NotNull(crestLevelTimeSeries, nameof(crestLevelTimeSeries));
            CrestLevelTimeSeries = crestLevelTimeSeries;
        }

        public IGeometry Geometry { get; set; }

        public IFeatureAttributeCollection Attributes { get; set; }

        [DisplayName("Group name")]
        [FeatureAttribute(Order = 1)]
        public string GroupName
        {
            get => groupName;
            set => groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value);
        }

        public bool IsDefaultGroup { get; set; } = false;

        [DisplayName("Name")]
        [FeatureAttribute(Order = 2)]
        public string Name { get; set; } = "Structure";

        /// <summary>
        /// Gets the name of the formula.
        /// </summary>
        [ReadOnly(true)]
        [DisplayName("Formula")]
        [FeatureAttribute(Order = 3)]
        public string FormulaName => Formula?.Name;

        public IWeirFormula Formula { get; set; }

        [DisplayName("Crest Width")]
        [FeatureAttribute(Order = 4)]
        public double CrestWidth
        {
            get => Formula is GeneralStructureWeirFormula formula 
                       ? formula.WidthStructureCentre
                       : crestWidth;
            set
            {
                crestWidth = value;

                if (Formula is GeneralStructureWeirFormula formula)
                {
                    formula.WidthStructureCentre = value;
                }
            }
        }

        public bool UseCrestLevelTimeSeries { get; set; }

        [DisplayName("Crest Level")]
        [FeatureAttribute(Order = 4)]
        public double CrestLevel
        {
            get => Formula is GeneralStructureWeirFormula formula
                       ? formula.BedLevelStructureCentre
                       : crestLevel;
            set
            {
                crestLevel = value;

                if (Formula is GeneralStructureWeirFormula formula)
                {
                    formula.WidthStructureCentre = value;
                }
            }
        }

        [DisplayName("Use Crest Level Time Series")]
        [FeatureAttribute(Order = 5)]
        public TimeSeries CrestLevelTimeSeries { get; }

        public object Clone()
        {
            return new Structure((TimeSeries) CrestLevelTimeSeries.Clone())
            {
                GroupName = GroupName,
                Geometry = (IGeometry) Geometry.Clone(),
                Attributes = (IFeatureAttributeCollection) Attributes.Clone(),
                Name = Name,
                IsDefaultGroup = IsDefaultGroup,
                Formula = (IWeirFormula) Formula.Clone(),
                CrestWidth = CrestWidth,
                CrestLevel = CrestLevel,
                UseCrestLevelTimeSeries = UseCrestLevelTimeSeries,
            };
        }
    }
}