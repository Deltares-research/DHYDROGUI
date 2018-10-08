using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
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
