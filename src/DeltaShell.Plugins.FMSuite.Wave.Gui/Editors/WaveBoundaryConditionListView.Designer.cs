namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    partial class WaveBoundaryConditionListView
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
            this.definitionPanel = new System.Windows.Forms.Panel();
            this.rbSp2File = new System.Windows.Forms.RadioButton();
            this.rbBoundarySegments = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.boundariesPanel = new System.Windows.Forms.Panel();
            this.definitionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // definitionPanel
            // 
            this.definitionPanel.Controls.Add(this.rbSp2File);
            this.definitionPanel.Controls.Add(this.rbBoundarySegments);
            this.definitionPanel.Controls.Add(this.label1);
            this.definitionPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.definitionPanel.Location = new System.Drawing.Point(0, 0);
            this.definitionPanel.Name = "definitionPanel";
            this.definitionPanel.Size = new System.Drawing.Size(360, 62);
            this.definitionPanel.TabIndex = 0;
            // 
            // rbSp2File
            // 
            this.rbSp2File.AutoSize = true;
            this.rbSp2File.Location = new System.Drawing.Point(129, 35);
            this.rbSp2File.Name = "rbSp2File";
            this.rbSp2File.Size = new System.Drawing.Size(170, 17);
            this.rbSp2File.TabIndex = 2;
            this.rbSp2File.Text = "from SWAN spectral file (*.sp2)";
            this.rbSp2File.UseVisualStyleBackColor = true;
            // 
            // rbBoundarySegments
            // 
            this.rbBoundarySegments.AutoSize = true;
            this.rbBoundarySegments.Checked = true;
            this.rbBoundarySegments.Location = new System.Drawing.Point(129, 12);
            this.rbBoundarySegments.Name = "rbBoundarySegments";
            this.rbBoundarySegments.Size = new System.Drawing.Size(207, 17);
            this.rbBoundarySegments.TabIndex = 1;
            this.rbBoundarySegments.TabStop = true;
            this.rbBoundarySegments.Text = "per boundary segment (xy-coordinates)";
            this.rbBoundarySegments.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Boundary Definition:";
            // 
            // boundariesPanel
            // 
            this.boundariesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundariesPanel.Location = new System.Drawing.Point(0, 62);
            this.boundariesPanel.Name = "boundariesPanel";
            this.boundariesPanel.Size = new System.Drawing.Size(360, 94);
            this.boundariesPanel.TabIndex = 1;
            // 
            // WaveBoundaryConditionListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.boundariesPanel);
            this.Controls.Add(this.definitionPanel);
            this.Name = "WaveBoundaryConditionListView";
            this.Size = new System.Drawing.Size(360, 156);
            this.definitionPanel.ResumeLayout(false);
            this.definitionPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel definitionPanel;
        private System.Windows.Forms.RadioButton rbSp2File;
        private System.Windows.Forms.RadioButton rbBoundarySegments;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel boundariesPanel;

    }
}
