namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    partial class WorkflowEditorControl
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
            this.workflowSelectionListBox = new System.Windows.Forms.ListBox();
            this.graphControl = new DelftTools.Controls.Swf.Graph.GraphControl();
            this.SuspendLayout();
            // 
            // workflowSelectionListBox
            // 
            this.workflowSelectionListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.workflowSelectionListBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.workflowSelectionListBox.FormattingEnabled = true;
            this.workflowSelectionListBox.Location = new System.Drawing.Point(0, 0);
            this.workflowSelectionListBox.Name = "workflowSelectionListBox";
            this.workflowSelectionListBox.Size = new System.Drawing.Size(183, 460);
            this.workflowSelectionListBox.TabIndex = 0;
            this.workflowSelectionListBox.SelectedIndexChanged += new System.EventHandler(this.WorkflowSelectionListBoxSelectedIndexChanged);
            // 
            // graphControl
            // 
            this.graphControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphControl.Location = new System.Drawing.Point(183, 0);
            this.graphControl.Name = "graphControl";
            this.graphControl.Padding = new System.Windows.Forms.Padding(5);
            this.graphControl.ReadOnly = true;
            this.graphControl.ScrollBars = true;
            this.graphControl.Size = new System.Drawing.Size(556, 460);
            this.graphControl.TabIndex = 3;
            // 
            // WorkflowEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.graphControl);
            this.Controls.Add(this.workflowSelectionListBox);
            this.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Name = "WorkflowEditorControl";
            this.Size = new System.Drawing.Size(739, 460);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox workflowSelectionListBox;
        private DelftTools.Controls.Swf.Graph.GraphControl graphControl;
    }
}
