using System;
using System.Collections.Generic;
using DelftTools.Controls;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView
{
    internal class BranchFeatureComparer : IComparer<ITreeNode>
    {
        private const double epsilon = 1.0e-7;

        public int Compare(ITreeNode x, ITreeNode y)
        {
            var branchFeatureX = (IBranchFeature)x.Tag;
            var branchFeatureY = (IBranchFeature)y.Tag;
            if (Math.Abs(branchFeatureX.Chainage - branchFeatureY.Chainage) < epsilon)
            {
                return 0;
            }
            if (branchFeatureX.Chainage > branchFeatureY.Chainage)
            {
                return 1;
            }
            return -1;
        }
    }
}