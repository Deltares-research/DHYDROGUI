using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public sealed partial class FlowBoundaryConditionPropertiesControl : BoundaryConditionPropertiesControl
    {
        private readonly string conditionTypePropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.FlowQuantity);

        private readonly string reflectionAlphaPropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.ReflectionAlpha);

        private readonly string forcingTypePropertyName =
            TypeUtils.GetMemberName<BoundaryCondition>(b => b.DataType);

        private readonly string verticalProfilePropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.VerticalInterpolationType);

        private readonly string offsetPropertyName = TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.Offset);

        private readonly string factorPropertyName = TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.Factor);

        private readonly string tracerNamePropertyName = TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.TracerName);

        private readonly string sedimentFractionNamePropertyName = TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.SedimentFractionName);

        private readonly string thatcherHarlemanPropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(b => b.ThatcherHarlemanTimeLag);

        private bool updatingView;

        private static void ComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            e.Value = EnumDescriptionAttributeTypeConverter.GetEnumDescription((Enum)e.Value);
        }

        public FlowBoundaryConditionPropertiesControl()
        {
            InitializeComponent();
            verticalInterpolationComboBox.Items.AddRange(
                Enum.GetValues(typeof(VerticalInterpolationType)).Cast<object>().ToArray());
            SubscribeEventHandlers();
        }

        private FlowBoundaryCondition FlowBoundaryCondition
        {
            get { return BoundaryCondition as FlowBoundaryCondition; }
        }

        public override IBoundaryCondition BoundaryCondition
        {
            protected get { return base.BoundaryCondition; }
            set
            {
                if (BoundaryCondition != null)
                {
                    ((INotifyPropertyChanged)BoundaryCondition).PropertyChanged -= BoundaryDataPropertyChanged;
                }

                base.BoundaryCondition = value as FlowBoundaryCondition;

                if (BoundaryCondition != null)
                {
                    ((INotifyPropertyChanged)BoundaryCondition).PropertyChanged += BoundaryDataPropertyChanged;
                }

                UpdateView();
            }
        }

        protected override IEnumerable<BoundaryConditionDataType> GetSupportedDataTypes(string variable)
        {
            return Controller.GetSupportedDataTypesForVariable(variable);
        }

        private void SubscribeEventHandlers()
        {
            verticalInterpolationComboBox.SelectionChangeCommitted += ComboBoxSelectionChangeCommitted;
            reflectionParameterTextBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) CommitReflectionText(); };
            reflectionParameterTextBox.Validated += (s, e) => CommitReflectionText();
            factorTextBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) CommitFactorText(); };
            factorTextBox.Validated += (s, e) => CommitFactorText();
            offsetTextBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) CommitOffsetText(); };
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
                var flowModel = ((FlowBoundaryConditionEditorController) Controller).Model;
                bool hasDepthLayers = flowModel != null && flowModel.UseDepthLayers;
                verticalInterpolationComboBox.Items.Clear();
                if (!FlowBoundaryCondition.IsVerticallyUniform && hasDepthLayers)
                {
                    verticalInterpolationComboBox.Enabled = true;
                    label3.Enabled = true;
                    verticalInterpolationComboBox.Items.AddRange(
                        FlowBoundaryCondition.SupportedVerticalInterpolationTypes.OfType<object>().ToArray());
                    verticalInterpolationComboBox.SelectedItem = FlowBoundaryCondition.VerticalInterpolationType;
                }
                else
                {
                    verticalInterpolationComboBox.Enabled = false;
                    label3.Enabled = false;
                }

                reflectionParameterTextBox.Enabled = FlowBoundaryCondition.SupportsReflection;
                if (reflectionParameterTextBox.Enabled)
                {
                    label4.Enabled = true;
                    reflectionParameterTextBox.Text = string.Format("{0:0.00}", FlowBoundaryCondition.ReflectionAlpha);
                    reflectionUnitLabel.Text = FlowBoundaryCondition.ReflectionUnit.Symbol;
                }
                else
                {
                    label4.Enabled = false;
                    reflectionParameterTextBox.Text = "";
                    reflectionUnitLabel.Text = "";
                }

                thatcherTimeSpanEditor.Enabled = FlowBoundaryCondition.SupportsThatcherHarleman;
                label8.Enabled = thatcherTimeSpanEditor.Enabled;
                if (thatcherTimeSpanEditor.Enabled)
                {
                    thatcherTimeSpanEditor.Value = FlowBoundaryCondition.ThatcherHarlemanTimeLag;    
                }
                else
                {
                    thatcherTimeSpanEditor.Value = TimeSpan.Zero;
                }
                

                factorTextBox.Text = string.Format("{0:0.00}", FlowBoundaryCondition.Factor);
                offsetTextBox.Text = string.Format("{0:0.00}", FlowBoundaryCondition.Offset);
                offsetUnitLabel.Text = FlowBoundaryCondition.VariableUnit.Symbol;
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
                e.PropertyName == thatcherHarlemanPropertyName || e.PropertyName == sedimentFractionNamePropertyName )
            {
                UpdateView();
            }
        }

        private void CommitReflectionText()
        {
            if (BoundaryCondition == null || updatingView)
                return;

            double reflectionAlpha;
            if (double.TryParse(reflectionParameterTextBox.Text, out reflectionAlpha))
            {
                FlowBoundaryCondition.ReflectionAlpha = reflectionAlpha;
            }
        }

        private void CommitFactorText()
        {
            if (BoundaryCondition == null || updatingView)
                return;

            double factor;
            if (double.TryParse(factorTextBox.Text, out factor))
            {
                FlowBoundaryCondition.Factor = factor;
            }
        }

        private void CommitThatcherHarlemanTimeLag()
        {
            if (BoundaryCondition == null || updatingView)
                return;

            FlowBoundaryCondition.ThatcherHarlemanTimeLag = thatcherTimeSpanEditor.Value;
        }

        private void CommitOffsetText()
        {
            if (BoundaryCondition == null || updatingView)
                return;

            double offset;
            if (double.TryParse(offsetTextBox.Text, out offset))
            {
                FlowBoundaryCondition.Offset = offset;
            }
        }

        private void ComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            if (BoundaryCondition != null && sender == verticalInterpolationComboBox)
            {
                FlowBoundaryCondition.VerticalInterpolationType =
                    (VerticalInterpolationType)verticalInterpolationComboBox.SelectedItem;
            }
        }

        protected override bool ShowMessageBoxUponChangeDataType(BoundaryConditionDataType targetDataType)
        {
            return !BoundaryDataConverter.CanConvert(BoundaryCondition.DataType, targetDataType) &&
                   base.ShowMessageBoxUponChangeDataType(targetDataType);
        }
    }
}
