using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class LateralSourceMappingPageTests
    {
        [Test]
        public void GivenForBoundaryConditions_UpdateAvailableRadioButtons_ShouldTurnRbhAndRbHtInvisible([Values] bool forBoundaryConditions)
        {
            // arrange
            var mappingPage = new LateralSourceMappingPage();

            // act
            mappingPage.ForBoundaryConditions = forBoundaryConditions;
            mappingPage.BatchMode = true; // prevent intervention from BatchMode

            // assert
            Assert.AreEqual(forBoundaryConditions, mappingPage.rbH.Visible);
            Assert.AreEqual(forBoundaryConditions, mappingPage.rbHT.Visible);
        }

        [Test]
        public void GivenBatchModeFalse_UpdateAvailableRadioButtons_ShouldTurnRbHAndRbQInvisible([Values] bool batchMode)
        {
            // arrange
            var mappingPage = new LateralSourceMappingPage();
            
            // act
            mappingPage.BatchMode = batchMode;
            mappingPage.ForBoundaryConditions = true; // prevent intervention from BoundaryConditions
            
            // assert
            Assert.AreEqual(batchMode, mappingPage.rbH.Visible);
            Assert.AreEqual(batchMode, mappingPage.rbQ.Visible);
        }
    }
}