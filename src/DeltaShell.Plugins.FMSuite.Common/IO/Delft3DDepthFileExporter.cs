using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class Delft3DDepthFileExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Delft3DDepthFileExporter));

        public string Name
        {
            get { return "Delft3D Depth File"; }
        }
        public string Description { get { return Name; } }
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var bathy = item as CurvilinearCoverage;
            if (bathy == null) return false;

            try
            {
                Delft3DDepthFileWriter.Write(bathy.GetValues<double>().ToArray(), bathy.Size1, bathy.Size2, path);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Writing depth file failed: {0}", e.Message);
                return false;
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (CurvilinearCoverage);
        }

        public string FileFilter
        {
            get { return "Delft3D Depth File (*.dep)|*.dep"; }
        }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}
