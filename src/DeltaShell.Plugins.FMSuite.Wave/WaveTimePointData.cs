using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Entity]
    public class WaveInputFieldData
    {
        public InputFieldDataType HydroDataType { get; set; }
        public InputFieldDataType WindDataType { get; set; }

        // constant hydro
        public double WaterLevelConstant { get; set; }
        public double VelocityXConstant { get; set; }
        public double VelocityYConstant { get; set; }

        // constant wind
        public double WindSpeedConstant { get; set; }
        public double WindDirectionConstant { get; set; }

        // meteo files
        public WaveMeteoData MeteoData { get; set; }

        /// <summary>
        /// For now, water level, velocity, and wind in a single function
        /// </summary>
        public IFunction InputFields { get; set; }

        public IList<DateTime> TimePoints => InputFields.Arguments[0].Values.OfType<DateTime>().ToList();

        public WaveInputFieldData()
        {
            HydroDataType = InputFieldDataType.Constant;
            WindDataType = InputFieldDataType.Constant;
            WaterLevelConstant = 0.0;
            VelocityXConstant = 0.0;
            VelocityYConstant = 0.0;
            WindSpeedConstant = 0.0;
            WindDirectionConstant = 0.0;
            MeteoData = new WaveMeteoData();

            InputFields = new Function("wave_input_fields");
            InputFields.Arguments.Add(new Variable<DateTime>("Time"));
            InputFields.Components.Add(new Variable<double>("Water Level", new Unit("meter", "m")));
            InputFields.Components.Add(new Variable<double>("Velocity X", new Unit("meter per second", "m/s")));
            InputFields.Components.Add(new Variable<double>("Velocity Y", new Unit("meter per second", "m/s")));
            InputFields.Components.Add(new Variable<double>("Wind Speed", new Unit("meter per second", "m/s")));
            InputFields.Components.Add(new Variable<double>("Wind Direction", new Unit("-", "-")));
        }
    }

    public enum InputFieldDataType
    {
        [Description("Constant")]
        Constant,

        [Description("Per Timepoint")]
        TimeVarying,

        [Description("From File")]
        FromInputFiles
    }
}