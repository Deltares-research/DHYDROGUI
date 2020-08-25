using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart
{
    /// <summary>
    /// Represent the output restart file tree folder..
    /// </summary>
    /// <seealso cref="TreeFolder"/>
    public class RealTimeControlRestartFileOutputTreeFolder : TreeFolder
    {
        private const string folderName = "Restart";

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlRestartFileOutputTreeFolder"/> class.
        /// </summary>
        /// <param name="model">The restart model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlRestartFileOutputTreeFolder(RealTimeControlModel model) : base(model,
                                                                                             GetDataItems(model).ToList(),
                                                                                             folderName,
                                                                                             FolderImageType.None) {}

        private static IEnumerable<IDataItem> GetDataItems(RealTimeControlModel model)
        {
            Ensure.NotNull(model, nameof(model));

            foreach (RealTimeControlRestartFile rstFile in model.RestartOutput)
            {
                yield return new DataItem(rstFile, DataItemRole.Output) {Tag = rstFile.Name};
            }
        }
    }
}