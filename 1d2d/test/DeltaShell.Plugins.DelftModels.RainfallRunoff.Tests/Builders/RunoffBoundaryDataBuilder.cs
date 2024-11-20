using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Builders
{
    /// <summary>
    /// Builder class for <see cref="RunoffBoundaryDataBuilder"/>.
    /// </summary>
    internal class RunoffBoundaryDataBuilder
    {
        private readonly RunoffBoundaryData runoffBoundaryData;

        private RunoffBoundaryDataBuilder()
        {
            var runoffBoundary = new RunoffBoundary();
            var rainfallRunoffBoundaryData = new RainfallRunoffBoundaryData();
            runoffBoundaryData = new RunoffBoundaryData(runoffBoundary) { Series = rainfallRunoffBoundaryData };
        }

        /// <summary>
        /// Initialize a new builder.
        /// </summary>
        /// <returns> The created builder. </returns>
        public static RunoffBoundaryDataBuilder Start() => new RunoffBoundaryDataBuilder();

        /// <summary>
        /// Sets the name of the runoff boundary to the specified name.
        /// </summary>
        /// <param name="name"> The runoff boundary name. </param>
        /// <returns> The current builder. </returns>
        public RunoffBoundaryDataBuilder WithName(string name)
        {
            runoffBoundaryData.Boundary.Name = name;
            return this;
        }

        /// <summary>
        /// Adds a time series to the boundary data of the runoff boundary with the provided values starting on the start date.
        /// Values are one day apart.
        /// </summary>
        /// <returns> The current builder. </returns>
        public RunoffBoundaryDataBuilder WithTimeSeries(DateTime startDate, params double[] values)
        {
            runoffBoundaryData.Series.IsConstant = false;

            for (var i = 0; i < values.Length; i++)
            {
                runoffBoundaryData.Series.Data[startDate.AddDays(i)] = values[i];
            }

            return this;
        }

        /// <summary>
        /// Adds a constant value to the boundary data of the runoff boundary.
        /// </summary>
        /// <returns> The current builder. </returns>
        public RunoffBoundaryDataBuilder WithConstantValue(double value)
        {
            runoffBoundaryData.Series.IsConstant = true;
            runoffBoundaryData.Series.Value = value;
            return this;
        }

        /// <summary>
        /// Builds the final runoff boundary data.
        /// </summary>
        /// <returns>
        /// The built unpaved data.
        /// </returns>
        public RunoffBoundaryData Build() => runoffBoundaryData;
    }
}