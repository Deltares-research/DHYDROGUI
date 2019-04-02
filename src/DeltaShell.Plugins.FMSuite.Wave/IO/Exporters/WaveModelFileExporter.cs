using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Exporters
{
    public class WaveModelFileExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveModelFileExporter));

        public string Name
        {
            get { return "Waves model"; }
        }

        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var wm = item as WaveModel;
            if (wm == null) return false;

            try
            {
                if (Directory.Exists(path))
                {
                    var fullPath = Path.Combine(path, wm.Name + ".mdw");
                    wm.ModelSaveTo(fullPath, false);
                }
                else
                {
                    wm.ModelSaveTo(path, false);                    
                }
                return true;
            }
            catch (Exception)
            {
                Log.ErrorFormat("Export of Waves model failed to path {0}.", path);
                return false;
            }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaveModel);
        }

        public string FileFilter
        {
            get { return "Master Definition WAVE File|*.mdw"; }
        }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}