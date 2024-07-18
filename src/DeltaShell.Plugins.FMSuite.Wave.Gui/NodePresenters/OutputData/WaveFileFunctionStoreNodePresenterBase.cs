using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataNodePresenter"/> provides the base
    /// implementation node presenter for file function stores.
    /// </summary>
    /// <seealso cref="TreeViewNodePresenterBaseForPluginGui{T}" />
    public abstract class WaveFileFunctionStoreNodePresenterBase<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : IFMNetCdfFileFunctionStore
    {
        private readonly Bitmap icon = new Bitmap(Resources.wave);

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
            node.Image = icon;
        }

        /// <summary>
        /// Gets the child coverages.
        /// </summary>
        /// <param name="nodeData">The node data.</param>
        /// <returns>
        /// A collection of child coverages of the provided <paramref name="nodeData"/>.
        /// </returns>
        protected IEnumerable GetChildCoverages(T nodeData) =>
            nodeData.Functions.Select(f => WrapIntoOutputItem(f, nodeData, f.Name));

        /// <summary>
        /// Wraps the into output item.
        /// </summary>
        /// <param name="o">The object to wrap.</param>
        /// <param name="parent">The parent data.</param>
        /// <param name="tag">The tag of the new data item.</param>
        /// <returns>
        /// A data item that wraps the provided <paramref name="o"/>.
        /// </returns>
        protected IDataItem WrapIntoOutputItem(object o, T parent, string tag) =>
            new DataItem(o, DataItemRole.Output)
            {
                Tag = tag,
                Owner = GetParentModel(parent) 
            };

        /// <summary>
        /// Determines whether the provided <paramref name="nodeData"/> is a
        /// stand-alone function store, or contained inside a model.
        /// </summary>
        /// <param name="nodeData">The node data.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="nodeData"/> is a stand-alone
        /// function store; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsStandAloneFunctionStore(T nodeData) =>
            GetParentModel(nodeData) == null;

        /// <summary>
        /// Determines whether <paramref name="nodeData"/> is contained in
        /// <paramref name="model"/>.
        /// </summary>
        /// <param name="nodeData">The node data.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// <c>true</c> if if <paramref name="nodeData"/> is contained in
        /// <paramref name="model"/>; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool IsContainedInModel(T nodeData, IWaveModel model);

        private IWaveModel GetParentModel(T nodeData) =>
            Gui.Application
               .ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<IWaveModel>()
               .FirstOrDefault(model => IsContainedInModel(nodeData, model));
    }
}