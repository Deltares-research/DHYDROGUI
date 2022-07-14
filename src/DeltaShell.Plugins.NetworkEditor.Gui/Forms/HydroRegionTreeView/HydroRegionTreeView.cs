using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.IO;
using MessageBox = DelftTools.Controls.Swf.MessageBox;
using TreeView = DelftTools.Controls.Swf.TreeViewControls.TreeView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView
{
    public partial class HydroRegionTreeView : UserControl, IView, ISuspendibleView
    {
        private readonly TreeView treeView;
        private readonly IGui gui;
        private IContainer components;
        private ContextMenuStrip contextMenuFeature;
        private ClonableToolStripMenuItem buttonMenuFeatureOpen;
        private ToolStripSeparator toolStripSeparator8;
        private ClonableToolStripMenuItem buttonMenuFeatureCut;
        private ClonableToolStripMenuItem buttonMenuFeatureCopy;
        private ClonableToolStripMenuItem buttonMenuFeatureDelete;
        private ClonableToolStripMenuItem buttonDataItemRename;
        private ContextMenuStrip contextMenuNetwork;
        private ClonableToolStripMenuItem buttonMenuNetworkAddBranch;
        private ClonableToolStripMenuItem buttonMenuNetworkPaste;
        private ContextMenuStrip contextMenuBranch;
        private ClonableToolStripMenuItem buttonMenuBranchCopy;
        private ClonableToolStripMenuItem buttonMenuBranchPaste;
        private ClonableToolStripMenuItem buttonMenuBranchDelete;
        private ClonableToolStripMenuItem buttonMenuBranchRename;
        private ClonableToolStripMenuItem buttonMenuBranchAddCS;
        private IHydroRegion region;
        private ContextMenuStrip contextMenuCrossSectionSectionTypes;

        public HydroRegionTreeView(GuiPlugin guiPlugin)
        {
            InitializeComponent();

            gui = guiPlugin.Gui;
            gui.SelectionChanged += GuiSelectionChanged;

            treeView = new TreeView
                           {
                               AllowDrop = true, 
                               Dock = DockStyle.Fill,
                           };

            AddNodePresenters(guiPlugin);

            treeView.KeyDown += TreeViewKeyDown;
            Controls.Add(treeView);
        }

        public new void Dispose()
        {
            gui.SelectionChanged -= GuiSelectionChanged;
            base.Dispose();
        }

        private void AddNodePresenters(GuiPlugin guiPlugin)
        {
            var treeNodePresenters = new ITreeNodePresenter[]
                                         {
                                             new HydroRegionTreeViewNodePresenter(guiPlugin),
                                             new NetworkTreeViewNodePresenter(guiPlugin),
                                             new DrainageBasinTreeViewNodePresenter(guiPlugin),
                                             new HydroAreaTreeViewNodePresenter(guiPlugin), 
                                             new CatchmentTypesNodePresenter(guiPlugin), 
                                             new CatchmentTypeNodePresenter(guiPlugin), 
                                             new ChannelTreeViewNodePresenter(guiPlugin),
                                             new CrossSectionTreeViewNodePresenter(guiPlugin),
                                             new LateralSourceTreeViewNodePresenter(guiPlugin),
                                             new RetentionNodePresenter(guiPlugin),
                                             new ObservationPointTreeViewNodePresenter(guiPlugin),
                                             new CrossSectionSectionTypeTreeViewNodePresenter(guiPlugin),
                                             new CrossSectionSectionTypesTreeViewNodePresenter(guiPlugin),
                                             new SharedCrossSectionDefinitionTreeViewNodePresenter(guiPlugin),
                                             new SharedCrossSectionDefinitionsTreeViewNodePresenter(guiPlugin),
                                             new NetworkRoutesTreeViewNodePresenter(guiPlugin),
                                             new NetworkRouteTreeViewNodePresenter(guiPlugin),
                                             new CompositeStructureTreeViewNodePresenter(guiPlugin),
                                             new PumpTreeViewNodePresenter(guiPlugin),
                                             new CulvertTreeViewNodePresenter(guiPlugin),
                                             new WeirTreeViewNodePresenter(guiPlugin),
                                             new BridgeTreeViewNodePresenter(guiPlugin),
                                             new CatchmentsTreeViewNodePresenter(guiPlugin),
                                             new CatchmentTreeViewNodePresenter(guiPlugin),
                                             new WasteWaterTreatmentPlantsTreeViewNodePresenter(guiPlugin),
                                             new WasteWaterTreatmentPlantTreeViewNodePresenter(guiPlugin),
                                             new RunoffBoundariesTreeViewNodePresenter(guiPlugin),
                                             new RunoffBoundaryTreeViewNodePresenter(guiPlugin)
                                         };

            foreach (var treeNodePresenter in treeNodePresenters)
            {
                TreeView.NodePresenters.Add(treeNodePresenter);
            }
        }

        public bool SynchronizingGuiSelection { get; private set; }

        private void GuiSelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            IFeature selectedFeature = null;

            // Try to select the network object in the treeview
            if (gui.Selection is IFeature)
            {
                selectedFeature = (IFeature) gui.Selection;
            }
            else if (gui.Selection is IEnumerable<IFeature>)
            {
                var features = ((IEnumerable<IFeature>)gui.Selection).ToList();
                if (features.Count == 0) return;

                selectedFeature = features[0];
            }

            if (selectedFeature == null) return;

            // Search by comparing the feature to all the node tags
            var treeNode = treeView.GetNodeByTag(selectedFeature);
            if (treeNode == null) return;

            // The node correspoinding to the feature was found: select it
            SynchronizingGuiSelection = true;
            treeView.SelectedNode = treeNode;
            SynchronizingGuiSelection = false;
        }

        private void TreeViewKeyDown(object sender, KeyEventArgs e)
        {          
            if ((e.KeyCode == Keys.Control || e.KeyCode == Keys.C) && 
                (treeView.SelectedNode.Tag is IBranch || treeView.SelectedNode.Tag is IBranchFeature))
            {
                HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard((INetworkFeature) treeView.SelectedNode.Tag);
            }

            if (e.KeyCode != Keys.Control && e.KeyCode != Keys.V) return;

            if (treeView.SelectedNode.Tag is IBranchFeature && HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard())
            {
                ButtonMenuFeaturePasteIntoClick(sender, e);   
            }

            if (treeView.SelectedNode.Tag is IChannel && HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard())
            {
                ButtonMenuBranchPasteClick(sender, e);
            }

            if (treeView.SelectedNode.Tag is IHydroNetwork && HydroNetworkCopyAndPasteHelper.IsChannelSetToClipBoard())
            {
                handleButtonPaste_Click(sender, e);
            }
        }

        #region IView Members

        object IView.Data
        {
            get { return region; }
            set { Region = (IHydroNetwork) value; }
        }

        public IHydroRegion Region
        {
            get { return region; }
            set
            {
                if (region == value)
                {
                    return;
                }

                SynchronizingGuiSelection = true;
                region = value;

                treeView.SelectedNode = null; 
                treeView.Data = Region;

                SynchronizingGuiSelection = false;
            }
        }
        
        public Image Image
        {
            get { return Properties.Resources.network_branches; }
            set { }
        }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        public TreeView TreeView
        {
            get { return treeView; }
        }

        #endregion

        public IMenuItem GetContextMenu(object node, object tag)
        {
            ITreeNode treeNode = (ITreeNode)node;
            SelectedRegion = GetParentRegionFromNode(node);

            var isActiveViewMapView = gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().Any() ;
            if (tag is IHydroNetwork)
            {
                buttonMenuNetworkPaste.Enabled = HydroNetworkCopyAndPasteHelper.IsChannelSetToClipBoard();
                return new MenuItemContextMenuStripAdapter(contextMenuNetwork);
            }
            if (tag is IEventedList<Route>)
            {
                return new MenuItemContextMenuStripAdapter(contextMenuRoutes);
            }
            if (tag is IEventedList<ICrossSectionDefinition>)
            {
                return new MenuItemContextMenuStripAdapter(contextMenuSharedCrossSectionDefinitions);
            }
            if (tag is ICrossSectionDefinition)
            {
                var strip = new ContextMenuStrip();
                strip.Items.Add(buttonDataItemRename);
                strip.Items.Add(buttonMenuFeatureDelete);
                strip.Items.Add(showUsageToolStripMenuItem);
                strip.Items.Add(setAsDefaultToolStripMenuItem);
                strip.Items.Add(placeOnEmptyBranchesToolStripMenuItem);
                buttonMenuFeatureDelete.Enabled = true;
                buttonDataItemRename.Enabled = true;
                showUsageToolStripMenuItem.Visible = true;
                setAsDefaultToolStripMenuItem.Visible = true;
                setAsDefaultToolStripMenuItem.Enabled = (SelectedNetwork != null && SelectedNetwork.DefaultCrossSectionDefinition != tag);
                placeOnEmptyBranchesToolStripMenuItem.Visible = CheckIfExistsEmptyBranchesWithinNetwork();
                return new MenuItemContextMenuStripAdapter(strip);
            }
            if (tag is IEventedList<CrossSectionSectionType>)
            {
                return new MenuItemContextMenuStripAdapter(contextMenuCrossSectionSectionTypes);
            }
            if (tag is CrossSectionSectionType)
            {
                var strip = new ContextMenuStrip();
                strip.Items.Add(buttonMenuFeatureDelete);
                ITreeNodePresenter p = treeNode.Presenter;
                var parentNodeData = treeView.SelectedNode.Parent.Tag;
                buttonMenuFeatureDelete.Enabled = p.CanRemove(parentNodeData, tag);
                return new MenuItemContextMenuStripAdapter(strip);
            }
            if (tag is IChannel)
            {
                buttonMenuBranchZoomTo.Enabled = isActiveViewMapView;
                buttonMenuBranchPaste.Enabled = HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard();
                return new MenuItemContextMenuStripAdapter(contextMenuBranch);
            }
            if (tag is HydroRegion)
            {
                return NetworkEditorGuiPlugin.Instance.GetContextMenu(node, tag);
            }
            if (tag is IFeature && treeView.SelectedNode.Parent != null)
            {
                ITreeNodePresenter p = treeNode.Presenter;
                
                var parentNodeData = treeView.SelectedNode.Parent.Tag;
                var clipBoardFeature = HydroNetworkCopyAndPasteHelper.GetBranchFeatureFromClipBoard();
                buttonMenuFeatureDelete.Enabled = p.CanRemove(parentNodeData, tag);
                buttonMenuFeatureCut.Enabled = p.CanRemove(parentNodeData, tag);
                buttonMenuFeatureCopy.Enabled = (tag is IBranchFeature);
                buttonMenuFeaturePasteInto.Enabled = ((tag is IBranchFeature) && clipBoardFeature != null && clipBoardFeature.GetType() == tag.GetType() && !(tag is ICompositeBranchStructure));
                buttonMenuFeatureZoomTo.Enabled = isActiveViewMapView;
                var contextMenuAdapter = new MenuItemContextMenuStripAdapter(contextMenuFeature);

//                if (tag is Catchment)
//                {
//                    //$%$*()#$*) stupid context menus this is crazy
//
//                    var catchment = tag as Catchment;
//
//                    var catchmentTypeMenu = new ChangeCatchmentTypeContextMenuHandler().Build(new[] {catchment});
//                    var strip = new ContextMenuStrip();
//                    strip.Items.Add(catchmentTypeMenu);
//
//                    var featureMenuItems = contextMenuAdapter.ContextMenuStrip.Items.OfType<ToolStripItem>().ToList();
//                    foreach (var item in featureMenuItems)
//                    {
//                        var clonableMenuitem = item as ClonableToolStripMenuItem;
//                        if (clonableMenuitem != null)
//                        {
//                            strip.Items.Add(clonableMenuitem.Clone());
//                        }
//                        else if (item is ToolStripSeparator)
//                        {
//                            strip.Items.Add(new ToolStripSeparator());
//                        }
//                        else
//                        {
//                            throw new NotSupportedException(string.Format("Toolstrip menu item: {0} must be clonable",
//                                                                          item));
//                        }
//                    }
//                    return new MenuItemContextMenuStripAdapter(strip);
//                }

                return contextMenuAdapter;
            }
            return null;
        }

        private IHydroRegion GetParentRegionFromNode(object obj)
        {
            var node = obj as ITreeNode;
            while (node != null)
            {
                if (node.Tag is IHydroRegion)
                {
                    return node.Tag as IHydroRegion;
                }
                node = node.Parent;
            }
            return null;
        }

        private IHydroRegion SelectedRegion { get; set; }
    
        private IHydroNetwork SelectedNetwork { get { return SelectedRegion as IHydroNetwork; } }

        private void handleButtonOpen_Click(object sender, EventArgs e)
        {
            gui.CommandHandler.OpenDefaultViewForSelection();
        }

        private void handleButtonAddBranch_Click(object sender, EventArgs e)
        {
            object selectedObject = treeView.SelectedNode.Tag;
            var network = (selectedObject as IHydroNetwork);

            if (network != null)
            {
                var channel = Channel.CreateDefault(network);

                // HACK: add geometry manually
                channel.Geometry =
                    new WKTReader().Read(String.Format("LINESTRING({0} 0,{0} 100)", network.Branches.Count*100));

                NetworkHelper.AddChannelToHydroNetwork(network, channel);
                channel.Name = HydroNetworkHelper.GetUniqueFeatureName(network, channel);
            }
        }

        private void handleButtonPaste_Click(object sender, EventArgs e)
        {
            if (SelectedNetwork == null)
                return;

            string errorMessage;
            if (!HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(SelectedNetwork, out errorMessage))
            {
                MessageBox.Show(errorMessage, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void handleButtonDelete_Click(object sender, EventArgs e)
        {
            TreeView.DeleteNodeData();
            return;
        }

        private void handleButtonRename_Click(object sender, EventArgs e)
        {
            treeView.StartLabelEdit();
        }

        private void handleButtonOpenWith_Click(object sender, EventArgs e)
        {
            gui.CommandHandler.OpenSelectViewDialog();
        }

        private void handleButtonAddLateralSource_Click(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                channel.BranchFeatures.Add(LateralSource.CreateDefault(channel));
            }
        }

        private void handleButtonAddBridge_Click(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                AddBranchFeatureToBranch(Bridge.CreateDefault(channel));
            }
        }

        private void handleButtonAddPump_Click(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel == null) return;

            var branchFeature = new Pump(false);
            BranchStructure.AddStructureToNetwork(branchFeature, channel);

            AddBranchFeatureToBranch(branchFeature);
        }

        private void handleButtonAddWeir_Click(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                var branchFeature = new Weir(true);
                BranchStructure.AddStructureToNetwork(branchFeature, channel);
            }
        }

        private void handleButtonAddCulvert_Click(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                AddBranchFeatureToBranch(Culvert.CreateDefault(channel));
            }
        }

        private void handleButtonAddObservationPoint_Click(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                channel.BranchFeatures.Add(ObservationPoint.CreateDefault(channel));
            }
        }

        private void handleButtonProperties_Click(object sender, EventArgs e)
        {
            gui.CommandHandler.ShowProperties();
        }

        private void AddBranchFeatureToBranch(IStructure1D branchFeature)
        {
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(branchFeature, branchFeature.Branch);
        }

        private void handleButtonZoomToItem_Click(object sender, EventArgs e)
        {
            var feature = treeView.SelectedNode.Tag as IFeature;
            if (feature != null)
            {
                var cmd = new MapZoomToFeatureCommand();
                cmd.Execute(feature);
            }
        }

        private void ButtonMenuFeatureCopyClick(object sender, EventArgs e)
        {
            var branchFeature = treeView.SelectedNode.Tag as IBranchFeature;
            if (branchFeature != null)
            {
                HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(branchFeature);
            }
        }

        private void ButtonMenuFeatureCutClick(object sender, EventArgs e)
        {

        }

        private void HandleButtonAddCrossSectionClick(object sender, EventArgs e)
        {
            var channel = treeView.SelectedNode.Tag as IChannel;
            if (channel == null)
            {
                return;
            }
            FormPasteBranchFeature formPasteCrossSection = new FormPasteBranchFeature
                                                               {
                                                                   Branch = channel,
                                                                   Title =
                                                                       "Insert new default cross section into channel.",
                                                               };          
            formPasteCrossSection.textBoxShift.Enabled = true;
            formPasteCrossSection.textBoxShift.Visible = true;
            formPasteCrossSection.labelShift.Visible = true;

            if (DialogResult.OK != formPasteCrossSection.ShowDialog())
            {
                return;
            }
            var crossSection = CrossSection.CreateDefault(CrossSectionType.YZ, channel,
                                                                       formPasteCrossSection.Chainage);
            channel.BranchFeatures.Add(crossSection);

            crossSection.Definition.ShiftLevel(formPasteCrossSection.Shift);
            crossSection.Name =  HydroNetworkHelper.GetUniqueFeatureName(region,crossSection);
            gui.Selection = crossSection;
        }

        private void ButtonMenuBranchCopyClick(object sender, EventArgs e)
        {
            var branch = treeView.SelectedNode.Tag as IBranch;
            if (branch != null)
            {
                HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(branch);
            }
        }

        private void ButtonMenuBranchPasteClick(object sender, EventArgs e)
        {
            var branch = treeView.SelectedNode.Tag as IChannel;
            if (branch == null) return;

            var source = HydroNetworkCopyAndPasteHelper.GetBranchFeatureFromClipBoard();
            if (source == null) return;

            var formPasteBranchFeature = new FormPasteBranchFeature
                                             {
                                                 Branch = branch,
                                                 Title = string.Format("Paste branch feature {0} into channel {1}", source.Name, branch.Name)
                                             };

            if (source is ICrossSection)
            {
                formPasteBranchFeature.textBoxShift.Enabled = true;
                formPasteBranchFeature.textBoxShift.Visible = true;
                formPasteBranchFeature.labelShift.Visible = true;
            }

            if (DialogResult.OK != formPasteBranchFeature.ShowDialog()) return;

            string errorMessage;
            if (!HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(branch, formPasteBranchFeature.Chainage, out errorMessage))
            {
                MessageBox.Show(errorMessage, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonMenuFeaturePasteIntoClick(object sender, EventArgs e)
        {
            var branchFeature = treeView.SelectedNode.Tag as IBranchFeature;
            if (branchFeature == null) return;

            string errorMessage;
            if (!HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(branchFeature, out errorMessage))
            {
                MessageBox.Show(errorMessage, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSectionTypeToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (SelectedNetwork != null)
            {
                var sectionType = new CrossSectionSectionType
                    {
                        Name = NetworkHelper.GetUniqueName(HydroNetwork.CrossSectionSectionFormat,
                                                        SelectedNetwork.CrossSectionSectionTypes, "section")
                    };

                SelectedNetwork.CrossSectionSectionTypes.Add(sectionType);

            }
        }
        
        private void ZWTabulatedToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddDefinitionToNetwork(CrossSectionDefinitionZW.CreateDefault());
        }

        private void YZToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddDefinitionToNetwork(CrossSectionDefinitionYZ.CreateDefault());
        }

        private void AddDefinitionToNetwork(ICrossSectionDefinition definition)
        {
            if (SelectedNetwork != null)
            {
                definition.Name = NetworkHelper.GetUniqueName("CrossSectionDefinition{0:D3}",
                                                              SelectedNetwork.SharedCrossSectionDefinitions, "");

                SelectedNetwork.SharedCrossSectionDefinitions.Add(definition);
            }
        }
        
        private void AddRouteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (SelectedNetwork != null)
            {
                HydroNetworkHelper.AddNewRouteToNetwork(SelectedNetwork);
            }
        }

        public void SuspendUpdates()
        {
            treeView.Data = null;
        }

        public void ResumeUpdates()
        {
            treeView.Data = region;
        }

        private void ShowUsageToolStripMenuItemClick(object sender, EventArgs e)
        {
            var definition = treeView.SelectedNode.Tag as ICrossSectionDefinition;

            if (definition != null && SelectedNetwork != null)
            {
                var usages = definition.FindUsage(SelectedNetwork);

                string message = string.Format("Cross section definition '{0}' ", definition.Name);

                if (usages.Any())
                {
                    var usage = string.Join("\n", usages.Select(
                        x => string.Format(" {0} at {1}, {2:0.###}", x.Name, x.Branch, x.Chainage)).ToArray());

                    message = string.Format("{0} is used in the following cross sections: \n\n{1}",
                                            message, usage);
                }
                else
                {
                    message = string.Format("{0} is unused", message);
                }

                var caption = string.Format("Usage of {0}", definition.Name);

                MessageBox.Show(message, caption);
            }
        }

        private void SetAsDefaultToolStripMenuItemClick(object sender, EventArgs e)
        {
            var definition = treeView.SelectedNode.Tag as ICrossSectionDefinition;

            if (definition != null && SelectedNetwork != null)
            {
                SelectedNetwork.DefaultCrossSectionDefinition = definition;
            }
        }
        
        private void PlaceOnEmptyBranchesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var definition = treeView.SelectedNode.Tag as ICrossSectionDefinition;

            if (definition != null && SelectedNetwork != null)
            {
                foreach (var channel in SelectedNetwork.Channels.Where(c => !c.CrossSections.Any()))
                {
                    var crossSection = new CrossSection(new CrossSectionDefinitionProxy(definition));
                    NetworkHelper.AddBranchFeatureToBranch(crossSection, channel, channel.Length / 2.0);

                    crossSection.Name = HydroNetworkHelper.GetUniqueFeatureName(SelectedNetwork, crossSection);
                }
            }
        }

        private bool CheckIfExistsEmptyBranchesWithinNetwork()
        {
            var channelsWithoutCrossSections = SelectedNetwork != null
                                                   ? SelectedNetwork.Channels.Where(c => !c.CrossSections.Any())
                                                   : new Channel[0];
            var count = channelsWithoutCrossSections.Count();
            return count > 0;
        }

        public void WaitUntilAllEventsAreProcessed()
        {
            treeView.WaitUntilAllEventsAreProcessed();
        }
    }
}