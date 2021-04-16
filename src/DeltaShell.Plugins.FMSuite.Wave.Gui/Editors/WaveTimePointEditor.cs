using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Functions.Binding;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Binding;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveTimePointEditor : UserControl, ILayerEditorView
    {
        private readonly TableView dataTableView;

        private WaveInputFieldData data;
        public event EventHandler SelectedFeaturesChanged;

        public WaveTimePointEditor()
        {
            InitializeComponent();

            dataTableView = new TableView {Dock = DockStyle.Fill};
            dataTableView.PasteController = new TableViewArgumentBasedPasteController(dataTableView, new[]
            {
                0
            }) {SkipRowsWithMissingArgumentValues = true};
            dataTableView.ShowImportExportToolbar = true;

            IList<KeyValuePair<InputFieldDataType, string>> hydroDataSource =
                EnumBindingHelper.ToList<InputFieldDataType>();
            hydroDataSource.RemoveAllWhere(kvp => kvp.Key == InputFieldDataType.FromInputFiles);
            hydroComboBox.DataSource = hydroDataSource;
            hydroComboBox.DisplayMember = "Value";
            hydroComboBox.ValueMember = "Key";
            windComboBox.DataSource = EnumBindingHelper.ToList<InputFieldDataType>();
            windComboBox.DisplayMember = "Value";
            windComboBox.ValueMember = "Key";

            hydroComboBox.SelectedValueChanged += HydroComboBoxOnSelectedValueChanged;
            windComboBox.SelectedValueChanged += WindComboBoxOnSelectedValueChanged;
        }

        public Func<string, string> ImportFileIntoModelDirectory { private get; set; }

        public object Data
        {
            get => data;
            set
            {
                data = value as WaveInputFieldData;
                dataTableView.Data = data != null
                                         ? new FunctionBindingList(data.InputFields)
                                         : null;

                tablePanel.Controls.Clear();
                dataTableView.BestFitColumns();
                dataTableView.Columns[0].Width = 160;

                tablePanel.Controls.Add(dataTableView);

                UpdateBindings(data);

                waveMeteoDataEditor1.Data = data?.MeteoData;
                waveMeteoDataEditor1.ImportFileIntoModelDirectory = s => ImportFileIntoModelDirectory(s);

                if (data != null)
                {
                    RefreshPanels();
                }
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }
        public IEnumerable<IFeature> SelectedFeatures { get; set; }
        public ILayer Layer { get; set; }
        public void EnsureVisible(object item) {}

        public void OnActivated() {}

        public void OnDeactivated() {}

        private void WindComboBoxOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            RefreshPanels();
        }

        private void HydroComboBoxOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            RefreshPanels();
        }

        private void RefreshPanels()
        {
            var selHydroDataType = (InputFieldDataType) hydroComboBox.SelectedValue;
            var selWindDataType = (InputFieldDataType) windComboBox.SelectedValue;

            hydroGroupBox.Visible = selHydroDataType == InputFieldDataType.Constant;
            windGroupBox.Visible = selWindDataType == InputFieldDataType.Constant;
            meteoBox.Visible = selWindDataType == InputFieldDataType.FromInputFiles;

            splitContainer1.Panel2Collapsed = selWindDataType == InputFieldDataType.TimeVarying &&
                                              selHydroDataType == InputFieldDataType.TimeVarying;

            dataTableView.Columns[1].Visible = selHydroDataType == InputFieldDataType.TimeVarying;
            dataTableView.Columns[2].Visible = selHydroDataType == InputFieldDataType.TimeVarying;
            dataTableView.Columns[3].Visible = selHydroDataType == InputFieldDataType.TimeVarying;

            dataTableView.Columns[4].Visible = selWindDataType == InputFieldDataType.TimeVarying;
            dataTableView.Columns[5].Visible = selWindDataType == InputFieldDataType.TimeVarying;

            // ensures proper order (see TOOLS-9526)
            var displayIndex = 0;
            dataTableView.Columns.ForEach(c =>
            {
                if (c.Visible)
                {
                    c.DisplayIndex = displayIndex++;
                }
            });
        }

        private void UpdateBindings(WaveInputFieldData inputFieldData)
        {
            hydroComboBox.DataBindings.Clear();
            windComboBox.DataBindings.Clear();

            waterlevelBox.DataBindings.Clear();
            velocityXBox.DataBindings.Clear();
            velocityYBox.DataBindings.Clear();
            windSpeedBox.DataBindings.Clear();
            windDirectionBox.DataBindings.Clear();

            if (inputFieldData != null)
            {
                hydroComboBox.DataBindings.Add(new Binding("SelectedValue", inputFieldData,
                                                           nameof(inputFieldData.HydroDataType)));
                windComboBox.DataBindings.Add(new Binding("SelectedValue", inputFieldData,
                                                          nameof(inputFieldData.WindDataType)));

                waterlevelBox.DataBindings.Add(new Binding("Text", inputFieldData,
                                                           nameof(inputFieldData.WaterLevelConstant)));
                velocityXBox.DataBindings.Add(new Binding("Text", inputFieldData,
                                                          nameof(inputFieldData.VelocityXConstant)));
                velocityYBox.DataBindings.Add(new Binding("Text", inputFieldData,
                                                          nameof(inputFieldData.VelocityYConstant)));

                windSpeedBox.DataBindings.Add(new Binding("Text", inputFieldData,
                                                          nameof(inputFieldData.WindSpeedConstant)));
                windDirectionBox.DataBindings.Add(new Binding("Text", inputFieldData,
                                                              nameof(inputFieldData.WindDirectionConstant)));
            }
        }
    }
}