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
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    /// <summary>
    /// Presents the nodes for the Class Map file function store.
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Gui.Swf.TreeViewNodePresenterBaseForPluginGui{FMClassMapFileFunctionStore}"/>
    public class FMClassMapFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<FMClassMapFileFunctionStore>
    {
        private static readonly IList<DataItem> DataItems = new List<DataItem>();

        /// <summary>
        /// Updates the node.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="node">The node.</param>
        /// <param name="nodeData">The node data.</param>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, FMClassMapFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Resources.unstrucWater;
        }

        /// <summary>
        /// Gets the child node objects.
        /// </summary>
        /// <param name="parentNodeData">The parent.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public override IEnumerable GetChildNodeObjects(FMClassMapFileFunctionStore parentNodeData, ITreeNode node)
        {
            WaterFlowFMModel model = GetModelByFunctionStore(parentNodeData);

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

        private IDataItem WrapIntoOutputItem(object o, FMClassMapFileFunctionStore store, string tag)
        {
            WaterFlowFMModel model = GetModelByFunctionStore(store);

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

        private WaterFlowFMModel GetModelByFunctionStore(FMClassMapFileFunctionStore store)
        {
            return Gui?.Application?.ProjectService.Project?.RootFolder?.Models?.OfType<WaterFlowFMModel>()
                      .FirstOrDefault(m => Equals(m.OutputClassMapFileStore, store));
        }
    }
}