using System;
using System.Windows.Forms;
using DelftTools.Controls.Swf;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    public partial class SelectSobekModelsWizardPage : UserControl, IWizardPage
    {
        public SelectSobekModelsWizardPage()
        {
            InitializeComponent();
        }

        public bool ImportFlow
        {
            get { return chkboxFlow.Checked; }
            set { chkboxFlow.Checked = value; }
        }

        public bool ImportFlowEnabled
        {
            get { return chkboxFlow.Enabled; }
            set { chkboxFlow.Enabled = value; }
        }

        public bool ImportRR
        {
            get { return chkboxRR.Checked; }
            set { chkboxRR.Checked = value; }
        }

        public bool ImportRREnabled
        {
            get { return chkboxRR.Enabled; }
            set { chkboxRR.Enabled = value; }
        }
        public bool ImportRtc
        {
            get { return chkboxRTC.Checked; }
            set { chkboxRTC.Checked = value; }
        }

        public bool ImportRtcEnabled
        {
            get { return chkboxRTC.Enabled; }
            set { chkboxRTC.Enabled = value; }
        }

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            return ImportFlow || ImportRR || ImportRtc;
        }

        public bool CanDoPrevious()
        {
            return true;
        }
    }
}
