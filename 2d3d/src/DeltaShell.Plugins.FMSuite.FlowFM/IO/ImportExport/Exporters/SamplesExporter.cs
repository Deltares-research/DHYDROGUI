using System;
using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    /// <summary>
    /// Exporter for the point values of a <see cref="Samples"/> object.
    /// </summary>
    public sealed class SamplesExporter : XyzFileExporter
    {
        /// <inheritdoc/>
        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof(Samples);
        }

        /// <summary>
        /// Gets the point values from the samples item.
        /// </summary>
        /// <param name="item"> The samples item. </param>
        /// <returns> An <see cref="IEnumerable{IPointValue}"/> containing the sample point values. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="item"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="item"/> is not a <see cref="Samples"/> instance.
        /// </exception>
        protected override IEnumerable<IPointValue> GetPointValues(object item)
        {
            Ensure.NotNull(item, nameof(item));

            if (!(item is Samples samples))
            {
                throw new ArgumentException(nameof(item), $"{nameof(item)} is not a {typeof(Samples)}.");
            }

            return samples.PointValues;
        }

        /// <summary>
        /// Checks whether or not the samples item can be exported.
        /// </summary>
        /// <param name="item"> The samples item. </param>
        /// <returns> A boolean value indicating whether the samples can be exported. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="item"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="item"/> is not a <see cref="Samples"/> instance.
        /// </exception>
        protected override bool CheckObject(object item)
        {
            Ensure.NotNull(item, nameof(item));

            if (!(item is Samples))
            {
                throw new ArgumentException(nameof(item), $"{nameof(item)} is not a {typeof(Samples)}.");
            }

            return true;
        }
    }
}