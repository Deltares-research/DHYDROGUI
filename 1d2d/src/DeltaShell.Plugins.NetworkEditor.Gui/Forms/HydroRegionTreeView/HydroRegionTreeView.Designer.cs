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
            this.buttonMenuFeatureCopy = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuFeaturePasteInto = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuFeatureDelete = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonDataItemRename = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuFeatureProperties = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuNetwork = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonMenuNetworkPaste = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuNetworkRename = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuNetworkAddBranch = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuNetworkProperties = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuBranch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonMenuBranchZoomTo = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuBranchCopy = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchPaste = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchDelete = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchRename = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuBranchAddCS = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddWeir = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddPump = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchCulvert = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddBridge = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddLateralSource = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.buttonMenuBranchAddObservationPoint = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMenuBranchProperties = new DelftTools.Controls.Swf.ClonableToolStripMenuItem();
            this.contextMenuCrossSectionSectionTypes = new System.Windows.Forms.ContextMenuStrip(this.components);
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
            this.contextMenuCrossSectionSectionTypes.SuspendLayout();
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
            this.buttonMenuFeatureCopy,
            this.buttonMenuFeaturePasteInto,
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
            // showUsageToolStripMenuItem
            // 
            this.showUsageToolStripMenuItem.Name = "showUsageToolStripMenuItem";
            this.showUsageToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.showUsageToolStripMenuItem.Text = "Show Usage...";
            this.showUsageToolStripMenuItem.Visible = false;
            this.showUsageToolStripMenuItem.Click += new System.EventHandler(this.ShowUsageToolStripMenuItemClick);
            // 
            // setAsDefaultToolStripMenuItem
            // 
            this.setAsDefaultToolStripMenuItem.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.favorite;
            this.setAsDefaultToolStripMenuItem.Name = "setAsDefaultToolStripMenuItem";
            this.setAsDefaultToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.setAsDefaultToolStripMenuItem.Text = "Set as default";
            this.setAsDefaultToolStripMenuItem.Visible = false;
            this.setAsDefaultToolStripMenuItem.Click += new System.EventHandler(this.SetAsDefaultToolStripMenuItemClick);
            // 
            // placeOnEmptyBranchesToolStripMenuItem
            // 
            this.placeOnEmptyBranchesToolStripMenuItem.Name = "placeOnEmptyBranchesToolStripMenuItem";
            this.placeOnEmptyBranchesToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.placeOnEmptyBranchesToolStripMenuItem.Text = "Quick fix: Place on empty branches";
            this.placeOnEmptyBranchesToolStripMenuItem.Visible = false;
            this.placeOnEmptyBranchesToolStripMenuItem.Click += new System.EventHandler(this.PlaceOnEmptyBranchesToolStripMenuItemClick);
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
            // buttonMenuFeatureCopy
            // 
            this.buttonMenuFeatureCopy.Enabled = false;
            this.buttonMenuFeatureCopy.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.CopyHS;
            this.buttonMenuFeatureCopy.Name = "buttonMenuFeatureCopy";
            this.buttonMenuFeatureCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.buttonMenuFeatureCopy.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeatureCopy.Text = "&Copy";
            this.buttonMenuFeatureCopy.Click += new System.EventHandler(this.ButtonMenuFeatureCopyClick);
            // 
            // buttonMenuFeaturePasteInto
            // 
            this.buttonMenuFeaturePasteInto.Enabled = false;
            this.buttonMenuFeaturePasteInto.Name = "buttonMenuFeaturePasteInto";
            this.buttonMenuFeaturePasteInto.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.buttonMenuFeaturePasteInto.Size = new System.Drawing.Size(259, 22);
            this.buttonMenuFeaturePasteInto.Text = "Paste into";
            this.buttonMenuFeaturePasteInto.Click += new System.EventHandler(this.ButtonMenuFeaturePasteIntoClick);
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
            this.buttonMenuNetworkPaste,
            this.buttonMenuNetworkRename,
            this.toolStripSeparator6,
            this.buttonMenuNetworkAddBranch,
            this.toolStripSeparator5,
            this.buttonMenuNetworkProperties});
            this.contextMenuNetwork.Name = "contextMenuNetwork";
            this.contextMenuNetwork.Size = new System.Drawing.Size(144, 104);
            // 
            // buttonMenuNetworkPaste
            // 
            this.buttonMenuNetworkPaste.Enabled = false;
            this.buttonMenuNetworkPaste.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.PasteHS;
            this.buttonMenuNetworkPaste.Name = "buttonMenuNetworkPaste";
            this.buttonMenuNetworkPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.buttonMenuNetworkPaste.Size = new System.Drawing.Size(143, 22);
            this.buttonMenuNetworkPaste.Text = "&Paste";
            this.buttonMenuNetworkPaste.Click += new System.EventHandler(this.handleButtonPaste_Click);
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
            // buttonMenuNetworkAddBranch
            // 
            this.buttonMenuNetworkAddBranch.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.new_autobranch_small;
            this.buttonMenuNetworkAddBranch.Name = "buttonMenuNetworkAddBranch";
            this.buttonMenuNetworkAddBranch.Size = new System.Drawing.Size(143, 22);
            this.buttonMenuNetworkAddBranch.Text = "Add &Branch";
            this.buttonMenuNetworkAddBranch.Click += new System.EventHandler(this.handleButtonAddBranch_Click);
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
            this.buttonMenuBranchCopy,
            this.buttonMenuBranchPaste,
            this.buttonMenuBranchDelete,
            this.buttonMenuBranchRename,
            this.toolStripSeparator1,
            this.buttonMenuBranchAddCS,
            this.buttonMenuBranchAddWeir,
            this.buttonMenuBranchAddPump,
            this.buttonMenuBranchCulvert,
            this.buttonMenuBranchAddBridge,
            this.buttonMenuBranchAddLateralSource,
            this.buttonMenuBranchAddObservationPoint,
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
            // buttonMenuBranchCopy
            // 
            this.buttonMenuBranchCopy.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.CopyHS;
            this.buttonMenuBranchCopy.Name = "buttonMenuBranchCopy";
            this.buttonMenuBranchCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.buttonMenuBranchCopy.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchCopy.Text = "&Copy";
            this.buttonMenuBranchCopy.Click += new System.EventHandler(this.ButtonMenuBranchCopyClick);
            // 
            // buttonMenuBranchPaste
            // 
            this.buttonMenuBranchPaste.Enabled = false;
            this.buttonMenuBranchPaste.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.PasteHS;
            this.buttonMenuBranchPaste.Name = "buttonMenuBranchPaste";
            this.buttonMenuBranchPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.buttonMenuBranchPaste.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchPaste.Text = "&Paste...";
            this.buttonMenuBranchPaste.Click += new System.EventHandler(this.ButtonMenuBranchPasteClick);
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
            // buttonMenuBranchAddCS
            // 
            this.buttonMenuBranchAddCS.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.CrossSectionSmall;
            this.buttonMenuBranchAddCS.Name = "buttonMenuBranchAddCS";
            this.buttonMenuBranchAddCS.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchAddCS.Text = "Add &Cross Section YZ...";
            this.buttonMenuBranchAddCS.Click += new System.EventHandler(this.HandleButtonAddCrossSectionClick);
            // 
            // buttonMenuBranchAddWeir
            // 
            this.buttonMenuBranchAddWeir.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.WeirSmall;
            this.buttonMenuBranchAddWeir.Name = "buttonMenuBranchAddWeir";
            this.buttonMenuBranchAddWeir.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchAddWeir.Text = "Add &Weir";
            this.buttonMenuBranchAddWeir.Click += new System.EventHandler(this.handleButtonAddWeir_Click);
            // 
            // buttonMenuBranchAddPump
            // 
            this.buttonMenuBranchAddPump.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.PumpSmall;
            this.buttonMenuBranchAddPump.Name = "buttonMenuBranchAddPump";
            this.buttonMenuBranchAddPump.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchAddPump.Text = "Add &Pump";
            this.buttonMenuBranchAddPump.Click += new System.EventHandler(this.handleButtonAddPump_Click);
            // 
            // buttonMenuBranchCulvert
            // 
            this.buttonMenuBranchCulvert.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.CulvertSmall;
            this.buttonMenuBranchCulvert.Name = "buttonMenuBranchCulvert";
            this.buttonMenuBranchCulvert.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchCulvert.Text = "Add C&ulvert";
            this.buttonMenuBranchCulvert.Click += new System.EventHandler(this.handleButtonAddCulvert_Click);
            // 
            // buttonMenuBranchAddBridge
            // 
            this.buttonMenuBranchAddBridge.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.BridgeSmall;
            this.buttonMenuBranchAddBridge.Name = "buttonMenuBranchAddBridge";
            this.buttonMenuBranchAddBridge.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchAddBridge.Text = "Add &Bridge";
            this.buttonMenuBranchAddBridge.Click += new System.EventHandler(this.handleButtonAddBridge_Click);
            // 
            // buttonMenuBranchAddLateralSource
            // 
            this.buttonMenuBranchAddLateralSource.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.LateralSourceSmall;
            this.buttonMenuBranchAddLateralSource.Name = "buttonMenuBranchAddLateralSource";
            this.buttonMenuBranchAddLateralSource.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchAddLateralSource.Text = "Add &Lateral Source";
            this.buttonMenuBranchAddLateralSource.Click += new System.EventHandler(this.handleButtonAddLateralSource_Click);
            // 
            // buttonMenuBranchAddObservationPoint
            // 
            this.buttonMenuBranchAddObservationPoint.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.Observation;
            this.buttonMenuBranchAddObservationPoint.Name = "buttonMenuBranchAddObservationPoint";
            this.buttonMenuBranchAddObservationPoint.Size = new System.Drawing.Size(196, 22);
            this.buttonMenuBranchAddObservationPoint.Text = "Add &Observation Point";
            this.buttonMenuBranchAddObservationPoint.Click += new System.EventHandler(this.handleButtonAddObservationPoint_Click);
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
            // contextMenuCrossSectionSectionTypes
            // 
            this.contextMenuCrossSectionSectionTypes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSectionTypeToolStripMenuItem});
            this.contextMenuCrossSectionSectionTypes.Name = "contextMenuCrossSectionSectionTypes";
            this.contextMenuCrossSectionSectionTypes.Size = new System.Drawing.Size(168, 26);
            // 
            // addSectionTypeToolStripMenuItem
            // 
            this.addSectionTypeToolStripMenuItem.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.CrossSectionSectionType;
            this.addSectionTypeToolStripMenuItem.Name = "addSectionTypeToolStripMenuItem";
            this.addSectionTypeToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addSectionTypeToolStripMenuItem.Text = "Add Section Type";
            this.addSectionTypeToolStripMenuItem.Click += new System.EventHandler(this.AddSectionTypeToolStripMenuItemClick);
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
            // zWTabulatedToolStripMenuItem
            // 
            this.zWTabulatedToolStripMenuItem.Name = "zWTabulatedToolStripMenuItem";
            this.zWTabulatedToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.zWTabulatedToolStripMenuItem.Text = "ZW (Tabulated)";
            this.zWTabulatedToolStripMenuItem.Click += new System.EventHandler(this.ZWTabulatedToolStripMenuItemClick);
            // 
            // yZToolStripMenuItem
            // 
            this.yZToolStripMenuItem.Name = "yZToolStripMenuItem";
            this.yZToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.yZToolStripMenuItem.Text = "YZ";
            this.yZToolStripMenuItem.Click += new System.EventHandler(this.YZToolStripMenuItemClick);
            // 
            // contextMenuRoutes
            // 
            this.contextMenuRoutes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addRouteToolStripMenuItem});
            this.contextMenuRoutes.Name = "contextMenuRoutes";
            this.contextMenuRoutes.Size = new System.Drawing.Size(153, 48);
            // 
            // addRouteToolStripMenuItem
            // 
            this.addRouteToolStripMenuItem.Image = global::DeltaShell.Plugins.NetworkEditor.Gui.Properties.Resources.route;
            this.addRouteToolStripMenuItem.Name = "addRouteToolStripMenuItem";
            this.addRouteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addRouteToolStripMenuItem.Text = "Add Route";
            this.addRouteToolStripMenuItem.Click += new System.EventHandler(this.AddRouteToolStripMenuItemClick);
            // 
            // HydroRegionTreeView
            // 
            this.Name = "HydroRegionTreeView";
            this.contextMenuFeature.ResumeLayout(false);
            this.contextMenuNetwork.ResumeLayout(false);
            this.contextMenuBranch.ResumeLayout(false);
            this.contextMenuCrossSectionSectionTypes.ResumeLayout(false);
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
        private ClonableToolStripMenuItem buttonMenuBranchAddWeir;
        private ClonableToolStripMenuItem buttonMenuBranchAddPump;
        private ClonableToolStripMenuItem buttonMenuBranchCulvert;
        private ClonableToolStripMenuItem buttonMenuBranchAddBridge;
        private ClonableToolStripMenuItem buttonMenuBranchAddLateralSource;
        private ClonableToolStripMenuItem buttonMenuBranchProperties;
        private ClonableToolStripMenuItem buttonMenuFeatureProperties;
        private ClonableToolStripMenuItem buttonMenuNetworkRename;
        private ClonableToolStripMenuItem buttonMenuNetworkProperties;
        private ClonableToolStripMenuItem buttonMenuFeatureZoomTo;
        private ClonableToolStripMenuItem buttonMenuBranchZoomTo;
        private ClonableToolStripMenuItem buttonMenuFeaturePasteInto;
        private ClonableToolStripMenuItem addSectionTypeToolStripMenuItem;
        private ClonableToolStripMenuItem buttonMenuBranchAddObservationPoint;
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