using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class Delft3DGridFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Delft3DGridFileImporter));

        public Delft3DGridFileImporter(string category)
        {
            Category = category;
        }

        public string Name
        {
            get { return "Delft3D Grid"; }
        }

        public string Category { get; private set; }

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(CurvilinearGrid); }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "Delft3D Grid (*.grd)|*.grd|All Files (*.*)|*.*"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            CurvilinearGrid grid;
            try
            {
                grid = Delft3DGridFileReader.Read(path);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("An error has occured while importing file {0}: {1}", path, e.Message);
                return null;
            }

            return grid;
        }
    }
}
