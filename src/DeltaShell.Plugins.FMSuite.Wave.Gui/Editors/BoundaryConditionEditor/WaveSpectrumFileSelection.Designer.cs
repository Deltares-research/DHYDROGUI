namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    partial class WaveSpectrumFileSelection
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
            this.spectrumFileBox = new System.Windows.Forms.TextBox();
            this.selectFileBtn = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Spectrum File:";
            // 
            // spectrumFileBox
            // 
            this.spectrumFileBox.Location = new System.Drawing.Point(154, 20);
            this.spectrumFileBox.Name = "spectrumFileBox";
            this.spectrumFileBox.ReadOnly = true;
            this.spectrumFileBox.Size = new System.Drawing.Size(147, 20);
            this.spectrumFileBox.TabIndex = 1;
            // 
            // selectFileBtn
            // 
            this.selectFileBtn.Location = new System.Drawing.Point(307, 18);
            this.selectFileBtn.Name = "selectFileBtn";
            this.selectFileBtn.Size = new System.Drawing.Size(30, 23);
            this.selectFileBtn.TabIndex = 2;
            this.selectFileBtn.Text = "...";
            this.selectFileBtn.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // WaveSpectrumFileSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.selectFileBtn);
            this.Controls.Add(this.spectrumFileBox);
            this.Controls.Add(this.label1);
            this.Name = "WaveSpectrumFileSelection";
            this.Size = new System.Drawing.Size(359, 61);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox spectrumFileBox;
        private System.Windows.Forms.Button selectFileBtn;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}
