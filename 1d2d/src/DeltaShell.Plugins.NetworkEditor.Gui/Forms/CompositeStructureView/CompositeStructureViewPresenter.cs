using System;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public sealed class CompositeStructureViewPresenter : ICanvasEditor, IDisposable
    {
        private CompositeStructureViewDataController _sideViewDataController;
        private ICompositeStructureView view;
        private ISelectionContainer selectionContainer;

        protected void OnSelectionChanged(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.Item is ICompositeBranchStructure) return;

            SelectObjectInViews(args.Item as IStructure1D);
        }

        public ICompositeStructureView View
        {
            get { return view; }
            set
            {
                view = value;
            }
        }

        public Func<object,IView> CreateView { get; set; } 

        public void SetModelIntoView()
        {
            if (View != null)
            {
                var compositeBranchStructure = View.Data;

                GenerateViewTitle();

                // Create the form view list
                if (compositeBranchStructure == null)
                {
                    View.SideView.Data = null;
                    View.CrossSectionStructureView.Data = null;
                    return;
                }

                View.SetFormViews(CreateView);

                _sideViewDataController = CompositeStructureViewDataBuilder.GetCompositeStructureViewDataForStructure((ICompositeBranchStructure) View.Data);
                View.SideView.Data = _sideViewDataController.NetworkRoute;
                View.SideView.DataController = _sideViewDataController;
                SideViewUpdateCrossSectionStyles();
                View.CrossSectionStructureView.Data = _sideViewDataController;
            }
        }

        /// <summary>
        /// Generates a view title based on the IStructure available in <paramref name=""/>
        /// </summary>
        public void GenerateViewTitle()
        {
            if (View == null)
            {
                return;
            }

            var data = View.Data as ICompositeBranchStructure;
            if (data == null)
            {
                View.Text = "{}";
            }
            else if (data.Name != "StructureFeature")
            {
                View.Text = data.Name; //original name for composite, so use it!
            }
            else if (data.Structures == null || data.Structures.Count == 0)
            {
                View.Text = "[no structures]";
            }
            else
            {
                var count = data.Structures.Count;
                var firstStructure = data.Structures.First();
                var name = firstStructure.Name;
                if (!String.IsNullOrEmpty(firstStructure.LongName))
                {
                    name += ", " + firstStructure.LongName;
                }

                if (count == 1)
                {
                    View.Text = name;
                }
                else
                {
                    View.Text = name + String.Format(" (+{0} more)", count - 1);
                }
            }
        }

        private void SideViewUpdateCrossSectionStyles()
        {
            if (null != _sideViewDataController.CrossSectionBefore)
            {
                View.SideView.UpdateStyles(_sideViewDataController.CrossSectionBefore,
                                           new VectorStyle
                                           {
                                               Fill = new SolidBrush(Color.FromArgb(100, Color.LightPink)),
                                               Line = new Pen(Color.Black)
                                           },
                                           new VectorStyle
                                           {
                                               Fill = new SolidBrush(Color.LightPink),
                                               Line = new Pen(Color.Black)
                                           }
                    );
            }
            if (null != _sideViewDataController.CrossSectionAfter)
            {
                View.SideView.UpdateStyles(_sideViewDataController.CrossSectionAfter,
                                           new VectorStyle
                                               {
                                                   Fill = new SolidBrush(Color.FromArgb(100, Color.LightGreen)),
                                                   Line = new Pen(Color.Black)
                                               },
                                           new VectorStyle
                                               {
                                                   Fill = new SolidBrush(Color.LightGreen),
                                                   Line = new Pen(Color.Black)
                                               }
                    );
            }
        }

        /// <summary>
        /// Selects the object in the contained views of the composite view
        /// </summary>
        /// <param name="structure">The structure which becomes the new selected object</param>
        public void SelectObjectInViews(IStructure1D structure)
        {
            if (View != null)
            {
                //don't update invisible views because setting the active form
                //activates the view :(
                if (!View.Visible)
                {
                    return;
                }
                IStructure1D visibleSelection = structure;
                if (!((ICompositeBranchStructure)View.Data).Structures.Contains(structure))
                {
                    visibleSelection = null;
                }

                if (visibleSelection != null)
                {
                    View.ActivateFormView(visibleSelection);                    
                }



                if (visibleSelection is ICompositeBranchStructure)
                {
                    visibleSelection = ((ICompositeBranchStructure) visibleSelection).Structures[0];
                }
                View.CrossSectionStructureView.SelectedStructure = visibleSelection;
                View.SideView.SelectedFeature = visibleSelection;
            }
        }

        public bool CanSelectItem
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).CanSelectItem; }
        }

        public bool IsSelectItemActive
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsSelectItemActive; }
            set
            {
                ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsSelectItemActive = value;
                ((StructurePresenter) View.SideView.CommandReceiver).IsSelectItemActive = value;
            }
        }

        public bool CanMoveItem
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).CanMoveItem; }
        }

        public bool IsMoveItemActive
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsMoveItemActive; }
            set
            {
                ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsMoveItemActive = value;
                ((StructurePresenter) View.SideView.CommandReceiver).IsMoveItemActive = value;
            }
        }

        public bool CanMoveItemLinear
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).CanMoveItemLinear; }
        }

        public bool IsMoveItemLinearActive
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsMoveItemLinearActive; }
            set
            {
                ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsMoveItemLinearActive = value;
                ((StructurePresenter) View.SideView.CommandReceiver).IsMoveItemLinearActive = value;
            }
        }

        public bool CanDeleteItem
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).CanDeleteItem; }
        }

        public bool IsDeleteItemActive
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsDeleteItemActive; }
            set
            {
                ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsDeleteItemActive = value;
                ((StructurePresenter) View.SideView.CommandReceiver).IsDeleteItemActive = value;
            }
        }

        public bool CanAddPoint
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).CanAddPoint; }
        }

        public bool IsAddPointActive
        {
            get { return ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsAddPointActive; }
            set
            {
                ((StructurePresenter) View.CrossSectionStructureView.CommandReceiver).IsAddPointActive = value;
                ((StructurePresenter) View.SideView.CommandReceiver).IsAddPointActive = value;
            }
        }

        public bool IsRemovePointActive { get; set; }
        public bool CanRemovePoint { get { return false; } }

        public ISelectionContainer SelectionContainer
        {
            get { return selectionContainer; }
            set
            {
                selectionContainer = value;
                if (selectionContainer == null) return;

                selectionContainer.SelectionChanged += OnSelectionChanged;
            }
        }

        public void Dispose()
        {
            if (selectionContainer != null)
            {
                selectionContainer.SelectionChanged -= OnSelectionChanged;
            }
        }

    }
}