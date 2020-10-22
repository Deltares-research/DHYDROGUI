using DelftTools.Utils;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public class ReadOnlyOutputTextDocument : TextDocumentBase
    {
        public ReadOnlyOutputTextDocument(string documentName, string content) : base(true)
        {
            Ensure.NotNull(documentName, nameof(documentName));
            Ensure.NotNull(content, nameof(content));

            Name = documentName;
            Content = content;
        }
    }
}
