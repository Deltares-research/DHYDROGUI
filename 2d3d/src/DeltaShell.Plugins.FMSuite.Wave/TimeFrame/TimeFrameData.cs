using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    /// <summary>
    /// <see cref="TimeFrameData"/> implements the <see cref="ITimeFrameData"/>
    /// interface as an <see cref="EntityAttribute"/>.
    /// </summary>
    /// <seealso cref="ITimeFrameData" />
    [Entity]
    public class TimeFrameData : ITimeFrameData
    {
        /// <summary>
        /// Creates a new <see cref="TimeFrameData"/>.
        /// </summary>
        public TimeFrameData()
        {
            // Note we need to initialize these values in the constructor
            // in order for PostSharp to do its magic and ensure the Entity
            // attribute is set up correctly, as newer C# syntax is not 
            // supported properly in the version of PostSharp we use.
            TimeVaryingData = CreateNewTimeVaryingDataFunction();
            HydrodynamicsConstantData = new HydrodynamicsConstantData();
            WindConstantData = new WindConstantData();
            WindFileData = new WaveMeteoData();
        }

        private static IFunction CreateNewTimeVaryingDataFunction()
        {
            var timeVaryingDataFunction = new Function("time frame data");
            timeVaryingDataFunction.Arguments.Add(new Variable<DateTime>("Time") { DefaultValue = DateTime.Today });
            timeVaryingDataFunction.Components.Add(new Variable<double>("Water Level", new Unit("meter", "m")));
            timeVaryingDataFunction.Components.Add(new Variable<double>("Velocity X", new Unit("meter per second", "m/s")));
            timeVaryingDataFunction.Components.Add(new Variable<double>("Velocity Y", new Unit("meter per second", "m/s")));
            timeVaryingDataFunction.Components.Add(new Variable<double>("Wind Speed", new Unit("meter per second", "m/s")));
            timeVaryingDataFunction.Components.Add(new Variable<double>("Wind Direction", new Unit("degrees", "deg")));

            return timeVaryingDataFunction;
        }

        public HydrodynamicsConstantData HydrodynamicsConstantData { get; private set; }
        public HydrodynamicsInputDataType HydrodynamicsInputDataType { get; set; } = HydrodynamicsInputDataType.Constant;
        public WindConstantData WindConstantData { get; private set; }
        public WaveMeteoData WindFileData { get; private set; }
        public WindInputDataType WindInputDataType { get; set; } = WindInputDataType.Constant;
        public IFunction TimeVaryingData { get; private set; }
        public IEnumerable<DateTime> TimePoints => TimeVaryingData.Arguments[0].Values.Cast<DateTime>();
    }
}