using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class SourceAndSinkView : UserControl, IReusableView, ICompositeView
    {
        private SourceAndSink sourceAndSink;
        private bool locked;
        private WaterFlowFMModel model;

        public SourceAndSinkView()
        {
            InitializeComponent();
            areaTextBox.Validating += AreaTextBoxOnValidating;
            areaTextBox.CausesValidation = true;
            functionView.CausesValidation = true;
            ChildViews = new EventedList<IView>(new[] {functionView});
        }

        private void AreaTextBoxOnValidating(object sender, CancelEventArgs e)
        {
            var text = ((TextBox) sender).Text;
            double value;
            if (double.TryParse(text, out value) && 
                value > 0 && 
                value < 1e+6)
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

        public FunctionView FunctionView
        {
            get { return functionView; }
        }

        private void FillAreaPanel()
        {
            if (sourceAndSink == null) return;
            areaTextBox.Text = sourceAndSink.Area.ToString();
            areaTextBox.Enabled = sourceAndSink.CanIncludeMomentum && sourceAndSink.MomentumSource;
            includeMomentumCheckBox.Enabled = sourceAndSink.CanIncludeMomentum;
            includeMomentumCheckBox.Checked = sourceAndSink.MomentumSource;
        }

        private SourceAndSink SourceAndSink
        {
            get { return sourceAndSink; }
            set
            {
                FunctionView.Data = null;
                if (sourceAndSink != null)
                {
                    ((INotifyPropertyChange)sourceAndSink).PropertyChanged -= OnPropertyChanged;
                }
                sourceAndSink = value;
                if (sourceAndSink != null)
                {
                    ((INotifyPropertyChange)sourceAndSink).PropertyChanged += OnPropertyChanged;
                    FunctionView.Data = sourceAndSink.Function;
                }
                FillAreaPanel();
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            FillAreaPanel();
        }

        public WaterFlowFMModel Model
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChange)model).PropertyChanged -= ModelPropertyChanged;
                }
                model = value;
                if (model != null)
                {
                    var visibilitySettings = CalculateComponentVisibilitySettings();
                    SetVisibility(visibilitySettings);
                    ((INotifyPropertyChange) model).PropertyChanged += ModelPropertyChanged;
                }
            }
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.UseSalinity) ||
                e.PropertyName == nameof(Model.HeatFluxModelType) ||
                e.PropertyName == nameof(Model.UseMorSed) ||
                e.PropertyName == nameof(Model.UseSecondaryFlow)
            )
            {
                var visibilitySettings = CalculateComponentVisibilitySettings();
                SetVisibility(visibilitySettings);
            }
        }

        private IList<bool> CalculateComponentVisibilitySettings()
        {
            var componentIsForTracer = false;
            var visibilitySettings = new List<bool>();

            foreach (var componentName in SourceAndSink.Function.Components.Select(c => c.Name))
            {
                switch (componentName)
                {
                    case SourceAndSink.DischargeVariableName:
                        visibilitySettings.Add(true);
                        break;
                    case SourceAndSink.SalinityVariableName:
                        visibilitySettings.Add(Model.UseSalinity);
                        break;
                    case SourceAndSink.TemperatureVariableName:
                        visibilitySettings.Add(Model.UseTemperature);
                        break;
                    case SourceAndSink.SecondaryFlowVariableName:
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
            int k = 1;
            for (int i = 0; i < SourceAndSink.Function.Components.Count; i++)
            {
                var component = SourceAndSink.Function.Components[i];
                var visibility = i >= visibilitySettings.Count || visibilitySettings[i];

                var columnIndex = i + 1;
                if (columnIndex < FunctionView.TableView.Columns.Count)
                {
                    var tableViewColumn = FunctionView.TableView.Columns[columnIndex];
                    tableViewColumn.Visible = visibility;
                    var chartSeries = FunctionView.ChartSeries.FirstOrDefault(s => s.YValuesDataMember == component.DisplayName);
                    if(chartSeries != null) chartSeries.Visible = visibility;

                    if (tableViewColumn.Visible)
                        tableViewColumn.DisplayIndex = k++;
                }
            }
            FunctionView.TableView.GetColumnByName(SourceAndSink.Function.Arguments[0].Name).DisplayIndex = 0;
            FunctionView.TableView.BestFitColumns(false);
        }

        public object Data
        {
            get { return SourceAndSink; }
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

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        public bool Locked
        {
            get { return locked; }
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

        public IEventedList<IView> ChildViews { get; private set; }

        public bool HandlesChildViews
        {
            get { return true; }
        }

        public void ActivateChildView(IView childView)
        {
        }

        private void includeMomentumCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (includeMomentumCheckBox.Checked)
            {
                if (!(sourceAndSink.Area > 0))
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
