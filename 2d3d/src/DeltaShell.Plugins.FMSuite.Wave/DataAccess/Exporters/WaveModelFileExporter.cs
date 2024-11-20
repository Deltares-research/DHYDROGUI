using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DeltaShell.Dimr;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Exporters
{
    /// <summary>
    /// <see cref="WaveModelFileExporter"/> implements the <see cref="DelftTools.Shell.Core.IFileExporter"/>
    /// to export <see cref="WaveModel"/> as a .mdw file.
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.IFileExporter" />
    public class WaveModelFileExporter : IDimrModelFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveModelFileExporter));

        public string Name => "Waves model";

        public string Category => "General";

        public string Description => string.Empty;

        public string FileFilter => "Master Definition WAVE File|*.mdw";

        public Bitmap Icon { get; } = null;

        public bool Export(object item, string path)
        {
            var wm = item as WaveModel;
            if (wm == null)
            {
                return false;
            }

            try
            {
                if (Directory.Exists(path))
                {
                    string fullPath = Path.Combine(path, wm.Name + ".mdw");
                    wm.ExportModelInputTo(fullPath);
                }
                else
                {
                    wm.ExportModelInputTo(path);
                }

                return true;
            }
            catch (Exception)
            {
                log.ErrorFormat("Export of Waves model failed to path {0}.", path);
                return false;
            }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaveModel);
        }

        public bool CanExportFor(object item) => true;
    }
}