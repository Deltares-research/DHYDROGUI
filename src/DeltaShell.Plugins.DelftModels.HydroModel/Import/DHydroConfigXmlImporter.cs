using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter : IFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(DHydroConfigXmlImporter));

        private readonly IFileImportService fileImportService;
        private readonly IHydroModelReader hydroModelReader;
        private readonly Func<string> workingDirectoryPathFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="DHydroConfigXmlImporter"/> class
        /// with the specified read function and dimrFileImporters.
        /// </summary>
        /// <param name="fileImportService">Provides the DIMR file importers.</param>
        /// <param name="hydroModelReader">Reader for hydro models.</param>
        /// <param name="workingDirectoryPathFunc">
        /// Func for retrieving working directory
        /// from the framework.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fileImportService"/> or <paramref name="workingDirectoryPathFunc"/> is <c>null</c>.
        /// </exception>
        public DHydroConfigXmlImporter(IFileImportService fileImportService, IHydroModelReader hydroModelReader, Func<string> workingDirectoryPathFunc)
        {
            Ensure.NotNull(fileImportService, nameof(fileImportService));
            Ensure.NotNull(hydroModelReader, nameof(hydroModelReader));
            Ensure.NotNull(workingDirectoryPathFunc, nameof(workingDirectoryPathFunc));
            
            this.fileImportService = fileImportService;
            this.hydroModelReader = hydroModelReader;
            this.workingDirectoryPathFunc = workingDirectoryPathFunc;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public string Name => "DIMR Configuration File (*.xml)";

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public string Category => "Integrated Model";

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
        public bool CanImportOnRootLevel => fileImportService.FileImporters.Any(e => e != this && e.CanImportOnRootLevel);

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
            return fileImportService.FileImporters.Any(e => e != this && e.CanImportOn(targetObject));
        }

        /// <inheritdoc/>
        public object ImportItem(string path, object target = null)
        {
            try
            {
                HydroModel importedModel = Read(path);
                importedModel.WorkingDirectoryPathFunc = workingDirectoryPathFunc;

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
                log.Error(string.Format(Resources.DHydroConfigXmlImporter_ImportItem_An_error_occurred_while_trying_to_import_a__0__, Name), e);
                return null;
            }
        }

        private HydroModel Read(string path)
        {
            return hydroModelReader.Read(path);
        }
    }
}