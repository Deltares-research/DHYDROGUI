using DHYDRO.Common.Logging;

namespace DelftTools.Hydro.SewerFeatures
{
    /// <summary>
    /// Base compartment class.
    /// </summary>
    public abstract class ACompartment
    {
        public virtual ILogHandler LogHandler { get; }
        
        /// <summary>
        /// Processes a compartment and then returns the processed compartment.
        /// </summary>
        /// <param name="gwswElement">A gwsw element.</param>
        /// <returns>The processed compartment.</returns>
        public abstract ACompartment ProcessInput(object gwswElement);
    }
}