using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class SourceAndSinkView : UserControl, IReusableView, ICompositeView
    {
        private SourceAndSink sourceAndSink;
        private bool locked;
        private WaterFlowFMModel model;

        public event EventHandler LockedChanged;

        public SourceAndSinkView()
        {
            InitializeComponent();
            areaTextBox.Validating += AreaTextBoxOnValidating;
            areaTextBox.CausesValidation = true;
            functionView.CausesValidation = true;
            ChildViews = new EventedList<IView>(new[]
            {
                functionView
            });
        }

        public FunctionView FunctionView => functionView;

        public WaterFlowFMModel Model
        {
            get => model;
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChange) model).PropertyChanged -= ModelPropertyChanged;
                }

                model = value;
                if (model != null)
                {
                    IList<bool> visibilitySettings = CalculateComponentVisibilitySettings();
                    SetVisibility(visibilitySettings);
                    ((INotifyPropertyChange) model).PropertyChanged += ModelPropertyChanged;
                }
            }
        }

        public IEventedList<IView> ChildViews { get; private set; }

        public bool HandlesChildViews => true;

        public object Data
        {
            get => SourceAndSink;
            set
            {
                SourceAndSink = value as SourceAndSink;
                if (SourceAndSink == null)
                {
                    Model = null;
                }
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public bool Locked
        {
            get => locked;
            set
            {
                locked = value;
                LockedChanged?.Invoke(this, new EventArgs());
            }
        }

        public void ActivateChildView(IView childView)
        {
            // Nothing to be done, enforced through ICompositeView
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        private SourceAndSink SourceAndSink
        {
            get => sourceAndSink;
            set
            {
                FunctionView.Data = null;
                if (sourceAndSink != null)
                {
                    ((INotifyPropertyChange) sourceAndSink).PropertyChanged -= OnPropertyChanged;
                }

                sourceAndSink = value;
                if (sourceAndSink != null)
                {
                    ((INotifyPropertyChange) sourceAndSink).PropertyChanged += OnPropertyChanged;
                    FunctionView.Data = sourceAndSink.Function;
                }

                FillAreaPanel();
            }
        }

        private void AreaTextBoxOnValidating(object sender, CancelEventArgs e)
        {
            string text = ((TextBox) sender).Text;
            if (double.TryParse(text, out double value) && value > 0 && value < 1e+6)
            {
                sourceAndSink.Area = value;
                errorProvider1.Clear();
                errorProvider1.SetError(areaTextBox, "");
                e.Cancel = false;
                return;
            }

            errorProvider1.Clear();
            errorProvider1.SetError(areaTextBox, "Choose an area between zero and 1e+6");
            e.Cancel = true;
        }

        private void FillAreaPanel()
        {
            if (sourceAndSink == null)
            {
                return;
            }

            areaTextBox.Text = sourceAndSink.Area.ToString();
            areaTextBox.Enabled = sourceAndSink.CanIncludeMomentum && sourceAndSink.MomentumSource;
            includeMomentumCheckBox.Enabled = sourceAndSink.CanIncludeMomentum;
            includeMomentumCheckBox.Checked = sourceAndSink.MomentumSource;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            FillAreaPanel();
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.UseSalinity) ||
                e.PropertyName == nameof(Model.HeatFluxModelType) ||
                e.PropertyName == nameof(Model.UseMorSed) ||
                e.PropertyName == nameof(Model.UseSecondaryFlow))
            {
                IList<bool> visibilitySettings = CalculateComponentVisibilitySettings();
                SetVisibility(visibilitySettings);
            }
        }

        private IList<bool> CalculateComponentVisibilitySettings()
        {
            var componentIsForTracer = false;
            var visibilitySettings = new List<bool>();

            foreach (string componentName in SourceAndSink.Function.Components.Select(c => c.Name))
            {
                switch (componentName)
                {
                    case SourceSinkVariableInfo.DischargeVariableName:
                        visibilitySettings.Add(true);
                        break;
                    case SourceSinkVariableInfo.SalinityVariableName:
                        visibilitySettings.Add(Model.UseSalinity);
                        break;
                    case SourceSinkVariableInfo.TemperatureVariableName:
                        visibilitySettings.Add(Model.UseTemperature);
                        break;
                    case SourceSinkVariableInfo.SecondaryFlowVariableName:
                        visibilitySettings.Add(Model.UseSecondaryFlow);
                        componentIsForTracer = true; // Tracers should come after SecondaryFlow
                        break;
                    default:
                        if (!componentIsForTracer && SourceAndSink.SedimentFractionNames.Contains(componentName))
                        {
                            visibilitySettings.Add(Model.UseMorSed);
                        }
                        else
                        {
                            visibilitySettings.Add(true);
                        }

                        break;
                }
            }

            return visibilitySettings;
        }

        private void SetVisibility(IList<bool> visibilitySettings)
        {
            FunctionView.TableView.GetColumnByName(SourceAndSink.Function.Arguments[0].Name).Visible = true;
            var k = 1;
            for (var i = 0; i < SourceAndSink.Function.Components.Count; i++)
            {
                IVariable component = SourceAndSink.Function.Components[i];
                bool visibility = i >= visibilitySettings.Count || visibilitySettings[i];

                int columnIndex = i + 1;
                if (columnIndex < FunctionView.TableView.Columns.Count)
                {
                    ITableViewColumn tableViewColumn = FunctionView.TableView.Columns[columnIndex];
                    tableViewColumn.Visible = visibility;
                    IChartSeries chartSeries = FunctionView.ChartSeries.FirstOrDefault(s => s.YValuesDataMember == component.DisplayName);
                    if (chartSeries != null)
                    {
                        chartSeries.Visible = visibility;
                    }

                    if (tableViewColumn.Visible)
                    {
                        tableViewColumn.DisplayIndex = k++;
                    }
                }
            }

            FunctionView.TableView.GetColumnByName(SourceAndSink.Function.Arguments[0].Name).DisplayIndex = 0;
            FunctionView.TableView.BestFitColumns(false);
        }

        private void includeMomentumCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (includeMomentumCheckBox.Checked)
            {
                if (sourceAndSink.Area <= 0)
                {
                    sourceAndSink.Area = 1;
                }
            }
            else
            {
                sourceAndSink.Area = 0;
            }
        }
    }
}