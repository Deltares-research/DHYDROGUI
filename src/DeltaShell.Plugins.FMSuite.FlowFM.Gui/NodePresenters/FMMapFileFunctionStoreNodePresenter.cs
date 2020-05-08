using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class FMMapFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<FMMapFileFunctionStore>
    {
        private static readonly IList<DataItem> DataItems = new List<DataItem>();

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, FMMapFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Resources.unstrucWater;
        }

        public override IEnumerable GetChildNodeObjects(FMMapFileFunctionStore parent, ITreeNode node)
        {
            WaterFlowFMModel model =
                Gui.Application.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                   .FirstOrDefault(m => Equals(m.OutputMapFileStore, parent));

            if (model == null)
            {
                UnstructuredGridCoverage coverage = parent.Functions.OfType<UnstructuredGridCoverage>().FirstOrDefault();
                if (coverage != null)
                {
                    yield return WrapIntoOutputItem(coverage.Grid, parent, "grid");
                }
            }

            foreach (IFunction function in parent.Functions)
            {
                yield return WrapIntoOutputItem(function, parent, function.Name);
            }
        }

        private IDataItem WrapIntoOutputItem(object o, FMMapFileFunctionStore store, string tag)
        {
            WaterFlowFMModel model =
                Gui.Application.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                   .FirstOrDefault(m => Equals(m.OutputMapFileStore, store));

            DataItem existingItem = DataItems.FirstOrDefault(di => Equals(di.Tag, tag) && Equals(di.Owner, model));
            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output)
                {
                    Tag = tag,
                    Owner = model
                };
                DataItems.Add(newItem);
                return newItem;
            }

            return existingItem;
        }
    }
}