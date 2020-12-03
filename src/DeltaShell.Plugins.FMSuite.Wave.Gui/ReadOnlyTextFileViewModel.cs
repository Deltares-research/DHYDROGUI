using System;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    /// <summary>
    /// <see cref="ReadOnlyTextFileData"/> implements <see cref="TextDocumentBase"/>
    /// by forwarding the <see cref="Name"/> and <see cref="Content"/> to a
    /// <see cref="ReadOnlyTextFileData"/>.
    /// </summary>
    /// <seealso cref="TextDocumentBase" />
    public class ReadOnlyTextFileViewModel : TextDocumentBase
    {
        /// <summary>
        /// Creates a new <see cref="ReadOnlyTextFileViewModel"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public ReadOnlyTextFileViewModel(ReadOnlyTextFileData data)
        {
            Ensure.NotNull(data, nameof(data));
            Data = data;
        }

        public override string Name
        {
            get => Data.DocumentName;
            set => throw new NotSupportedException("Cannot modify a readonly text document.");
        }

        public override string Content 
        { 
            get => Data.Content; 
            set => throw new NotSupportedException("Cannot modify a readonly text document.");
        }

        /// <summary>
        /// Gets the underlying data.
        /// </summary>
        public ReadOnlyTextFileData Data { get; }
    }
}