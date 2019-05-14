using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    public class HydroModelHelperTest
    {
        [Test]
        [TestCase("structure01.levelcenter", "structure01.CrestLevel")]
        [TestCase("structure01.sill_level", "structure01.CrestLevel")]
        [TestCase("structure01.crest_level", "structure01.CrestLevel")]
        [TestCase("structure01.gateheight", "structure01.GateLowerEdgeLevel")]
        [TestCase("structure01.lower_edge_level", "structure01.GateLowerEdgeLevel")]
        [TestCase("structure01.door_opening_width", "structure01.GateOpeningWidth")]
        [TestCase("structure01.opening_width", "structure01.GateOpeningWidth")]
        [TestCase("structure.one.sill_level", "structure.one.CrestLevel")]
        [TestCase("structure01_sill_level.sill_level", "structure01_sill_level.CrestLevel")]
        [TestCase("structure01_sill_level", "structure01_sill_level")]
        [TestCase("structure01.CrestLevel", "structure01.CrestLevel")]
        [TestCase("structure01.GateLowerEdgeLevel", "structure01.GateLowerEdgeLevel")]
        [TestCase("structure01.GateOpeningWidth", "structure01.GateOpeningWidth")]
        public void GivenOldComponentNamesOfStructuresInTheJsonFile_WhenOpeningAnOldProjectWithRtcAndFm_ThenTheOldNamesShouldBeCorrected(string oldTargetName, string expectedCorrectedTargetName)
        {
            string correctedTargetName = HydroModelHelper.UpdateOldNamesOfStructuresComponentsToNewNamesIfNeeded(oldTargetName);
            Assert.AreEqual(expectedCorrectedTargetName, correctedTargetName, "The retrieved corrected target name {0} is not the same as the expected corrected target name {1} for target name {2}", correctedTargetName, expectedCorrectedTargetName, oldTargetName);
        }
    }
}