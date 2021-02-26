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
            using (var model = new WaterFlowFMModel())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
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
