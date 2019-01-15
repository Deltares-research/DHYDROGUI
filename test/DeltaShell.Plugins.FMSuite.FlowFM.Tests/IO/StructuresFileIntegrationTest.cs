using NUnit.Framework;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructuresFileIntegrationTest
    {
        /// <summary>
        /// GIVEN a structures file
        ///   AND a structure containing empty width fields
        /// WHEN this structure is written
        ///  AND this structure is read
        /// THEN the obtained structure is equal to the original structure
        ///  AND no exceptions are thrown
        /// </summary>
        [Test]
        public void GivenAStructuresFileAndAStructureContainingEmptyWidthFields_WhenThisStructureIsWrittenAndThisStructureIsRead_ThenTheObtainedStructureIsEqualToTheOriginalStructureAndNoExceptionsAreThrown()
        {
        }
    }
}
