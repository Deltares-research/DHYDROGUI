using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public static class CrossSectionRegion
    {
        public static class CrossSectionDefinitionType
        {
            public const string Yz = "yz";
            public const string Xyz = "xyz";
            public const string Zw = "tabulated";
            public const string Standard = "standard";
            public const string Elliptical = "ellipse";
            public const string Circle = "circle";
            public const string Rectangle = "rectangle";
            public const string Egg = "egg";
            public const string Arch = "arch";
            public const string Cunette = "cunette";
            public const string SteelCunette = "steelcunette";
            public const string Trapezium = "trapezium";
        }

        public static readonly ConfigurationSetting Shift = new ConfigurationSetting(key: "shift", description: "Level shift of the cross section definition(m)");
        public static readonly ConfigurationSetting Definition = new ConfigurationSetting(key: "definition", description: "Id of cross section definition");
    }
}
