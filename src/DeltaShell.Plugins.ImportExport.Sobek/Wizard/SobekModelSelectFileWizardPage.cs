using System;
using System.IO;
using System.Security;
using DelftTools.Controls.Swf.WizardPages;
using DeltaShell.NGHS.Common.Extensions;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    public partial class SobekModelSelectFileWizardPage : SelectFileWizardPage
    {
        public SobekModelSelectFileWizardPage()
        {
            InitializeComponent();
            Filter = "set"; //fake set
        }

        public override string Filter
        {
            get
            {
                return base.Filter;
            }
            set
            {
                //ugly: ignore actual set filter
                base.Filter =
                    "All supported files|network.tp;deftop.1;caselist.cmt|Sobek 2.1* network files|network.tp|SobekRE network files|deftop.1|Sobek case list files|CASELIST.CMT";
            }
        }
        
        public override string FileName
        {
            get
            {
                var fileName = base.FileName;
                if (IsCaseList())
                {
                    fileName = GetSelectedCaseFile();
                }
                return fileName;
            }
        }

        private string CaseListFileName
        {
            get { return base.FileName; }
        }
        
        public override bool CanDoNext()
        {
            return File.Exists(FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="SecurityException"></exception>
        protected override void OnFileSelected()
        {
            ShowCaseSelection(IsCaseList());
        }

        private bool IsCaseList()
        {
            var extension = Path.GetExtension(CaseListFileName);
            return extension != null && extension.ToLower() == ".cmt";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="show"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="SecurityException"></exception>
        private void ShowCaseSelection(bool show)
        {
            caseBox.Visible = show;
            if (show)
            {
                FillCaseListBox(CaseListFileName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseFile"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="SecurityException"></exception>
        private void FillCaseListBox(string caseFile)
        {
            caseListBox.Items.Clear();
            
            string[] cases = File.ReadAllLines(caseFile);

            foreach (var @case in cases)
            {
                caseListBox.Items.Add(@case);
            }
        }

        private string GetSelectedCaseFile()
        {
            if (caseListBox.SelectedItem != null)
            {
                var caseDescription = (string)caseListBox.SelectedItem;
                var caseId = caseDescription.SplitOnEmptySpace()[0];

                var rootDirectory = Path.GetDirectoryName(CaseListFileName);
                var caseDirectory = rootDirectory + Path.DirectorySeparatorChar + caseId;

                var networkFile = caseDirectory + Path.DirectorySeparatorChar + "network.tp";
                var reFile = caseDirectory + Path.DirectorySeparatorChar + "deftop.1";

                if (File.Exists(networkFile))
                {
                    return networkFile;
                }
                if (File.Exists(reFile))
                {
                    return reFile;
                }
            }
            return "";
        }
    }
}
