using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.Friction
{
    public partial class ChannelFrictionDefinitionsView : UserControl, ILayerEditorView, ISuspendibleView
    {
        private readonly VectorLayerAttributeTableView vectorLayerAttributeTableView;

        private readonly DelayedEventHandler<EventArgs> delayedEventHandlerDefinitionsCollectionChanged;

        private const int ChannelColumnIndex = 0;
        private const int SpecificationColumnIndex = 1;
        private const int TypeColumnIndex = 2;
        private const int ValueColumnIndex = 3;
        private const int UnitColumnIndex = 4;
        private const int FunctionTypeColumnIndex = 5;
        private const int ButtonColumnIndex = 6;

        private ILayer data;
        private WaterFlowFMModel waterFlowFmModel;
        private Action openGlobalFrictionSettings;

        public event EventHandler SelectedFeaturesChanged;

        public ChannelFrictionDefinitionsView()
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
                new DelayedEventHandler<EventArgs>(ChannelFrictionDefinitionsCollectionChanged)
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
            waterFlowFmModel = model;

            UpdateTableView();

            SubscribeDataEvents();
        }

        public void SetOpenGlobalFrictionSettingsMethod(Action openGlobalFrictionSettingsMethod)
        {
            openGlobalFrictionSettings = openGlobalFrictionSettingsMethod;
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
                DataSource = new FeatureCollection(new EventedList<ChannelFrictionDefinition>(), typeof(ChannelFrictionDefinition))
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
                var channelFrictionDefinition =  (ChannelFrictionDefinition) vectorLayerAttributeTableView.TableView.GetRowObjectAt(arg.RowIndex);

                switch (arg.Column.AbsoluteIndex)
                {
                    case ChannelColumnIndex:
                        editable = false;
                        break;
                    case SpecificationColumnIndex:
                        break;
                    case TypeColumnIndex:
                        editable = channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition
                                   || channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        break;
                    case ValueColumnIndex:
                        editable = channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
                        break;
                    case UnitColumnIndex:
                        editable = false;
                        break;
                    case FunctionTypeColumnIndex:
                        editable = false;
                        break;
                    case ButtonColumnIndex:
                        editable = channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ModelSettings
                                   || channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
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
            var channelFrictionDefinition = (ChannelFrictionDefinition) vectorLayerAttributeTableView.TableView.GetRowObjectAt(rowIndex);
            if (channelFrictionDefinition == null) return null;

            if (columnIndex == ChannelColumnIndex)
            {
                return channelFrictionDefinition.Channel.Name;
            }

            if (columnIndex == SpecificationColumnIndex)
            {
                if (isGetData)
                {
                    return channelFrictionDefinition.SpecificationType;
                }

                if (isSetData)
                {
                    var roughnessTypeToSet = channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition
                            ? channelFrictionDefinition.ConstantChannelFrictionDefinition.Type
                            : channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition
                                ? channelFrictionDefinition.SpatialChannelFrictionDefinition.Type
                                : GetModelSettingsType();

                    channelFrictionDefinition.SpecificationType = (ChannelFrictionSpecificationType) Enum.Parse(typeof(ChannelFrictionSpecificationType), value.ToString());

                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition)
                    {
                        channelFrictionDefinition.ConstantChannelFrictionDefinition.Type = roughnessTypeToSet;
                        channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = GetModelSettingsValue();
                    }

                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition)
                    {
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessTypeToSet;
                    }
                }
            }

            if (columnIndex == TypeColumnIndex)
            {
                if (isGetData)
                {
                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition)
                    {
                        return channelFrictionDefinition.ConstantChannelFrictionDefinition.Type;
                    }

                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition)
                    {
                        return channelFrictionDefinition.SpatialChannelFrictionDefinition.Type;
                    }

                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ModelSettings)
                    {
                        return GetModelSettingsType();
                    }
                }

                if (isSetData)
                {
                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition)
                    {
                        var roughnessType = (RoughnessType) Enum.Parse(typeof(RoughnessType), value.ToString());

                        channelFrictionDefinition.ConstantChannelFrictionDefinition.Type = roughnessType;
                        channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = RoughnessHelper.GetDefault(roughnessType);
                    }

                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition)
                    {
                        var roughnessType = (RoughnessType) Enum.Parse(typeof(RoughnessType), value.ToString());

                        channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessType;
                    }
                }
            }

            if (columnIndex == ValueColumnIndex)
            {
                if (isGetData)
                {
                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition)
                    {
                        return channelFrictionDefinition.ConstantChannelFrictionDefinition.Value;
                    }

                    if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ModelSettings)
                    {
                        return GetModelSettingsValue();
                    }
                }

                if (isSetData && channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition)
                {
                    channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = double.Parse(value.ToString());
                }
            }

            if (columnIndex == UnitColumnIndex)
            {
                if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition)
                {
                    return RoughnessHelper.GetUnit(channelFrictionDefinition.ConstantChannelFrictionDefinition.Type);
                }

                if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition)
                {
                    return RoughnessHelper.GetUnit(channelFrictionDefinition.SpatialChannelFrictionDefinition.Type);
                }

                if (channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.ModelSettings)
                {
                    return RoughnessHelper.GetUnit(GetModelSettingsType());
                }
            }

            if (columnIndex == FunctionTypeColumnIndex)
            {
                if (isGetData && channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition)
                {
                    return channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType;
                }

                if (isSetData && channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition)
                {
                    channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = (RoughnessFunction) Enum.Parse(typeof(RoughnessFunction), value.ToString());
                }
            }

            if (isSetData)
            {
                vectorLayerAttributeTableView.TableView.BestFitColumns();
            }

            return null;
        }

        private void SetTableViewColumns()
        {
            var tableView = vectorLayerAttributeTableView.TableView;

            tableView.Columns.Clear();

            AddChannelColumn(tableView);
            AddSpecificationColumn(tableView);
            AddTypeColumn(tableView);
            AddValueColumn(tableView);
            AddUnitColumn(tableView);
            AddFunctionTypeColumn(tableView);
            AddButtonColumn(tableView);
        }

        private static void AddChannelColumn(TableView tableView)
        {
            tableView.AddUnboundColumn("Branch", typeof(string));
        }

        private static void AddSpecificationColumn(TableView tableView)
        {
            var columnIndex = tableView.AddUnboundColumn("Specification", typeof(ChannelFrictionSpecificationType));

            tableView.Columns[columnIndex].Editor = new ComboBoxTypeEditor
            {
                Items = Enum.GetValues(typeof(ChannelFrictionSpecificationType)).Except(new[] {ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions}),
                CustomFormatter = new EnumFormatter(typeof(ChannelFrictionSpecificationType)),
                ItemsMandatory = false // Note: Don't remove, necessary for copy/paste
            };
        }

        private static void AddTypeColumn(TableView tableView)
        {
            var columnIndex = tableView.AddUnboundColumn("Roughness type", typeof(RoughnessType));

            tableView.Columns[columnIndex].Editor = new ComboBoxTypeEditor
            {
                Items = Enum.GetValues(typeof(RoughnessType)),
                CustomFormatter = new EnumFormatter(typeof(RoughnessType)),
                ItemsMandatory = false // Note: Don't remove, necessary for copy/paste
            };
        }

        private static void AddValueColumn(TableView tableView)
        {
            tableView.AddUnboundColumn("Value", typeof(double));
        }

        private static void AddUnitColumn(TableView tableView)
        {
            tableView.AddUnboundColumn("Unit", typeof(string));
        }

        private static void AddFunctionTypeColumn(TableView tableView)
        {
            var columnIndex = tableView.AddUnboundColumn("Function type", typeof(RoughnessFunction));

            tableView.Columns[columnIndex].Editor = new ComboBoxTypeEditor
            {
                Items = Enum.GetValues(typeof(RoughnessFunction)),
                CustomFormatter = new EnumFormatter(typeof(RoughnessFunction))
            };
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
            var channelFrictionDefinition = (ChannelFrictionDefinition) vectorLayerAttributeTableView.TableView.CurrentFocusedRowObject;

            switch (channelFrictionDefinition.SpecificationType)
            {
                case ChannelFrictionSpecificationType.ModelSettings:
                {
                    openGlobalFrictionSettings();
                    return;
                }
                case ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition:
                {
                    Form form;

                    var spatialChannelFrictionDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;

                    switch (spatialChannelFrictionDefinition.FunctionType)
                    {
                        case RoughnessFunction.Constant:
                        {
                            form = new ConstantSpatialChannelFrictionDefinitionsForm(
                                spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions,
                                spatialChannelFrictionDefinition.Type,
                                channelFrictionDefinition.Channel.Name);
                            break;
                        }
                        case RoughnessFunction.FunctionOfQ:
                        {
                            form = RoughnessAsFunctionOfView("Q", channelFrictionDefinition);
                            break;
                        }
                        case RoughnessFunction.FunctionOfH:
                        {
                            form = RoughnessAsFunctionOfView("H", channelFrictionDefinition);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    form.ShowDialog();

                    break;
                }
            }
        }

        private static RoughnessAsFunctionOfView RoughnessAsFunctionOfView(string variableName, ChannelFrictionDefinition channelFrictionDefinition)
        {
            return new RoughnessAsFunctionOfView(
                variableName,
                channelFrictionDefinition.Channel.Name,
                channelFrictionDefinition.SpatialChannelFrictionDefinition.Type,
                RoughnessHelper.GetUnit(channelFrictionDefinition.SpatialChannelFrictionDefinition.Type),
                false)
            {
                Data = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function
            };
        }

        private WaterFlowFMProperty GetFrictionTypeModelProperty()
        {
            return waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.UnifFrictTypeChannels);
        }

        private WaterFlowFMProperty GetFrictionValueModelProperty()
        {
            return waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.UnifFrictCoefChannels);
        }

        private RoughnessType GetModelSettingsType()
        {
            return (RoughnessType) GetFrictionTypeModelProperty().Value;
        }

        private double GetModelSettingsValue()
        {
            return (double) GetFrictionValueModelProperty().Value;
        }

        private void SubscribeDataEvents()
        {
            waterFlowFmModel.ChannelFrictionDefinitions.CollectionChanged += delayedEventHandlerDefinitionsCollectionChanged;
            ((INotifyPropertyChanged) waterFlowFmModel.ChannelFrictionDefinitions).PropertyChanged += ChannelFrictionDefinitionsPropertyChanged;
            ((INotifyPropertyChanged) GetFrictionTypeModelProperty()).PropertyChanged += FrictionModelSettingPropertyChanged;
            ((INotifyPropertyChanged) GetFrictionValueModelProperty()).PropertyChanged += FrictionModelSettingPropertyChanged;
        }

        private void UnsubscribeDataEvents()
        {
            waterFlowFmModel.ChannelFrictionDefinitions.CollectionChanged -= delayedEventHandlerDefinitionsCollectionChanged;
            ((INotifyPropertyChanged) waterFlowFmModel.ChannelFrictionDefinitions).PropertyChanged -= ChannelFrictionDefinitionsPropertyChanged;
            ((INotifyPropertyChanged) GetFrictionTypeModelProperty()).PropertyChanged -= FrictionModelSettingPropertyChanged;
            ((INotifyPropertyChanged) GetFrictionValueModelProperty()).PropertyChanged -= FrictionModelSettingPropertyChanged;
        }

        private void ChannelFrictionDefinitionsCollectionChanged(object sender, EventArgs e)
        {
            vectorLayerAttributeTableView.TableView.BestFitColumns();
        }

        private void ChannelFrictionDefinitionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IChannel && e.PropertyName == nameof(IChannel.Name))
            {
                vectorLayerAttributeTableView.TableView.BestFitColumns();
            }
        }

        private void FrictionModelSettingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            vectorLayerAttributeTableView.TableView.RefreshData();
            vectorLayerAttributeTableView.TableView.BestFitColumns();
        }
    }
}
