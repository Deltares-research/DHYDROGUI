using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class NetworkPropertiesHelperTest
    {
        [Test]
        public void ReadPropertiesPerBranchFromFile_BranchesGuiFileDoesNotExist_ReturnsEmptyCollection()
        {
            // Setup
            var pathToDirectoryWithoutBranchesFile = @"C:\nonExistingDirectory\";

            // Call
            IEnumerable<BranchProperties> branchProperties = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(pathToDirectoryWithoutBranchesFile);

            // Assert
            Assert.That(branchProperties, Is.Empty);
        }

        [Test]
        public void ReadPropertiesPerNodeFromFile_NodeIniFileDoesNotExist_ReturnsEmptyCollection()
        {
            // Setup
            var pathToDirectoryWithoutNodeFile = @"C:\nonExistingDirectory\";

            // Call
            IEnumerable<CompartmentProperties> compartmentProperties = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(pathToDirectoryWithoutNodeFile);

            // Assert
            Assert.That(compartmentProperties, Is.Empty);
        }
    }
}