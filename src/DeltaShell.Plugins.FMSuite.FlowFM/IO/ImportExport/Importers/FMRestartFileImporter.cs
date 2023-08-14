using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using WaterFlowFMRestartModel = DeltaShell.NGHS.Common.Restart.IRestartModel<DeltaShell.Plugins.FMSuite.FlowFM.Restart.WaterFlowFMRestartFile>;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    
    /// <summary>
    /// Importer for importing restart files for D-Flow FM models.
    /// </summary>
    /// <seealso cref="IFileImporter"/>
    public class FMRestartFileImporter : IFileImporter
    {
        private readonly Func<IEnumerable<WaterFlowFMRestartModel>> getModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="FMRestartFileImporter"/> class.
        /// </summary>
        /// <param name="getModels">Func to retrieve the available collection of <seealso cref="WaterFlowFMRestartModel"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="getModels"/> is <c>null</c>.
        /// </exception>
        public FMRestartFileImporter(Func<IEnumerable<WaterFlowFMRestartModel>> getModels)
        {
            Ensure.NotNull(getModels, nameof(getModels));

            this.getModels = getModels;
        }

        /// <inheritdoc cref="IFileImporter"/>
        public string Name => "Restart File";

        /// <inheritdoc cref="IFileImporter"/>
        public string Category => "NetCdf";

        /// <inheritdoc cref="IFileImporter"/>
        public string Description => string.Empty;

        /// <inheritdoc cref="IFileImporter"/>
        public Bitmap Image => Resources.unstrucModel;

        /// <inheritdoc cref="IFileImporter"/>
        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(WaterFlowFMRestartFile);
            }
        }

        /// <inheritdoc cref="IFileImporter"/>
        public bool CanImportOnRootLevel => false;

        /// <inheritdoc cref="IFileImporter"/>
        public string FileFilter => $"FM restart files|*{FileConstants.RestartFileExtension};*{FileConstants.MapFileExtension}";

        /// <inheritdoc cref="IFileImporter"/>
        public string TargetDataDirectory { get; set; }

        /// <inheritdoc cref="IFileImporter"/>
        public bool ShouldCancel { get; set; }

        /// <inheritdoc cref="IFileImporter"/>
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc cref="IFileImporter"/>
        public bool OpenViewAfterImport { get; private set; }

        /// <summary>
        /// Indicates whether this importer can import on the specified <paramref name="targetObject"/>.
        /// </summary>
        /// <param name="targetObject">Target object to check.</param>
        /// <returns>
        /// <c>true</c> when the <paramref name="targetObject"/> is a <see cref="WaterFlowFMRestartFile"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool CanImportOn(object targetObject) => GetModel(targetObject) != null;

        /// <summary>
        /// Imports the restart file with path <paramref name="path"/>
        /// </summary>
        /// <param name="path"> The path of the restart file. </param>
        /// <param name="target"> The target restart file. </param>
        /// <returns>
        /// The imported <seealso cref="WaterFlowFMRestartFile"/>.
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
        public object ImportItem(string path, object target)
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

            WaterFlowFMRestartModel model = GetModel(target);

            model.RestartInput = new WaterFlowFMRestartFile(path);

            if (model is ITimeDependentModel timeDependentModel)
            {
                timeDependentModel.MarkOutputOutOfSync();
            }

            return model.RestartInput;
        }

        private WaterFlowFMRestartModel GetModel(object obj)
        {
            return getModels().FirstOrDefault(m => m.RestartInput == obj);
        }
    }
}