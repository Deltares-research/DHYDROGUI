using System;
using System.IO;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    internal partial class WindSelectionDialog : Form
    {
        private const string TimFileFilter = "time series file|*.tim";
        private const string XGridFileFilter = "arcinfo file|*.amu";
        private const string YGridFileFilter = "arcinfo file|*.amv";
        private const string PGridFileFilter = "arcinfo file|*.amp";
        private const string CurviGridFileFilter = "curvi-grid file|*.apwxwy";
        private const string SpiderWebFileFilter = "spider web file|*.spw";

        public static string MakeFileFilter(IWindField windField)
        {
            if (windField is UniformWindField)
            {
                return TimFileFilter;
            }
            if (windField is SpiderWebWindField)
            {
                return SpiderWebFileFilter;
            }
            if (windField is GriddedWindField)
            {
                switch (((GriddedWindField)windField).Quantity)
                {
                    case WindQuantity.VelocityX:
                        return XGridFileFilter;
                    case WindQuantity.VelocityY:
                        return YGridFileFilter;
                    case WindQuantity.AirPressure:
                        return PGridFileFilter;
                    case WindQuantity.VelocityVectorAirPressure:
                        return CurviGridFileFilter;
                    default:
                        throw new ArgumentException("Wind quantity does not have a corresponding file filter");
                }
            }
            return null;
        }

        public WindSelectionDialog()
        {
            InitializeComponent();
            WndXRadioButton.Checked = true;
        }

        public IWindField WindField { get; private set; }

        private void OkButtonClick(object sender, EventArgs e)
        {
            WindField = CreateWindField();
            if (WindField != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private IWindField CreateWindField()
        {
            if (WndXRadioButton.Checked)
            {
                return UniformWindField.CreateWindXSeries();
            }
            if (WndYRadioButton.Checked)
            {
                return UniformWindField.CreateWindYSeries();
            }
            if (PressureRadioButton.Checked)
            {
                return UniformWindField.CreatePressureSeries();
            }
            if (WndXYRadioButton.Checked)
            {
                return UniformWindField.CreateWindXYSeries();
            }
            if (WndMagDirRadioButton.Checked)
            {
                return UniformWindField.CreateWindPolarSeries();
            }
            if (WndXGridRadioButton.Checked)
            {
                var filePath = SelectFilePath(XGridFileFilter);
                return filePath == null ? null : GriddedWindField.CreateXField(filePath);
            }
            if (WndYGridRadioButton.Checked)
            {
                var filePath = SelectFilePath(YGridFileFilter);
                return filePath == null ? null : GriddedWindField.CreateYField(filePath);
            }
            if (PressureGridRadioButton.Checked)
            {
                var filePath = SelectFilePath(PGridFileFilter);
                return filePath == null ? null : GriddedWindField.CreatePressureField(filePath);

            }
            if (VelocityPressureGridRadioButton.Checked)
            {
                var filePath = SelectFilePath(CurviGridFileFilter);
                if (filePath != null)
                {
                    var gridFile = GetCorrespondingGridFile(filePath);
                    return gridFile == null ? null : GriddedWindField.CreateCurviField(filePath, gridFile);
                }
            }
            if (SpiderWebRadioButton.Checked)
            {
                var filePath = SelectFilePath(SpiderWebFileFilter);
                return filePath == null ? null : SpiderWebWindField.Create(filePath);
            }
            return null;
        }

        public static string GetCorrespondingGridFile(string filePath)
        {
            var path = Path.GetDirectoryName(filePath);
            var fname = Path.GetFileNameWithoutExtension(filePath);
            var gridFile = Path.Combine(path, string.Join(".", fname, "grd"));
            if (!File.Exists(gridFile))
            {
                MessageBox.Show("The corresponding grid file " + gridFile + "could not be found.",
                    "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return gridFile;
        }

        private static string SelectFilePath(string filter)
        {
            var dialog = new OpenFileDialog {Title = "Open file", Filter = filter};
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }
            return null;
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
