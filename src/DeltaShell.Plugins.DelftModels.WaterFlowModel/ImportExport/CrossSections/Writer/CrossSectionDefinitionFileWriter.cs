using System.IO;
using DeltaShell.NGHS.IO.FileWriters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer
{
    public static class CrossSectionDefinitionFileWriter
    {
        public static void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            if (File.Exists(targetFile)) File.Delete(targetFile);

            var categories = CrossSectionDefinitionFileConverter.Convert(waterFlowModel1D);
            
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }
    }
}
