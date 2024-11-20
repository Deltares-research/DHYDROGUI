using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public sealed partial class FlowBoundaryConditionPropertiesControl : UserControl
    {
        private readonly string conditionTypePropertyName = nameof(FlowBoundaryCondition.FlowQuantity);
        private readonly string reflectionAlphaPropertyName = nameof(FlowBoundaryCondition.ReflectionAlpha);
        private readonly string forcingTypePropertyName = nameof(FlowBoundaryCondition.DataType);
        private readonly string verticalProfilePropertyName = nameof(FlowBoundaryCondition.VerticalInterpolationType);
        private readonly string offsetPropertyName = nameof(FlowBoundaryCondition.Offset);
        private readonly string factorPropertyName = nameof(FlowBoundaryCondition.Factor);
        private readonly string tracerNamePropertyName = nameof(FlowBoundaryCondition.TracerName);
        private readonly string sedimentFractionNamePropertyName = nameof(FlowBoundaryCondition.SedimentFractionName);
        private readonly string thatcherHarlemanPropertyName = nameof(FlowBoundaryCondition.ThatcherHarlemanTimeLag);

        private bool updatingView;
        private FlowBoundaryCondition boundaryCondition;

        public FlowBoundaryConditionPropertiesControl()
        {
            InitializeComponent();
            verticalInterpolationComboBox.Items.AddRange(
                Enum.GetValues(typeof(VerticalInterpolationType)).Cast<object>().ToArray());
            SubscribeEventHandlers();
            TimeZoneTextBox.Text = TimeSpan.Zero.ToString();
        }

        public FlowBoundaryCondition BoundaryCondition
        {
            private get
            {
                return boundaryCondition;
            }
            set
            {
                if (BoundaryCondition != null)
                {
                    ((INotifyPropertyChanged) BoundaryCondition).PropertyChanged -= BoundaryDataPropertyChanged;
                }

                boundaryCondition = value;
                UpdateTimeZoneTextBoxForBoundaryCondition();
                UpdateDataTypeComboBoxForBoundaryCondition();

                if (BoundaryCondition != null)
                {
                    ((INotifyPropertyChanged) BoundaryCondition).PropertyChanged += BoundaryDataPropertyChanged;
                }

                UpdateView();
            }
        }

        private void UpdateTimeZoneTextBoxForBoundaryCondition()
        {
            TimeZoneTextBox.Text = boundaryCondition == null ? TimeSpan.Zero.ToString() : boundaryCondition.TimeZone.ToString();
        }

        private void UpdateDataTypeComboBoxForBoundaryCondition()
        {
            dataTypeComboBox.Items.Clear();

            if (boundaryCondition != null)
            {
                bcTypeLabel.Text = boundaryCondition.VariableDescription;
                List<BoundaryConditionDataType> supportedDataTypes = GetSupportedDataTypes(boundaryCondition.VariableName).ToList();
                if (supportedDataTypes.Any())
                {
                    dataTypeComboBox.Visible = true;
                    dataTypeComboBox.Items.AddRange(supportedDataTypes.Cast<object>().ToArray());
                }
                else
                {
                    dataTypeComboBox.Visible = false;
                }

                dataTypeComboBox.SelectedValueChanged -= DataTypeComboBoxOnSelectedValueChanged;
                dataTypeComboBox.SelectedItem = boundaryCondition.DataType;
                dataTypeComboBox.SelectedValueChanged += DataTypeComboBoxOnSelectedValueChanged;
            }
        }

        public FlowBoundaryConditionEditorController Controller { get; set; }

        private static void ComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((Enum) e.Value).GetDescription();
        }

        private void SubscribeEventHandlers()
        {
            verticalInterpolationComboBox.SelectionChangeCommitted += ComboBoxSelectionChangeCommitted;
            reflectionParameterTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitReflectionText();
                }
            };
            reflectionParameterTextBox.Validated += (s, e) => CommitReflectionText();
            factorTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitFactorText();
                }
            };
            factorTextBox.Validated += (s, e) => CommitFactorText();
            offsetTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitOffsetText();
                }
            };
            offsetTextBox.Validated += (s, e) => CommitOffsetText();
            verticalInterpolationComboBox.Format += ComboBoxFormat;
            thatcherTimeSpanEditor.ValueChanged += (s, e) => CommitThatcherHarlemanTimeLag();
        }

        private void UpdateView()
        {
            if (BoundaryCondition == null)
            {
                return;
            }

            updatingView = true;
            try
            {
                WaterFlowFMModel flowModel = Controller.Model;
                bool hasDepthLayers = flowModel != null && flowModel.UseDepthLayers;
                verticalInterpolationComboBox.Items.Clear();
                if (!BoundaryCondition.IsVerticallyUniform && hasDepthLayers)
                {
                    verticalInterpolationComboBox.Enabled = true;
                    verticalInterpolationTypeLabel.Enabled = true;
                    verticalInterpolationComboBox.Items.AddRange(
                        BoundaryCondition.SupportedVerticalInterpolationTypes.OfType<object>().ToArray());
                    verticalInterpolationComboBox.SelectedItem = BoundaryCondition.VerticalInterpolationType;
                }
                else
                {
                    verticalInterpolationComboBox.Enabled = false;
                    verticalInterpolationTypeLabel.Enabled = false;
                }

                reflectionParameterTextBox.Enabled = BoundaryCondition.SupportsReflection;
                if (reflectionParameterTextBox.Enabled)
                {
                    reflectionParameterLabel.Enabled = true;
                    reflectionParameterTextBox.Text = string.Format("{0:0.00}", BoundaryCondition.ReflectionAlpha);
                    reflectionUnitLabel.Text = BoundaryCondition.ReflectionUnit.Symbol;
                }
                else
                {
                    reflectionParameterLabel.Enabled = false;
                    reflectionParameterTextBox.Text = "";
                    reflectionUnitLabel.Text = "";
                }

                thatcherTimeSpanEditor.Enabled = BoundaryCondition.SupportsThatcherHarleman;
                thatcherTimeSpanLabel.Enabled = thatcherTimeSpanEditor.Enabled;
                if (thatcherTimeSpanEditor.Enabled)
                {
                    thatcherTimeSpanEditor.Value = BoundaryCondition.ThatcherHarlemanTimeLag;
                }
                else
                {
                    thatcherTimeSpanEditor.Value = TimeSpan.Zero;
                }

                factorTextBox.Text = string.Format("{0:0.00}", BoundaryCondition.Factor);
                offsetTextBox.Text = string.Format("{0:0.00}", BoundaryCondition.Offset);
                offsetUnitLabel.Text = BoundaryCondition.VariableUnit.Symbol;
                if (BoundaryCondition.DataType == BoundaryConditionDataType.Empty)
                {
                    factorTextBox.Enabled = false;
                    offsetTextBox.Enabled = false;
                    offsetUnitLabel.Enabled = false;
                }
            }
            finally
            {
                updatingView = false;
            }
        }

        private void BoundaryDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == forcingTypePropertyName || e.PropertyName == verticalProfilePropertyName ||
                e.PropertyName == conditionTypePropertyName || e.PropertyName == reflectionAlphaPropertyName ||
                e.PropertyName == factorPropertyName || e.PropertyName == offsetPropertyName || e.PropertyName == tracerNamePropertyName ||
                e.PropertyName == thatcherHarlemanPropertyName || e.PropertyName == sedimentFractionNamePropertyName)
            {
                UpdateView();
            }
        }

        private void CommitReflectionText()
        {
            if (BoundaryCondition == null || updatingView)
            {
                return;
            }

            double reflectionAlpha;
            if (double.TryParse(reflectionParameterTextBox.Text, out reflectionAlpha))
            {
                BoundaryCondition.ReflectionAlpha = reflectionAlpha;
            }
        }

        private void CommitFactorText()
        {
            if (BoundaryCondition == null || updatingView)
            {
                return;
            }

            double factor;
            if (double.TryParse(factorTextBox.Text, out factor))
            {
                BoundaryCondition.Factor = factor;
            }
        }

        private void CommitThatcherHarlemanTimeLag()
        {
            if (BoundaryCondition == null || updatingView)
            {
                return;
            }

            BoundaryCondition.ThatcherHarlemanTimeLag = thatcherTimeSpanEditor.Value;
        }

        private void CommitOffsetText()
        {
            if (BoundaryCondition == null || updatingView)
            {
                return;
            }

            if (double.TryParse(offsetTextBox.Text, out var offset))
            {
                BoundaryCondition.Offset = offset;
            }
        }

        private void ComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            if (BoundaryCondition != null && sender == verticalInterpolationComboBox)
            {
                BoundaryCondition.VerticalInterpolationType =
                    (VerticalInterpolationType) verticalInterpolationComboBox.SelectedItem;
            }
        }

        private IEnumerable<BoundaryConditionDataType> GetSupportedDataTypes(string variable)
        {
            return Controller.GetSupportedDataTypesForVariable(variable);
        }

        private bool ShowMessageBoxUponChangeDataType(BoundaryConditionDataType targetDataType)
        {
            return !BoundaryDataConverter.CanConvert(BoundaryCondition.DataType, targetDataType) &&
                   boundaryCondition.PointData.Any(f => f.Components.Any(v => v.Values.Count != 0));
        }
        
        private void DataTypeComboBoxOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            if (boundaryCondition != null)
            {
                var boundaryConditionDataType = (BoundaryConditionDataType) dataTypeComboBox.SelectedItem;
                if (boundaryConditionDataType != boundaryCondition.DataType)
                {
                    if (ShowMessageBoxUponChangeDataType(boundaryConditionDataType))
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            "All data for this boundary condition will be removed. Continue?", "Change forcing type",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (dialogResult == DialogResult.Yes)
                        {
                            boundaryCondition.DataType = boundaryConditionDataType;
                        }
                        else
                        {
                            dataTypeComboBox.SelectedItem = boundaryCondition.DataType;
                        }
                    }
                    else
                    {
                        boundaryCondition.DataType = boundaryConditionDataType;
                    }
                }
            }
        }
    }
}