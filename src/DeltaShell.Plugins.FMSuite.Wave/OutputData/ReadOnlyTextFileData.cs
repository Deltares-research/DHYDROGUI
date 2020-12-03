using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="ReadOnlyTextFileData"/> defines the data obtained from a
    /// single text document.
    /// </summary>
    public class ReadOnlyTextFileData
    {
        /// <summary>
        /// Creates a new <see cref="ReadOnlyTextFileData"/>.
        /// </summary>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="documentName"/> or
        /// <paramref name="content"/> are <c>null</c>.
        /// </exception>
        public ReadOnlyTextFileData(string documentName, string content)
        {
            Ensure.NotNull(documentName, nameof(documentName));
            Ensure.NotNull(content, nameof(content));

            DocumentName = documentName;
            Content = content;
        }

        /// <summary>
        /// Gets the name of the document.
        /// </summary>
        /// <remarks>
        /// <see cref="DocumentName"/> is never <c>null</c>.
        /// </remarks>
        public string DocumentName { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <remarks>
        /// <see cref="DocumentName"/> is never <c>null</c>.
        /// </remarks>
        public string Content { get; }
    }
}