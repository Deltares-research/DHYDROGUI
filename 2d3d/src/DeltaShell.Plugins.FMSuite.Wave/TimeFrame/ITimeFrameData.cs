namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    using System;
    using System.Collections.Generic;
    using DelftTools.Functions;

    namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
    {
        /// <summary>
        /// <see cref="ITimeFrameData"/> defines the time frame data.
        /// </summary>
        public interface ITimeFrameData
        {
            /// <summary>
            /// Gets the constant hydrodynamics data.
            /// </summary>
            HydrodynamicsConstantData HydrodynamicsConstantData { get; }

            /// <summary>
            /// Gets or sets the type of the hydrodynamics input data.
            /// </summary>
            HydrodynamicsInputDataType HydrodynamicsInputDataType { get; set; }

            /// <summary>
            /// Gets the constant wind data.
            /// </summary>
            WindConstantData WindConstantData { get; }

            /// <summary>
            /// Gets the wind file data.
            /// </summary>
            WaveMeteoData WindFileData { get; }

            /// <summary>
            /// Gets or sets the type of the wind input data.
            /// </summary>
            WindInputDataType WindInputDataType { get; set; }

            /// <summary>
            /// Gets the time varying data stored in a single function.
            /// </summary>
            /// <remarks>
            /// Note this function contains all the different components,
            /// however these should only be used if specified by the
            /// <see cref="HydrodynamicsInputDataType"/> and
            /// <see cref="WindInputDataType"/>.
            /// </remarks>
            IFunction TimeVaryingData { get; }

            /// <summary>
            /// Gets the time points of the <see cref="TimeVaryingData"/>.
            /// </summary>
            IEnumerable<DateTime> TimePoints { get; }
        }
    }
}