using DelftTools.Utils;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    /// <summary>
    /// Class for connecting the output files of RTC.
    /// </summary>
    public class ReadOnlyOutputTextDocument : TextDocumentBase
    {
        /// <summary>
        /// Constructor for creating <see cref="ReadOnlyOutputTextDocument"/>.
        /// </summary>
        /// <param name="fileName">
        /// The fileName name with extension. Extension used by node presenter
        /// for selecting correct icon.
        /// </param>
        /// <param name="content"> The content of the file.s </param>
        public ReadOnlyOutputTextDocument(string fileName, string content) : base(true)
        {
            Ensure.NotNull(fileName, nameof(fileName));
            Ensure.NotNull(content, nameof(content));

            Name = fileName;
            Content = content;
        }
    }
}
