using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using DHYDRO.Common.Logging;
using System;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public abstract class ASewerCompartmentGenerator : ASewerGenerator, IGwswFeatureGenerator<ISewerFeature>
    {
        protected ASewerCompartmentGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public abstract ISewerFeature Generate(GwswElement gwswElement);
        
        protected ISewerFeature CreateCompartment<T>(GwswElement gwswElement) 
            where T : Compartment
        {
            if (gwswElement == null) return null;

            var compartmentToBeAdded = CreateNewCompartment<T>(gwswElement);
            SetCompartmentProperties(compartmentToBeAdded, gwswElement);

            return compartmentToBeAdded;
        }

        private T CreateNewCompartment<T>(GwswElement gwswElement) where T : Compartment
        {
            string logMessage = string.Format(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_will_be_added_to_the_network_with_a_unique_name, "Compartment", gwswElement.GetElementLine());
            string compartmentName = gwswElement.GetAttributeValueFromList<string>(ManholeMapping.PropertyKeys.UniqueId, logHandler, null, logMessage);
            
            return (T)Activator.CreateInstance(typeof(T), logHandler, compartmentName);
        }

        protected abstract void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement);

        protected void SetBaseCompartmentProperties(ACompartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment(logHandler)) return;

            var sewerCompartmentDecoratorFactory = new SewerCompartmentDecoratorFactory();
            compartment = sewerCompartmentDecoratorFactory.SetDecorators(compartment);
            compartment.ProcessInput(gwswElement);
        }
    }
}