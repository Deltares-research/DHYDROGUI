using System.IO;
using DeltaShell.NGHS.IO.FileWriters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer
{
    public sealed class CrossSectionDefinitionFileWriter
    {
        private readonly ICrossSectionDefinitionFileConverter converter;
        private readonly IniFileWriter writer;

        public CrossSectionDefinitionFileWriter(ICrossSectionDefinitionFileConverter converter, IniFileWriter writer)
        {
            this.converter = converter;
            this.writer = writer;
        }

        /// <summary>
        /// Writes a cross section definition .ini format file at target location.
        /// </summary>
        /// <param name="targetFile">Specified file path.</param>
        /// <param name="writer">Writes a cross section definition .ini format file at the specified file path.</param>
        public void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
           if (File.Exists(targetFile)) File.Delete(targetFile);

            var categories = converter.Convert(waterFlowModel1D);
            
            writer.WriteIniFile(categories, targetFile);
        }
    }
}
