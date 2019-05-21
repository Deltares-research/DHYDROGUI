using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.Fews.Tests;
using NUnit.Framework;

namespace Deltares.IO.FewsPI.Tests
{
    [TestFixture]
    public class RunInfoTest
    {
        private string testFolderName;
        private string fewsTestWorkFolderName;
        private string fewsTestInputFolderName;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            testFolderName = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(FewsAdapterTest).Assembly);
            fewsTestWorkFolderName = Path.Combine(testFolderName, "LWM");
            fewsTestInputFolderName = Path.Combine(fewsTestWorkFolderName, "Input");
            Assert.IsTrue(Directory.Exists(testFolderName));
            Assert.IsTrue(Directory.Exists(fewsTestWorkFolderName));
            Assert.IsTrue(Directory.Exists(fewsTestInputFolderName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void RunInfoConstructor_ValidFilePath_InstanceWithResolvedTempPath()
        {
            const string xmlString =
                @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                @"<Run xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
                @"xmlns=""http://www.wldelft.nl/fews/PI"" xsi:schemaLocation=""http://www.wldelft.nl/fews/PI " +
                @"http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"" version=""1.5"">" +
                @"    <timeZone>0.0</timeZone>" + @"    <startDateTime date=""2012-01-18"" time=""02:00:00""/>" +
                @"    <endDateTime date=""2012-01-30"" time=""02:00:00""/>" +
                @"    <time0 date=""2012-01-18"" time=""02:00:00""/>" + @"    <workDir>%TEST_DIR%\work</workDir>" +
                @"    <inputParameterFile>%TEST_DIR%\Input\params.xml</inputParameterFile>" +
                @"    <inputTimeSeriesFile>%TEST_DIR%\Input\export_pi_1.xml</inputTimeSeriesFile>" +
                @"    <inputTimeSeriesFile>%TEST_DIR%\Input\export_pi_2.xml</inputTimeSeriesFile>" +
                @"    <outputDiagnosticFile>%TEST_DIR%\Output\diagnostics.xml</outputDiagnosticFile>" +
                @"    <outputTimeSeriesFile>%TEST_DIR%\Output\ds_results.xml</outputTimeSeriesFile>" +
                @"    <properties>" +
                @"        <string key=""deltaShellProjectFile"" value=""%TEST_DIR%\dsproj\?????""/>" +
                @"        <string key=""compositeModelID"" value=""????""/>" +
                @"        <string key=""piTimeSeriesAsBin"" value=""false""/>" +
                @"    </properties>" +
                @"</Run>";

            string piRunTestFile = WritePiRunTestFile("piRunTestFile.xml", xmlString);
            var runInfo = new RunInfo(piRunTestFile);
            string outputDiagnosticsFile = runInfo.OutputDiagnosticsFile;
            Assert.IsTrue(outputDiagnosticsFile.StartsWith(fewsTestWorkFolderName), runInfo.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ValidateRunInfoData_ModelNameIsMissing_ErrorIsReturned()
        {
            const string xmlString =
            @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
            @"<Run xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
            @"xmlns=""http://www.wldelft.nl/fews/PI"" xsi:schemaLocation=""http://www.wldelft.nl/fews/PI " +
            @"http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"" version=""1.5"">" +
            @"    <timeZone>1.0</timeZone>" +
            @"    <startDateTime date=""2012-02-25"" time=""11:00:00""/>" +
            @"    <endDateTime date=""2012-03-01"" time=""11:00:00""/>" +
            @"    <time0 date=""2012-02-29"" time=""11:00:00""/>" +
            @"    <workDir>{0}</workDir>" +
            @"    <inputTimeSeriesFile>{0}\Input\rain.xml</inputTimeSeriesFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\evap.xml</inputTimeSeriesFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\wind.xml</inputTimeSeriesFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\boundaries.xml</inputTimeSeriesFile>" +
            @"    <outputDiagnosticFile>{0}\Logs\diagnostics.xml</outputDiagnosticFile>" +
            @"    <outputTimeSeriesFile>{0}\Output\ow.xml</outputTimeSeriesFile>" +
            @"    <outputTimeSeriesFile>{0}\Output\struc.xml</outputTimeSeriesFile>" +
            @"    <properties>" +
            @"        <string key=""piTimeSeriesAsBin"" value=""false""/>" +
            @"        <string key=""model"" value=""""/>" +
            @"        <string key=""deltaShellProjectFile"" value=""{0}\DSModel\lauwersmeer.dsproj""/>" +
            @"    </properties>" +
            @"</Run>";

            string piRunTestFile = WritePiRunTestFile("piRunTestFile.xml", string.Format(xmlString, fewsTestWorkFolderName));
            var runInfo = new RunInfo(piRunTestFile);
            var errors = runInfo.Validate();
            Assert.AreEqual(1, errors.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ValidateRunInfoData_ProjectFileIsMissing_ErrorIsReturned()
        {
            const string xmlString =
            @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
            @"<Run xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
            @"xmlns=""http://www.wldelft.nl/fews/PI"" xsi:schemaLocation=""http://www.wldelft.nl/fews/PI " +
            @"http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"" version=""1.5"">" +
            @"    <timeZone>1.0</timeZone>" +
            @"    <startDateTime date=""2012-02-25"" time=""11:00:00""/>" +
            @"    <endDateTime date=""2012-03-01"" time=""11:00:00""/>" +
            @"    <time0 date=""2012-02-29"" time=""11:00:00""/>" +
            @"    <workDir>{0}</workDir>" +
            @"    " +
            @"    <inputStateDescriptionFile>{0}\States\Inputstates.xml</inputStateDescriptionFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\rain.xml</inputTimeSeriesFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\evap.xml</inputTimeSeriesFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\wind.xml</inputTimeSeriesFile>" +
            @"    <inputTimeSeriesFile>{0}\Input\boundaries.xml</inputTimeSeriesFile>" +
            @"    <outputDiagnosticFile>{0}\Logs\diagnostics.xml</outputDiagnosticFile>" +
            @"    <outputStateDescriptionFile>{0}\States\Outputstates.xml</outputStateDescriptionFile>" +
            @"    <outputTimeSeriesFile>{0}\Output\ow.xml</outputTimeSeriesFile>" +
            @"    <outputTimeSeriesFile>{0}\Output\struc.xml</outputTimeSeriesFile>" +
            @"    <properties>" +
            @"        <string key=""piTimeSeriesAsBin"" value=""false""/>" +
            @"        <string key=""model"" value=""test""/>" +
            @"        <string key=""deltaShellProjectFile"" value=""""/>" +
            @"    </properties>" +
            @"</Run>";

            string piRunTestFile = WritePiRunTestFile("piRunTestFile.xml", string.Format(xmlString, fewsTestWorkFolderName));
            var runInfo = new RunInfo(piRunTestFile);
            var errors = runInfo.Validate();
            Assert.AreEqual(2, errors.Count());
        }

        /// <remarks>
        /// Note that the file used here contains %TEST_DIR% placeholders so that the path
        /// can be complemented with the client side path
        /// </remarks>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ValidateRunInfoData_UsingValidFile_NoErrors()
        {
            string fileName = Path.Combine(fewsTestWorkFolderName, @"Input\pi-run.xml");
            var runInfo = new RunInfo(fileName);
            var errors = runInfo.Validate();
            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WorkingFolder_Test_ShouldNotContainTestDir()
        {
            string fileName = Path.Combine(fewsTestWorkFolderName, @"Input\pi-run.xml");
            var runInfo = new RunInfo(fileName);
            Assert.IsFalse(runInfo.WorkingDirectory.Contains("TEST_DIR"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ProjectFile_UsingFile_ShouldReturnValidFileLocation()
        {
            string fileName = Path.Combine(fewsTestWorkFolderName, @"Input\pi-runSmallWithProfile.xml");
            var runInfo = new RunInfo(fileName);
            var file = runInfo.ProjectFile;
            Assert.IsTrue(file.Contains("lauwersmeer"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void PiTimeSeriesAsBin_XmlValueIsSetToTrue_ShouldReturnTrue()
        {
            const string xmlString =
            @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
            @"<Run xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
            @"xmlns=""http://www.wldelft.nl/fews/PI"" xsi:schemaLocation=""http://www.wldelft.nl/fews/PI " +
            @"http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"" version=""1.5"">" +
            @"    <timeZone>1.0</timeZone>" +
            @"    <startDateTime date=""2012-03-01"" time=""15:00:00""/>" +
            @"    <endDateTime date=""2012-03-06"" time=""15:00:00""/>" +
            @"    <time0 date=""2012-03-05"" time=""15:00:00""/>" +
            @"    <workDir>D:\FEWS_DS\DeltaShell\Modules\DS\LWM</workDir>" +
            @"    <outputDiagnosticFile>D:\FEWS_DS\DeltaShell\Modules\DS\LWM\Logs\diagnostics.xml</outputDiagnosticFile>" +
            @"    <properties>" +
            @"        <string key=""piTimeSeriesAsBin"" value=""true""/>" +
            @"    </properties>" +
            @"</Run>";

            string piRunTestFile = WritePiRunTestFile("piRunTestFile.xml", xmlString);
            var runInfo = new RunInfo(piRunTestFile);
            Assert.IsTrue(runInfo.PiTimeSeriesAsBin);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void PiTimeSeriesAsBin_XmlValueIsSetToFalse_ShouldReturnFalse()
        {
            const string xmlString =
            @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
            @"<Run xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
            @"xmlns=""http://www.wldelft.nl/fews/PI"" xsi:schemaLocation=""http://www.wldelft.nl/fews/PI " +
            @"http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"" version=""1.5"">" +
            @"    <timeZone>1.0</timeZone>" +
            @"    <startDateTime date=""2012-03-01"" time=""15:00:00""/>" +
            @"    <endDateTime date=""2012-03-06"" time=""15:00:00""/>" +
            @"    <time0 date=""2012-03-05"" time=""15:00:00""/>" +
            @"    <workDir>D:\FEWS_DS\DeltaShell\Modules\DS\LWM</workDir>" +
            @"    <outputDiagnosticFile>D:\FEWS_DS\DeltaShell\Modules\DS\LWM\Logs\diagnostics.xml</outputDiagnosticFile>" +
            @"    <properties>" +
            @"        <string key=""piTimeSeriesAsBin"" value=""false""/>" +
            @"    </properties>" +
            @"</Run>";

            string piRunTestFile = WritePiRunTestFile("piRunTestFile.xml", xmlString);
            var runInfo = new RunInfo(piRunTestFile);
            Assert.IsFalse(runInfo.PiTimeSeriesAsBin);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void PiTimeSeriesAsBin_XmlKeyValuePairIsLeftOut_ShouldReturnFalse()
        {
            const string xmlString =
            @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
            @"<Run xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
            @"xmlns=""http://www.wldelft.nl/fews/PI"" xsi:schemaLocation=""http://www.wldelft.nl/fews/PI " +
            @"http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"" version=""1.5"">" +
            @"    <timeZone>1.0</timeZone>" +
            @"    <startDateTime date=""2012-03-01"" time=""15:00:00""/>" +
            @"    <endDateTime date=""2012-03-06"" time=""15:00:00""/>" +
            @"    <time0 date=""2012-03-05"" time=""15:00:00""/>" +
            @"    <workDir>D:\FEWS_DS\DeltaShell\Modules\DS\LWM</workDir>" +
            @"    <outputDiagnosticFile>D:\FEWS_DS\DeltaShell\Modules\DS\LWM\Logs\diagnostics.xml</outputDiagnosticFile>" +
            @"</Run>";

            string piRunTestFile = WritePiRunTestFile("piRunTestFile.xml", xmlString);
            var runInfo = new RunInfo(piRunTestFile);
            Assert.IsFalse(runInfo.PiTimeSeriesAsBin);
        }

        private string WritePiRunTestFile(string piruntestfileXml, string xmlString)
        {
            string piRunTestFile = Path.Combine(fewsTestInputFolderName, piruntestfileXml);
            StreamWriter writer = new StreamWriter(piRunTestFile);
            writer.WriteLine(xmlString);
            writer.Close();
            Assert.IsTrue(File.Exists(piRunTestFile));
            return piRunTestFile;
        }
    }
}