using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Class that creates a <see cref="Lateral"/> from a <see cref="LateralDTO"/> object.
    /// </summary>
    public sealed class LateralFactory
    {
        /// <summary>
        /// Create a <see cref="Lateral"/> from a <see cref="LateralDTO"/> object
        /// </summary>
        /// <param name="lateralDTO"> The validated lateral DTO. </param>
        /// <param name="lateralTimeSeriesSetter">
        /// The lateral time series setter.
        /// </param>
        /// <returns>
        /// The created <see cref="Lateral"/> object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lateralDTO"/> or <paramref name="lateralTimeSeriesSetter"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="lateralDTO"/> has a forcing type other than <see cref=" LateralForcingType.Discharge"/>
        /// or <c>null</c> (defaults to <see cref=" LateralForcingType.Discharge"/>).
        /// </exception>
        /// <remarks>
        /// The <paramref name="lateralDTO"/> is assumed to be valid and up to the requirements.
        /// </remarks>
        public Lateral CreateLateral(LateralDTO lateralDTO, ILateralTimeSeriesSetter lateralTimeSeriesSetter)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));
            Ensure.NotNull(lateralTimeSeriesSetter, nameof(lateralTimeSeriesSetter));

            if (!HasDischargeForcingType(lateralDTO))
            {
                throw new InvalidOperationException("Lateral can only be parsed with discharge forcing type.");
            }

            var lateral = new Lateral
            {
                Name = lateralDTO.Id,
                Feature = GetFeature(lateralDTO)
            };

            SetDischargeData(lateralDTO, lateral.Data.Discharge, lateralTimeSeriesSetter);

            return lateral;
        }

        private static bool HasDischargeForcingType(LateralDTO lateralDTO) =>
            lateralDTO.Type == LateralForcingType.None || lateralDTO.Type == LateralForcingType.Discharge;

        private static Feature2D GetFeature(LateralDTO lateralDTO)
        {
            var feature = new Feature2D { Name = lateralDTO.Id };

            if (lateralDTO.NumCoordinates == 1)
            {
                feature.Geometry = GetPoint(lateralDTO);
            }
            else if (lateralDTO.NumCoordinates > 1)
            {
                feature.Geometry = GetPolygon(lateralDTO);
            }

            return feature;
        }

        private static Point GetPoint(LateralDTO lateralDTO)
        {
            double x = lateralDTO.XCoordinates.ElementAt(0);
            double y = lateralDTO.YCoordinates.ElementAt(0);
            return new Point(x, y);
        }

        private static Polygon GetPolygon(LateralDTO lateralDTO)
        {
            Coordinate[] coordinates = GetPolygonCoordinates(lateralDTO);
            var linearRing = new LinearRing(coordinates);
            return new Polygon(linearRing);
        }

        private static Coordinate[] GetPolygonCoordinates(LateralDTO lateralDTO)
        {
            Coordinate CreateCoordinate(double x, double y) => new Coordinate(x, y);
            List<Coordinate> coordinates = lateralDTO.XCoordinates.Zip(lateralDTO.YCoordinates, CreateCoordinate).ToList();

            Coordinate firstCoordinate = coordinates[0];
            Coordinate lastCoordinate = coordinates[coordinates.Count - 1];

            if (!firstCoordinate.Equals2D(lastCoordinate))
            {
                coordinates.Add(firstCoordinate);
            }

            return coordinates.ToArray();
        }

        private static void SetDischargeData(LateralDTO lateralDTO, LateralDischarge lateralDischarge, ILateralTimeSeriesSetter lateralTimeSeriesSetter)
        {
            switch (lateralDTO.Discharge.Mode)
            {
                case SteerableMode.ConstantValue:
                    lateralDischarge.Type = LateralDischargeType.Constant;
                    lateralDischarge.Constant = lateralDTO.Discharge.ConstantValue;
                    break;
                case SteerableMode.TimeSeries:
                    lateralDischarge.Type = LateralDischargeType.TimeSeries;
                    lateralTimeSeriesSetter.SetDischargeFunction(lateralDTO.Id, lateralDischarge.TimeSeries);
                    break;
                case SteerableMode.External:
                    lateralDischarge.Type = LateralDischargeType.RealTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lateralDTO), $@"Discharge mode of {nameof(lateralDTO)} is out of range.");
            }
        }
    }
}