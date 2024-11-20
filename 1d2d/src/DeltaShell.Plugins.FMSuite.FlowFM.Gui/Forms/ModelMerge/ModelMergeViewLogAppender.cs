using System;
using log4net.Appender;
using log4net.Core;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.ModelMerge
{
    /// <summary>
    /// Appender specifically for the ModelMergeView
    /// </summary>
    public sealed class ModelMergeViewLogAppender : AppenderSkeleton
    {
        /// <summary>
        /// Action to specify what should happen with the generated <see cref="LoggingEvent"/>.
        /// </summary>
        public Action<LoggingEvent> AddLogAction { get; set; } 
        
        /// <inheritdoc />
        /// Calls the registered <see cref="AddLogAction"/> to handle the logging event.
        protected override void Append(LoggingEvent loggingEvent)
        {
            AddLogAction?.Invoke(loggingEvent);
        }
    }
}