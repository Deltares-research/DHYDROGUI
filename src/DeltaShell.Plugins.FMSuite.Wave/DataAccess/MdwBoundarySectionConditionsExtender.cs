using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Static class containing static method to start extending the boundary
    /// section with condition properties. This class has a visitor as private
    /// nested class, since the visitor must only be used in this context.
    /// </summary>
    public static class MdwBoundarySectionConditionsExtender
    {
        /// <summary>
        /// Static method for retrieving boundary condition properties of each boundary
        /// and add them to the existing section.
        /// </summary>
        /// <param name="boundarySection"> The section that needs to be extended.</param>
        /// <param name="conditionDefinition"> The condition definition of the boundary.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundarySection"/>
        /// is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="conditionDefinition"/>
        /// is <c>null</c>.
        /// </exception>
        public static void AddNewProperties(IniSection boundarySection, IWaveBoundaryConditionDefinition conditionDefinition)
        {
            Ensure.NotNull(boundarySection, nameof(boundarySection));
            Ensure.NotNull(conditionDefinition, nameof(conditionDefinition));

            var visitor = new Visitor(boundarySection);
            conditionDefinition.AcceptVisitor(visitor);

            SetSpreadingTypeOfSpatiallyVaryingBoundaryWithoutActiveSupportPoints(boundarySection, conditionDefinition);
        }

        private static void SetSpreadingTypeOfSpatiallyVaryingBoundaryWithoutActiveSupportPoints(
            IniSection boundarySection, IWaveBoundaryConditionDefinition conditionDefinition)
        {
            if (boundarySection.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType) != string.Empty)
            {
                return;
            }

            switch (conditionDefinition.DataComponent)
            {
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                    boundarySection.AddOrUpdateProperty(KnownWaveProperties.DirectionalSpreadingType, KnownWaveBoundariesFileConstants.DegreesDefinedSpreading);
                    break;
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                    boundarySection.AddOrUpdateProperty(KnownWaveProperties.DirectionalSpreadingType, KnownWaveBoundariesFileConstants.PowerDefinedSpreading);
                    break;
                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }

        private class Visitor : IBoundaryConditionVisitor, IShapeVisitor, ISpatiallyDefinedDataComponentVisitor, IForcingTypeDefinedParametersVisitor, ISpreadingVisitor
        {
            private bool hasConstantValues;
            private bool isUniform;
            private int supportPointCounter;

            /// <summary>
            /// The constructor should set the section.
            /// </summary>
            /// <param name="boundarySection"> The boundary section that needs to be extended</param>
            public Visitor(IniSection boundarySection)
            {
                BoundarySection = boundarySection;
            }

            /// <summary>
            /// Visit method for adding place holders for shape type and directional spreading type. Also adds
            /// period type to the section. Calls next shape object to visit and data component.
            /// </summary>
            /// <param name="waveBoundaryConditionDefinition">The visited <see cref="IWaveBoundaryConditionDefinition"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="waveBoundaryConditionDefinition"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(IWaveBoundaryConditionDefinition waveBoundaryConditionDefinition)
            {
                Ensure.NotNull(waveBoundaryConditionDefinition, nameof(waveBoundaryConditionDefinition));

                //place holder
                BoundarySection.AddProperty(KnownWaveProperties.ShapeType, string.Empty);

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

                BoundarySection.AddProperty(KnownWaveProperties.PeriodType, periodTypePropertyValue);

                BoundarySection.AddProperty(KnownWaveProperties.DirectionalSpreadingType, string.Empty);

                waveBoundaryConditionDefinition.Shape.AcceptVisitor(this);

                waveBoundaryConditionDefinition.DataComponent.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for setting <see cref="hasConstantValues"/> and adding constant values
            /// to the section with/without distances. Calls the next AcceptVisitor method for the spreading.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IBoundaryConditionSpreading"/> object</typeparam>
            /// <param name="constantParameters"> The visited <see cref="ConstantParameters{TSpreading}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="constantParameters"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new()
            {
                Ensure.NotNull(constantParameters, nameof(constantParameters));
                hasConstantValues = true;
                if (!isUniform)
                {
                    BoundarySection.AddSpatialProperty(KnownWaveProperties.CondSpecAtDist,
                                                       SupportPoints[supportPointCounter].Distance);
                    supportPointCounter++;
                }

                BoundarySection.AddProperty(KnownWaveProperties.WaveHeight, constantParameters.Height);
                BoundarySection.AddProperty(KnownWaveProperties.Period, constantParameters.Period);
                BoundarySection.AddProperty(KnownWaveProperties.Direction, constantParameters.Direction);

                constantParameters.Spreading.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for setting <see cref="hasConstantValues"/> and writing distances if needed.
            /// Calls the next AcceptVisitor method for the spreading.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IBoundaryConditionSpreading"/> object</typeparam>
            /// <param name="timeDependentParameters"> The visited <see cref="TimeDependentParameters{TSpreading}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="timeDependentParameters"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
            {
                Ensure.NotNull(timeDependentParameters, nameof(timeDependentParameters));
                hasConstantValues = false;
                if (!isUniform)
                {
                    BoundarySection.AddSpatialProperty(KnownWaveProperties.CondSpecAtDist,
                                                       SupportPoints[supportPointCounter].Distance);
                    supportPointCounter++;
                }

                new T().AcceptVisitor(this);
            }

            /// <summary>
            /// This method throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="fileBasedParameters">This parameter is not used.</param>
            public void Visit(FileBasedParameters fileBasedParameters)
            {
                throw new NotSupportedException("File based boundaries are not supported here.");
            }

            /// <summary>
            /// Visit method for setting the shape type at the place holder and adding the Gaussian spreading
            /// to the section.
            /// </summary>
            /// <param name="gaussShape"> The visited <see cref="GaussShape"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="gaussShape"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(GaussShape gaussShape)
            {
                Ensure.NotNull(gaussShape, nameof(gaussShape));
                BoundarySection.AddOrUpdateProperty(KnownWaveProperties.ShapeType, KnownWaveBoundariesFileConstants.GaussShape);
                BoundarySection.AddProperty(KnownWaveProperties.GaussianSpreading, gaussShape.GaussianSpread);
            }

            /// <summary>
            /// Visit method for setting the shape type at the place holder and adding the Peak Enhancement factor
            /// to the section.
            /// </summary>
            /// <param name="jonswapShape">The visited <see cref="JonswapShape"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="jonswapShape"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(JonswapShape jonswapShape)
            {
                Ensure.NotNull(jonswapShape, nameof(jonswapShape));
                BoundarySection.AddOrUpdateProperty(KnownWaveProperties.ShapeType, KnownWaveBoundariesFileConstants.JonswapShape);
                BoundarySection.AddProperty(KnownWaveProperties.PeakEnhancementFactor, jonswapShape.PeakEnhancementFactor);
            }

            /// <summary>
            /// Visit method for setting the shape type at the place holder in the section.
            /// </summary>
            /// <param name="piersonMoskowitzShape"> The visited <see cref="PiersonMoskowitzShape"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="piersonMoskowitzShape"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(PiersonMoskowitzShape piersonMoskowitzShape)
            {
                Ensure.NotNull(piersonMoskowitzShape, nameof(piersonMoskowitzShape));
                BoundarySection.AddOrUpdateProperty(KnownWaveProperties.ShapeType, KnownWaveBoundariesFileConstants.PiersonMoskowitzShape);
            }

            /// <summary>
            /// Visit method for setting <see cref="isUniform"/> and calls the next AcceptVisitor method
            /// of the Data stored in the <see cref="UniformDataComponent{T}"/> object.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IForcingTypeDefinedParameters"/> object</typeparam>
            /// <param name="uniformDataComponent">The visited <see cref="UniformDataComponent{T}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="uniformDataComponent"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters
            {
                Ensure.NotNull(uniformDataComponent, nameof(uniformDataComponent));
                isUniform = true;
                uniformDataComponent.Data.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for setting <see cref="isUniform"/> and <see cref="SupportPoints"/>.
            /// Calls the next AcceptVisitors methods of the stored data for all support points in
            /// the <see cref="SpatiallyVaryingDataComponent{T}"/> object.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IForcingTypeDefinedParameters"/> object</typeparam>
            /// <param name="spatiallyVaryingDataComponent"> The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="spatiallyVaryingDataComponent"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IForcingTypeDefinedParameters
            {
                Ensure.NotNull(spatiallyVaryingDataComponent, nameof(spatiallyVaryingDataComponent));

                isUniform = false;
                SupportPoints = spatiallyVaryingDataComponent.Data.Keys.OrderBy(sp => sp.Distance).ToList();

                IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);

                foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
                {
                    supportPointKeyValuePair.Value.AcceptVisitor(this);
                }
            }

            /// <summary>
            /// Visit method for setting the directional spreading type and adding the directional
            /// spreading value to the section.
            /// </summary>
            /// <param name="degreesDefinedSpreading"> The visited <see cref="DegreesDefinedSpreading"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="degreesDefinedSpreading"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(DegreesDefinedSpreading degreesDefinedSpreading)
            {
                Ensure.NotNull(degreesDefinedSpreading, nameof(degreesDefinedSpreading));
                BoundarySection.AddOrUpdateProperty(KnownWaveProperties.DirectionalSpreadingType,
                                                    KnownWaveBoundariesFileConstants.DegreesDefinedSpreading);

                if (hasConstantValues)
                {
                    BoundarySection.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                                 degreesDefinedSpreading.DegreesSpreading);
                }
            }

            /// <summary>
            /// Visit method for setting the directional spreading type at the place holder and adding the
            /// directional spreading value to the section.
            /// </summary>
            /// <param name="powerDefinedSpreading"> The visited <see cref="PowerDefinedSpreading"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="powerDefinedSpreading"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(PowerDefinedSpreading powerDefinedSpreading)
            {
                Ensure.NotNull(powerDefinedSpreading, nameof(powerDefinedSpreading));
                BoundarySection.AddOrUpdateProperty(KnownWaveProperties.DirectionalSpreadingType,
                                                    KnownWaveBoundariesFileConstants.PowerDefinedSpreading);

                if (hasConstantValues)
                {
                    BoundarySection.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                                 powerDefinedSpreading.SpreadingPower);
                }
            }

            private IniSection BoundarySection { get; }

            private IList<SupportPoint> SupportPoints { get; set; }
        }
    }
}