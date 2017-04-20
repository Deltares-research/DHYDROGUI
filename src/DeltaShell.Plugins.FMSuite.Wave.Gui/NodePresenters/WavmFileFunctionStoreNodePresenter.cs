using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WavmFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<WavmFileFunctionStore>
    {
        private static readonly Bitmap Icon = new Bitmap(Properties.Resources.wave);

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WavmFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Icon;
        }

        public override IEnumerable GetChildNodeObjects(WavmFileFunctionStore parent, ITreeNode node)
        {
            var model =
                Gui.Application.Project.RootFolder.Models.OfType<WaveModel>()
                    .FirstOrDefault(m => m.WavmFunctionStores.Contains(parent));
            if (model == null)
            {
                yield return WrapIntoOutputItem(parent.Grid, parent, "grid");
            }
            foreach (var function in parent.Functions)
            {
                yield return WrapIntoOutputItem(function, parent, function.Name);
            }
        }

        private IDataItem WrapIntoOutputItem(object o, WavmFileFunctionStore parent, string tag)
        {
            var model =
                Gui.Application.Project.RootFolder.Models.OfType<WaveModel>()
                    .FirstOrDefault(m => m.WavmFunctionStores.Contains(parent));

            var subTag = tag;
            if (model != null)
            {
                var modelDataItem = model.GetDataItemByValue(parent);
                if (modelDataItem != null)
                {
                    subTag += modelDataItem.Tag;
                }
            }

            var existingItem = DataItems.FirstOrDefault(di => Equals(di.Tag, subTag) && Equals(di.Owner, model));
            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output) { Tag = subTag, Owner = model };
                DataItems.Add(newItem);
                return newItem;
            }

            if (!ReferenceEquals(existingItem.Value, o))
            {
                existingItem.Value = o;
            }
            return existingItem;
        }

        private static readonly IList<DataItem> DataItems = new List<DataItem>();
    }
}
