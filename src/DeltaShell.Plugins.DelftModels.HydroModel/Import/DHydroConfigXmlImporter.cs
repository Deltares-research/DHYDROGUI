using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Guards;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter : IFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(DHydroConfigXmlImporter));

        /// <summary> Function responsible for reading the model. </summary>
        private readonly Func<string, IList<IDimrModelFileImporter>, HydroModel> readFunc;

        private readonly Func<IList<IDimrModelFileImporter>> getDimrModelFileImporters;

        private readonly Func<string> storeWorkingDirectoryPathFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="DHydroConfigXmlImporter"/> class
        /// with the specified read function and dimrFileImporters.
        /// </summary>
        /// <param name="readFunc">The read function.</param>
        /// <param name="dimrFileImporters">The DIMR file importers.</param>
        /// <param name="getWorkingDirectoryPathFunc">
        /// Func for retrieving working directory
        /// from the framework.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="getWorkingDirectoryPathFunc"/> is null.
        /// </exception>
        public DHydroConfigXmlImporter(Func<string, IList<IDimrModelFileImporter>, HydroModel> readFunc,
                                       Func<IList<IDimrModelFileImporter>> dimrFileImporters, Func<string> getWorkingDirectoryPathFunc)
        {
            Ensure.NotNull(getWorkingDirectoryPathFunc, nameof(getWorkingDirectoryPathFunc));
            storeWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;

            this.readFunc = readFunc;
            getDimrModelFileImporters = dimrFileImporters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHydroConfigXmlImporter"/> class
        /// with the default read function and the specified dimrFileImporters.
        /// </summary>
        /// <param name="dimrFileImporters">The DIMR file importers.</param>
        /// <param name="getWorkingDirectoryPathFunc">
        /// Func for retrieving working directory
        /// from the framework.
        /// </param>
        public DHydroConfigXmlImporter(Func<IList<IDimrModelFileImporter>> dimrFileImporters, Func<string> getWorkingDirectoryPathFunc) : this(
            HydroModelReader.Read,
            dimrFileImporters, getWorkingDirectoryPathFunc) {}

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public string Name => "DIMR Configuration File (*.xml)";

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public string Category => ProductCategories.OneDTwoDModelImportCategory;

        public string Description
        {
            get
            {
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public Bitmap Image { get; }

        /// <inheritdoc/>
        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(ICompositeActivity);
            }
        }

        /// <inheritdoc/>
        public bool CanImportOnRootLevel => GetDimrModelFileImporters.Any(e => e.CanImportOnRootLevel);

        /// <inheritdoc/>
        public string FileFilter => "xml|*.xml";

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public string TargetDataDirectory { get; set; }

        /// <inheritdoc/>
        public bool ShouldCancel { get; set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc/>
        public bool OpenViewAfterImport => true;

        /// <inheritdoc/>
        public bool CanImportOn(object targetObject)
        {
            return GetDimrModelFileImporters.Any(e => e.CanImportOn(targetObject));
        }

        /// <inheritdoc/>
        public object ImportItem(string path, object target = null)
        {
            try
            {
                HydroModel importedModel = Read(path);
                importedModel.WorkingDirectoryPathFunc = storeWorkingDirectoryPathFunc;

                var targetModel = target as HydroModel;
                if (targetModel != null)
                {
                    target = targetModel.Owner();
                }

                if (target is Folder folder)
                {
                    folder.Items.Remove(targetModel);
                    folder.Items.Add(importedModel);
                }

                return ShouldCancel ? null : importedModel;
            }
            catch (Exception e) when (e is ArgumentException ||
                                      e is PathTooLongException ||
                                      e is FormatException ||
                                      e is OutOfMemoryException ||
                                      e is IOException ||
                                      e is InvalidOperationException)
            {
                log.Error(string.Format("An error occurred while trying to import a {0}; ", Name), e);
                return null;
            }
        }

        private IList<IDimrModelFileImporter> GetDimrModelFileImporters =>
            getDimrModelFileImporters?.Invoke() ?? new List<IDimrModelFileImporter>();

        /// <summary>
        /// Reads the model located at the specified path with this Importers
        /// read delegate.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns> The model read from the specified path. </returns>
        private HydroModel Read(string path)
        {
            return readFunc.Invoke(path, GetDimrModelFileImporters);
        }
    }
}