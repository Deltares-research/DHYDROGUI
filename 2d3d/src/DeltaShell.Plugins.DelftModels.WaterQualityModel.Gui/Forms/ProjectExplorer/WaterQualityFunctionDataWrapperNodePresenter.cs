using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    /// <summary>
    /// Water quality function data wrapper node presenter
    /// </summary>
    public class
        WaterQualityFunctionDataWrapperNodePresenter : TreeViewNodePresenterBase<WaterQualityFunctionDataWrapper>
    {
        private static readonly Bitmap FolderImage = Resources.Folder;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterQualityFunctionDataWrapper nodeData)
        {
            node.Image = FolderImage;
        }

        public override IEnumerable GetChildNodeObjects(WaterQualityFunctionDataWrapper parentNodeData, ITreeNode node)
        {
            var dataItem = node.Tag as IDataItem;
            if (dataItem == null)
            {
                return Enumerable.Empty<IDataItem>();
            }

            var waterQualityModel = dataItem.Owner as WaterQualityModel;
            if (waterQualityModel != null)
            {
                return waterQualityModel.AllDataItems.Where(
                    di => di.Role.HasFlag(DataItemRole.Input) &&
                          ShowDataItem(di, parentNodeData.Functions));
            }

            return Enumerable.Empty<IDataItem>();
        }

        private static bool ShowDataItem(IDataItem dataItem, IEnumerable<IFunction> functionsToShow)
        {
            var function = dataItem.Value as IFunction;
            return ShowFunction(function) && functionsToShow.Contains(function);
        }

        private static bool ShowFunction(IFunction function)
        {
            return function != null && !function.IsConst() && !function.IsFromHydroDynamics() &&
                   !function.IsSegmentFile();
        }
    }
}