using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Web;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class BoundaryConditionWpsDialog : Form, IConfigureDialog, IView
    {
        public BoundaryConditionWpsDialog()
        {
            InitializeComponent();
            Title = "Import water level time series from WPS";
            importToExistingBCButton.Checked = true;
            importOnAllPointsButton.Checked = true;
        }

        public bool AllowCreateNewBoundaryCondition
        {
            set
            {
                createNewSeriesButton.Enabled = value;
                if (!value)
                {
                    createNewSeriesButton.Checked = false;
                }
            }
        }

        public bool AllowSelectedSupportPointImport
        {
            set
            {
                importOnSelectedPointButton.Checked = value;
                importOnSelectedPointButton.Enabled = value;
            }
        }

        public DateTime StartTime { private get; set; }

        public DateTime StopTime { private get; set; }

        public TimeSpan TimeStep { private get; set; }

        public ICoordinateSystem CoordinateSystem { get; set; }

        public BoundaryConditionWpsImporter CreateImporter()
        {
            var importer = new BoundaryConditionWpsImporter();
            Configure(importer);
            return importer;
        }

        private BoundaryConditionWpsImporter CreateTemporaryImporter()
        {
            return new BoundaryConditionWpsImporter {StartDate = StartTime, EndDate = StopTime, TimeStep = TimeStep};
        }

        private void FillControl()
        {
            startDatePicker.Value = StartTime;
            startTimePicker.Value = StartTime;
            endDatePicker.Value = StopTime;
            endTimePicker.Value = StopTime;

            var temporaryImporter = CreateTemporaryImporter();
            try
            {
                temporaryImporter.InitializeClient();
            }
            catch (Exception e)
            {
                processDescriptionLabel.Text = e.Message;
                okButton.Enabled = false;
                return;
            }
            serverPathLabel.Text = temporaryImporter.Client.Server.ToString();
            var link = temporaryImporter.Client.Server + "?Request=GetCapabilities&Service=wps";
            serverPathLabel.Links.Add(0, link.Length, link);
            processLabel.Text = temporaryImporter.Process;

            try
            {
                var process = temporaryImporter.Client.Processes.First(i => i.Id == temporaryImporter.Process);
                processDescriptionLabel.Text = process.Description;
                timeStepComboBox.Items.Clear();
                var input = process.Inputs.First(i => i.Id == "frequency") as WpsDataTypeLiteral;
                if (input != null)
                {
                    timeStepComboBox.Items.AddRange(input.AllowedValues.OfType<object>().ToArray());
                }
                timeStepComboBox.SelectedItem = temporaryImporter.Frequency;    
            }
            catch (Exception e)
            {
                processDescriptionLabel.Text = e.Message;
                okButton.Enabled = false;
            }
        }

        private void OkButtonClick(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void ServerPathLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void ImportToExistingBcButtonCheckedChanged(object sender, EventArgs e)
        {
            if (createNewSeriesButton.Checked)
            {
                importOnAllPointsButton.Checked = true;
                groupBox3.Enabled = false;
            }
            else
            {
                groupBox3.Enabled = true;
            }
        }

        public string Title { get; set; }

        public DelftDialogResult ShowModal()
        {
            FillControl();
            if (ShowDialog() == DialogResult.OK)
            {
                return DelftDialogResult.OK;
            }
            return DelftDialogResult.Cancel;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object obj)
        {
            var importer = obj as BoundaryConditionWpsImporter;

            if (importer != null)
            {
                importer.InitializeClient();

                importer.StartDate = new DateTime(startDatePicker.Value.Year, startDatePicker.Value.Month,
                                                  startDatePicker.Value.Day, startTimePicker.Value.Hour,
                                                  startTimePicker.Value.Minute, startTimePicker.Value.Second);

                importer.EndDate = new DateTime(endDatePicker.Value.Year, endDatePicker.Value.Month,
                                                endDatePicker.Value.Day, endTimePicker.Value.Hour,
                                                endTimePicker.Value.Minute, endTimePicker.Value.Second);

                importer.Frequency = (string) timeStepComboBox.SelectedItem;

                importer.CreateNewBoundaryConditions = createNewSeriesButton.Checked;

                importer.InputCoordinateSystem = CoordinateSystem;

                if (importOnActivePointsButton.Checked)
                {
                    importer.ImportMode = BoundaryConditionWpsImporter.SupportPointImportMode.Active;
                }
                if (importOnInactivePointsButton.Checked)
                {
                    importer.ImportMode = BoundaryConditionWpsImporter.SupportPointImportMode.Inactive;
                }
                if (importOnAllPointsButton.Checked)
                {
                    importer.ImportMode = BoundaryConditionWpsImporter.SupportPointImportMode.All;
                }
                if (importOnSelectedPointButton.Checked)
                {
                    importer.ImportMode = BoundaryConditionWpsImporter.SupportPointImportMode.Selected;
                }

            }
        }

        #region IView

        public object Data { get; set; }
        public Image Image { get; set; }
        public void EnsureVisible(object item){}

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}
