using System.Windows.Forms;
using DelftTools.Controls.Swf.WizardPages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Importers
{
    partial class LandUsePolderMappingWizardPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>

        private void InitializeComponent()
        {
            this.headerPanel = new System.Windows.Forms.Panel();
            this.radioLandUseFile = new System.Windows.Forms.RadioButton();
            this.radioAttributes = new System.Windows.Forms.RadioButton();
            this.radioNone = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.landUseCombobox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.landUseMappingPanel = new System.Windows.Forms.Panel();
            this.mappingPanel = new System.Windows.Forms.Panel();
            this.landUseMappingControl = new LandUseMappingControl();
            this.selectLandUseFileControl = new SelectFileWizardPage();
            this.line = new System.Windows.Forms.Panel();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.attributeMappingPanel = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.openwaterComboBox = new System.Windows.Forms.ComboBox();
            this.greenhouseComboBox = new System.Windows.Forms.ComboBox();
            this.pavedComboBox = new System.Windows.Forms.ComboBox();
            this.unpavedComboBox = new System.Windows.Forms.ComboBox();
            this.unitComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.headerPanel.SuspendLayout();
            this.landUseMappingPanel.SuspendLayout();
            this.mappingPanel.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.attributeMappingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.Controls.Add(this.radioLandUseFile);
            this.headerPanel.Controls.Add(this.radioAttributes);
            this.headerPanel.Controls.Add(this.radioNone);
            this.headerPanel.Controls.Add(this.label2);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(10, 10);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(605, 88);
            this.headerPanel.TabIndex = 4;
            // 
            // radioLandUseFile
            // 
            this.radioLandUseFile.AutoSize = true;
            this.radioLandUseFile.Location = new System.Drawing.Point(20, 62);
            this.radioLandUseFile.Name = "radioLandUseFile";
            this.radioLandUseFile.Size = new System.Drawing.Size(151, 17);
            this.radioLandUseFile.TabIndex = 1;
            this.radioLandUseFile.Text = "From separate land-use file";
            this.radioLandUseFile.UseVisualStyleBackColor = true;
            this.radioLandUseFile.CheckedChanged += new System.EventHandler(this.radioCheckedChanged);
            // 
            // radioAttributes
            // 
            this.radioAttributes.AutoSize = true;
            this.radioAttributes.Location = new System.Drawing.Point(20, 39);
            this.radioAttributes.Name = "radioAttributes";
            this.radioAttributes.Size = new System.Drawing.Size(217, 17);
            this.radioAttributes.TabIndex = 1;
            this.radioAttributes.Text = "From attributes in catchment data source";
            this.radioAttributes.UseVisualStyleBackColor = true;
            this.radioAttributes.CheckedChanged += new System.EventHandler(this.radioCheckedChanged);
            // 
            // radioNone
            // 
            this.radioNone.AutoSize = true;
            this.radioNone.Checked = true;
            this.radioNone.Location = new System.Drawing.Point(20, 16);
            this.radioNone.Name = "radioNone";
            this.radioNone.Size = new System.Drawing.Size(51, 17);
            this.radioNone.TabIndex = 1;
            this.radioNone.TabStop = true;
            this.radioNone.Text = "None";
            this.radioNone.UseVisualStyleBackColor = true;
            this.radioNone.CheckedChanged += new System.EventHandler(this.radioCheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(464, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "If you want to define a land use mapping to Polder Concept areas, please select a" +
                "n option below:";
            // 
            // landUseCombobox
            // 
            this.landUseCombobox.FormattingEnabled = true;
            this.landUseCombobox.Location = new System.Drawing.Point(123, 6);
            this.landUseCombobox.Name = "landUseCombobox";
            this.landUseCombobox.Size = new System.Drawing.Size(162, 21);
            this.landUseCombobox.TabIndex = 6;
            this.landUseCombobox.SelectionChangeCommitted += new System.EventHandler(this.LandUseComboboxSelectionChangeCommitted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Land use column";
            // 
            // landUseMappingPanel
            // 
            this.landUseMappingPanel.Controls.Add(this.mappingPanel);
            this.landUseMappingPanel.Controls.Add(this.selectLandUseFileControl);
            this.landUseMappingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.landUseMappingPanel.Location = new System.Drawing.Point(0, 0);
            this.landUseMappingPanel.Name = "landUseMappingPanel";
            this.landUseMappingPanel.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.landUseMappingPanel.Size = new System.Drawing.Size(605, 427);
            this.landUseMappingPanel.TabIndex = 8;
            this.landUseMappingPanel.Visible = false;
            // 
            // mappingPanel
            // 
            this.mappingPanel.Controls.Add(this.label1);
            this.mappingPanel.Controls.Add(this.landUseCombobox);
            this.mappingPanel.Controls.Add(this.landUseMappingControl);
            this.mappingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mappingPanel.Location = new System.Drawing.Point(20, 39);
            this.mappingPanel.Name = "mappingPanel";
            this.mappingPanel.Size = new System.Drawing.Size(585, 388);
            this.mappingPanel.TabIndex = 8;
            this.mappingPanel.Visible = false;
            // 
            // landUseMappingControl
            // 
            this.landUseMappingControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.landUseMappingControl.LandUseCategories = null;
            this.landUseMappingControl.Location = new System.Drawing.Point(6, 33);
            this.landUseMappingControl.Name = "landUseMappingControl";
            this.landUseMappingControl.Size = new System.Drawing.Size(356, 352);
            this.landUseMappingControl.TabIndex = 5;
            // 
            // selectLandUseFileControl
            // 
            this.selectLandUseFileControl.BackColor = System.Drawing.Color.Transparent;
            this.selectLandUseFileControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.selectLandUseFileControl.FileDescription = "Filename";
            this.selectLandUseFileControl.FileName = null;
            this.selectLandUseFileControl.Filter = "Shape files |*.shp";
            this.selectLandUseFileControl.Location = new System.Drawing.Point(20, 0);
            this.selectLandUseFileControl.Name = "selectLandUseFileControl";
            this.selectLandUseFileControl.Size = new System.Drawing.Size(585, 39);
            this.selectLandUseFileControl.TabIndex = 3;
            // 
            // line
            // 
            this.line.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.line.BackColor = System.Drawing.SystemColors.ControlDark;
            this.line.Location = new System.Drawing.Point(1, 98);
            this.line.Name = "line";
            this.line.Size = new System.Drawing.Size(624, 1);
            this.line.TabIndex = 9;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.attributeMappingPanel);
            this.mainPanel.Controls.Add(this.landUseMappingPanel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(10, 98);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(605, 427);
            this.mainPanel.TabIndex = 10;
            // 
            // attributeMappingPanel
            // 
            this.attributeMappingPanel.Controls.Add(this.unitComboBox);
            this.attributeMappingPanel.Controls.Add(this.label6);
            this.attributeMappingPanel.Controls.Add(this.label5);
            this.attributeMappingPanel.Controls.Add(this.label4);
            this.attributeMappingPanel.Controls.Add(this.label7);
            this.attributeMappingPanel.Controls.Add(this.label3);
            this.attributeMappingPanel.Controls.Add(this.openwaterComboBox);
            this.attributeMappingPanel.Controls.Add(this.greenhouseComboBox);
            this.attributeMappingPanel.Controls.Add(this.pavedComboBox);
            this.attributeMappingPanel.Controls.Add(this.unpavedComboBox);
            this.attributeMappingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.attributeMappingPanel.Location = new System.Drawing.Point(0, 0);
            this.attributeMappingPanel.Name = "attributeMappingPanel";
            this.attributeMappingPanel.Size = new System.Drawing.Size(605, 427);
            this.attributeMappingPanel.TabIndex = 8;
            this.attributeMappingPanel.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 123);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Open water area";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 96);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Greenhouse area";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Paved area";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Unpaved area";
            // 
            // openwaterComboBox
            // 
            this.openwaterComboBox.FormattingEnabled = true;
            this.openwaterComboBox.Location = new System.Drawing.Point(113, 120);
            this.openwaterComboBox.Name = "openwaterComboBox";
            this.openwaterComboBox.Size = new System.Drawing.Size(121, 21);
            this.openwaterComboBox.TabIndex = 0;
            // 
            // greenhouseComboBox
            // 
            this.greenhouseComboBox.FormattingEnabled = true;
            this.greenhouseComboBox.Location = new System.Drawing.Point(113, 93);
            this.greenhouseComboBox.Name = "greenhouseComboBox";
            this.greenhouseComboBox.Size = new System.Drawing.Size(121, 21);
            this.greenhouseComboBox.TabIndex = 0;
            // 
            // pavedComboBox
            // 
            this.pavedComboBox.FormattingEnabled = true;
            this.pavedComboBox.Location = new System.Drawing.Point(113, 66);
            this.pavedComboBox.Name = "pavedComboBox";
            this.pavedComboBox.Size = new System.Drawing.Size(121, 21);
            this.pavedComboBox.TabIndex = 0;
            // 
            // unpavedComboBox
            // 
            this.unpavedComboBox.FormattingEnabled = true;
            this.unpavedComboBox.Location = new System.Drawing.Point(113, 39);
            this.unpavedComboBox.Name = "unpavedComboBox";
            this.unpavedComboBox.Size = new System.Drawing.Size(121, 21);
            this.unpavedComboBox.TabIndex = 0;
            // 
            // unitComboBox
            // 
            this.unitComboBox.FormattingEnabled = true;
            this.unitComboBox.Location = new System.Drawing.Point(113, 12);
            this.unitComboBox.Name = "unitComboBox";
            this.unitComboBox.Size = new System.Drawing.Size(58, 21);
            this.unitComboBox.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Data source unit";
            // 
            // LandUsePolderMappingWizardPage
            // 
            this.Controls.Add(this.line);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.headerPanel);
            this.Name = "LandUsePolderMappingWizardPage";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(625, 535);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.landUseMappingPanel.ResumeLayout(false);
            this.mappingPanel.ResumeLayout(false);
            this.mappingPanel.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            this.attributeMappingPanel.ResumeLayout(false);
            this.attributeMappingPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SelectFileWizardPage selectLandUseFileControl;
        private Panel headerPanel;
        private LandUseMappingControl landUseMappingControl;
        private ComboBox landUseCombobox;
        private Label label1;
        private Panel landUseMappingPanel;
        private Panel line;
        private Panel mappingPanel;
        private Label label2;
        private Panel mainPanel;
        private RadioButton radioLandUseFile;
        private RadioButton radioAttributes;
        private RadioButton radioNone;
        private Panel attributeMappingPanel;
        private Label label3;
        private ComboBox unpavedComboBox;
        private Label label6;
        private Label label5;
        private Label label4;
        private ComboBox openwaterComboBox;
        private ComboBox greenhouseComboBox;
        private ComboBox pavedComboBox;
        private ComboBox unitComboBox;
        private Label label7;
    }
}
