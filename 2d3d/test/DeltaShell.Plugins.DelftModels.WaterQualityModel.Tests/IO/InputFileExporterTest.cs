using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class InputFileExporterTest
    {
        [Test]
        public void ImporterNeedsValidWaqModel()
        {
            var mocks = new MockRepository();
            var hydroData = mocks.Stub<IHydroData>();

            mocks.ReplayAll();

            var waqModel = new WaterQualityModel();
            var inputFileExporter = new InputFileExporter();

            TypeUtils.SetPrivatePropertyValue(waqModel, nameof(waqModel.HydroData), hydroData);

            TestHelper.AssertLogMessageIsGenerated(() => inputFileExporter.Export(null, ""), "Can't export model ''. It is not a valid water quality model.");
            TestHelper.AssertLogMessageIsGenerated(() => inputFileExporter.Export(waqModel, ""), "Water quality model is not valid. Please check the validation report.");

            // Make model valid by adding a substance to the SubstanceProcessLibrary (at least one needs to be present)
            waqModel.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance {Name = "Test substance"});

            using (var tempDirectory = new TemporaryDirectory())
            {
                string hydFilePath = Path.Combine(tempDirectory.Path, "file.hyd");
                File.WriteAllText(hydFilePath, "");
                hydroData.Stub(d => d.FilePath).Return(hydFilePath);

                TestHelper.AssertLogMessageIsGenerated(() => inputFileExporter.Export(waqModel, "aaa\\test.inc"), "Could not find directory 'aaa'");
            }
        }
    }
}