using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class BoundaryConditionEditor : UserControl, ICompositeView, IReusableView, ISuspendibleView
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BoundaryConditionEditor));
        private readonly IEventedList<IView> childViews;

        private BoundaryConditionFactory boundaryConditionFactory;
        private BoundaryConditionSet boundaryConditionSet;
        private IBoundaryCondition selectedBoundaryCondition;
        private string selectedCategory;
        private int selectedSupportPointIndex;
        private bool guiHandling;
        private BoundaryConditionEditorController controller;
        private BoundaryConditionPropertiesControl boundaryConditionPropertiesControl;
        private bool supportPointsListBoxUpdating;
        private BoundaryConditionSet cachedBoundaryConditionSet;
        private IBoundaryCondition cachedBoundaryCondition;
        private int cachedSelectedSupportPoint;
        private Control boundaryConditionDataView;

        private bool updatingVerticalProfile;
        public event EventHandler<EventArgs<int>> SelectedSupportPointChanged;

        public BoundaryConditionEditor()
        {
            InitializeComponent();
            childViews = new EventedList<IView>();

            categoryComboBox.SelectedValueChanged += SelectedCategoryChanged;

            conditionsListBox.SelectedIndexChanged += SelectedBoundaryConditionChanged;
            conditionsListBox.Format += ConditionsListBoxFormat;
            conditionsListBox.OnItemRemoved += SelectedBoundaryConditionRemoved;

            supportPointListBox.SelectedIndexChanged += OnSelectedSupportPointChanged;
            supportPointListBox.ItemCheck += SupportPointListBoxItemCheck;

            supportPointListBox.CheckOnClick = false;

            verticalProfileControl.AfterProfileDefinitionCreated = ExtractDepthLayerControlData;

            addDefinitionButton.Click += AddBoundaryConditionButtonClick;

            RefreshCategoryComboBox();
        }

        public bool ShowSupportPointChainages
        {
            private get
            {
                return showSupportPointChainages;
            }
            set
            {
                showSupportPointChainages = value;
                UpdateSupportPointLabels();
            }
        }

        public bool ShowSupportPointNames
        {
            private get
            {
                return showSupportPointNames;
            }
            set
            {
                showSupportPointNames = value;
                UpdateSupportPointLabels();
            }
        }

        public string SelectedCategory
        {
            get
            {
                return selectedCategory;
            }
            set
            {
                if (!guiHandling)
                {
                    categoryComboBox.SelectedItem = value;
                }

                if (selectedCategory == value)
                {
                    return;
                }

                selectedCategory = value;
                RefreshQuantitiesComboBox();
                RefreshBoundaryConditionsListBox();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedSupportPointIndex
        {
            get
            {
                return selectedSupportPointIndex;
            }
            private set
            {
                if (!guiHandling)
                {
                    supportPointListBox.SelectedIndex = value;
                }

                selectedSupportPointIndex = value;
                FireSelectedSupportPointChanged();
            }
        }

        public BoundaryConditionEditorController Controller
        {
            get
            {
                return controller;
            }
            set
            {
                controller = value;
                controller.Editor = this;
                controller.OnBoundaryConditionSelectionChanged(SelectedBoundaryCondition);

                RefreshAvailableCategories();

                // Enforce consistency:
                if (boundaryConditionPropertiesControl != null)
                {
                    boundaryConditionPropertiesControl.Controller = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BoundaryConditionPropertiesControl BoundaryConditionPropertiesControl
        {
            private get
            {
                return boundaryConditionPropertiesControl;
            }
            set
            {
                boundaryConditionPropertiesControl = value;
                if (boundaryConditionPropertiesControl != null)
                {
                    // Enforce consistency:
                    boundaryConditionPropertiesControl.Controller = controller;

                    boundaryConditionPropertiesControl.BoundaryCondition = SelectedBoundaryCondition;
                    boundaryConditionPropertiesControl.Visible = SelectedBoundaryCondition != null;

                    boundaryConditionPropertiesPanel.Controls.Clear();
                    boundaryConditionPropertiesControl.Dock = DockStyle.Fill;
                    boundaryConditionPropertiesPanel.Controls.Add(boundaryConditionPropertiesControl);
                }
            }
        }

        public bool DepthLayerControlVisible { private get; set; }

        public IBoundaryCondition SelectedBoundaryCondition
        {
            private get
            {
                return selectedBoundaryCondition;
            }
            set
            {
                if (!guiHandling)
                {
                    conditionsListBox.SelectedItem = value;
                }

                if (ReferenceEquals(selectedBoundaryCondition, value))
                {
                    return;
                }

                if (selectedBoundaryCondition != null)
                {
                    ((INotifyPropertyChange) selectedBoundaryCondition).PropertyChanged -= OnBoundaryConditionPropertyChanged;
                    selectedBoundaryCondition.DataPointIndices.CollectionChanged -= OnDataPointsChanged;
                }

                selectedBoundaryCondition = value;

                if (selectedBoundaryCondition != null)
                {
                    ((INotifyPropertyChange) selectedBoundaryCondition).PropertyChanged += OnBoundaryConditionPropertyChanged;
                    selectedBoundaryCondition.DataPointIndices.CollectionChanged += OnDataPointsChanged;
                }

                RefreshGeometryPanel();

                if (BoundaryConditionPropertiesControl != null)
                {
                    BoundaryConditionPropertiesControl.BoundaryCondition = selectedBoundaryCondition;
                    BoundaryConditionPropertiesControl.Visible = selectedBoundaryCondition != null;
                }

                if (Controller != null)
                {
                    Controller.OnBoundaryConditionSelectionChanged(selectedBoundaryCondition);
                }

                if (SelectedSupportPointIndex == -1 && BoundaryConditionSet != null) //occurs after delete
                {
                    SelectedSupportPointIndex = 0;
                }

                FillDepthLayerControl();
            }
        }

        public BoundaryConditionFactory BoundaryConditionFactory
        {
            private get
            {
                return boundaryConditionFactory;
            }
            set
            {
                boundaryConditionFactory = value;

                bool supportsMultiple = boundaryConditionFactory != null &&
                                        boundaryConditionFactory.SupportsMultipleConditionsPerSet;
                addDefinitionButton.Enabled = supportsMultiple;
                conditionsListBox.AllowItemDelete = supportsMultiple;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control BoundaryConditionDataView
        {
            get
            {
                return boundaryConditionDataView;
            }
            set
            {
                childViews.Clear();

                boundaryConditionDataView = value;
                value.Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Clear();
                splitContainer1.Panel2.Controls.Add(boundaryConditionDataView);

                var compositeView = boundaryConditionDataView as ICompositeView;
                if (compositeView != null)
                {
                    childViews.AddRange(compositeView.ChildViews);
                }
            }
        }

        public DepthLayerDefinition ModelDepthLayerDefinition
        {
            set
            {
                verticalProfileControl.ModelDepthLayerDefinition = value;
            }
        }

        public ToolStripItem[] SupportPointListBoxContextMenuItems
        {
            get
            {
                return supportPointListBox.ContextMenuItems;
            }
            set
            {
                supportPointListBox.ContextMenuItems = value;
            }
        }

        public BoundaryConditionSet BoundaryConditionSet
        {
            get
            {
                return boundaryConditionSet;
            }
            set
            {
                UnSubscribeListBoxes();
                boundaryConditionSet = value;
                SubscribeListBoxes();

                if (boundaryConditionSet != null && boundaryConditionSet.Feature.Geometry.Coordinates.Any())
                {
                    selectedSupportPointIndex = 0;
                }
                else
                {
                    selectedSupportPointIndex = -1;
                }

                boundaryGeometryPreview.Feature = boundaryConditionSet == null
                                                      ? null
                                                      : boundaryConditionSet.Feature;
                boundaryGeometryPreview.FeatureGeometry = boundaryConditionSet == null
                                                              ? null
                                                              : boundaryConditionSet.Feature.Geometry;

                RefreshSupportPointsListBox();
                RefreshBoundaryConditionsListBox();

                SelectedBoundaryCondition = boundaryConditionSet == null
                                                ? null
                                                : boundaryConditionSet.BoundaryConditions.FirstOrDefault();

                UpdateGeometryPanel();

                RefreshQuantitiesComboBox();

                SelectedSupportPointIndex = selectedSupportPointIndex;
            }
        }

        public IEnumerable<VerticalProfileType> SupportedVerticalProfileTypes
        {
            set
            {
                verticalProfileControl.SetSupportedProfileTypes(value);
            }
        }

        [InvokeRequired]
        public void UpdateGeometryPanel()
        {
            bool horizontallyUniform = SelectedBoundaryCondition == null || SelectedBoundaryCondition.IsHorizontallyUniform;
            bool verticallyUniform = SelectedBoundaryCondition == null || SelectedBoundaryCondition.IsVerticallyUniform ||
                                     !DepthLayerControlVisible;

            tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, horizontallyUniform ? 0.0f : 33.33f);
            tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, verticallyUniform ? 0.0f : 33.33f);
            UpdateSupportPointsListBox();
            UpdateGeometryPreview();
        }

        public void RefreshAvailableCategories()
        {
            RefreshCategoryComboBox();
            RefreshQuantitiesComboBox();
        }

        public void RefreshQuantitiesComboBox()
        {
            quantitiesComboBox.Items.Clear();

            if (BoundaryConditionFactory == null)
            {
                return;
            }

            quantitiesComboBox.Items.AddRange(
                controller.GetAllowedVariablesFor(SelectedCategory, BoundaryConditionSet)
                          .Select(v => controller.GetVariableDescription(v, SelectedCategory))
                          .Cast<object>()
                          .ToArray());

            quantitiesComboBox.SelectedIndex = quantitiesComboBox.Items.Count != 0 ? 0 : -1;
            quantitiesComboBox.Enabled = quantitiesComboBox.Items.Count != 0;
        }

        public void UpdateSupportPointLabel(int j)
        {
            if (BoundaryConditionSet == null)
            {
                return;
            }

            var chainage = 0.0;
            if (ShowSupportPointChainages)
            {
                Coordinate[] coordinates = BoundaryConditionSet.Feature.Geometry.Coordinates;
                for (var i = 1; i < j; ++i)
                {
                    chainage += coordinates[i].Distance(coordinates[i - 1]);
                }
            }

            string format = ShowSupportPointChainages ? "{0}: {1,10}" : "{0}";
            supportPointListBox.Items[j] = string.Format(format, SupportPointLabel(j), chainage.ToString("F2"));
        }

        public void SuspendUpdates()
        {
            cachedBoundaryConditionSet = boundaryConditionSet;
            cachedBoundaryCondition = SelectedBoundaryCondition;
            cachedSelectedSupportPoint = SelectedSupportPointIndex;
            Data = null;

            updatesSuspended = true;
        }

        public void ResumeUpdates()
        {
            if (!updatesSuspended)
            {
                return;
            }
            
            Data = cachedBoundaryConditionSet;

            IBoundaryCondition boundaryCondition =
                boundaryConditionSet.BoundaryConditions.Contains(cachedBoundaryCondition)
                    ? cachedBoundaryCondition
                    : boundaryConditionSet.BoundaryConditions.FirstOrDefault();

            if (boundaryCondition != null)
            {
                SelectedCategory = boundaryCondition.ProcessName;
                SelectedBoundaryCondition = boundaryCondition;
            }

            SelectedSupportPointIndex = cachedSelectedSupportPoint <
                                        boundaryConditionSet.Feature.Geometry.Coordinates.Count()
                                            ? cachedSelectedSupportPoint
                                            : 0;

            updatesSuspended = false;
        }

        private IEnumerable<string> Categories
        {
            get
            {
                return controller != null
                           ? controller.SupportedProcessNames
                           : new List<string>();
            }
        }

        private void RefreshSupportPointsListBox()
        {
            supportPointListBox.Items.Clear();
            if (BoundaryConditionSet == null)
            {
                return;
            }

            List<Coordinate> coordinates = ((IFeature) boundaryConditionSet.Feature).Geometry.Coordinates.ToList();
            if (coordinates.Count == 0)
            {
                return;
            }

            var i = 0;
            var chainage = 0.0;
            string format = ShowSupportPointChainages ? "{0}: {1,10}" : "{0}";
            supportPointListBox.Items.Add(string.Format(format, SupportPointLabel(i), chainage.ToString("F2")));
            for (i = 1; i < coordinates.Count; ++i)
            {
                chainage += coordinates[i].Distance(coordinates[i - 1]);
                supportPointListBox.Items.Add(string.Format(format, SupportPointLabel(i), chainage.ToString("F2")));
            }
        }

        private string SupportPointLabel(int i)
        {
            return ShowSupportPointNames ? BoundaryConditionSet.SupportPointNames[i] : (i + 1).ToString();
        }

        private void RefreshCategoryComboBox()
        {
            categoryComboBox.Items.Clear();
            categoryComboBox.Items.AddRange(Categories.OfType<object>().ToArray());
            categoryComboBox.SelectedIndex = categoryComboBox.Items.Count == 0 ? -1 : 0;
        }

        private void RefreshBoundaryConditionsListBox()
        {
            var definitions = new List<object>();

            if (Data != null && categoryComboBox.SelectedIndex != -1)
            {
                definitions =
                    boundaryConditionSet.BoundaryConditions.Where(
                        bc => bc.ProcessName == (string) categoryComboBox.SelectedItem).OfType<object>().ToList();
            }

            conditionsListBox.Items.Clear();
            conditionsListBox.Items.AddRange(definitions.ToArray());
            if (conditionsListBox.Items.Count != 0)
            {
                conditionsListBox.SelectedIndex = 0;
            }
            else // force bc refresh
            {
                SelectedBoundaryCondition = null;
            }
        }

        private void RefreshGeometryPanel()
        {
            boundaryGeometryPreview.Feature = boundaryConditionSet == null
                                                  ? null
                                                  : boundaryConditionSet.Feature;
            boundaryGeometryPreview.FeatureGeometry = BoundaryConditionSet == null
                                                          ? null
                                                          : BoundaryConditionSet.Feature.Geometry;
            if (SelectedBoundaryCondition == null)
            {
                boundaryGeometryPreview.DataPoints = null;
            }
            else
            {
                boundaryGeometryPreview.DataPoints = SelectedBoundaryCondition.IsHorizontallyUniform
                                                         ? new EventedList<int>()
                                                         : SelectedBoundaryCondition.DataPointIndices;
            }

            UpdateGeometryPanel();
        }

        private void UpdateGeometryPreview()
        {
            if (BoundaryConditionSet == null || SelectedBoundaryCondition == null || SelectedSupportPointIndex == -1)
            {
                boundaryGeometryPreview.SelectedPoints = new List<int>();
            }

            else if (SelectedBoundaryCondition.IsHorizontallyUniform)
            {
                boundaryGeometryPreview.SelectedPoints = new[]
                {
                    0
                };
            }
            else
            {
                boundaryGeometryPreview.SelectedPoints = new[]
                {
                    SelectedSupportPointIndex
                };
            }
        }

        private void UpdateSupportPointsListBox()
        {
            if (supportPointsListBoxUpdating || Data == null)
            {
                return;
            }

            supportPointsListBoxUpdating = true;
            UpdateSupportPointLabels();
            for (var i = 0; i < supportPointListBox.Items.Count; i++)
            {
                supportPointListBox.SetItemChecked(i, false);
            }

            if (SelectedBoundaryCondition == null)
            {
                supportPointListBox.SelectedIndex = -1;
                supportPointListBox.Enabled = false;
                supportPointsListBoxUpdating = false;
                return;
            }

            supportPointListBox.Enabled = true;

            foreach (int i in SelectedBoundaryCondition.DataPointIndices)
            {
                supportPointListBox.SetItemChecked(i, true);
            }

            supportPointsListBoxUpdating = false;
        }

        private void FillDepthLayerControl()
        {
            if (updatingVerticalProfile)
            {
                return;
            }

            verticalProfileControl.VerticalProfileDefinition = SelectedBoundaryCondition?.GetDepthLayerDefinitionAtPoint(
                SelectedSupportPointIndex);
        }

        private void ExtractDepthLayerControlData(VerticalProfileDefinition depthLayerDefinition)
        {
            if (SelectedBoundaryCondition == null)
            {
                return;
            }

            int index = SelectedBoundaryCondition.DataPointIndices.IndexOf(SelectedSupportPointIndex);

            if (index == -1)
            {
                return;
            }

            updatingVerticalProfile = true;

            SelectedBoundaryCondition.PointDepthLayerDefinitions[index] = depthLayerDefinition;

            updatingVerticalProfile = false;
        }

        private void UnSubscribeListBoxes()
        {
            if (boundaryConditionSet != null)
            {
                boundaryConditionSet.BoundaryConditions.CollectionChanged -= BoundaryConditionsCollectionChanged;
                ((INotifyPropertyChanged) boundaryConditionSet.Feature).PropertyChanged -= BoundaryPropertyChanged;
            }
        }

        private void SubscribeListBoxes()
        {
            if (boundaryConditionSet != null)
            {
                boundaryConditionSet.BoundaryConditions.CollectionChanged += BoundaryConditionsCollectionChanged;
                ((INotifyPropertyChanged) boundaryConditionSet.Feature).PropertyChanged += BoundaryPropertyChanged;
            }
        }

        private string GenerateUniqueName(IBoundaryCondition newCondition)
        {
            List<string> names = boundaryConditionSet.BoundaryConditions.Select(bc => bc.Name).ToList();
            string newName = newCondition.Name;
            var i = 2;
            while (names.Contains(newName))
            {
                newName = newCondition.Name + "(" + i + ")";
                i++;
            }

            return newName;
        }

        private void AddBoundaryConditionButtonClick(object sender, EventArgs e)
        {
            int index = quantitiesComboBox.SelectedIndex;
            List<string> quantities = controller.GetAllowedVariablesFor(SelectedCategory, BoundaryConditionSet).ToList();
            if (index < 0 || index >= quantities.Count)
            {
                return;
            }

            string quantity = quantities.ElementAt(index);
            if (BoundaryConditionFactory != null)
            {
                List<BoundaryConditionDataType> dataTypes = controller.GetSupportedDataTypesForVariable(quantity).ToList();
                if (dataTypes.Any())
                {
                    IBoundaryCondition newCondition = BoundaryConditionFactory.CreateBoundaryCondition(boundaryConditionSet.Feature,
                                                                                                       quantity,
                                                                                                       dataTypes.First(),
                                                                                                       SelectedCategory);
                    if (newCondition == null)
                    {
                        Log.ErrorFormat("Could not create boundary condition of quantity type {0}", quantity);
                        return;
                    }

                    newCondition.Name = GenerateUniqueName(newCondition);
                    Controller.InsertBoundaryCondition(boundaryConditionSet, newCondition);
                    conditionsListBox.SelectedItem = newCondition;
                }
            }
        }

        private void UpdateSupportPointLabels()
        {
            if (BoundaryConditionSet == null)
            {
                return;
            }

            if (ShowSupportPointChainages)
            {
                int i;
                var chainage = 0.0;
                string format = ShowSupportPointChainages ? "{0}: {1,10}" : "{0}";
                Coordinate[] coordinates = BoundaryConditionSet.Feature.Geometry.Coordinates;
                supportPointListBox.Items[0] = string.Format(format, SupportPointLabel(0), chainage.ToString("F2"));
                for (i = 1; i < coordinates.Length; ++i)
                {
                    chainage += coordinates[i].Distance(coordinates[i - 1]);
                    supportPointListBox.Items[i] = string.Format(format, SupportPointLabel(i), chainage.ToString("F2"));
                }
            }
            else
            {
                for (var i = 0; i < BoundaryConditionSet.Feature.Geometry.Coordinates.Length; ++i)
                {
                    supportPointListBox.Items[i] = SupportPointLabel(i);
                }
            }
        }

        #region IView implementation

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Data
        {
            get
            {
                return BoundaryConditionSet;
            }
            set
            {
                BoundaryConditionSet = (BoundaryConditionSet) value;
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        # region event handlers

        private void SelectedCategoryChanged(object sender, EventArgs e)
        {
            guiHandling = true;
            SelectedCategory = categoryComboBox.SelectedItem as string;
            guiHandling = false;
        }

        private void SelectedBoundaryConditionChanged(object sender, EventArgs e)
        {
            guiHandling = true;
            SelectedBoundaryCondition = conditionsListBox.SelectedItem as IBoundaryCondition;
            guiHandling = false;
        }

        private void OnSelectedSupportPointChanged(object sender, EventArgs e)
        {
            guiHandling = true;
            SelectedSupportPointIndex = supportPointListBox.SelectedIndex;
            guiHandling = false;
        }

        private void OnBoundaryConditionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var bc = sender as BoundaryCondition;
            if (bc == null)
            {
                return;
            }

            if (bc.IsEditing)
            {
                return;
            }

            if (!geometryPanelRefreshRequired && e.PropertyName.Equals(nameof(bc.IsEditing)))
            {
                return;
            }

            RefreshSupportPointsListBox();
            if (selectedSupportPointIndex < BoundaryConditionSet.Feature.Geometry.Coordinates.Count())
            {
                SelectedSupportPointIndex = selectedSupportPointIndex;
            }
            else
            {
                SelectedSupportPointIndex = -1;
            }

            RefreshGeometryPanel();
            geometryPanelRefreshRequired = false;
        }

        private void BoundaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BoundaryConditionSet.Feature.Name))
            {
                RefreshSupportPointsListBox();
                supportPointListBox.SelectedIndex = SelectedSupportPointIndex;
                UpdateSupportPointsListBox();
                return;
            }

            if (e.PropertyName == nameof(BoundaryConditionSet.Feature.Geometry))
            {
                var pointCountChanged = false;
                if (boundaryConditionSet.Feature.Geometry.Coordinates.Count() != supportPointListBox.Items.Count)
                {
                    pointCountChanged = true;
                    RefreshSupportPointsListBox();
                }

                if (pointCountChanged)
                {
                    if (selectedSupportPointIndex < supportPointListBox.Items.Count)
                    {
                        SelectedSupportPointIndex = selectedSupportPointIndex;
                    }
                    else
                    {
                        SelectedSupportPointIndex = -1;
                    }
                }

                // only call RefreshGeometryPanel when there is no data on the boundary
                // because this will be called via OnBoundaryConditionPropertyChanged
                if (SelectedBoundaryCondition == null)
                {
                    RefreshGeometryPanel();
                }
            }
        }

        private bool geometryPanelRefreshRequired;

        private void OnDataPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedBoundaryCondition == null)
            {
                return;
            }

            if (SelectedBoundaryCondition.IsEditing)
            {
                geometryPanelRefreshRequired = true;
                return;
            }

            UpdateSupportPointsListBox();
        }

        private void BoundaryConditionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            var boundaryConditionData = removedOrAddedItem as IBoundaryCondition;

            if (boundaryConditionData == null ||
                boundaryConditionData.ProcessName != (string) categoryComboBox.SelectedItem)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                conditionsListBox.Items.Add(removedOrAddedItem);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                conditionsListBox.Items.Remove(removedOrAddedItem);
            }

            RefreshQuantitiesComboBox();
        }

        private void ConditionsListBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            var item = e.ListItem as IBoundaryCondition;
            if (item == null)
            {
                return;
            }

            e.Value = item.Name;
        }

        private void SupportPointListBoxItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (supportPointsListBoxUpdating || e.CurrentValue == e.NewValue) // double-click to fire event
            {
                return;
            }

            guiHandling = true;
            if (e.NewValue == CheckState.Unchecked)
            {
                SelectedBoundaryCondition.RemovePoint(e.Index);
            }
            else
            {
                SelectedBoundaryCondition.AddPoint(e.Index);
            }

            if (SelectedSupportPointChanged != null)
            {
                SelectedSupportPointChanged(SelectedBoundaryCondition, new EventArgs<int>(SelectedSupportPointIndex));
                FillDepthLayerControl();
            }

            UpdateGeometryPanel();
            guiHandling = false;
        }

        private void FireSelectedSupportPointChanged()
        {
            if (SelectedSupportPointChanged != null)
            {
                SelectedSupportPointChanged(SelectedBoundaryCondition, new EventArgs<int>(SelectedSupportPointIndex));
            }

            FillDepthLayerControl();
            UpdateGeometryPreview();
        }

        private void SelectedBoundaryConditionRemoved(object sender, ListBoxItemRemovedEventArgs e)
        {
            var boundaryCondition = e.Value as IBoundaryCondition;
            int removedIndex = e.Index;
            if (boundaryCondition != null)
            {
                var condition = boundaryCondition as BoundaryCondition;
                if (condition != null)
                {
                    condition.Feature = null; // unsubscribe to geometry changes
                }

                boundaryConditionSet.BoundaryConditions.Remove(boundaryCondition);
            }

            if (removedIndex < conditionsListBox.Items.Count)
            {
                conditionsListBox.SelectedIndex = removedIndex;
            }
            else if (removedIndex == conditionsListBox.Items.Count)
            {
                conditionsListBox.SelectedIndex = removedIndex - 1;
            }
        }

        #endregion

        #region ICompositeView

        public IEventedList<IView> ChildViews
        {
            get
            {
                return childViews;
            }
        }

        public bool HandlesChildViews
        {
            get
            {
                return true;
            }
        }

        public void ActivateChildView(IView childView)
        {
            // Nothing to be done, enforced through ICompositeView
        }

        #endregion

        #region IReusableView

        private bool locked;
        private bool showSupportPointChainages;
        private bool showSupportPointNames;
        private bool updatesSuspended;

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

        public event EventHandler LockedChanged;

        #endregion
    }
}