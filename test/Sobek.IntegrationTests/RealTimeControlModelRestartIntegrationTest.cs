using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class RealTimeControlModelRestartIntegrationTest : NHibernateIntegrationTestBase
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Ignore("No such things as RTC outputs with DIMR (so far)")]
        public void OpenProjectAndVerify()
        {
            var projectRepository = factory.CreateNew();
            var legacyPath = TestHelper.GetTestFilePath(@"RtcFlow1DRestart\RtcWithPidFlow1DRestart.dsproj");
            var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectRepository.Open(localLegacyPath);
            var hydroModel = (HydroModel)project.RootFolder.Models.First();
            hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));

            var fullRunStartTime = hydroModel.StartTime;
            var fullRunStopTime = hydroModel.StopTime;
            var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();
            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();

            flowModel.OutputSettings.GetEngineParameter(QuantityType.FiniteGridType,
               ElementSet.FiniteVolumeGridOnGridPoints).AggregationOptions = (int)FiniteVolumeDiscretizationType.None;

            var originalDirectory = Environment.CurrentDirectory;
            
            ActivityRunner.RunActivity(hydroModel);
            Environment.CurrentDirectory = originalDirectory;

            var fullRunRtcCrestLevel = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();

            // run again for first half of time, and write restart
            var timeSpan = fullRunStopTime - fullRunStartTime;
            hydroModel.StopTime = fullRunStartTime.AddHours(timeSpan.TotalHours/2);
            rtcModel.WriteRestart = true;
            flowModel.WriteRestart = true;
            ActivityRunner.RunActivity(hydroModel);
            var halfWayStateRtc = (FileBasedRestartState)rtcModel.GetRestartOutputStates().Last().Clone();
            var halfWayStateFlow = (FileBasedRestartState)flowModel.GetRestartOutputStates().Last().Clone();

            var firstHalfRtcCrestLevel = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();

            // run for second half of time, using restart from previous run
            hydroModel.StartTime = fullRunStartTime.AddHours(timeSpan.TotalHours/2);
            hydroModel.StopTime = fullRunStopTime;
            rtcModel.UseRestart = true;
            flowModel.UseRestart = true;
            rtcModel.RestartInput = halfWayStateRtc;
            flowModel.RestartInput = halfWayStateFlow;
            ActivityRunner.RunActivity(hydroModel);

            var secondHalfRtcCrestLevel = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();

            var crestLevelCombined = firstHalfRtcCrestLevel.Concat(secondHalfRtcCrestLevel).ToArray();

            Assert.IsTrue(fullRunRtcCrestLevel.Length > 0);
            Assert.IsTrue(fullRunRtcCrestLevel.Length == crestLevelCombined.Length); 
            for (var i = 0; i < fullRunRtcCrestLevel.Length; i++)
            {
                Assert.AreEqual(fullRunRtcCrestLevel[i], crestLevelCombined[i], 0.0001);
            }
        }

        private IEnumerable<IFileExporter> GetFactoryFileExportersForDimr()
        {
            return factory.SessionProvider.ConfigurationProvider.Plugins.OfType<ApplicationPlugin>().SelectMany(p => p.GetFileExporters()).Plus(new Iterative1D2DCouplerExporter());
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }
    }
}