namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    public static class IniFileImporterExporterTypes
    {
        // this class sets the Structure type specific data keywords that are retrieved in the importers / exporters
        // add keywords when need to support them (see D-Flow User Manual p. 350 for reference)
        public static string WeirImportTypeDescription { get; private set; } = "weir";
        public static string GateImportTypeDescription { get; private set; } = "gate";
        public static string GeneralStructureImportTypeDescription { get; private set; } = "generalstructure";
        public static string PumpImportTypeDescription { get; private set; } = "pump";
    }
}