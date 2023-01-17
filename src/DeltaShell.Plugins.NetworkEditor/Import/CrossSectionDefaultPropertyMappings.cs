namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Default property mappings used for cross section related gis importers.
    /// </summary>
    public static class CrossSectionDefaultPropertyMappings
    {
        public static PropertyMapping Name => new PropertyMapping("Name", true, true);
        public static PropertyMapping LongName => new PropertyMapping("LongName", false, false);
        public static PropertyMapping Description => new PropertyMapping("Description", false, false);
        public static PropertyMapping ShiftLevel => new PropertyMapping("ShiftLevel", false, false);
        public static PropertyMapping FlowWidth => new PropertyMapping("Flow width" , false, true) {PropertyUnit = "m"};
        public static PropertyMapping StorageWidth => new PropertyMapping("Storage width" , false, true) {PropertyUnit = "m"};
        
    }
}