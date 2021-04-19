using DelftTools.Hydro.SewerFeatures;

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
            
            newPipe.PipeId = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.PipeId);
            newPipe.CrossSectionDefinitionName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.CrossSectionDefinitionId);
        }
    }
}