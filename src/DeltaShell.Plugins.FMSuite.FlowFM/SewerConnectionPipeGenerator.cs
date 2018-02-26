using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionPipeGenerator: SewerConnectionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (gwswElement == null) return null;
            return CreateSewerConnection<Pipe>(gwswElement, network, importHelper);
        }

        protected override void SetSewerConnectionAttributes(ISewerConnection element, GwswElement gwswElement, IHydroNetwork network, object helper = null)
        {
            var newPipe = element as IPipe;
            if (newPipe == null) return;

            base.SetSewerConnectionAttributes(newPipe, gwswElement, network, helper);

            var auxDouble = 0.0;

            var pipeIndicator = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeIndicator);
            if( pipeIndicator.IsValidAttribute())
                newPipe.PipeId = pipeIndicator.GetValidStringValue();

            var profileDef = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.CrossSectionDef);
            if (profileDef.IsValidAttribute() && network != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
                var foundCsd = network.SharedCrossSectionDefinitions.FirstOrDefault(n => n.Name.Equals(profileDef.ValueAsString));
                if (foundCsd == null)
                {
                    foundCsd = CrossSectionDefinitionStandard.CreateDefault();
                    foundCsd.Name = profileDef.ValueAsString;
                    network.SharedCrossSectionDefinitions.Add(foundCsd);
                }
                newPipe.SewerProfileDefinition = (CrossSectionDefinitionStandard)foundCsd;
            }
        }
    }
}