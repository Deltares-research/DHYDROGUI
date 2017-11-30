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

            var pipeIndicator = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.PipeIndicator);
            if (pipeIndicator?.ValueAsString != null)
            {
                newPipe.PipeId = pipeIndicator.ValueAsString;
            }

            var profileDef = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.CrossSectionDef);
            if (profileDef != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
                if (profileDef.ValueAsString != string.Empty && network != null)
                {
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
}