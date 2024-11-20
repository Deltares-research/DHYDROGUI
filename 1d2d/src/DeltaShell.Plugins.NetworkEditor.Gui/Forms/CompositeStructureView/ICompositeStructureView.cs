using System;
using DelftTools.Controls;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public interface ICompositeStructureView : ICompositeView
    {
        /// <summary>
        /// Gets or sets the selected structure
        /// </summary>
        IStructure1D SelectedStructure { get; }

        /// <summary>
        /// Sets the view data into the contained cross section view
        /// </summary>
        IStructureView CrossSectionStructureView { get; }

        /// <summary>
        /// Gets the network side view instance
        /// </summary>
        INetworkSideView SideView { get; }

        /// <summary>
        /// Activates the form view for the given structure
        /// </summary>
        /// <param name="structure"></param>
        void ActivateFormView(IStructure1D structure);
        
        /// <summary>
        /// Sets the specific form views (child forms in tab pages) for each structure
        /// </summary>        
        void SetFormViews(Func<object,IView> createView);

        /// <summary>
        /// Used to see if the view is visible and selection within view should change.
        /// </summary>
        bool Visible { get; }
    }
}