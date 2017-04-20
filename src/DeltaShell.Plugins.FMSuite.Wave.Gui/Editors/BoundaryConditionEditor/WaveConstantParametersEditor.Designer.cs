namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    partial class WaveConstantParametersEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.waveHeightBox = new System.Windows.Forms.TextBox();
            this.waveSpreadingBox = new System.Windows.Forms.TextBox();
            this.waveDirectionBox = new System.Windows.Forms.TextBox();
            this.wavePeriodBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Height:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Spreading:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Direction:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Period:";
            // 
            // waveHeightBox
            // 
            this.waveHeightBox.Location = new System.Drawing.Point(107, 14);
            this.waveHeightBox.Name = "waveHeightBox";
            this.waveHeightBox.Size = new System.Drawing.Size(100, 20);
            this.waveHeightBox.TabIndex = 1;
            // 
            // waveSpreadingBox
            // 
            this.waveSpreadingBox.Location = new System.Drawing.Point(107, 112);
            this.waveSpreadingBox.Name = "waveSpreadingBox";
            this.waveSpreadingBox.Size = new System.Drawing.Size(100, 20);
            this.waveSpreadingBox.TabIndex = 4;
            // 
            // waveDirectionBox
            // 
            this.waveDirectionBox.Location = new System.Drawing.Point(107, 80);
            this.waveDirectionBox.Name = "waveDirectionBox";
            this.waveDirectionBox.Size = new System.Drawing.Size(100, 20);
            this.waveDirectionBox.TabIndex = 3;
            // 
            // wavePeriodBox
            // 
            this.wavePeriodBox.Location = new System.Drawing.Point(107, 47);
            this.wavePeriodBox.Name = "wavePeriodBox";
            this.wavePeriodBox.Size = new System.Drawing.Size(100, 20);
            this.wavePeriodBox.TabIndex = 2;
            // 
            // WaveConstantParametersEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wavePeriodBox);
            this.Controls.Add(this.waveDirectionBox);
            this.Controls.Add(this.waveSpreadingBox);
            this.Controls.Add(this.waveHeightBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "WaveConstantParametersEditor";
            this.Size = new System.Drawing.Size(227, 156);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox waveHeightBox;
        private System.Windows.Forms.TextBox waveSpreadingBox;
        private System.Windows.Forms.TextBox waveDirectionBox;
        private System.Windows.Forms.TextBox wavePeriodBox;
    }
}
