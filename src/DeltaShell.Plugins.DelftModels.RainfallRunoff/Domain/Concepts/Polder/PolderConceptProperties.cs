using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder
{
    public class PolderConceptProperties : ObjectProperties<PolderConcept>
    {
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        //todo: extend this when required
    }
}