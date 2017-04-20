using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Forms.NetworkTreeView;
using ITreeNode=DelftTools.Controls.ITreeNode;

namespace DeltaShell.Plugins.NetworkEditor.Forms.CrossSectionView
{
    public class CrossSectionTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<ICrossSection>
    {
        public CrossSectionTreeViewNodePresenter(IPluginGui pluginGui)
            : base(pluginGui)
        {
        }

        #region ITreeNodePresenter Members

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ICrossSection crossSection)
        {
            UpdateNodeText(crossSection, node);
            node.Image = GetIcon(crossSection.CrossSectionType);
        }

        private static Bitmap GetIcon(CrossSectionType type)
        {
            switch(type)
            {
                case CrossSectionType.GeometryBased:
                    return Properties.Resources.CrossSectionSmallXYZ;
                case CrossSectionType.TabulatedZW:
                    return Properties.Resources.CrossSectionTabulatedSmall;
                default:
                    return Properties.Resources.CrossSectionSmall;
            }
        }

        public override bool CanRemove(ICrossSection nodeData)
        {
            return true;
        }

        //public override IMenuItem GetContextMenu(object nodeData)
        //{
        //    return PluginGui.GetContextMenu(nodeData);
        //}

        public static void UpdateNodeText(ICrossSection feature, ITreeNode node)
        {
            if (feature != null)
            {
                var offsetString = feature.Offset.ToString("F", CultureInfo.InvariantCulture);
                node.Text = string.IsNullOrEmpty(feature.Name)
                                ? string.Format("<no name>: {0}", offsetString)
                                : string.Format("{0}: {1}", feature.Name.Clone(), offsetString);
            }
        }

        public override void OnPropertyChanged(ICrossSection crossSection, PropertyChangedEventArgs e)
        {
            var node = TreeView.GetNodeByTag(crossSection);

            if (node == null)
            {
                return;
            }

            if (!e.PropertyName.Equals("Chainage", StringComparison.Ordinal) &&
                !e.PropertyName.Equals("Name", StringComparison.Ordinal))
            {
                return;
            }

            //treeView.SuspendLayout();
            UpdateNodeText(crossSection, node);
            //BranchFeature branchFeature = (BranchFeature) node.Tag;

            ITreeNode parentNode;
            ////hack in case cross section is moved to another branch
            //if (node.Parent.Tag != crossSection.Branch)
            //{
            //    node.Parent.Nodes.Remove(node);
            //    parentNode = treeView.GetNodeByTag(crossSection.Branch);



            //    if(parentNode.IsLoaded)
            //    {
            //        parentNode.Nodes.Add(node);
            //    }
            //    else
            //    {
            //        //hack force update of node
            //        parentNode.Refresh();
            //        return;
            //    }
            //}
            parentNode = node.Parent;
            if (parentNode.Nodes.Count == 1) return;
            var index = parentNode.Nodes.IndexOf(node);
            var nodes = new List<ITreeNode>(parentNode.Nodes);
            nodes.Sort(new BranchFeatureComparer());
            var newIndex = nodes.IndexOf(node);
            if (newIndex == index)
            {
                return;
            }
            parentNode.Nodes.Remove(node);
            parentNode.Nodes.Insert(newIndex, node);
            //treeView.ResumeLayout();
        }

        public override void RemoveNodeData(ICrossSection crossSection)
        {
            //remove the cross section from both branch and networkschematization.
            var channel = (IChannel) crossSection.Branch;
            channel.BranchFeatures.Remove(crossSection);
        }

        /// <summary>
        /// Cross sections can be renamed by the user
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }


        #endregion
    }
}