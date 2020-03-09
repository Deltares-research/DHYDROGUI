using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class DiaFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Ignore("This test failed on the trunk and is fixed in the meantime.It will go green when we merge from trunk.")]
        [Category("ToCheck")]
        public void CollectAllCutOffErrorMessages()
        {
            //arrange
            var diaFileWhereXyzFileNameIsCutOff = TestHelper.GetTestFilePath(@"diaFile\FileWithCutOffMessage.dia");

            File.Exists(diaFileWhereXyzFileNameIsCutOff);
          
            //act
            var result = DiaFileReader.CollectAllErrorMessages(diaFileWhereXyzFileNameIsCutOff);

            //assert
            Assert.NotNull(result);
            Assert.True(result.Contains("** ERROR ERROR: RDMORLYR Error reading samples (not covering full grid) f_IniSedThick.xyz .") );
        }
    }
}
