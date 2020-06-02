using System.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class BndExtForceFile : NGHSFileBase
    {
        private const string areaKey = "area";
        private static readonly ILog log = LogManager.GetLogger(typeof(BndExtForceFile));

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolyLineFiles;
        private readonly IDictionary<IBoundaryCondition, DelftIniCategory> existingBndForceFileItems;

        public BndExtForceFile()
        {
            existingPolyLineFiles = new Dictionary<Feature2D, string>();
            existingBndForceFileItems = new Dictionary<IBoundaryCondition, DelftIniCategory>();
            WriteToDisk = true;
        }
    }
}