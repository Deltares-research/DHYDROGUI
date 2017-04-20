using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.ModelExchange.Queries
{
    public class AggregationResult
    {
        private const string CsvDelimeter = ",";

        public AggregationResult()
        {
            AggregationType = FunctionAttributes.AggregationTypes.None;
            LocationType = "Undefined";
        }

        /// <summary>
        /// Gets or sets the type: ie. input or output
        /// </summary>
        public ExchangeType ExchangeType { get; set; }

        public string LocationId { get; set; }

        public string ParameterId { get; set; }

        public IGeometry Geometry
        {
            get { return Feature == null ? null : Feature.Geometry; }
        }

        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the object
        /// </summary>
        public string FeatureOwnerName { get; set; }

        /// <summary>
        /// Gets the x coordinate of the geometry
        /// </summary>
        public double X
        {
            get { return Geometry != null ? Geometry.Coordinate.X : double.NaN; }
        }

        /// <summary>
        /// Gets the y coordinate of the geometry
        /// </summary>
        public double Y
        {
            get { return Geometry != null ? Geometry.Coordinate.Y : double.NaN; }
        }

        /// <summary>
        /// Gets the x coordinate of the geometry
        /// </summary>
        public double Z
        {
            get { return Geometry != null ? Geometry.Coordinate.Z : double.NaN; }
        }

        /// <summary>
        /// Gets or sets a specific time series iterator
        /// </summary>
        public Func<IEnumerable<Utils.Tuple<DateTime, double>>> TimeSeriesIterator { get; protected internal set; }

        /// <summary>
        /// Gets or sets the time dependent time series found while collecting data from the model
        /// </summary>
        public IFunction TimeSeries { get; set; }

        /// <summary>
        /// Gets or sets the feature (argument of the time series like ObservationPoint or other)
        /// </summary>
        public IFeature Feature { get; set; }

        public string LocationType { get; set; }

        /// <summary>
        /// Gets or sets the owner of the feature
        /// </summary>
        public object FeatureOwner { get; set; }

        /// <summary>
        /// Gets or the type name of the feature owner
        /// </summary>
        public string FeatureOwnerTypeName
        {
            get { return FeatureOwner != null ? FeatureOwner.GetType().Name : "object"; }
        }

        public string AggregationType { get; set; }

        public override string ToString()
        {
            return String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", CsvDelimeter, ExchangeType, Name,
                FeatureOwnerName, ParameterId, LocationId, LocationType, X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture));
        }

        public static string[] ToSeperatedValues(IEnumerable<AggregationResult> results)
        {
            var header = string.Format("ParameterType{0}LocationName{0}UserInterfaceName{0}ParameterId{0}LocationId{0}LocationType{0}X{0}Y", CsvDelimeter);
            var values = results.Select(r => r.ToString());

            return new[] { header }.Concat(values).ToArray();
        }
    }
}