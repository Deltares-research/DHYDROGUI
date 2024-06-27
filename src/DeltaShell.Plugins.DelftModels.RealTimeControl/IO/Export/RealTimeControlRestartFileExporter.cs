using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Importer for exporting restart files for D-Flow RealTimeControl models.
    /// </summary>
    /// <seealso cref="IFileExporter"/>
    public class RealTimeControlRestartFileExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlRestartFileExporter));

        public string Name => "Restart File";

        public string Category => "XML";

        public string Description => string.Empty;

        public string FileFilter => "Real Time Control restart files|*.xml";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.rtcmodel;

        public bool Export(object item, string path)
        {
            Ensure.NotNull(item, nameof(item));

            if (string.IsNullOrEmpty(path))
            {
                log.Error("Path cannot be null or empty.");
                return false;
            }

            if (!(item is RealTimeControlRestartFile restartFile))
            {
                log.Error($"Cannot export type {item.GetType().Name}.");
                return false;
            }

            try
            {
                File.WriteAllText(path, restartFile.Content);
            }
            catch (Exception e)
            {
                log.Error("Critical exception: " + e.Message);
                return false;
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(RealTimeControlRestartFile);
        }

        public bool CanExportFor(object item) => item is RealTimeControlRestartFile restartFile && !restartFile.IsEmpty;
    }
}