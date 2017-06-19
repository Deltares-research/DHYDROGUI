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
        private string message;
        private bool showCancelButton;
        private Point buttonOkOrgLocation;

        public ImportHydFileDialog(WaterQualityModel waterQualityModel, string path)
        {
            this.waterQualityModel = waterQualityModel;
            this.path = path;

            InitializeComponent();
            checkBoxTimers.Text = String.Format("Keep current timers for '{0}'", waterQualityModel.Name);
            checkBoxCoordinateSystem.Text = String.Format("Keep current coordinate system for '{0}'", waterQualityModel.Name);
            buttonOkOrgLocation = buttonOk.Location;
        }

        public string Message
        {
            get { return labelMessage.Text; }
            set { labelMessage.Text = value; }
        }

        public bool ShowCancelButton
        {
            get { return showCancelButton; }
            set
            {
                showCancelButton = value;
                buttonCancel.Visible = ShowCancelButton;
                buttonOk.Location = ShowCancelButton ? buttonOkOrgLocation : buttonCancel.Location;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var hydroData = HydFileReader.ReadAll(new FileInfo(path));
            waterQualityModel.ImportHydroData(hydroData, !checkBoxTimers.Checked, !checkBoxCoordinateSystem.Checked);
        }
    }
}
