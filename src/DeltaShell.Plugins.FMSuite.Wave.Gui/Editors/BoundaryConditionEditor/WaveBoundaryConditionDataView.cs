using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    public partial class WaveBoundaryConditionDataView : UserControl, ICompositeView
    {
        private readonly WaveSpectralParametersEditor spectralParametersEditor = new WaveSpectralParametersEditor();
        private readonly WaveSpectrumFileSelection spectralFileSelection = new WaveSpectrumFileSelection();
        private readonly FunctionView functionView = new FunctionView();

        private readonly WaveConstantParametersEditor constantParametersEditor = new WaveConstantParametersEditor();

        public WaveBoundaryConditionDataView()
        {
            InitializeComponent();

            functionView.Dock = DockStyle.Fill;
            functionView.ChartView.Chart.Legend.ShowCheckBoxes = true;
        }

        private WaveBoundaryCondition boundaryCondition;

        public object Data
        {
            get => boundaryCondition;
            set
            {
                if (boundaryCondition != null)
                {
                    ((INotifyPropertyChange) boundaryCondition).PropertyChanged -= OnBoundaryConditionPropertyChanged;
                }

                boundaryCondition = value as WaveBoundaryCondition;
                if (boundaryCondition != null)
                {
                    ((INotifyPropertyChange) boundaryCondition).PropertyChanged += OnBoundaryConditionPropertyChanged;
                }

                FullRefresh();
            }
        }

        private DateTime StartTime => Model != null ? Model.ModelDefinition.ModelReferenceDateTime : DateTime.Today;

        private DateTime StopTime => StartTime.AddDays(1);

        private TimeSpan Timestep => new TimeSpan(1, 0, 0, 0);

        public int SelectedPointIndex { get; set; }

        public WaveModel Model { get; set; }

        private Func<string, string> importIntoModelDirectory;

        public Func<string, string> ImportIntoModelDirectory
        {
            private get
            {
                return importIntoModelDirectory;
            }
            set
            {
                importIntoModelDirectory = value;
                spectralFileSelection.ImportIntoDirectory = importIntoModelDirectory;
            }
        }

        private void FullRefresh()
        {
            spectralPanel.Controls.Clear();
            functionViewPanel.Controls.Clear();
            buttonPanel.Visible = false;

            if (boundaryCondition == null)
            {
                return;
            }

            if (boundaryCondition.DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                spectralFileSelection.Data = boundaryCondition;
                spectralFileSelection.SelectedPointIndex = SelectedPointIndex;
                spectralPanel.Controls.Add(spectralFileSelection);
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                spectralParametersEditor.Data = boundaryCondition;
                spectralPanel.Controls.Add(spectralParametersEditor);
                constantParametersEditor.Data = boundaryCondition.DataPointIndices.Contains(SelectedPointIndex)
                                                    ? boundaryCondition.SpectrumParameters[SelectedPointIndex]
                                                    : null;
                functionViewPanel.Controls.Add(constantParametersEditor);
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                spectralParametersEditor.Data = boundaryCondition;
                spectralPanel.Controls.Add(spectralParametersEditor);
                functionView.Data = null;
                functionView.Data = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                functionView.ChartView.Chart.LeftAxis.Automatic = true;
                functionView.TableView.PasteController = new TableViewArgumentBasedPasteController(
                    (TableView) functionView.TableView, new[]
                    {
                        0
                    }) {SkipRowsWithMissingArgumentValues = true};
                functionViewPanel.Controls.Add(functionView);
                buttonPanel.Visible = true;
            }
        }

        private void OnBoundaryConditionPropertyChanged(object sender,
                                                        PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName ==
                TypeUtils.GetMemberName(() => boundaryCondition.SpatialDefinitionType))
            {
                if (boundaryCondition.IsHorizontallyUniform)
                {
                    SelectedPointIndex = 0;
                }

                UpdateDataView();
            }

            if (propertyChangedEventArgs.PropertyName == TypeUtils.GetMemberName(() => boundaryCondition.DataType))
            {
                FullRefresh();
            }
        }

        public void OnSelectedPointChanged(object sender, EventArgs<int> e)
        {
            SelectedPointIndex = e.Value;
            UpdateDataView();
        }

        private void UpdateDataView()
        {
            if (boundaryCondition == null)
            {
                spectralFileSelection.SelectedPointIndex = -1;
                functionView.Data = null;
                return;
            }

            if (boundaryCondition.DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                spectralFileSelection.SelectedPointIndex = SelectedPointIndex;
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                constantParametersEditor.Data = boundaryCondition.DataPointIndices.Contains(SelectedPointIndex)
                                                    ? boundaryCondition.SpectrumParameters[SelectedPointIndex]
                                                    : null;
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                functionView.Data = null;
                functionView.Data = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                functionView.TableView.PasteController = new TableViewArgumentBasedPasteController(
                    (TableView) functionView.TableView, new[]
                    {
                        0
                    }) {SkipRowsWithMissingArgumentValues = true};
                functionView.ChartView.Chart.LeftAxis.Automatic = true;
            }
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item) {}
        public ViewInfo ViewInfo { get; set; }

        public IEventedList<IView> ChildViews => functionView.ChildViews;

        public bool HandlesChildViews => true;

        public void ActivateChildView(IView childView) {}

        private void genDataButton_Click(object sender, EventArgs e)
        {
            TimeSeriesDialog();
        }

        private void TimeSeriesDialog()
        {
            var generateDialog = new TimeSeriesGeneratorDialog {ApplyOnAccept = false};
            generateDialog.SetData(null, StartTime, StopTime, Timestep);
            generateDialog.ShowDialog(this);

            if (generateDialog.DialogResult != DialogResult.OK)
            {
                return;
            }

            SupportPointMode supportPointOperationMode;
            if (boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform)
            {
                // When the spatial definition type of a wave boundary condition is equal to Uniform, there is only
                // one data point with a function (point data) on it. This point data represents the point data on
                // all points. Thus, only the data on the first point (which is always selected) should be altered.
                supportPointOperationMode = SupportPointMode.SelectedPoint;
            }
            else
            {
                var supportPointsDialog = new SupportPointSelectionForm();
                supportPointsDialog.ShowDialog(this);
                supportPointOperationMode = supportPointsDialog.SupportPointOperationMode;
            }

            functionView.Data = null;

            boundaryCondition.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));

            int numberOfCoordinates = boundaryCondition.Feature.Geometry.NumPoints;
            switch (supportPointOperationMode)
            {
                case SupportPointMode.NoPoints:
                    return;
                case SupportPointMode.SelectedPoint:
                    IFunction boundaryConditionData = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                    if (boundaryConditionData == null)
                    {
                        boundaryCondition.AddPoint(SelectedPointIndex);
                        boundaryConditionData = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                    }

                    boundaryConditionData.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                    generateDialog.Apply(boundaryConditionData
                                         .Arguments.OfType<IVariable<DateTime>>().FirstOrDefault());
                    boundaryConditionData.EndEdit();
                    break;
                case SupportPointMode.ActivePoints:
                    foreach (IFunction function in boundaryCondition.PointData)
                    {
                        function.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                        IVariable<DateTime> argument =
                            function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
                        if (!generateDialog.Apply(argument))
                        {
                            function.EndEdit();
                            break;
                        }

                        function.EndEdit();
                    }

                    break;
                case SupportPointMode.InactivePoints:

                    for (var i = 0; i < numberOfCoordinates; ++i)
                    {
                        if (boundaryCondition.DataPointIndices.Contains(i))
                        {
                            continue;
                        }

                        boundaryCondition.AddPoint(i);
                        IFunction function = boundaryCondition.GetDataAtPoint(i);

                        function.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                        IVariable<DateTime> argument =
                            function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
                        if (!generateDialog.Apply(argument))
                        {
                            function.EndEdit();
                            break;
                        }

                        function.EndEdit();
                    }

                    break;
                case SupportPointMode.AllPoints:

                    for (var i = 0; i < numberOfCoordinates; ++i)
                    {
                        if (!boundaryCondition.DataPointIndices.Contains(i))
                        {
                            boundaryCondition.AddPoint(i);
                        }

                        IFunction function = boundaryCondition.GetDataAtPoint(i);

                        function.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                        IVariable<DateTime> argument =
                            function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
                        if (!generateDialog.Apply(argument))
                        {
                            function.EndEdit();
                            break;
                        }

                        function.EndEdit();
                    }

                    break;
                default:
                    throw new NotImplementedException("Support point selection method not recognized.");
            }

            boundaryCondition.EndEdit();
            FullRefresh();
        }
    }
}