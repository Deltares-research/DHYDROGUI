using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class CrossSectionPipeView : UserControl, IReusableView, ICrossSectionHistoryCapableView
    {
        private static readonly Bitmap XYZImage = Properties.Resources.CrossSectionSmallXYZ;
        private static readonly Bitmap YZImage = Properties.Resources.CrossSectionSmall;
        private static readonly Bitmap ZWImage = Properties.Resources.CrossSectionTabulatedSmall;


        private bool locked;
        private CrossSectionViewModel crossSectionViewModel;

        private CrossSectionViewModel CrossSectionViewModel
        {
            get { return crossSectionViewModel; }
            set
            {
                if (crossSectionViewModel != null)
                {
                    crossSectionViewModel.SharedDefinitionsChanged -= CrossSectionViewModelSharedDefinitionsChanged;
                }

                crossSectionViewModel = value;
                if (crossSectionViewModel != null)
                {
                    crossSectionViewModel.SharedDefinitionsChanged += CrossSectionViewModelSharedDefinitionsChanged;
                }
            }
        }

        void CrossSectionViewModelSharedDefinitionsChanged(object sender, EventArgs e)
        {
            InitializeDefinitionSharingPanel();
        }

        public CrossSectionPipeView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get { return CrossSection; }
            set
            {
                if (Data == value)
                {
                    return;
                }

                if (CrossSection != null)
                {
                    Unsubscribe();
                }

                CrossSection = (ICrossSection) value;
                if (CrossSection != null)
                {
                    Subscribe();
                    Text = CrossSection.Name;

                    SetDefinitionView();

                    CrossSectionViewModel = new CrossSectionViewModel(CrossSection);
                    bindingSourceCrossSectionViewModel.DataSource = CrossSectionViewModel;

                    InitializeDefinitionSharingPanel();
                }
                else
                {
                    //clean up via data
                    definitionView.Data = null;
                }
            }
        }

        private void InitializeDefinitionSharingPanel()
        {
            panel1.Visible = CrossSectionViewModel.IsShareableCrossSectionType;

            comboBoxDefinitions.SelectedIndexChanged -= ComboBoxDefinitionsSelectedIndexChanged;
            comboBoxDefinitions.Items.Clear();

            var names = CrossSection.HydroNetwork.SharedCrossSectionDefinitions.Select(d => d.Name);
            var uniqueNames = GetUniqueNames(names);

            foreach (var name in uniqueNames)
            {
                comboBoxDefinitions.Items.Add(name);
            }

            if (CrossSection.Definition.IsProxy)
            {
                comboBoxDefinitions.SelectedIndex = CrossSectionViewModel.SharedDefinitionIndex;
            }

            comboBoxDefinitions.SelectedIndexChanged += ComboBoxDefinitionsSelectedIndexChanged;
        }

        private static IEnumerable<string> GetUniqueNames(IEnumerable<string> names)
        {
            var uniqueNames = new List<string>();

            foreach (var name in names)
            {
                if (!uniqueNames.Contains(name))
                {
                    uniqueNames.Add(name);
                }
                else
                {
                    int id = 1;
                    string unique;
                    do
                    {
                        unique = String.Format("{0}({1})", name, id++);
                    } while (uniqueNames.Contains(unique));

                    uniqueNames.Add(unique);
                }
            }

            return uniqueNames;
        }

        private void Unsubscribe()
        {
            ((INotifyPropertyChanged) CrossSection).PropertyChanged -= CrossSectionPropertyChanged;
            CrossSectionViewModel = null;
        }

        private void Subscribe()
        {
            ((INotifyPropertyChanged) CrossSection).PropertyChanged += CrossSectionPropertyChanged;
        }

        void CrossSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CrossSection.Definition) ||
                e.PropertyName == nameof(CrossSectionDefinitionProxy.InnerDefinition))
            {
                SetDefinitionView();
                InitializeDefinitionSharingPanel();
            }

            if (e.PropertyName == nameof(CrossSection.Name))
            {
                Text = CrossSection.Name;
            }
        }

        private void SetDefinitionView()
        {
            var definitionViewModel =
                CrossSectionDefinitionViewModelProvider.GetViewModel(CrossSection.Definition,
                    CrossSection.HydroNetwork);
            definitionViewModel.IsCurrentlyOnChannel = CrossSection.Branch is IChannel;
            definitionView.Data = CrossSection.Definition;
            definitionView.ViewModel = definitionViewModel;
        }


        private ICrossSection CrossSection { get; set; }

        public Image Image
        {
            get
            {
                if (Data != null)
                {
                    if (CrossSection.CrossSectionType == CrossSectionType.GeometryBased)
                        return XYZImage;
                    if (CrossSection.CrossSectionType == CrossSectionType.YZ)
                        return YZImage;
                    if (CrossSection.CrossSectionType == CrossSectionType.ZW)
                        return ZWImage;
                }

                return null;
            }
            set { }
        }

        public bool Locked
        {
            get { return locked; }
            set
            {

                if (locked == value)
                    return;
                locked = value;
                if (LockedChanged != null)
                {
                    LockedChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler LockedChanged;

        public void EnsureVisible(object item)
        {
            definitionView.EnsureVisible(item);
        }

        public ViewInfo ViewInfo { get; set; }

        public event EventHandler StatusMessage
        {
            add { definitionView.StatusMessage += value; }
            remove { definitionView.StatusMessage -= value; }
        }

        private void ComboBoxDefinitionsSelectedIndexChanged(object sender, EventArgs e)
        {
            //don't turn to proxy on this call. should be because of a change in radiobutton..

            //might get here because a def is added to the shared list
            if (!CrossSection.Definition.IsProxy)
                return;

            //update the VM...do this with binding later
            var idx = comboBoxDefinitions.SelectedIndex;
            if (idx != -1)
            {
                CrossSectionViewModel.SetSharedDefinition(idx);
            }

        }

        private void BtnShareClick(object sender, EventArgs e)
        {
            CrossSectionViewModel.CreateNewSharedCrossSectionDefinition();
        }

        public Action<object, EventArgs> EditClickedAction { get; set; }

        private void BtnEditClick(object sender, EventArgs e)
        {
            var args = new SelectedItemChangedEventArgs(CrossSectionViewModel?.SharedDefinition);
            EditClickedAction?.Invoke(this, args);
        }

        public bool HistoryToolEnabled
        {
            get { return definitionView != null && definitionView.HistoryToolEnabled; }
            set
            {
                if (definitionView != null)
                {
                    definitionView.HistoryToolEnabled = value;
                }
            }
        }
    }
}
