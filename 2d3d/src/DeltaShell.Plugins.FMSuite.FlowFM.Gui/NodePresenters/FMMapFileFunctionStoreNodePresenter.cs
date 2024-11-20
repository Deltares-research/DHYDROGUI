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
    public class FMMapFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<IFMMapFileFunctionStore>
    {
        private static readonly IList<DataItem> DataItems = new List<DataItem>();

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IFMMapFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Resources.unstrucWater;
        }

        public override IEnumerable GetChildNodeObjects(IFMMapFileFunctionStore parentNodeData, ITreeNode node)
        {
            WaterFlowFMModel model =
                Gui.Application.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                   .FirstOrDefault(m => Equals(m.OutputMapFileStore, parentNodeData));

            if (model == null)
            {
                UnstructuredGridCoverage coverage = parentNodeData.Functions.OfType<UnstructuredGridCoverage>().FirstOrDefault();
                if (coverage != null)
                {
                    yield return WrapIntoOutputItem(coverage.Grid, parentNodeData, "grid");
                }
            }

            foreach (IFunction function in parentNodeData.Functions)
            {
                yield return WrapIntoOutputItem(function, parentNodeData, function.Name);
            }
        }

        private IDataItem WrapIntoOutputItem(object o, IFMMapFileFunctionStore store, string tag)
        {
            WaterFlowFMModel model =
                Gui.Application.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
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