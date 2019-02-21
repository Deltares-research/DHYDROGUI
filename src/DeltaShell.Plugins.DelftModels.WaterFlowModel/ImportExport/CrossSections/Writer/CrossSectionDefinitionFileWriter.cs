using System.IO;
using DeltaShell.NGHS.IO.FileWriters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer
{
    public class CrossSectionDefinitionFileWriter
    {
        private readonly CrossSectionDefinitionFileConverter converter;
        private readonly IniFileWriter writer;

        public CrossSectionDefinitionFileWriter(CrossSectionDefinitionFileConverter converter, IniFileWriter writer)
        {
            this.converter = converter;
            this.writer = writer;
        }

        public virtual void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
           if (File.Exists(targetFile)) File.Delete(targetFile);

            var categories = converter.Convert(waterFlowModel1D);
            
            writer.WriteIniFile(categories, targetFile);
        }
    }
}
