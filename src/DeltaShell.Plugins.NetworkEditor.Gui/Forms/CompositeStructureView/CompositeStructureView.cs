using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public partial class CompositeStructureView : UserControl, ICompositeStructureView, IReusableView
    {
        private static Control PreviousFocusedControl;
        private CompositeStructureViewPresenter presenter;
        private ICompositeBranchStructure data;
        private IStructure1D selectedStructure;

        private bool settingFormsView;
        private bool locked;

        public event EventHandler LockedChanged;

        public CompositeStructureView()
        {
            ChildViews = new EventedList<IView>();
            Text = "Composite structure view";
            InitializeComponent();
            Load += delegate
            {
                //presenter.ViewReady(); // <- should always be called first
                tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
                CrossSectionStructureView.SelectionChanged += CrossSectionStructureView_SelectionChanged;
                networkSideView.SelectionChanged += NetworkSideView_SelectionChanged;
                //presenter.ViewReady(); // <- should always be called first
            };
            //hide chart header in this view.
            networkSideView.AllowFeatureVisibilityChanges = false;
            networkSideView.ChartHeaderVisible = false;
            networkSideView.ChartLegendVisible = false;
            //disable context menu in side (with show coverages etc)
            networkSideView.ContextMenuStripEnabled = false;
        }

        public ICompositeBranchStructure Data
        {
            get
            {
                return data;
            }
            set
            {
                DataBindings.Clear();

                if (value == null)
                {
                    networkSideView.Data = null;
                    structureView.Data = null;
                }

                // TODO: subscribe to the CompositeStructure.CollectionChange here!

                if (data != null && data.Network != null)
                {
                    ((INotifyCollectionChanged) Data.Network).CollectionChanged -= NetworkCollectionChanged;
                    ((INotifyPropertyChanged) Data.Network).PropertyChanged -= NetworkPropertyChanged;
                }

                data = value;

                if (data == null)
                {
                    foreach (IView childView in ChildViews.ToArray())
                    {
                        childView.Data = null;
                        childView.Dispose();
                    }
                }

                if (value != null && value.Network != null)
                {
                    ((INotifyCollectionChanged) Data.Network).CollectionChanged += NetworkCollectionChanged;
                    ((INotifyPropertyChanged) Data.Network).PropertyChanged += NetworkPropertyChanged;
                }

                if (presenter != null)
                {
                    Presenter.SetModelIntoView();
                }
            }
        }

        public CompositeStructureViewPresenter Presenter
        {
            get
            {
                return presenter;
            }
            set
            {
                presenter = value;
                presenter.View = this;

                if (data != null)
                {
                    Presenter.SetModelIntoView();
                }
            }
        }

        public sealed override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        object IView.Data
        {
            get
            {
                return Data;
            }
            set
            {
                Data = (ICompositeBranchStructure) value;
            }
        }

        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Gets the structure that has the focus
        /// </summary>
        public IStructure1D SelectedStructure
        {
            get
            {
                return selectedStructure;
            }
            set
            {
                selectedStructure = value;
                SetSelection(value);
            }
        }

        public IStructureView CrossSectionStructureView
        {
            get
            {
                return structureView;
            }
        }

        /// <summary>
        /// Gets the network side view instance
        /// </summary>
        public INetworkSideView SideView
        {
            get
            {
                return networkSideView;
            }
        }

        public IEventedList<IView> ChildViews { get; }

        public bool HandlesChildViews
        {
            get
            {
                return true;
            }
        }

        public bool Locked
        {
            get
            {
                return locked;
            }
            set
            {
                locked = value;
                if (LockedChanged != null)
                {
                    LockedChanged(this, new EventArgs());
                }
            }
        }

        public void EnsureVisible(object item) {}

        public new void Dispose()
        {
            PreviousFocusedControl = null;
            tabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
            CrossSectionStructureView.SelectionChanged -= CrossSectionStructureView_SelectionChanged;
            networkSideView.SelectionChanged -= NetworkSideView_SelectionChanged;

            base.Dispose();
        }

        public void SetFormViews(Func<object, IView> createView)
        {
            RememberPreviouslyFocusedControl();
            settingFormsView = true;

            if (Data != null)
            {
                tabControl.Enabled = false; // prevent firing Enter event
                tabControl.TabPages.Clear();
                ChildViews.Clear();

                IEnumerable<IStructure1D> structures = Data.Structures;

                foreach (IStructure1D structure in structures)
                {
                    IView view = createView?.Invoke(structure);
                    if (view == null)
                    {
                        continue;
                    }

                    var tabPage = new TabPage(structure.Name)
                    {
                        Name = structure.Name,
                        AutoScroll = true,
                        Tag = structure
                    };

                    Control control = structure is Culvert
                                          ? new ElementHost {Child = (System.Windows.Controls.Control) view}
                                          : (Control) view;

                    control.Dock = DockStyle.Fill;

                    // HACK: increase height by 30%, looks like a bug in DotNetBar in combination with TabControl
                    tabPage.AutoScrollMinSize = new Size(control.Width, (int) (control.Height * 1.3));
                    tabPage.Controls.Add(control);

                    tabControl.TabPages.Add(tabPage);
                    ChildViews.Add(view);
                }

                if (tabControl.SelectedTab != null)
                {
                    SelectedStructure = Data.Structures.ElementAt(tabControl.SelectedTab.TabIndex);
                }

                tabControl.Enabled = true;
            }

            settingFormsView = false;
            RestorePreviouslyFocusedControl();
        }

        /// <summary>
        /// Activates the form view for the given structure
        /// </summary>
        /// <param name="structure"></param>
        public void ActivateFormView(IStructure1D structure)
        {
            RememberPreviouslyFocusedControl();

            string pageKey = structure.Name;
            if (tabControl.TabPages.ContainsKey(pageKey))
            {
                tabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;

                tabControl.Enabled = false;
                tabControl.SelectTab(pageKey);
                tabControl.Enabled = true;

                SelectedStructure = structure;
                tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            }

            RestorePreviouslyFocusedControl();
        }

        public void ActivateChildView(IView childView)
        {
            if (childView.Data is IStructure1D structure)
            {
                RememberPreviouslyFocusedControl();

                string pageKey = structure.Name;
                if (tabControl.TabPages.ContainsKey(pageKey))
                {
                    tabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;

                    tabControl.Enabled = false;
                    tabControl.SelectTab(pageKey);
                    tabControl.Enabled = true;

                    SelectedStructure = structure;
                    tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
                }

                RestorePreviouslyFocusedControl();
            }
        }

        private void NetworkSideView_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            SetSelection(e.Item);
        }

        private void CrossSectionStructureView_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            SetSelection(e.Item);
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                SetSelection(tabControl.TabPages[tabControl.SelectedIndex].Tag);
            }
        }

        private void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            INetwork network = Data.Network;

            if (Equals(sender, network) && e.PropertyName == nameof(network.IsEditing))
            {
                if (!network.IsEditing) // finished editing...
                {
                    Presenter.SetModelIntoView(); // refresh the view
                }
            }

            var structure = sender as IStructure1D;
            if (structure == null)
            {
                return;
            }

            UpdateStructure(structure, e.PropertyName);
        }

        private void UpdateStructure(IStructure1D structure, string propertyName)
        {
            if (!Data.Structures.Contains(structure))
            {
                return;
            }

            Presenter.GenerateViewTitle();
            structureView.Refresh();

            if (propertyName != "Name" && propertyName != "LongName")
            {
                return;
            }

            TabPage tabPage = tabControl.TabPages.OfType<TabPage>().FirstOrDefault(t => t.Tag == structure);
            if (tabPage == null)
            {
                return;
            }

            tabPage.Text = structure.Name;
        }

        /// <summary>
        /// Update CompositeStructureView for changes in the composite structure
        /// todo: move all this logic to presenter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            if (!(removedOrAddedItem is IStructure1D))
            {
                return;
            }

            var structure = (IStructure1D) removedOrAddedItem;
            if (structure.Network != null && structure.Network.IsEditing)
            {
                return; //TODO: wait for finish and refresh
            }

            if (structure.ParentStructure != Data)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Add)
            {
                Presenter.SetModelIntoView();
            }
        }

        private void SetSelection(object value)
        {
            if (null != Presenter.SelectionContainer)
            {
                Presenter.SelectionContainer.Selection = value;
            }
        }

        /// <summary>
        /// HACK: set previous focused control due to bug in windows forms tabcontrol
        /// </summary>
        private void RememberPreviouslyFocusedControl()
        {
            if (settingFormsView)
            {
                return; // Prevent unwanted/unexpected control remembering
            }

            PreviousFocusedControl = ControlHelper.GetFocusControl();
        }

        private void RestorePreviouslyFocusedControl()
        {
            if (PreviousFocusedControl != null && !settingFormsView) // Prevent unwanted/unexpected control restoring
            {
                PreviousFocusedControl.Focus();
            }
        }

        /// <summary>
        /// Draw the text horizontal for light aligned tabs.
        /// Todo: probably much better solution is to use list or a more advanced tab than the standard in winforms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(tabControl.TabPages[e.Index].Text, tabControl.Font, SystemBrushes.ControlText,
                                  e.Bounds, stringFormat);
        }
    }
}