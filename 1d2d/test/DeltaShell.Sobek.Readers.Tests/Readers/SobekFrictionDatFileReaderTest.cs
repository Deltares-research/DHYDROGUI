using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekFrictionDatFileReaderTest
    {
        /// <summary>
        /// parses file with:
        /// BDFR id '1' ci '1' mf 4 mt cp 0  .003 0 mr cp 0  .003 0 s1 6 s2 6 bdfr
        /// </summary>
        [Test]
        public void SimpleBedFrictionTest()
        {
            const string source = @"BDFR id '1' ci '1' mf 4 mt cp 0  .003 0 mr cp 0  .003 0 s1 6 s2 6 bdfr";
            var sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);

            //SobekFriction sobekFriction = SobekFrictionDatFileReader.ReadSobekFriction(TestHelper.GetTestDataDirectory() + @"\friction\SimpleBedFriction.dat");
            Assert.AreEqual(1, sobekFriction.SobekBedFrictionList.Count);
            SobekBedFriction sobekBedFriction = sobekFriction.SobekBedFrictionList[0];
            Assert.AreEqual(SobekFrictionFunctionType.Constant, sobekBedFriction.MainFriction.Positive.FunctionType);
            Assert.AreEqual(SobekBedFrictionType.WhiteColebrook, sobekBedFriction.MainFriction.FrictionType); // White-Colebrook
            Assert.AreEqual(0.003, sobekBedFriction.MainFriction.Positive.FrictionConst, 1.0e-6);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFrictionAdige()
        {
            // source: P:\sobek\maintenance\Jira\18001-19000\18131\DBS2SBK\Ars-7864\1\ADIGE.lit\6\
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"friction\friction.dat");
            string defFileText = File.ReadAllText(path, Encoding.Default);
            SobekFriction sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(defFileText);
            Assert.AreEqual(199, sobekFriction.CrossSectionFrictionList.Count);
            SobekCrossSectionFriction crossSectionFriction = sobekFriction.CrossSectionFrictionList[198];

            Assert.AreEqual("374", crossSectionFriction.CrossSectionID);
            Assert.AreEqual("374", crossSectionFriction.ID);
            Assert.AreEqual("Ponte Trento Centro", crossSectionFriction.Name);

            Assert.AreEqual(1, crossSectionFriction.Segments.Count);
            Assert.AreEqual(91.048, crossSectionFriction.Segments[0].Start, 1e-3);
            Assert.AreEqual(314.246, crossSectionFriction.Segments[0].End, 1e-3);
            Assert.AreEqual(RoughnessType.Manning, crossSectionFriction.Segments[0].FrictionType);
            Assert.AreEqual(0.03, crossSectionFriction.Segments[0].Friction, 1e-3);


            Assert.AreEqual(1, sobekFriction.SobekBedFrictionList.Count);
            SobekBedFriction sobekBedFriction = sobekFriction.SobekBedFrictionList[0];
            Assert.AreEqual("1", sobekBedFriction.Id);
            Assert.AreEqual("1", sobekBedFriction.BranchId);
            Assert.AreEqual(SobekBedFrictionType.Mannning /*1*/, sobekBedFriction.MainFriction.FrictionType);
        }

        [Test]
        public void SobekCrossSectionRelatedFrictionTest()
        {
            string source = @"CRFR id '55' nm 'Friction1' cs 'CsDef' 
                            lt ys 
                            TBLE 
                            0.0 3.0 <
                            3.0 5.0 < 
                            tble 
                            ft ys 
                            TBLE 7 20 < 
                            7 20 < 
                            tble 
                            fr ys 
                            TBLE 7 20 < 
                            7 20 < 
                            tble 
                            crfr";
            SobekFriction sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            var sobekCrossSectionFriction = sobekFriction.CrossSectionFrictionList.First();
            Assert.AreEqual("55", sobekCrossSectionFriction.ID);
            Assert.AreEqual("Friction1", sobekCrossSectionFriction.Name);
            Assert.AreEqual("CsDef", sobekCrossSectionFriction.CrossSectionID);
            Assert.AreEqual(2, sobekCrossSectionFriction.Segments.Count);
            Assert.AreEqual(RoughnessType.DeBosBijkerk, sobekCrossSectionFriction.Segments.FirstOrDefault().FrictionType);
        }

        [Test]
        public void SobekExtendedCharacterTest()
        {
            string source = @"CRFR id '282' nm 'Ponte FS Trento-Malè' cs '282' 
                                            lt ys
                                            TBLE
            	                                147.01005    247.1001    <
                                            tble
                                            ft ys
                                            TBLE
            	                                1    0.03    <
                                            tble
                                            fr ys
                                            TBLE
            	                                1    0.03    <
                                            tble
                                            crfr";
            const string pattern = @"id\s*'(?<Id>" + RegularExpression.Characters + ")'" +
                                   @"\snm\s'(?<bedFrictionDef>" + RegularExpression.ExtendedCharacters + ")'" +
                                   @"\scs\s'(?<CSDefenition>" + RegularExpression.Characters + ")'" +
                        @"[\s\r\n\t]*(lt ys[\s\r\n\t]*(?<yValuesSections>" + RegularExpression.CharactersAndQuote + @"))" +
                        @"[\s\r\n\t]*(ft ys[\s\r\n\t]*(?<frictionValues>" + RegularExpression.CharactersAndQuote + @"))fr ys";



            var regex = new Regex(pattern, RegexOptions.Singleline);
            var matches = regex.Matches(source);
            Assert.AreEqual(1, matches.Count);
        }

        /// <summary>
        /// Reads friction for river profile. A river profile is a profile of type 0 (tabulated) but with extra specification of
        /// main channel, floodplain 1 and floodplain 2.
        /// This test reads the friction of a cross section that has a miximum with of 120 and where the friction in the ui
        /// is given as:
        ///              flow width      type             value
        /// Main            60        Strickler(ks)        33
        /// FP1             20          Manning            66 
        /// FP2             40        White-Colebrook      99
        /// </summary>
        [Test]
        public void RiverProfileFrictionTest()
        {
            string source =
                // mf 3
                @"BDFR id '2' nm 'null' ci '2' em 1 er 0 e1 1 e2 0 e3 1 e4 0 mf 3 mt cp 0 33 9.9999e+009 'Chezy Coefficient' PDIN  0 0 '' pdin CLTT 'Column 1' cltt CLID '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"mr cp 0 33 9.9999e+009 'Chezy Coefficient' PDIN  0 0 '' pdin CLTT 'Column 1' cltt CLID '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                // s1 1
                @"s1 1 c1 cp 0 66 9.9999e+009 'Chezy Coefficient' PDIN  0 0 '' pdin CLTT 'Column 1' cltt CLID '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"r1 cp 0 66 9.9999e+009 'Chezy Coefficient' PDIN  0 0 '' pdin CLTT 'Column 1' cltt CLID '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                // s2 4
                @"s2 4 c2 cp 0 99 9.9999e+009 'Chezy Coefficient' PDIN  0 0 '' pdin CLTT 'Column 1' cltt CLID '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"r2 cp 0 99 9.9999e+009 'Chezy Coefficient' PDIN  0 0 '' pdin CLTT 'Column 1' cltt CLID '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"d9 f9 0  0.001 9.9999e+009 'D90 Grain Size' PDIN  0 0 '' pdin CLTT 'Location' 'D90' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"sf 3 st cp 0 33 0 sr cp 0 33 bdfr";
            SobekFriction sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            var sobekBedFriction = sobekFriction.SobekBedFrictionList.First();
            Assert.AreEqual("2", sobekBedFriction.Id);
            Assert.AreEqual(SobekBedFrictionType.StricklerKs/*3*/, sobekBedFriction.MainFriction.FrictionType);
            Assert.AreEqual(33, sobekBedFriction.MainFriction.Positive.FrictionConst);
            Assert.AreEqual(SobekBedFrictionType.Mannning /*1*/, sobekBedFriction.FloodPlain1Friction.FrictionType);
            Assert.AreEqual(66, sobekBedFriction.FloodPlain1Friction.Positive.FrictionConst);
            Assert.AreEqual(SobekBedFrictionType.WhiteColebrook /*4*/, sobekBedFriction.FloodPlain2Friction.FrictionType);
            Assert.AreEqual(99, sobekBedFriction.FloodPlain2Friction.Positive.FrictionConst);
        }


        [Test]
        public void SobekStructureRelatedFrictionTest()
        {
            string source =
                @"STFR id 'brug_1' ci 'brug_1' mf 3 mt cp 0 30 0 mr cp 0 30 0 s1 6 s2 6 sf 3 st cp 0 30 0 sr cp 0 30 stfr +
                STFR id 'brug_11' ci 'brug_11' mf 3 mt cp 0 30.000 30.000 mr cp 0 30.000 30.000 s1 6 s2 6 sf 3 st cp 0 30.000 30.000 sr cp 0 30.000 stfr
                STFR id '3' ci '3' mf 1 mt cp 0 0.042 0 mr cp 0 0.042 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 stfr
                STFR id '4' ci '4' mf 0 mt cp 0 42 0 mr cp 0 42 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 stfr";
            SobekFriction sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            var sobekStructureFriction = sobekFriction.StructureFrictionList.First();
            Assert.AreEqual("brug_1", sobekStructureFriction.ID);
            Assert.AreEqual(3, sobekStructureFriction.MainFrictionType);
            Assert.AreEqual(30, sobekStructureFriction.MainFrictionConst);
            var structure3 = sobekFriction.StructureFrictionList.First(br => br.ID == "3");
            Assert.AreEqual(1, structure3.MainFrictionType);
            Assert.AreEqual(0.042, structure3.MainFrictionConst);
        }

        [Test]
        public void SobekStructureWithNonConstantFrictionTypeTest()
        {
            string source =
                @"STFR id 'brug_1' ci 'brug_1' mf 3 mt fq 0 30 0 mr cp 0 30 0 s1 6 s2 6 sf 3 st cp 0 30 0 sr cp 0 30 stfr";
            SobekFriction sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            var sobekStructureFriction = sobekFriction.StructureFrictionList.First();
            Assert.AreEqual("brug_1", sobekStructureFriction.ID);
            Assert.AreEqual(3, sobekStructureFriction.MainFrictionType);
        }



        [Test]
        [Category(TestCategory.DataAccess)]
        public void SobekFrictionOfSW_MAXTest()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"SW_max_1.lit\3\FRICTION.DAT");
            string defFileText = File.ReadAllText(path);
            SobekFriction sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(defFileText);
            Assert.AreEqual(12, sobekFriction.SobekBedFrictionList.Count());
            Assert.AreEqual(2, sobekFriction.CrossSectionFrictionList.Count());
            Assert.AreEqual(43, sobekFriction.StructureFrictionList.Count());

            SobekBedFriction sobekBedFriction = sobekFriction.SobekBedFrictionList[1];
            Assert.AreEqual("stadsgrachten Zwolle Noord", sobekBedFriction.BranchId); //ci = branchId
        }

        [Test]
        public void Rijn301()
        {
            // main channel F(Q) Strickler KS=3 chainages '0' '26861.89539' '27410.09734' '37825.93433' '38374.13628' '61946.81999' '62495.02194'
            // flood plain1 F(Place) Strickler KS=3
            // flood plain2 F(Place) White Colebrook=4 (in UI SobekRe this is called Nikuradse)

            var source =
                // mf 3
                @"BDFR id 'MM1_1015' nm '(null)' ci 'MM1_6' em 1 er 0 e1 1 e2 0 e3 1 e4 0 mf 3 mt fq 4 35 9.9999e+009 'Coefficient Discharge' PDIN 0 0 '' pdin CLTT 'Q' '0' '26861.89539' '27410.09734' '37825.93433' '38374.13628' '61946.81999' '62495.02194' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"586 -52 46 46 43 43 43 43 < " + Environment.NewLine +
                @"776 46 44 44 41.5 42 42 42 < " + Environment.NewLine +
                @"1162 40 43 43 40.5 40.5 41 41 < " + Environment.NewLine +
                @"1700 33.5 43 43 39 39 42 42 < " + Environment.NewLine +
                @"2000 33.5 42 42 37.5 37.5 42 42 < " + Environment.NewLine +
                @"2400 31.5 41 41 37 37 43 43 < " + Environment.NewLine +
                @"2700 31.5 40 40 38 38 39 39 < " + Environment.NewLine +
                @"3000 30.5 39 39 36 36 39 39 < " + Environment.NewLine +
                @"3250 30.5 39.5 39.5 33 33 39 39 < " + Environment.NewLine +
                @"3500 30.5 40.5 40.5 34 34 38 38 < " + Environment.NewLine +
                @"4067 30 39 39 33 33 39 39.1 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" mr fq 4 35 9.9999e+009 'Coefficient Discharge' PDIN 0 0 '' pdin CLTT 'Q' '0' '26861.89539' '27410.09734' '37825.93433' '38374.13628' '61946.81999' '62495.02194' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"186 52 46 46 43 43 43 43 < " + Environment.NewLine +
                @"776 46 44 44 41.5 42 42 42 < " + Environment.NewLine +
                @"1162 40 43 43 40.5 40.5 41 41 < " + Environment.NewLine +
                @"1700 33.5 43 43 39 39 42 42 < " + Environment.NewLine +
                @"2000 33.5 42 42 37.5 37.5 42 42 < " + Environment.NewLine +
                @"2400 31.5 41 41 37 37 43 43 < " + Environment.NewLine +
                @"2700 31.5 40 40 38 38 39 39 < " + Environment.NewLine +
                @"3000 30.5 39 39 36 36 39 39 < " + Environment.NewLine +
                @"3250 30.5 39.5 39.5 33 33 39 39 < " + Environment.NewLine +
                @"3500 30.5 40.5 40.5 34 34 38 38 < " + Environment.NewLine +
                @"1067 30 39 39 33 33 39 19 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                // s1 3
                @" s1 3 c1 cp 2 35 9.9999e+009 'Strickler Ks Coefficient' PDIN 0 0 '' pdin CLTT 'Location' 'Coefficient' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 30 < " + Environment.NewLine +
                @"25000 35 < " + Environment.NewLine +
                @"65784 35.1 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" r1 cp 2 35 9.9999e+009 'Strickler Ks Coefficient' PDIN 0 0 '' pdin CLTT 'Location' 'Coefficient' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"10 130 < " + Environment.NewLine +
                @"25000 35 < " + Environment.NewLine +
                @"165784 135 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                // s2  4
                @" s2  4 c2 cp 2 9.9999e+009 9.9999e+009 'Nikuradse Coefficient' PDIN 1 0 '' pdin CLTT 'Location' 'Coefficient' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 10 < " + Environment.NewLine +
                @"29329 1 < " + Environment.NewLine +
                @"61125 5.2 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" r2 cp 2 9.9999e+009 9.9999e+009 'Nikuradse Coefficient' PDIN 1 0 '' pdin CLTT 'Location' 'Coefficient' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"10 110 < " + Environment.NewLine +
                @"29329 1 < " + Environment.NewLine +
                @"161125 15 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" d9 f9 0 9.9999e+009 9.9999e+009 bdfr";
            var sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            var sobekBedFriction = sobekFriction.SobekBedFrictionList.First();
            // as long as we do not support chainage based roughness expect converted to Chezy standard = 40
            Assert.AreEqual(SobekBedFrictionType.StricklerKs, sobekBedFriction.MainFriction.FrictionType);
            Assert.AreEqual(SobekFrictionFunctionType.FunctionOfQ, sobekBedFriction.MainFriction.Positive.FunctionType);
            Assert.IsNotNull(sobekBedFriction.MainFriction.Positive.QTable);
            Assert.AreEqual(8, sobekBedFriction.MainFriction.Positive.QTable.Columns.Count); // 1 for q, 7 for locations
            Assert.AreEqual(11, sobekBedFriction.MainFriction.Positive.QTable.Rows.Count);
            Assert.AreEqual(586.0, (double)sobekBedFriction.MainFriction.Positive.QTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(-52.0, (double)sobekBedFriction.MainFriction.Positive.QTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(4067.0, (double)sobekBedFriction.MainFriction.Positive.QTable.Rows[10][0], 1.0e-6);
            Assert.AreEqual(39.1, (double)sobekBedFriction.MainFriction.Positive.QTable.Rows[10][7], 1.0e-6);

            // added for negative direction
            Assert.AreEqual(SobekFrictionFunctionType.FunctionOfQ, sobekBedFriction.MainFriction.Negative.FunctionType);
            Assert.IsNotNull(sobekBedFriction.MainFriction.Negative.QTable);
            Assert.AreEqual(8, sobekBedFriction.MainFriction.Negative.QTable.Columns.Count); // 1 for q, 7 for locations
            Assert.AreEqual(11, sobekBedFriction.MainFriction.Negative.QTable.Rows.Count);
            Assert.AreEqual(186.0, (double)sobekBedFriction.MainFriction.Negative.QTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(52.0, (double)sobekBedFriction.MainFriction.Negative.QTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1067.0, (double)sobekBedFriction.MainFriction.Negative.QTable.Rows[10][0], 1.0e-6);
            Assert.AreEqual(19, (double)sobekBedFriction.MainFriction.Negative.QTable.Rows[10][7], 1.0e-6);

            Assert.AreEqual(SobekBedFrictionType.StricklerKs, sobekBedFriction.FloodPlain1Friction.FrictionType);
            Assert.AreEqual(SobekFrictionFunctionType.FunctionOfLocation, sobekBedFriction.FloodPlain1Friction.Positive.FunctionType);
            Assert.IsNotNull(sobekBedFriction.FloodPlain1Friction.Positive.LocationTable);
            Assert.AreEqual(2, sobekBedFriction.FloodPlain1Friction.Positive.LocationTable.Columns.Count); // 1 for q, 7 for locations
            Assert.AreEqual(3, sobekBedFriction.FloodPlain1Friction.Positive.LocationTable.Rows.Count);
            Assert.AreEqual(0.0, (double)sobekBedFriction.FloodPlain1Friction.Positive.LocationTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(30.0, (double)sobekBedFriction.FloodPlain1Friction.Positive.LocationTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(65784.0, (double)sobekBedFriction.FloodPlain1Friction.Positive.LocationTable.Rows[2][0], 1.0e-6);
            Assert.AreEqual(35.1, (double)sobekBedFriction.FloodPlain1Friction.Positive.LocationTable.Rows[2][1], 1.0e-6);

            // added for negative direction
            Assert.AreEqual(10.0, (double)sobekBedFriction.FloodPlain1Friction.Negative.LocationTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(130.0, (double)sobekBedFriction.FloodPlain1Friction.Negative.LocationTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(165784.0, (double)sobekBedFriction.FloodPlain1Friction.Negative.LocationTable.Rows[2][0], 1.0e-6);
            Assert.AreEqual(135, (double)sobekBedFriction.FloodPlain1Friction.Negative.LocationTable.Rows[2][1], 1.0e-6);

            Assert.AreEqual(SobekBedFrictionType.WhiteColebrook, sobekBedFriction.FloodPlain2Friction.FrictionType);
            Assert.AreEqual(SobekFrictionFunctionType.FunctionOfLocation, sobekBedFriction.FloodPlain2Friction.Positive.FunctionType);
            Assert.IsNotNull(sobekBedFriction.FloodPlain2Friction.Positive.LocationTable);
            Assert.AreEqual(2, sobekBedFriction.FloodPlain2Friction.Positive.LocationTable.Columns.Count); // 1 for q, 7 for locations
            Assert.AreEqual(3, sobekBedFriction.FloodPlain2Friction.Positive.LocationTable.Rows.Count);
            Assert.AreEqual(0.0, (double)sobekBedFriction.FloodPlain2Friction.Positive.LocationTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(10.0, (double)sobekBedFriction.FloodPlain2Friction.Positive.LocationTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(61125.0, (double)sobekBedFriction.FloodPlain2Friction.Positive.LocationTable.Rows[2][0], 1.0e-6);
            Assert.AreEqual(5.2, (double)sobekBedFriction.FloodPlain2Friction.Positive.LocationTable.Rows[2][1], 1.0e-6);

            // added for negative direction
            Assert.AreEqual(10.0, (double)sobekBedFriction.FloodPlain2Friction.Negative.LocationTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(110.0, (double)sobekBedFriction.FloodPlain2Friction.Negative.LocationTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(161125.0, (double)sobekBedFriction.FloodPlain2Friction.Negative.LocationTable.Rows[2][0], 1.0e-6);
            Assert.AreEqual(15.0, (double)sobekBedFriction.FloodPlain2Friction.Negative.LocationTable.Rows[2][1], 1.0e-6);
        }

        [Test]
        public void ReadGlobalFriction()
        {
            var source =
                @"GLFR st 0 dd 1.65 s1 0.0474 s2 0.55 p1 -6 p2 2.75 p3 5.5 p4 4.125 p5 -0.2 p6 2.447 a1 0.0005 a2 6e-005 ra 1 BDFR id '0' ci '0' mf 3 mt cp 0 30 0 mr cp 0 30 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 bdfr          glfr" +
                Environment.NewLine +
                @"BDFR id 'stadsgrachten Zwolle Noord' ci 'stadsgrachten Zwolle Noord' mf 3 mt cp 0 30 0 mr cp 0 30 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003          bdfr" +
                Environment.NewLine +
                @"tble crfr";
            var sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            Assert.IsNotNull(sobekFriction.GlobalBedFrictionList.First());
            // mf 3 = main is stricklerKS
            // mt cp 0 30 0 = friction positive constant
            // mr cp 0 30 0 = friction negative constant 
            // s1 6 s2 6 = floodplain1 = floodplain2 = main
            // sf 4 = ground layer is White Colebrook
            // st cp 0 0.003 0 = friction positive constant
            // sr cp 0 0.003 = friction negative constant
            Assert.AreEqual(SobekBedFrictionType.StricklerKs, sobekFriction.GlobalBedFrictionList.First().MainFriction.FrictionType);
            Assert.AreEqual(30.0, sobekFriction.GlobalBedFrictionList.First().MainFriction.Positive.FrictionConst, 1.0e-6);
        }

        [Test]
        public void ReadSobekReGlobalFrictionRecord()
        {
            var source =
                @"GLFR dd 1.65 s1 0.0474 s2 0.55 p1 -6 p2 2.75 p3 5.5 p4 4.125 p5 -0.2 p6 2.447 a1 0.00015 a2 1.5e-005 ra 1.29 BDFR id '1475' nm '(null)' ci '-1' em 0 er 0 e1 0 e2 0 e3 0 e4 0 mf 0 mt cp 0 45 9.9999e+009 mr cp 0 45 9.9999e+009 s1 4 c1 cp 0 0.25 9.9999e+009 r1 cp 0 0.25 9.9999e+009 s2  6 c2 cp 0 9.9999e+009 9.9999e+009 r2 cp 0 9.9999e+009 9.9999e+009 d9 f9 0 9.9999e+009 9.9999e+009 bdfr" +
                Environment.NewLine +
                @"glfr";
            var sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            Assert.IsNotNull(sobekFriction.GlobalBedFrictionList.First());
            Assert.AreEqual(SobekBedFrictionType.Chezy, sobekFriction.GlobalBedFrictionList.First().MainFriction.FrictionType);
            Assert.AreEqual(45.0, sobekFriction.GlobalBedFrictionList.First().MainFriction.Positive.FrictionConst, 1.0e-6);
        }

        [Test]
        public void ReadSobekFrictionTestCase254bInterpolation()
        {
            var source =
            @"GLFR  st 0 dd 1.65 s1 0.0474 s2 0.55 p1 -6 p2 2.75 p3 5.5 p4 -4.125 p5 -0.2 p6 2.447 a1 0.0005 a2 6e-005 ra 1 BDFR id '0' nm 'null' ci '0' em 1 er 0 e1 1 e2 0 e3 1 e4 0 mf 0 mt cp 0 55" + Environment.NewLine +
            @"mr cp 0 55" + Environment.NewLine +
            @"s1 0 c1 cp 0 35" + Environment.NewLine +
            @"r1 cp 0 35" + Environment.NewLine +
            @"s2 0 c2 cp 0 20" + Environment.NewLine +
            @"r2 cp 0 20" + Environment.NewLine +
            @"d9 f9 0  0.001 PDIN 0 0 '' pdin TBLE" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @" sf 0 st cp 0 55 0 sr cp 0 55 bdfr glfr" + Environment.NewLine +
            @"BDFR id '1' nm 'null' ci '1' em 1 er 0 e1 1 e2 0 e3 1 e4 0 mf 4 mt cp 1 0.2 PDIN 1 0 '' pdin TBLE " +
            Environment.NewLine +
            @"0 0.2 < " + Environment.NewLine +
            @"2000 0.3 < " + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"mr cp 1 0.2 PDIN 1 0 '' pdin TBLE" + Environment.NewLine +
            @"0 0.2 <" + Environment.NewLine +
            @"2000 0.3 <" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"s1 4 c1 fh 3 3 PDIN 1 0 '' pdin CLTT 'h' '0' '2000' cltt TBLE" + Environment.NewLine +
            @"0 0.2 0.205 < " + Environment.NewLine +
            @"0.5 0.22 0.2255 < " + Environment.NewLine +
            @"1 0.18 0.1845 < " + Environment.NewLine +
            @"2 0.3 0.3075 < " + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"r1 fh 3 3 PDIN 1 0 '' pdin CLTT 'h' '0' '2000' cltt TBLE" + Environment.NewLine +
            @"0 0.2 0.205 < " + Environment.NewLine +
            @"0.5 0.22 0.2255 < " + Environment.NewLine +
            @"1 0.18 0.1845 < " + Environment.NewLine +
            @"2 0.3 0.3075 < " + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"s2 4 c2 fq 3 3 PDIN 0 0 '' pdin CLTT 'h' '0' '2000' cltt TBLE" + Environment.NewLine +
            @"75 0.2 0.205 <" + Environment.NewLine +
            @"870 0.22 0.2255 <" + Environment.NewLine +
            @"1500 0.18 0.1845 <" + Environment.NewLine +
            @"2500 0.3 0.3075 <" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"r2 fq 3 3 PDIN 1 0 '' pdin CLTT 'h' '0' '2000' cltt TBLE" + Environment.NewLine +
            @"75 0.2 0.205 < " + Environment.NewLine +
            @"870 0.22 0.2255 < " + Environment.NewLine +
            @"1500 0.18 0.1845 < " + Environment.NewLine +
            @"2500 0.3 0.3075 < " + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"d9 f9 0  0.0007 PDIN 0 0 '' pdin TBLE" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @"sf 1 st cp 0 1234 0 sr cp 0 1234 bdfr";

            var sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(source);
            var sobekBedFriction = sobekFriction.SobekBedFrictionList.LastOrDefault();

            Assert.IsNotNull(sobekBedFriction);
            Assert.IsNotNull(sobekBedFriction.MainFriction);
            Assert.IsNotNull(sobekBedFriction.FloodPlain1Friction);
            Assert.IsNotNull(sobekBedFriction.FloodPlain2Friction);

            Assert.AreEqual(InterpolationType.Constant, sobekBedFriction.MainFriction.Positive.Interpolation);
            Assert.AreEqual(InterpolationType.Constant, sobekBedFriction.FloodPlain1Friction.Positive.Interpolation);
            Assert.AreEqual(InterpolationType.Linear, sobekBedFriction.FloodPlain2Friction.Positive.Interpolation);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadMultipleFrictionDefinitionsFromFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"friction\friction.dat");

            var sobekFriction = new SobekFrictionDatFileReader().ReadSobekFriction(path);

            Assert.AreEqual(199, sobekFriction.CrossSectionFrictionList.Count);
            SobekCrossSectionFriction crossSectionFriction = sobekFriction.CrossSectionFrictionList[2];

            // read the third cross section friction record from the file:
            //
            // * record open field, id of friction record, name, id of cross section
            // CRFR id '178' nm 'Ponte di Bailey' cs '178'
            // * table with y values of segments: 1 segment from 65.3426 to 185.5862
            // lt ys
            // TBLE
            //     65.3426    185.5862    <
            // tble
            // ft ys
            // TBLE
            // * table with positive friction values 1 segment type 1 = Manning, value = 0.03
            //     1    0.03    <
            // tble
            // fr ys
            // * table with negative friction values 1 segment type 1 = Manning, value = 0.03
            // TBLE
            //     1    0.03    <
            // tble
            // * close record field
            // crfr

            Assert.AreEqual("178", crossSectionFriction.CrossSectionID);
            Assert.AreEqual("178", crossSectionFriction.ID);
            Assert.AreEqual("Ponte di Bailey", crossSectionFriction.Name);

            Assert.AreEqual(1, crossSectionFriction.Segments.Count);
            Assert.AreEqual(65.3426, crossSectionFriction.Segments[0].Start, 1e-3);
            Assert.AreEqual(185.5862, crossSectionFriction.Segments[0].End, 1e-3);
            Assert.AreEqual(RoughnessType.Manning, crossSectionFriction.Segments[0].FrictionType);
            Assert.AreEqual(0.03, crossSectionFriction.Segments[0].Friction, 1e-3);

            Assert.AreEqual(1, sobekFriction.SobekBedFrictionList.Count);
            SobekBedFriction sobekBedFriction = sobekFriction.SobekBedFrictionList[0];
            Assert.AreEqual("1", sobekBedFriction.Id);
            Assert.AreEqual("1", sobekBedFriction.BranchId);
            Assert.AreEqual(SobekBedFrictionType.Mannning /*1*/, sobekBedFriction.MainFriction.FrictionType);
        }

        [Test]
        public void GetSobekFriction_ForUnsupportedExtraResistance_LogsWarning()
        {
            // Setup
            string text = string.Join(Environment.NewLine,
                                      "XRST id '5' nm '' ty 0 rt rs",
                                      "TBLE",
                                      "0 .1 <",
                                      "1 5 <",
                                      "tble xrst");

            // Call
            void Call() => SobekFrictionDatFileReader.GetSobekFriction(text);

            // Assert
            string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();
            Assert.That(warnings, Has.Length.EqualTo(1));
            Assert.That(warnings[0], Is.EqualTo("The extra resistance functionality is not supported, skipping this item with id: 5"));
        }
    }
}

