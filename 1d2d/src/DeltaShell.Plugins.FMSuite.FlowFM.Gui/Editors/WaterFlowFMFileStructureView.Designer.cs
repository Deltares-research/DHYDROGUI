namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class WaterFlowFMFileStructureView
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
            this.treeView = new DelftTools.Controls.Swf.TreeViewControls.TreeView();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.LabelEdit = true;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.SelectNodeOnRightMouseClick = true;
            this.treeView.Size = new System.Drawing.Size(339, 161);
            this.treeView.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 161);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid1.Size = new System.Drawing.Size(339, 72);
            this.propertyGrid1.TabIndex = 1;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // WaterFlowFMFileStructureView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.propertyGrid1);
            this.Name = "WaterFlowFMFileStructureView";
            this.Size = new System.Drawing.Size(339, 233);
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.TreeViewControls.TreeView treeView;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
    }
}
