using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class CrossSectionDefinitionView : UserControl,IReusableView, ICrossSectionHistoryCapableView
    {
        private readonly ILog Log = LogManager.GetLogger(typeof (CrossSectionDefinitionView));
        private static readonly Bitmap XYZImage = Properties.Resources.CrossSectionSmallXYZ;
        private static readonly Bitmap YZImage = Properties.Resources.CrossSectionSmall;
        private static readonly Bitmap ZWImage = Properties.Resources.CrossSectionTabulatedSmall;

        private ICrossSectionDefinition crossSectionDefinition;

        private SectionsBindingList crossSectionSections;
        private ZWSectionsViewModel crossSectionZWSectionsViewModel;

        private bool locked;
        private bool editingSelection;
        private CrossSectionDefinitionViewModel viewModel;

        public CrossSectionDefinitionView()
        {
            InitializeComponent();
            Text = "Cross Section View";
            
            InitializeTableView();
            InitializeChartView();
            InitializeRoughnessSectionsView();
        }

        public bool HistoryToolEnabled
        {
            get { return crossSectionChart.HistoryToolEnabled; }
            set { crossSectionChart.HistoryToolEnabled = value; }
        }

        private ICrossSectionDefinition CrossSectionDefinition
        {
            get { return crossSectionDefinition; }
            set
            {
                Cleanup();

                crossSectionDefinition = value;

                Setup();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CrossSectionDefinitionViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                Cleanup();

                viewModel = value;

                Setup();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Data
        {
            get { return CrossSectionDefinition; }
            set
            {
                CrossSectionDefinition = (ICrossSectionDefinition) value;

                if (CrossSectionDefinition == null) return;
                
                Enabled = !CrossSectionDefinition.IsProxy;
            }
        }

        private SectionsBindingList CrossSectionSections
        {
            get { return crossSectionSections; }
            set
            {
                if (crossSectionSections != null)
                {
                    crossSectionSections.ListChanged -= SectionsListChanged;
                }

                crossSectionSections = value;

                if (crossSectionSections != null)
                {
                    crossSectionSections.ListChanged += SectionsListChanged;
                }
            }
        }

        #region Subscribe

        private void Setup()
        {
            if (CrossSectionDefinition == null || ViewModel == null)
            {
                return;
            }

            Text = CrossSectionDefinition.Name;
            SubscribeToData();

            CrossSectionSections = new SectionsBindingList(SynchronizationContext.Current, CrossSectionDefinition.Sections);

            SetupChartView();
            SetupRoughnessSectionsView();
            SetupSummerDikeView();
            
            SetupCrossSectionDataView();
            tableView.PasteController.SelectPasteReplacer.ReplaceVerifier = CrossSectionDefinition.GenerateReplaceVerifier(ViewModel.MinimalNumberOfTableRows);
            tableView.PasteController.PasteFinished += PasteControllerOnPasteFinished;
        }

        private void PasteControllerOnPasteFinished(object sender, EventArgs e)
        {
            CrossSectionDefinition.FinishPasteHandling();
        }
        

        private void SubscribeToData()
        {
            if (CrossSectionDefinition != null)
            {
                CrossSectionDefinition.ForceSectionsSpanFullWidth = true;
                ((INotifyPropertyChanged)CrossSectionDefinition).PropertyChanged += LevelShiftChanged;
                var innerDefinition = CrossSectionDefinition;
                if (innerDefinition is CrossSectionDefinitionProxy)
                {
                    innerDefinition = ((CrossSectionDefinitionProxy)CrossSectionDefinition).InnerDefinition;
                }
                if (innerDefinition is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged)innerDefinition).PropertyChanged += CrossSectionDefinitionPropertyChanged;
                    ((INotifyPropertyChanged)innerDefinition).PropertyChanged += CrossSectionDefinitionDataChanged;
                    ((INotifyPropertyChanged)innerDefinition.Sections).PropertyChanged +=
                        CrossSectionSectionsPropertyChanged;
                }
                innerDefinition.Sections.CollectionChanged += CrossSectionSectionsCollectionChanged;
            }
        }

        private void UnsubscribeToData()
        {
            if (CrossSectionDefinition != null)
            {
                CrossSectionDefinition.ForceSectionsSpanFullWidth = false;
                ((INotifyPropertyChanged)CrossSectionDefinition).PropertyChanged -= LevelShiftChanged;
                var innerDefinition = CrossSectionDefinition;

                if(innerDefinition is CrossSectionDefinitionProxy)
                {
                    innerDefinition = ((CrossSectionDefinitionProxy) CrossSectionDefinition).InnerDefinition;
                }

                if (innerDefinition is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged) innerDefinition).PropertyChanged -= CrossSectionDefinitionPropertyChanged;
                    ((INotifyPropertyChanged) innerDefinition).PropertyChanged -= CrossSectionDefinitionDataChanged;
                    ((INotifyPropertyChanged) innerDefinition.Sections).PropertyChanged -=
                        CrossSectionSectionsPropertyChanged;
                }

                innerDefinition.Sections.CollectionChanged -= CrossSectionSectionsCollectionChanged;
            }
        }

        private void CrossSectionDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" && CrossSectionDefinition != null)
            {
                Text = CrossSectionDefinition.Name;
                crossSectionChart.RefreshChartTitle();
            }
        }

        private void LevelShiftChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LevelShift")
            {
                crossSectionChart.RefreshChart();
            }
        }

        void CrossSectionSectionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshView();
        }

        private void Cleanup()
        {
            CleanUpSectionsViewModel();
            UnsubscribeToData();

            CrossSectionSections = null; //unsubscribe

            crossSectionChart.SetData(null, null,null);
            crossSectionViewZwSectionsView1.Data = null;
            crossSectionSectionsTable.Data = null;
            summerDikeView.Data = null;

            tableView.PasteController.SelectPasteReplacer.ReplaceVerifier = null;
            tableView.PasteController.PasteFinished -= PasteControllerOnPasteFinished;

            tableView.Data = null;
            tableView.EditableObject = null;
        }

        private void CleanUpSectionsViewModel()
        {
            if (crossSectionZWSectionsViewModel != null)
            {
                crossSectionZWSectionsViewModel.Dispose();
                crossSectionZWSectionsViewModel = null;
            }
        }
        
        private void SetupChartView()
        {
            crossSectionChart.SetData(CrossSectionDefinition, CrossSectionSections,ViewModel);
        }

        private void SetupCrossSectionDataView()
        {
            if (CrossSectionDefinition.CrossSectionType == CrossSectionType.Standard)
            {
                splitContainerCrossSectionData.Panel1Collapsed = true;
                splitContainerCrossSectionData.Panel2Collapsed = false;

                //use the inner definition if it is a proxy
                var standardDefinition = CrossSectionDefinition.IsProxy
                                             ? ((CrossSectionDefinitionProxy)
                                                CrossSectionDefinition).InnerDefinition
                                             : CrossSectionDefinition;
                
                crossSectionStandardDataView1.Data = new CrossSectionDefinitionStandardViewModel{ Definition = (CrossSectionDefinitionStandard)standardDefinition, IsOnChannel = ViewModel.IsCurrentlyOnChannel};
                
            }
            else
            {
                splitContainerCrossSectionData.Panel1Collapsed = false;
                splitContainerCrossSectionData.Panel2Collapsed = true;

                tableView.Data = CrossSectionDefinition.RawData;
                tableView.EditableObject = CrossSectionDefinition;
                tableView.BestFitColumns();
                tableGroupBox.Text = ViewModel.TableDescription;
            }
        }

        private void SetupRoughnessSectionsView()
        {
            var isCrossSectionZW = CrossSectionDefinition.CrossSectionType == CrossSectionType.ZW;
            splitContainerSectionViews.Panel2Collapsed = !isCrossSectionZW;
            splitContainerSectionViews.Panel1Collapsed = isCrossSectionZW;
            if (isCrossSectionZW)
            {
                SetupSectionsViewForZW();
            }
            else
            {
                SetupSectionsViewForNonZW();
            }
            
        }

        private void SetupSectionsViewForZW()
        {
            crossSectionZWSectionsViewModel = new ZWSectionsViewModel(CrossSectionDefinition,ViewModel.CrossSectionSectionTypes);
            crossSectionViewZwSectionsView1.Data = crossSectionZWSectionsViewModel;
        }

        private void SetupSectionsViewForNonZW()
        {
            crossSectionSectionsTable.MaximumNumberOfRoughnessSections = ViewModel.MaxSections;
            crossSectionSectionsTable.Data = CrossSectionSections;
            crossSectionSectionsTable.SectionTypeList = ViewModel.CrossSectionSectionTypes;
            
            SetSectionsMinMax();
        }

        private void SetupSummerDikeView()
        {
            //potential summerdike
            var summerDikeDefinition = CrossSectionDefinition as ISummerDikeEnabledDefinition;
            if ((summerDikeDefinition != null) && (summerDikeDefinition.CanHaveSummerDike))
            {
                summerDikeView.Data = summerDikeDefinition.SummerDike;
                summerDikeView.Visible = true;
                leftSplitContainer.Panel2Collapsed = false;
                leftSplitContainer.SplitterDistance = leftSplitContainer.Height - summerDikeView.Height;    
            }
            else
            {
                leftSplitContainer.Panel2Collapsed = true;
            }
        }

        #endregion

        #region Initialize

        private void InitializeChartView()
        {
            crossSectionChart.ChartView.SelectionPointChanged += (s, e) => SynchronizeSelection(true, crossSectionChart.ChartView.SelectedPointIndex);
            crossSectionChart.SectionSelectionChanged += (s, e) => SynchronizeSectionSelection(true, s as CrossSectionSection);
            crossSectionChart.StatusMessage += (s, e) => { if (StatusMessage == null) return; StatusMessage(s, e); };
        }

        private void InitializeTableView()
        {
            tableView.AllowColumnSorting = false;
            tableView.PasteController = new TableViewArgumentBasedPasteController(tableView, new List<int> {0});
            tableView.SelectionChanged += (s, e) => SynchronizeSelection(false, e.Cells.Count > 0 ? e.Cells.First().RowIndex : -1);
            tableView.CellChanged += (s, e) => RefreshView();
            tableView.InputValidator += InputValidator;
            tableView.CanDeleteCurrentSelection += CanDeleteCurrentSelection;
        }

        private bool CanDeleteCurrentSelection()
        {
            // Determine if a block selection was done across whole rows, 
            // by ensuring the number of selected cells per row is equal to the number of visible columns.
            var doingRowSelection = tableView.SelectedCells.GroupBy(c => c.RowIndex)
                                                           .All(g => g.Count() == tableView.Columns.Count(c => c.Visible));
            if (doingRowSelection &&
                crossSectionDefinition.RawData.Rows.Count - tableView.SelectedRowsIndices.Count() < ViewModel.MinimalNumberOfTableRows)
            {
                Log.ErrorFormat("Cannot delete the selected rows, as a minimum number of rows equal to {0} is required for {1}.", 
                    ViewModel.MinimalNumberOfTableRows, ViewModel.TableDescription);
                return false;
            }
            return true;
        }

        private DelftTools.Utils.Tuple<string, bool> InputValidator(TableViewCell tableViewCell, object o)
        {
            var sortedRowIndex = tableView.GetDataSourceIndexByRowIndex(tableViewCell.RowIndex);

            if (sortedRowIndex < 0 || sortedRowIndex >= tableView.RowCount)
                return new DelftTools.Utils.Tuple<string, bool>("", true);

            return crossSectionDefinition.ValidateCellValue(sortedRowIndex, tableViewCell.Column.AbsoluteIndex, o);
        }

        private void InitializeRoughnessSectionsView()
        {
            crossSectionSectionsTable.SelectionChanged += (s, e) => SynchronizeSectionSelection(false, s as CrossSectionSection);
        }

        #endregion

        #region Changed Events

        void CrossSectionSectionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshView();
        }

        private void CrossSectionDefinitionDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditing" && sender == CrossSectionDefinition && CrossSectionDefinition.IsEditing)
            {
                return;
            }

            RefreshView();
        }

        private void RefreshView()
        {
            UnsubscribeToData();

            SetSectionsMinMax();
            crossSectionChart.RefreshChart();
            crossSectionStandardDataView1.RefreshView();
            if (crossSectionZWSectionsViewModel != null)
            {
                crossSectionZWSectionsViewModel.UpdateViewModelFromCrossSection(true);
            }
            SubscribeToData();
        }

        private void SectionsListChanged(object sender, ListChangedEventArgs e)
        {
            crossSectionChart.RefreshChart();
        }

        #endregion

        #region Synchronization
        
        private ChartRectangle GetProfileRectangle()
        {
            var profile = CrossSectionDefinition.GetProfile().ToList();

            if (profile.Any())
            {
                double minX = profile.Min(c => c.X);
                double maxX = profile.Max(c => c.X);

                var flowProfile = CrossSectionDefinition.FlowProfile.ToList();
                double minY = Math.Min(profile.Min(c => c.Y), flowProfile.Min(c => c.Y));
                double maxY = Math.Max(profile.Max(c => c.Y), flowProfile.Max(c => c.Y));

                return new ChartRectangle(minX, maxX, minY, maxY);
            }

            return new ChartRectangle(0, 0, 0, 0);
        }
        
        private void SetSectionsMinMax()
        {
            var profileRectangle = GetProfileRectangle();
            crossSectionSectionsTable.MinY = ViewModel.IsSymmetrical ? 0.0 : profileRectangle.Left;
            crossSectionSectionsTable.MaxY = profileRectangle.Right;
        }
        private void SynchronizeSelection(bool chartInitiated, int index)
        {
            if (editingSelection)
            {
                return;
            }

            editingSelection = true;
            if (chartInitiated)
            {
                tableView.ClearSelection();

                //set the table row for table based definitions
                if ((index != -1) && CrossSectionDefinition.RawData != null)
                {
                    var displayRowIndex = tableView.GetRowIndexByDataSourceIndex(((CrossSectionDefinition)CrossSectionDefinition).GetRawDataTableIndex(index));
                    tableView.SelectRow(displayRowIndex);
                    tableView.FocusedRowIndex = displayRowIndex;
                }
            }
            else
            {
                crossSectionChart.SelectIndex(index);
            }
            editingSelection = false;
        }

        private void SynchronizeSectionSelection(bool chartInitiated, CrossSectionSection section)
        {
            
            if ((editingSelection) || (CrossSectionDefinition is CrossSectionDefinitionZW))
            {
                return;
            }

            editingSelection = true;
            if (chartInitiated)
            {
                crossSectionChart.SelectSection(section);
                crossSectionSectionsTable.Select(section);
            }
            else
            {
                crossSectionChart.SelectSection(section);
            }
            editingSelection = false;
        }

        #endregion

        public event EventHandler StatusMessage;

        #region View

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        public Image Image
        {
            get
            {
                if (Data != null)
                {
                    if (CrossSectionDefinition.CrossSectionType == CrossSectionType.GeometryBased)
                        return XYZImage;
                    if (CrossSectionDefinition.CrossSectionType == CrossSectionType.YZ)
                        return YZImage;
                    if (CrossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
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

        #endregion
    }

        /// <summary>
    /// big ugly hack..partially for undo redo. Why ThreadsafeBindingList got involved I have no idea, but I don't want to worry about it either
    /// </summary>
    public class SectionsBindingList : ThreadsafeBindingList<CrossSectionSection>
    {
        public Action<CrossSectionSection> BeforeAddItem { get; set; }

        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            base.OnAddingNew(e);
            if (BeforeAddItem != null)
            {
                e.NewObject = new CrossSectionSection();
                BeforeAddItem((CrossSectionSection)e.NewObject);
            }
        }

        public SectionsBindingList(SynchronizationContext context)
            : base(context)
        {
        }

        public SectionsBindingList(SynchronizationContext context, IList<CrossSectionSection> list)
            : base(context, list)
        {
        }

        public SectionsBindingList(IList<CrossSectionSection> list) : base(SynchronizationContext.Current, list)
        {
        }
    }
}
