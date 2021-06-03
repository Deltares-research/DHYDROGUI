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

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void ImportREModel_NDB_Check_Structure13()
        {
            rtcMmodel = GetRtcModel(@"\ReModels\20110331_NDB.sbk\6\deftop.1");
            //condition0(13:head difference > 0) - rule2212(P_003_0:waterlevel) --|
            //                                                                    |
            //                                                                     --structure13
            //                                                                    |
            //condition1(13:head difference < 0) - rule18044(time trigger     ) --|


            //controlgroup per controlled structure
            var controlGroup = GetControlGroupOfStructure("13");
            Assert.IsNotNull(controlGroup);
            Assert.AreEqual(2, controlGroup.Rules.Count);
            Assert.AreEqual(4, controlGroup.Conditions.Count);

            var rule22128 = controlGroup.Rules.FirstOrDefault(r => r.Name == "CTR_22128");
            var rule18044 = controlGroup.Rules.FirstOrDefault(r => r.Name == "CTR_18044");
            var condition0Time = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_0");
            var condition0Hydraulic = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_0_1");
            var condition1Time = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_1");
            var condition1Hydraulic = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_1_1");

            Assert.IsNotNull(rule22128);
            Assert.IsNotNull(rule18044);
            Assert.IsNotNull(condition0Time);
            Assert.IsNotNull(condition0Hydraulic);
            Assert.IsNotNull(condition1Time);
            Assert.IsNotNull(condition1Hydraulic);

            Assert.IsNotNull(rule22128.Inputs.FirstOrDefault(i => i.Name.StartsWith("P_003_0")));
            Assert.AreEqual(condition0Hydraulic, condition0Time.TrueOutputs.FirstOrDefault());
            Assert.AreEqual(rule22128, condition0Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.IsNotNull(condition0Hydraulic.Input);

            Assert.AreEqual(0,rule18044.Inputs.Count);
            Assert.AreEqual(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
            Assert.AreEqual(rule18044, condition1Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.IsNotNull(condition1Hydraulic.Input);

        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void ImportREModel_NDB_Check_Structure24()
        {
            rtcMmodel = GetRtcModel(@"\ReModels\20110331_NDB.sbk\6\deftop.1");

            //condition4(28_200:water level > 1.8) AND condition20770(24:head difference < 0) - rule68                --|
            //                                                                                                               |
            //                                                                                                                --structure24
            //                                                                                                               |
            //condition5(28_200:water level < 1.8) OR condition20771(24:head difference > 0) - rule69(timerule)        --|

            //controlgroup per controlled structure
            var controlGroup = GetControlGroupOfStructure("24");
            Assert.IsNotNull(controlGroup);
            Assert.AreEqual(2, controlGroup.Rules.Count);
            Assert.AreEqual(8, controlGroup.Conditions.Count);
            Assert.AreEqual(2, controlGroup.Inputs.Count);

            var rule68 = controlGroup.Rules.FirstOrDefault(r => r.Name == "CTR_68");
            var rule69 = controlGroup.Rules.FirstOrDefault(r => r.Name == "CTR_69");

            var condition4Time = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_4");
            var condition4Hydraulic = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_4_1");
            var condition20770Time = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_20770");
            var condition20770Hydraulic = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_20770_1");

            var condition5Time = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_5");
            var condition5Hydraulic = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_5_1");
            var condition20771Time = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_20771");
            var condition20771Hydraulic = controlGroup.Conditions.FirstOrDefault(r => r.Name == "TRG_20771_1");

            Assert.IsNotNull(rule68);
            Assert.IsNotNull(rule69);

            Assert.IsNotNull(condition4Time);
            Assert.IsNotNull(condition20770Time);
            Assert.IsNotNull(condition5Time);
            Assert.IsNotNull(condition20771Time);

            Assert.IsNotNull(condition4Hydraulic);
            Assert.IsNotNull(condition20770Hydraulic);
            Assert.IsNotNull(condition5Hydraulic);
            Assert.IsNotNull(condition20771Hydraulic);

            Assert.AreEqual(condition20770Hydraulic, condition20770Time.TrueOutputs.FirstOrDefault());
            Assert.AreEqual(rule68, condition20770Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.IsNotNull(condition20770Hydraulic.Input); //time of testing -> head difference is not supported -> no name check
            Assert.AreEqual(condition20770Time, condition4Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreEqual(condition20770Hydraulic, condition20770Time.TrueOutputs.FirstOrDefault());
            Assert.IsTrue(condition4Hydraulic.Input.Name.StartsWith("28_200_"));

            Assert.IsNull(rule69.Inputs.FirstOrDefault()); //time rule no input
            Assert.AreEqual(rule69, condition20771Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreEqual(condition20771Hydraulic, condition20771Time.TrueOutputs.FirstOrDefault());
            Assert.IsNotNull(condition20771Hydraulic.Input); //time of testing -> head difference is not supported -> no name check
            Assert.AreEqual(rule69, condition5Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreEqual(condition5Hydraulic, condition5Time.TrueOutputs.FirstOrDefault());
            Assert.IsTrue(condition5Hydraulic.Input.Name.StartsWith("28_200_"));

        }

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
