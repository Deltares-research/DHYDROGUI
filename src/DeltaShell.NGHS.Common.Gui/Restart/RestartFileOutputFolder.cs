using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.RestartFiles;

namespace DeltaShell.NGHS.Common.Gui.Restart
{
    /// <summary>
    /// Represent the output restart file tree folder..
    /// </summary>
    /// <seealso cref="TreeFolder"/>
    public class RestartFileOutputTreeFolder : TreeFolder
    {
        private const string folderName = "Restart";

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartFileOutputTreeFolder"/> class.
        /// </summary>
        /// <param name="model">The restart model.</param>
        public RestartFileOutputTreeFolder(IRestartModel model) : base(model,
                                                                       GetDataItems(model).ToList(),
                                                                       folderName,
                                                                       FolderImageType.None) {}

        private static IEnumerable<IDataItem> GetDataItems(IRestartModel model)
        {
            Ensure.NotNull(model, nameof(model));

            foreach (RestartFile rstFile in model.RestartOutput)
            {
                yield return new DataItem(rstFile, DataItemRole.Output)
                {
                    Tag = rstFile.Path,
                };
            }
        }
    }
}