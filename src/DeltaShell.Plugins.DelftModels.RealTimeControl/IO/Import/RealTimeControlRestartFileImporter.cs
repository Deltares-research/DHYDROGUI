using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import
{
    /// <summary>
    /// Importer for importing restart files for D-Flow RealTimeControl models.
    /// </summary>
    /// <seealso cref="IFileImporter"/>
    public class RealTimeControlRestartFileImporter : IFileImporter
    {
        private readonly Func<IEnumerable<IRealTimeControlModel>> getModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlRestartFileImporter"/> class.
        /// </summary>
        /// <param name="getModels">Func to retrieve the available collection of <seealso cref="IRealTimeControlModel"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="getModels"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlRestartFileImporter(Func<IEnumerable<IRealTimeControlModel>> getModels)
        {
            Ensure.NotNull(getModels, nameof(getModels));

            this.getModels = getModels;
        }

        /// <inheritdoc cref="IFileImporter"/>
        public string Name => "Restart File";

        /// <inheritdoc cref="IFileImporter"/>
        public string Category => "XML";

        /// <inheritdoc cref="IFileImporter"/>
        public string Description => string.Empty;

        /// <inheritdoc cref="IFileImporter"/>
        public Bitmap Image => Resources.rtcmodel;

        /// <inheritdoc cref="IFileImporter"/>
        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(RealTimeControlRestartFile);
            }
        }

        /// <inheritdoc cref="IFileImporter"/>
        public bool CanImportOnRootLevel => false;

        /// <inheritdoc cref="IFileImporter"/>
        public string FileFilter => "Real Time Control restart files|*.xml";

        /// <inheritdoc cref="IFileImporter"/>
        public string TargetDataDirectory { get; set; }

        /// <inheritdoc cref="IFileImporter"/>
        public bool ShouldCancel { get; set; }

        /// <inheritdoc cref="IFileImporter"/>
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc cref="IFileImporter"/>
        public bool OpenViewAfterImport => false;

        /// <summary>
        /// Indicates whether this importer can import on the specified <paramref name="targetObject"/>.
        /// </summary>
        /// <param name="targetObject">Target object to check.</param>
        /// <returns>
        /// <c>true</c> when the <paramref name="targetObject"/> is an input file of a <see cref="RealTimeControlModel"/>;
        /// otherwise <c>false</c>.
        /// </returns>
        public bool CanImportOn(object targetObject) => GetRealTimeControlModelWithRestartInput(targetObject) != null;

        /// <summary>
        /// Imports the restart file with path <paramref name="path"/>
        /// </summary>
        /// <param name="path"> The path of the restart file. </param>
        /// <param name="target"> The target restart file. </param>
        /// <returns>
        /// The imported <seealso cref="RealTimeControlRestartFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the restart file at the specified <paramref name="path"/> does not exist.
        /// </exception>
        public object ImportItem(string path, object target = null)
        {
            Ensure.NotNull(target, nameof(target));

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Restart file does not exist: {path}");
            }

            IRealTimeControlModel model = GetRealTimeControlModelWithRestartInput(target);
            
            model.RestartInput = RealTimeControlRestartFile.CreateFromFile(path);
            model.MarkOutputOutOfSync();
            
            return model.RestartInput;
        }

        private IRealTimeControlModel GetRealTimeControlModelWithRestartInput(object obj)
        {
            return obj is RealTimeControlRestartFile ? getModels().FirstOrDefault(m => ReferenceEquals(m.RestartInput, obj)) : null;
        }
    }
}