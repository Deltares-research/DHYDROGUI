using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    /// <summary>
    /// <see cref="TimeFrameDataExtensions"/> provides data transfer methods for the <see cref="ITimeFrameData"/>
    /// interface.
    /// </summary>
    public static class TimeFrameDataExtensions
    {
        /// <summary>
        /// Synchronizes the data of <paramref name="goal"/> with <paramref name="source"/> such
        /// that goal will contain the same values as source afterwards.
        /// </summary>
        /// <param name="goal">The goal.</param>
        /// <param name="source">The source.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public static void SynchronizeDataWith(this ITimeFrameData goal, ITimeFrameData source)
        {
            Ensure.NotNull(goal, nameof(goal));
            Ensure.NotNull(source, nameof(source));

            goal.HydrodynamicsInputDataType = source.HydrodynamicsInputDataType;
            goal.HydrodynamicsConstantData.SynchronizeDataWith(source.HydrodynamicsConstantData);

            goal.WindInputDataType = source.WindInputDataType;
            goal.WindConstantData.SynchronizeDataWith(source.WindConstantData);
            goal.WindFileData.SynchronizeDataWith(source.WindFileData);

            goal.TimeVaryingData.SynchronizeDataWith(source.TimeVaryingData);
        }

        private static void SynchronizeDataWith(this HydrodynamicsConstantData goal, HydrodynamicsConstantData source)
        {
            goal.VelocityX = source.VelocityX;
            goal.VelocityY = source.VelocityY;
            goal.WaterLevel = source.WaterLevel;
        }

        private static void SynchronizeDataWith(this WindConstantData goal, WindConstantData source)
        {
            goal.Speed = source.Speed;
            goal.Direction = source.Direction;
        }

        private static void SynchronizeDataWith(this WaveMeteoData goal, WaveMeteoData source)
        {
            goal.FileType = source.FileType;

            goal.XYVectorFilePath = source.XYVectorFilePath;

            goal.XComponentFilePath = source.XComponentFilePath;
            goal.YComponentFilePath = source.YComponentFilePath;

            goal.HasSpiderWeb = source.HasSpiderWeb;
            goal.SpiderWebFilePath = source.SpiderWebFilePath;
        }

        private static void SynchronizeDataWith(this IFunction goal, IFunction source)
        {
            // We know how the TimeVaryingData is structured, and thus copy the
            // data of the columns directly.
            goal.Arguments[0].SetValues(source.Arguments[0].Values);   // Time
            goal.Components[0].SetValues(source.Components[0].Values); // Water level
            goal.Components[1].SetValues(source.Components[1].Values); // Velocity X
            goal.Components[2].SetValues(source.Components[2].Values); // Velocity Y
            goal.Components[3].SetValues(source.Components[3].Values); // Wind Speed
            goal.Components[4].SetValues(source.Components[4].Values); // Wind Direction
        }
    }
}