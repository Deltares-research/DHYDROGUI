using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    /// <summary>
    /// Tests for parsing data found in Sobek initial.dat files. 
    /// </summary>
    [TestFixture]
    public class InitalFlowConditionsReaderTest
    {

        [Test]
        public void IdentificationInfo()
        {
            const string initialConditionsText =
                @"FLIN id 'R_RND_45' nm '(null)' ci 'R_RND_45' q_ lq 0 1.5 9.9999e+009 ty 1 lv ll 0 -0.3 9.9999e+009  flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.AreEqual("R_RND_45", flowCondition.ID);
            Assert.AreEqual("(null)", flowCondition.Name);
            Assert.AreEqual("R_RND_45", flowCondition.BranchID);
        }

        [Test]
        public void ConstantDischarge()
        {
            const string initialConditionsText =
                @"FLIN id 'R_RND_45' nm '(null)' ci 'R_RND_45' q_ lq 0 1.5 9.9999e+009  flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsFalse(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(1.5, flowCondition.Discharge.Constant, 1.0e-6);
        }

        [Test]
        public void TableDischarge()
        {
            string initialConditionsText =
                @"FLIN id 'R_RND_45' nm '(null)' ci 'R_RND_45' q_ lq 2 9.9999e+009 9.9999e+009 'Initial Discharge on Branch <MAMO001> with length: 1405.0' " +
                Environment.NewLine +
                @"PDIN 0 0 '' pdin CLTT 'Location' 'Q' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 3008 <" + Environment.NewLine +
                @"1405 3008 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsFalse(flowCondition.IsLevelBoundary);
            Assert.IsFalse(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(2, flowCondition.Discharge.Data.Rows.Count);
            Assert.AreEqual(0, (double)flowCondition.Discharge.Data.Rows[0][0], 1.0e-6);
            Assert.AreEqual(3008, (double)flowCondition.Discharge.Data.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1405, (double)flowCondition.Discharge.Data.Rows[1][0], 1.0e-6);
            Assert.AreEqual(3008, (double)flowCondition.Discharge.Data.Rows[1][1], 1.0e-6);
        }

        [Test]
        public void ConstantLevel()
        {
            string initialConditionsText =
                @"FLIN id 'R_RND_45' nm '(null)' ci 'R_RND_45' ty 1 lv ll 0 -0.3 9.9999e+009  flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsFalse(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Level.IsConstant);
            Assert.AreEqual(-0.3, flowCondition.Level.Constant, 1.0e-6);

            initialConditionsText = 
                @"FLIN id 'R_RND_45' nm '(null)' ci 'R_RND_45' ty 1 lv ll 0  -0.3 9.9999e+009 flin";
            initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            flowCondition = initialConditions.Last();
            Assert.IsFalse(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Level.IsConstant);
            Assert.AreEqual(-0.3, flowCondition.Level.Constant, 1.0e-6);

            initialConditionsText =
                @"FLIN nm 'initial' ss 0 id '689' ci '689' lc 9.9999e+009 q_ lq 0  0 9.9999e+009 ty 1 lv ll 0  -0.4 9.9999e+009 flin";
            initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            flowCondition = initialConditions.Last();
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Level.IsConstant);
            Assert.AreEqual(-0.4, flowCondition.Level.Constant, 1.0e-6);

            initialConditionsText =
                @"FLIN nm 'initial' ss 0 id '688' ci '688' lc 9.9999e+009 q_ lq 0 0 9.9999e+009 ty 1 lv ll 0 -0.4 9.9999e+009 flin";
            initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            flowCondition = initialConditions.Last();
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Level.IsConstant);
            Assert.AreEqual(-0.4, flowCondition.Level.Constant, 1.0e-6);
        }

        [Test]
        public void TableLevel()
        {
            string initialConditionsText =
            @"FLIN id 'R_NDB_0' nm '(null)' ci 'R_NDB_0' " +
            @"ty 1 lv ll 2 9.9999e+009 9.9999e+009 'Initial WaterLevel on Branch <MAMO001> with length: 1405.0' PDIN 0 0 '' pdin CLTT 'Location' " +
            @"'WaterLevel' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
            @"0 0.19 <" + Environment.NewLine +
            @"1405 0.19 <" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @" flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsFalse(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsFalse(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(2, flowCondition.Level.Data.Rows.Count);
            Assert.AreEqual(0, (double)flowCondition.Level.Data.Rows[0][0], 1.0e-6);
            Assert.AreEqual(0.19, (double)flowCondition.Level.Data.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1405, (double)flowCondition.Level.Data.Rows[1][0], 1.0e-6);
            Assert.AreEqual(0.19, (double)flowCondition.Level.Data.Rows[1][1], 1.0e-6);
        }

        /// <summary>
        /// Test with lv ll 1 table as written by current Sobek version
        /// TOOLS-1612
        /// </summary>
        [Test]
        public void TableLevel213()
        {
            string source = @"FLIN id '2' nm 'initial' ci '2' ty 1 lv ll 1 PDIN 0 0 '' pdin" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 3 <" + Environment.NewLine +
                            @"500 2.5 <" + Environment.NewLine +
                            @"1200 2 <" + Environment.NewLine +
                            @"1600 3 <" + Environment.NewLine +
                            @"2100 3.7 <" + Environment.NewLine +
                            @"2800 4 <" + Environment.NewLine +
                            @"3150 4.1 <" + Environment.NewLine +
                                                        @"tble flin";
            //@"tble q_ lq 0 -13 flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(source);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsFalse(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsFalse(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(7, flowCondition.Level.Data.Rows.Count);
        }

        /// <summary>
        /// record found in vsa.lit\3 
        /// constants are given as 
        /// q lq 0 0 opposed to q_ lq 0 0 0 -> q is just invalid and will be skipped
        /// lv ll 0 2 opposed to lv ll 0 2 0 -> will be supported
        /// </summary>
        [Test]
        public void SimpleRecord()
        {
            const string source = @"FLIN id 'reach_1160110' nm 'initial' ci 'reach_1160110' q lq 0 0 ty 0 lv ll 0 2 flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(source);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsFalse(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Level.IsConstant);
            Assert.AreEqual(FlowInitialCondition.FlowConditionType.WaterDepth, flowCondition.WaterLevelType);
            Assert.AreEqual(2.0, flowCondition.Level.Constant, 1.0e-6);
        }

        [Test]
        public void ConstantDischargeConstantLevel()
        {
            const string initialConditionsText =
                @"FLIN id 'R_RND_45' nm '(null)' ci 'R_RND_45' q_ lq 0 1.5 9.9999e+009 ty 1 lv ll 0 -0.3 9.9999e+009  flin";
            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsTrue(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(1.5, flowCondition.Discharge.Constant, 1.0e-6);

            Assert.AreEqual(FlowInitialCondition.FlowConditionType.WaterLevel, flowCondition.WaterLevelType);
            Assert.IsTrue(flowCondition.Level.IsConstant);
            Assert.AreEqual(-0.3, flowCondition.Level.Constant, 1.0e-6);
        }

        [Test]
        public void TableDischargeTableLevel()
        {
            string initialConditionsText =
                @"FLIN id 'R_NDB_0' nm '(null)' ci 'R_NDB_0' q_ lq 2 9.9999e+009 9.9999e+009 'Initial Discharge on Branch <MAMO001> with length: 1405.0' " + Environment.NewLine +
                @"PDIN 0 0 '' pdin CLTT 'Location' 'Q' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 3008 <" + Environment.NewLine +
                @"1405 3008 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"ty 1 lv ll 2 9.9999e+009 9.9999e+009 'Initial WaterLevel on Branch <MAMO001> with length: 1405.0' PDIN 0 0 '' pdin CLTT 'Location' 'WaterLevel' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 0.19 <" + Environment.NewLine +
                @"1405 0.19 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" flin";
            //var initialConditions = new InitalFlowConditionsReader().ParseInitialConditions(initialConditionsText);

            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(1, initialConditions.Count());
            var flowCondition = initialConditions.Last();
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsTrue(flowCondition.IsLevelBoundary);

            Assert.IsFalse(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(2, flowCondition.Discharge.Data.Rows.Count);
            Assert.AreEqual(0, (double)flowCondition.Discharge.Data.Rows[0][0], 1.0e-6);
            Assert.AreEqual(3008, (double)flowCondition.Discharge.Data.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1405, (double)flowCondition.Discharge.Data.Rows[1][0], 1.0e-6);
            Assert.AreEqual(3008, (double)flowCondition.Discharge.Data.Rows[1][1], 1.0e-6);

            Assert.IsFalse(flowCondition.Level.IsConstant);
            Assert.AreEqual(2, flowCondition.Level.Data.Rows.Count);
            Assert.AreEqual(0, (double)flowCondition.Level.Data.Rows[0][0], 1.0e-6);
            Assert.AreEqual(0.19, (double)flowCondition.Level.Data.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1405, (double)flowCondition.Level.Data.Rows[1][0], 1.0e-6);
            Assert.AreEqual(0.19, (double)flowCondition.Level.Data.Rows[1][1], 1.0e-6);
        }

        [Test]
        public void InterpolationInitialCondition()
        {
            string initialConditionsText =
                @"FLIN id 'R_NDB_0' nm '(null)' ci 'R_NDB_0' q_ lq 2 9.9999e+009 9.9999e+009 'Initial Discharge on Branch <MAMO001> with length: 1405.0' " + Environment.NewLine +
                @"PDIN 0 0 '' pdin CLTT 'Location' 'Q' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 3008 <" + Environment.NewLine +
                @"1405 3008 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"ty 1 lv ll 2 9.9999e+009 9.9999e+009 'Initial WaterLevel on Branch <MAMO001> with length: 1405.0' PDIN 1 0 '' pdin CLTT 'Location' 'WaterLevel' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 0.19 <" + Environment.NewLine +
                @"1405 0.19 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" flin" + Environment.NewLine +
                @"FLIN id 'R_NDB_1' nm '(null)' ci 'R_NDB_0' q_ lq 2 9.9999e+009 9.9999e+009 'Initial Discharge on Branch <MAMO001> with length: 1405.0' " + Environment.NewLine +
                @"PDIN 0 0 '' pdin CLTT 'Location' 'L' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 3008 <" + Environment.NewLine +
                @"1405 3008 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"ty 1 lv ll 2 9.9999e+009 9.9999e+009 'Initial WaterLevel on Branch <MAMO001> with length: 1405.0' PDIN 1 0 '' pdin CLTT 'Location' 'WaterLevel' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 0.19 <" + Environment.NewLine +
                @"1405 0.19 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" flin";

            var initialConditions = new InitalFlowConditionsReader().Parse(initialConditionsText);
            Assert.AreEqual(2, initialConditions.Count());
            var flowCondition = initialConditions.First();
            var levelCondition = initialConditions.Last();

            Assert.IsTrue(flowCondition.IsQBoundary);

            Assert.IsTrue(levelCondition.IsLevelBoundary);

            Assert.AreEqual(InterpolationType.Constant,flowCondition.Discharge.Interpolation);
            Assert.AreEqual(InterpolationType.Linear, levelCondition.Level.Interpolation);
 
        }

        [Test]
        public void ParseInitRwecordWithRareCharacters()
        {
            // test with some allowed but rarely used characters: []:
            string initialConditionsText =
                @"FLIN id 'R_RT_001' nm '(null)' ci 'R_RT_001' q_ lq 2 0 9.9999e+009 'Initial Discharge on Branch " +
                    @"<Bovenryn>with length BRCHLT' PDIN 0 0 '' pdin CLTT 'Location' 'Q [m3/s]' cltt CLID '(null)' '(null)' clid TBLE" +
                Environment.NewLine +
                @"0 1327 <" + Environment.NewLine +
                @"706 1326.97 <" + Environment.NewLine +
                @"1293 1326.69 <" + Environment.NewLine +
                @"1880 1326.42 <" + Environment.NewLine +
                @"2467 1326.3 <" + Environment.NewLine +
                @"3053 1326.28 <" + Environment.NewLine +
                @"3640 1326.31 <" + Environment.NewLine +
                @"4227 1326.37 <" + Environment.NewLine +
                @"4814 1326.44 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"ty 1 lv ll 2 9.9999e+009 9.9999e+009 'Initial WaterLevel on Branch <Bovenryn> with length: BRCHLT' PDIN " +
                    @"0 0 '' pdin CLTT 'Location' 'WaterLevel' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0 8.18871 <" + Environment.NewLine +
                @"706 8.14647 <" + Environment.NewLine +
                @"1293 8.11339 <" + Environment.NewLine +
                @"1880 8.08134 <" + Environment.NewLine +
                @"2467 8.05406 <" + Environment.NewLine +
                @"3053 8.02509 <" + Environment.NewLine +
                @"3640 7.99672 <" + Environment.NewLine +
                @"4227 7.96284 <" + Environment.NewLine +
                @"4814 7.93037 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                 @"flin";

            InitalFlowConditionsReader initalFlowConditionsReader = new InitalFlowConditionsReader();

            var initialConditions = initalFlowConditionsReader.Parse(initialConditionsText);

            Assert.AreEqual(1, initialConditions.Count());

            var flowCondition = initialConditions.Last();
            Assert.AreEqual("R_RT_001", flowCondition.ID);
            // Test if record is correctly parsed
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsFalse(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(9, flowCondition.Discharge.Data.Rows.Count);

            Assert.IsTrue(flowCondition.IsLevelBoundary);
            Assert.IsFalse(flowCondition.Level.IsConstant);
            Assert.AreEqual(9, flowCondition.Level.Data.Rows.Count);
        }

        [Test]
        public void SplitRecords()
        {
            string initialConditionsText =
                "GLIN" + Environment.NewLine +
                "fi 0 fr '(null)' FLIN id 'NDB_0' nm '(null)' ci '-1' q_ lq 0 0 9.9999e+009 ty 0 lv ll 0 5 9.9999e+009 flin" + Environment.NewLine +
                "glin" + Environment.NewLine +
                "FLIN id 'R_NDB_0' nm '(null)' ci 'R_NDB_0' q_ lq 2 9.9999e+009 9.9999e+009 'Initial Discharge on Branch <MAMO001> with length: 1405.0' PDIN 0 0 '' pdin CLTT 'Location' 'Q' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                "0 3008 <" + Environment.NewLine +
                "1405 3008 <" + Environment.NewLine +
            "tble flin ";

            InitalFlowConditionsReader initalFlowConditionsReader = new InitalFlowConditionsReader();

            var initialConditions = initalFlowConditionsReader.Parse(initialConditionsText);

            Assert.AreEqual(2, initialConditions.Count());

            var globalCondition = initialConditions.First();
            Assert.IsTrue(globalCondition.IsGlobalDefinition);
            Assert.IsTrue(globalCondition.Level.IsConstant);
            Assert.AreEqual(globalCondition.Level.Constant, 5);

            var flowCondition = initialConditions.Last();

            Assert.IsFalse(flowCondition.IsGlobalDefinition);
            Assert.AreEqual("R_NDB_0", flowCondition.ID);
            Assert.IsTrue(flowCondition.IsQBoundary);
            Assert.IsFalse(flowCondition.IsLevelBoundary);

            Assert.IsFalse(flowCondition.Discharge.IsConstant);
            Assert.AreEqual(1405, (double)flowCondition.Discharge.Data.Rows[1][0], 1.0e-6);
        }

        /// <summary>
        /// Parse record found in SW_max_1.lit\3\INITIAL.DAT
        /// nm field precedes id field; ss undocumented in Help; according to Jaap Zeekant this 
        /// field is a remnant of older Sobek versions and no longer supported
        /// </summary>
        [Test]
        public void ParseGlobalFrictionInDifferentFormat()
        {
            const string initialConditionsText = "GLIN fi 0 fr '(null)' FLIN nm '(null)' ss 0 id '-1' ci '-1' " +
                        "lc 9.9999e+009 q_ lq 0 0 9.9999e+009 ty 0 lv ll 0 3 9.9999e+009 flin glin";

            InitalFlowConditionsReader initalFlowConditionsReader = new InitalFlowConditionsReader();

            var initialConditions = initalFlowConditionsReader.Parse(initialConditionsText);

            Assert.AreEqual(1, initialConditions.Count());

            var globalCondition = initialConditions.First();
            Assert.IsTrue(globalCondition.IsGlobalDefinition);

            Assert.IsTrue(globalCondition.IsQBoundary);
            Assert.IsTrue(globalCondition.Discharge.IsConstant);
            Assert.AreEqual(0, globalCondition.Discharge.Constant, 1.0e-6);

            Assert.IsTrue(globalCondition.IsLevelBoundary);
            Assert.IsTrue(globalCondition.Level.IsConstant);
            Assert.AreEqual(3, globalCondition.Level.Constant, 1.0e-6);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ReadNationalModel()
        {
            string structureDefinitionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"initial\NATSOBEKINITIAL.DAT");

            InitalFlowConditionsReader initalFlowConditionsReader = new InitalFlowConditionsReader();


            FlowInitialCondition[] initialConditions = null;
            TestHelper.AssertIsFasterThan(1100, "Read 433 initial conditions", 
                () => { initialConditions = initalFlowConditionsReader.Read(structureDefinitionFile).ToArray(); });
            
            Assert.AreEqual(433, initialConditions.Length);

            // last condition is 
            // FLIN id 'R_5636451' nm '(null)' ci 'R_5636451' q_ lq 0 0 9.9999e+009 ty 1 lv ll 0 -0.4 9.9999e+009  flin
            var lastCondition = initialConditions.Last();

            Assert.AreEqual("R_5636451", lastCondition.ID);

        }

        [Test]
        public void ReadLineFromLMWModel()
        {
            var line = @"FLIN id '33' nm 'initial' ci '33' ty 1 lv ll 0 7 q_ lq 0  flin";

            var initalFlowConditionsReader = new InitalFlowConditionsReader();

            var initialConditions = initalFlowConditionsReader.Parse(line);

            Assert.AreEqual(1, initialConditions.Count());
        }
    }
}
