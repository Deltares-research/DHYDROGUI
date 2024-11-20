namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Default property mappings used for cross section related gis importers.
    /// </summary>
    public static class CrossSectionDefaultGisPropertyMappings
    {
        public static PropertyMapping YValues => new PropertyMapping("Y'-values");
        public static PropertyMapping ZValues => new PropertyMapping("Z-values");
        public static PropertyMapping Name => new PropertyMapping("Name", true, true);
        public static PropertyMapping LongName => new PropertyMapping("LongName");
        public static PropertyMapping Description => new PropertyMapping("Description");
        public static PropertyMapping ShiftLevel => new PropertyMapping("ShiftLevel");
        public static PropertyMapping FlowWidth => new PropertyMapping("Flow width" , false, true) {PropertyUnit = "m"};
        public static PropertyMapping StorageWidth => new PropertyMapping("Storage width" , false, true) {PropertyUnit = "m"};
    }
}