using System.Windows.Forms;
using DelftTools.Controls.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView
{
    partial class HydroRegionTreeView
    {
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuFeature = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonMenuFeatureOpen = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuFeatureOpenWith = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuFeatureZoomTo = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.showUsageToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.setAsDefaultToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.placeOnEmptyBranchesToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.changeCatchmentTypeToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuFeatureCut = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuFeatureDelete = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonDataItemRename = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuFeatureProperties = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuNetwork = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonMenuNetworkRename = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuNetworkProperties = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuBranch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonMenuBranchZoomTo = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuBranchDelete = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchRename = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuBranchAddPump = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchCulvert = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddBridge = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddExtraResistance = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuBranchProperties = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.addSectionTypeToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuSharedCrossSectionDefinitions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addSharedCrossSectionDefinitionToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.zWTabulatedToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.yZToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuRoutes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addRouteToolStripMenuItem = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuFeature.SuspendLayout();
            this.contextMenuNetwork.SuspendLayout();
            this.contextMenuBranch.SuspendLayout();
            this.contextMenuSharedCrossSectionDefinitions.SuspendLayout();
            this.contextMenuRoutes.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuFeature
            // 
            this.contextMenuFeature.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonMenuFeatureOpen,
            this.buttonMenuFeatureOpenWith,
            this.buttonMenuFeatureZoomTo,
            this.showUsageToolStripMenuItem,
            this.setAsDefaultToolStripMenuItem,
            this.placeOnEmptyBranchesToolStripMenuItem,
            this.changeCatchmentTypeToolStripMenuItem,
            this.toolStripSeparator8,
            this.buttonMenuFeatureCut,
            this.buttonMenuFeatureDelete,
            this.buttonDataItemRename,
            this.toolStripSeparator3,
            this.buttonMenuFeatureProperties});
            this.contextMenuFeature.Name = "contextMenuDataItem";
            this.contextMenuFeature.Size = new System.Drawing.Size(260, 302);
            // 
            // buttonMenuFeatureOpen
            // 
            this.buttonMenuFeatureOpen.Name = "buttonMenuFeatureOpen";
            this.buttonMenuFeatureOpen.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureOpen.Text = "&Open";
            this.buttonMenuFeatureOpen.Click += new System.EventHandler(this.handleButtonOpen_Click);
            // 
            // buttonMenuFeatureOpenWith
            // 
            this.buttonMenuFeatureOpenWith.Name = "buttonMenuFeatureOpenWith";
            this.buttonMenuFeatureOpenWith.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureOpenWith.Text = "Open With...";
            this.buttonMenuFeatureOpenWith.Click += new System.EventHandler(this.handleButtonOpenWith_Click);
            // 
            // buttonMenuFeatureZoomTo
            // 
            this.buttonMenuFeatureZoomTo.Name = "buttonMenuFeatureZoomTo";
            this.buttonMenuFeatureZoomTo.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureZoomTo.Text = "Zoom to Feature";
            this.buttonMenuFeatureZoomTo.Click += new System.EventHandler(this.handleButtonZoomToItem_Click);
            // 
            // changeCatchmentTypeToolStripMenuItem
            // 
            this.changeCatchmentTypeToolStripMenuItem.Name = "changeCatchmentTypeToolStripMenuItem";
            this.changeCatchmentTypeToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.changeCatchmentTypeToolStripMenuItem.Text = "Change Type";
            this.changeCatchmentTypeToolStripMenuItem.Visible = false;
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(256, 6);
            // 
            // buttonMenuFeatureCut
            // 
            this.buttonMenuFeatureCut.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.cut;
            this.buttonMenuFeatureCut.Name = "buttonMenuFeatureCut";
            this.buttonMenuFeatureCut.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureCut.Text = "C&ut";
            this.buttonMenuFeatureCut.Click += new System.EventHandler(this.ButtonMenuFeatureCutClick);
            // 
            // buttonMenuFeatureDelete
            // 
            this.buttonMenuFeatureDelete.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.DeleteHS;
            this.buttonMenuFeatureDelete.Name = "buttonMenuFeatureDelete";
            this.buttonMenuFeatureDelete.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureDelete.Text = "&Delete";
            this.buttonMenuFeatureDelete.Click += new System.EventHandler(this.handleButtonDelete_Click);
            // 
            // buttonDataItemRename
            // 
            this.buttonDataItemRename.Name = "buttonDataItemRename";
            this.buttonDataItemRename.Size = new System.Drawing.Size(259, 22);
            this.buttonDataItemRename.Text = "&Rename";
            this.buttonDataItemRename.Click += new System.EventHandler(this.handleButtonRename_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(256, 6);
            // 
            // buttonMenuFeatureProperties
            // 
            this.buttonMenuFeatureProperties.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.PropertiesHS;
            this.buttonMenuFeatureProperties.Name = "buttonMenuFeatureProperties";
            this.buttonMenuFeatureProperties.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureProperties.Text = "&Properties";
            this.buttonMenuFeatureProperties.Click += new System.EventHandler(this.handleButtonProperties_Click);
            // 
            // contextMenuNetwork
            // 
            this.contextMenuNetwork.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonMenuNetworkRename,
            this.toolStripSeparator6,
            this.toolStripSeparator5,
            this.buttonMenuNetworkProperties});
            this.contextMenuNetwork.Name = "contextMenuNetwork";
            this.contextMenuNetwork.Size = new System.Drawing.Size(144, 104);
            // 
            // buttonMenuNetworkRename
            // 
            this.buttonMenuNetworkRename.Name = "buttonMenuNetworkRename";
            this.buttonMenuNetworkRename.Size = new System.Drawing.Size(143, 22);
            this.buttonMenuNetworkRename.Text = "&Rename";
            this.buttonMenuNetworkRename.Click += new System.EventHandler(this.handleButtonRename_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(140, 6);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(140, 6);
            // 
            // buttonMenuNetworkProperties
            // 
            this.buttonMenuNetworkProperties.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.PropertiesHS;
            this.buttonMenuNetworkProperties.Name = "buttonMenuNetworkProperties";
            this.buttonMenuNetworkProperties.Size = new System.Drawing.Size(143, 22);
            this.buttonMenuNetworkProperties.Text = "&Properties";
            this.buttonMenuNetworkProperties.Click += new System.EventHandler(this.handleButtonProperties_Click);
            // 
            // contextMenuBranch
            // 
            this.contextMenuBranch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonMenuBranchZoomTo,
            this.toolStripSeparator7,
            this.buttonMenuBranchDelete,
            this.buttonMenuBranchRename,
            this.toolStripSeparator1,
            this.buttonMenuBranchAddPump,
            this.buttonMenuBranchCulvert,
            this.buttonMenuBranchAddBridge,
            this.buttonMenuBranchAddExtraResistance,
            this.toolStripSeparator2,
            this.buttonMenuBranchProperties});
            this.contextMenuBranch.Name = "contextMenuDataItem";
            this.contextMenuBranch.Size = new System.Drawing.Size(197, 330);
            // 
            // buttonMenuBranchZoomTo
            // 
            this.buttonMenuBranchZoomTo.Name = "buttonMenuBranchZoomTo";
            this.buttonMenuBranchZoomTo.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchZoomTo.Text = "Zoom to Branch";
            this.buttonMenuBranchZoomTo.Click += new System.EventHandler(this.handleButtonZoomToItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(193, 6);
            // 
            // buttonMenuBranchDelete
            // 
            this.buttonMenuBranchDelete.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.DeleteHS;
            this.buttonMenuBranchDelete.Name = "buttonMenuBranchDelete";
            this.buttonMenuBranchDelete.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchDelete.Text = "&Delete";
            this.buttonMenuBranchDelete.Click += new System.EventHandler(this.handleButtonDelete_Click);
            // 
            // buttonMenuBranchRename
            // 
            this.buttonMenuBranchRename.Name = "buttonMenuBranchRename";
            this.buttonMenuBranchRename.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchRename.Text = "&Rename";
            this.buttonMenuBranchRename.Click += new System.EventHandler(this.handleButtonRename_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // buttonMenuBranchProperties
            // 
            this.buttonMenuBranchProperties.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.PropertiesHS;
            this.buttonMenuBranchProperties.Name = "buttonMenuBranchProperties";
            this.buttonMenuBranchProperties.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchProperties.Text = "Properties";
            this.buttonMenuBranchProperties.Click += new System.EventHandler(this.handleButtonProperties_Click);
            // 
            // contextMenuSharedCrossSectionDefinitions
            // 
            this.contextMenuSharedCrossSectionDefinitions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSharedCrossSectionDefinitionToolStripMenuItem});
            this.contextMenuSharedCrossSectionDefinitions.Name = "contextMenuSharedCrossSectionDefinitions";
            this.contextMenuSharedCrossSectionDefinitions.Size = new System.Drawing.Size(191, 26);
            // 
            // addSharedCrossSectionDefinitionToolStripMenuItem
            // 
            this.addSharedCrossSectionDefinitionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zWTabulatedToolStripMenuItem,
            this.yZToolStripMenuItem});
            this.addSharedCrossSectionDefinitionToolStripMenuItem.Name = "addSharedCrossSectionDefinitionToolStripMenuItem";
            this.addSharedCrossSectionDefinitionToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.addSharedCrossSectionDefinitionToolStripMenuItem.Text = "Add Shared Definition";
            // 
            // contextMenuRoutes
            // 
            this.contextMenuRoutes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addRouteToolStripMenuItem});
            this.contextMenuRoutes.Name = "contextMenuRoutes";
            this.contextMenuRoutes.Size = new System.Drawing.Size(153, 48);
           
            // 
            // HydroRegionTreeView
            // 
            this.Name = "HydroRegionTreeView";
            this.contextMenuFeature.ResumeLayout(false);
            this.contextMenuNetwork.ResumeLayout(false);
            this.contextMenuBranch.ResumeLayout(false);
            this.contextMenuSharedCrossSectionDefinitions.ResumeLayout(false);
            this.contextMenuRoutes.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private ClonableToolStripMenuItem buttonMenuFeatureOpenWith;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator7;
        private ContextMenuStrip contextMenuSharedCrossSectionDefinitions;
        private ContextMenuStrip contextMenuRoutes;
        private ClonableToolStripMenuItem buttonMenuBranchAddPump;
        private ClonableToolStripMenuItem buttonMenuBranchCulvert;
        private ClonableToolStripMenuItem buttonMenuBranchAddBridge;
        private ClonableToolStripMenuItem buttonMenuBranchProperties;
        private ClonableToolStripMenuItem buttonMenuFeatureProperties;
        private ClonableToolStripMenuItem buttonMenuNetworkRename;
        private ClonableToolStripMenuItem buttonMenuNetworkProperties;
        private ClonableToolStripMenuItem buttonMenuFeatureZoomTo;
        private ClonableToolStripMenuItem buttonMenuBranchZoomTo;
        private ClonableToolStripMenuItem addSectionTypeToolStripMenuItem;
        private ClonableToolStripMenuItem buttonMenuBranchAddExtraResistance;
        private ClonableToolStripMenuItem addSharedCrossSectionDefinitionToolStripMenuItem;
        private ClonableToolStripMenuItem zWTabulatedToolStripMenuItem;
        private ClonableToolStripMenuItem yZToolStripMenuItem;
        private ClonableToolStripMenuItem showUsageToolStripMenuItem;
        private ClonableToolStripMenuItem setAsDefaultToolStripMenuItem;
        private ClonableToolStripMenuItem placeOnEmptyBranchesToolStripMenuItem;
        private ClonableToolStripMenuItem addRouteToolStripMenuItem;
        private ClonableToolStripMenuItem changeCatchmentTypeToolStripMenuItem;

    }
}