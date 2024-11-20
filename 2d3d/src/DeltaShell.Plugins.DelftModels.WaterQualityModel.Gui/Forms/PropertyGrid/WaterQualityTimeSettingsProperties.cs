using System;
using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Output timers")]
    public class WaterQualityTimeSettingsProperties
    {
        private readonly WaterQualityTimeSettings waterQualityTimeSettings;

        public WaterQualityTimeSettingsProperties(WaterQualityTimeSettings waterQualityTimeSettings)
        {
            this.waterQualityTimeSettings = waterQualityTimeSettings;
        }

        [PropertyOrder(1)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DisplayName("Start time")]
        public DateTime OutputStartTime
        {
            get => waterQualityTimeSettings.StartTime;
            set => waterQualityTimeSettings.StartTime = value;
        }

        [PropertyOrder(2)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DisplayName("Stop time")]
        public DateTime OutputStopTime
        {
            get => waterQualityTimeSettings.StopTime;
            set => waterQualityTimeSettings.StopTime = value;
        }

        [PropertyOrder(3)]
        [DisplayName("Time step")]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        public TimeSpan OutputTimeStep
        {
            get => waterQualityTimeSettings.TimeStep;
            set => waterQualityTimeSettings.TimeStep = value;
        }

        public override string ToString()
        {
            return "";
        }
    }
}