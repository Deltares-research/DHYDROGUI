namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files {
    public static class ExtForceFileConstants
    {
        public static readonly string[] UnsupportedQuantityKeys = 
        {
            "WUANTITY",
            "_UANTITY"
        };

        public const string FricTypeKey = "IFRCTYP";
        public const string AreaKey = "AREA";
        public const string AveragingTypeKey = "AVERAGINGTYPE";
        public const string RelSearchCellSizeKey = "RELATIVESEARCHCELLSIZE";
        public const string ExtForcesFileQuantBlockStarter = "QUANTITY=";
        public const string DisabledQuantityKey = "DISABLED_QUANTITY";
        public const string QuantityKey = "QUANTITY";
        public const string FileNameKey = "FILENAME";
        public const string FileTypeKey = "FILETYPE";
        public const string MethodKey = "METHOD";
        public const string OperandKey = "OPERAND";
        public const string ValueKey = "VALUE";
        public const string FactorKey = "FACTOR";
        public const string OffsetKey = "OFFSET";
        public const string SedConcPostfix = "_SedConc";
    }
}