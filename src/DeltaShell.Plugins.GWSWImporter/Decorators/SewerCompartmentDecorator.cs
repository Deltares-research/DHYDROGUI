using DelftTools.Hydro.SewerFeatures;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Base decorator for sewer compartments.
    /// </summary>
    public abstract class SewerCompartmentDecorator : ACompartment
    {
        private readonly ACompartment compartment;

        public override ILogHandler LogHandler => compartment?.LogHandler;

        /// <summary>
        /// Creates a new instance of <see cref="SewerCompartmentDecorator"/>.
        /// </summary>
        /// <param name="compartment">The compartment to decorate.</param>
        protected SewerCompartmentDecorator(ACompartment compartment)
        {
            this.compartment = compartment;
        }
        
        public override ACompartment ProcessInput(object gwswElement)
        {
            return compartment.ProcessInput(gwswElement);
        }
    }
}