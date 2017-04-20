namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    partial class WaveBoundaryConditionDataView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaveBoundaryConditionDataView));
            this.spectralPanel = new System.Windows.Forms.Panel();
            this.functionViewPanel = new System.Windows.Forms.Panel();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.syncTimesButton = new System.Windows.Forms.Button();
            this.genDataButton = new System.Windows.Forms.Button();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // spectralPanel
            // 
            this.spectralPanel.AutoSize = true;
            this.spectralPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.spectralPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.spectralPanel.Location = new System.Drawing.Point(0, 0);
            this.spectralPanel.Name = "spectralPanel";
            this.spectralPanel.Size = new System.Drawing.Size(0, 310);
            this.spectralPanel.TabIndex = 0;
            // 
            // functionViewPanel
            // 
            this.functionViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.functionViewPanel.Location = new System.Drawing.Point(0, 0);
            this.functionViewPanel.Name = "functionViewPanel";
            this.functionViewPanel.Size = new System.Drawing.Size(999, 273);
            this.functionViewPanel.TabIndex = 1;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.syncTimesButton);
            this.buttonPanel.Controls.Add(this.genDataButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(0, 273);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(999, 37);
            this.buttonPanel.TabIndex = 2;
            // 
            // syncTimesButton
            // 
            this.syncTimesButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.syncTimesButton.Location = new System.Drawing.Point(201, 0);
            this.syncTimesButton.Name = "syncTimesButton";
            this.syncTimesButton.Size = new System.Drawing.Size(201, 37);
            this.syncTimesButton.TabIndex = 0;
            this.syncTimesButton.Text = "Copy to model Time Point data";
            this.syncTimesButton.UseVisualStyleBackColor = true;
            // 
            // genDataButton
            // 
            this.genDataButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.genDataButton.Image = ((System.Drawing.Image)(resources.GetObject("genDataButton.Image")));
            this.genDataButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.genDataButton.Location = new System.Drawing.Point(0, 0);
            this.genDataButton.Name = "genDataButton";
            this.genDataButton.Size = new System.Drawing.Size(201, 37);
            this.genDataButton.TabIndex = 17;
            this.genDataButton.Text = "Generate series ...";
            this.genDataButton.UseVisualStyleBackColor = true;
            this.genDataButton.Click += new System.EventHandler(this.genDataButton_Click);
            // 
            // WaveBoundaryConditionDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.functionViewPanel);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.spectralPanel);
            this.Name = "WaveBoundaryConditionDataView";
            this.Size = new System.Drawing.Size(999, 310);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel spectralPanel;
        private System.Windows.Forms.Panel functionViewPanel;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button syncTimesButton;
        private System.Windows.Forms.Button genDataButton;
    }
}
