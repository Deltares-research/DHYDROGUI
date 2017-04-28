using System;
using DelftTools.Controls;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class TemperatureCoverageNodePresenter : FunctionNodePresenter
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IFunction nodeData)
        {
            base.UpdateNode(parentNode, node, nodeData);
            if(nodeData.Name.Equals("Initial Temperature"))
                node.Image = Properties.Resources.Temperature;
        }

        public override Type NodeTagType
        {
            get { return typeof(NetworkCoverage); }
        }
    }
}