using System;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Validators
{
    /// <summary>
    /// A class containing a collection of <see cref="IFeature"/> that have been validated.
    /// </summary>
    public class ValidatedFeatures
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedFeatures"/> class.
        /// </summary>
        /// <param name="region"> The region containing the features. </param>
        /// <param name="features"> The features that are validated. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="features"/> or <paramref name="region"/> is <c>null</c>.
        /// </exception>
        public ValidatedFeatures(IComplexFeature region, params IFeature[] features)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(features, nameof(features));

            FeatureRegion = region;
            Features = features;
        }

        /// <summary>
        /// The region containing the features.
        /// </summary>
        public IComplexFeature FeatureRegion { get; }

        /// <summary>
        /// The validated features.
        /// </summary>
        public IFeature[] Features { get; }

        /// <summary>
        /// Gets the envelope containing the geometries of all the <see cref="Features"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="Envelope"/> containing the geometries of all the <see cref="Features"/>.
        /// </returns>
        public Envelope GetEnvelope()
        {
            var envelope = new Envelope();

            foreach (IFeature feature in Features)
            {
                envelope.ExpandToInclude(feature.Geometry.EnvelopeInternal);
            }

            const int pointExpansion = 10;
            if (envelope.Area == 0)
            {
                envelope.ExpandBy(pointExpansion);
            }

            return envelope;
        }
    }
}