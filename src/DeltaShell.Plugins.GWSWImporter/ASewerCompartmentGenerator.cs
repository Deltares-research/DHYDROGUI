using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public abstract class ASewerCompartmentGenerator : IGwswFeatureGenerator<ISewerFeature>
    {
        private static ILog Log = LogManager.GetLogger(typeof(ASewerCompartmentGenerator));

        public abstract ISewerFeature Generate(GwswElement gwswElement);

        protected ISewerFeature CreateCompartment<T>(GwswElement gwswElement) 
            where T : Compartment, new()
        {
            if (gwswElement == null) return null;

            var compartmentToBeAdded = CreateNewCompartment<T>(gwswElement);
            SetCompartmentProperties(compartmentToBeAdded, gwswElement);

            return compartmentToBeAdded;
        }

        private T CreateNewCompartment<T>(GwswElement gwswElement) where T : Compartment, new()
        {
            var compartmentIdAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);
            var compartmentName = compartmentIdAttribute.GetValidStringValue();
            if (compartmentName == null)
            {
                Log.WarnFormat(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_will_be_added_to_the_network_with_a_unique_name, "Compartment", gwswElement.GetElementLine());
            }

            return new T { Name = compartmentName };
        }

        protected abstract void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement);
    }
}