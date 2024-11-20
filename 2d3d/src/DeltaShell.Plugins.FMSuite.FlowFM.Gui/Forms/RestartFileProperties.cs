using System;
using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Grid properties for <see cref="RestartFile"/>.
    /// </summary>
    public sealed class RestartFileProperties : ObjectProperties<WaterFlowFMRestartFile>
    {
        /// <summary>
        /// Gets the name of the <see cref="RestartFile"/>.
        /// </summary>
        [Category("General")]
        [PropertyOrder(0)]
        [Description("Name of the restart file")]
        [DynamicVisible]
        public string Name
        {
            get => data.Name;
        }

        /// <summary>
        /// Gets or sets the restart start time.
        /// </summary>
        [TypeConverter(typeof (DeltaShellDateTimeConverter))]
        [Category("General")]
        [PropertyOrder(1)]
        [DisplayName("Restart start time")]
        [Description("Restart date and time when restarting from *_map.nc.")]
        [DynamicVisible]
        public DateTime RestartDateTime
        {
            get => data.StartTime;
            set => data.StartTime = value;
        }

        [DynamicVisibleValidationMethod]
        public bool IsPropertyVisible(string propertyName)
        {
            return data.IsMapFile;
        }
    }
}