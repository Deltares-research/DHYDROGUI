using System;
using System.Collections.Generic;
using DelftTools.Controls;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView
{
    internal class BranchFeatureComparer : IComparer<ITreeNode>
    {
        public int Compare(ITreeNode x, ITreeNode y)
        {
            var branchFeatureX = (IBranchFeature) x.Tag;
            var branchFeatureY = (IBranchFeature) y.Tag;
            if (Math.Abs(branchFeatureX.Chainage - branchFeatureY.Chainage) < BranchFeature.Epsilon)
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