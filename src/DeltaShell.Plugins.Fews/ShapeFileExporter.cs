using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.ModelExchange.Queries;
using DelftTools.Shell.Core;
using log4net;

namespace DeltaShell.Plugins.Fews
{
    public class ShapeFileExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ShapeFileExporter));
        private Project project;

        public ShapeFileExporter()
        {
        }

        #region IFileExporter Members

        public string Name
        {
            get { return "FEWS ShapeFile Exporter"; }
        }
        public string Description { get { return Name; } }
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            FilePath = path;

            project = item as Project;
            if (project != null)
            {
                return Execute();
            }

            Log.ErrorFormat("Type {0} is not supported as export item", item.GetType());

            return false;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (Project);
        }

        public string FileFilter
        {
            get { return "Shape Files (*.shp)|*.shp"; }
        }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion

        #region properties/methods based on pseudocode commandline request

        public string FilePath { get; set; }

        internal ExtendedQueryContext ExtendedContext { get; set; }

        public bool Execute()
        {
            if (!Validate()) return false;



            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }



            return true;
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(FilePath) || FilePath.Trim() == string.Empty)
            {
                Log.Error("File path to export has not been set.");
                return false;
            }

            return true;
        }

        #endregion
    }
}