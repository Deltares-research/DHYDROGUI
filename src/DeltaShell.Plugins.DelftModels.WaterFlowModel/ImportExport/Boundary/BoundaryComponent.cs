using DelftTools.Functions;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// Base class for the BoundaryCondition and LateralDischarge components.
    /// Provides the shared attributes to these classes.
    /// </summary>
    public abstract class BoundaryComponent
    {
        protected BoundaryComponent(InterpolationType interpolationType,
            bool isPeriodic,
            double constantBoundaryValue,
            IFunction timeDependentBoundaryValue)
        {
            this.InterpolationType = interpolationType;
            this.IsPeriodic = isPeriodic;
            this.ConstantBoundaryValue = constantBoundaryValue;
            this.TimeDependentBoundaryValue = timeDependentBoundaryValue;
        }

        /// <summary> The type of interpolation for the values of this BoundaryConditionComponent. </summary>
        public readonly InterpolationType InterpolationType;
        /// <summary> Whether this BoundaryConditionComponent values are repeating or not. </summary>
        public readonly bool IsPeriodic;
        /// <summary> The constant value of this BoundaryConditionComponent.  </summary>
        public readonly double ConstantBoundaryValue;
        /// <summary> The TimeDependent value of this BoundaryConditionComponent.  </summary>
        public readonly IFunction TimeDependentBoundaryValue;
    }
}
