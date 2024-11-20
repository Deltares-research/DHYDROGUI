namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    partial class ImportFromGisWizardPage
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
            this.groupBoxImportProperties = new System.Windows.Forms.GroupBox();
            this.textBoxSnappingPrecision = new System.Windows.Forms.TextBox();
            this.lblSnappingPrecisionUnit = new System.Windows.Forms.Label();
            this.lblSnappingPrecision = new System.Windows.Forms.Label();
            this.buttonSaveMappingFile = new System.Windows.Forms.Button();
            this.groupBoxImportProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxImportProperties
            // 
            this.groupBoxImportProperties.Controls.Add(this.textBoxSnappingPrecision);
            this.groupBoxImportProperties.Controls.Add(this.lblSnappingPrecisionUnit);
            this.groupBoxImportProperties.Controls.Add(this.lblSnappingPrecision);
            this.groupBoxImportProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxImportProperties.Location = new System.Drawing.Point(0, 0);
            this.groupBoxImportProperties.Name = "groupBoxImportProperties";
            this.groupBoxImportProperties.Size = new System.Drawing.Size(435, 100);
            this.groupBoxImportProperties.TabIndex = 0;
            this.groupBoxImportProperties.TabStop = false;
            this.groupBoxImportProperties.Text = "Import properties";
            // 
            // textBoxSnappingPrecision
            // 
            this.textBoxSnappingPrecision.Location = new System.Drawing.Point(197, 23);
            this.textBoxSnappingPrecision.Name = "textBoxSnappingPrecision";
            this.textBoxSnappingPrecision.Size = new System.Drawing.Size(55, 20);
            this.textBoxSnappingPrecision.TabIndex = 3;
            this.textBoxSnappingPrecision.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxSnappingPrecision.TextChanged += new System.EventHandler(this.textBoxSnappingPrecision_TextChanged);
            // 
            // lblSnappingPrecisionUnit
            // 
            this.lblSnappingPrecisionUnit.AutoSize = true;
            this.lblSnappingPrecisionUnit.Location = new System.Drawing.Point(258, 23);
            this.lblSnappingPrecisionUnit.Name = "lblSnappingPrecisionUnit";
            this.lblSnappingPrecisionUnit.Size = new System.Drawing.Size(15, 13);
            this.lblSnappingPrecisionUnit.TabIndex = 2;
            this.lblSnappingPrecisionUnit.Text = "m";
            // 
            // lblSnappingPrecision
            // 
            this.lblSnappingPrecision.AutoSize = true;
            this.lblSnappingPrecision.Location = new System.Drawing.Point(7, 20);
            this.lblSnappingPrecision.Name = "lblSnappingPrecision";
            this.lblSnappingPrecision.Size = new System.Drawing.Size(97, 13);
            this.lblSnappingPrecision.TabIndex = 0;
            this.lblSnappingPrecision.Text = "Snapping precision";
            // 
            // buttonSaveMappingFile
            // 
            this.buttonSaveMappingFile.Location = new System.Drawing.Point(4, 107);
            this.buttonSaveMappingFile.Name = "buttonSaveMappingFile";
            this.buttonSaveMappingFile.Size = new System.Drawing.Size(191, 23);
            this.buttonSaveMappingFile.TabIndex = 1;
            this.buttonSaveMappingFile.Text = "Save mapping file of GIS-importers";
            this.buttonSaveMappingFile.UseVisualStyleBackColor = true;
            this.buttonSaveMappingFile.Click += new System.EventHandler(this.buttonSaveMappingFile_Click);
            // 
            // ImportFromGisWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.buttonSaveMappingFile);
            this.Controls.Add(this.groupBoxImportProperties);
            this.Name = "ImportFromGisWizardPage";
            this.Size = new System.Drawing.Size(435, 307);
            this.groupBoxImportProperties.ResumeLayout(false);
            this.groupBoxImportProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxImportProperties;
        private System.Windows.Forms.Label lblSnappingPrecisionUnit;
        private System.Windows.Forms.Label lblSnappingPrecision;
        private System.Windows.Forms.TextBox textBoxSnappingPrecision;
        private System.Windows.Forms.Button buttonSaveMappingFile;
    }
}
