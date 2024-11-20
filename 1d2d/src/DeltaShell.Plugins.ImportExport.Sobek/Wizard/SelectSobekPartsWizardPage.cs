using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    public partial class SelectSobekPartsWizardPage : UserControl, IWizardPage
    {
        private IPartialSobekImporter partialSobekImporter;
        private int x, y;
        private int rowHeight = 24;

        public SelectSobekPartsWizardPage()
        {
            InitializeComponent();
        }

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            return true;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        public IPartialSobekImporter PartialSobekImporter
        {
            get
            {
                return partialSobekImporter;
            }
            set
            {
                partialSobekImporter = value;
                InitPage();
            } 
        }

        private void InitPage()
        {
            x = y = rowHeight;
            Controls.Clear();
            var checkBoxSelectAll = new CheckBox
            {
                Text = "Select all",
                Checked = true,
                Width = 400
            };
            this.Controls.Add(checkBoxSelectAll);
            checkBoxSelectAll.CheckedChanged += checkBoxSelectAll_CheckedChanged;


            AddImporterAsCheckBox(partialSobekImporter);
        }

        void checkBoxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            var checkBoxSender = (CheckBox) sender;
            foreach (var checkBox in Controls.OfType<CheckBox>())
            {
                if (checkBox != checkBoxSender)
                {
                    checkBox.Checked = checkBoxSender.Checked;
                }
            }
        }

        private void AddImporterAsCheckBox(IPartialSobekImporter rootImporter)
        {
            var getAllImporters = GetImporters(rootImporter).Reverse().ToList();

            var left = true;

            foreach (var importer in getAllImporters)
            {
                if (!importer.IsVisible || string.IsNullOrEmpty(importer.DisplayName))
                {
                    continue;
                }

                var checkBox = new CheckBox
                    {
                        Text = importer.DisplayName,
                        Checked = importer.IsActive,
                        Tag = importer,
                        Location = new Point(x + (left ? 0 : 320), y),
                        Width = 300
                    };
                this.Controls.Add(checkBox);
                checkBox.CheckedChanged += checkBox_CheckedChanged;
                if (!left)
                    y += rowHeight;
                left = !left;
            }
        }

        private IEnumerable<IPartialSobekImporter> GetImporters(IPartialSobekImporter partialImporter)
        {
            while (partialImporter != null)
            {
                yield return partialImporter;
                partialImporter = partialImporter.PartialSobekImporter;
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox) sender;
            ((IPartialSobekImporter) checkBox.Tag).IsActive = checkBox.Checked;
        }
    }
}
