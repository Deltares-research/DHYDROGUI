using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class NWRWCatchmentFrom3BImporter : IFileImporter
    {
        public string Name
        {
            get { return "NWRW 3B importer"; }
        }

        public string Category
        {
            get { return "NWRW"; }
        }

        public string Description
        {
            get { return Name; }
        }

        public Bitmap Image { get; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(RainfallRunoffModel); }
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "3B file|*.3B"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport
        {
            get { return false; }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            var model = target as RainfallRunoffModel;

            if (model == null || !File.Exists(path))
            {
                // throw new Exception()
                return null;
            }

            List<NwrwData> data = new NwrwFileReader().ReadNwrwFile(path).ToList();

            if (data.Count == 0)
            {
                // throw new Exception()
                return null;
            }

            model.ModelData.AddRange(data);
            return null;
        }
    }
}