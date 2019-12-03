using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerConnectionPipeGenerator: SewerConnectionGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            if (gwswElement == null) return null;
            return CreateSewerConnection<Pipe>(gwswElement);
        }

        protected override void SetSewerConnectionAttributes(ISewerConnection sewerConnection, GwswElement gwswElement)
        {
            var newPipe = sewerConnection as IPipe;
            if (newPipe == null) return;

            base.SetSewerConnectionAttributes(newPipe, gwswElement);

            var auxDouble = 0.0;

            var pipeIdAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeId);
            newPipe.PipeId = pipeIdAttribute.GetValidStringValue();

            var profileDefinitionIdAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.CrossSectionDefinitionId);
            newPipe.CrossSectionDefinitionName = profileDefinitionIdAttribute.GetValidStringValue();
        }
    }
}