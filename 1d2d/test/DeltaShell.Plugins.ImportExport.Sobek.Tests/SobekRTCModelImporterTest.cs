using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRTCModelImporterTest
    {
        private RealTimeControlModel rtcMmodel;

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportPidControllerFrom212()
        {
            rtcMmodel = GetRtcModel(@"\171_001.lit\2\network.tp");

            var controlGroup = GetControlGroupOfStructure("5");
            Assert.IsNotNull(controlGroup);
            var pidRule = controlGroup.Rules.OfType<PIDRule>().FirstOrDefault(r => r.Name == "CTR_##2");
            Assert.IsNotNull(pidRule);

            var ki = -5.0;
            var kp = -2.5;
            var kd = 0.0;

            Assert.AreEqual(ki,pidRule.Ki);
            Assert.AreEqual(kp,pidRule.Kp);
            Assert.AreEqual(kd,pidRule.Kd);

            Assert.AreEqual(ExtrapolationType.Periodic,pidRule.TimeSeries.Time.ExtrapolationType);
        }


        private ControlGroup GetControlGroupOfStructure(string structureName)
        {
            return rtcMmodel.ControlGroups.FirstOrDefault(c => c.Outputs.First().Name.StartsWith(structureName + "_")); 
        }

        private RealTimeControlModel GetRtcModel(string path)
        {
            var pathToSobekNetwork =TestHelper.GetTestDataDirectory() + path;
            var importer = new SobekHydroModelImporter(false);
            var model = ((ICompositeActivity)importer.ImportItem(pathToSobekNetwork)).Activities.OfType<RealTimeControlModel>().First();
            return model;
        }
    }
}
