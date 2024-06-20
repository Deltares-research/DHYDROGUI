using System.Windows.Forms;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    partial class SubstanceProcessLibraryWizardPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("SOBEK-Delft3D library", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("test1", 0);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("test2", 0);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubstanceProcessLibraryWizardPage));
            this.listView2 = new System.Windows.Forms.ListView();
            this.listViewHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.standardProcessTypeTab = new System.Windows.Forms.TabPage();
            this.customProcessTypeTab = new System.Windows.Forms.TabPage();
            this.groupBoxProcessType = new System.Windows.Forms.GroupBox();
            this.pictureBoxProcessDefinitionsFilePath = new System.Windows.Forms.PictureBox();
            this.pictureBoxProcessFilePath = new System.Windows.Forms.PictureBox();
            this.buttonSelectProcessDefinitionsFilePath = new System.Windows.Forms.Button();
            this.buttonSelectProcessFilePath = new System.Windows.Forms.Button();
            this.textBoxProcessDefinitionsFile = new System.Windows.Forms.TextBox();
            this.textBoxProcessFile = new System.Windows.Forms.TextBox();
            this.processDefinitionFilesLabel = new System.Windows.Forms.Label();
            this.radioButtonStandardSobekProcesses = new System.Windows.Forms.RadioButton();
            this.processFileLabel = new System.Windows.Forms.Label();
            this.radioButtonCustomProcesses = new System.Windows.Forms.RadioButton();
            this.customLibraryPanel = new DelftTools.Controls.Swf.WizardPages.SelectFileWizardPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.substanceProcessLibraryView = new DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.SubstanceProcessLibraryView();
            this.groupBoxSubFile = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolTipProcessFilePath = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipProcessDefinitionsFilePath = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.standardProcessTypeTab.SuspendLayout();
            this.customProcessTypeTab.SuspendLayout();
            this.groupBoxProcessType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessDefinitionsFilePath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessFilePath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxSubFile.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView2
            // 
            this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewHeaderName});
            this.listView2.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup3.Header = "SOBEK-Delft3D library";
            listViewGroup3.Name = "listViewGroupSobek";
            this.listView2.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3});
            this.listView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView2.HideSelection = false;
            listViewItem3.StateImageIndex = 0;
            listViewItem4.StateImageIndex = 0;
            this.listView2.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem3,
            listViewItem4});
            this.listView2.Location = new System.Drawing.Point(3, 3);
            this.listView2.MultiSelect = false;
            this.listView2.Name = "listView2";
            this.listView2.ShowItemToolTips = true;
            this.listView2.Size = new System.Drawing.Size(289, 561);
            this.listView2.SmallImageList = this.imageList1;
            this.listView2.TabIndex = 8;
            this.listView2.TileSize = new System.Drawing.Size(204, 30);
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            this.listView2.SelectedIndexChanged += new System.EventHandler(this.SubFileSelectionChanged);
            this.listView2.Resize += new System.EventHandler(this.StandardSubFileListViewResize);
            // 
            // listViewHeaderName
            // 
            this.listViewHeaderName.Text = "Name";
            this.listViewHeaderName.Width = 178;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "book.png");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.standardProcessTypeTab);
            this.tabControl1.Controls.Add(this.customProcessTypeTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(303, 593);
            this.tabControl1.TabIndex = 9;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.SubFileSelectionChanged);
            // 
            // standardProcessTypeTab
            // 
            this.standardProcessTypeTab.Controls.Add(this.listView2);
            this.standardProcessTypeTab.Location = new System.Drawing.Point(4, 22);
            this.standardProcessTypeTab.Name = "standardProcessTypeTab";
            this.standardProcessTypeTab.Padding = new System.Windows.Forms.Padding(3);
            this.standardProcessTypeTab.Size = new System.Drawing.Size(295, 567);
            this.standardProcessTypeTab.TabIndex = 0;
            this.standardProcessTypeTab.Text = "Standard";
            this.standardProcessTypeTab.UseVisualStyleBackColor = true;
            // 
            // customProcessTypeTab
            // 
            this.customProcessTypeTab.Controls.Add(this.groupBoxProcessType);
            this.customProcessTypeTab.Controls.Add(this.customLibraryPanel);
            this.customProcessTypeTab.Location = new System.Drawing.Point(4, 22);
            this.customProcessTypeTab.Name = "customProcessTypeTab";
            this.customProcessTypeTab.Padding = new System.Windows.Forms.Padding(3);
            this.customProcessTypeTab.Size = new System.Drawing.Size(295, 567);
            this.customProcessTypeTab.TabIndex = 1;
            this.customProcessTypeTab.Text = "Custom sub file";
            this.customProcessTypeTab.UseVisualStyleBackColor = true;
            // 
            // groupBoxProcessType
            // 
            this.groupBoxProcessType.AutoSize = true;
            this.groupBoxProcessType.Controls.Add(this.pictureBoxProcessDefinitionsFilePath);
            this.groupBoxProcessType.Controls.Add(this.pictureBoxProcessFilePath);
            this.groupBoxProcessType.Controls.Add(this.buttonSelectProcessDefinitionsFilePath);
            this.groupBoxProcessType.Controls.Add(this.buttonSelectProcessFilePath);
            this.groupBoxProcessType.Controls.Add(this.textBoxProcessDefinitionsFile);
            this.groupBoxProcessType.Controls.Add(this.textBoxProcessFile);
            this.groupBoxProcessType.Controls.Add(this.processDefinitionFilesLabel);
            this.groupBoxProcessType.Controls.Add(this.radioButtonStandardSobekProcesses);
            this.groupBoxProcessType.Controls.Add(this.processFileLabel);
            this.groupBoxProcessType.Controls.Add(this.radioButtonCustomProcesses);
            this.groupBoxProcessType.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxProcessType.Location = new System.Drawing.Point(3, 407);
            this.groupBoxProcessType.Name = "groupBoxProcessType";
            this.groupBoxProcessType.Size = new System.Drawing.Size(289, 157);
            this.groupBoxProcessType.TabIndex = 12;
            this.groupBoxProcessType.TabStop = false;
            this.groupBoxProcessType.Text = "Process files";
            // 
            // pictureBoxProcessDefinitionsFilePath
            // 
            this.pictureBoxProcessDefinitionsFilePath.Image = global::DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties.Resources.information_frame;
            this.pictureBoxProcessDefinitionsFilePath.Location = new System.Drawing.Point(10, 83);
            this.pictureBoxProcessDefinitionsFilePath.Name = "pictureBoxProcessDefinitionsFilePath";
            this.pictureBoxProcessDefinitionsFilePath.Size = new System.Drawing.Size(19, 17);
            this.pictureBoxProcessDefinitionsFilePath.TabIndex = 9;
            this.pictureBoxProcessDefinitionsFilePath.TabStop = false;
            // 
            // pictureBoxProcessFilePath
            // 
            this.pictureBoxProcessFilePath.Image = global::DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties.Resources.information_frame;
            this.pictureBoxProcessFilePath.Location = new System.Drawing.Point(10, 120);
            this.pictureBoxProcessFilePath.Name = "pictureBoxProcessFilePath";
            this.pictureBoxProcessFilePath.Size = new System.Drawing.Size(19, 18);
            this.pictureBoxProcessFilePath.TabIndex = 8;
            this.pictureBoxProcessFilePath.TabStop = false;
            // 
            // buttonSelectProcessDefinitionsFilePath
            // 
            this.buttonSelectProcessDefinitionsFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectProcessDefinitionsFilePath.Location = new System.Drawing.Point(255, 79);
            this.buttonSelectProcessDefinitionsFilePath.Name = "buttonSelectProcessDefinitionsFilePath";
            this.buttonSelectProcessDefinitionsFilePath.Size = new System.Drawing.Size(28, 23);
            this.buttonSelectProcessDefinitionsFilePath.TabIndex = 7;
            this.buttonSelectProcessDefinitionsFilePath.Text = "...";
            this.buttonSelectProcessDefinitionsFilePath.UseVisualStyleBackColor = true;
            this.buttonSelectProcessDefinitionsFilePath.Click += new System.EventHandler(this.ButtonSelectProcessDefinitionsFilePathClick);
            // 
            // buttonSelectProcessFilePath
            // 
            this.buttonSelectProcessFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectProcessFilePath.Location = new System.Drawing.Point(255, 115);
            this.buttonSelectProcessFilePath.Name = "buttonSelectProcessFilePath";
            this.buttonSelectProcessFilePath.Size = new System.Drawing.Size(28, 23);
            this.buttonSelectProcessFilePath.TabIndex = 6;
            this.buttonSelectProcessFilePath.Text = "...";
            this.buttonSelectProcessFilePath.UseVisualStyleBackColor = true;
            this.buttonSelectProcessFilePath.Click += new System.EventHandler(this.ButtonSelectProcessFilePathClick);
            // 
            // textBoxProcessDefinitionsFile
            // 
            this.textBoxProcessDefinitionsFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProcessDefinitionsFile.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxProcessDefinitionsFile.Enabled = false;
            this.textBoxProcessDefinitionsFile.Location = new System.Drawing.Point(33, 81);
            this.textBoxProcessDefinitionsFile.Name = "textBoxProcessDefinitionsFile";
            this.textBoxProcessDefinitionsFile.Size = new System.Drawing.Size(216, 20);
            this.textBoxProcessDefinitionsFile.TabIndex = 5;
            // 
            // textBoxProcessFile
            // 
            this.textBoxProcessFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProcessFile.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxProcessFile.Enabled = false;
            this.textBoxProcessFile.Location = new System.Drawing.Point(33, 117);
            this.textBoxProcessFile.Name = "textBoxProcessFile";
            this.textBoxProcessFile.Size = new System.Drawing.Size(216, 20);
            this.textBoxProcessFile.TabIndex = 4;
            // 
            // processDefinitionFilesLabel
            // 
            this.processDefinitionFilesLabel.AutoSize = true;
            this.processDefinitionFilesLabel.Location = new System.Drawing.Point(7, 67);
            this.processDefinitionFilesLabel.Name = "processDefinitionFilesLabel";
            this.processDefinitionFilesLabel.Size = new System.Drawing.Size(135, 13);
            this.processDefinitionFilesLabel.TabIndex = 3;
            this.processDefinitionFilesLabel.Text = "Process definitions file path";
            // 
            // radioButtonStandardSobekProcesses
            // 
            this.radioButtonStandardSobekProcesses.AutoSize = true;
            this.radioButtonStandardSobekProcesses.Checked = true;
            this.radioButtonStandardSobekProcesses.Location = new System.Drawing.Point(10, 24);
            this.radioButtonStandardSobekProcesses.Name = "radioButtonStandardSobekProcesses";
            this.radioButtonStandardSobekProcesses.Size = new System.Drawing.Size(230, 17);
            this.radioButtonStandardSobekProcesses.TabIndex = 0;
            this.radioButtonStandardSobekProcesses.TabStop = true;
            this.radioButtonStandardSobekProcesses.Text = "Use standard D-WAQ processes library files";
            this.radioButtonStandardSobekProcesses.UseVisualStyleBackColor = true;
            this.radioButtonStandardSobekProcesses.CheckedChanged += new System.EventHandler(this.RadioButtonStandardSobekProcessesCheckedChanged);
            // 
            // processFileLabel
            // 
            this.processFileLabel.AutoSize = true;
            this.processFileLabel.Location = new System.Drawing.Point(7, 104);
            this.processFileLabel.Name = "processFileLabel";
            this.processFileLabel.Size = new System.Drawing.Size(98, 13);
            this.processFileLabel.TabIndex = 2;
            this.processFileLabel.Text = "Process dll file path";
            // 
            // radioButtonCustomProcesses
            // 
            this.radioButtonCustomProcesses.AutoSize = true;
            this.radioButtonCustomProcesses.Location = new System.Drawing.Point(10, 47);
            this.radioButtonCustomProcesses.Name = "radioButtonCustomProcesses";
            this.radioButtonCustomProcesses.Size = new System.Drawing.Size(213, 17);
            this.radioButtonCustomProcesses.TabIndex = 1;
            this.radioButtonCustomProcesses.Text = "Use D-WAQ open processes library files";
            this.radioButtonCustomProcesses.UseVisualStyleBackColor = true;
            this.radioButtonCustomProcesses.CheckedChanged += new System.EventHandler(this.RadioButtonCustomProcessesCheckedChanged);
            // 
            // customLibraryPanel
            // 
            this.customLibraryPanel.AutoSize = true;
            this.customLibraryPanel.BackColor = System.Drawing.Color.Transparent;
            this.customLibraryPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.customLibraryPanel.FileDescription = "Sub file";
            this.customLibraryPanel.Filter = "Sub files|*.sub";
            this.customLibraryPanel.Location = new System.Drawing.Point(3, 3);
            this.customLibraryPanel.Name = "customLibraryPanel";
            this.customLibraryPanel.Size = new System.Drawing.Size(289, 34);
            this.customLibraryPanel.TabIndex = 1;
            this.customLibraryPanel.FileSelected += new System.EventHandler(this.SubFileSelectionChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(5, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.substanceProcessLibraryView);
            this.splitContainer1.Size = new System.Drawing.Size(745, 593);
            this.splitContainer1.SplitterDistance = 303;
            this.splitContainer1.TabIndex = 10;
            // 
            // substanceProcessLibraryView
            // 
            this.substanceProcessLibraryView.Data = null;
            this.substanceProcessLibraryView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.substanceProcessLibraryView.Image = ((System.Drawing.Image)(resources.GetObject("substanceProcessLibraryView.Image")));
            this.substanceProcessLibraryView.Location = new System.Drawing.Point(0, 0);
            this.substanceProcessLibraryView.Name = "substanceProcessLibraryView";
            this.substanceProcessLibraryView.Padding = new System.Windows.Forms.Padding(4);
            this.substanceProcessLibraryView.ShowNameAndDescriptionColumnsOnly = true;
            this.substanceProcessLibraryView.Size = new System.Drawing.Size(438, 593);
            this.substanceProcessLibraryView.TabIndex = 7;
            this.substanceProcessLibraryView.ViewInfo = null;
            // 
            // groupBoxSubFile
            // 
            this.groupBoxSubFile.Controls.Add(this.panel1);
            this.groupBoxSubFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSubFile.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSubFile.Name = "groupBoxSubFile";
            this.groupBoxSubFile.Size = new System.Drawing.Size(761, 622);
            this.groupBoxSubFile.TabIndex = 13;
            this.groupBoxSubFile.TabStop = false;
            this.groupBoxSubFile.Text = "Sub file";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(755, 603);
            this.panel1.TabIndex = 11;
            // 
            // SubstanceProcessLibraryWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(560, 0);
            this.Controls.Add(this.groupBoxSubFile);
            this.Name = "SubstanceProcessLibraryWizardPage";
            this.Size = new System.Drawing.Size(761, 622);
            this.tabControl1.ResumeLayout(false);
            this.standardProcessTypeTab.ResumeLayout(false);
            this.customProcessTypeTab.ResumeLayout(false);
            this.customProcessTypeTab.PerformLayout();
            this.groupBoxProcessType.ResumeLayout(false);
            this.groupBoxProcessType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessDefinitionsFilePath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessFilePath)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxSubFile.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SubstanceProcessLibraryView substanceProcessLibraryView;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage standardProcessTypeTab;
        private System.Windows.Forms.TabPage customProcessTypeTab;
        private SplitContainer splitContainer1;
        private ColumnHeader listViewHeaderName;
        private RadioButton radioButtonCustomProcesses;
        private RadioButton radioButtonStandardSobekProcesses;
        private GroupBox groupBoxProcessType;
        private Button buttonSelectProcessDefinitionsFilePath;
        private Button buttonSelectProcessFilePath;
        private TextBox textBoxProcessDefinitionsFile;
        private TextBox textBoxProcessFile;
        private Label processDefinitionFilesLabel;
        private Label processFileLabel;
        private PictureBox pictureBoxProcessDefinitionsFilePath;
        private PictureBox pictureBoxProcessFilePath;
        private ToolTip toolTipProcessFilePath;
        private ToolTip toolTipProcessDefinitionsFilePath;
        private GroupBox groupBoxSubFile;
        private Panel panel1;
        private ImageList imageList1;
        private DelftTools.Controls.Swf.WizardPages.SelectFileWizardPage customLibraryPanel;
    }
}
