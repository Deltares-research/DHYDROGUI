using DelftTools.Hydro.SewerFeatures;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerConnectionPipeGenerator: SewerConnectionGenerator
    {
        public SewerConnectionPipeGenerator(ILogHandler logHandler) : base(logHandler)
        {
        }
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
            
            newPipe.PipeId = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.PipeId, logHandler);
            newPipe.CrossSectionDefinitionName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.CrossSectionDefinitionId, logHandler);
        }
    }
}