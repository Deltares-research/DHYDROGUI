using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visitor used for retrieving the boundary condition properties needed for writing the Mdw file.
    /// </summary>
    public class MdwBoundaryConditionPropertiesCreator : BaseDataComponentVisitor, IBoundaryConditionVisitor
    {
        private bool hasConstantValues;
        private bool isUniform;
        private int supportPointCounter;

        public MdwBoundaryConditionPropertiesCreator(DelftIniCategory boundaryCategory)
        {
            BoundaryCategory = boundaryCategory;
        }

        private DelftIniCategory BoundaryCategory { get; }

        private IList<SupportPoint> SupportPoints { get; set; }
        
        public override void Visit<T>(UniformDataComponent<T> uniformDataComponent) 
        {
            isUniform = true;
        }

        public override void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) 
        {
            isUniform = false;
            SupportPoints = spatiallyVaryingDataComponent.Data.Keys.OrderBy(sp => sp.Distance).ToList();
        }

        public override void Visit<T>(ConstantParameters<T> constantParameters)
        { 
            hasConstantValues = true;
            if (isUniform)
            {
                BoundaryCategory.AddProperty(KnownWaveProperties.WaveHeight, constantParameters.Height);
                BoundaryCategory.AddProperty(KnownWaveProperties.Period, constantParameters.Period);
                BoundaryCategory.AddProperty(KnownWaveProperties.Direction, constantParameters.Direction);
            }
            else
            {
                BoundaryCategory.AddProperty(KnownWaveProperties.CondSpecAtDist,
                                             SupportPoints[supportPointCounter].Distance);
                BoundaryCategory.AddProperty(KnownWaveProperties.WaveHeight, constantParameters.Height);
                BoundaryCategory.AddProperty(KnownWaveProperties.Period, constantParameters.Period);
                BoundaryCategory.AddProperty(KnownWaveProperties.Direction, constantParameters.Direction);
                
                supportPointCounter++;
            }
        }

        public override void Visit<T>(TimeDependentParameters<T> timeDependentParameters) 
        {
            hasConstantValues = false;
            if (!isUniform)
            {
                BoundaryCategory.AddProperty(KnownWaveProperties.CondSpecAtDist,
                                             SupportPoints[supportPointCounter].Distance);
                supportPointCounter++;
            }
        }
        
        public override void Visit(DegreesDefinedSpreading degreesDefinedSpreading)
        {
            BoundaryCategory.SetProperty(KnownWaveProperties.DirectionalSpreadingType,
                                         KnownWaveBoundariesFileConstants.DegreesDefinedSpreading);

            if (hasConstantValues)
            {
                BoundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                             degreesDefinedSpreading.DegreesSpreading);
            }
        }

        public override void Visit(PowerDefinedSpreading powerDefinedSpreading)
        {
            BoundaryCategory.SetProperty(KnownWaveProperties.DirectionalSpreadingType,
                                         KnownWaveBoundariesFileConstants.PowerDefinedSpreading);

            if (hasConstantValues)
            {
                BoundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                             powerDefinedSpreading.SpreadingPower);
            }
        }

        public void Visit(GaussShape gaussShape)
        {
            BoundaryCategory.SetProperty(KnownWaveProperties.ShapeType, KnownWaveBoundariesFileConstants.GaussShape);
            BoundaryCategory.AddProperty(KnownWaveProperties.GaussianSpreading, gaussShape.GaussianSpread);
        }

        public void Visit(JonswapShape jonswapShape)
        {
            BoundaryCategory.SetProperty(KnownWaveProperties.ShapeType, KnownWaveBoundariesFileConstants.JonswapShape);
            BoundaryCategory.AddProperty(KnownWaveProperties.PeakEnhancementFactor, jonswapShape.PeakEnhancementFactor);
        }

        public void Visit(PiersonMoskowitzShape piersonMoskowitzShape)
        {
            BoundaryCategory.SetProperty(KnownWaveProperties.ShapeType, KnownWaveBoundariesFileConstants.PiersonMoskowitzShape);
        }

        public void Visit(IWaveBoundaryConditionDefinition waveBoundaryConditionDefinition)
        {
            //place holder
            BoundaryCategory.AddProperty(KnownWaveProperties.ShapeType, string.Empty);
            
            BoundaryCategory.AddProperty(KnownWaveProperties.PeriodType, waveBoundaryConditionDefinition.PeriodType.GetDescription());
            
            //place holder
            BoundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingType, string.Empty);
        }

        /// <summary>
        /// Static method for retrieving boundary condition properties of each boundary.
        /// </summary>
        /// <param name="boundaryCategory"></param>
        /// <param name="conditionDefinition"> </param>
        public static void AddNewProperties(DelftIniCategory boundaryCategory,
                                            IWaveBoundaryConditionDefinition conditionDefinition)
        {
            var visitor = new MdwBoundaryConditionPropertiesCreator(boundaryCategory);
            conditionDefinition.AcceptVisitor(visitor);

            SetSpreadingTypeOfSpatiallyVaryingBoundaryWithoutActiveSupportPoints(boundaryCategory, conditionDefinition);
        }

        private static void SetSpreadingTypeOfSpatiallyVaryingBoundaryWithoutActiveSupportPoints(
            DelftIniCategory boundaryCategory, IWaveBoundaryConditionDefinition conditionDefinition)
        {
            if (boundaryCategory.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType) == string.Empty)
            {
                string retrievedValue;
                switch (conditionDefinition.DataComponent)
                {
                    case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                    case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                    case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                    case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                        retrievedValue = "Power";
                        break;
                    case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                    case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                    case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                    case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                        retrievedValue = "Degrees";
                        break;
                    default:
                        throw new NotSupportedException(
                            "The type of the specified dataComponent does not correspond with a supported type");
                }

                boundaryCategory.SetProperty(KnownWaveProperties.DirectionalSpreadingType, retrievedValue);
            }
        }
    }
}