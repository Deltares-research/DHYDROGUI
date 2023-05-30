namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    partial class RRBoundarySeriesView
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.waterLevelConstantTextBox = new System.Windows.Forms.TextBox();
            this.variableRadioButton = new System.Windows.Forms.RadioButton();
            this.constantRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.waterLevelConstantTextBox);
            this.panel1.Controls.Add(this.variableRadioButton);
            this.panel1.Controls.Add(this.constantRadioButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.panel1.Size = new System.Drawing.Size(1123, 106);
            this.panel1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Water level boundary";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(315, 44);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "m AD";
            // 
            // waterLevelConstantTextBox
            // 
            this.waterLevelConstantTextBox.Location = new System.Drawing.Point(200, 42);
            this.waterLevelConstantTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.waterLevelConstantTextBox.Name = "waterLevelConstantTextBox";
            this.waterLevelConstantTextBox.Size = new System.Drawing.Size(104, 22);
            this.waterLevelConstantTextBox.TabIndex = 2;
            this.waterLevelConstantTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.waterLevelConstantTextBox.Validated += new System.EventHandler(this.WaterLevelConstantTextBoxValidated);
            // 
            // variableRadioButton
            // 
            this.variableRadioButton.AutoSize = true;
            this.variableRadioButton.Location = new System.Drawing.Point(32, 69);
            this.variableRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.variableRadioButton.Name = "variableRadioButton";
            this.variableRadioButton.Size = new System.Drawing.Size(126, 21);
            this.variableRadioButton.TabIndex = 1;
            this.variableRadioButton.Text = "Use time series";
            this.variableRadioButton.UseVisualStyleBackColor = true;
            // 
            // constantRadioButton
            // 
            this.constantRadioButton.AutoSize = true;
            this.constantRadioButton.Checked = true;
            this.constantRadioButton.Location = new System.Drawing.Point(32, 42);
            this.constantRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.constantRadioButton.Name = "constantRadioButton";
            this.constantRadioButton.Size = new System.Drawing.Size(112, 21);
            this.constantRadioButton.TabIndex = 0;
            this.constantRadioButton.TabStop = true;
            this.constantRadioButton.Text = "Use constant";
            this.constantRadioButton.UseVisualStyleBackColor = true;
            this.constantRadioButton.CheckedChanged += new System.EventHandler(this.ConstantRadioButtonCheckedChanged);
            // 
            // RRBoundarySeriesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RRBoundarySeriesView";
            this.Size = new System.Drawing.Size(1123, 575);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton variableRadioButton;
        private System.Windows.Forms.RadioButton constantRadioButton;
        private System.Windows.Forms.TextBox waterLevelConstantTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
