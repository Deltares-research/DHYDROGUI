using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    public class WavmFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<WavmFileFunctionStore>
    {
        private static readonly Bitmap Icon = new Bitmap(Resources.wave);

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WavmFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Icon;
        }

        public override IEnumerable GetChildNodeObjects(WavmFileFunctionStore parentNodeData, ITreeNode node)
        {
            WaveModel model = Gui.Application.GetAllModelsInProject().OfType<WaveModel>()
                                 .FirstOrDefault(m => m.WavmFunctionStores.Contains(parentNodeData));
            if (model == null)
            {
                yield return WrapIntoOutputItem(parentNodeData.Grid, parentNodeData, "grid");
            }

            foreach (IFunction function in parentNodeData.Functions)
            {
                yield return WrapIntoOutputItem(function, parentNodeData, function.Name);
            }
        }

        private IDataItem WrapIntoOutputItem(object o, WavmFileFunctionStore parent, string tag)
        {
            WaveModel model = Gui.Application.GetAllModelsInProject().OfType<WaveModel>()
                                 .FirstOrDefault(m => m.WavmFunctionStores.Contains(parent));

            string subTag = tag;
            if (model != null)
            {
                IDataItem modelDataItem = model.GetDataItemByValue(parent);
                if (modelDataItem != null)
                {
                    subTag += modelDataItem.Tag;
                }
            }

            return new DataItem(o, DataItemRole.Output)
            {
                Tag = subTag,
                Owner = model
            };
        }
    }
}