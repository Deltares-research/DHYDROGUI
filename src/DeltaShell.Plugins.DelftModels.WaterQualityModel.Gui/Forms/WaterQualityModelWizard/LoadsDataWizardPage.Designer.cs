namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    partial class LoadsDataWizardPage
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openCsvFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.importBoundaryDataFile = new System.Windows.Forms.Button();
            this.loadsLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.previewTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openCsvFileDialog
            // 
            this.openCsvFileDialog.FileName = "openCsvFileDialog";
            this.openCsvFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            // 
            // importBoundaryDataFile
            // 
            this.importBoundaryDataFile.Location = new System.Drawing.Point(31, 58);
            this.importBoundaryDataFile.Name = "importBoundaryDataFile";
            this.importBoundaryDataFile.Size = new System.Drawing.Size(75, 23);
            this.importBoundaryDataFile.TabIndex = 0;
            this.importBoundaryDataFile.Text = "Open File";
            this.importBoundaryDataFile.UseVisualStyleBackColor = true;
            this.importBoundaryDataFile.Click += new System.EventHandler(this.OpenCsvButton_Click);
            // 
            // loadsLabel
            // 
            this.loadsLabel.AutoSize = true;
            this.loadsLabel.Location = new System.Drawing.Point(28, 22);
            this.loadsLabel.Name = "loadsLabel";
            this.loadsLabel.Size = new System.Drawing.Size(110, 13);
            this.loadsLabel.TabIndex = 1;
            this.loadsLabel.Text = "Open Loads Data File";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.loadsLabel);
            this.splitContainer1.Panel1.Controls.Add(this.importBoundaryDataFile);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.previewTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(759, 384);
            this.splitContainer1.SplitterDistance = 253;
            this.splitContainer1.TabIndex = 3;
            // 
            // previewTextBox
            // 
            this.previewTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewTextBox.Location = new System.Drawing.Point(0, 0);
            this.previewTextBox.Multiline = true;
            this.previewTextBox.Name = "previewTextBox";
            this.previewTextBox.Size = new System.Drawing.Size(502, 384);
            this.previewTextBox.TabIndex = 0;
            // 
            // LoadsDataWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "LoadsDataWizardPage";
            this.Size = new System.Drawing.Size(759, 384);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openCsvFileDialog;
        private System.Windows.Forms.Button importBoundaryDataFile;
        private System.Windows.Forms.Label loadsLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox previewTextBox;
    }
}