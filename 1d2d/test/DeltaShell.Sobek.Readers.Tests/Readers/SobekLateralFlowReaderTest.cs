using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekLateralFlowReaderTest
    {
        [Test]
        public void ConstantDischarge()
        {
            const string source = @"FLBR id 'Zwolle' sc 0 lt 0 dc lt 0 0.071 0 flbr";

            SobekLateralFlowReader sobekLateralFlowReader = new SobekLateralFlowReader();
            var sobekLateralFlow = sobekLateralFlowReader.GetLateralFlow(source, new Dictionary<string, IList<string>>());
            Assert.AreEqual("Zwolle", sobekLateralFlow.Id);
            Assert.IsTrue(sobekLateralFlow.IsPointDischarge);
            Assert.IsTrue(sobekLateralFlow.IsConstantDischarge);
            Assert.AreEqual(0.071, sobekLateralFlow.ConstantDischarge, 1.0e-6);
        }

        [Test]
        public void TimeDependentDischarge()
        {
            string source =
                //@"FLBR id 'L_MS_01' sc 0 lc 0 dc lt 1 0 0 PDIN 0 0 '' pdin CLTT 'Time' 'Q' cltt CLID '(null)' '(null)' clid " + Environment.NewLine +
                @"FLBR id 'L_MS_01' sc 0 lc 0 dc lt 1 0 0 PDIN 0 0 '' pdin CLTT 'Time' 'Q' cltt CLID '(null)' '(null)' clid " + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1998/10/01;00:00:00' 11.76 <" + Environment.NewLine +
                @"'1998/10/15;12:00:00' 11.76 <" + Environment.NewLine +
                @"'1998/11/09;12:00:00' 10.63 <" + Environment.NewLine +
                @"'1998/11/10;12:00:00' 9.26 <" + Environment.NewLine +
                @"tble flbr";


            SobekLateralFlowReader sobekLateralFlowReader = new SobekLateralFlowReader();
            var sobekLateralFlow = sobekLateralFlowReader.GetLateralFlow(source, new Dictionary<string, IList<string>>());
            Assert.AreEqual("L_MS_01", sobekLateralFlow.Id);
            Assert.IsTrue(sobekLateralFlow.IsPointDischarge);
            Assert.IsFalse(sobekLateralFlow.IsConstantDischarge);
            Assert.AreEqual(4, sobekLateralFlow.FlowTimeTable.Rows.Count);
            Assert.AreEqual(new DateTime(1998, 10, 01, 00, 00, 00), sobekLateralFlow.FlowTimeTable.Rows[0][0]);
            Assert.AreEqual(11.76, (double)sobekLateralFlow.FlowTimeTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(new DateTime(1998, 11, 10, 12, 00, 00), sobekLateralFlow.FlowTimeTable.Rows[3][0]);
            Assert.AreEqual(9.26, (double)sobekLateralFlow.FlowTimeTable.Rows[3][1], 1.0e-6);
        }


        [Test]
        public void ParseLaterals()
        {
            string source =
                @"FLBR id 'Zwolle' sc 0 lt 0 dc lt 0 0.071 0 flbr" + Environment.NewLine +
                @"FLBR id 'Herfte' sc 0 lt 0 dc lt 0 3.67 0 flbr" + Environment.NewLine +
                @"FLBR id 'Linterzijl' sc 0 lt 0 dc lt 0 3.33 0 flbr" + Environment.NewLine +
                @"FLBR id 'Marslanden_Noord_Zuid' sc 0 lt 0 dc lt 0 0.058 0 flbr" + Environment.NewLine;
            SobekLateralFlowReader sobekLateralFlowReader = new SobekLateralFlowReader();
            var lateralFlows = sobekLateralFlowReader.ParseBoundaryConditions(source, new Dictionary<string, IList<string>>());
            Assert.AreEqual(4, lateralFlows.Count());
            Assert.AreEqual(4, lateralFlows.Where(lc => lc.IsConstantDischarge == true).Count());
            Assert.AreEqual(4, lateralFlows.Where(lc => lc.IsPointDischarge == true).Count());
        }


        [Test]
        public void LateralFlowDataMaas()
        {
            string initialConditionsText =
                @"FLBR id 'L_02' sc 0 lc 0 dc lt 1 0 0 PDIN 0 0 '' pdin CLTT 'Time' 'Q' cltt CLID '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'2000/12/15;00:00:00' 5.9 <" + Environment.NewLine +
                @"'2000/12/15;12:00:00' 5.9 <" + Environment.NewLine +
                @"'2000/12/16;12:00:00' 5.9 <" + Environment.NewLine +
                @"'2000/12/17;12:00:00' 6.48 <" + Environment.NewLine +
                @"'2000/12/18;12:00:00' 3.31 <" + Environment.NewLine +
                @"'2000/12/19;12:00:00' 2.6 <" + Environment.NewLine +
                @"'2000/12/20;12:00:00' 2.5 <" + Environment.NewLine +
                @"'2000/12/21;12:00:00' 2.34 <" + Environment.NewLine +
                @"'2000/12/22;12:00:00' 2.27 <" + Environment.NewLine +
                @"'2000/12/23;12:00:00' 2.18 <" + Environment.NewLine +
                @"'2000/12/24;12:00:00' 2.2 <" + Environment.NewLine +
                @"'2000/12/25;12:00:00' 2.16 <" + Environment.NewLine +
                @"'2000/12/26;12:00:00' 2.11 <" + Environment.NewLine +
                @"'2000/12/27;12:00:00' 2.13 <" + Environment.NewLine +
                @"'2000/12/28;12:00:00' 2.17 <" + Environment.NewLine +
                @"'2000/12/29;12:00:00' 2.63 <" + Environment.NewLine +
                @"'2000/12/30;12:00:00' 2.41 <" + Environment.NewLine +
                @"'2000/12/31;12:00:00' 2.26 <" + Environment.NewLine +
                @"'2001/01/01;12:00:00' 2.24 <" + Environment.NewLine +
                @"'2001/01/02;12:00:00' 2.36 <" + Environment.NewLine +
                @"'2001/01/03;12:00:00' 2.76 <" + Environment.NewLine +
                @"'2001/01/04;12:00:00' 2.53 <" + Environment.NewLine +
                @"'2001/01/05;12:00:00' 2.39 <" + Environment.NewLine +
                @"'2001/01/06;12:00:00' 3.08 <" + Environment.NewLine +
                @"'2001/01/07;12:00:00' 8.2 <" + Environment.NewLine +
                @"'2001/01/08;12:00:00' 4.99 <" + Environment.NewLine +
                @"'2001/01/09;12:00:00' 3.38 <" + Environment.NewLine +
                @"'2001/01/10;12:00:00' 2.75 <" + Environment.NewLine +
                @"'2001/01/11;12:00:00' 2.64 <" + Environment.NewLine +
                @"'2001/01/12;12:00:00' 5.41 <" + Environment.NewLine +
                @"'2001/01/13;12:00:00' 3.58 <" + Environment.NewLine +
                @"'2001/01/14;12:00:00' 2.9 <" + Environment.NewLine +
                @"'2001/01/15;12:00:00' 2.63 <" + Environment.NewLine +
                @"'2001/01/16;12:00:00' 2.54 <" + Environment.NewLine +
                @"'2001/01/17;12:00:00' 2.4 <" + Environment.NewLine +
                @"'2001/01/18;12:00:00' 2.35 <" + Environment.NewLine +
                @"tble " + Environment.NewLine +
                @"flbr" + Environment.NewLine +
                @"FLBR id 'L_03' sc 0 lc 0 dc lt 1 0 0 PDIN 0 0 '' pdin CLTT 'Time' 'Q' cltt CLID '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'2000/12/15;00:00:00' -16.28 <" + Environment.NewLine +
                @"'2000/12/15;12:00:00' -16.28 <" + Environment.NewLine +
                @"'2000/12/16;12:00:00' -15.36 <" + Environment.NewLine +
                @"'2000/12/17;12:00:00' -14.71 <" + Environment.NewLine +
                @"'2000/12/18;12:00:00' -14.94 <" + Environment.NewLine +
                @"'2000/12/19;12:00:00' -14.66 <" + Environment.NewLine +
                @"'2000/12/20;12:00:00' -13.51 <" + Environment.NewLine +
                @"'2000/12/21;12:00:00' -14.28 <" + Environment.NewLine +
                @"'2000/12/22;12:00:00' -16.3 <" + Environment.NewLine +
                @"'2000/12/23;12:00:00' -14.37 <" + Environment.NewLine +
                @"'2000/12/24;12:00:00' -13.3 <" + Environment.NewLine +
                @"'2000/12/25;12:00:00' -13.26 <" + Environment.NewLine +
                @"'2000/12/26;12:00:00' -13.32 <" + Environment.NewLine +
                @"'2000/12/27;12:00:00' -13.14 <" + Environment.NewLine +
                @"'2000/12/28;12:00:00' -11.9 <" + Environment.NewLine +
                @"'2000/12/29;12:00:00' -12.01 <" + Environment.NewLine +
                @"'2000/12/30;12:00:00' -11.48 <" + Environment.NewLine +
                @"'2000/12/31;12:00:00' -11.31 <" + Environment.NewLine +
                @"'2001/01/01;12:00:00' -11.38 <" + Environment.NewLine +
                @"'2001/01/02;12:00:00' -11.81 <" + Environment.NewLine +
                @"'2001/01/03;12:00:00' -13.9 <" + Environment.NewLine +
                @"'2001/01/04;12:00:00' -15.25 <" + Environment.NewLine +
                @"'2001/01/05;12:00:00' -15.57 <" + Environment.NewLine +
                @"'2001/01/06;12:00:00' -16.75 <" + Environment.NewLine +
                @"'2001/01/07;12:00:00' -16.94 <" + Environment.NewLine +
                @"'2001/01/08;12:00:00' -15.89 <" + Environment.NewLine +
                @"'2001/01/09;12:00:00' -14.7 <" + Environment.NewLine +
                @"'2001/01/10;12:00:00' -14.63 <" + Environment.NewLine +
                @"'2001/01/11;12:00:00' -14.9 <" + Environment.NewLine +
                @"'2001/01/12;12:00:00' -14.76 <" + Environment.NewLine +
                @"'2001/01/13;12:00:00' -14.34 <" + Environment.NewLine +
                @"'2001/01/14;12:00:00' -13.44 <" + Environment.NewLine +
                @"'2001/01/15;12:00:00' -13.63 <" + Environment.NewLine +
                @"'2001/01/16;12:00:00' -13.63 <" + Environment.NewLine +
                @"'2001/01/17;12:00:00' -13.18 <" + Environment.NewLine +
                @"'2001/01/18;12:00:00' -13.58 <" + Environment.NewLine +
                @"tble " + Environment.NewLine +
                @"flbr";

            var reader = new SobekLateralFlowReader();

            var lateralFlowRecords = reader.ParseBoundaryConditions(initialConditionsText, new Dictionary<string, IList<string>>());

            Assert.AreEqual(2, lateralFlowRecords.Count());
        }


        // record found in rijnmodel 
        // s2 not found in documentation
        // dc lt 5 and sd is a combination not described in docs
        [Test]
        public void ReadLateralFlowReRijn301()
        {
            string source =
                @"FLBR id 'AL1_O_016' sc 0 dc lt 5 9.9999e+009 9.9999e+009 s2 'AL1_O_016_uit' ar 1.21e+007 bl 40.46 ih 40.46 u1 1 ca 1 1 0 0 cj 'AL1_O_016_1' 'AL1_O_016_2' '-1' '-1' cb 0 0 0 0 ck '-1' '-1' '-1' '-1' lt 0 sd 'AL1_O_016_in' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";
            var reader = new SobekLateralFlowReader();

            var lateralFlowRecords = reader.ParseBoundaryConditions(source, new Dictionary<string, IList<string>>());

            Assert.AreEqual(1, lateralFlowRecords.Count());
            var lateralFlow = lateralFlowRecords.FirstOrDefault();
            Assert.AreEqual("AL1_O_016", lateralFlow.Id);
        }

        [Test]
        public void ParseRecordQhTable()
        {
            var source =
                @"FLBR id 'AL1_1031' sc 0 dc lw 2 9.9999e+009 9.9999e+009 'Lateral Discharge' PDIN 0 0 '' pdin CLTT 'H [m]' 'Q' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"49.86 0 < " + Environment.NewLine +
                @"49.96 -10 < " + Environment.NewLine +
                @"52.44 -952 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" lt 0 sd '-1' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";
            var reader = new SobekLateralFlowReader();

            var lateralFlowRecords = reader.ParseBoundaryConditions(source, new Dictionary<string, IList<string>>());

            Assert.AreEqual(1, lateralFlowRecords.Count());
            var lateralFlow = lateralFlowRecords.First();
            Assert.IsNotNull(lateralFlow.LevelQhTable);
            var table = lateralFlow.LevelQhTable;
            Assert.AreEqual(49.86, (double)table.Rows[0][0], 1.0e-6);
            Assert.AreEqual(0.0, (double)table.Rows[0][1], 1.0e-6);
            Assert.AreEqual(52.44, (double)table.Rows[2][0], 1.0e-6);
            Assert.AreEqual(-952, (double)table.Rows[2][1], 1.0e-6);
        }



        [Test]
        public void ReadLateralFlowWithQhTableAndConstantInterpolation()
        {
            var source =
                @"FLBR id 'AL1_1031' sc 0 dc lw 2 9.9999e+009 9.9999e+009 'Lateral Discharge' PDIN 1 0 '' pdin CLTT 'H [m]' 'Q' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"49.86 0 < " + Environment.NewLine +
                @"49.96 -10 < " + Environment.NewLine +
                @"52.44 -952 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" lt 0 sd '-1' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";
            var reader = new SobekLateralFlowReader();

            var lateralFlow = reader.GetLateralFlow(source, new Dictionary<string, IList<string>>());
            // Qh should always be linear
            Assert.IsNotNull(lateralFlow.LevelQhTable);
            Assert.AreEqual(InterpolationType.Linear, lateralFlow.InterpolationType);
        }

        [Test]
        public void ParseDiffuseLateralSobek2()
        {
            //diffuse lateral discharge on the reach; SOBEK 2
            string source =
                @"FLDI id '2' ci '2' sc 0 lt -1 dc lt 0 33.3 0 fldi";
            SobekLateralFlowReader sobekLateralFlowReader = new SobekLateralFlowReader();
            var lateralFlows = sobekLateralFlowReader.ParseBoundaryConditions(source, new Dictionary<string, IList<string>>());
            var diffuseLF = lateralFlows.First();
            Assert.IsFalse(diffuseLF.IsPointDischarge);

        }

        [Test]
        public void ParseDiffuseLatSobekRE()
        {
            string source =
                @"FLBR id '28' sc 0 dc lw 2 9.9999e+009 9.9999e+009 'Lateral Discharge' PDIN 0 0 '' pdin CLTT 'H [m]' 'q' cltt CLID '(null)' '(null)' clid TBLE" +
                Environment.NewLine +
                @"0.1 0.005 < " + Environment.NewLine +
                @"0.5 0.001 < " + Environment.NewLine +
                @"5 0.0001 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"lt 1800 sd '-1' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";
                SobekLateralFlowReader sobekLateralFlowReader = new SobekLateralFlowReader();
                var lateralFlows = sobekLateralFlowReader.ParseBoundaryConditions(source, new Dictionary<string, IList<string>>());
                var diffuseLF = lateralFlows.First();
                Assert.IsFalse(diffuseLF.IsPointDischarge);
                Assert.AreEqual(1800, diffuseLF.Length);

        }

        [Test]
        public void ParseRationalMethodWithConstantIntensity()
        {
            string source =
                @"FLBR id 'E_AFW_10' sc 0 lt 0 dc lt 6 ir 0.75 ms 'VOLKEL EN DEELEN' ii 0.25 ar 1234 flbr" +
                Environment.NewLine +
                @"FLBR id 'E_AFW_100' sc 0 lt 0 dc lt 6 ir 3.0e-01 ms 'VOLKEL EN DEELEN' ii 0 ar 30000 flbr";

            var lateralFlows = new SobekLateralFlowReader().ParseBoundaryConditions(source, new Dictionary<string, IList<string>>()).ToList();
            Assert.AreEqual(2, lateralFlows.Count);
            Assert.IsTrue(lateralFlows.First().IsConstantDischarge);
            Assert.AreEqual(1.234, lateralFlows.First().ConstantDischarge);
            Assert.IsTrue(lateralFlows.Last().IsConstantDischarge);
            Assert.AreEqual(9.0, lateralFlows.Last().ConstantDischarge);
        }

    }
}
