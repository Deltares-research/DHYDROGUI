using DelftTools.Units;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// A class that represents boundary conditions that are used in wave models.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.Common.FeatureData.BoundaryCondition"/>
    /// <seealso cref="GeoAPI.Extensions.Feature.IFeature"/>
    [Entity]
    public class WaveBoundaryCondition : BoundaryCondition, IFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveBoundaryCondition"/> class.
        /// </summary>
        /// <param name="bcDataType"> The data type of the wave boundary condition. </param>
        public WaveBoundaryCondition(BoundaryConditionDataType bcDataType) : base(bcDataType) {}

        public override string ProcessName { get; }

        public override string VariableName { get; }

        public override string VariableDescription { get; }

        public override IUnit VariableUnit { get; }

        public override bool IsHorizontallyUniform { get; }
        public override bool IsVerticallyUniform { get; }

        public override int VariableDimension => 1;

        public IGeometry Geometry { get; set; }

        public IFeatureAttributeCollection Attributes { get; set; }
    }
}