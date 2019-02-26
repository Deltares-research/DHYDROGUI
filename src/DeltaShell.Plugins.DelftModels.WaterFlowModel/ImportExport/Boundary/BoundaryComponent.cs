using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// Base class for the BoundaryCondition and LateralDischarge components.
    /// Provides the shared attributes to these classes.
    /// </summary>
    public abstract class BoundaryComponent
    {
        protected BoundaryComponent(Flow1DInterpolationType? interpolationType,
                                    Flow1DExtrapolationType? extrapolationType,
                                    bool isPeriodic,
                                    double constantBoundaryValue,
                                    IFunction timeDependentBoundaryValue)
        {
            InterpolationType = interpolationType;
            ExtrapolationType = extrapolationType;
            IsPeriodic = isPeriodic;
            ConstantBoundaryValue = constantBoundaryValue;
            TimeDependentBoundaryValue = timeDependentBoundaryValue;
        }

        /// <summary>The type of interpolation for the values of this BoundaryComponent.</summary>
        public readonly Flow1DInterpolationType? InterpolationType;
        /// <summary>The type of extrapolation for the values of this BoundaryComponent.</summary>
        public readonly Flow1DExtrapolationType? ExtrapolationType;

        /// <summary>Whether this BoundaryComponent values are repeating or not.</summary>
        public readonly bool IsPeriodic;
        /// <summary>The constant value of this BoundaryComponent.</summary>
        public readonly double ConstantBoundaryValue;
        /// <summary>The TimeDependent value of this BoundaryComponent.</summary>
        public readonly IFunction TimeDependentBoundaryValue;
    }
}
