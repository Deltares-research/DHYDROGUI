using System.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    partial class WaterFlowFMModelTest
    {
        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetMduDirectory_MduFilePathIsNullOrEmpty_ReturnsEmptyString(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetMduDirectory(), Is.Empty);
        }

        [Test]
        [TestCase(@"c:\models\FlowFM\FlowFM.mdu")]
        [TestCase(@"c:\models\MyModel\dir1\dir2\dir3\FlowFM.mdu")]
        public void GetMduDirectory_MduFilePathIsValid_ReturnsMduDirectory(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetMduDirectory(), Is.EqualTo(Path.GetDirectoryName(mduFilePath)));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetModelDirectory_MduFilePathIsNullOrEmpty_ReturnsModelName(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetModelDirectory(), Is.EqualTo(model.Name));
        }

        [Test]
        [TestCase(@"c:\models\FlowFM\FlowFM.mdu")]
        [TestCase(@"c:\models\FlowFM\dir1\dir2\dir3\FlowFM.mdu")]
        public void GetModelDirectory_ModelDirectoryNameMatchesMduFileName_ReturnsModelDirectory(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetModelDirectory(), Is.EqualTo(@"c:\models\FlowFM"));
        }

        [Test]
        [TestCase(@"c:\models\MyModel\input\FlowFM.mdu")]
        [TestCase(@"c:\models\MyModel\input\dir1\dir2\FlowFM.mdu")]
        public void GetModelDirectory_ModelDirectoryHasInputSubDirectory_ReturnsModelDirectory(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetModelDirectory(), Is.EqualTo(@"c:\models\MyModel"));
        }

        [Test]
        public void GetModelDirectory_ModelDirectoryNameDoesNotMatchMduFileNameAndHasNoInputSubdirectory_ReturnsMduFileDirectory()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\MyModel\FlowFM.mdu" };

            Assert.That(model.GetModelDirectory(), Is.EqualTo(@"c:\models\MyModel"));
        }

        [Test]
        [TestCase(@"c:\models\MyModel\dir1\dir2\dir3\FlowFM.mdu")]
        [TestCase(@"c:\models\MyModel\input\dir1\dir2\dir3\FlowFM.mdu")]
        public void GetModelDirectory_FilePropertyHasRelativePathUpwards_ReturnsModelDirectory(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            model.ModelDefinition
                 .GetModelProperty(KnownProperties.BndExtForceFile)
                 .SetValueFromString(@"..\..\..\boundaries\FlowFM_bnd.ext");

            Assert.That(model.GetModelDirectory(), Is.EqualTo(@"c:\models\MyModel"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetModelOutputDirectory_MduFilePathIsNullOrEmpty_ReturnsModelNamePlusOutput(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetModelOutputDirectory(), Is.EqualTo($@"{model.Name}\output"));
        }

        [Test]
        [TestCase(@"c:\models\FlowFM\FlowFM.mdu")]
        [TestCase(@"c:\models\FlowFM\dir1\dir2\dir3\FlowFM.mdu")]
        public void GetModelOutputDirectory_ModelDirectoryNameMatchesMduFileName_ReturnsModelDirectoryPlusOutput(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetModelOutputDirectory(), Is.EqualTo(@"c:\models\FlowFM\output"));
        }

        [Test]
        [TestCase(@"c:\models\MyModel\input\FlowFM.mdu")]
        [TestCase(@"c:\models\MyModel\input\dir1\dir2\FlowFM.mdu")]
        public void GetModelOutputDirectory_ModelDirectoryHasInputSubDirectory_ReturnsModelDirectoryPlusOutput(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetModelOutputDirectory(), Is.EqualTo(@"c:\models\MyModel\output"));
        }

        [Test]
        public void GetModelOutputDirectory_ModelDirectoryNameDoesNotMatchMduFileNameAndHasNoInputSubdirectory_ReturnsMduDirectoryPlusOutput()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\MyModel\FlowFM.mdu" };

            Assert.That(model.GetModelOutputDirectory(), Is.EqualTo(@"c:\models\MyModel\output"));
        }

        [Test]
        public void GetModelOutputDirectory_FilePropertyHasRelativePathUpwards_ReturnsModelDirectoryPlusOutput()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\MyModel\dir\FlowFM.mdu" };

            model.ModelDefinition
                 .GetModelProperty(KnownProperties.BndExtForceFile)
                 .SetValueFromString(@"..\bc\FlowFM_bnd.ext");

            Assert.That(model.GetModelOutputDirectory(), Is.EqualTo(@"c:\models\MyModel\output"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetMduExportPath_BaseDirIsNullOrEmpty_ReturnsMduFileName(string baseDir)
        {
            var model = new WaterFlowFMModel();

            Assert.That(model.GetMduExportPath(baseDir), Is.EqualTo($@"{model.Name}.mdu"));
        }

        [Test]
        public void GetMduExportPath_ModelDirectoryNameMatchesMduFileNameInSubDirectory_ReturnsBaseDirectoryPlusMduSubDirectoryPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\FlowFM\dir1\dir2\dir3\FlowFM.mdu" };

            Assert.That(model.GetMduExportPath(@"c:\export"), Is.EqualTo($@"c:\export\dir1\dir2\dir3\{model.Name}.mdu"));
        }
        
        [Test]
        public void GetMduExportPath_ModelDirectoryHasInputSubDirectory_ReturnsBaseDirectoryPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\MyModel\input\FlowFM.mdu" };

            Assert.That(model.GetMduExportPath(@"c:\export"), Is.EqualTo($@"c:\export\{model.Name}.mdu"));
        }

        [Test]
        public void GetMduExportPath_FilePropertyHasRelativePathUpwards_ReturnsBaseDirectoryPlusMduSubDirectoryPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\FlowFM\dir1\dir2\dir3\FlowFM.mdu" };

            model.ModelDefinition
                 .GetModelProperty(KnownProperties.BndExtForceFile)
                 .SetValueFromString(@"..\..\..\boundaries\FlowFM_bnd.ext");

            Assert.That(model.GetMduExportPath(@"c:\export"), Is.EqualTo($@"c:\export\dir1\dir2\dir3\{model.Name}.mdu"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetMduSavePath_MduFilePathIsNullOrEmpty_ReturnsModelNamePlusInputPlusMduFileName(string mduFilePath)
        {
            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };

            Assert.That(model.GetMduSavePath(), Is.EqualTo($@"{model.Name}\input\{model.Name}.mdu"));
        }

        [Test]
        public void GetMduSavePath_ModelDirectoryNameMatchesMduFileName_ReturnsModelDirectoryPlusInputPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\FlowFM\FlowFM.mdu" };

            Assert.That(model.GetMduSavePath(), Is.EqualTo($@"c:\models\{model.Name}\input\{model.Name}.mdu"));
        }

        [Test]
        public void GetMduSavePath_ModelDirectoryNameMatchesMduFileNameInSubDirectory_ReturnsModelDirectoryPlusInputPlusMduSubDirectoryPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\FlowFM\dir1\dir2\dir3\FlowFM.mdu" };

            Assert.That(model.GetMduSavePath(), Is.EqualTo($@"c:\models\{model.Name}\input\dir1\dir2\dir3\{model.Name}.mdu"));
        }

        [Test]
        public void GetMduSavePath_ModelDirectoryHasInputSubDirectory_ReturnsModelDirectoryPlusInputPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\MyModel\input\FlowFM.mdu" };

            Assert.That(model.GetMduSavePath(), Is.EqualTo($@"c:\models\{model.Name}\input\{model.Name}.mdu"));
        }

        [Test]
        public void GetMduSavePath_FilePropertyHasRelativePathUpwards_ReturnsModelDirectoryPlusInputPlusMduSubDirectoryPlusMduFileName()
        {
            var model = new WaterFlowFMModel { MduFilePath = @"c:\models\FlowFM\dir1\dir2\dir3\FlowFM.mdu" };

            model.ModelDefinition
                 .GetModelProperty(KnownProperties.BndExtForceFile)
                 .SetValueFromString(@"..\..\..\boundaries\FlowFM_bnd.ext");

            Assert.That(model.GetMduSavePath(), Is.EqualTo($@"c:\models\{model.Name}\input\dir1\dir2\dir3\{model.Name}.mdu"));
        }
    }
}