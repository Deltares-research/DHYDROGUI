using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class BoundaryConditionPropertiesControl : UserControl
    {
        private IBoundaryCondition boundaryCondition;

        protected BoundaryConditionPropertiesControl()
        {
            InitializeComponent();
        }

        public virtual IBoundaryCondition BoundaryCondition
        {
            protected get
            {
                return boundaryCondition;
            }
            set
            {
                boundaryCondition = value;
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
        }

        public BoundaryConditionEditorController Controller { get; set; }

        protected virtual IEnumerable<BoundaryConditionDataType> GetSupportedDataTypes(string variable)
        {
            yield break;
        }

        protected virtual bool ShowMessageBoxUponChangeDataType(BoundaryConditionDataType targetDataType)
        {
            return boundaryCondition.PointData.Any(f => f.Components.Any(v => v.Values.Count != 0));
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