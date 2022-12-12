namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Default property mappings used for bridge related gis importers.
    /// </summary>
    public static class BridgeDefaultPropertyMappings
    {
        public static PropertyMapping Name => new PropertyMapping("Name", true, true);
        public static PropertyMapping LongName => new PropertyMapping("LongName");
        public static PropertyMapping Description => new PropertyMapping("Description");
        public static PropertyMapping Level => new PropertyMapping("Bed level") { PropertyUnit = "m" };
        public static PropertyMapping Length => new PropertyMapping("Length") { PropertyUnit = "m" };
        public static PropertyMapping FrictionValue => new PropertyMapping("Roughness") { PropertyUnit = "Chezy (C) m^1/2*s^-1" };
        public static PropertyMapping Height => new PropertyMapping("Height") { PropertyUnit = "m" };
        public static PropertyMapping Width => new PropertyMapping("Width") { PropertyUnit = "m" };
        public static PropertyMapping YValues => new PropertyMapping("Y'-values");
        public static PropertyMapping ZValues => new PropertyMapping("Z'-values");
    }
}