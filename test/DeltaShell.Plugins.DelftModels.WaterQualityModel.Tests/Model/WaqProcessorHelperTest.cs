using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqProcessorHelperTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: filePath")]
        public void ParseHisFileDataThrowsOnNullFilePathParameter()
        {
            var waterQualityModel1D = new WaterQualityModel();
            WaqProcessorHelper.ParseHisFileData(null, waterQualityModel1D.ObservationVariableOutputs,waterQualityModel1D.ModelSettings.MonitoringOutputLevel);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseHisFileDataWithoutSkippingSpecificOutput()
        {
            var mocks = new MockRepository();
            var waterQualityModel1D = CreateWaterQualityModel1DStub(mocks);

            mocks.ReplayAll();

            string historyFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.his");

            WaqProcessorHelper.ParseHisFileData(historyFilePath, waterQualityModel1D.ObservationVariableOutputs, waterQualityModel1D.ModelSettings.MonitoringOutputLevel);

            // Output data should be added to "O2" for all output variables
            AssertObservationVariableOutput(waterQualityModel1D, 0, 865, 865, 865, 865, 865);

            // Output data should be added to "ALL SEGMENTS" for all output variables
            AssertObservationVariableOutput(waterQualityModel1D, 1, 865, 865, 865, 865, 865);

            mocks.VerifyAll();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseHisFileDataWithSkippingSpecificOutput()
        {
            var mocks = new MockRepository();
            var waterQualityModel1D = CreateWaterQualityModel1DStub(mocks);

            mocks.ReplayAll();

            var historyFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.his");

            WaqProcessorHelper.ParseHisFileData(historyFilePath, waterQualityModel1D.ObservationVariableOutputs, waterQualityModel1D.ModelSettings.MonitoringOutputLevel, new List<string> { "ALL SEGMENTS" }, new List<string> { "cTR2", "Continuity" });

            // Output data should be added to "O2" for the output variables "cTR1", "cTR3" and "cTR4"
            AssertObservationVariableOutput(waterQualityModel1D, 0, 865, 865, 865, 865, 865);

            // No output data should be added to "ALL SEGMENTS"
            AssertObservationVariableOutput(waterQualityModel1D, 1, 865, 0, 865, 865, 0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseHisFileDataWithIrrelevantObservationPointOutputConfiguration()
        {
            var mocks = new MockRepository();
            var waterQualityModel1D = CreateWaterQualityModel1DStub(mocks);


            mocks.ReplayAll();

            var historyFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.his");

            // Monitoring output level "None" => no data should be parsed from the his file
            WaqProcessorHelper.ParseHisFileData(historyFilePath, waterQualityModel1D.ObservationVariableOutputs, MonitoringOutputLevel.None);

            // No output data should be added to "O2"
            AssertObservationVariableOutput(waterQualityModel1D, 0, 0, 0, 0, 0, 0);

            // No output data should be added to "ALL SEGMENTS"
            AssertObservationVariableOutput(waterQualityModel1D, 1, 0, 0, 0, 0, 0);

            // Monitoring output level "Points" + no observation points => no data should be parsed from the his file
            WaqProcessorHelper.ParseHisFileData(historyFilePath, waterQualityModel1D.ObservationVariableOutputs.Where(v => v.ObservationVariable != null).ToList(), MonitoringOutputLevel.Points);
            
            // No output data should be added to "O2"
            AssertObservationVariableOutput(waterQualityModel1D, 0, 0, 0, 0, 0, 0);

            // No output data should be added to "ALL SEGMENTS"
            AssertObservationVariableOutput(waterQualityModel1D, 1, 0, 0, 0, 0, 0);
        }

        private static WaterQualityModel CreateWaterQualityModel1DStub(MockRepository mocks)
        {
            var waterQualityModel1D = mocks.Stub<WaterQualityModel>();
            var modelSettings = new WaterQualityModelSettings
                {
                    MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas
                };

            var outputVariableTuples = new List<DelftTools.Utils.Tuple<string, string>>
                                           {
                                               new DelftTools.Utils.Tuple<string, string>("cTR1", ""),
                                               new DelftTools.Utils.Tuple<string, string>("cTR2", ""),
                                               new DelftTools.Utils.Tuple<string, string>("cTR3", ""),
                                               new DelftTools.Utils.Tuple<string, string>("cTR4", ""),
                                               new DelftTools.Utils.Tuple<string, string>("Continuity", "")
                                           };

            var observationVariableOutputs = new List<WaterQualityObservationVariableOutput>
                                                 {
                                                     new WaterQualityObservationVariableOutput(outputVariableTuples) { Name = "O2" },
                                                     new WaterQualityObservationVariableOutput(outputVariableTuples) { Name = "ALL SEGMENTS" }
                                                 };

            waterQualityModel1D.Stub(m => m.ObservationVariableOutputs).Return(observationVariableOutputs);
            waterQualityModel1D.Stub(m => m.ModelSettings).Return(modelSettings);
            
            return waterQualityModel1D;
        }

        private static void AssertObservationVariableOutput(WaterQualityModel waterQualityModel1D, int observationVariableOutputIndex, int cTR1ValueCount, int cTR2ValueCount, int cTR3ValueCount, int cTR4ValueCount, int continuityValueCount)
        {
            Assert.AreEqual(cTR1ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(0).GetValues().Count);
            Assert.AreEqual(cTR2ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(1).GetValues().Count);
            Assert.AreEqual(cTR3ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(2).GetValues().Count);
            Assert.AreEqual(cTR4ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(3).GetValues().Count);
            Assert.AreEqual(continuityValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(4).GetValues().Count);
        }
    }
}
