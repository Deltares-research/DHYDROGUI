namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    partial class WaveBoundaryConditionEditor
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
            this.BoundaryConditionEditor = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.BoundaryConditionEditor();
            this.SuspendLayout();
            // 
            // boundaryConditionEditor1
            // 
            this.BoundaryConditionEditor.AutoScroll = true;
            this.BoundaryConditionEditor.AutoScrollMinSize = new System.Drawing.Size(1020, 570);
            this.BoundaryConditionEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BoundaryConditionEditor.Image = null;
            this.BoundaryConditionEditor.Location = new System.Drawing.Point(0, 0);
            this.BoundaryConditionEditor.Name = "Editor";
            this.BoundaryConditionEditor.Size = new System.Drawing.Size(1079, 640);
            this.BoundaryConditionEditor.TabIndex = 0;
            this.BoundaryConditionEditor.ViewInfo = null;
            // 
            // WaveBoundaryConditionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BoundaryConditionEditor);
            this.Name = "WaveBoundaryConditionEditor";
            this.Size = new System.Drawing.Size(1079, 640);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
