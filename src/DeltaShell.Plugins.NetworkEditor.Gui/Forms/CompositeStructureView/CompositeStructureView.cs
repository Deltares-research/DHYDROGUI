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

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public partial class CompositeStructureView : UserControl, ICompositeStructureView, IReusableView
    {
        private CompositeStructureViewPresenter presenter;
        private ICompositeBranchStructure data;
        private IStructure1D selectedStructure;
        private static Control previousFocusedControl;

        private bool settingFormsView;
        private bool locked;
        private readonly IEventedList<IView> childViews = new EventedList<IView>();
        
        public CompositeStructureView()
        {
            Text = "Compound structure view";
            InitializeComponent();
            Load += delegate
                        {
                            tabControl1.SelectedIndexChanged += TabControl1SelectedIndexChanged;
                            CrossSectionStructureView.SelectionChanged += CrossSectionStructureViewSelectionChanged;
                            networkSideView1.SelectionChanged += networkSideView1_SelectionChanged;
                        };

            //hide chart header in this view.
            networkSideView1.AllowFeatureVisibilityChanges = false;
            networkSideView1.ChartHeaderVisible = false;
            networkSideView1.ChartLegendVisible = false;
            //disable context menu in side (with show coverages etc)
            networkSideView1.ContextMenuStripEnabled = false;
        }

        void networkSideView1_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            SetSelection(e.Item);
        }

        void CrossSectionStructureViewSelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            SetSelection(e.Item);
        }

        void TabControl1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex != -1)
            {
                SetSelection(tabControl1.TabPages[tabControl1.SelectedIndex].Tag);
            }
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public ICompositeBranchStructure Data
        {
            get { return data; }
            set
            {
                DataBindings.Clear();

                if (value == null)
                {
                    networkSideView1.Data = null;
                    structureView1.Data = null;
                }

                if (data != null && data.Network != null)
                {
                    ((INotifyCollectionChanged)Data.Network).CollectionChanged -= NetworkCollectionChanged;
                    ((INotifyPropertyChanged)Data.Network).PropertyChanged -= NetworkPropertyChanged;
                }
                
                data = value;

                if(data == null)
                {
                    foreach (var childView in ChildViews.ToArray())
                    {
                        childView.Data = null;
                        childView.Dispose();
                    }
                }

                if (value != null && value.Network != null)
                {
                    ((INotifyCollectionChanged)Data.Network).CollectionChanged += NetworkCollectionChanged;
                    ((INotifyPropertyChanged)Data.Network).PropertyChanged += NetworkPropertyChanged;
                }

                if (presenter != null)
                {
                    Presenter.SetModelIntoView();
                }
            }
        }

        void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var network = Data.Network;

            if ((Equals(sender, network)) && e.PropertyName == nameof(network.IsEditing) && !network.IsEditing)
            {
                // finished editing...
                Presenter.SetModelIntoView(); // refresh the view
            }

            var structure = sender as IStructure1D;
            if ( structure == null) return;

            UpdateStucture(structure,e.PropertyName);
        }

        private void UpdateStucture(IStructure1D structure, string propertyName)
        {
            if (!Data.Structures.Contains(structure)) return;

            Presenter.GenerateViewTitle();
            structureView1.Refresh();

            if (propertyName != "Name" && propertyName != "LongName") return;

            var tabPage = tabControl1.TabPages.OfType<TabPage>().FirstOrDefault(t => t.Tag == structure);
            if (tabPage == null) return;

            tabPage.Text = structure.Name;
        }

        object IView.Data
        {
            get { return Data; }
            set
            {
                Data = (ICompositeBranchStructure) value;
            }
        }

        /// <summary>
        /// Update CompositeStructureView for changes in the composite structure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(e.GetRemovedOrAddedItem() is IStructure1D))
            {
                return;
            }
            var structure = (IStructure1D) e.GetRemovedOrAddedItem();
            if (structure.Network != null && structure.Network.IsEditing)
            {
                return;
            }
                
            if (structure.ParentStructure != Data)
            {
                return;
            }
            if (((e.Action == NotifyCollectionChangedAction.Remove)) || (e.Action == NotifyCollectionChangedAction.Add))
            {
                Presenter.SetModelIntoView();
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Gets the structure that has the focus
        /// </summary>
        public IStructure1D SelectedStructure
        {
            get { return selectedStructure; }
            set
            {
                selectedStructure = value;
                SetSelection(value);
            }
        }

        private void SetSelection(object value)
        {
            if (null != Presenter.SelectionContainer)
            {
                Presenter.SelectionContainer.Selection = value;
            }
        }

        public IStructureView CrossSectionStructureView
        {
            get { return structureView1; }
        }

        /// <summary>
        /// Gets the network side view instance
        /// </summary>
        public INetworkSideView SideView
        {
            get { return networkSideView1; }
        }

        public new void Dispose()
        {
            previousFocusedControl = null;
            tabControl1.SelectedIndexChanged -= TabControl1SelectedIndexChanged;
            CrossSectionStructureView.SelectionChanged -= CrossSectionStructureViewSelectionChanged;
            networkSideView1.SelectionChanged -= networkSideView1_SelectionChanged;


            base.Dispose();
        }

        public void SetFormViews(Func<object,IView> createView)
        {
            RememberPreviouslyFocusedControl();
            settingFormsView = true;

            if (Data != null)
            {
                tabControl1.Enabled = false; // prevent firing Enter event
                tabControl1.TabPages.Clear();
                childViews.Clear();

                IEnumerable<IStructure1D> structures = Data.Structures;
                
                foreach (var structure in structures)
                {
                    var view = createView != null ? createView(structure) : null;
                    if (view == null) continue;

                    var tabPage = new TabPage(structure.Name)
                    {
                        Name = structure.Name,
                        AutoScroll = true,
                        Tag = structure
                    };

                    var control = structure is Culvert
                        ? new ElementHost { Child = (System.Windows.Controls.Control) view }
                        : (Control) view;

                    tabPage.Controls.Add(control);

                    tabControl1.TabPages.Add(tabPage);
                    childViews.Add(view);
                }
                if (tabControl1.SelectedTab != null)
                {
                    SelectedStructure = Data.Structures.ElementAt(tabControl1.SelectedTab.TabIndex);
                }

                tabControl1.Enabled = true;
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

            //this.tabControl1.TabIndexChanged -= OnTabIndexChanged;
            string pageKey = structure.Name;
            if (tabControl1.TabPages.ContainsKey(pageKey))
            {
                tabControl1.SelectedIndexChanged -= TabControl1SelectedIndexChanged;
                
                tabControl1.Enabled = false;
                tabControl1.SelectTab(pageKey);
                tabControl1.Enabled = true;

                SelectedStructure = structure;
                tabControl1.SelectedIndexChanged += TabControl1SelectedIndexChanged;
            }

            //this.tabControl1.TabIndexChanged += OnTabIndexChanged;

            RestorePreviouslyFocusedControl();
        }

        /// <summary>
        /// HACK: set previous focused control due to bug in windows forms tabcontrol
        /// </summary>
        private void RememberPreviouslyFocusedControl()
        {
            if (settingFormsView) return; // Prevent unwanted/unexpected control remembering

            previousFocusedControl = ControlHelper.GetFocusControl();
        }

        private void RestorePreviouslyFocusedControl()
        {
            if (previousFocusedControl != null && !settingFormsView) // Prevent unwanted/unexpected control restoring
            {
                previousFocusedControl.Focus();
            }
        }

        public IEventedList<IView> ChildViews
        {
            get { return childViews; }
        }

        public bool HandlesChildViews { get { return true; } }
        
        public void ActivateChildView(IView childView)
        {
            var structure = childView.Data as IStructure1D;
            if (structure == null) return;

            RememberPreviouslyFocusedControl();

            var pageKey = structure.Name;
            if (tabControl1.TabPages.ContainsKey(pageKey))
            {
                tabControl1.SelectedIndexChanged -= TabControl1SelectedIndexChanged;

                tabControl1.Enabled = false;
                tabControl1.SelectTab(pageKey);
                tabControl1.Enabled = true;

                SelectedStructure = structure;
                tabControl1.SelectedIndexChanged += TabControl1SelectedIndexChanged;
            }

            RestorePreviouslyFocusedControl();
        }

        /// <summary>
        /// Draw the text horizontal for light aligned tabs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
            var stringFormat = new StringFormat
                                            {
                                                Alignment = StringAlignment.Center,
                                                LineAlignment = StringAlignment.Center
                                           };
            e.Graphics.DrawString(tabControl1.TabPages[e.Index].Text, tabControl1.Font, SystemBrushes.ControlText,
                                  e.Bounds, stringFormat);
        }

        public bool Locked
        {
            get { return locked; }
            set
            {

                locked = value;
                if (LockedChanged != null)
                {
                    LockedChanged(this, new EventArgs());
                }
            }
        }

        public event EventHandler LockedChanged;

        public CompositeStructureViewPresenter Presenter
        {
            get { return presenter; }
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
    }
}