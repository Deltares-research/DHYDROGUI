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
    /// Static class containing static method to start extending the boundary
    /// category with condition properties. This class has a visitor as private
    /// nested class, since the visitor must only be used in this context.
    /// </summary>
    public static class MdwBoundaryCategoryConditionsExtender 
    {
        /// <summary>
        /// Static method for retrieving boundary condition properties of each boundary
        /// and add them to the existing category.
        /// </summary>
        /// <param name="boundaryCategory"> The category that needs to be extended.</param>
        /// <param name="conditionDefinition"> The condition definition of the boundary.</param>
        public static void AddNewProperties(DelftIniCategory boundaryCategory,
                                            IWaveBoundaryConditionDefinition conditionDefinition)
        {
            var visitor = new Visitor(boundaryCategory);
            conditionDefinition.AcceptVisitor(visitor);

            SetSpreadingTypeOfSpatiallyVaryingBoundaryWithoutActiveSupportPoints(boundaryCategory, conditionDefinition);
        }

        private static void SetSpreadingTypeOfSpatiallyVaryingBoundaryWithoutActiveSupportPoints(
            DelftIniCategory boundaryCategory, IWaveBoundaryConditionDefinition conditionDefinition)
        {
            if (boundaryCategory.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType) == string.Empty)
            {
                Type spreadingType = conditionDefinition.DataComponent.GetType().GenericTypeArguments[0].GenericTypeArguments[0];
                if (spreadingType == typeof(PowerDefinedSpreading))
                {
                    boundaryCategory.SetProperty(KnownWaveProperties.DirectionalSpreadingType, KnownWaveBoundariesFileConstants.PowerDefinedSpreading);
                }
                else if (spreadingType == typeof(DegreesDefinedSpreading))
                {
                    boundaryCategory.SetProperty(KnownWaveProperties.DirectionalSpreadingType, KnownWaveBoundariesFileConstants.DegreesDefinedSpreading);
                }
                else
                {
                    throw new NotSupportedException(
                        "The type of the specified dataComponent does not correspond with a supported type");
                }
            }
        }

        private class Visitor : IBoundaryConditionVisitor, IShapeVisitor, IDataComponentVisitor, IParametersVisitor, ISpreadingVisitor
        {
            private bool hasConstantValues;
            private bool isUniform;
            private int supportPointCounter;

            /// <summary>
            /// The constructor should set the category. 
            /// </summary>
            /// <param name="boundaryCategory"> The boundary category that needs to be extended</param>
            public Visitor(DelftIniCategory boundaryCategory)
            {
                BoundaryCategory = boundaryCategory;
            }

            private DelftIniCategory BoundaryCategory { get; }

            private IList<SupportPoint> SupportPoints { get; set; }

            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IBoundaryConditionParameters
            {
                isUniform = true;
                uniformDataComponent.Data.AcceptVisitor(this);
            }

            public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IBoundaryConditionParameters
            {
                isUniform = false;
                SupportPoints = spatiallyVaryingDataComponent.Data.Keys.OrderBy(sp => sp.Distance).ToList();

                IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);

                foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
                {
                    supportPointKeyValuePair.Value.AcceptVisitor(this);
                }
            }

            public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new()
            {
                hasConstantValues = true;
                if (!isUniform)
                {
                    BoundaryCategory.AddProperty(KnownWaveProperties.CondSpecAtDist,
                                                 SupportPoints[supportPointCounter].Distance);
                    supportPointCounter++;
                }

                BoundaryCategory.AddProperty(KnownWaveProperties.WaveHeight, constantParameters.Height);
                BoundaryCategory.AddProperty(KnownWaveProperties.Period, constantParameters.Period);
                BoundaryCategory.AddProperty(KnownWaveProperties.Direction, constantParameters.Direction);

                constantParameters.Spreading.AcceptVisitor(this);
            }

            public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
            {
                hasConstantValues = false;
                if (!isUniform)
                {
                    BoundaryCategory.AddProperty(KnownWaveProperties.CondSpecAtDist,
                                                 SupportPoints[supportPointCounter].Distance);
                    supportPointCounter++;
                }

                new T().AcceptVisitor(this);
            }

            public void Visit(DegreesDefinedSpreading degreesDefinedSpreading)
            {
                BoundaryCategory.SetProperty(KnownWaveProperties.DirectionalSpreadingType,
                                             KnownWaveBoundariesFileConstants.DegreesDefinedSpreading);

                if (hasConstantValues)
                {
                    BoundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                                 degreesDefinedSpreading.DegreesSpreading);
                }
            }

            public void Visit(PowerDefinedSpreading powerDefinedSpreading)
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
                
                string periodTypePropertyValue;

                switch (waveBoundaryConditionDefinition.PeriodType)
                {
                    case BoundaryConditionPeriodType.Mean:
                        periodTypePropertyValue = PeriodImportExportType.Mean.GetDescription();
                        break;
                    case BoundaryConditionPeriodType.Peak:
                        periodTypePropertyValue = PeriodImportExportType.Peak.GetDescription();
                        break;
                    default:
                        throw new NotSupportedException($"Value '{waveBoundaryConditionDefinition.PeriodType}' is not a valid period type for the exporter.");
                }

                BoundaryCategory.AddProperty(KnownWaveProperties.PeriodType, periodTypePropertyValue);
                
                BoundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingType, string.Empty);
                
                waveBoundaryConditionDefinition.Shape.AcceptVisitor(this);

                waveBoundaryConditionDefinition.DataComponent.AcceptVisitor(this);
            }
        }
    }
}