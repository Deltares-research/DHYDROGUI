using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Creator for creating <see cref="DelftIniCategory"/> for wave purposes.
    /// </summary>
    public static class WaveDelftIniCategoryCreator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveDelftIniCategoryCreator));

        /// <summary>
        /// Creates and returns a <see cref="DelftIniCategory"/> from the data of a wave spatiallyVaryingDataComponent condition.
        /// </summary>
        /// <param name="boundaryContainer"> </param>
        /// <returns>The requested <see cref="DelftIniCategory"/>.</returns>
        public static IEnumerable<DelftIniCategory> CreateBoundaryConditionCategories(
            IBoundaryContainer boundaryContainer)
        {
            foreach (IWaveBoundary boundary in boundaryContainer.Boundaries)
            {
                var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
                boundaryCategory.AddProperty(KnownWaveProperties.Name, boundary.Name);
                boundaryCategory.AddProperty(KnownWaveProperties.Definition, "xy-coordinates");

                SupportPoint[] sortedSupportPoints =
                    boundary.GeometricDefinition.SupportPoints.OrderBy(sp => sp.Distance).ToArray();

                boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateX,
                                             boundaryContainer
                                                 .GetBoundarySnappingCalculator()
                                                 .CalculateCoordinateFromSupportPoint(sortedSupportPoints.First()).X);
                boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateX,
                                             boundaryContainer
                                                 .GetBoundarySnappingCalculator()
                                                 .CalculateCoordinateFromSupportPoint(sortedSupportPoints.Last()).X);
                boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateY,
                                             boundaryContainer
                                                 .GetBoundarySnappingCalculator()
                                                 .CalculateCoordinateFromSupportPoint(sortedSupportPoints.First()).Y);
                boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateY,
                                             boundaryContainer
                                                 .GetBoundarySnappingCalculator()
                                                 .CalculateCoordinateFromSupportPoint(sortedSupportPoints.Last()).Y);
                boundaryCategory.AddProperty(KnownWaveProperties.SpectrumSpec, "parametric");
                
                var visitor = new ExtendBoundaryCategoriesOfMdwDataComponentVisitor(boundaryCategory);
                boundary.ConditionDefinition.AcceptVisitor(visitor);
               
                yield return boundaryCategory;
            }
        }
    }
}