using System;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Water quality time settings
    /// </summary>
    public class WaterQualityTimeSettings
    {
        private readonly WaterQualityModelSettings waterQualityModelSettings;
        private Func<DateTime?, DateTime> startTimeFunc;
        private Func<DateTime?, DateTime> stopTimeFunc;
        private Func<TimeSpan?, TimeSpan> timeStepFunc;

        /// <summary>
        /// Creates water quality time settings with start time, stop time and time step according to
        /// <param name="waterQualityModelSettings"/>
        /// and
        /// <param name="waterQualityTimeSettingsType"/>
        /// </summary>
        /// <param name="waterQualityModelSettings"> The water quality model settings to create the time settings for </param>
        /// <param name="waterQualityTimeSettingsType"> The type of time settings that should be created </param>
        public WaterQualityTimeSettings(WaterQualityModelSettings waterQualityModelSettings,
                                        WaterQualityTimeSettingsType waterQualityTimeSettingsType)
        {
            this.waterQualityModelSettings = waterQualityModelSettings;
            Initialize(waterQualityTimeSettingsType);
        }

        /// <summary>
        /// The start time of the time settings
        /// </summary>
        public DateTime StartTime
        {
            get => startTimeFunc(null);
            set => startTimeFunc(value);
        }

        /// <summary>
        /// The stop time of the time settings
        /// </summary>
        public DateTime StopTime
        {
            get => stopTimeFunc(null);
            set => stopTimeFunc(value);
        }

        /// <summary>
        /// The time step of the time settings
        /// </summary>
        public TimeSpan TimeStep
        {
            get => timeStepFunc(null);
            set => timeStepFunc(value);
        }

        private void Initialize(WaterQualityTimeSettingsType waterQualityTimeSettingsType)
        {
            switch (waterQualityTimeSettingsType)
            {
                case WaterQualityTimeSettingsType.His:
                    CreateFunctions("HisStartTime", "HisStopTime", "HisTimeStep");
                    break;
                case WaterQualityTimeSettingsType.Map:
                    CreateFunctions("MapStartTime", "MapStopTime", "MapTimeStep");
                    break;
                case WaterQualityTimeSettingsType.Balance:
                    CreateFunctions("BalanceStartTime", "BalanceStopTime", "BalanceTimeStep");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(waterQualityTimeSettingsType));
            }
        }

        private void CreateFunctions(string startTime, string stopTime, string timeSpan)
        {
            startTimeFunc = CreateFunction<DateTime>(startTime);
            stopTimeFunc = CreateFunction<DateTime>(stopTime);
            timeStepFunc = CreateFunction<TimeSpan>(timeSpan);
        }

        private Func<T?, T> CreateFunction<T>(string propertyName) where T : struct
        {
            return CreateFunction(() => (T) TypeUtils.GetPropertyValue(waterQualityModelSettings, propertyName),
                                  t => TypeUtils.SetPropertyValue(waterQualityModelSettings, propertyName, t));
        }

        private static Func<T?, T> CreateFunction<T>(Func<T> getVariable, Action<T> setVariable)
            where T : struct
        {
            return t =>
            {
                if (t == null)
                {
                    return getVariable();
                }

                setVariable((T) t);

                return Activator.CreateInstance<T>();
            };
        }
    }
}