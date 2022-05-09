using System;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    class MockCompositeStructureView : ICompositeStructureView
    {
        private CompositeStructureViewPresenter presenter;
        private IEventedList<IView> childViews = new EventedList<IView>();

        public MockCompositeStructureView(CompositeStructureViewPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void Dispose()
        {            
        }

        object IView.Data
        {
            get { return Data; }
            set { Data = value as CompositeBranchStructure; }
        }

        public ICompositeBranchStructure Data { get; set; }

        public string Text { get; set; }
        public Image Image { get; set; }
        public void EnsureVisible(object item) { }

        public IStructure1D SelectedStructure { get; private set; }

        public IStructureView CrossSectionStructureView
        {
            get; private set; 
        }

        public INetworkSideView SideView { get; private set; }

        public void ActivateFormView(IStructure1D structure)
        {
            SelectedStructure = structure;
        }

        public void SetFormViews(Func<object, IView> createView)
        {
            throw new NotImplementedException();
        }
        
        public bool Visible
        {
            get { return true; }
        }

        public ViewInfo ViewInfo { get; set; }

        public bool HandlesChildViews { get; private set; }
        
        public void ActivateChildView(IView childView) { }

        IEventedList<IView> ICompositeView.ChildViews { get { return childViews; } }
    }
}