using System.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class BndExtForceFile : NGHSFileBase
    {
        private string bndExtFilePath;
        private string bndExtSubFilesReferenceFilePath;
        
        /// <summary>
        /// File version of the file.
        /// </summary>
        public string FileVersion => "2.01";

        /// <summary>
        /// File type of the file.
        /// </summary>
        public string FileType => "extForce";
        
        private const string areaKey = "area";
        private static readonly ILog log = LogManager.GetLogger(typeof(BndExtForceFile));

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolyLineFiles;
        private readonly IDictionary<IBoundaryCondition, BoundaryDTO> existingBoundaryItems;
        private readonly IDictionary<Lateral, LateralDTO> existingLateralItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceFile"/> class.
        /// </summary>
        public BndExtForceFile()
        {
            existingPolyLineFiles = new Dictionary<Feature2D, string>();
            existingBoundaryItems = new Dictionary<IBoundaryCondition, BoundaryDTO>();
            existingLateralItems = new Dictionary<Lateral, LateralDTO>();
            WriteToDisk = true;
        }
    }
}