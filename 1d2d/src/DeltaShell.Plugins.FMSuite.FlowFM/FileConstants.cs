namespace DeltaShell.Plugins.FMSuite.FlowFM {
    /// <summary>
    /// Contains known file constants, such as names and extensions
    /// </summary>
    public static class FileConstants
    {
        #region Directory names


        public const string PrefixDelwaqDirectoryName = "DFM_DELWAQ_";
        public const string SnappedFeaturesDirectoryName = "snapped";

        #endregion

        #region Extensions

        public const string PliFileExtension = ".pli";
        public const string PlizFileExtension = ".pliz";
        public const string PolylineFileExtension = ".pol";
        public const string TimFileExtension = ".tim";
        public const string XyzFileExtension = ".xyz";
        public const string XynFileExtension = ".xyn";
        public const string IniFileExtension = ".ini";
        public const string NetCdfFileExtension = ".nc";
        public const string MduFileExtension = ".mdu";
        public const string MorphologyFileExtension = ".mor";
        public const string SedimentFileExtension = ".sed";
        public const string LandBoundaryFileExtension = ".ldb";
        public const string ExternalForcingFileExtension = ".ext";
        public const string GriddedHeatFluxModelFileExtension = ".htc";
        public const string WindFileExtension = ".wnd";
        public const string CachingFileExtension = ".cache";

        public const string NetFileExtension = "_net" + NetCdfFileExtension;
        public const string DiaFileExtension = ".dia";
        public const string MapFileExtension = "_map" + NetCdfFileExtension;
        public const string HisFileExtension = "_his" + NetCdfFileExtension;
        public const string ComFileExtension = "_com" + NetCdfFileExtension;
        public const string ClassMapFileExtension = "_clm" + NetCdfFileExtension;
        public const string RestartFileExtension = "_rst" + NetCdfFileExtension;
        public const string GeomFileExtension = "geom" + NetCdfFileExtension;
        public const string FouFileExtension = "_fou" + NetCdfFileExtension;

        public const string ThinDamPliFileExtension = "_thd" + PliFileExtension;
        public const string ThinDamPlizFileExtension = "_thd" + PlizFileExtension;
        public const string FixedWeirPlizFileExtension = "_fxw" + PlizFileExtension;
        public const string FixedWeirPliFileExtension = "_fxw" + PliFileExtension;
        public const string ObsCrossSectionPliFileExtension = "_crs" + PliFileExtension;
        public const string ObsCrossSectionPlizFileExtension = "_crs" + PlizFileExtension;
        public const string DryAreaFileExtension = "_dry" + PolylineFileExtension;
        public const string DryPointFileExtension = "_dry" + XyzFileExtension;
        public const string StructuresFileExtension = "_structures" + IniFileExtension;
        public const string ObsPointFileExtension = "_obs" + XynFileExtension;
        public const string EnclosureExtension = "_enc" + PolylineFileExtension;
        public const string EmbankmentFileExtension = "_bnk" + PlizFileExtension;
        public const string MeteoFileExtension = "_meteo" + TimFileExtension;
        public const string BoundaryExternalForcingFileExtension = "_bnd" + ExternalForcingFileExtension;
        public const string RoofAreaFileExtension = "_roofs" + PolylineFileExtension;

        public const string MdwFileExtension = ".mdw";
        public const string SpectrumFileExtension = ".sp2";

        #endregion
    }
}