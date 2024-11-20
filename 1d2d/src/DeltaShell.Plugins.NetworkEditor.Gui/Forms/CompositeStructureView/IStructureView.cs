using System;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public interface IStructureView : IView
    {
        event EventHandler<SelectedItemChangedEventArgs> SelectionChanged;
        IStructure1D SelectedStructure { get; set; }
        object CommandReceiver { get; }
    }

    /// <summary>
    /// Interface used by structureview to render data
    /// </summary>
    public interface IStructureViewData
    {
        ICompositeBranchStructure CompositeBranchStructure { get; }
        double ZMinValue { get; }
        double ZMaxValue { get; }
        void ResetMinMaxZ();
        HydroNetwork HydroNetwork { get;  }
    }
}