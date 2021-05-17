using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class ExtForceQuantNames
    {
        // quantity names for filetype=9 method=2,3
        public const string WaterLevelAtBound = "waterlevelbnd";
        public const string DischargeAtBound = "dischargebnd";
        public const string QhAtBound = "qhbnd";
        public const string VelocityAtBound = "velocitybnd";
        public const string NormalVelocityAtBound = "normalvelocitybnd";
        public const string TangentialVelocityAtBound = "tangentialvelocitybnd";
        public const string NeumannConditionAtBound = "neumannbnd";
        public const string RiemannConditionAtBound = "riemannbnd";
        public const string RiemannVelocityAtBound = "riemann_velocitybnd";
        public const string OpenFlowConditionAtBound = "outflowbnd";
        public const string SalinityAtBound = "salinitybnd";
        public const string TemperatureAtBound = "temperaturebnd";
        public const string TracerAtBound = "tracerbnd";
        public const string LowerGateLevel = "lowergatelevel";
        public const string DamLevel = "damlevel";
        public const string SourceAndSink = "discharge_salinity_temperature_sorsin";

        // quantity names for filetype=4,10 method=4
        public const string FrictCoef = "frictioncoefficient";
        public const string HorEddyViscCoef = "horizontaleddyviscositycoefficient";
        public const string HorEddyDiffCoef = "horizontaleddydiffusivitycoefficient";
        public const string AdvectionType = "advectiontype";
        public const string InitialWaterLevel = "initialwaterlevel";
        public const string InitialWaterDepth = "initialwaterdepth";
        public const string WaterLevel = "waterlevel";
        public const string WaterDepth = "waterdepth";
        public const string InitialSalinity = "initialsalinity";
        public const string InitialSalinityTop = "initialsalinitytop";
        public const string InitialTemperature = "initialtemperature";

        // quantity names for filetype=1,2,4,7,8 method=1,2,3
        public const string WindX = "windx";
        public const string WindY = "windy";
        public const string WindXY = "windxy";
        public const string Rain = "rain";
        public const string AtmosphericPressure = "atmosphericpressure";
        public const string PressureWindXWindY = "airpressure_windx_windy";
        public const string SpiderWeb = "spiderweb";
        public const string MeteoData = "humidity_airtemperature_cloudiness";
        public const string MeteoDataWithRadiation = "humidity_airtemperature_cloudiness_solarradiation";

        
        public const string Precipitation = "rainfall_rate";

        // trying to complicate things a bit further :-(, for flooding:
        public const string EmbankmentBnd = "1d2dbnd";
        public const string EmbankmentForcingFile = "REALTIME";


        public const string PliFileExtension = "pli";
        public const string PolFileExtension = "pol";
        public const string TimFileExtension = "tim";
        public const string CmpFileExtension = "cmp";
        public const string QhFileExtension = "qh";
        public const string T3DFileExtension = "t3d";
        public const string XyzFileExtension = "xyz";

        public static readonly IDictionary<BoundaryConditionDataType, string> ForcingToFileExtensionMapping =
            new Dictionary<BoundaryConditionDataType, string>
            {
                {BoundaryConditionDataType.TimeSeries, TimFileExtension},
                {BoundaryConditionDataType.AstroComponents, CmpFileExtension},
                {BoundaryConditionDataType.AstroCorrection, CmpFileExtension},
                {BoundaryConditionDataType.Harmonics, CmpFileExtension},
                {BoundaryConditionDataType.HarmonicCorrection, CmpFileExtension},
                {BoundaryConditionDataType.Qh, QhFileExtension}
            };

        public static class FileTypes
        {
            public const int Uniform = 1;
            public const int UniMagDir = 2;
            public const int SVWP = 3;
            public const int ArcInfo = 4;
            public const int SpiderWeb = 5;
            public const int Curvi = 6;
            public const int Triangulation = 7;
            public const int TriangulationMagDir = 8;
            public const int PolyTim = 9;
            public const int InsidePolygon = 10;
            public const int NCgrid = 11;
            public const int GeoTiff = 12;
        }

        // spatial operation operator mappings
        public static readonly IDictionary<PointwiseOperationType, Operator> OperatorMapping =
            new Dictionary<PointwiseOperationType, Operator>
            {
                {PointwiseOperationType.Overwrite, Operator.Overwrite},
                {PointwiseOperationType.OverwriteWhereMissing, Operator.ApplyOnly},
                {PointwiseOperationType.Add, Operator.Add},
                {PointwiseOperationType.Multiply, Operator.Multiply},
                {PointwiseOperationType.Maximum, Operator.Maximum},
                {PointwiseOperationType.Minimum, Operator.Minimum}
            };

        // operator strings
        public static readonly IDictionary<Operator, string> OperatorToStringMapping =
            new Dictionary<Operator, string>
            {
                {Operator.Overwrite, "O"},
                {Operator.ApplyOnly, "A"},
                {Operator.Add, "+"},
                {Operator.Multiply, "*"},
                {Operator.Maximum, "X"},
                {Operator.Minimum, "N"}
            };

        public static PointwiseOperationType ParseOperationType(string operationString)
        {
            if (!OperatorToStringMapping.Values.Contains(operationString))
            {
                throw new ArgumentException("Cannot parse " + operationString + " into valid pointwise operator");
            }
            var spatialOperator = OperatorToStringMapping.First(kvp => kvp.Value == operationString).Key;
            if (!OperatorMapping.Values.Contains(spatialOperator))
            {
                throw new ArgumentException("Cannot parse " + operationString + " into valid pointwise operator");
            }
            return OperatorMapping.First(kvp => kvp.Value == spatialOperator).Key;
        }

        // wind quantities
        public static readonly IDictionary<WindQuantity, string> WindQuantityNames = new Dictionary
            <WindQuantity, string>
        {
            {WindQuantity.VelocityX, WindX},
            {WindQuantity.VelocityY, WindY},
            {WindQuantity.VelocityVector, WindXY},
            {WindQuantity.AirPressure, AtmosphericPressure},
            {WindQuantity.VelocityVectorAirPressure, PressureWindXWindY},
        };

        // meteo quantities
        public static readonly IDictionary<FmMeteoQuantity, string> MeteoQuantityNames = new Dictionary
            <FmMeteoQuantity, string>
        {
            {FmMeteoQuantity.Precipitation, Precipitation},
        };

        // Boundary condition quantities
        private static readonly IDictionary<string, FlowBoundaryQuantityType> BoundaryToQuantityMapping =
            new Dictionary<string, FlowBoundaryQuantityType>
            {
                {WaterLevelAtBound, FlowBoundaryQuantityType.WaterLevel},
                {VelocityAtBound, FlowBoundaryQuantityType.Velocity},
                {DischargeAtBound, FlowBoundaryQuantityType.Discharge},
                {NormalVelocityAtBound, FlowBoundaryQuantityType.NormalVelocity},
                {TangentialVelocityAtBound, FlowBoundaryQuantityType.TangentVelocity},
                {NeumannConditionAtBound, FlowBoundaryQuantityType.Neumann},
                {RiemannConditionAtBound, FlowBoundaryQuantityType.Riemann},
                {RiemannVelocityAtBound, FlowBoundaryQuantityType.RiemannVelocity},
                {OpenFlowConditionAtBound, FlowBoundaryQuantityType.Outflow},
                {SalinityAtBound, FlowBoundaryQuantityType.Salinity},
                {TemperatureAtBound,FlowBoundaryQuantityType.Temperature},
                {BcFileFlowBoundaryDataBuilder.ConcentrationAtBound,FlowBoundaryQuantityType.SedimentConcentration},
                {BcmFileFlowBoundaryDataBuilder.BedLevelAtBound, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
                {BcmFileFlowBoundaryDataBuilder.BedLevelChangeAtBound, FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed},
                {BcmFileFlowBoundaryDataBuilder.BedLoadAtBound, FlowBoundaryQuantityType.MorphologyBedLoadTransport},
                {QhAtBound, FlowBoundaryQuantityType.WaterLevel}
            };



        /// <summary>
        /// Try to parse the quantity name and see if it is a boundary.
        /// </summary>
        public static bool TryParseBoundaryQuantityType(string standardName, out FlowBoundaryQuantityType quantityType)
        {
            if (standardName.StartsWith(TracerAtBound))
            {
                quantityType = FlowBoundaryQuantityType.Tracer;
                return true;
            }

            var mapping = BoundaryToQuantityMapping.FirstOrDefault(m => standardName.StartsWith(m.Key));
            quantityType = mapping.Value;

            return (mapping.Key != null);
        }


        /// <summary>
        /// Returns the quantity string for a given flow boundary condition
        /// </summary>
        public static string GetQuantityString(FlowBoundaryCondition boundaryCondition)
        {
            var quantity = boundaryCondition.FlowQuantity;
            if (quantity == FlowBoundaryQuantityType.Tracer)
            {
                return TracerAtBound + boundaryCondition.TracerName;
            }
            if (boundaryCondition.DataType == BoundaryConditionDataType.Qh)
            {
                return QhAtBound;
            }
            if (quantity == FlowBoundaryQuantityType.SedimentConcentration)
            {
                return BcmFileFlowBoundaryDataBuilder.ConcentrationAtBound + boundaryCondition.SedimentFractionName;
            }
            if (BoundaryToQuantityMapping.Values.Contains(quantity))
            {
                return BoundaryToQuantityMapping.First(kvp => kvp.Value.Equals(quantity)).Key;
            }
            throw new ArgumentException(String.Format("Quantity {0} cannot be mapped to standard name.", quantity));
        }

        /// <summary>
        /// Utility suffixes for multiple pli-files
        /// </summary>
        public static string GetPliQuantitySuffix(IFeatureData featureData)
        {
            var boundaryCondition = featureData as IBoundaryCondition;
            if (boundaryCondition != null)
            {
                var quantity = boundaryCondition.VariableName;

                if (quantity.Equals(FlowBoundaryQuantityType.WaterLevel.ToString()))
                {
                    return "_h";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Velocity.ToString()))
                {
                    return "_v";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Discharge.ToString()))
                {
                    return "_q";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.NormalVelocity.ToString()))
                {
                    return "_un";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.TangentVelocity.ToString()))
                {
                    return "_ut";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Neumann.ToString()))
                {
                    return "_nm";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Riemann.ToString()))
                {
                    return "_rm";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.RiemannVelocity.ToString()))
                {
                    return "_rv";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Outflow.ToString()))
                {
                    return "_op";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Salinity.ToString()))
                {
                    return "_sal";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.Temperature.ToString()))
                {
                    return "_tmp";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.SedimentConcentration.ToString()))
                {
                    return "_con";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed.ToString()))
                {
                    return "_blp";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed.ToString()))
                {
                    return "_blcp";
                }
                if (quantity.Equals(FlowBoundaryQuantityType.MorphologyBedLoadTransport.ToString()))
                {
                    return "_blt";
                }
                return "_" + boundaryCondition.VariableName.ToLower().Substring(0, 3);
            }
            return String.Empty;
        }
    }
}