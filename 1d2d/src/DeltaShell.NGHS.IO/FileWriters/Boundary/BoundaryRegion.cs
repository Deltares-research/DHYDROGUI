using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public static class BoundaryRegion
    {
        public static class FunctionStrings
        {
            public const string TimeSeries = "timeseries";
            public const string QhTable = "qhtable";
            public const string Constant = "constant";
        }

        public static class TimeInterpolationStrings
        {
            public const string LinearAndExtrapolate = "linear";
            public const string BlockFrom = "blockfrom";
            public const string BlockTo = "blockto"; // used when reading only?
        }

        public static class QuantityStrings
        {
            public const string QHDischargeWaterLevelDependency = "qhbnd";
            public const string QHWaterLevelDependencyKey = "waterlevel";
            public const string QHDischargeDependencyKey = "discharge";
            public const string WaterDischarge = "dischargebnd";
            public const string WaterLevel = "waterlevelbnd";
            public const string WaterLevelQuantityInRR = "water_level";
            public const string Time = "time";
            public const string WaterSalinity = "water_salinity";
            public const string WaterTemperature = "water_temperature";
            public const string WindSpeed = "wind_speed";
            public const string WindDirection = "wind_from_direction";
            public const string MeteoDataHumidity = "humidity";
            public const string MeteoDataAirTemperature = "air_temperature";
            public const string MeteoDataCloudiness = "cloudiness";
            public const string LateralDischarge = "lateral_discharge";
        }

        public static class UnitStrings
        {
            public const string WaterDischarge = "m³/s";
            public const string WaterLevel = "m";
            public const string TimeSeconds = "seconds since";
            public const string TimeMinutes = "minutes since";
            public const string TimeHours = "hours since";
            public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";
            public const string SaltPpt = "ppt";
            public const string SaltMass = "kg/s";
            public const string WaterTemperature = "degrees C";
            public const string WindSpeed = "m/s";
            public const string WindDirection = "degree";
            public const string MeteoDataHumidity = "percentage";
            public const string MeteoDataAirTemperature = "degrees C";
            public const string MeteoDataCloudiness = "percentage";
        }

        public const string BcBoundaryHeader = "Boundary";
        public const string BcForcingHeader = "forcing";
        public const string BcLateralHeader = "LateralDischarge";

        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Name of the boundary location (node id)");
        public static readonly ConfigurationSetting Function = new ConfigurationSetting(key: "function", description:
            "Possible values: " +
            "TimeSeries, " +
            "QHTable" );

        public static readonly ConfigurationSetting Interpolation = new ConfigurationSetting(key: "timeInterpolation", description:
            "Possible values: " +
            "linear, " +
            "block-from (value holds from specified time step), " +
            "block-to (value holds until next specified time step)" );

        public static readonly ConfigurationSetting Periodic = new ConfigurationSetting(key: "periodic", description:
            "Possible values:  true, false");

        public static readonly ConfigurationSetting Quantity = new ConfigurationSetting(key: "quantity", description:
            "Possible values (netcdf-CF standard): " +
            "time, " +
            "water_level, " +
            "water_discharge, " +
            "sea_water_salinity" );

        public static readonly ConfigurationSetting Unit = new ConfigurationSetting(key: "unit", description:
            "Possible values for 'time' column: " +
            "yyyy-MM-dd hh:mm:ss, " +
            "seconds since begintime format: yyyy-MM-dd hh:mm:ss +00:00 (+00:00: time zone), " +
            "minutes since begintime, " +
            "hours since begintime" );
    }
}
