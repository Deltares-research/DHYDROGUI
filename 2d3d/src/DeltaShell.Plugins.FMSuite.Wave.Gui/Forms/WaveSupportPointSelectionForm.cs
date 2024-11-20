using System;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Forms
{
    /// <summary>
    /// <see cref="WaveSupportPointSelectionForm"/> allows for selecting which
    /// support points should be effected by a generate series.
    /// </summary>
    /// <seealso cref="Form"/>
    public partial class WaveSupportPointSelectionForm : Form
    {
        /// <summary>
        /// Creates a new <see cref="WaveSupportPointSelectionForm"/>.
        /// </summary>
        public WaveSupportPointSelectionForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the <see cref="WaveSupportPointMode"/>.
        /// </summary>
        public WaveSupportPointMode SupportPointOperationMode { get; private set; }

        private WaveSupportPointMode Mode
        {
            get
            {
                if (radioButtonSelectedSP.Checked)
                {
                    return WaveSupportPointMode.SelectedActiveSupportPoint;
                }

                if (radioButtonActiveSP.Checked)
                {
                    return WaveSupportPointMode.AllActiveSupportPoints;
                }

                return WaveSupportPointMode.NoSupportPoints;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SupportPointOperationMode = Mode;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            SupportPointOperationMode = WaveSupportPointMode.NoSupportPoints;
            Close();
        }
    }
}