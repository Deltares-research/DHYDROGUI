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
            this.chkboxRTC = new System.Windows.Forms.CheckBox();
            this.chkboxRR = new System.Windows.Forms.CheckBox();
            this.chkboxFlow = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkboxRTC);
            this.groupBox1.Controls.Add(this.chkboxRR);
            this.groupBox1.Controls.Add(this.chkboxFlow);
            this.groupBox1.Location = new System.Drawing.Point(17, 22);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(487, 137);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Models to import";
            // 
            // chkboxRTC
            // 
            this.chkboxRTC.AutoSize = true;
            this.chkboxRTC.Location = new System.Drawing.Point(29, 92);
            this.chkboxRTC.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkboxRTC.Name = "chkboxRTC";
            this.chkboxRTC.Size = new System.Drawing.Size(220, 21);
            this.chkboxRTC.TabIndex = 3;
            this.chkboxRTC.Text = "Controllers and triggers (RTC)";
            this.chkboxRTC.UseVisualStyleBackColor = true;
            // 
            // chkboxRR
            // 
            this.chkboxRR.AutoSize = true;
            this.chkboxRR.Location = new System.Drawing.Point(29, 65);
            this.chkboxRR.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkboxRR.Name = "chkboxRR";
            this.chkboxRR.Size = new System.Drawing.Size(220, 21);
            this.chkboxRR.TabIndex = 2;
            this.chkboxRR.Text = "Rainfall runoff model (lumped)";
            this.chkboxRR.UseVisualStyleBackColor = true;
            // 
            // chkboxFlow
            // 
            this.chkboxFlow.AutoSize = true;
            this.chkboxFlow.Location = new System.Drawing.Point(29, 37);
            this.chkboxFlow.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkboxFlow.Name = "chkboxFlow";
            this.chkboxFlow.Size = new System.Drawing.Size(168, 21);
            this.chkboxFlow.TabIndex = 0;
            this.chkboxFlow.Text = "Water flow model (1d)";
            this.chkboxFlow.UseVisualStyleBackColor = true;
            // 
            // SelectSobekModelsWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SelectSobekModelsWizardPage";
            this.Size = new System.Drawing.Size(524, 149);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkboxRTC;
        private System.Windows.Forms.CheckBox chkboxRR;
        private System.Windows.Forms.CheckBox chkboxFlow;
    }
}
