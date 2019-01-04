using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Forms
{
    public partial class WaveBoundaryTimeSelectionDialog : Form, IDialog, IView
    {
        public WaveBoundaryTimeSelectionDialog()
        {
            InitializeComponent();

            boundaryListBox.DisplayMember = "FeatureName";

            boundaryListBox.SelectedIndexChanged += BoundaryListBoxOnSelectedIndexChanged;
            supportPointListbox.SelectedIndexChanged += SupportPointListboxOnSelectedIndexChanged;
        }

        private void SupportPointListboxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            UpdateDateTimeBox();
        }
        
        private IList<DateTime> GetSelectedDateTimes()
        {
            if (supportPointListbox.Items.Count == 0)
                return new DateTime[0];

            var selectedValue = (int)supportPointListbox.SelectedValue;
            var function = boundaryConditions[boundaryListBox.SelectedIndex].GetDataAtPoint(selectedValue);
            var times = function != null ? function.Arguments[0].GetValues<DateTime>() : Enumerable.Empty<DateTime>();
            return times.ToList();
        }

        private void BoundaryListBoxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            UpdateSupportPointBox();
            UpdateDateTimeBox();
        }

        private void UpdateSupportPointBox()
        {
            var selectedCondition = boundaryConditions[boundaryListBox.SelectedIndex];
            supportPointListbox.DataSource = new BindingList<int>(selectedCondition.DataPointIndices.ToList());
        }

        private void UpdateDateTimeBox()
        {
            var times = GetSelectedDateTimes();
            var timeStrings = times.Select(t => t.ToString(CultureInfo.InvariantCulture)).ToList();
            timesListBox.DataSource = new BindingList<string>(timeStrings);
        }


        private IList<WaveBoundaryCondition> boundaryConditions;
        public object Data
        {
            get { return boundaryConditions; }
            set
            {
                boundaryConditions =
                    ((IList<WaveBoundaryCondition>) value).Where(
                        bc => bc.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries).ToList();

                boundaryListBox.DataSource = new BindingList<WaveBoundaryCondition>(boundaryConditions);
            }
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item){
        }

        public ViewInfo ViewInfo { get; set; }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SelectedDateTimes = GetSelectedDateTimes();
            Close();
        }

        public IList<DateTime> SelectedDateTimes { get; private set; }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            SelectedDateTimes = null;
            Close();
        }

        public string Title { get; set; }

        public DelftDialogResult ShowModal()
        {
            return ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }
    }
}
