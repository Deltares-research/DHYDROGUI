using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Controls.Swf.Table.Validation;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class FlowBoundaryConditionDataView : UserControl, ICompositeView
    {
        private enum ViewMode
        {
            Single,
            Combined
        };

        private const string NoDataText = "No data defined; to create boundary data, activate a support" + " point.";
        private const string NoBcText = "No boundary condition defined; to create one, click the '+' button.";
        private static readonly ILog log = LogManager.GetLogger(typeof(FlowBoundaryConditionDataView));

        private readonly IFileDialogService fileDialogService = new FileDialogService();

        private readonly SaveFileDialog SaveFileDialog = new SaveFileDialog()
        {
            AddExtension = true,
            DefaultExt = BcFile.Extension
        };

        private readonly FlowBoundaryConditionSeriesFactory seriesFactory;

        private readonly AddSeriesTool addSeriesTool;

        private IBoundaryCondition boundaryCondition;
        private BoundaryConditionSet boundaryConditionSet;
        private IFunction boundaryConditionData;

        private WaterFlowFMModel model;

        private ViewMode mode;

        private bool componentsChanged;

        public FlowBoundaryConditionDataView()
        {
            InitializeComponent();

            const double factor = 180 / Math.PI;
            AstroComponents = HarmonicComponent.DefaultAstroComponentsRadPerHour.Take(10).ToDictionary(kvp => kvp.Key,
                                                                                                       kvp =>
                                                                                                           kvp.Value *
                                                                                                           factor);
            foreach (KeyValuePair<string, double> kvp in HarmonicComponent
                                                         .DefaultAstroComponentsRadPerHour.Skip(10).ToDictionary(
                                                             kvp => kvp.Key,
                                                             kvp =>
                                                                 kvp.Value * factor).OrderBy(kvp => kvp.Key))
            {
                AstroComponents.Add(kvp);
            }

            seriesFactory = new FlowBoundaryConditionSeriesFactory {AstroComponents = AstroComponents};
            functionView.CreateSeriesMethod = seriesFactory.CreateSeries;
            genDataButton.Click += GenerateDataButtonClick;
            boundaryDataListBox.CheckOnClick = true;
            boundaryDataListBox.Format += BoundaryDataListBoxFormat;
            boundaryDataListBox.ItemCheck += BoundaryDataListBoxOnItemCheck;
            if (functionView.ChartView is ChartView chartView)
            {
                var customDateTimeFormatInfo = (DateTimeFormatInfo) CultureInfo.CurrentCulture.DateTimeFormat.Clone();
                customDateTimeFormatInfo.LongTimePattern = "HH:mm:ss";
                customDateTimeFormatInfo.ShortTimePattern = "HH:mm";
                chartView.DateTimeLabelFormatProvider.CustomDateTimeFormatInfo = customDateTimeFormatInfo;
            }

            addSeriesTool = new AddSeriesTool
            {
                ChartView = functionView.ChartView,
                Active = true,
                AddSeriesToView = AddSeriesToView,
                RemoveSeriesFromView = RemoveSeriesFromView
            };
            functionView.ChartView.Tools.Add(addSeriesTool);

            UpdateControl();

            Mode = ViewMode.Single;
        }

        public int SupportPointIndex { private get; set; }

        public IBoundaryCondition BoundaryCondition
        {
            private get
            {
                return boundaryCondition;
            }
            set
            {
                if (Equals(value, boundaryCondition))
                {
                    return;
                }

                if (boundaryCondition != null)
                {
                    ((INotifyPropertyChanged) boundaryCondition).PropertyChanged -= BoundaryConditionPropertyChanged;
                }

                boundaryCondition = value;
                addSeriesTool.BoundaryCondition = value;
                if (boundaryCondition != null)
                {
                    ((INotifyPropertyChanged) boundaryCondition).PropertyChanged += BoundaryConditionPropertyChanged;
                }

                SupportPointIndex = 0;
                RefreshBoundaryData();
            }
        }

        public WaterFlowFMModel Model
        {
            private get
            {
                return model;
            }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChanged) model).PropertyChanged -= ModelPropertyChanged;
                }

                model = value;
                if (model != null)
                {
                    ((INotifyPropertyChanged) model).PropertyChanged += ModelPropertyChanged;
                    seriesFactory.ModelStartTime = model.StartTime;
                    seriesFactory.ModelStopTime = model.StopTime;
                    seriesFactory.ModelReferenceTime = model.ReferenceTime;
                }
            }
        }

        public BoundaryConditionSet BoundaryConditionSet
        {
            private get
            {
                return boundaryConditionSet;
            }
            set
            {
                if (Equals(value, boundaryConditionSet))
                {
                    return;
                }

                boundaryConditionSet = value;
                addSeriesTool.BoundaryConditionSet = boundaryConditionSet;
            }
        }

        public object Data { get; set; }
        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public IEventedList<IView> ChildViews => functionView.ChildViews;

        public bool HandlesChildViews => true;

        public void RefreshBoundaryData()
        {
            BoundaryConditionData = BoundaryCondition != null && SupportPointIndex != -1
                                        ? BoundaryCondition.GetDataAtPoint(SupportPointIndex)
                                        : null;
        }

        public void OnSupportPointChanged(object sender, EventArgs<int> e)
        {
            SupportPointIndex = e.Value;
            addSeriesTool.SelectedIndex = SupportPointIndex;
            RefreshBoundaryData();
        }

        [InvokeRequired]
        public void UpdateControl()
        {
            ClearFunctionView();
            ConfigureSeriesFactory();
            FillCheckedListBox();
            FillFunctionView();
            UpdateFunctionViewMode();
            UpdateButtons();
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public void ActivateChildView(IView childView)
        {
            // Nothing to be done, enforced through ICompositeView
        }

        private IDictionary<string, double> AstroComponents { get; set; }

        private IFunction BoundaryConditionData
        {
            get => boundaryConditionData;
            set
            {
                if (ReferenceEquals(boundaryConditionData, value))
                {
                    return;
                }

                if (boundaryConditionData != null)
                {
                    boundaryConditionData.Components.CollectionChanged -= ComponentsCollectionChanged;
                    if (model != null)
                    {
                        model.SedimentFractions.CollectionChanged -= SedimentsCollectionChanged;
                    }

                    ((INotifyPropertyChange) boundaryConditionData).PropertyChanged -= OnPropertyChanged;
                }

                boundaryConditionData = value;
                if (boundaryConditionData != null)
                {
                    boundaryConditionData.Components.CollectionChanged += ComponentsCollectionChanged;
                    if (model != null)
                    {
                        model.SedimentFractions.CollectionChanged += SedimentsCollectionChanged;
                    }

                    ((INotifyPropertyChange) boundaryConditionData).PropertyChanged += OnPropertyChanged;
                }

                UpdateControl();

                if (boundaryConditionData != null && Model != null)
                {
                    TimeArgumentConfigurer.Configure(BoundaryConditionData, Model);
                }
            }
        }

        private DateTime ModelStartTime => Model == null ? DateTime.MinValue : Model.StartTime;

        private DateTime ModelStopTime => Model == null ? ModelStartTime.AddDays(1) : Model.StopTime;

        private bool FourierDataType =>
            BoundaryCondition.DataType == BoundaryConditionDataType.AstroComponents ||
            BoundaryCondition.DataType == BoundaryConditionDataType.AstroCorrection ||
            BoundaryCondition.DataType == BoundaryConditionDataType.Harmonics ||
            BoundaryCondition.DataType == BoundaryConditionDataType.HarmonicCorrection;

        private ViewMode Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    UpdateFunctionViewMode();
                }
            }
        }

        private void BoundaryDataListBoxOnItemCheck(object sender, ItemCheckEventArgs itemCheckEventArgs)
        {
            if (!(boundaryDataListBox.SelectedItem is IBoundaryCondition bc))
            {
                return;
            }

            if (bc == BoundaryCondition)
            {
                itemCheckEventArgs.NewValue = CheckState.Indeterminate;
                return;
            }

            if (itemCheckEventArgs.NewValue == CheckState.Checked)
            {
                AddSeriesToView(bc);
                addSeriesTool.AddedBoundaryConditions.Add(bc);
            }
            else
            {
                addSeriesTool.AddedBoundaryConditions.Remove(bc);
                RemoveSeriesFromView(bc);
            }
        }

        private void AddSeriesToView(IBoundaryCondition flowBoundaryCondition)
        {
            seriesFactory.BackgroundFunctions.Add(
                BoundaryConditionWrapper(flowBoundaryCondition as FlowBoundaryCondition));
            functionView.RefreshChartView();
        }

        private void RemoveSeriesFromView(IBoundaryCondition flowBoundaryCondition)
        {
            IFunction function = flowBoundaryCondition.GetDataAtPoint(SupportPointIndex);
            seriesFactory.BackgroundFunctions.RemoveAllWhere(fw => ReferenceEquals(fw.Function, function));
            functionView.RefreshChartView();
        }

        private FlowBoundaryConditionPointData BoundaryConditionWrapper(FlowBoundaryCondition condition)
        {
            return condition == null || condition.DataType == BoundaryConditionDataType.Constant ||
                   condition.DataType == BoundaryConditionDataType.Empty
                       ? null
                       : new FlowBoundaryConditionPointData(condition, SupportPointIndex,
                                                            model != null && model.UseDepthLayers ||
                                                            condition.FlowQuantity == FlowBoundaryQuantityType
                                                                .MorphologyBedLoadTransport);
        }

        private FlowBoundaryConditionPointData BoundaryConditionWrapper()
        {
            return BoundaryConditionWrapper(BoundaryCondition as FlowBoundaryCondition);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (boundaryConditionData.IsNestedEditingDone() && componentsChanged)
            {
                UpdateControl();
                componentsChanged = false;
            }
        }

        private void SedimentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateControl();
            componentsChanged = false;
        }

        private void ComponentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            componentsChanged = true;
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(model.StartTime)) ||
                e.PropertyName.Equals(nameof(model.StopTime)) ||
                e.PropertyName.Equals(nameof(model.ReferenceTime)))
            {
                seriesFactory.ModelStartTime = model.StartTime;
                seriesFactory.ModelStopTime = model.StopTime;
                seriesFactory.ModelReferenceTime = model.ReferenceTime;
                UpdateControl();
            }

            if (e.PropertyName.Equals(nameof(model.DepthLayerDefinition)))
            {
                UpdateControl();
            }
        }

        private void BoundaryConditionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (boundaryCondition.IsEditing)
            {
                return;
            }

            if (e.PropertyName == nameof(boundaryCondition.IsEditing) ||
                e.PropertyName == nameof(boundaryCondition.DataType) ||
                e.PropertyName == nameof(FlowBoundaryCondition.Offset) ||
                e.PropertyName == nameof(FlowBoundaryCondition.Factor) ||
                e.PropertyName == nameof(FlowBoundaryCondition.ThatcherHarlemanTimeLag))
            {
                RefreshBoundaryData();
                UpdateControl();
            }
        }

        private void FillCheckedListBox()
        {
            boundaryDataListBox.Items.Clear();

            if (boundaryCondition == null || BoundaryConditionData == null ||
                boundaryCondition.DataType == BoundaryConditionDataType.Empty ||
                boundaryCondition.DataType == BoundaryConditionDataType.Constant)
            {
                return;
            }

            if (BoundaryCondition.DataType == BoundaryConditionDataType.Qh)
            {
                boundaryDataListBox.Items.Add(BoundaryCondition);
                boundaryDataListBox.SetItemCheckState(0, CheckState.Indeterminate);
                return;
            }

            boundaryDataListBox.Items.AddRange(
                BoundaryConditionSet.BoundaryConditions.Where(
                                        bc =>
                                            bc.DataPointIndices.Contains(SupportPointIndex) &&
                                            bc.DataType != BoundaryConditionDataType.Qh &&
                                            bc.DataType != BoundaryConditionDataType.Constant &&
                                            bc.DataType != BoundaryConditionDataType.Empty)
                                    .OfType<object>()
                                    .ToArray());

            int boundaryConditionIndex = boundaryDataListBox.Items.IndexOf(BoundaryCondition);
            if (boundaryConditionIndex != -1)
            {
                boundaryDataListBox.SetItemCheckState(boundaryConditionIndex, CheckState.Indeterminate);
            }

            foreach (FlowBoundaryCondition bc in seriesFactory.BackgroundFunctions.Select(bf => bf.BoundaryCondition))
            {
                int index = boundaryDataListBox.Items.IndexOf(bc);
                if (index != -1)
                {
                    boundaryDataListBox.SetItemChecked(index, true);
                }
            }
        }

        private void BoundaryDataListBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            if (!(e.ListItem is IBoundaryCondition item))
            {
                return;
            }

            e.Value = item.Name;
        }

        private void ClearFunctionView()
        {
            functionView.Data = null;
        }

        private void FillFunctionView()
        {
            if (BoundaryConditionSet == null)
            {
                return;
            }

            if (BoundaryConditionData == null)
            {
                functionView.Visible = false;
                if (BoundaryCondition != null && (BoundaryCondition.DataType == BoundaryConditionDataType.Empty ||
                                                  BoundaryCondition.DataType == BoundaryConditionDataType.Constant))
                {
                    noDataLabel.Text = "";
                }
                else
                {
                    noDataLabel.Text = boundaryCondition == null ? NoBcText : NoDataText;
                }

                noDataLabel.Visible = true;
                return;
            }

            BoundaryConditionDataType boundaryConditionDataType = BoundaryCondition.DataType;
            if (boundaryConditionDataType == BoundaryConditionDataType.Empty)
            {
                functionView.Visible = false;
                noDataLabel.Visible = false;
                return;
            }

            functionView.Visible = true;
            noDataLabel.Visible = false;

            if (FourierDataType && functionView.SelectPointTool != null)
            {
                functionView.SelectPointTool.Active = false;
            }

            UnsubscribeFunctionViewData();
            functionView.TableView.EditableObject = BoundaryConditionData;
            functionView.Data = BoundaryConditionData;
            SubscribeFunctionViewData();

            functionView.TableView.PasteController =
                new TableViewArgumentBasedPasteController((TableView) functionView.TableView, new[]
                {
                    0
                }) {SkipRowsWithMissingArgumentValues = true};
            ((TableView) functionView.TableView).ExceptionMode = TableView.ValidationExceptionMode.NoAction;
            if (BoundaryConditionData.Arguments[0].ValueType == typeof(DateTime)) //override time navigator selection
            {
                functionView.SetCurrentTimeSelection(
                    BoundaryConditionData.Arguments[0].Values.Cast<DateTime>().FirstOrDefault(),
                    BoundaryConditionData.Arguments[0].Values.Cast<DateTime>().LastOrDefault());
            }

            if (model == null || !model.UseDepthLayers)
            {
                FlowBoundaryConditionPointData boundaryConditionWrapper = BoundaryConditionWrapper();
                if (boundaryConditionWrapper != null)
                {
                    int dimension = boundaryConditionWrapper.ForcingTypeDimension *
                                    boundaryConditionWrapper.VariableDimension;

                    foreach (ITableViewColumn column in functionView.TableView.Columns)
                    {
                        column.Visible = column.AbsoluteIndex - 1 < dimension;
                    }
                }
            }
            else
            {
                foreach (ITableViewColumn column in functionView.TableView.Columns)
                {
                    column.Visible = true;
                }
            }

            // Fix display order after setting visibility (someone please fix this...)
            for (var i = 0; i < functionView.TableView.Columns.Count; ++i)
            {
                if (!functionView.TableView.Columns[i].Visible)
                {
                    break;
                }

                functionView.TableView.Columns[i].DisplayIndex = i;
            }

            functionView.TableView.BestFitColumns(false);

            if (FourierDataType)
            {
                functionView.ChartView.Chart.BottomAxis.Title = "Time [h]";
                functionView.TableView.RowValidator = ValidateTableRow;
                if (boundaryConditionDataType == BoundaryConditionDataType.AstroComponents ||
                    boundaryConditionDataType == BoundaryConditionDataType.AstroCorrection)
                {
                    functionView.TableView.Columns[0].Editor = new ComboBoxTypeEditor
                    {
                        Items = AstroComponents.Keys,
                        UseWaitCursor = true
                    };
                    functionView.TableView.PasteController.PasteBehaviour =
                        TableViewPasteBehaviourOptions.SkipRowWhenValueIsInvalid;
                    IVariable<string> argument = BoundaryConditionData.Arguments.OfType<IVariable<string>>().First();
                    argument.NextValueGenerator = new FuncNextValueGenerator<string>(GenerateNextAstroComponent);
                }
            }
        }

        private void ConfigureSeriesFactory()
        {
            seriesFactory.SignalFunction = BoundaryConditionWrapper();
        }

        private static string GenerateNextAstroComponent()
        {
            return "zzz";
        }

        private IRowValidationResult ValidateTableRow(int arg1, object[] arg2)
        {
            if ((BoundaryCondition.DataType == BoundaryConditionDataType.AstroComponents ||
                 BoundaryCondition.DataType == BoundaryConditionDataType.AstroCorrection) &&
                (!(arg2[0] is string field) || field.StartsWith("zzz") || !AstroComponents.ContainsKey(field)))
            {
                return new RowValidationResult(0, "Astronomic component not recognized");
            }

            if (BoundaryCondition.DataType == BoundaryConditionDataType.Harmonics ||
                BoundaryCondition.DataType == BoundaryConditionDataType.HarmonicCorrection)
            {
                var frequency = (double) arg2[0];
                if (frequency < 0 || frequency > 1000000)
                {
                    return new RowValidationResult(0, "Input frequency should be between 0 and 1e+06 deg/h");
                }
            }

            return new RowValidationResult(0, "");
        }

        private void UnsubscribeFunctionViewData()
        {
            if (functionView.Data is IFunction function)
            {
                function.ValuesChanged -= FunctionValuesChanged;
                ((INotifyPropertyChanged) function).PropertyChanged -= FunctionPropertyChanged;
            }
        }

        private void SubscribeFunctionViewData()
        {
            if (functionView.Data is IFunction function)
            {
                function.ValuesChanged += FunctionValuesChanged;
                ((INotifyPropertyChanged) function).PropertyChanged += FunctionPropertyChanged;
            }
        }

        [InvokeRequired]
        private void FunctionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BoundaryCondition == null || BoundaryCondition.IsEditing)
            {
                return;
            }

            var function = (IFunction) functionView.Data;

            if (!Equals(sender, function) || e.PropertyName == "Dummy")
            {
                return;
            }

            if (function != null && function.IsNestedEditingDone())
            {
                seriesFactory.EvaluateSignal();
                functionView.RefreshChartView();
            }
        }

        [InvokeRequired]
        private void FunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (BoundaryCondition == null || BoundaryCondition.IsEditing)
            {
                return;
            }

            var function = (IFunction) functionView.Data;

            if (!Equals(sender, function))
            {
                return;
            }

            if (function != null && !function.IsEditing)
            {
                seriesFactory.EvaluateSignal();
                functionView.RefreshChartView();
            }
        }

        private void UpdateButtons()
        {
            if (BoundaryCondition == null)
            {
                genDataButton.Text = "";
                genDataButton.Enabled = false;
            }
            else
            {
                if (BoundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
                {
                    genDataButton.Text = "Generate series...";
                    genDataButton.Enabled = true;
                }
                else if (BoundaryCondition.DataType == BoundaryConditionDataType.AstroComponents ||
                         BoundaryCondition.DataType == BoundaryConditionDataType.AstroCorrection)
                {
                    genDataButton.Text = "Select components...";
                    genDataButton.Enabled = true;
                }
                else if (BoundaryCondition.DataType == BoundaryConditionDataType.Harmonics ||
                         BoundaryCondition.DataType == BoundaryConditionDataType.HarmonicCorrection)
                {
                    genDataButton.Text = "Add components...";
                    genDataButton.Enabled = true;
                }
                else
                {
                    genDataButton.Text = "";
                    genDataButton.Enabled = false;
                }
            }

            fileImportButton.Enabled = BoundaryCondition != null &&
                                       BoundaryCondition.DataType != BoundaryConditionDataType.Empty;
            fileExportButton.Enabled = BoundaryCondition != null &&
                                       BoundaryCondition.DataType != BoundaryConditionDataType.Empty;
        }

        private void GenerateDataButtonClick(object sender, EventArgs e)
        {
            if (BoundaryCondition != null)
            {
                switch (BoundaryCondition.DataType)
                {
                    case BoundaryConditionDataType.TimeSeries:
                        TimeSeriesDialog();
                        break;
                    case BoundaryConditionDataType.AstroComponents:
                    case BoundaryConditionDataType.AstroCorrection:
                        AstroComponentsDialog();
                        break;
                    case BoundaryConditionDataType.Harmonics:
                        HarmonicComponentDialog();
                        break;
                    case BoundaryConditionDataType.HarmonicCorrection:
                        HarmonicComponentDialog(true);
                        break;
                }
            }

            RefreshBoundaryData();
            FillFunctionView();
        }

        private void TimeSeriesDialog()
        {
            var generateDialog = new TimeSeriesGeneratorDialog {ApplyOnAccept = false};
            DateTime startTime = ModelStartTime;
            DateTime stopTime = ModelStopTime;
            var timeStep = new TimeSpan(0, 12, 0, 0);

            generateDialog.StartPosition = FormStartPosition.CenterScreen;
            generateDialog.SetData(null, startTime, stopTime, timeStep);

            generateDialog.ShowDialog(this);

            if (generateDialog.DialogResult != DialogResult.OK)
            {
                return;
            }

            ApplyBoundaryConditionsForSupportPointMode(new DateTime[]
                                                           {},
                                                       (values, function) =>
                                                           GenerateTimeSeries(function, generateDialog),
                                                       "Generate/modify timeseries");
        }

        private static bool GenerateTimeSeries(IFunction function, TimeSeriesGeneratorDialog generateDialog)
        {
            function.BeginEdit("Generate/modify timeseries");
            IVariable<DateTime> argument = function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
            if (!generateDialog.Apply(argument))
            {
                function.EndEdit();
                return true;
            }

            function.EndEdit();

            return false;
        }

        private void HarmonicComponentDialog(bool correctionsEnabled = false)
        {
            var dialog = new HarmonicConditionsDialog(correctionsEnabled);

            bool? dialogResult = dialog.ShowDialog();
            if (!dialogResult.HasValue || !dialogResult.Value)
            {
                return;
            }

            var viewModel = (HarmonicConditionsDialogViewModel) dialog.DataContext;
            double amplitude = viewModel.Amplitude;
            double frequency = viewModel.Frequency;
            double phase = viewModel.Phase;
            double amplitudeCorrection = viewModel.AmplitudeCorrection;
            double phaseCorrection = viewModel.PhaseCorrection;

            double[] newComponentValues =
            {
                frequency,
                amplitude,
                phase,
                amplitudeCorrection,
                phaseCorrection
            };

            ApplyBoundaryConditionsForSupportPointMode(newComponentValues, ApplyHarmonicComponentValues,
                                                       "Generate/modify harmonic component values");
        }

        private void ApplyBoundaryConditionsForSupportPointMode<T>(T[] newComponentValues,
                                                                   Func<T[], IFunction, bool> applyToFunction,
                                                                   string actionName)
        {
            var supportPointsDialog = new SupportPointSelectionForm();
            var defaultPointMode = SupportPointMode.SelectedPoint;
            if (!FlowBoundaryCondition.IsMorphologyBoundary(BoundaryCondition))
            {
                supportPointsDialog.ShowDialog(this);
                defaultPointMode = supportPointsDialog.SupportPointOperationMode;
            }

            BoundaryCondition.ApplyForSupportPointMode(defaultPointMode, newComponentValues, applyToFunction,
                                                       actionName, SupportPointIndex);

            RefreshBoundaryData();
            FillFunctionView();
            functionView.RefreshChartView();
        }

        private static bool ApplyHarmonicComponentValues(double[] newValues, IFunction function)
        {
            IVariable variable = function.Arguments.FirstOrDefault();
            if (variable == null)
            {
                throw new NotSupportedException("Function has no arguments");
            }

            if (!(function.Components.Count == 2 || function.Components.Count == 4))
            {
                throw new NotSupportedException("Incorrect number of components");
            }

            bool isCorrected = function.Components.Count == 4;

            function.BeginEdit("Generate/modify harmonic component values");

            double frequency = newValues[0];
            double amplitude = newValues[1];
            double phase = newValues[2];

            function.Clear();
            function[frequency] = !isCorrected
                                      ? new[]
                                      {
                                          amplitude,
                                          phase
                                      }
                                      : new[]
                                      {
                                          amplitude,
                                          phase,
                                          newValues[3], // Amplitude correction
                                          newValues[4]  // Phase correction
                                      };

            function.EndEdit();
            return true;
        }

        private static bool ApplyAstroComponentSelection(string[] components, IFunction function)
        {
            IVariable variable = function.Arguments.FirstOrDefault();
            if (!(variable is IVariable<string>))
            {
                return false;
            }

            function.BeginEdit("Generate/modify astro component values");

            List<string> previousValues = variable.Values.Cast<string>().ToList();
            foreach (string value in previousValues.Except(components))
            {
                variable.Values.Remove(value);
            }

            variable.AddValues(components.Except(previousValues));

            function.EndEdit();

            return true;
        }

        private void AstroComponentsDialog()
        {
            var selectDialog = new AstroComponentSelection(AstroComponents);
            if (BoundaryConditionData != null)
            {
                IVariable argumentVariable = BoundaryConditionData.Arguments.FirstOrDefault();
                if (argumentVariable is IVariable<string>)
                {
                    selectDialog.SelectComponents(argumentVariable.Values.Cast<string>().ToList());
                }

                if (argumentVariable is IVariable<double>)
                {
                    var componentsInVariable = new List<string>();
                    foreach (double frequency in argumentVariable.GetValues<double>())
                    {
                        string component = AstroComponents.FirstOrDefault(kvp => kvp.Value == frequency).Key;
                        if (!string.IsNullOrEmpty(component))
                        {
                            componentsInVariable.Add(component);
                        }
                    }

                    selectDialog.SelectComponents(componentsInVariable);
                }
            }

            selectDialog.ShowDialog();
            if (selectDialog.DialogResult != DialogResult.OK)
            {
                return;
            }

            string[] selectedComponents = selectDialog.SelectedComponents.Select(kvp => kvp.Key).ToArray();

            ApplyBoundaryConditionsForSupportPointMode(selectedComponents, ApplyAstroComponentSelection,
                                                       "Generate/modify astro component values");
        }

        private void UpdateFunctionViewMode()
        {
            if (mode == ViewMode.Single)
            {
                boundaryDataSplitContainer.Panel1Collapsed = true;
                functionView.ShowTableView = true;
                drawButton.Text = "Combined BC view";
                if (!functionView.ChartView.Tools.Contains(addSeriesTool))
                {
                    functionView.ChartView.Tools.Add(addSeriesTool);
                }
            }
            else
            {
                boundaryDataSplitContainer.Panel1Collapsed = false;
                functionView.ShowTableView = false;
                drawButton.Text = "Single BC view";
                if (functionView.ChartView.Tools.Contains(addSeriesTool))
                {
                    functionView.ChartView.Tools.Remove(addSeriesTool);
                }

                FillCheckedListBox();
            }
        }

        private void DrawButtonClick(object sender, EventArgs e)
        {
            Mode = Mode == ViewMode.Single ? ViewMode.Combined : ViewMode.Single;
        }

        private void FileImportButtonClick(object sender, EventArgs e)
        {
            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogService, BoundaryCondition as FlowBoundaryCondition,
                                                                 SupportPointIndex, Model.ReferenceTime);
            FillFunctionView();
        }

        private void FileExportButtonClick(object sender, EventArgs e)
        {
            try
            {
                BoundaryConditionDialogLauncher.LaunchExporterDialog(SaveFileDialog,
                                                                     BoundaryCondition as FlowBoundaryCondition,
                                                                     SupportPointIndex, Model.ReferenceTime);
            }
            catch (NotSupportedException exception)
            {
                log.ErrorFormat(exception.Message);
            }
        }

        private class AddSeriesTool : IChartViewContextMenuTool
        {
            public readonly IList<IBoundaryCondition> AddedBoundaryConditions = new List<IBoundaryCondition>();

            public Action<IBoundaryCondition> AddSeriesToView;
            public Action<IBoundaryCondition> RemoveSeriesFromView;
            private bool active;
            public event EventHandler<EventArgs> ActiveChanged;

            public BoundaryConditionSet BoundaryConditionSet { get; set; }
            public IBoundaryCondition BoundaryCondition { get; set; }
            public int SelectedIndex { get; set; }

            public IChartView ChartView { get; set; }

            public bool Active
            {
                get => active;
                set
                {
                    active = value;
                    ActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool Enabled { get; set; }

            public void OnBeforeContextMenu(ContextMenuStrip menu)
            {
                List<IGrouping<string, IBoundaryCondition>> procsToAdd =
                    BoundaryConditionSet.BoundaryConditions.Except(
                                            AddedBoundaryConditions.Concat(new[]
                                            {
                                                BoundaryCondition
                                            }))
                                        .Where(bc => bc.DataPointIndices.Contains(SelectedIndex))
                                        .GroupBy(bc => bc.ProcessName).ToList();

                if (procsToAdd.Any())
                {
                    if (menu.Items.Count > 0)
                    {
                        menu.Items.Add(new ToolStripSeparator());
                    }

                    menu.Items.Add(new ToolStripMenuItem("Add series", null,
                                                         procsToAdd.Select(
                                                                       g => CreateMenuItem(
                                                                           g, AddBoundaryConditionSeries))
                                                                   .ToArray()));
                }

                List<IGrouping<string, IBoundaryCondition>> procsToRemove =
                    AddedBoundaryConditions.GroupBy(bc => bc.ProcessName).ToList();

                if (procsToRemove.Any())
                {
                    menu.Items.Add(new ToolStripMenuItem("Remove series", null,
                                                         procsToRemove.Select(
                                                                          g => CreateMenuItem(
                                                                              g, RemoveBoundaryConditionSeries))
                                                                      .ToArray()));
                }
            }

            private ToolStripItem CreateMenuItem(IGrouping<string, IBoundaryCondition> grouping,
                                                 EventHandler eventHandler)
            {
                return new ToolStripMenuItem(grouping.Key, null,
                                             grouping.Select(
                                                         bc =>
                                                             new ToolStripMenuItem(bc.Name, null, eventHandler))
                                                     .Cast<ToolStripItem>()
                                                     .ToArray());
            }

            private void AddBoundaryConditionSeries(object sender, EventArgs e)
            {
                IBoundaryCondition boundaryCondition =
                    BoundaryConditionSet.BoundaryConditions.FirstOrDefault(
                        bc => bc.Name == ((ToolStripMenuItem) sender).Text);
                AddedBoundaryConditions.Add(boundaryCondition);
                if (AddSeriesToView != null)
                {
                    AddSeriesToView(boundaryCondition);
                }
            }

            private void RemoveBoundaryConditionSeries(object sender, EventArgs e)
            {
                IBoundaryCondition boundaryCondition =
                    BoundaryConditionSet.BoundaryConditions.FirstOrDefault(
                        bc => bc.Name == ((ToolStripMenuItem) sender).Text);
                AddedBoundaryConditions.Remove(boundaryCondition);
                RemoveSeriesFromView?.Invoke(boundaryCondition);
            }
        }
    }
}