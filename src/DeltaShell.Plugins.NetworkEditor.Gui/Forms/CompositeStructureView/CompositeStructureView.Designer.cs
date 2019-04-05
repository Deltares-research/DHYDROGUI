using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    partial class CompositeStructureView
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (Presenter != null)
                {
                    Presenter.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompositeStructureView));
            this.completeViewContainer = new System.Windows.Forms.SplitContainer();
            this.upperPartViews = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage = new System.Windows.Forms.TabPage();
            this.structureView = new StructureView();
            this.networkSideView = new NetworkSideView.NetworkSideView();
            this.completeViewContainer.Panel1.SuspendLayout();
            this.completeViewContainer.Panel2.SuspendLayout();
            this.completeViewContainer.SuspendLayout();
            this.upperPartViews.Panel1.SuspendLayout();
            this.upperPartViews.Panel2.SuspendLayout();
            this.upperPartViews.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // completeViewContainer
            // 
            this.completeViewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.completeViewContainer.Location = new System.Drawing.Point(0, 0);
            this.completeViewContainer.Name = "completeViewContainer";
            this.completeViewContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // completeViewContainer.Panel1
            // 
            this.completeViewContainer.Panel1.Controls.Add(this.upperPartViews);
            // 
            // completeViewContainer.Panel2
            // 
            this.completeViewContainer.Panel2.Controls.Add(this.tabControl);
            this.completeViewContainer.Size = new System.Drawing.Size(597, 438);
            this.completeViewContainer.SplitterDistance = 199;
            this.completeViewContainer.TabIndex = 0;
            // 
            // upperPartViews
            // 
            this.upperPartViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.upperPartViews.Location = new System.Drawing.Point(0, 0);
            this.upperPartViews.Name = "upperPartViews";
            // 
            // upperPartViews.Panel1
            // 
            this.upperPartViews.Panel1.Controls.Add(this.structureView);
            // 
            // upperPartViews.Panel2
            // 
            this.upperPartViews.Panel2.Controls.Add(this.networkSideView);
            this.upperPartViews.Size = new System.Drawing.Size(597, 199);
            this.upperPartViews.SplitterDistance = 293;
            this.upperPartViews.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl.Controls.Add(this.tabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl.ItemSize = new System.Drawing.Size(22, 90);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(597, 235);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.TabIndex = 0;
            this.tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl_DrawItem);
            // 
            // tabPage
            // 
            this.tabPage.Location = new System.Drawing.Point(94, 4);
            this.tabPage.Name = "tabPage";
            this.tabPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage.Size = new System.Drawing.Size(499, 227);
            this.tabPage.TabIndex = 5;
            this.tabPage.Text = "tabPage";
            this.tabPage.UseVisualStyleBackColor = true;
            // 
            // structureView
            // 
            this.structureView.Data = null;
            this.structureView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.structureView.Image = ((System.Drawing.Image)(resources.GetObject("structureView.Image")));
            this.structureView.Location = new System.Drawing.Point(0, 0);
            this.structureView.Name = "structureView";
            this.structureView.SelectedStructure = null;
            this.structureView.Size = new System.Drawing.Size(293, 199);
            this.structureView.TabIndex = 0;
            // 
            // networkSideView
            // 
            this.networkSideView.Data = null;
            this.networkSideView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.networkSideView.Image = null;
            this.networkSideView.Location = new System.Drawing.Point(0, 0);
            this.networkSideView.Name = "networkSideView";
            this.networkSideView.SelectedFeature = null;
            this.networkSideView.Size = new System.Drawing.Size(300, 199);
            this.networkSideView.TabIndex = 0;
            // 
            // CompositeStructureView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.completeViewContainer);
            this.Name = "CompositeStructureView";
            this.Size = new System.Drawing.Size(597, 438);
            this.completeViewContainer.Panel1.ResumeLayout(false);
            this.completeViewContainer.Panel2.ResumeLayout(false);
            this.completeViewContainer.ResumeLayout(false);
            this.upperPartViews.Panel1.ResumeLayout(false);
            this.upperPartViews.Panel2.ResumeLayout(false);
            this.upperPartViews.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer completeViewContainer;
        private System.Windows.Forms.SplitContainer upperPartViews;
        private TabControl tabControl;
        private StructureView structureView;
        private NetworkSideView.NetworkSideView networkSideView;
        private System.Windows.Forms.TabPage tabPage;
    }
}