using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMapFileAsRestartFile_WhenExportingModel_RestartStartTimeIsWrittenInMdu()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var fmModel = new WaterFlowFMModel())
            {
                const string restartFilename = "random_map.nc";
                string restartFilepath = Path.Combine(tempDir.Path, restartFilename);
                fmModel.RestartInput = new WaterFlowFMRestartFile(restartFilepath) { StartTime = new DateTime(1990, 07, 18, 12, 34, 56) };

                // Call
                string mduFilepath = Path.Combine(tempDir.Path, "randomName.mdu");
                fmModel.ExportTo(mduFilepath, true, false, false);

                // Assert
                using (var stream = new FileStream(mduFilepath, FileMode.Open))
                {
                    IList<DelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(stream, mduFilepath);
                    Assert.That(categories, Is.Not.Null);

                    DelftIniCategory restartCategory = categories.FirstOrDefault(c => c.Name.EqualsCaseInsensitive("restart"));
                    Assert.That(restartCategory, Is.Not.Null);

                    DelftIniProperty restartFile = restartCategory.Properties.FirstOrDefault(p => p.Name.EqualsCaseInsensitive(KnownProperties.RestartFile));
                    Assert.That(restartFile, Is.Not.Null);
                    Assert.That(restartFile.Value, Is.EqualTo(restartFilename));

                    DelftIniProperty restartStartDate = restartCategory.Properties.FirstOrDefault(p => p.Name.EqualsCaseInsensitive(KnownProperties.RestartDateTime));
                    Assert.That(restartStartDate, Is.Not.Null);
                    const string expectedRestartStartDate = "19900718123456"; // 18-07-1990 12:34:56
                    Assert.That(restartStartDate.Value, Is.EqualTo(expectedRestartStartDate));
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenARstFileAsRestartFile_WhenExportingModel_RestartStartTimeInMduIsEmpty()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var fmModel = new WaterFlowFMModel())
            {
                const string restartFilename = "random_rst.nc";
                string restartFilepath = Path.Combine(tempDir.Path, restartFilename);
                fmModel.RestartInput = new WaterFlowFMRestartFile(restartFilepath) { StartTime = new DateTime(1990, 07, 18, 12, 34, 56) };

                // Call
                string mduFilepath = Path.Combine(tempDir.Path, "randomName.mdu");
                fmModel.ExportTo(mduFilepath, true, false, false);

                // Assert
                using (var stream = new FileStream(mduFilepath, FileMode.Open))
                {
                    IList<DelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(stream, mduFilepath);
                    Assert.That(categories, Is.Not.Null);

                    DelftIniCategory restartCategory = categories.FirstOrDefault(c => c.Name.EqualsCaseInsensitive("restart"));
                    Assert.That(restartCategory, Is.Not.Null);

                    DelftIniProperty restartFile = restartCategory.Properties.FirstOrDefault(p => p.Name.EqualsCaseInsensitive(KnownProperties.RestartFile));
                    Assert.That(restartFile, Is.Not.Null);
                    Assert.That(restartFile.Value, Is.EqualTo(restartFilename));

                    DelftIniProperty restartStartDate = restartCategory.Properties.FirstOrDefault(p => p.Name.EqualsCaseInsensitive(KnownProperties.RestartDateTime));
                    Assert.That(restartStartDate, Is.Not.Null);
                    var expectedRestartStartDate = string.Empty;
                    Assert.That(restartStartDate.Value, Is.EqualTo(expectedRestartStartDate));
                }
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenNoRestartFile_WhenExportingModel_RestartStartTimeInMduIsEmpty()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var fmModel = new WaterFlowFMModel())
            {
                // Call
                string mduFilepath = Path.Combine(tempDir.Path, "randomName.mdu");
                fmModel.ExportTo(mduFilepath, true, false, false);

                // Assert
                using (var stream = new FileStream(mduFilepath, FileMode.Open))
                {
                    IList<DelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(stream, mduFilepath);
                    Assert.That(categories, Is.Not.Null);

                    DelftIniCategory restartCategory = categories.FirstOrDefault(c => c.Name.EqualsCaseInsensitive("restart"));
                    Assert.That(restartCategory, Is.Not.Null);

                    DelftIniProperty restartFile = restartCategory.Properties.FirstOrDefault(p => p.Name.EqualsCaseInsensitive(KnownProperties.RestartFile));
                    Assert.That(restartFile, Is.Not.Null);
                    Assert.That(restartFile.Value, Is.Empty);

                    DelftIniProperty restartStartDate = restartCategory.Properties.FirstOrDefault(p => p.Name.EqualsCaseInsensitive(KnownProperties.RestartDateTime));
                    Assert.That(restartStartDate, Is.Not.Null);
                    var expectedRestartStartDate = string.Empty;
                    Assert.That(restartStartDate.Value, Is.EqualTo(expectedRestartStartDate));
                }
            }
        }
    }
}