using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
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
            get { return boundaryCondition; }
            set
            {
                if (boundaryCondition != null)
                {
                    ((INotifyPropertyChange)boundaryCondition).PropertyChanged -= OnBoundaryConditionPropertyChanged;
                }
                boundaryCondition = value as WaveBoundaryCondition;
                if (boundaryCondition != null)
                {
                    ((INotifyPropertyChange)boundaryCondition).PropertyChanged += OnBoundaryConditionPropertyChanged;
                }

                FullRefresh();
            }
        }

        private DateTime ModelStartTime
        {
            get { return model != null ? model.StartTime : DateTime.MinValue; }
        }
        private DateTime ModelStopTime
        {
            get { return model != null ? model.StopTime : ModelStartTime.AddDays(1); }
        }
        private TimeSpan ModelTimestep
        {
            get { return new TimeSpan(0, 1, 0, 0); }
        }

        public int SelectedPointIndex { get; set; }

        private ITimeDependentModel model;
        public ITimeDependentModel Model
        {
            get { return model; }
            set { model = value; }
        }

        private Func<string, string> importIntoModelDirectory;
        public Func<string, string> ImportIntoModelDirectory
        {
            private get { return importIntoModelDirectory; }
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

            if (boundaryCondition == null) return;

            if (boundaryCondition.DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                spectralFileSelection.Data = boundaryCondition;
                spectralFileSelection.SelectedPointIndex = SelectedPointIndex;
                spectralPanel.Controls.Add(spectralFileSelection);
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                spectralParametersEditor.Data = boundaryCondition;
                spectralPanel.Controls.Add(spectralParametersEditor);
                constantParametersEditor.Data = boundaryCondition.DataPointIndices.Contains(SelectedPointIndex)
                                                    ? boundaryCondition.SpectrumParameters[SelectedPointIndex]
                                                    : null;
                functionViewPanel.Controls.Add(constantParametersEditor);
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
            {
                spectralParametersEditor.Data = boundaryCondition;
                spectralPanel.Controls.Add(spectralParametersEditor);
                functionView.Data = null;
                functionView.Data = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                functionView.ChartView.Chart.LeftAxis.Automatic = true;
                functionView.TableView.PasteController = new TableViewArgumentBasedPasteController((TableView)functionView.TableView, new[] { 0 })
                {
                    SkipRowsWithMissingArgumentValues = true
                };
                functionViewPanel.Controls.Add(functionView);
                buttonPanel.Visible = true;
            }
        }

        private void OnBoundaryConditionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == TypeUtils.GetMemberName(() => boundaryCondition.SpatialDefinitionType))
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
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                constantParametersEditor.Data = boundaryCondition.DataPointIndices.Contains(SelectedPointIndex)
                                                    ? boundaryCondition.SpectrumParameters[SelectedPointIndex]
                                                    : null;
            }
            else if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
            {
                functionView.Data = null;
                functionView.Data = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                functionView.TableView.PasteController = new TableViewArgumentBasedPasteController((TableView)functionView.TableView, new[] { 0 })
                {
                    SkipRowsWithMissingArgumentValues = true
                };
                functionView.ChartView.Chart.LeftAxis.Automatic = true;
            }
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item)
        {
        }
        public ViewInfo ViewInfo { get; set; }

        public IEventedList<IView> ChildViews
        {
            get { return functionView.ChildViews; }
        }

        public bool HandlesChildViews
        {
            get { return true; }
        }

        public void ActivateChildView(IView childView)
        {
        }

        private void genDataButton_Click(object sender, EventArgs e)
        {
            TimeSeriesDialog();
        }

        private void TimeSeriesDialog()
        {
            var generateDialog = new TimeSeriesGeneratorDialog { ApplyOnAccept = false };

            generateDialog.SetData(null, ModelStartTime, ModelStopTime, ModelTimestep);

            generateDialog.ShowDialog(this);

            if (generateDialog.DialogResult != DialogResult.OK)
            {
                return;
            }

            var supportPointsDialog = new SupportPointSelectionForm();
            supportPointsDialog.ShowDialog(this);

            functionView.Data = null;

            boundaryCondition.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
            
            var count = boundaryCondition.Feature.Geometry.Coordinates.Count();
            var boundaryConditionData = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
            switch (supportPointsDialog.SupportPointOperationMode)
            {
                case SupportPointMode.NoPoints:
                    return;
                case SupportPointMode.SelectedPoint:
                    if (boundaryConditionData == null)
                    {
                        boundaryCondition.AddPoint(SelectedPointIndex);
                        boundaryConditionData = boundaryCondition.GetDataAtPoint(SelectedPointIndex);
                    }
                    boundaryConditionData.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                    generateDialog.Apply(boundaryConditionData.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault());
                    boundaryConditionData.EndEdit();
                    break;
                case SupportPointMode.ActivePoints:
                    foreach (var function in boundaryCondition.PointData)
                    {
                        function.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                        var argument = function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
                        if (!generateDialog.Apply(argument))
                        {
                            function.EndEdit();
                            break;
                        }
                        function.EndEdit();
                    }
                    break;
                case SupportPointMode.InactivePoints:

                    for (var i = 0; i < count; ++i)
                    {
                        if (boundaryCondition.DataPointIndices.Contains(i)) continue;
                        boundaryCondition.AddPoint(i);
                        var function = boundaryCondition.GetDataAtPoint(i);
                        
                        function.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                        var argument = function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
                        if (!generateDialog.Apply(argument))
                        {
                            function.EndEdit();
                            break;
                        }
                        function.EndEdit();
                    }
                    break;
                case SupportPointMode.AllPoints:

                    for (var i = 0; i < count; ++i)
                    {
                        if (!boundaryCondition.DataPointIndices.Contains(i))
                        {
                            boundaryCondition.AddPoint(i);
                        }
                        var function = boundaryCondition.GetDataAtPoint(i);
                        
                        function.BeginEdit(new DefaultEditAction("Generate/modify timeseries"));
                        var argument = function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
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
