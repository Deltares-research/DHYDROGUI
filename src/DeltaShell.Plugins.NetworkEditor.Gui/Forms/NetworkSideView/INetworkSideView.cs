using System;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public interface INetworkSideView : IView
    {
        event EventHandler<SelectedItemChangedEventArgs> SelectionChanged;
        NetworkSideViewDataController DataController { get; set; }
        IFeature SelectedFeature { get; set; }
        object CommandReceiver { get; }
        void UpdateStyles(IBranchFeature branchFeature, VectorStyle normalStyle, VectorStyle selectedStyle);
    }
}