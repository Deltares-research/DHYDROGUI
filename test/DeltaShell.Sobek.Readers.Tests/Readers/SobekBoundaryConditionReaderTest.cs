using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekBoundaryConditionReaderTest
    {
        [Test, Category(TestCategory.Performance)]
        public void TestPerformanceParsingOneRecord()
        {
            var path = TestHelper.GetTestFilePath("FlowBoundaryConditionRecord.txt");
            var data = File.ReadAllText(path);

            TestHelper.AssertIsFasterThan(300,() => new SobekBoundaryConditionReader().Parse(data).ToList(), true, true);
        }

        [Test]
        public void ConstantLevel()
        {
            const string initialConditionsText =
                @"FLBO id '1' ty 0 h_ wd 0 1.2 0 flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Constant, boundaryCondition.StorageType);
            Assert.AreEqual(1.2, boundaryCondition.LevelConstant, 1.0e-6);
        }

        [Test]
        public void ConstantDischarge()
        {
            const string initialConditionsText =
                @"FLBO id 'N_NDB_3' ty 1 q_ dw 0 0.001 0 flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Constant, boundaryCondition.StorageType);
            Assert.AreEqual("N_NDB_3", boundaryCondition.ID);
            Assert.AreEqual(0.001, boundaryCondition.FlowConstant, 1.0e-6);
        }

        [Test]
        public void BoundaryConditionWithTableLibraryShouldNotThowException()
        {
            //Variable water level as a function of time from a separate table library are not supported
            //if the table match StartsWith "H_ WT 1" it will skip reading the table
            string text =
                @"FLBO id 'ND_7' st 0 ty 0 h_ wt 11 'BC_9' flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            Assert.DoesNotThrow(() => boundaryConditions.GetFlowBoundaryCondition(text));
        }

        [Test] public void TableLevelTime()
        {
            string initialConditionsText =
                @"FLBO id '1' ty 0 h_ wt 1 0 0 PDIN 0 0  pdin" + Environment.NewLine + 
                @"TBLE" + Environment.NewLine + 
                @"'1975/12/31;21:00:00' -.35 < " + Environment.NewLine + 
                @"'1975/12/31;22:00:00' .1 < " + Environment.NewLine + 
                @"'1975/12/31;23:00:00' .56 < " + Environment.NewLine + 
                @"'1976/01/03;03:00:00' .66 < " + Environment.NewLine + 
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Variable, boundaryCondition.StorageType);
            Assert.AreEqual(4, boundaryCondition.LevelTimeTable.Rows.Count);
            Assert.AreEqual(new DateTime(1975, 12, 31, 21, 00, 00), boundaryCondition.LevelTimeTable.Rows[0][0]);
            Assert.AreEqual(-0.35, (double)boundaryCondition.LevelTimeTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(new DateTime(1976, 01, 03, 03, 00, 00), boundaryCondition.LevelTimeTable.Rows[3][0]);
            Assert.AreEqual(0.66, (double)boundaryCondition.LevelTimeTable.Rows[3][1], 1.0e-6);
        }

        [Test]
        public void TableDischargeTime()
        {
            string initialConditionsText =
                @"FLBO id 'SH2' ty 1 q_ dt 1 0 0 PDIN 0 0  pdin" + Environment.NewLine + 
                @"TBLE" + Environment.NewLine + 
                @"'2001/09/14;00:00:00' 0.1 <" + Environment.NewLine + 
                @"'2001/09/15;00:00:00' 0. <" + Environment.NewLine + 
                @"'2001/09/19;00:00:00' 0. <" + Environment.NewLine +
                @"'2001/10/01;00:00:00' 0.2 <" + Environment.NewLine + 
                @"tble flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Variable, boundaryCondition.StorageType);
            Assert.AreEqual(4, boundaryCondition.FlowTimeTable.Rows.Count);
            Assert.AreEqual(new DateTime(2001, 09, 14, 00, 00, 00), boundaryCondition.FlowTimeTable.Rows[0][0]);
            Assert.AreEqual(0.1, (double)boundaryCondition.FlowTimeTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(new DateTime(2001, 10, 01, 00, 00, 00), boundaryCondition.FlowTimeTable.Rows[3][0]);
            Assert.AreEqual(0.2, (double)boundaryCondition.FlowTimeTable.Rows[3][1], 1.0e-6);
        }

        [Test]
        public void TableDischargeQhType4()
        {
            string initialConditionsText =
                @"FLBO  id '55' ty 1 q_ dw 4 0 0" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @" 15 40 < " + Environment.NewLine +
                @" 18 77 < " + Environment.NewLine +
                @" 19 88 < " + Environment.NewLine +
                @" 20 90 <" + Environment.NewLine +
                @"tble flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, boundaryCondition.StorageType);
            Assert.AreEqual(4, boundaryCondition.FlowHqTable.Rows.Count);
            Assert.AreEqual(15, (double)boundaryCondition.FlowHqTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(40, (double)boundaryCondition.FlowHqTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(20, (double)boundaryCondition.FlowHqTable.Rows[3][0], 1.0e-6);
            Assert.AreEqual(90, (double)boundaryCondition.FlowHqTable.Rows[3][1], 1.0e-6);
        }

        /// <summary>
        /// q_ dw 1 = q_ dw 4
        /// </summary>
        [Test]
        public void TableDischargeQhType1()
        {
            string text =
                @"FLBO  id '55' ty 1 q_ dw 1 0 0" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @" 15 40 < " + Environment.NewLine +
                @" 18 77 < " + Environment.NewLine +
                @" 19 88 < " + Environment.NewLine +
                @" 20 90 <" + Environment.NewLine +
                @"tble flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(text);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, boundaryCondition.StorageType);
            Assert.AreEqual(4, boundaryCondition.FlowHqTable.Rows.Count);
            Assert.AreEqual(15, (double)boundaryCondition.FlowHqTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(40, (double)boundaryCondition.FlowHqTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(20, (double)boundaryCondition.FlowHqTable.Rows[3][0], 1.0e-6);
            Assert.AreEqual(90, (double)boundaryCondition.FlowHqTable.Rows[3][1], 1.0e-6);

            Assert.AreEqual(InterpolationType.Linear, boundaryCondition.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, boundaryCondition.ExtrapolationType);
        }

        [Test]
        public void TableLevelQhType4()
        {
            string text =
                @"FLBO  id '55' ty 0 h_ wd 4 0 0" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @" 18 77 < " + Environment.NewLine +
                @" 15 40 < " + Environment.NewLine +
                @" 20 90 <" + Environment.NewLine +
                @" 19 88 < " + Environment.NewLine +
                @"tble flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(text);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, boundaryCondition.StorageType);
            Assert.AreEqual(4, boundaryCondition.LevelQhTable.Rows.Count);
            Assert.AreEqual(18, (double)boundaryCondition.LevelQhTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(77, (double)boundaryCondition.LevelQhTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(19, (double)boundaryCondition.LevelQhTable.Rows[3][0], 1.0e-6);
            Assert.AreEqual(88, (double)boundaryCondition.LevelQhTable.Rows[3][1], 1.0e-6);
        }

        [Test]
        public void TableLevelQhType1()
        {
            string initialConditionsText =
                @"FLBO  id '55' ty 0 h_ wd 1 0 0" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @" 18 77 < " + Environment.NewLine +
                @" 15 40 < " + Environment.NewLine +
                @" 20 90 <" + Environment.NewLine +
                @" 19 88 < " + Environment.NewLine +
                @"tble flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, boundaryCondition.StorageType);
            Assert.AreEqual(4, boundaryCondition.LevelQhTable.Rows.Count);
            Assert.AreEqual(18, (double)boundaryCondition.LevelQhTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(77, (double)boundaryCondition.LevelQhTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(19, (double)boundaryCondition.LevelQhTable.Rows[3][0], 1.0e-6);
            Assert.AreEqual(88, (double)boundaryCondition.LevelQhTable.Rows[3][1], 1.0e-6);
        }


        /// <summary>
        /// Just a simpe test tp parse a record found in NetworkWithStructures\BOUNDARY.DAT that has an extra whitespace
        /// </summary>
        [Test]
        public void ParseFileWithExtraSpace()
        {
            const string initialConditionsText = @"FLBO id '27' ty 1 q_ dw 0  50 0 flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Constant, boundaryCondition.StorageType);
        }

        /// <summary>
        /// Undocumented field st found in 
        /// \SW_max_1.lit\3\BOUNDARY.DAT
        /// Field not defined in sobek online help
        /// </summary>
        [Test]
        public void ParseWithUnDocumentedField()
        {
            const string initialConditionsText = @"FLBO id 'benedenrand_ZwarteWater' st 0 ty 0 h_ wd 0  1.76 0 flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var boundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Constant, boundaryCondition.StorageType);
        }


        [Test]
        public void ReadBoundaryConditionQhWithInterpolationTypeConstant()
        {
            // PDIN ..pdin = period and interpolation method, 0 0 or 0 1 = interpolation continuous, 1 0 or 1 1 = interpolation block 
            string initialConditionsText = @"FLBO id '1' st 0 ty 1 q_ dw 4 0 0 PDIN 1 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @" 15 40 < " + Environment.NewLine +
                @" 18 77 < " + Environment.NewLine +
                @" 19 88 < " + Environment.NewLine +
                @" 20 90 <" + Environment.NewLine +
                @"tble flbo";
            var boundaryCondition = new SobekBoundaryConditionReader().GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, boundaryCondition.StorageType);
            // Qh should always be linear
            Assert.AreEqual(InterpolationType.Linear, boundaryCondition.InterpolationType);
        }

        [Test]
        public void ReadBoundaryConditionHQ()
        {
            string initialConditionsText = @"FLBO id '06' ty 0 se 0 h0 9.9999e+009 w0 0 q_ dw 0 9.9999e+009 9.9999e+009 h_ wd 4 9.9999e+009 9.9999e+009 'Waterlevel at Boundary' PDIN 0 0 '' pdin CLTT 'Q [m3/s]' 'H [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
            @"-2000 0.413 < " + Environment.NewLine +
            @"10 0.418 < " + Environment.NewLine +
            @"55 0.438 < " + Environment.NewLine +
            @"272 0.691 < " + Environment.NewLine +
            @"771 0.792 < " + Environment.NewLine +
            @"855 0.855 < " + Environment.NewLine +
            @"1382 1.254 < " + Environment.NewLine +
            @"1909 1.633 < " + Environment.NewLine +
            @"2437 1.98 < " + Environment.NewLine +
            @"3228 2.578 < " + Environment.NewLine +
            @"3700 2.898 < " + Environment.NewLine +
            @"4546 3.449 < " + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"qs dm 0 9.9999e+009 9.9999e+009 flbo";

            var boundaryCondition = new SobekBoundaryConditionReader().GetFlowBoundaryCondition(initialConditionsText);

            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, boundaryCondition.StorageType);
            Assert.AreEqual(InterpolationType.Linear, boundaryCondition.InterpolationType);

            Assert.AreEqual(12, boundaryCondition.LevelQhTable.Rows.Count);

        }






        [Test]
        [Category(TestCategory.Integration)]
        public void FileReaderTest()
        {
            string boundaryConditionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"NetworkWithStructures\BOUNDARY.DAT");
            var sobekBoundaryConditionReader = new SobekBoundaryConditionReader();

            var boundaryConditions = sobekBoundaryConditionReader.Read(boundaryConditionFile);

            // FLBO id '1' ty 0 h_ wt 1 0 0 PDIN 0 0  pdin
            // FLBO id '27' ty 1 q_ dw 0  50 0 flbo
            // FLBO id '10_34' ty 1 q_ dw 0  250 0 flbo
            // FLBO  id '55' ty 1 q_ dw 4 0 0
            // FLBO id '95' ty 0 h_ wd 0  0.5 0 flbo
            Assert.AreEqual(5, boundaryConditions.Count());
            Assert.AreEqual(3, boundaryConditions.Where(bc => bc.StorageType == SobekFlowBoundaryStorageType.Constant).Count());
            Assert.AreEqual(1, boundaryConditions.Where(bc => bc.StorageType == SobekFlowBoundaryStorageType.Variable).Count());
            Assert.AreEqual(1, boundaryConditions.Where(bc => bc.StorageType == SobekFlowBoundaryStorageType.Qh).Count());
            Assert.AreEqual(3, boundaryConditions.Where(bc => bc.BoundaryType == SobekFlowBoundaryConditionType.Flow).Count());
            Assert.AreEqual(2, boundaryConditions.Where(bc => bc.BoundaryType == SobekFlowBoundaryConditionType.Level).Count());
        }

        [Test]
        public void SobekReBoundaryQtWithExtraFields()
        {
            // ty 1 -> Flow
            // q_ dt 1 -> discharge as function of t
            var source =
                @"FLBO id 'P712' ty 1 se 0 h0 9.9999e+009 w0 0 q_ dt 1 9.9999e+009 9.9999e+009 'Discharge at Boundary' PDIN 0 0 '' pdin CLTT 'Time' 'Q [m3/s]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                @"'2002/12/15;00:00:00' 31.541 < " + Environment.NewLine +
                @"'2002/12/15;08:15:00' 31.541 < " + Environment.NewLine +
                @"'2003/02/02;07:30:00' 171.604 < " + Environment.NewLine +
                @"'2003/02/02;07:45:00' 170.534 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                 @"h_ wd 0 9.9999e+009 9.9999e+009 qs dm 0 9.9999e+009 9.9999e+009 flbo";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(source);

            Assert.AreEqual(SobekFlowBoundaryConditionType.Flow, sobekFlowBoundaryCondition.BoundaryType);
            Assert.IsNotNull(sobekFlowBoundaryCondition.FlowTimeTable);
            Assert.AreEqual(4, sobekFlowBoundaryCondition.FlowTimeTable.Rows.Count);
        }

        [Test]
        public void SobekReMultipleBoundariesGivenUseOneDefinedByTy()
        {
            // ty 0 -> Level
            // h_ wd 4 -> water level as function of Q
            string source =
                @"FLBO id 'P603' ty 0 se 0 h0 9.9999e+009 w0 0 q_ dw 0 9.9999e+009 9.9999e+009 h_ wd 4 9.9999e+009 9.9999e+009 'Water Level at Boundary' PDIN 0 0 '' pdin CLTT 'Q [m3/s]' 'H [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"571 5.5 < " + Environment.NewLine +
                @"629 5.95 < " + Environment.NewLine +
                @"722 6.4 < " + Environment.NewLine +
                @"16628 17 < " + Environment.NewLine +
                @"18681 17.5 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"";
            var boundaryConditions = new SobekBoundaryConditionReader();

            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(source);

            Assert.AreEqual(SobekFlowBoundaryConditionType.Level, sobekFlowBoundaryCondition.BoundaryType);
            Assert.AreEqual(SobekFlowBoundaryStorageType.Qh, sobekFlowBoundaryCondition.StorageType);
            Assert.IsNotNull(sobekFlowBoundaryCondition.LevelQhTable);
            Assert.AreEqual(5, sobekFlowBoundaryCondition.LevelQhTable.Rows.Count);
        }

    }
}
