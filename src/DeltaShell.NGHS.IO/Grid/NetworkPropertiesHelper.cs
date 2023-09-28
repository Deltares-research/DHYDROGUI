using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class NetworkPropertiesHelper
    {
        public const string BranchGuiFileName = "branches.gui";
        public const string StorageNodeFileName = "nodeFile.ini";

        /// <summary>
        /// Reads the branch properties from the given NetCDF file.
        /// </summary>
        /// <param name="netFilePath">Path to the NetCDF file to read from.</param>
        /// <returns>The read branch properties. Returns an empty collection if the branch file does not exist.</returns>
        public static IEnumerable<BranchProperties> ReadPropertiesPerBranchFromFile(string netFilePath)
        {
            string branchTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, BranchGuiFileName);
            var logHandler = new LogHandler(Resources.Reading_the_branches_gui_file, typeof(BranchFile));
            IEnumerable<BranchProperties> propertiesPerBranch = File.Exists(branchTypeFilePath)
                                                                    ? BranchFile.Read(branchTypeFilePath, netFilePath, new IniReader(), logHandler)
                                                                    : Enumerable.Empty<BranchProperties>();

            logHandler.LogReport();

            return propertiesPerBranch;
        }

        /// <summary>
        /// Reads the compartment properties from the given NetCDF file.
        /// </summary>
        /// <param name="netFilePath">Path to the NetCDF file to read from.</param>
        /// <returns>The read compartment properties. Returns an empty collection if the node file does not exist.</returns>
        public static IEnumerable<CompartmentProperties> ReadPropertiesPerNodeFromFile(string netFilePath)
        {
            string nodeTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, StorageNodeFileName);
            IEnumerable<CompartmentProperties> propertiesPerNode = File.Exists(nodeTypeFilePath)
                                                                       ? NodeFile.Read(nodeTypeFilePath)
                                                                       : Enumerable.Empty<CompartmentProperties>();

            return propertiesPerNode;
        }
    }
}