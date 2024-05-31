using System.IO;
using System.IO.Abstractions;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// RTC model restart file writer.
    /// </summary>
    public sealed class RealTimeControlRestartXmlWriter : IRealTimeControlXmlWriter
    {
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlRestartXmlWriter"/> class.
        /// </summary>
        public RealTimeControlRestartXmlWriter()
            : this(new FileSystem())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlRestartXmlWriter"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public RealTimeControlRestartXmlWriter(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public void WriteToXml(RealTimeControlModel model, string directory)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNullOrEmpty(directory, nameof(directory));
            
            if (!fileSystem.Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($@"Directory '{directory}' does not exist.");
            }

            if (!model.UseRestart)
            {
                return;
            }

            string path = Path.Combine(directory, RealTimeControlXmlFiles.XmlImportState);
            
            using (StreamWriter stream = fileSystem.File.CreateText(path))
            {
                stream.Write(model.RestartInput.Content);
            }
        }
    }
}