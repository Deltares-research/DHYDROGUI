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
            public string CrossSectionDefinitionFilePath { get; set; }
            public string NodeFilePath { get; set; }
            public string StructuresFilePath { get; set; }
        }
    }
}
