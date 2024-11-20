using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public abstract class BranchFeatureTreeViewNodePresenterBase<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : class ,IBranchFeature
    {
        protected BranchFeatureTreeViewNodePresenterBase(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T data)
        {
            UpdateNodeText(data, node);
            UpdateImage(data, node);
        }

        protected override void OnPropertyChanged(T feature, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null) return;

            if (!e.PropertyName.Equals("Chainage", StringComparison.Ordinal) &&
                !e.PropertyName.Equals("Name", StringComparison.Ordinal)) return;
            
            UpdateNodeText(feature, node);

            var parentNode = node.Parent;
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
        }

        protected abstract Image GetImage(T feature);

        protected virtual string GetText(T feature)
        {
            var offsetString = feature.Chainage.ToString("F", CultureInfo.InvariantCulture);
            return string.Format("{0}: {1}",
                                     string.IsNullOrEmpty(feature.Name) ? "<no name>" : feature.Name.Clone(),
                                     offsetString);
        }

        private void UpdateImage(T feature, ITreeNode node)
        {
            var image = GetImage(feature);

            if (node.Image != image)
            {
                node.Image = image;
            }
        }

        private void UpdateNodeText(T feature, ITreeNode node)
        {
            if (feature == null) return;

            var text = GetText(feature);

            if (node.Text != text)
            {
                node.Text = text;
            }
        }
    }
}