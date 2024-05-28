using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekGridPointReaderTest
    {
        [Test]
        public void ReadGridPointsOfOneBranch()
        {
            string input =
                @"GR_1.2" + Environment.NewLine +
                @"GRID id '1' ci '1' re 0 dc 0 gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' '3' '1' 0 0 '' '' '' <" + Environment.NewLine +
                @"100 0 '2' '1_1' '1_1' 100 0 '' '' '' <" + Environment.NewLine +
                @"200 0 '1_1' '1_2' '1_2' 200 0 '' '' '' <" + Environment.NewLine +
                @"300 0 '1_2' '1_3' '1_3' 300 0 '' '' '' <" + Environment.NewLine +
                @"400 0 '1_3' '1_4' '1_4' 400 0 '' '' '' <" + Environment.NewLine +
                @"500 0 '1_4' '1_5' '1_5' 500 0 '' '' '' <" + Environment.NewLine +
                @"600 0 '1_5' '1_6' '1_6' 600 0 '' '' '' <" + Environment.NewLine +
                @"700 0 '1_6' '1_7' '1_7' 700 0 '' '' '' <" + Environment.NewLine +
                @"800 0 '1_7' '1_8' '1_8' 800 0 '' '' '' <" + Environment.NewLine +
                @"900 0 '1_8' '1_9' '1_9' 900 0 '' '' '' <" + Environment.NewLine +
                @"1000 0 '1_9' '1_10' '1_10' 1000 0 '' '' '' <" + Environment.NewLine +
                @"1100 0 '1_10' '1_11' '1_11' 1100 0 '' '' '' <" + Environment.NewLine +
                @"1200 0 '1_11' '1_12' '1_12' 1200 0 '' '' '' <" + Environment.NewLine +
                @"1300 0 '1_12' '1_13' '1_13' 1300 0 '' '' '' <" + Environment.NewLine +
                @"1400 0 '1_13' '1_14' '1_14' 1400 0 '' '' '' <" + Environment.NewLine +
                @"1500 0 '1_14' '1_15' '1_15' 1500 0 '' '' '' <" + Environment.NewLine +
                @"1600 0 '1_15' '1_16' '1_16' 1600 0 '' '' '' <" + Environment.NewLine +
                @"1700 0 '1_16' '1_17' '1_17' 1700 0 '' '' '' <" + Environment.NewLine +
                @"1800 0 '1_17' '1_18' '1_18' 1800 0 '' '' '' <" + Environment.NewLine +
                @"1900 0 '1_18' '1_19' '1_19' 1900 0 '' '' '' <" + Environment.NewLine +
                @"2000 0 '3' '4' '' 2000 0 '' '' '' <" + Environment.NewLine +
                @"tble grid";

            var gridPointReader = new SobekGridPointsReader
                {
                    SobekGridPointsType = SobekGridPointsReader.SobekGridPointsTypeEnum.Gr12
                };
            var lstGridPointsBranch = gridPointReader.Parse(input).ToList();
            Assert.AreEqual(1, lstGridPointsBranch.Count);
            Assert.AreEqual(21, lstGridPointsBranch[0].GridPoints.Count);
            Assert.AreEqual(300.0, lstGridPointsBranch[0].GridPoints[3].Offset);
            Assert.AreEqual("1_3", lstGridPointsBranch[0].GridPoints[3].Id);
            Assert.AreEqual("1_3", lstGridPointsBranch[0].GridPoints[3].SegmentId);
        }

        [Test]
        public void ReadGridPointsOfTwoBranches()
        {
            string input =
                @"GR_1.2" + Environment.NewLine +
                @"GRID id '1' ci '1' re 0 dc 0 gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' '3' '1' 0 0 '' '' '' <" + Environment.NewLine +
                @"100 0 '2' '1_1' '1_1' 100 0 '' '' '' <" + Environment.NewLine +
                @"tble grid" + Environment.NewLine +
                @"GRID id '2' ci '2' re 0 dc 0 gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' '5' '2' 0 0 '' '' '' <" + Environment.NewLine +
                @"100 0 '4' '2_1' '2_2' 100 0 '' '' '' <" + Environment.NewLine +
                @"tble grid";
            
            var gridPointReader = new SobekGridPointsReader
            {
                SobekGridPointsType = SobekGridPointsReader.SobekGridPointsTypeEnum.Gr12
            };
            var lstGridPointsBranch = gridPointReader.Parse(input).ToList();
            Assert.AreEqual(2,lstGridPointsBranch.Count);
            Assert.AreEqual("2", lstGridPointsBranch[1].BranchID);
            Assert.AreEqual(100.0, lstGridPointsBranch[1].GridPoints.Last().Offset);
            Assert.AreEqual("2_1", lstGridPointsBranch[1].GridPoints.Last().Id);
            Assert.AreEqual("2_2", lstGridPointsBranch[1].GridPoints.Last().SegmentId);
        }

        [Test]
        public void ReadGridPointsOfGR1dot1()
        {
            string input =
                @"GR_1.1" + Environment.NewLine +
                @"GRID id '1' ci '1' re 0 dc 0 gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' '1' '1_1' 0 0 <" + Environment.NewLine +
                @"127.015281637859 0 '1_1' 'C4' '1_2' 127.015281637859 0 <" + Environment.NewLine +
                @"3359.319312144 0 '1_4' '6' '1_5' 3359.319312144 0 <" + Environment.NewLine +
                @"5603.78133990958 0 '1_5' '7' '1_6' 5603.78133990958 0 <" + Environment.NewLine +
                @"9078.58095625083 0 '1_6' '5' '1_7' 9078.58095625083 0 <" + Environment.NewLine +
                @"9189.0100423178 0 '1_7' '1_1' '1_8' 9189.0100423178 0 <" + Environment.NewLine +
                @"9190.0100423178 0 '1_9' '1_2' '1_10' 9190.0100423178 0 <" + Environment.NewLine +
                @"16362.208604901 0 '1_10' '14' '1_11' 16362.208604901 0 <" + Environment.NewLine +
                @"16431.2984695909 0 '1_11' '1_3' '1_12' 16431.2984695909 0 <" + Environment.NewLine +
                @"16432.2984695909 0 '1_13' '1_4' '1_14' 16432.2984695909 0 <" + Environment.NewLine +
                @"19877.043946651 0 '1_14' 'C3' '1_15' 19877.043946651 0 <" + Environment.NewLine +
                @"20000 0 '1_16' '2' '' 20000 0 <" + Environment.NewLine +
                @"tble grid";

            var lstGridPointsBranch = new SobekGridPointsReader().Parse(input).ToList();
            Assert.AreEqual(1, lstGridPointsBranch.Count);
            Assert.AreEqual(12, lstGridPointsBranch[0].GridPoints.Count);
        }

        [Test]
        public void ReadGridPointsOfGR1dot0()
        {
            string input =
                @"GR_1.0" + Environment.NewLine +
                @"GRID id '2' ci '2' gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' '4' '2' <" + Environment.NewLine +
                @"83.1666666669771 0 '2' '2_1' '2_3' <" + Environment.NewLine +
                @"166.333333333023 0 '7' '2_2' '2_5' <" + Environment.NewLine +
                @"249.5 0 '2_5' '15' '2_4' <" + Environment.NewLine +
                @"250 0 '2_4' '8' '2_1' <" + Environment.NewLine +
                @"250.5 0 '2_1' '16' '2_2' <" + Environment.NewLine +
                @"333.666666666977 0 '2_2' '2_3' '2_6' <" + Environment.NewLine +
                @"416.833333333023 0 '5' '2_4' '2_7' <" + Environment.NewLine +
                @"500 0 '2_7' '1' '' <" + Environment.NewLine +
                @"tble grid";

            var lstGridPointsBranch = new SobekGridPointsReader().Parse(input).ToList();
            Assert.AreEqual(1, lstGridPointsBranch.Count);
            Assert.AreEqual(9, lstGridPointsBranch[0].GridPoints.Count);
        }

        [Test]
        public void ReadGridPointName()
        {
            string input =
                @"GR_1.2" + Environment.NewLine +
                @"GRID id '1' ci '1' re 0 dc 0 gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' 'Upstream Waterlevel Boundary1' '1' 0 0 '' 'Upstream Waterlevel Boundary1' '' <" + Environment.NewLine +
                @"99.9 0 '1' '1_1' '1_1' 99.9 0 '' '' '' <" + Environment.NewLine +
                @"199.8 0 '1_1' '1_2' '1_2' 199.8 0 '' '' '' <" + Environment.NewLine +
                @"299.7 0 '5' '1_3' '1_3' 299.7 0 '' '' '' <" + Environment.NewLine +
                @"399.6 0 '1_3' '1_4' '1_4' 399.6 0 '' '' '' <" + Environment.NewLine +
                @"499.5 0 '1_4' '4' '3' 499.5 0 '' '' '' <" + Environment.NewLine +
                @"500 0 '3' 'Flow - Weir1' '2' 500 0 '' 'Flow - Weir1' '' <" + Environment.NewLine +
                @"500.5 0 '2' '5' '4' 500.5 0 '' '' '' <" + Environment.NewLine +
                @"600.400000000001 0 '4' '1_5' '1_5' 600.400000000001 0 '' '' '' <" + Environment.NewLine +
                @"700.300000000001 0 '1_5' '1_6' '1_6' 700.300000000001 0 '' '' '' <" + Environment.NewLine +
                @"800.2 0 '1_6' '1_7' '1_7' 800.2 0 '' '' '' <" + Environment.NewLine +
                @"900.1 0 '1_7' '1_8' '1_8' 900.1 0 '' '' '' <" + Environment.NewLine +
                @"1000 0 '1_8' 'Downstream waterlevel boundary1' '' 1000 0 '' 'Downstream waterlevel boundary1' '' <" + Environment.NewLine +
                @"tble grid";

            var gridPointReader = new SobekGridPointsReader
                {
                    SobekGridPointsType = SobekGridPointsReader.SobekGridPointsTypeEnum.Gr12
                };
            var lstGridPointsBranch = gridPointReader.Parse(input);
            var lastPoint = lstGridPointsBranch.Last().GridPoints.Last();
            Assert.AreEqual("Downstream waterlevel boundary1", lastPoint.Id);
            Assert.AreEqual("Downstream waterlevel boundary1", lastPoint.Name);
        }

        [Test]
        public void ReadGridPointNameAndId()
        {
            string input =
                @"GR_1.2" + Environment.NewLine +
                @"GRID id '1' ci '1' re 0 dc 0 gr gr " + Environment.NewLine +
                @"'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 '' '3' '1' 66576.783053912 443710.052352591 '' '' '' <" + Environment.NewLine +
                @"979.120032035962 0 '1' '1_1' '1_1' 67422.0708197074 443215.922177302 '' 'Node1' '' <" +
                Environment.NewLine +
                @"1958.24006407198 0 '1_1' '1_2' '1_2' 68267.3585855028 442721.792002014 '' '' '' <" +
                Environment.NewLine +
                @"2937.36009610793 0 '1_2' '1_3' '1_3' 69112.6463512982 442227.661826725 '' '' '' <" +
                Environment.NewLine +
                @"3916.48012814389 0 '1_3' '1_4' '1_4' 69957.9341170936 441733.531651437 '' '' '' <" +
                Environment.NewLine +
                @"4895.60016017991 0 '1_4' '1_5' '1_5' 70803.221882889 441239.401476148 '' '' '' <" +
                Environment.NewLine +
                @"5874.72019221587 0 '1_5' '1_6' '1_6' 71648.5096486844 440745.271300859 '' '' '' <" +
                Environment.NewLine +
                @"6853.84022425182 0 '1_6' '1_7' '1_7' 72493.7974144798 440251.141125571 '' '' '' <" +
                Environment.NewLine +
                @"7832.96025628784 0 '1_7' '1_8' '1_8' 73339.0851802752 439757.010950282 '' '' '' <" +
                Environment.NewLine +
                @"8812.0802883238 0 '1_8' '1_9' '1_9' 74184.3729460706 439262.880774994 '' '' '' <" +
                Environment.NewLine +
                @"9791.20032035975 0 '1_9' '1_10' '1_10' 75029.660711866 438768.750599705 '' '' '' <" +
                Environment.NewLine +
                @"10770.3203523958 0 '1_10' '1_11' '1_11' 75874.9484776614 438274.620424416 '' '' '' <" +
                Environment.NewLine +
                @"11749.4403844317 0 '1_11' '1_12' '1_12' 76720.2362434568 437780.490249128 '' '' '' <" +
                Environment.NewLine +
                @"12728.5604164677 0 '1_12' '1_13' '1_13' 77565.5240092522 437286.360073839 '' '' '' <" +
                Environment.NewLine +
                @"13707.6804485037 0 '1_13' '1_14' '1_14' 78410.8117750476 436792.22989855 '' '' '' <" +
                Environment.NewLine +
                @"14686.8004805397 0 '1_14' '1_15' '1_15' 79256.099540843 436298.099723262 '' '' '' <" +
                Environment.NewLine +
                @"15665.9205125756 0 '1_15' '1_16' '1_16' 80101.3873066384 435803.969547973 '' '' '' <" +
                Environment.NewLine +
                @"16645.0405446116 0 '1_16' '1_17' '1_17' 80946.6750724338 435309.839372685 '' 'NodeZoveel' '' <" +
                Environment.NewLine +
                @"17624.1605766476 0 '1_17' '2' '' 81791.9628382292 434815.709197396 '' '' '' <" + Environment.NewLine +
                @"tble grid";

            var gridPointReader = new SobekGridPointsReader
            {
                SobekGridPointsType = SobekGridPointsReader.SobekGridPointsTypeEnum.Gr12
            };
            var lstGridPointsBranch = gridPointReader.Parse(input);
            var namedPoint = lstGridPointsBranch.Last().GridPoints[1];
            Assert.AreEqual("1_1", namedPoint.Id);
            Assert.AreEqual("Node1", namedPoint.Name);
        }

        [Test]
        public void ReadReGridRecord()
        {
            var source =
                @"GRID id 'LA1_883' nm '(null)' ci 'LA1_14' lc 9.9999e+009 se 0 oc 0 gr gr 'GridPoints on Branch <La06UPLa-MdLa> with length: 1341.0' PDIN 0 0 '' pdin CLTT 'Location [m]' '1/R [1/m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 9.9999e+009 < " + Environment.NewLine +
                @"670 9.9999e+009 < " + Environment.NewLine +
                @"1341 9.9999e+009 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" grid";
            var calcGrid = SobekGridPointsReader.GetSobekGridPointsPerBranchRe(source);
            Assert.AreEqual(3, calcGrid.GridPoints.Count);
            Assert.AreEqual(0, calcGrid.GridPoints[0].Offset, 1.0e-6);
            Assert.AreEqual(670, calcGrid.GridPoints[1].Offset, 1.0e-6);
            Assert.AreEqual(1341, calcGrid.GridPoints[2].Offset, 1.0e-6);
        }

        [Test]
        public void ReadReGridFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly,  @"ReModels\RIJN301.SBK\8\DEFGRD.1");
            var calcGrids = new SobekGridPointsReader().Read(path).ToList();
            Assert.AreEqual(38, calcGrids.Count);

            // the ones with oc = 1 (on cross sections) have no points (16x)
            Assert.AreEqual(calcGrids.Count(c => !c.GridPoints.Any()), 16, "Number of branches without points");
        }
    }
}
