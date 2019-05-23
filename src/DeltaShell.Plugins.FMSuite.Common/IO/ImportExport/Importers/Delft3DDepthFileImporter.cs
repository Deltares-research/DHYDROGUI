using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Common.IO.ImportExport.Importers
{
    public class Delft3DDepthFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Delft3DDepthFileImporter));

        public Delft3DDepthFileImporter(string category)
        {
            Category = category;
        }

        public string Name => "Delft3D Depth File";

        public string Category { get; private set; }
        public string Description => string.Empty;

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(CurvilinearCoverage);
            }
        }

        public bool OpenViewAfterImport => false;

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Delft3D Depth Files (*.dep)|*.dep|All files (*.*)|*.*";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var targetBathy = target as CurvilinearCoverage;

            if (targetBathy == null)
            {
                throw new NotSupportedException("Cannot import depth file (.dep) without a non-null target bed level");
            }

            List<double> values = Delft3DDepthFileReader.Read(path, targetBathy.Size1, targetBathy.Size2).ToList();

            if (values.Count != targetBathy.Size1 * targetBathy.Size2)
            {
                Log.ErrorFormat(
                    "Failed to import bed level from depth file; data in file does not match the target grid: {0}x{1}",
                    targetBathy.Size1, targetBathy.Size2);
                return null;
            }

            targetBathy.SetValues(values);
            return target;
        }
    }
}