using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class CrossSectionView : UserControl, IReusableView, ICrossSectionHistoryCapableView
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionView));

        private static readonly Bitmap XYZImage = Properties.Resources.CrossSectionSmallXYZ;
        private static readonly Bitmap YZImage = Properties.Resources.CrossSectionSmall;
        private static readonly Bitmap ZWImage = Properties.Resources.CrossSectionTabulatedSmall;

        private bool locked;
        private CrossSectionViewModel crossSectionViewModel;
        
        private Func<ICrossSection, IEnumerable<IConveyanceCalculator>> getConveyanceCalculators;
        
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
        
        public CrossSectionView()
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

                UpdateShowConveyanceButton();
            }
        }

        private void InitializeDefinitionSharingPanel()
        {
            panel1.Visible = CrossSectionViewModel.IsShareableCrossSectionType;

            // hack because data binding does not work properly...?
            rbLocal.Checked = CrossSectionViewModel.UseLocalDefinition;
            rbShared.Checked = CrossSectionViewModel.UseSharedDefinition;

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
                    } 
                    while (uniqueNames.Contains(unique));
                    uniqueNames.Add(unique);    
                }
            }
            return uniqueNames;
        }

        private void Unsubscribe()
        {
            ((INotifyPropertyChanged)CrossSection).PropertyChanged -= CrossSectionPropertyChanged;
            CrossSectionViewModel = null;
        }

        private void Subscribe()
        {
            ((INotifyPropertyChanged)CrossSection).PropertyChanged += CrossSectionPropertyChanged;
        }

        void CrossSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName ==  "Definition" || e.PropertyName == "InnerDefinition")
            {
                SetDefinitionView(); 
                InitializeDefinitionSharingPanel();
            }
            if (e.PropertyName == "Name")
            {
                Text = CrossSection.Name;
            }
            if (e.PropertyName == nameof(CrossSectionViewModel.LevelShift))
            {
                CrossSectionViewModel.FireLevelShiftChanged();
            }
        }

        private void SetDefinitionView()
        {
            var definitionViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(CrossSection.Definition, CrossSection.HydroNetwork);
            definitionView.Data = CrossSection.Definition;
            definitionView.ViewModel = definitionViewModel;
        }


        private ICrossSection CrossSection { get; set; }
        
        public Image Image {
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
            CrossSectionViewModel.ShareDefinition();
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

        public Func<ICrossSection, IEnumerable<IConveyanceCalculator>> GetConveyanceCalculators
        {
            set
            {
                getConveyanceCalculators = value;
                UpdateShowConveyanceButton();
            }
        }

        private void UpdateShowConveyanceButton()
        {
            panelForConveyanceBtn.Visible = Data != null
                                            && getConveyanceCalculators != null
                                            && getConveyanceCalculators(CrossSection).Any()
                                            && (CrossSection.CrossSectionType == CrossSectionType.YZ ||
                                                CrossSection.CrossSectionType == CrossSectionType.GeometryBased);
        }


        private void btnShowConveyance_Click(object sender, EventArgs e)
        {
            if (getConveyanceCalculators == null) return;
            
            var calculator = getConveyanceCalculators(CrossSection).FirstOrDefault();
            if (calculator == null) return;

            IFunction conveyanceTable = null;
            try
            {
                conveyanceTable = calculator.GetConveyance(CrossSection);
            }
            catch (Exception)
            {
                Log.Error("Failed to get conveyance data");
                return;
            }

            if(conveyanceTable == null) return;

            var functionView = new FunctionView
                {
                    Data = conveyanceTable,
                    Dock = DockStyle.Fill
                };
                
            var form = new Form
                {
                    Size = new Size(1200, 600),
                    MinimizeBox = false,
                    StartPosition = FormStartPosition.CenterScreen,
                    ShowInTaskbar = false,
                    TopMost = true,
                    Text = "Conveyance view"
                };

            functionView.ChartView.Chart.Legend.ShowCheckBoxes = true;
            form.Shown += (o, args) => functionView.TableView.BestFitColumns(false);
            form.Controls.Add(functionView);
            form.ShowDialog();
        }
    }

}
