using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerCompartmentGenerator: SewerManholeGenerator
    {
        /*It generates compartments AND manholes when needed. However the return value will always be a manhole because compartments are not
         INetworkFeatures */
        private static ILog Log = LogManager.GetLogger(typeof(SewerCompartmentGenerator));

        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (gwswElement == null) return null;
            var manhole = CreateCompartmentForManhole<Compartment>(gwswElement, network, importHelper);
            return manhole;
        }

        private T FindOrGetNewCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            //We know they are not null, otherwise we would have already returned in the CompartmentFactory method.
            var compartmentAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);
            string compartmentName;
            if (compartmentAttribute.IsValidAttribute())
            {
                compartmentName = compartmentAttribute.GetValidStringValue();
            }
            else
            {
                compartmentName = GetCompartmentUniqueName(network);
                Log.WarnFormat(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_has_been_created_as__2_, "Compartment", gwswElement.GetElementLine(), compartmentName);
            }

            var parentManhole = FindManhole(gwswElement, network);

            if (parentManhole != null && parentManhole.ContainsCompartmentWithName(compartmentName))
            {
                var foundCompartment = parentManhole.GetCompartmentByName(compartmentName) as T;
                if (foundCompartment != null)
                    return foundCompartment;
            }

            return new T { Name = compartmentName };
        }

        private static string GetCompartmentUniqueName(IHydroNetwork network)
        {
            var compartmentList = new List<Compartment>();
            if (network != null)
                compartmentList = network.Manholes.SelectMany(m => m.Compartments).ToList();

            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartmentList, "Compartment"); ;
        }

        private static string GetCompartmentName(GwswElement gwswElement, IHydroNetwork network)
        {
            var compartmentAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);

            //if we could not place an ID then we use the compartment name to create a place holder (if needed)
            var compartmentName = compartmentAttribute.IsValidAttribute()
                ? compartmentAttribute.GetValidStringValue()
                : GetCompartmentUniqueName(network);
            return compartmentName;
        }

        private static string GetManholeUniqueNameForCompartment(IHydroNetwork network, string compartmentName)
        {
            var manholeList = new List<IManhole>();
            if (network != null)
                manholeList = network.Manholes.ToList();

            return NetworkHelper.GetUniqueName("Manhole{0:D2}ForCompartment"+compartmentName, manholeList, "Manhole");
        }

        protected IManhole CreateCompartmentForManhole<T>(GwswElement gwswElement, IHydroNetwork network = null,
            object importHelper = null) where T : Compartment, new()
        {
            if (gwswElement == null) return null;

            var compartment = FindOrGetNewCompartment<T>(gwswElement, network);

            SetCompartmentAttributes(compartment, gwswElement);
            var manhole = GetNewOrExistingManhole(gwswElement, network);
            if (!manhole.Compartments.Contains(compartment))
            {
                SetManholeCoordinateAttributes(manhole, gwswElement);
                manhole.Compartments.Add(compartment);
            }
            return manhole;
        }

        protected override IManhole GetNewOrExistingManhole(GwswElement gwswElement, IHydroNetwork network)
        {
            var manhole = FindManhole(gwswElement, network);
            if (manhole != null) return manhole;

            var manholeName = GetManholeName(gwswElement, network);
            manhole = new Manhole(manholeName);
            SetManholeCoordinateAttributes(manhole, gwswElement);
            network?.Nodes.Add(manhole);

            Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholeName, GetCompartmentName(gwswElement, network));
            return manhole;
        }

        protected override string GetManholeName(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            var compartmentName = GetCompartmentName(gwswElement, network);
            if (manholeAttribute.IsValidAttribute())
                return manholeAttribute.GetValidStringValue();
            
            Log.WarnFormat(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_has_been_created_as__2_, "Manhole", gwswElement.GetElementLine(), compartmentName);
            return manholeAttribute.IsValidAttribute() ? manholeAttribute.GetValidStringValue() : GetManholeUniqueNameForCompartment(network, compartmentName);
        }

        protected override IManhole FindManhole(GwswElement gwswElement, IHydroNetwork network)
        {
            if (network == null) return null;

            Manhole manholeInNetwork = null;
            string compartmentName;
            if (gwswElement.IsValidGwswCompartment())
            {
                var manholeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
                compartmentName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId).ValueAsString;

                var manholeName = manholeAttribute.IsValidAttribute()
                    ? manholeAttribute.ValueAsString
                    : compartmentName;
                manholeInNetwork = network.Manholes.FirstOrDefault(m => m.Name.Equals(manholeName)) as Manhole;
            }

            //If it was not found, try searching by containing this compartment.
            if (manholeInNetwork == null)
            {
                compartmentName = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId).ValueAsString;
                manholeInNetwork = network.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(compartmentName)) as Manhole;
            }

            return manholeInNetwork;
        }

        protected virtual void SetCompartmentAttributes(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment()) return;

            // Set the rest of manhole values
            double auxDouble;
            var nodeLength = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeLength);
            if( nodeLength.TryGetValueAsDouble(out auxDouble))
                compartment.ManholeLength = auxDouble;

            var nodeWidth = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeWidth);
            if( nodeWidth.TryGetValueAsDouble(out auxDouble))
                compartment.ManholeWidth = auxDouble;

            var floodableArea = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.FloodableArea);
            if(floodableArea.TryGetValueAsDouble(out auxDouble))
                compartment.FloodableArea = auxDouble;

            var bottomLevel = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.BottomLevel);
            if( bottomLevel.TryGetValueAsDouble(out auxDouble))
                compartment.BottomLevel = auxDouble;

            var surfaceLevel = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.SurfaceLevel);
            if (surfaceLevel.TryGetValueAsDouble(out auxDouble))
                compartment.SurfaceLevel = auxDouble;

            /* Coordinates are not set in the ManholeGenerator*/
            // Set shape value of the manhole
            var nodeShape = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeShape);
            if(nodeShape.IsValidAttribute())
            {
                compartment.Shape = nodeShape.GetValueFromDescription<CompartmentShape>();
            }
        }
    }
}