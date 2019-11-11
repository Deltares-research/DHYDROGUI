namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    partial class SelectSobekModelsWizardPage
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkboxRR = new System.Windows.Forms.CheckBox();
            this.chkboxFlow = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkboxRR);
            this.groupBox1.Controls.Add(this.chkboxFlow);
            this.groupBox1.Location = new System.Drawing.Point(13, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(365, 85);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Models to import";
            // 
            // chkboxRR
            // 
            this.chkboxRR.AutoSize = true;
            this.chkboxRR.Location = new System.Drawing.Point(22, 53);
            this.chkboxRR.Name = "chkboxRR";
            this.chkboxRR.Size = new System.Drawing.Size(165, 17);
            this.chkboxRR.TabIndex = 2;
            this.chkboxRR.Text = "Rainfall runoff model (lumped)";
            this.chkboxRR.UseVisualStyleBackColor = true;
            // 
            // chkboxFlow
            // 
            this.chkboxFlow.AutoSize = true;
            this.chkboxFlow.Location = new System.Drawing.Point(22, 30);
            this.chkboxFlow.Name = "chkboxFlow";
            this.chkboxFlow.Size = new System.Drawing.Size(279, 17);
            this.chkboxFlow.TabIndex = 0;
            this.chkboxFlow.Text = "Water flow model (1d) + Controllers and triggers (RTC)";
            this.chkboxFlow.UseVisualStyleBackColor = true;
            // 
            // SelectSobekModelsWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "SelectSobekModelsWizardPage";
            this.Size = new System.Drawing.Size(393, 121);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkboxRR;
        private System.Windows.Forms.CheckBox chkboxFlow;
    }
}
