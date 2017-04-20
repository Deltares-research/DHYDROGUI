using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Functions;

namespace DeltaShell.Plugins.Fews.Forms
{
    public partial class SelectTimeSeriesWizardPage : UserControl, IWizardPage
    {
        private IList<TimeSeries> timeSeries;

        public SelectTimeSeriesWizardPage()
        {
            InitializeComponent();
        }

        public void InitPage()
        {
            PiTimeSeriesImporter.Execute();
            lbTimeSeries.DataSource = PiTimeSeriesImporter.TimeSeries;
            if (MultiSelect)
            {
                SelectAll();
            }
        }

        public IEnumerable<TimeSeries> GetSelectedTimeSeries
        {
            get
            {
                return lbTimeSeries.SelectedItems.Cast<TimeSeries>();
            }
        }

        #region IWizardPage members

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            return lbTimeSeries.SelectedItems.Count > 0;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        #endregion

        public PiTimeSeriesImporter PiTimeSeriesImporter { get; set; }

        public bool MultiSelect
        {
            get { return lbTimeSeries.SelectionMode == SelectionMode.MultiSimple; }
            set { 
                lbTimeSeries.SelectionMode = (value ? SelectionMode.MultiSimple : SelectionMode.One);
                if(value)
                {
                    SelectAll();
                }

            }
        }

        #region private

        private void SelectAll()
        {
            for (int i=0; i < lbTimeSeries.Items.Count; i++)
            {
                lbTimeSeries.SetSelected(i, true);
            }
        }

        #endregion
    }
}
