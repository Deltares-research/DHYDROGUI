using System;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// <see cref="CacheFile"/> describes the .cache file associated with a model.
    /// The .cache file is not visualised within the GUI, and as such this class only
    /// provides file io operations.
    /// </summary>
    /// <remarks>
    /// The following invariants have been defined:
    /// * The <see cref="CacheFile"/> always points to the last version of the .cache file
    /// even if this file is incorrect or corrupted.
    /// * The .cache file is not validated, this is the responsibility of the kernel.
    /// * If the option is turned on, the .cache file is never deleted, but might be
    /// overwritten.
    /// * The name of the .cache file is assumed to be always equal to the .mdu file
    /// and will be updated accordingly.
    /// * A model will always have one, and only one <see cref="CacheFile"/>, regardless
    /// of the option setting.
    /// </remarks>
    public class CacheFile
    {
        private readonly ICopyHandler copyHandler;

        /// <summary> The model this CacheFile observes. </summary>
        private readonly WaterFlowFMModel model;

        /// <summary>
        /// Creates a new instance of the <see cref="CacheFile"/> associated with the
        /// provided <paramref name="model"/>.
        /// </summary>
        /// <param name="model">The model with which this CacheFile is associated.</param>
        /// <param name="copyHandler">The copy handler.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="model"/> or <paramref name="copyHandler"/> is <c>null</c>.
        /// </exception>
        public CacheFile(WaterFlowFMModel model, ICopyHandler copyHandler)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.copyHandler = copyHandler ?? throw new ArgumentNullException(nameof(copyHandler));

            UpdatePathToMduLocation(model.MduFilePath);
        }

        /// <summary>
        /// Gets the path of this <see cref="CacheFile"/>.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the .cache file at <see cref="Path"/> of this <see cref="CacheFile"/>  exists.
        /// </summary>
        /// <value>
        /// <c>true</c> if the .cache file exists; otherwise, <c>false</c>.
        /// </value>
        public bool Exists => File.Exists(Path);

        /// <summary>
        /// Gets a value indicating whether use caching has been turned on for this model.
        /// </summary>
        /// <value>
        /// <c>true</c> if caching has been turned on; otherwise, <c>false</c>.
        /// </value>
        public bool UseCaching => (bool) model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value;

        /// <summary>
        /// Exports this <see cref="CacheFile"/> to the specified export directory.
        /// </summary>
        /// <param name="exportMduPath">The path to the mdu file being exported.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="exportMduPath"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// It is assumed that <paramref name="exportMduPath"/> is a valid path, no further
        /// checks are done besides checking for null. If an incorrect path is provided, the
        /// behaviour is undetermined.
        /// </remarks>
        public void Export(string exportMduPath)
        {
            Export(exportMduPath, new LogHandler("CacheFile export:", typeof(CacheFile)));
        }

        /// <summary>
        /// Updates the path to correspond to with the provided <paramref name="mduFilePath"/>.
        /// </summary>
        /// <param name="mduFilePath">The mdu file path.</param>
        /// <remarks>
        /// If <paramref name="mduFilePath"/> is <c>null</c> then (new this).Path will be null.
        /// It is assumed that <paramref name="mduFilePath"/> is a valid path, no further
        /// checks are done. If an incorrect path is provided, the behaviour is undetermined.
        /// </remarks>
        public void UpdatePathToMduLocation(string mduFilePath)
        {
            if (mduFilePath != null)
            {
                Path = GetPathFromMduFilePath(mduFilePath);
            }
            else
            {
                Path = null;
            }
        }

        /// <summary>
        /// Exports this <see cref="CacheFile"/> to the specified export directory.
        /// </summary>
        /// <param name="exportMduPath">The path to the mdu file being exported.</param>
        /// <param name="logHandler">The log handler to handle logs with.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="exportMduPath"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// It is assumed that <paramref name="exportMduPath"/> is a valid path, no further
        /// checks are done besides checking for null. If an incorrect path is provided, the
        /// behaviour is undetermined.
        /// </remarks>
        internal void Export(string exportMduPath, ILogHandler logHandler)
        {
            if (exportMduPath == null)
            {
                throw new ArgumentNullException(nameof(exportMduPath));
            }

            if (!UseCaching || !Exists)
            {
                return;
            }

            string targetCacheFilePath = GetPathFromMduFilePath(exportMduPath);

            string fullSourceCacheFilePath = System.IO.Path.GetFullPath(Path);
            string fullTargetCacheFilePath = System.IO.Path.GetFullPath(targetCacheFilePath);

            if (string.Equals(fullSourceCacheFilePath,
                              fullTargetCacheFilePath,
                              StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            CopyInternally(fullSourceCacheFilePath, fullTargetCacheFilePath, logHandler);
            logHandler?.LogReport();
        }

        private void CopyInternally(string sourceCacheFilePath, string targetCacheFilePath, ILogHandler logHandler)
        {
            try
            {
                copyHandler.Copy(sourceCacheFilePath, targetCacheFilePath);
            }
            catch (FileCopyException e)
            {
                logHandler?.ReportWarningFormat(Resources.CacheFile_CopyInternally_Could_not_copy__0__to__1__due_to___2_,
                                                sourceCacheFilePath,
                                                targetCacheFilePath,
                                                e.Message ?? "An undocumented exception.");
            }
        }

        private static string GetPathFromMduFilePath(string mduFilePath)
        {
            return System.IO.Path.ChangeExtension(mduFilePath, FileConstants.CachingFileExtension);
        }
    }
}