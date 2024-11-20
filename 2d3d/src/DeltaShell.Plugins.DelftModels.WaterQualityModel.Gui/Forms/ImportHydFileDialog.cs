using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms
{
    public partial class ImportHydFileDialog : Form
    {
        private readonly WaterQualityModel waterQualityModel;
        private readonly string path;
        private bool showCancelButton;
        private Point buttonOkOrgLocation;

        public ImportHydFileDialog(WaterQualityModel waterQualityModel, string path)
        {
            this.waterQualityModel = waterQualityModel;
            this.path = path;

            InitializeComponent();
            checkBoxTimers.Text = string.Format("Keep current timers for '{0}'", waterQualityModel.Name);
            checkBoxCoordinateSystem.Text =
                string.Format("Keep current coordinate system for '{0}'", waterQualityModel.Name);
            buttonOkOrgLocation = buttonOk.Location;
        }

        public bool ShowCancelButton
        {
            get => showCancelButton;
            set
            {
                showCancelButton = value;
                buttonCancel.Visible = ShowCancelButton;
                buttonOk.Location = ShowCancelButton ? buttonOkOrgLocation : buttonCancel.Location;
            }
        }

        public void SetLabelMessage(string text)
        {
            labelMessage.Text = text;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            HydFileData hydroData = HydFileReader.ReadAll(new FileInfo(path));
            waterQualityModel.ImportHydroData(hydroData, !checkBoxCoordinateSystem.Checked);
        }
    }
}