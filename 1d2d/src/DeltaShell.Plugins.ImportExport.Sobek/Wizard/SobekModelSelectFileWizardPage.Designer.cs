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
            this.caseBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.caseBox.Controls.Add(this.caseListBox);
            this.caseBox.Location = new System.Drawing.Point(9, 65);
            this.caseBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.caseBox.Name = "caseBox";
            this.caseBox.Padding = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this.caseBox.Size = new System.Drawing.Size(683, 458);
            this.caseBox.TabIndex = 6;
            this.caseBox.TabStop = false;
            this.caseBox.Text = "Select a case:";
            this.caseBox.Visible = false;
            // 
            // caseListBox
            // 
            this.caseListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caseListBox.FormattingEnabled = true;
            this.caseListBox.ItemHeight = 20;
            this.caseListBox.Location = new System.Drawing.Point(15, 34);
            this.caseListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.caseListBox.Name = "caseListBox";
            this.caseListBox.Size = new System.Drawing.Size(653, 409);
            this.caseListBox.TabIndex = 0;
            // 
            // SobekModelSelectFileWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.caseBox);
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.Name = "SobekModelSelectFileWizardPage";
            this.Size = new System.Drawing.Size(696, 615);
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
