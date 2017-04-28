using System.Collections.Generic;
using System.Collections.ObjectModel;
using log4net.Appender;
using log4net.Core;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class Iterative1D2DCouplerAppender : AppenderSkeleton
    {
        private readonly ICollection<string> messages = new Collection<string>();

        public Iterative1D2DCouplerAppender()
        {
            Threshold = Level.Debug;
        }

        public bool Enabled { get; set; }

        public ICollection<string> Messages { get { return messages; } }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (!Enabled || loggingEvent.LoggerName != typeof(Iterative1D2DCoupler).FullName) return;

            messages.Add(string.Format("{0} - ({1}) - {2}", loggingEvent.TimeStamp.ToShortTimeString(), loggingEvent.Level, loggingEvent.RenderedMessage));
        }
    }
}