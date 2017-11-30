using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionPipeGenerator: SewerConnectionGenerator, ISewerNetworkFeatureGenerator
    {
        public new INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement == null) return null;
            return CreateSewerConnection<Pipe>(gwswElement, network, SetPipeAttributes);
        }

        private static void SetPipeAttributes(ISewerConnection element, GwswElement gwswElement, IHydroNetwork network = null)
        {
            var newPipe = element as Pipe;
            if (newPipe == null) return;

            var auxDouble = 0.0;

            var pipeIndicator = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeIndicator);
            if( pipeIndicator.IsValidAttribute())
                newPipe.PipeId = pipeIndicator.GetValidStringValue();

            var profileDef = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.CrossSectionDef);
            if (profileDef.IsValidAttribute() && network != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
                var foundCs = network.SewerProfiles.FirstOrDefault(n => n.Name.Equals(profileDef.ValueAsString));
                if (foundCs == null)
                {
                    foundCs = CrossSection.CreateDefault(CrossSectionType.Standard, null);
                    foundCs.Name = profileDef.ValueAsString;
                    network.SewerProfiles.Add(foundCs);
                }
                newPipe.SewerProfile = (CrossSection)foundCs;
            }
        }
    }
}