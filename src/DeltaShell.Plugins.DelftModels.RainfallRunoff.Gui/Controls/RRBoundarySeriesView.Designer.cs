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
            this.label1 = new System.Windows.Forms.Label();
            this.waterLevelConstantTextBox = new System.Windows.Forms.TextBox();
            this.variableRadioButton = new System.Windows.Forms.RadioButton();
            this.constantRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
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
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(10);
            this.panel1.Size = new System.Drawing.Size(842, 86);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(222, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "m AD";
            // 
            // waterLevelConstantTextBox
            // 
            this.waterLevelConstantTextBox.Location = new System.Drawing.Point(137, 30);
            this.waterLevelConstantTextBox.Name = "waterLevelConstantTextBox";
            this.waterLevelConstantTextBox.Size = new System.Drawing.Size(79, 20);
            this.waterLevelConstantTextBox.TabIndex = 2;
            this.waterLevelConstantTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.waterLevelConstantTextBox.Validated += new System.EventHandler(this.WaterLevelConstantTextBoxValidated);
            // 
            // variableRadioButton
            // 
            this.variableRadioButton.AutoSize = true;
            this.variableRadioButton.Location = new System.Drawing.Point(13, 56);
            this.variableRadioButton.Name = "variableRadioButton";
            this.variableRadioButton.Size = new System.Drawing.Size(96, 17);
            this.variableRadioButton.TabIndex = 1;
            this.variableRadioButton.Text = "Use time series";
            this.variableRadioButton.UseVisualStyleBackColor = true;
            // 
            // constantRadioButton
            // 
            this.constantRadioButton.AutoSize = true;
            this.constantRadioButton.Checked = true;
            this.constantRadioButton.Location = new System.Drawing.Point(13, 31);
            this.constantRadioButton.Name = "constantRadioButton";
            this.constantRadioButton.Size = new System.Drawing.Size(88, 17);
            this.constantRadioButton.TabIndex = 0;
            this.constantRadioButton.TabStop = true;
            this.constantRadioButton.Text = "Use constant";
            this.constantRadioButton.UseVisualStyleBackColor = true;
            this.constantRadioButton.CheckedChanged += new System.EventHandler(this.ConstantRadioButtonCheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Water level boundary";
            // 
            // RainfallRunoffBoundaryDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "RRBoundarySeriesView";
            this.Size = new System.Drawing.Size(842, 467);
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
