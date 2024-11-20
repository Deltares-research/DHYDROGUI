namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// File and directory constants used by D-Water Quality
    /// </summary>
    public static class FileConstants
    {
        /// <summary>
        /// The delwaq work file name: deltashell
        /// </summary>
        public const string WorkFilesName = "deltashell";

        /// <summary>
        /// The NetCDF map file name: deltashell_map.nc
        /// </summary>
        public const string NetCdfMapFileName = WorkFilesName + "_map.nc";

        /// <summary>
        /// The NetCDF history file name: deltashell_his.nc
        /// </summary>
        public const string NetCdfHisFileName = WorkFilesName + "_his.nc";

        /// <summary>
        /// The binary map file name: deltashell.map
        /// </summary>
        public const string BinaryMapFileName = WorkFilesName + ".map";

        /// <summary>
        /// The binary history file name: deltashell.his
        /// </summary>
        public const string BinaryHisFileName = WorkFilesName + ".his";

        /// <summary>
        /// The list file name: deltashell.lst
        /// </summary>
        public const string ListFileName = WorkFilesName + ".lst";

        /// <summary>
        /// The process file name: deltashell.lsp
        /// </summary>
        public const string ProcessFileName = WorkFilesName + ".lsp";

        /// <summary>
        /// The input file name: deltashell.inp
        /// </summary>
        public const string InputFileName = WorkFilesName + ".inp";

        /// <summary>
        /// The monitoring file name: deltashell.mon
        /// </summary>
        public const string MonitoringFileName = WorkFilesName + ".mon";

        /// <summary>
        /// The balance output file name: deltashell-bal.prn
        /// </summary>
        public const string BalanceOutputFileName = WorkFilesName + "-bal.prn";

        /// <summary>
        /// The restart file name: deltashell_res.map
        /// </summary>
        public const string RestartFileName = WorkFilesName + "_res.map";

        /// <summary>
        /// The restart in file name: deltashell_res_in.map
        /// </summary>
        public const string RestartInFileName = WorkFilesName + "_res_in.map";

        /// <summary>
        /// The initial conditions file name: deltashell-initials.map
        /// </summary>
        public const string InitialConditionsFileName = WorkFilesName + "-initials.map";

        /// <summary>
        /// The output directory name: output
        /// </summary>
        public const string OutputDirectoryName = "output";

        /// <summary>
        /// The boundary data directory name: boundary_data_tables
        /// </summary>
        public const string BoundaryDataDirectoryName = "boundary_data_tables";

        /// <summary>
        /// The loads data directory name: load_data_tables
        /// </summary>
        public const string LoadsDataDirectoryName = "load_data_tables";

        /// <summary>
        /// The include files directory name: includes_deltashell
        /// </summary>
        public const string IncludesDirectoryName = "includes_" + WorkFilesName;

        /// <summary>
        /// The work directory postfix: _output
        /// </summary>
        public const string WorkDirectoryPostfix = "_output";
    }
}