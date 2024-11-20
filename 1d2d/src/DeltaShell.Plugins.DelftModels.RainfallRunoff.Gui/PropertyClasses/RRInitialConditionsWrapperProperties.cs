using System.ComponentModel;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    [DisplayName("Initial condition properties")]
    public class RRInitialConditionsWrapperProperties : ObjectProperties<RRInitialConditionsWrapper>
    {
        public RRInitialConditionsWrapper.InitialConditionsType Type
        {
            get { return data.Type; }
        }

        public string Name
        {
            get { return data.Name; }
        }
    }
}