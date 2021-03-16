using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class NetworkPropertiesHelper
    {
        public const string BranchGuiFileName = "branches.gui";
        public const string StorageNodeFileName = "nodeFile.ini";

        public static IList<BranchProperties> ReadPropertiesPerBranchFromFile(string netFilePath)
        {
            var brancheTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, BranchGuiFileName);
            var propertiesPerBranch = File.Exists(brancheTypeFilePath) ? BranchFile.Read(brancheTypeFilePath, netFilePath) : null;
            return propertiesPerBranch;
        }

        public static IList<CompartmentProperties> ReadPropertiesPerNodeFromFile(string netFilePath)
        {
            var nodeTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, StorageNodeFileName);
            var propertiesPerNode = File.Exists(nodeTypeFilePath) ? NodeFile.Read(nodeTypeFilePath) : null;
            return propertiesPerNode;
        }
    }
}
