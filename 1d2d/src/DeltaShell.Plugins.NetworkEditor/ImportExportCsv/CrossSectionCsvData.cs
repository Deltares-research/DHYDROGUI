using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    /// <summary>
    /// Cross section data container base class.
    /// </summary>
    public class CrossSectionCsvData
    {
        public string Name { get; set; }
        public string LongName { get; set; }
        public CrossSectionType CrossSectionType { get; set; }
        public string Branch { get; set; } //null if corrupted
        public double Chainage { get; set; } //NaN if corrupted
    }
}