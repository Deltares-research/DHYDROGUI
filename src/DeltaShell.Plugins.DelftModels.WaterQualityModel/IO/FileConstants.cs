namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// File and directory constants used by D-Water Quality
    /// </summary>
    public static class FileConstants
    {
        public const string WorkFilesName = "deltashell";
        public const string NetCdfMapFileName = WorkFilesName + "_map.nc";
        public const string NetCdfHisFileName = WorkFilesName + "_his.nc";
        public const string BinaryMapFileName = WorkFilesName + ".map";
        public const string BinaryHisFileName = WorkFilesName + ".his";
        public const string ListFileName = WorkFilesName + ".lst";
        public const string ProcessFileName = WorkFilesName + ".lsp";
        public const string InputFileName = WorkFilesName + ".inp";
        public const string MonitoringFileName = WorkFilesName + ".mon";
        public const string BalanceOutputFileName = WorkFilesName + "-bal.prn";
        public const string RestartFileName = WorkFilesName + "_res.map";
        public const string RestartInFileName = WorkFilesName + "_res_in.map";
        public const string InitialConditionsFileName = WorkFilesName + "-initials.map";

        public const string OutputDirectoryName = "output";
        public const string BoundaryDataDirectoryName = "boundary_data_tables";
        public const string LoadsDataDirectoryName = "load_data_tables";
        public const string IncludesDirectoryName = "includes_" + WorkFilesName;
        public const string WorkDirectoryPostfix = "_output";
    }
}
