namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    internal partial class WindSelectionDialog
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
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.WndXRadioButton = new System.Windows.Forms.RadioButton();
            this.WndYRadioButton = new System.Windows.Forms.RadioButton();
            this.PressureRadioButton = new System.Windows.Forms.RadioButton();
            this.WndXYRadioButton = new System.Windows.Forms.RadioButton();
            this.WndMagDirRadioButton = new System.Windows.Forms.RadioButton();
            this.WndXGridRadioButton = new System.Windows.Forms.RadioButton();
            this.WndYGridRadioButton = new System.Windows.Forms.RadioButton();
            this.PressureGridRadioButton = new System.Windows.Forms.RadioButton();
            this.VelocityPressureGridRadioButton = new System.Windows.Forms.RadioButton();
            this.SpiderWebRadioButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkButton.Location = new System.Drawing.Point(118, 261);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(199, 261);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // WndXRadioButton
            // 
            this.WndXRadioButton.AutoSize = true;
            this.WndXRadioButton.Location = new System.Drawing.Point(12, 12);
            this.WndXRadioButton.Name = "WndXRadioButton";
            this.WndXRadioButton.Size = new System.Drawing.Size(205, 17);
            this.WndXRadioButton.TabIndex = 2;
            this.WndXRadioButton.TabStop = true;
            this.WndXRadioButton.Text = "Wind velocity x-component time series";
            this.WndXRadioButton.UseVisualStyleBackColor = true;
            // 
            // WndYRadioButton
            // 
            this.WndYRadioButton.AutoSize = true;
            this.WndYRadioButton.Location = new System.Drawing.Point(12, 35);
            this.WndYRadioButton.Name = "WndYRadioButton";
            this.WndYRadioButton.Size = new System.Drawing.Size(205, 17);
            this.WndYRadioButton.TabIndex = 3;
            this.WndYRadioButton.TabStop = true;
            this.WndYRadioButton.Text = "Wind velocity y-component time series";
            this.WndYRadioButton.UseVisualStyleBackColor = true;
            // 
            // PressureRadioButton
            // 
            this.PressureRadioButton.AutoSize = true;
            this.PressureRadioButton.Location = new System.Drawing.Point(12, 58);
            this.PressureRadioButton.Name = "PressureRadioButton";
            this.PressureRadioButton.Size = new System.Drawing.Size(132, 17);
            this.PressureRadioButton.TabIndex = 4;
            this.PressureRadioButton.TabStop = true;
            this.PressureRadioButton.Text = "Air pressure time series";
            this.PressureRadioButton.UseVisualStyleBackColor = true;
            // 
            // WndXYRadioButton
            // 
            this.WndXYRadioButton.AutoSize = true;
            this.WndXYRadioButton.Location = new System.Drawing.Point(12, 81);
            this.WndXYRadioButton.Name = "WndXYRadioButton";
            this.WndXYRadioButton.Size = new System.Drawing.Size(174, 17);
            this.WndXYRadioButton.TabIndex = 5;
            this.WndXYRadioButton.TabStop = true;
            this.WndXYRadioButton.Text = "Wind velocity vector time series";
            this.WndXYRadioButton.UseVisualStyleBackColor = true;
            // 
            // WndMagDirRadioButton
            // 
            this.WndMagDirRadioButton.AutoSize = true;
            this.WndMagDirRadioButton.Location = new System.Drawing.Point(12, 104);
            this.WndMagDirRadioButton.Name = "WndMagDirRadioButton";
            this.WndMagDirRadioButton.Size = new System.Drawing.Size(257, 17);
            this.WndMagDirRadioButton.TabIndex = 6;
            this.WndMagDirRadioButton.TabStop = true;
            this.WndMagDirRadioButton.Text = "Wind velocity magnitude and direction time series";
            this.WndMagDirRadioButton.UseVisualStyleBackColor = true;
            // 
            // WndXGridRadioButton
            // 
            this.WndXGridRadioButton.AutoSize = true;
            this.WndXGridRadioButton.Location = new System.Drawing.Point(12, 127);
            this.WndXGridRadioButton.Name = "WndXGridRadioButton";
            this.WndXGridRadioButton.Size = new System.Drawing.Size(188, 17);
            this.WndXGridRadioButton.TabIndex = 7;
            this.WndXGridRadioButton.TabStop = true;
            this.WndXGridRadioButton.Text = "Wind velocity x-component on grid";
            this.WndXGridRadioButton.UseVisualStyleBackColor = true;
            // 
            // WndYGridRadioButton
            // 
            this.WndYGridRadioButton.AutoSize = true;
            this.WndYGridRadioButton.Location = new System.Drawing.Point(12, 150);
            this.WndYGridRadioButton.Name = "WndYGridRadioButton";
            this.WndYGridRadioButton.Size = new System.Drawing.Size(188, 17);
            this.WndYGridRadioButton.TabIndex = 8;
            this.WndYGridRadioButton.TabStop = true;
            this.WndYGridRadioButton.Text = "Wind velocity y-component on grid";
            this.WndYGridRadioButton.UseVisualStyleBackColor = true;
            // 
            // PressureGridRadioButton
            // 
            this.PressureGridRadioButton.AutoSize = true;
            this.PressureGridRadioButton.Location = new System.Drawing.Point(12, 173);
            this.PressureGridRadioButton.Name = "PressureGridRadioButton";
            this.PressureGridRadioButton.Size = new System.Drawing.Size(115, 17);
            this.PressureGridRadioButton.TabIndex = 9;
            this.PressureGridRadioButton.TabStop = true;
            this.PressureGridRadioButton.Text = "Air pressure on grid";
            this.PressureGridRadioButton.UseVisualStyleBackColor = true;
            // 
            // VelocityPressureGridRadioButton
            // 
            this.VelocityPressureGridRadioButton.AutoSize = true;
            this.VelocityPressureGridRadioButton.Location = new System.Drawing.Point(12, 196);
            this.VelocityPressureGridRadioButton.Name = "VelocityPressureGridRadioButton";
            this.VelocityPressureGridRadioButton.Size = new System.Drawing.Size(228, 17);
            this.VelocityPressureGridRadioButton.TabIndex = 10;
            this.VelocityPressureGridRadioButton.TabStop = true;
            this.VelocityPressureGridRadioButton.Text = "Wind velocity and air pressure on curvi-grid";
            this.VelocityPressureGridRadioButton.UseVisualStyleBackColor = true;
            // 
            // SpiderWebRadioButton
            // 
            this.SpiderWebRadioButton.AutoSize = true;
            this.SpiderWebRadioButton.Location = new System.Drawing.Point(12, 219);
            this.SpiderWebRadioButton.Name = "SpiderWebRadioButton";
            this.SpiderWebRadioButton.Size = new System.Drawing.Size(98, 17);
            this.SpiderWebRadioButton.TabIndex = 11;
            this.SpiderWebRadioButton.TabStop = true;
            this.SpiderWebRadioButton.Text = "Spider web grid";
            this.SpiderWebRadioButton.UseVisualStyleBackColor = true;
            // 
            // WindSelectionDialog
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 296);
            this.Controls.Add(this.SpiderWebRadioButton);
            this.Controls.Add(this.VelocityPressureGridRadioButton);
            this.Controls.Add(this.PressureGridRadioButton);
            this.Controls.Add(this.WndYGridRadioButton);
            this.Controls.Add(this.WndXGridRadioButton);
            this.Controls.Add(this.WndMagDirRadioButton);
            this.Controls.Add(this.WndXYRadioButton);
            this.Controls.Add(this.PressureRadioButton);
            this.Controls.Add(this.WndYRadioButton);
            this.Controls.Add(this.WndXRadioButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.MaximumSize = new System.Drawing.Size(300, 334);
            this.MinimumSize = new System.Drawing.Size(300, 334);
            this.Name = "WindSelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select wind item";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.RadioButton WndXRadioButton;
        private System.Windows.Forms.RadioButton WndYRadioButton;
        private System.Windows.Forms.RadioButton PressureRadioButton;
        private System.Windows.Forms.RadioButton WndXYRadioButton;
        private System.Windows.Forms.RadioButton WndMagDirRadioButton;
        private System.Windows.Forms.RadioButton WndXGridRadioButton;
        private System.Windows.Forms.RadioButton WndYGridRadioButton;
        private System.Windows.Forms.RadioButton PressureGridRadioButton;
        private System.Windows.Forms.RadioButton VelocityPressureGridRadioButton;
        private System.Windows.Forms.RadioButton SpiderWebRadioButton;
    }
}