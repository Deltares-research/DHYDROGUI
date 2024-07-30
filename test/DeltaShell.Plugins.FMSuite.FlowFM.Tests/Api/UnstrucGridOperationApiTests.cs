using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    public class UnstrucGridOperationApiTests
    {
        [Test]
        public void GivenModelWithTrachytopes_WhenGridSnappingIsCalled_ThenTrachytopesShouldBeRemovedFromSmallExport()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false, false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueFromString(Path.GetFileName(model.NetFilePath));

                model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).SetValueFromString("Y");

                var api = new UnstrucGridOperationApi(model, false);
                var tempMduPath = (string)TypeUtils.GetField<UnstrucGridOperationApi, String>(api, "mduFilePath");

                var mduFileDir = Path.GetDirectoryName(tempMduPath);
                var name = Path.GetFileNameWithoutExtension(tempMduPath);
                var fmModelUsedByApi = new WaterFlowFMModel(Path.Combine(mduFileDir,tempMduPath));
                var trtRouUsedInOriginalFMModel = model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInOriginalFMModel, Is.EqualTo("Y"));
                var trtRouUsedInFMModelByApi = fmModelUsedByApi.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInFMModelByApi, Is.EqualTo("N"));
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [TestCase("bla_bnd.ext" , KnownProperties.ExtForceFile, TestName = "ExtForceFile")]
        [TestCase("bla_thd.pli", KnownProperties.ThinDamFile, TestName = "ThinDamFile")]
        public void GivenModelsWithPropertiesToClear_WhenGridSnappingIsCalled_ThenThesePropertiesShouldBeEmpty(string file, string knownProperties)
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false, false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueFromString(Path.GetFileName(model.NetFilePath));

                model.ModelDefinition.GetModelProperty(knownProperties).SetValueFromString(file);
               

                var api = new UnstrucGridOperationApi(model, false);
                var tempMduPath = (string)TypeUtils.GetField<UnstrucGridOperationApi, String>(api, "mduFilePath");

                var mduFileDir = Path.GetDirectoryName(tempMduPath);
                var name = Path.GetFileNameWithoutExtension(tempMduPath);
                var fmModelUsedByApi = new WaterFlowFMModel(Path.Combine(mduFileDir, tempMduPath));
                var FileUsedInOriginalFMModel = model.ModelDefinition.GetModelProperty(knownProperties).GetValueAsString();
                Assert.That(FileUsedInOriginalFMModel, Is.EqualTo(file));
                var FileUsedInFMModelByApi = fmModelUsedByApi.ModelDefinition.GetModelProperty(knownProperties).GetValueAsString();
                Assert.IsEmpty(FileUsedInFMModelByApi);
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }
    }
}


