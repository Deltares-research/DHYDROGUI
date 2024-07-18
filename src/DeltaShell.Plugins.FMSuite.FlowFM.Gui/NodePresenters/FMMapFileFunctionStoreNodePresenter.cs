using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class FM1DFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<FM1DFileFunctionStore>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, FM1DFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Properties.Resources.waterLayers;
        }

        public override IEnumerable GetChildNodeObjects(FM1DFileFunctionStore parent, ITreeNode node)
        {
            var model =
                Gui.Application.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                    .FirstOrDefault(m => Equals(m.OutputMapFileStore, parent));

            if (model == null)
            {
                var coverage = parent.Functions.OfType<NetworkCoverage>().FirstOrDefault();
                if (coverage != null)
                {
                    yield return WrapIntoOutputItem(coverage.Network, parent, "network");
                }
            }
            foreach (var function in parent.Functions)
            {
                yield return WrapIntoOutputItem(function, parent, function.Name);
            }
        }

        private IDataItem WrapIntoOutputItem(object o, FM1DFileFunctionStore store, string tag)
        {
            var model =
                Gui.Application.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                    .FirstOrDefault(m => Equals(m.OutputMapFileStore, store));

            var existingItem = DataItems.FirstOrDefault(di => Equals(di.Tag, tag) && Equals(di.Owner, model));
            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output) { Tag = tag, Owner = model };
                DataItems.Add(newItem);
                return newItem;
            }
            return existingItem;
        }

        private static readonly IList<DataItem> DataItems = new List<DataItem>();
    }
    public class FMMapFileFunctionStoreNodePresenter : TreeViewNodePresenterBaseForPluginGui<FMMapFileFunctionStore>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, FMMapFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = Properties.Resources.unstrucWater;
        }

        public override IEnumerable GetChildNodeObjects(FMMapFileFunctionStore parent, ITreeNode node)
        {
            var model =
                Gui.Application.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                    .FirstOrDefault(m => Equals(m.OutputMapFileStore, parent));

            if (model == null)
            {
                var coverage = parent.Functions.OfType<UnstructuredGridCoverage>().FirstOrDefault();
                if (coverage != null)
                {
                    yield return WrapIntoOutputItem(coverage.Grid, parent, "grid");
                }
            }
            foreach (var function in parent.Functions)
            {
                yield return WrapIntoOutputItem(function, parent, function.Name);
            }
        }

        private IDataItem WrapIntoOutputItem(object o, FMMapFileFunctionStore store, string tag)
        {
            var model =
                Gui.Application.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>()
                    .FirstOrDefault(m => Equals(m.OutputMapFileStore, store));

            var existingItem = DataItems.FirstOrDefault(di => Equals(di.Tag, tag) && Equals(di.Owner, model));
            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output) { Tag = tag, Owner = model };
                DataItems.Add(newItem);
                return newItem;
            }
            return existingItem;
        }

        private static readonly IList<DataItem> DataItems = new List<DataItem>();
    }
}