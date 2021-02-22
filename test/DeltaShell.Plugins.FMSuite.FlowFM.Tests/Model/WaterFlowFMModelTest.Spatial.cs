using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        public void SettingADifferentGrid_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);

                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);

                // Act
                model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }
    }
}
