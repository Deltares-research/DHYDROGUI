namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    partial class CrossSectionImportSettingsWizardPage
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.cbCreateIfNotFound = new System.Windows.Forms.CheckBox();
            this.cbImportChainages = new System.Windows.Forms.CheckBox();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.cbCreateIfNotFound);
            this.groupBox.Controls.Add(this.cbImportChainages);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(467, 383);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Settings";
            // 
            // cbCreateIfNotFound
            // 
            this.cbCreateIfNotFound.AutoSize = true;
            this.cbCreateIfNotFound.Location = new System.Drawing.Point(16, 84);
            this.cbCreateIfNotFound.Name = "cbCreateIfNotFound";
            this.cbCreateIfNotFound.Size = new System.Drawing.Size(266, 17);
            this.cbCreateIfNotFound.TabIndex = 1;
            this.cbCreateIfNotFound.Text = "Create cross section if Name was not found in network";
            this.cbCreateIfNotFound.UseVisualStyleBackColor = true;
            // 
            // cbImportChainages
            // 
            this.cbImportChainages.AutoSize = true;
            this.cbImportChainages.Location = new System.Drawing.Point(16, 41);
            this.cbImportChainages.Name = "cbImportChainages";
            this.cbImportChainages.Size = new System.Drawing.Size(107, 17);
            this.cbImportChainages.TabIndex = 0;
            this.cbImportChainages.Text = "Import chainages";
            this.cbImportChainages.UseVisualStyleBackColor = true;
            // 
            // CrossSectionImportSettingsWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "CrossSectionImportSettingsWizardPage";
            this.Size = new System.Drawing.Size(467, 383);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.CheckBox cbCreateIfNotFound;
        private System.Windows.Forms.CheckBox cbImportChainages;
    }
}
