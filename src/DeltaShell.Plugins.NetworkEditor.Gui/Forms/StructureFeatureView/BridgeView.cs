using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class BridgeView : UserControl, IView
    {
        private IBridge bridge;

        public BridgeView()
        {
            InitializeComponent();
            bridgeTypeCombobox.DataSource = Enum.GetValues(typeof(BridgeType));
            tableViewTabulatedData.PasteController = new TableViewArgumentBasedPasteController(tableViewTabulatedData, new List<int> { 0 });
            tableViewTabulatedData.InputValidator += InputValidator;
        }

        public new void Dispose()
        {
            Data = null;
            tableViewTabulatedData.Dispose();
            base.Dispose();
        }

        private void SubscribeEvents()
        {
            ((INotifyPropertyChanged)bridge).PropertyChanged += Bridge_PropertyChanged;
        }

        private void UnSubscribeEvents()
        {
            if (bridge != null)
            {
                ((INotifyPropertyChanged)bridge).PropertyChanged -= Bridge_PropertyChanged;
            }

            if (tableViewTabulatedData != null)
            {
                tableViewTabulatedData.FocusedRowChanged -= OnTableViewTabulatedDataOnFocusedRowChanged;
                tableViewTabulatedData.FocusedRowChanged -= OnTableViewYzDataOnFocusedRowChanged;
            }
        }

        void Bridge_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(bridge.BridgeType)))
            {
                switch (bridge.BridgeType)
                {
                    case BridgeType.YzProfile 
                        when tableViewTabulatedData.EditableObject != null &&
                             tableViewTabulatedData.EditableObject.Equals(bridge.TabulatedCrossSectionDefinition):
                    case BridgeType.Tabulated 
                        when tableViewTabulatedData.EditableObject != null &&
                             tableViewTabulatedData.EditableObject.Equals(bridge.YZCrossSectionDefinition):
                        SetTableData();
                        break;
                }
            }
            SyncUI();
        }

        /// <summary>
        /// Gets or sets data shown by this view. Usually it is any object in the system which can be shown by some IView derived class.
        /// </summary>
        public object Data
        {
            get { return bridge; }
            set
            {
                UnsubscribeFrictionTypeComboBox();
                UnSubscribeEvents();
                bridge = (IBridge) value;
                tableViewTabulatedData.Data = null;
                tableViewTabulatedData.EditableObject = null;

                bindingSourceBridge.DataSource = (object)bridge ?? typeof(Bridge);
                
                if (value == null)
                {
                    return;
                }

                SetComboboxDataSources();
                
                SetTableData();

                SubscribeEvents();
                SyncUI();
                SubscribeFrictionTypeComboBox();
            }
        }

        private void SubscribeFrictionTypeComboBox()
        {
            comboBoxFrictionType.SelectedValueChanged += ComboBoxFrictionTypeSelectedValueChanged;
        }

        private void UnsubscribeFrictionTypeComboBox()
        {
            comboBoxFrictionType.SelectedValueChanged -= ComboBoxFrictionTypeSelectedValueChanged;
        }

        private void SetTableData()
        {
            if (bridge.BridgeType == BridgeType.Tabulated)
            {
                if (bridge.TabulatedCrossSectionDefinition == null)
                {
                    return;
                }
                if (tableViewTabulatedData.EditableObject != null && tableViewTabulatedData.EditableObject.Equals(bridge.TabulatedCrossSectionDefinition))
                    tableViewTabulatedData.FocusedRowChanged -= OnTableViewTabulatedDataOnFocusedRowChanged;
                if (tableViewTabulatedData.EditableObject != null && (bridge.YZCrossSectionDefinition != null && tableViewTabulatedData.EditableObject.Equals(bridge.YZCrossSectionDefinition)))
                    tableViewTabulatedData.FocusedRowChanged -= OnTableViewYzDataOnFocusedRowChanged; 
                tableViewTabulatedData.Data = bridge.TabulatedCrossSectionDefinition.ZWDataTable;
                tableViewTabulatedData.EditableObject = bridge.TabulatedCrossSectionDefinition;
                tableViewTabulatedData.FocusedRowChanged += OnTableViewTabulatedDataOnFocusedRowChanged;
            }
            else if (bridge.BridgeType == BridgeType.YzProfile)
            {
                if (bridge.YZCrossSectionDefinition == null)
                {
                    return;
                }
                if (tableViewTabulatedData.EditableObject != null && (bridge.TabulatedCrossSectionDefinition != null && tableViewTabulatedData.EditableObject.Equals(bridge.TabulatedCrossSectionDefinition)))
                    tableViewTabulatedData.FocusedRowChanged -= OnTableViewTabulatedDataOnFocusedRowChanged;
                if (tableViewTabulatedData.EditableObject != null && tableViewTabulatedData.EditableObject.Equals(bridge.YZCrossSectionDefinition))
                    tableViewTabulatedData.FocusedRowChanged -= OnTableViewYzDataOnFocusedRowChanged;
                tableViewTabulatedData.Data = bridge.YZCrossSectionDefinition.YZDataTable;
                tableViewTabulatedData.EditableObject = bridge.YZCrossSectionDefinition;
                tableViewTabulatedData.BestFitColumns();
                tableViewTabulatedData.FocusedRowChanged += OnTableViewYzDataOnFocusedRowChanged;
            }
            

            for (int i = 0; i < tableViewTabulatedData.Columns.Count; i++)
            {
                if (i > 1)
                {
                    tableViewTabulatedData.Columns[i].Visible = false; //hide storage columns
                }
            }
        }

        private void OnTableViewTabulatedDataOnFocusedRowChanged(object sender, EventArgs e)
        {
            bridge.IsTabulated = true;
        }

        private void OnTableViewYzDataOnFocusedRowChanged(object sender, EventArgs e)
        {
            bridge.IsYz = true;
        }

        private void SetComboboxDataSources()
        {
            SetFrictionComboboxDataSource(comboBoxFrictionType);
        }

        private void SetFrictionComboboxDataSource(ComboBox comboBox)
        {
            var enumVals = (from BridgeFrictionType value in Enum.GetValues(typeof (BridgeFrictionType)) 
                            select new ListItem<BridgeFrictionType>(value, GetFrictionDescription(value))).ToList();

            comboBox.ValueMember = "Key";
            comboBox.DisplayMember = "Description";
            comboBox.DataSource = enumVals;
        }

        private static string GetFrictionDescription(BridgeFrictionType value)
        {
            switch (value)
            {
                case BridgeFrictionType.Chezy:
                    return "Chezy (C)";
                case BridgeFrictionType.Manning:
                    return "Manning (nm)";
                case BridgeFrictionType.StricklerKn:
                    return "Strickler (kn)";
                case BridgeFrictionType.StricklerKs:
                    return "Strickler (ks)";
                case BridgeFrictionType.WhiteColebrook:
                    return "White-Colebrook";
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        private void SyncUI()
        {
            // no databinding here yet..not in IBridge..
            SetBridgeGUI();
            SetFrictionUnitLabels();
        }

        private void SetBridgeGUI()
        {
            bridgeTypeCombobox.SelectedItem = bridge.BridgeType;
            tableViewTabulatedData.Visible = bridge.BridgeType == BridgeType.Tabulated || bridge.BridgeType == BridgeType.YzProfile;
            var isPillar = bridge.IsPillar;
            var isRectangle = bridge.BridgeType == BridgeType.Rectangle;

            geometrySplitContainer.SplitterDistance = isPillar
                                                          ? 0
                                                          : geometrySplitContainer.Width;

            if (!isPillar)
            {
                splitContainerGeometry.SplitterDistance = isRectangle
                                                              ? splitContainerGeometry.Width
                                                              : 0;
            }

            labelShift.Enabled = true;
            labelWidth.Enabled = isRectangle;
            labelHeight.Enabled = isRectangle;
            textBoxShift.Enabled = true;
            textBoxWidth.Enabled = isRectangle;
            textBoxHeight.Enabled = isRectangle;
            textBoxPillarWidth.Enabled = isPillar;
            textBoxShapeFactor.Enabled = isPillar;

            roughnessGroupBox.Enabled = !isPillar;

            textBoxLength.Enabled = !isPillar;
            textBoxOutletLoss.Enabled = !isPillar;
            textBoxInletLoss.Enabled = !isPillar;
        }

        private DelftTools.Utils.Tuple<string, bool> InputValidator(TableViewCell cell, object o)     
        {
            var sortedRowIndex = tableViewTabulatedData.GetDataSourceIndexByRowIndex(cell.RowIndex);

            if (sortedRowIndex < 0 || sortedRowIndex >= tableViewTabulatedData.RowCount || bridge == null)
                return new DelftTools.Utils.Tuple<string, bool>("", true);

            return bridge.BridgeType == BridgeType.Tabulated 
                ? bridge.TabulatedCrossSectionDefinition.ValidateCellValue(sortedRowIndex, cell.Column.AbsoluteIndex, o)
                : bridge.YZCrossSectionDefinition.ValidateCellValue(sortedRowIndex, cell.Column.AbsoluteIndex, o);
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        private void ComboBoxFrictionTypeSelectedValueChanged(object sender, EventArgs e)
        {
            var oldValue = bridge.FrictionType;
            var newValue = (BridgeFrictionType) comboBoxFrictionType.SelectedValue;

            if (oldValue != newValue)
            {
                var roughness = RoughnessHelper.GetDefault(newValue).ToString();
                textBoxGroundLayerRoughnessValue.Text = roughness;
                textBoxRoughnessValue.Text = roughness;
                bindingSourceBridge.EndEdit();
            }
            SetFrictionUnitLabels();
        }

        private void SetFrictionUnitLabels()
        {
            var frictionUnit = GetFrictionUnit(bridge.FrictionType);
            labelFrictionUnit.Text = frictionUnit;
            labelGroundLayerFrictionUnit.Text = frictionUnit;
        }

        private static string GetFrictionUnit(BridgeFrictionType type)
        {
            switch (type)
            {
                case BridgeFrictionType.Chezy:
                    return "m^1/2*s^-1";
                case BridgeFrictionType.Manning:
                    return "s*m^-1/3";
                case BridgeFrictionType.StricklerKn:
                    return "m";
                case BridgeFrictionType.StricklerKs:
                    return "m^1/3*s^-1";
                case BridgeFrictionType.WhiteColebrook:
                    return "m";
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        #region Validation

        private void textBoxPillarWidth_Validating(object sender, CancelEventArgs e)
        {
            double number = 0;
            try
            {
                number = double.Parse(textBoxPillarWidth.Text);
            }
            catch (Exception)
            {
                e.Cancel = true;
                textBoxPillarWidth.BackColor = Color.OrangeRed;
            }
            if (number < 0)
            {
                e.Cancel = true;
                textBoxPillarWidth.BackColor = Color.OrangeRed;
            }
        }

        private void textBoxPillarWidth_Validated(object sender, EventArgs e)
        {
            textBoxPillarWidth.BackColor = default;
        }

        #endregion
    }

    public class ListItem<T>
    {
        public ListItem(T key, string description)
        {
            Key = key;
            Description = description;
        }

        public T Key { get; set; }
        public string Description { get; set; }
    }
}