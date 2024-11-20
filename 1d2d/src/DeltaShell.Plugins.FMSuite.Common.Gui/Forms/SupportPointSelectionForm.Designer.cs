using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    partial class SupportPointSelectionForm
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
            this.radioButtonAllSP = new System.Windows.Forms.RadioButton();
            this.radioButtonInactiveSP = new System.Windows.Forms.RadioButton();
            this.radioButtonActiveSP = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedSP = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radioButtonAllSP
            // 
            this.radioButtonAllSP.AutoSize = true;
            this.radioButtonAllSP.Location = new System.Drawing.Point(130, 91);
            this.radioButtonAllSP.Name = "radioButtonAllSP";
            this.radioButtonAllSP.Size = new System.Drawing.Size(105, 17);
            this.radioButtonAllSP.TabIndex = 3;
            this.radioButtonAllSP.TabStop = true;
            this.radioButtonAllSP.Text = "All support points";
            this.radioButtonAllSP.UseVisualStyleBackColor = true;
            // 
            // radioButtonInactiveSP
            // 
            this.radioButtonInactiveSP.AutoSize = true;
            this.radioButtonInactiveSP.Location = new System.Drawing.Point(130, 68);
            this.radioButtonInactiveSP.Name = "radioButtonInactiveSP";
            this.radioButtonInactiveSP.Size = new System.Drawing.Size(132, 17);
            this.radioButtonInactiveSP.TabIndex = 2;
            this.radioButtonInactiveSP.TabStop = true;
            this.radioButtonInactiveSP.Text = "Inactive support points";
            this.radioButtonInactiveSP.UseVisualStyleBackColor = true;
            // 
            // radioButtonActiveSP
            // 
            this.radioButtonActiveSP.AutoSize = true;
            this.radioButtonActiveSP.Location = new System.Drawing.Point(130, 45);
            this.radioButtonActiveSP.Name = "radioButtonActiveSP";
            this.radioButtonActiveSP.Size = new System.Drawing.Size(124, 17);
            this.radioButtonActiveSP.TabIndex = 1;
            this.radioButtonActiveSP.TabStop = true;
            this.radioButtonActiveSP.Text = "Active support points";
            this.radioButtonActiveSP.UseVisualStyleBackColor = true;
            // 
            // radioButtonSelectedSP
            // 
            this.radioButtonSelectedSP.AutoSize = true;
            this.radioButtonSelectedSP.Location = new System.Drawing.Point(130, 22);
            this.radioButtonSelectedSP.Name = "radioButtonSelectedSP";
            this.radioButtonSelectedSP.Size = new System.Drawing.Size(131, 17);
            this.radioButtonSelectedSP.TabIndex = 0;
            this.radioButtonSelectedSP.TabStop = true;
            this.radioButtonSelectedSP.Text = "Selected support point";
            this.radioButtonSelectedSP.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Apply operation to";
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(249, 147);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 5;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OkButton
            // 
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkButton.Location = new System.Drawing.Point(168, 147);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 6;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // SupportPointSelectionForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 182);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radioButtonAllSP);
            this.Controls.Add(this.radioButtonInactiveSP);
            this.Controls.Add(this.radioButtonSelectedSP);
            this.Controls.Add(this.radioButtonActiveSP);
            this.MinimumSize = new System.Drawing.Size(350, 220);
            this.Name = "SupportPointSelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose support points";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonAllSP;
        private System.Windows.Forms.RadioButton radioButtonInactiveSP;
        private System.Windows.Forms.RadioButton radioButtonActiveSP;
        private System.Windows.Forms.RadioButton radioButtonSelectedSP;
        private Label label1;
        private Button CancelButton;
        private Button OkButton;

    }
}