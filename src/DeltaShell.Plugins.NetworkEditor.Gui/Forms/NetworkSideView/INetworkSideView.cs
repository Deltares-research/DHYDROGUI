using DelftTools.Controls;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public interface INetworkSideView : IView
    {
        NetworkSideViewDataController DataController { get; set; }
        
        IFeature SelectedFeature { get; set; }

        void UpdateStyles(IBranchFeature branchFeature, VectorStyle normalStyle, VectorStyle selectedStyle);
        
        object CommandReceiver { get; }
    }
}