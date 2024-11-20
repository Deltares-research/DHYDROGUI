using System;
using System.Windows.Forms;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms
{
    /// <summary>
    /// Form to allow the user to chose a grid cell ID in a <see cref="WaterQualityModel"/>.
    /// </summary>
    public partial class FindGridCellDialog : Form
    {
        private readonly int maximumGridCellId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindGridCellDialog"/> class.
        /// </summary>
        /// <param name="maximumGridCell"> The highest grid cell index in the model. </param>
        /// <param name="initialValue"> The value filled in the text box at creation. </param>
        public FindGridCellDialog(int maximumGridCell, int initialValue)
        {
            InitializeComponent();

            maximumGridCellId = maximumGridCell;
            NumericBoundsLabel.Text = string.Format("[1, {0}]", maximumGridCell);
            gridIndexTextBox.Text = initialValue.ToString();
            gridIndexTextBox.Select(); // To allow to enter a number immediately
        }

        /// <summary>
        /// Gets or sets the grid cell ID filled in in the form's picker. (-1 when invalid)
        /// </summary>
        public int GridCellId { get; private set; }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (Validate())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void GridIndexTextBoxOnTextChanged(object sender, EventArgs eventArgs)
        {
            int parsedIndex = -1;
            bool hasValidIndex = int.TryParse(gridIndexTextBox.Text, out parsedIndex) && parsedIndex >= 1 &&
                                 parsedIndex <= maximumGridCellId;

            string errorMessage =
                string.Format("Value must be a whole number in the range [{0}, {1}]", 1, maximumGridCellId);
            errorProvider.SetError(gridIndexTextBox, hasValidIndex ? "" : errorMessage);

            OkButton.Enabled = hasValidIndex;
            GridCellId = parsedIndex;
        }

        private void FindGridCellDialog_Shown(object sender, EventArgs e)
        {
            Cursor.Position = PointToScreen(OkButton.Location);
        }
    }
}