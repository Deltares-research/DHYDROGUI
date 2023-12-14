using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class BoundaryConditionPropertiesControl : UserControl
    {
        protected BoundaryConditionPropertiesControl()
        {
            InitializeComponent();
            TimeZoneTextBox.Text = TimeSpan.Zero.ToString();
        }

        private IBoundaryCondition boundaryCondition;
        
        public virtual IBoundaryCondition BoundaryCondition
        {
            protected get { return boundaryCondition; }
            set
            {
                boundaryCondition = value;
                dataTypeComboBox.Items.Clear();

                if (boundaryCondition != null)
                {
                    bcTypeLabel.Text = boundaryCondition.VariableDescription;
                    var supportedDataTypes = GetSupportedDataTypes(boundaryCondition.VariableName).ToList();
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
                UpdateTimeZoneTextBoxForBoundaryCondition();
            }
        }
        
        private void UpdateTimeZoneTextBoxForBoundaryCondition()
        {
            TimeZoneTextBox.Text = boundaryCondition == null ? TimeSpan.Zero.ToString() : boundaryCondition.TimeZone.ToString();
        }

        public BoundaryConditionEditorController Controller { get; set; }

        protected virtual IEnumerable<BoundaryConditionDataType> GetSupportedDataTypes(string variable)
        {
            yield break;
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
                        var dialogResult = MessageBox.Show(
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

        protected virtual bool ShowMessageBoxUponChangeDataType(BoundaryConditionDataType targetDataType)
        {
            return boundaryCondition.PointData.Any(f => f.Components.Any(v => v.Values.Count != 0));
        }
    }
}
