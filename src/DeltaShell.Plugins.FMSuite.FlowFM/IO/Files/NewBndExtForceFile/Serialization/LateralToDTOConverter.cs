using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization
{
    /// <summary>
    /// Class for the conversion of a <see cref="Lateral"/> to a <see cref="LateralDTO"/>.
    /// </summary>
    public sealed class LateralToDTOConverter
    {
        /// <summary>
        /// Converts the provided lateral to a lateral DTO.
        /// </summary>
        /// <param name="lateral"> The lateral to convert. </param>
        /// <returns>
        /// A new <see cref="LateralDTO"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lateral"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the discharge type of the <paramref name="lateral"/> is not a defined <see cref="LateralDischargeType"/>.
        /// </exception>
        public LateralDTO Convert(Lateral lateral)
        {
            Ensure.NotNull(lateral, nameof(lateral));

            List<Coordinate> coordinates = GetCoordinates(lateral);
            int numCoordinates = coordinates.Count;
            IEnumerable<double> xCoordinates = coordinates.Select(c => c.X);
            IEnumerable<double> yCoordinates = coordinates.Select(c => c.Y);
            Steerable discharge = GetDischarge(lateral.Data.Discharge);

            return new LateralDTO(lateral.Name, lateral.Name, LateralForcingType.Discharge, LateralLocationType.TwoD,
                                  numCoordinates, xCoordinates, yCoordinates,
                                  discharge);
        }

        private static List<Coordinate> GetCoordinates(Lateral lateral)
        {
            List<Coordinate> coordinates = lateral.Feature.Geometry.Coordinates.ToList();

            // In the case of a polygon, the first and last coordinates are the same.
            // However, we have to write an "unclosed" polygon, therefore removing the last coordinate.
            if (coordinates.Count > 1)
            {
                coordinates.RemoveAt(coordinates.Count - 1);
            }

            return coordinates;
        }

        private static Steerable GetDischarge(LateralDischarge discharge)
        {
            switch (discharge.Type)
            {
                case LateralDischargeType.Constant:
                    return new Steerable
                    {
                        Mode = SteerableMode.ConstantValue,
                        ConstantValue = discharge.Constant
                    };
                case LateralDischargeType.TimeSeries:
                    return new Steerable
                    {
                        Mode = SteerableMode.TimeSeries,
                        TimeSeriesFilename = GetLateralDischargeTimeSeriesFilename()
                    };
                case LateralDischargeType.RealTime:
                    return new Steerable { Mode = SteerableMode.External };
                default:
                    throw new ArgumentOutOfRangeException(nameof(discharge), $@"Type of {nameof(discharge)} is out of range.");
            }
        }

        private static string GetLateralDischargeTimeSeriesFilename() =>
            $"{BcFileConstants.LateralDischargeQuantityName}.bc";
    }
}