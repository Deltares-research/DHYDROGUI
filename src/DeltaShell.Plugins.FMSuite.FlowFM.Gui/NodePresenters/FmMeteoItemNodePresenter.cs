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
    /// <summary>
    /// Class responsible for providing the nodes that are generated when adding in FmMeteoItemListNodePresenter.cs
    /// </summary>
    /// <seealso cref="IFmMeteoField" />
    class FmMeteoItemNodePresenter : FMSuiteNodePresenterBase<IFmMeteoField>
    {
        private static readonly Bitmap PrecipitationImage = Resources.precipitation;

        protected override string GetNodeText(IFmMeteoField data)
        {
            return data.Name;
        }
        protected override Image GetNodeImage(IFmMeteoField data)
        {
            if (data is IFmMeteoField)
            {
                return PrecipitationImage;
            }

            return null;
        }

        protected override bool RemoveNodeData(object parentNodeData, IFmMeteoField nodeData)
        {
            var fmMeteoFields = parentNodeData as IEventedList<IFmMeteoField>;
            if (fmMeteoFields != null)
            {
                fmMeteoFields.Remove(nodeData);
                return true;
            }
            return false;
        }

        protected override bool CanRemove(IFmMeteoField nodeData)
        {
            return true;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }
    }
}
