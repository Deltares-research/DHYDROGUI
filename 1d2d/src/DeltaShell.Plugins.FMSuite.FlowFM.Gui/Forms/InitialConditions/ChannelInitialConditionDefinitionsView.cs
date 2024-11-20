using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.InitialConditions
{
    public partial class ChannelInitialConditionDefinitionsView : UserControl, ILayerEditorView, ISuspendibleView
    {
        private readonly VectorLayerAttributeTableView vectorLayerAttributeTableView;

        private readonly DelayedEventHandler<EventArgs> delayedEventHandlerDefinitionsCollectionChanged;

        private const int ChannelColumnIndex = 0;
        private const int SpecificationColumnIndex = 1;
        private const int ValueColumnIndex = 2;
        private const int ButtonColumnIndex = 3;

        private ILayer data;
        private static WaterFlowFMModel waterFlowFmModel;
        private Action openGlobalInitialConditionSettings;
        public event EventHandler SelectedFeaturesChanged;
        private static IDictionary<InitialConditionQuantity, IEnumerable<ChannelInitialConditionDefinition>> initialConditionValuesByQuantity = new Dictionary<InitialConditionQuantity, IEnumerable<ChannelInitialConditionDefinition>>();
        private InitialConditionQuantity currentQuantity;

        public ChannelInitialConditionDefinitionsView()
        {
            InitializeComponent();

            vectorLayerAttributeTableView = new VectorLayerAttributeTableView
            {
                Dock = DockStyle.Fill,
                CanAddDeleteAttributes = false,
                DeleteSelectedFeatures = () => { },
                OpenViewMethod = feature => DoEditFunction(),
                TableView =
                {
                    AutoGenerateColumns = false
                }
            };

            vectorLayerAttributeTableView.SelectedFeaturesChanged += OnSelectedFeaturesChanged;
            
            Controls.Add(vectorLayerAttributeTableView);

            SubscribeViewEvents();

            delayedEventHandlerDefinitionsCollectionChanged =
                new DelayedEventHandler<EventArgs>(ChannelInitialConditionDefinitionsCollectionChanged)
                {
                    FireLastEventOnly = true,
                    Delay = 500,
                    SynchronizingObject = this
                };
        }

        public object Data
        {
            get => data;
            set
            {
                data = value as ILayer;

                vectorLayerAttributeTableView.Data = data;

                UpdateTableView();
            }
        }

        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public IEnumerable<IFeature> SelectedFeatures
        {
            get => vectorLayerAttributeTableView.SelectedFeatures;
            set => vectorLayerAttributeTableView.SelectedFeatures = value;
        }

        public ILayer Layer
        {
            get => vectorLayerAttributeTableView.Layer;
            set => vectorLayerAttributeTableView.Layer = value;
        }

        public void EnsureVisible(object item)
        {
            vectorLayerAttributeTableView.EnsureVisible(item);
        }

        public void OnActivated()
        {
            vectorLayerAttributeTableView.OnActivated();
        }

        public void OnDeactivated()
        {
            vectorLayerAttributeTableView.OnDeactivated();
        }

        public void SetWaterFlowFmModel(WaterFlowFMModel model)
        {
            if (waterFlowFmModel != model)
            {
                waterFlowFmModel = model;
                initialConditionValuesByQuantity = new Dictionary<InitialConditionQuantity, IEnumerable<ChannelInitialConditionDefinition>>();
            }
            
            UpdateTableView();

            SubscribeDataEvents();
        }

        public void SetInitialConditionValuesByQuantity()
        {
            InitialConditionQuantity quantity = GetModelSettingsQuantity();
            
            foreach (var channelInitialConditionDefinition in waterFlowFmModel.ChannelInitialConditionDefinitions)
            {
                if (channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ModelSettings) continue;
            
                var constantDefinition = channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition;
                var spatialDefinition = channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;
                if (constantDefinition != null)
                {
                    quantity = constantDefinition.Quantity;
                    break;
                }
                else if (spatialDefinition != null)
                {
                    quantity = spatialDefinition.Quantity;
                    break;
                }
            }
            
            var copyOfCurrentValues = new List<ChannelInitialConditionDefinition>();
            waterFlowFmModel.ChannelInitialConditionDefinitions.ForEach(definition => copyOfCurrentValues.Add((ChannelInitialConditionDefinition)definition.Clone()));
            initialConditionValuesByQuantity[quantity] = copyOfCurrentValues;
        }

        public void SetCurrentQuantity()
        {
            currentQuantity = GetModelSettingsQuantity();
            SetCorrectDefinitionsOnModelBasedOnSelectedGlobalQuantity(currentQuantity);
        }

        public void SetOpenGlobalInitialConditionSettingsMethod(Action openGlobalInitialConditionSettingsMethod)
        {
            openGlobalInitialConditionSettings = openGlobalInitialConditionSettingsMethod;
        }

        public void SetZoomToFeatureMethod(Action<object> zoomToFeatureMethod)
        {
            vectorLayerAttributeTableView.ZoomToFeature = zoomToFeatureMethod;
        }

        public void SuspendUpdates()
        {
            UnsubscribeViewEvents();
            UnsubscribeDataEvents();

            // Set dummy layer to bypass eventing
            vectorLayerAttributeTableView.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new EventedList<ChannelInitialConditionDefinition>(), typeof(ChannelInitialConditionDefinition))
            };
        }

        public void ResumeUpdates()
        {
            SubscribeViewEvents();
            SubscribeDataEvents();
            
            // Reset original layer
            vectorLayerAttributeTableView.Data = data;
            
            UpdateTableView();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                UnsubscribeDataEvents();

                delayedEventHandlerDefinitionsCollectionChanged.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnSelectedFeaturesChanged(object s, EventArgs a)
        {
            SelectedFeaturesChanged?.Invoke(s, a);
        }

        private void UpdateTableView()
        {
            if (data != null && waterFlowFmModel != null)
            {
                SetTableViewColumns();

                vectorLayerAttributeTableView.TableView.BestFitColumns();
            }
        }

        private void SubscribeViewEvents()
        {
            var tableView = vectorLayerAttributeTableView.TableView;

            tableView.ReadOnlyCellFilter = ReadOnlyCellFilter;
            tableView.UnboundColumnData = UnboundColumnData;
        }

        private void UnsubscribeViewEvents()
        {
            var tableView = vectorLayerAttributeTableView.TableView;

            tableView.ReadOnlyCellFilter = null;
            tableView.UnboundColumnData = null;
        }

        private bool ReadOnlyCellFilter(TableViewCell arg)
        {
            var editable = true;

            if (data != null && arg.RowIndex >= 0 && arg.RowIndex < vectorLayerAttributeTableView.TableView.RowCount && arg.Column.AbsoluteIndex >= 0)
            {
                var channelInitialConditionDefinition = (ChannelInitialConditionDefinition)vectorLayerAttributeTableView.TableView.GetRowObjectAt(arg.RowIndex);

                switch (arg.Column.AbsoluteIndex)
                {
                    case ChannelColumnIndex:
                        editable = false;
                        break;
                    case SpecificationColumnIndex:
                        break;
                    case ValueColumnIndex:
                        editable = channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
                        break;
                    case ButtonColumnIndex:
                        editable = channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ModelSettings
                                   || channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
                        break;
                }
            }

            return !editable;
        }

        private object UnboundColumnData(int columnIndex, int dataSourceIndex, bool isGetData, bool isSetData, object value)
        {
            if (dataSourceIndex < 0 || columnIndex == ButtonColumnIndex)
            {
                return null;
            }

            var rowIndex = vectorLayerAttributeTableView.TableView.GetRowIndexByDataSourceIndex(dataSourceIndex);
            var channelInitialConditionDefinition = (ChannelInitialConditionDefinition)vectorLayerAttributeTableView.TableView.GetRowObjectAt(rowIndex);

            if (columnIndex == ChannelColumnIndex)
            {
                return channelInitialConditionDefinition.Channel.Name;
            }

            if (columnIndex == SpecificationColumnIndex)
            {
                if (isGetData)
                {
                    return channelInitialConditionDefinition.SpecificationType;
                }

                if (isSetData)
                {
                    var initialConditionQuantityToSet = channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition
                            ? channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Quantity
                            : channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition
                                ? channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity
                                : GetModelSettingsQuantity();

                    channelInitialConditionDefinition.SpecificationType = (ChannelInitialConditionSpecificationType)Enum.Parse(typeof(ChannelInitialConditionSpecificationType), value.ToString());

                    if (channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition)
                    {
                        channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Quantity = initialConditionQuantityToSet;
                        channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value = GetModelSettingsValue();
                    }

                    if (channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition)
                    {
                        channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity = initialConditionQuantityToSet;
                    }
                }
            }

            if (columnIndex == ValueColumnIndex)
            {
                if (isGetData)
                {
                    if (channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition)
                    {
                        return channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value;
                    }

                    if (channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ModelSettings)
                    {
                        return GetModelSettingsValue();
                    }
                }

                if (isSetData && channelInitialConditionDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition)
                {
                    channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value = double.Parse(value.ToString());
                }
            }

            return null;
        }

        private void SetTableViewColumns()
        {
            var tableView = vectorLayerAttributeTableView.TableView;

            tableView.Columns.Clear();

            AddChannelColumn(tableView);
            AddSpecificationColumn(tableView);
            AddValueColumn(tableView);
            UpdateQuantityColumnName(GetModelSettingsQuantity());
            AddButtonColumn(tableView);
        }

        private static void AddChannelColumn(TableView tableView)
        {
            tableView.AddUnboundColumn("Branch", typeof(string));
        }

        private static void AddSpecificationColumn(TableView tableView)
        {
            var columnIndex = tableView.AddUnboundColumn("Specification", typeof(ChannelInitialConditionSpecificationType));

            tableView.Columns[columnIndex].Editor = new ComboBoxTypeEditor
            {
                Items = Enum.GetValues(typeof(ChannelInitialConditionSpecificationType)),
                CustomFormatter = new EnumFormatter(typeof(ChannelInitialConditionSpecificationType)),
                ItemsMandatory = false // Note: Don't remove, necessary for copy/paste
            };
        }

        private static void AddValueColumn(TableView tableView)
        {
            tableView.AddUnboundColumn("Value", typeof(double));
        }

        private void AddButtonColumn(TableView tableView)
        {
            var buttonTypeEditor = new ButtonTypeEditor
            {
                ButtonClickAction = DoEditFunction,
                HideOnReadOnly = true
            };

            tableView.AddUnboundColumn(" ", typeof(string), -1, buttonTypeEditor);
        }

        private void DoEditFunction()
        {
            var channelInitialConditionDefinition = (ChannelInitialConditionDefinition)vectorLayerAttributeTableView.TableView.CurrentFocusedRowObject;

            switch (channelInitialConditionDefinition.SpecificationType)
            {
                case ChannelInitialConditionSpecificationType.ModelSettings:
                {
                    openGlobalInitialConditionSettings();
                    return;
                }
                case ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition:
                {
                    var spatialChannelInitialConditionDefinition = channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;

                    Form form = new ConstantSpatialChannelInitialConditionDefinitionsForm(
                        spatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions,
                        spatialChannelInitialConditionDefinition.Quantity,
                        channelInitialConditionDefinition.Channel.Name);

                    form.ShowDialog();
                    break;
                }
            }
        }

        private WaterFlowFMProperty GetInitialConditionValueModelProperty()
        {
            return waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D);
        }

        private WaterFlowFMProperty GetInitialConditionQuantityModelProperty()
        {
            return waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D);
        }

        private double GetModelSettingsValue()
        {
            return (double)GetInitialConditionValueModelProperty().Value;
        }

        private InitialConditionQuantity GetModelSettingsQuantity()
        {
            return (InitialConditionQuantity)GetInitialConditionQuantityModelProperty().Value;
        }
        
        private void SubscribeDataEvents()
        {
            waterFlowFmModel.ChannelInitialConditionDefinitions.CollectionChanged += delayedEventHandlerDefinitionsCollectionChanged;
            ((INotifyPropertyChanged)waterFlowFmModel.ChannelInitialConditionDefinitions).PropertyChanged += ChannelInitialConditionDefinitionsPropertyChanged;
            ((INotifyPropertyChanged)GetInitialConditionQuantityModelProperty()).PropertyChanged += InitialConditionModelSettingQuantityPropertyChanged;
            ((INotifyPropertyChanged)GetInitialConditionValueModelProperty()).PropertyChanged += InitialConditionModelSettingPropertyChanged;
        }

        private void UnsubscribeDataEvents()
        {
            waterFlowFmModel.ChannelInitialConditionDefinitions.CollectionChanged -= delayedEventHandlerDefinitionsCollectionChanged;
            ((INotifyPropertyChanged)waterFlowFmModel.ChannelInitialConditionDefinitions).PropertyChanged -= ChannelInitialConditionDefinitionsPropertyChanged;
            ((INotifyPropertyChanged)GetInitialConditionQuantityModelProperty()).PropertyChanged -= InitialConditionModelSettingQuantityPropertyChanged;
            ((INotifyPropertyChanged)GetInitialConditionValueModelProperty()).PropertyChanged -= InitialConditionModelSettingPropertyChanged;
        }

        private void ChannelInitialConditionDefinitionsCollectionChanged(object sender, EventArgs e)
        {
            vectorLayerAttributeTableView.TableView.BestFitColumns();
            RefreshData();
        }

        private void ChannelInitialConditionDefinitionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IChannel && e.PropertyName == nameof(IChannel.Name))
            {
                vectorLayerAttributeTableView.TableView.BestFitColumns();
            }
        }

        private void InitialConditionModelSettingQuantityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var newQuantityValue = ((WaterFlowFMProperty) sender).Value;
            var newQuantity = (InitialConditionQuantity)(int)newQuantityValue;
            if (newQuantity != currentQuantity)
            {
                var copyOfCurrentValues = new List<ChannelInitialConditionDefinition>();
                waterFlowFmModel.ChannelInitialConditionDefinitions.ForEach(definition => copyOfCurrentValues.Add((ChannelInitialConditionDefinition)definition.Clone()));
                initialConditionValuesByQuantity[currentQuantity] = copyOfCurrentValues;
                UpdateQuantityColumnName(newQuantity);

                SetCorrectDefinitionsOnModelBasedOnSelectedGlobalQuantity(newQuantity);
                
                currentQuantity = newQuantity;
            }

            RefreshData();
        }

        private void SetCorrectDefinitionsOnModelBasedOnSelectedGlobalQuantity(InitialConditionQuantity quantity)
        {
            if (initialConditionValuesByQuantity.ContainsKey(quantity))
            {
                waterFlowFmModel.ChannelInitialConditionDefinitions.ForEach(definition =>
                {
                    var channelInitialConditionDefinition = initialConditionValuesByQuantity[quantity]
                        .FirstOrDefault(d => Equals(d.Channel, definition.Channel));
                    if (channelInitialConditionDefinition == null)
                    {
                        return;
                    }

                    definition.SpecificationType = channelInitialConditionDefinition.SpecificationType;
                    switch (definition.SpecificationType)
                    {
                        case ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition:
                            definition.ConstantChannelInitialConditionDefinition.Value = channelInitialConditionDefinition
                                .ConstantChannelInitialConditionDefinition.Value;
                            definition.ConstantChannelInitialConditionDefinition.Quantity = channelInitialConditionDefinition
                                .ConstantChannelInitialConditionDefinition.Quantity;
                            break;
                        case ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition:
                            definition.SpatialChannelInitialConditionDefinition
                                .ConstantSpatialChannelInitialConditionDefinitions.AddRange(channelInitialConditionDefinition
                                    .SpatialChannelInitialConditionDefinition
                                    .ConstantSpatialChannelInitialConditionDefinitions);
                            definition.SpatialChannelInitialConditionDefinition.Quantity = channelInitialConditionDefinition
                                .SpatialChannelInitialConditionDefinition.Quantity;
                            break;
                        case ChannelInitialConditionSpecificationType.ModelSettings:
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                });
            }
            else
            {
                waterFlowFmModel.ChannelInitialConditionDefinitions.ForEach(definition =>
                    definition.SpecificationType = ChannelInitialConditionSpecificationType.ModelSettings);
            }
        }

        private void InitialConditionModelSettingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshData();
        }

        private void UpdateQuantityColumnName(InitialConditionQuantity quantity)
        {
            vectorLayerAttributeTableView.TableView.Columns[ValueColumnIndex].Caption =
                InitialConditionQuantityTypeConverter.ConvertInitialConditionQuantityToString(quantity);
        }

        [InvokeRequired]
        private void RefreshData()
        {
            vectorLayerAttributeTableView.TableView.RefreshData();
            vectorLayerAttributeTableView.TableView.BestFitColumns();
        }
    }
}
