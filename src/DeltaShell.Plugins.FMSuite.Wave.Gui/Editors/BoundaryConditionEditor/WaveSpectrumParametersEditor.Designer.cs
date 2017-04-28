namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    partial class WaveSpectralParametersEditor
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
            this.components = new System.ComponentModel.Container();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.spreadingTypeBox = new System.Windows.Forms.ComboBox();
            this.periodTypeBox = new System.Windows.Forms.ComboBox();
            this.shapeTypeBox = new System.Windows.Forms.ComboBox();
            this.gaussSpreadBox = new System.Windows.Forms.TextBox();
            this.peakEnhBox = new System.Windows.Forms.TextBox();
            this.gaussSpreadLabel = new System.Windows.Forms.Label();
            this.peakEnhLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // spreadingTypeBox
            // 
            this.spreadingTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spreadingTypeBox.FormattingEnabled = true;
            this.spreadingTypeBox.Location = new System.Drawing.Point(153, 71);
            this.spreadingTypeBox.Name = "spreadingTypeBox";
            this.spreadingTypeBox.Size = new System.Drawing.Size(105, 21);
            this.spreadingTypeBox.TabIndex = 22;
            // 
            // periodTypeBox
            // 
            this.periodTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.periodTypeBox.FormattingEnabled = true;
            this.periodTypeBox.Location = new System.Drawing.Point(153, 43);
            this.periodTypeBox.Name = "periodTypeBox";
            this.periodTypeBox.Size = new System.Drawing.Size(105, 21);
            this.periodTypeBox.TabIndex = 21;
            // 
            // shapeTypeBox
            // 
            this.shapeTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shapeTypeBox.FormattingEnabled = true;
            this.shapeTypeBox.Location = new System.Drawing.Point(153, 14);
            this.shapeTypeBox.Name = "shapeTypeBox";
            this.shapeTypeBox.Size = new System.Drawing.Size(105, 21);
            this.shapeTypeBox.TabIndex = 20;
            // 
            // gaussSpreadBox
            // 
            this.gaussSpreadBox.Location = new System.Drawing.Point(174, 152);
            this.gaussSpreadBox.Name = "gaussSpreadBox";
            this.gaussSpreadBox.Size = new System.Drawing.Size(84, 20);
            this.gaussSpreadBox.TabIndex = 24;
            // 
            // peakEnhBox
            // 
            this.peakEnhBox.Location = new System.Drawing.Point(174, 125);
            this.peakEnhBox.Name = "peakEnhBox";
            this.peakEnhBox.Size = new System.Drawing.Size(84, 20);
            this.peakEnhBox.TabIndex = 23;
            // 
            // gaussSpreadLabel
            // 
            this.gaussSpreadLabel.AutoSize = true;
            this.gaussSpreadLabel.Location = new System.Drawing.Point(11, 155);
            this.gaussSpreadLabel.Name = "gaussSpreadLabel";
            this.gaussSpreadLabel.Size = new System.Drawing.Size(91, 13);
            this.gaussSpreadLabel.TabIndex = 17;
            this.gaussSpreadLabel.Text = "Gaussian Spread:";
            // 
            // peakEnhLabel
            // 
            this.peakEnhLabel.AutoSize = true;
            this.peakEnhLabel.Location = new System.Drawing.Point(10, 128);
            this.peakEnhLabel.Name = "peakEnhLabel";
            this.peakEnhLabel.Size = new System.Drawing.Size(137, 13);
            this.peakEnhLabel.TabIndex = 16;
            this.peakEnhLabel.Text = "Peak Enhancement Factor:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Period:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Directional Spreading:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Shape:";
            // 
            // WaveSpectralParametersEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spreadingTypeBox);
            this.Controls.Add(this.periodTypeBox);
            this.Controls.Add(this.shapeTypeBox);
            this.Controls.Add(this.gaussSpreadBox);
            this.Controls.Add(this.peakEnhBox);
            this.Controls.Add(this.gaussSpreadLabel);
            this.Controls.Add(this.peakEnhLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(290, 190);
            this.Name = "WaveSpectralParametersEditor";
            this.Size = new System.Drawing.Size(290, 190);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ComboBox spreadingTypeBox;
        private System.Windows.Forms.ComboBox periodTypeBox;
        private System.Windows.Forms.ComboBox shapeTypeBox;
        private System.Windows.Forms.TextBox gaussSpreadBox;
        private System.Windows.Forms.TextBox peakEnhBox;
        private System.Windows.Forms.Label gaussSpreadLabel;
        private System.Windows.Forms.Label peakEnhLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
