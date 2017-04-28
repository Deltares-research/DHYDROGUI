using System;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekWaqTimer
    {
        /// <summary>
        /// Start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Stop time
        /// </summary>
        public DateTime StopTime { get; set; }

        /// <summary>
        /// Time step
        /// </summary>
        public TimeSpan TimeStep { get; set; }
    }
}