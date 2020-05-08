using System;
using System.Collections;
using DelftTools.Controls;
using DelftTools.Hydro;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView
{
    /// <summary>
    /// Sort branchfeatures based on their offset
    /// </summary>
    internal class TreeViewBranchFeatureNodeSorter : IComparer
    {
        private readonly ITreeNode nodeToSort;

        public TreeViewBranchFeatureNodeSorter(ITreeNode nodeToSort)
        {
            this.nodeToSort = nodeToSort;
            if (!(nodeToSort.Tag is IChannel))
            {
                throw new ArgumentException("Treenode is not a branchnode.");
            }
        }

        public int Compare(object x, object y)
        {
            var tx = (ITreeNode) x;
            var ty = (ITreeNode) y;

            if (tx.Parent == nodeToSort && ty.Parent == nodeToSort)
            {
                var crossSectionx = (IBranchFeature) tx.Tag;
                var crossSectiony = ty.Tag as IBranchFeature;

                if (crossSectiony == null || crossSectionx == null)
                {
                    return 0;
                }

                if (Math.Abs(crossSectionx.Chainage - crossSectiony.Chainage) < BranchFeature.Epsilon)
                {
                    return 0;
                }

                if (crossSectionx.Chainage > crossSectiony.Chainage)
                {
                    return 1;
                }

                return -1;
            }

            return 0;
        }
    }
}