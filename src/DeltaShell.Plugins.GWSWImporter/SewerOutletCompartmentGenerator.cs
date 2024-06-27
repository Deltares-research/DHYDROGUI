using DelftTools.Hydro.SewerFeatures;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerOutletCompartmentGenerator : ASewerCompartmentGenerator
    {
        public SewerOutletCompartmentGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            bool validGwswStructure = gwswElement.IsValidGwswStructure(logHandler);

            if (validGwswStructure)
            {
                return CreateCompartment<GwswStructureOutletCompartment>(gwswElement);
            }

            return CreateCompartment<GwswPointOutletCompartment>(gwswElement);
        }

        protected override void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment(logHandler)) return;

            double auxDouble;
            var outletCompartment = compartment as OutletCompartment;
            if (outletCompartment != null)
            {
                var surfaceWaterLevelAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel, logHandler);
                if (surfaceWaterLevelAttribute.TryGetValueAsDouble(logHandler, out auxDouble))
                    outletCompartment.SurfaceWaterLevel = auxDouble;
            }
            else
            {
                logHandler?.ReportWarning($"Missing surface water level value for '{compartment.Name}', using default value: {compartment.SurfaceLevel}");
            }
            
            if (compartment is GwswStructureOutletCompartment)
            {
                compartment.ManholeLength = 0.8d;
                compartment.ManholeWidth = 0.8d;
                compartment.FloodableArea = 500;

                return;
            }

            SetBaseCompartmentProperties(compartment, gwswElement);
        }
    }
}