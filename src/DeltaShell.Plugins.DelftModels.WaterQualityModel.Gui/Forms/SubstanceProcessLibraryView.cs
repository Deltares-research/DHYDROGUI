using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms
{
    public partial class SubstanceProcessLibraryView : UserControl, IView, INameable
    {
        private readonly DelayedEventHandler<NotifyCollectionChangedEventArgs> dataCollectionChangedDelayedEventHandler;
        private SubstanceProcessLibrary library;
        private bool showNameAndDescriptionColumnsOnly;

        public SubstanceProcessLibraryView()
        {
            InitializeComponent();
            Image = new Bitmap(5, 5);

            dataCollectionChangedDelayedEventHandler =
                new DelayedEventHandler<NotifyCollectionChangedEventArgs>(delegate { UpdateDataGridViews(); }) {SynchronizingObject = this};

            InitializeTableViews();
        }

        /// <summary>
        /// Whether columns other than "Name" and "Description" should be hidden or not
        /// </summary>
        public bool ShowNameAndDescriptionColumnsOnly
        {
            get => showNameAndDescriptionColumnsOnly;
            set
            {
                showNameAndDescriptionColumnsOnly = value;

                InitializeTableViews();
            }
        }

        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// The <see cref="SubstanceProcessLibrary"/> associated with this view.
        /// </summary>
        public object Data
        {
            get => library;
            set
            {
                if (library != null)
                {
                    ((INotifyCollectionChanged) library).CollectionChanged -= dataCollectionChangedDelayedEventHandler;
                }

                library = (SubstanceProcessLibrary) value;

                if (library != null)
                {
                    ((INotifyCollectionChanged) library).CollectionChanged += dataCollectionChangedDelayedEventHandler;
                }

                UpdateDataGridViews();
            }
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                dataCollectionChangedDelayedEventHandler.Enabled = false;
                dataCollectionChangedDelayedEventHandler.Dispose();

                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void UpdateDataGridViews()
        {
            if (library == null)
            {
                return;
            }

            tableViewProcesses.Data = library.Processes != null
                                          ? new BindingList<WaterQualityProcess>(library.Processes)
                                          : null;
            tableViewParameters.Data = library.Parameters != null
                                           ? new BindingList<WaterQualityParameter>(library.Parameters)
                                           : null;
            tableViewActiveSubstances.Data = library.ActiveSubstances != null
                                                 ? new BindingList<WaterQualitySubstance>(
                                                     library.ActiveSubstances.ToList())
                                                 : null;
            tableViewInactiveSubstances.Data = library.InActiveSubstances != null
                                                   ? new BindingList<WaterQualitySubstance>(
                                                       library.InActiveSubstances.ToList())
                                                   : null;
            tableViewOutputParameters.Data = library.Parameters != null
                                                 ? new BindingList<WaterQualityOutputParameter>(
                                                     library.OutputParameters)
                                                 : null;

            tableViewProcesses.BestFitColumns();
            tableViewParameters.BestFitColumns();
            tableViewActiveSubstances.BestFitColumns();
            tableViewInactiveSubstances.BestFitColumns();
            tableViewOutputParameters.BestFitColumns();
        }

        private void InitializeTableViews()
        {
            InitializeTableViewProcesses();
            InitializeTableViewParameters();
            InitializeTableViewActiveSubstances();
            InitializeTableViewInactiveSubstances();
            InitializeTableViewOutputParameters();
        }

        private void InitializeTableViewProcesses()
        {
            tableViewProcesses.Columns.Clear();

            tableViewProcesses.AddColumn("Name", Resources.SubstanceProcessLibraryView_InitializeTableView_Name, true,
                                         100);
            tableViewProcesses.AddColumn("Description",
                                         Resources.SubstanceProcessLibraryView_InitializeTableView_Description, true,
                                         100);

            tableViewProcesses.BestFitColumns();
        }

        private void InitializeTableViewParameters()
        {
            tableViewParameters.Columns.Clear();

            tableViewParameters.AddColumn("Name", Resources.SubstanceProcessLibraryView_InitializeTableView_Name, true,
                                          100);
            tableViewParameters.AddColumn("Description",
                                          Resources.SubstanceProcessLibraryView_InitializeTableView_Description, true,
                                          100);

            if (!ShowNameAndDescriptionColumnsOnly)
            {
                tableViewParameters.AddColumn("Unit", Resources.SubstanceProcessLibraryView_InitializeTableView_Unit,
                                              true, 100);
                tableViewParameters.AddColumn("DefaultValue",
                                              Resources.SubstanceProcessLibraryView_InitializeTableView_Default_value,
                                              true, 100);
            }

            tableViewParameters.BestFitColumns();
        }

        private void InitializeTableViewActiveSubstances()
        {
            tableViewActiveSubstances.Columns.Clear();

            tableViewActiveSubstances.AddColumn("Name", Resources.SubstanceProcessLibraryView_InitializeTableView_Name,
                                                true, 100);
            tableViewActiveSubstances.AddColumn("Description",
                                                Resources.SubstanceProcessLibraryView_InitializeTableView_Description,
                                                true, 100);

            if (!ShowNameAndDescriptionColumnsOnly)
            {
                tableViewActiveSubstances.AddColumn("InitialValue",
                                                    Resources
                                                        .SubstanceProcessLibraryView_InitializeTableViewActiveSubstances_Default_value,
                                                    true, 100);
                tableViewActiveSubstances.AddColumn("ConcentrationUnit",
                                                    Resources
                                                        .SubstanceProcessLibraryView_InitializeTableView_Concentration_unit,
                                                    true, 100);
            }

            tableViewActiveSubstances.BestFitColumns();
        }

        private void InitializeTableViewInactiveSubstances()
        {
            tableViewInactiveSubstances.Columns.Clear();

            tableViewInactiveSubstances.AddColumn(
                "Name", Resources.SubstanceProcessLibraryView_InitializeTableView_Name, true, 100);
            tableViewInactiveSubstances.AddColumn("Description",
                                                  Resources.SubstanceProcessLibraryView_InitializeTableView_Description,
                                                  true, 100);

            if (!ShowNameAndDescriptionColumnsOnly)
            {
                tableViewInactiveSubstances.AddColumn("InitialValue",
                                                      Resources
                                                          .SubstanceProcessLibraryView_InitializeTableViewActiveSubstances_Default_value,
                                                      true, 100);
                tableViewInactiveSubstances.AddColumn("ConcentrationUnit",
                                                      Resources
                                                          .SubstanceProcessLibraryView_InitializeTableView_Concentration_unit,
                                                      true, 100);
            }

            tableViewInactiveSubstances.BestFitColumns();
        }

        private void InitializeTableViewOutputParameters()
        {
            tableViewOutputParameters.Columns.Clear();

            tableViewOutputParameters.AddColumn("Name", Resources.SubstanceProcessLibraryView_InitializeTableView_Name,
                                                true, 100);
            tableViewOutputParameters.AddColumn("Description",
                                                Resources.SubstanceProcessLibraryView_InitializeTableView_Description,
                                                true, 100);

            if (!ShowNameAndDescriptionColumnsOnly)
            {
                tableViewOutputParameters.AddColumn("ShowInMap",
                                                    Resources
                                                        .SubstanceProcessLibraryView_InitializeTableView_Show_in_Map,
                                                    false, 100);
                tableViewOutputParameters.AddColumn("ShowInHis",
                                                    Resources
                                                        .SubstanceProcessLibraryView_InitializeTableView_Show_in_His,
                                                    false, 100);
            }

            tableViewOutputParameters.BestFitColumns();
        }
    }
}