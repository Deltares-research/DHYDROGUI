using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class NetworkCoverageFromGisImporter : IFileImporter
    {
        public PointValuePairsFromGisImporter PointValuePairsFromGisImporter { get; set; }

        public string Name
        {
            get { return "Network Data (GIS)"; }
        }
        public string Description { get { return Name; } }
        public string Category { get; private set; }

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(INetworkCoverage); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }
        public bool OpenViewAfterImport { get { return false; } }
        public string FileFilter { get; private set; }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        
        public object ImportItem(string path, object target)
        {
            var networkCoverage = target as INetworkCoverage;
            if (networkCoverage == null) return null;

            PointValuePairsFromGisImporter.ImportItem(path, target);
            
            var pointValuePairs = PointValuePairsFromGisImporter.PointValuePairs;
            NetworkCoverageHelper.SnapToCoverage(pointValuePairs, networkCoverage, PointValuePairsFromGisImporter.SnappingPrecision);

            return networkCoverage;
        }
    }
}
