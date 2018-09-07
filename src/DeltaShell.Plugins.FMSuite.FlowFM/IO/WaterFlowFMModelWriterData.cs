using DeltaShell.Plugins.NetworkEditor;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class WaterFlowFMModelWriterData
    {
        public string ModelName { get; set; }

        public FileNames FilePaths { get; set; }
        public NetworkUGridDataModel NetworkDataModel { get; set; }
        public NetworkDiscretisationUGridDataModel NetworkDiscretisationDataModel { get; set; }

        public class FileNames
        {
            public string NetFilePath { get; set; }
            public string CrossSectionLocationFilePath { get; set; }
        }
    }
}
