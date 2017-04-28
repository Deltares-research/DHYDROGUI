namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    partial class SobekModelSelectFileWizardPage
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
            this.caseBox = new System.Windows.Forms.GroupBox();
            this.caseListBox = new System.Windows.Forms.ListBox();
            this.caseBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // caseBox
            // 
            this.caseBox.Controls.Add(this.caseListBox);
            this.caseBox.Location = new System.Drawing.Point(6, 42);
            this.caseBox.Name = "caseBox";
            this.caseBox.Padding = new System.Windows.Forms.Padding(10);
            this.caseBox.Size = new System.Drawing.Size(381, 298);
            this.caseBox.TabIndex = 6;
            this.caseBox.TabStop = false;
            this.caseBox.Text = "Select a case:";
            this.caseBox.Visible = false;
            // 
            // caseListBox
            // 
            this.caseListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caseListBox.FormattingEnabled = true;
            this.caseListBox.Location = new System.Drawing.Point(10, 23);
            this.caseListBox.Name = "caseListBox";
            this.caseListBox.Size = new System.Drawing.Size(361, 265);
            this.caseListBox.TabIndex = 0;
            // 
            // WaterFlowModelSelectFileWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.caseBox);
            this.Name = "SobekModelSelectFileWizardPage";
            this.Size = new System.Drawing.Size(464, 400);
            this.Controls.SetChildIndex(this.caseBox, 0);
            this.caseBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox caseBox;
        private System.Windows.Forms.ListBox caseListBox;
    }
}
