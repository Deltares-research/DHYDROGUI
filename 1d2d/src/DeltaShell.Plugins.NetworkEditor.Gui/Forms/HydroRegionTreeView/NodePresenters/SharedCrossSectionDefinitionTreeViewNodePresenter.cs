using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class SharedCrossSectionDefinitionTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<ICrossSectionDefinition>
    {
        private static Image defaultImage = Properties.Resources.favorite;

        public SharedCrossSectionDefinitionTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
            
        }
        
        private bool IsDefaultDefinition(ITreeNode parentNode, ICrossSectionDefinition nodeData)
        {
            var hydroNetwork = GetHydroNetwork(parentNode);
            return (hydroNetwork != null && hydroNetwork.DefaultCrossSectionDefinition == nodeData);
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ICrossSectionDefinition nodeData)
        {
            node.Text = nodeData.Name;
            var image = CrossSectionNodePresenterIconHelper.GetIcon(nodeData.CrossSectionType);

            if (IsDefaultDefinition(parentNode, nodeData))
            {
                image = (Image) image.Clone();
                var graphics = Graphics.FromImage(image);
                graphics.DrawImage(defaultImage, 0, 6, 10, 10);
                graphics.Dispose();
            }

            node.Image = image;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(ICrossSectionDefinition data, string newName)
        {
            if (data.Name != newName)
            {
                data.Name = newName;
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            if (GuiPlugin == null)
            {
                return null;
            }
            return GuiPlugin.GetContextMenu(sender, nodeData);
        }

        protected override bool CanRemove(ICrossSectionDefinition nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, ICrossSectionDefinition definition)
        {
            var network = GetHydroNetwork(TreeView.SelectedNode.Parent);
            if (network != null)
            {
                var crossSectionsUsingDefinitionBeingRemoved = definition.FindUsage(network);
                
                if (crossSectionsUsingDefinitionBeingRemoved.Count > 0)
                {
                    var crossSectionsList = string.Join("\n", crossSectionsUsingDefinitionBeingRemoved.Select(x => x.Name).ToArray());

                    var message =
                        String.Format(
                            "The cross section definition you are trying to delete is being used. " +
                            "If you continue, the definition will be replaced by local copies in each cross section. " +
                            "Are you sure you want to continue?\n\nThe following cross sections use this definition:\n{0}",
                            crossSectionsList);

                    if (MessageBox.Show(message, "Replace definition with local copies?", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    {
                        return false;
                    }
                    //fix the cross sections
                    foreach (var cs in crossSectionsUsingDefinitionBeingRemoved)
                    {
                        cs.MakeDefinitionLocal();
                    }
                }

                //actual delete
                var sharedDefinitions = network.SharedCrossSectionDefinitions;
                if (sharedDefinitions != null)
                {
                    sharedDefinitions.Remove(definition);
                }
            }
            else
            {
                throw new ArgumentException("Cannot find network! Did the treeview structure change?");
            }

            return true;
        }

        private IHydroNetwork GetHydroNetwork(ITreeNode sharedDefinitionsNode)
        {
            return (IHydroNetwork) sharedDefinitionsNode.Parent.Tag;
        }
    }
}