namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    partial class ControlGroupGraphView
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
            this.controlGroupEditor = new ControlGroupEditor();
            this.SuspendLayout();
            // 
            // controlGroupEditor
            // 
            //this.controlGroupEditor.Data = null;
            this.controlGroupEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlGroupEditor.Gui = null;
            this.controlGroupEditor.Image = null;
            this.controlGroupEditor.Location = new System.Drawing.Point(0, 0);
            this.controlGroupEditor.Name = "controlGroupEditor";
            this.controlGroupEditor.Size = new System.Drawing.Size(397, 505);
            this.controlGroupEditor.TabIndex = 0;
            // 
            // ControlGroupGraphView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.controlGroupEditor);
            this.Name = "ControlGroupGraphView";
            this.Size = new System.Drawing.Size(820, 505);
            this.ResumeLayout(false);

        }

        #endregion

        private ControlGroupEditor controlGroupEditor;
    }
}
