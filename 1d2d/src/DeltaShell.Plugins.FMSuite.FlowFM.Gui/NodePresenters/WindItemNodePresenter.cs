using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    class WindItemNodePresenter: FMSuiteNodePresenterBase<IWindField>
    {
        private static readonly Bitmap UniformWindImage = Resources.TimeSeries;
        private static readonly Bitmap GriddedWindImage = Resources.FunctionGrid2D;
        private static readonly Bitmap SpiderWebWindImage = Resources.hurricane2;

        protected override string GetNodeText(IWindField data)
        {
            return data.Name;
        }

        protected override Image GetNodeImage(IWindField data)
        {
            if (data is UniformWindField)
            {
                return UniformWindImage;
            }
            if (data is GriddedWindField)
            {
                return GriddedWindImage;
            }
            if (data is SpiderWebWindField)
            {
                return SpiderWebWindImage;
            }
            return null;
        }

        protected override bool RemoveNodeData(object parentNodeData, IWindField nodeData)
        {
            var windFields = parentNodeData as IEventedList<IWindField>;
            if (windFields != null)
            {
                windFields.Remove(nodeData);
                return true;
            }
            return false;
        }

        protected override bool CanRemove(IWindField nodeData)
        {
            return true;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }
    }
}
