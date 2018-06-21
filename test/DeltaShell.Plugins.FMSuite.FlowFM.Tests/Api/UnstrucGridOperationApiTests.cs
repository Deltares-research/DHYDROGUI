using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
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
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(model.NetFilePath));

                model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).SetValueAsString("Y");

                var api = new UnstrucGridOperationApi(model, false);
                var tempMduPath = (string)TypeUtils.GetField<UnstrucGridOperationApi, String>(api, "mduFilePath");

                var mduFileDir = Path.GetDirectoryName(tempMduPath);
                var name = Path.GetFileNameWithoutExtension(tempMduPath);
                var fmModelDefinitionUsedByApi = new WaterFlowFMModelDefinition(mduFileDir, name);
                var trtRouUsedInOriginalFMModel = model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInOriginalFMModel, Is.EqualTo("Y"));
                var trtRouUsedInFMModelByApi = fmModelDefinitionUsedByApi.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInFMModelByApi, Is.EqualTo("N"));
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
            
            
        }
    }
}
