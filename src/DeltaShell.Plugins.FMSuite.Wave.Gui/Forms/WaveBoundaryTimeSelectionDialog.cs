using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Forms
{
    public partial class WaveBoundaryTimeSelectionDialog : Form, IDialog, IView
    {
        private IList<WaveBoundaryCondition> boundaryConditions;

        public WaveBoundaryTimeSelectionDialog()
        {
            InitializeComponent();

            boundaryListBox.DisplayMember = "FeatureName";

            boundaryListBox.SelectedIndexChanged += BoundaryListBoxOnSelectedIndexChanged;
            supportPointListbox.SelectedIndexChanged += SupportPointListboxOnSelectedIndexChanged;
        }

        public IList<DateTime> SelectedDateTimes { get; private set; }

        public string Title { get; set; }

        public object Data
        {
            get => boundaryConditions;
            set
            {
                boundaryConditions =
                    ((IList<WaveBoundaryCondition>) value).Where(
                        bc => bc.DataType == BoundaryConditionDataType
                                  .ParameterizedSpectrumTimeseries).ToList();

                boundaryListBox.DataSource = new BindingList<WaveBoundaryCondition>(boundaryConditions);
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public DelftDialogResult ShowModal()
        {
            return ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void EnsureVisible(object item) {}

        private void SupportPointListboxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            UpdateDateTimeBox();
        }

        private IList<DateTime> GetSelectedDateTimes()
        {
            if (supportPointListbox.Items.Count == 0)
            {
                return new DateTime[0];
            }

            var selectedValue = (int) supportPointListbox.SelectedValue;
            IFunction function = boundaryConditions[boundaryListBox.SelectedIndex].GetDataAtPoint(selectedValue);
            IEnumerable<DateTime> times = function != null
                                              ? function.Arguments[0].GetValues<DateTime>()
                                              : Enumerable.Empty<DateTime>();
            return times.ToList();
        }

        private void BoundaryListBoxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            UpdateSupportPointBox();
            UpdateDateTimeBox();
        }

        private void UpdateSupportPointBox()
        {
            WaveBoundaryCondition selectedCondition = boundaryConditions[boundaryListBox.SelectedIndex];
            supportPointListbox.DataSource = new BindingList<int>(selectedCondition.DataPointIndices.ToList());
        }

        private void UpdateDateTimeBox()
        {
            IList<DateTime> times = GetSelectedDateTimes();
            List<string> timeStrings = times.Select(t => t.ToString(CultureInfo.InvariantCulture)).ToList();
            timesListBox.DataSource = new BindingList<string>(timeStrings);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SelectedDateTimes = GetSelectedDateTimes();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            SelectedDateTimes = null;
            Close();
        }
    }
}