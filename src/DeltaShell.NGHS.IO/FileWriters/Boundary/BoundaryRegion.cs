using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public static class BoundaryRegion
    {
        public static class QuantityStrings
        {
            public const string WaterDischarge = "water_discharge";
            public const string WaterLevel = "water_level";
            public const string Time = "time";
            public const string WaterSalinity = "water_salinity";
            public const string WaterTemperature = "water_temperature";
            public const string WindSpeed = "wind_speed";
            public const string WindDirection = "wind_from_direction";
            public const string MeteoDataHumidity = "humidity";
            public const string MeteoDataAirTemperature = "air_temperature";
            public const string MeteoDataCloudiness = "cloudiness";
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
        public const string BcLateralHeader = "LateralDischarge";

        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Name of the boundary location (node id)");
        public static readonly ConfigurationSetting Function = new ConfigurationSetting(key: "function", description:
            "Possible values: " +
            "TimeSeries, " +
            "QHTable" );

        public static readonly ConfigurationSetting Interpolation = new ConfigurationSetting(key: "time-interpolation", description:
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
