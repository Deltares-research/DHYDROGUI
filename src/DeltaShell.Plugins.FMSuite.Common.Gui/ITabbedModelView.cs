using DelftTools.Controls;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public interface ITabbedModelView : IView
    {
        void SwitchToTab(string tabTitle);
    }
}