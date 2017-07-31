using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests;
using NUnit.Framework;
using SobekCompare.Tests.Helpers;

namespace SobekCompare.Tests
{
    [TestFixture]
    [Category(TestCategorySobekValidation.WaterFlow1D)]
    public class SOBEKTestBench
    {
        private static string TestDataDir; 
        private static string TestCasesDirectory;
        private static IDictionary<int, string> TestDictionary;

        private const string TestCasesDirectoryName = "testbench\\testcases";
        private static ModelRunnerAndResultComparer modelRunnerAndResultComparer;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            //set the testcases dir
            TestDataDir = TestHelper.GetDataDir(); 
            TestCasesDirectory = Path.Combine(TestDataDir, TestCasesDirectoryName);

            //create a lookup of nr->testdirectory which we can use in the separate tests.
            TestDictionary = SobekTestBenchHelper.GetTestsDictionary(TestCasesDirectory);

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        private static void RunTest(int testNumber)
        {
            var info = SobekTestBenchHelper.GetTestInfo(testNumber, TestCasesDirectory, TestDictionary);
            if (info != null)
            {
                modelRunnerAndResultComparer = new ModelRunnerAndResultComparer(info.CaseDirectory);
                modelRunnerAndResultComparer.RunModels();

                // Compare the *.his file with the original reference *.his file within the SOBEK212 testbench using TCL
                // Specific parameters on specific locations are being compared using the sobek.cnf file that comes with the testcase
                var error = modelRunnerAndResultComparer.CompareDeltaShellHisWithSobek212His(info.Name, TestCasesDirectory, info.TestDirectory, info.CaseDirectory);

                if (!String.IsNullOrEmpty(error)) Assert.Fail(error);

                // Convert the *.his file from sobeksim to NetCDF and compare with the NetCDF file generated within DeltaShell
                // All parameters on all locations are compared to make sure ModelAPI -> NetCDF interface is correct.
                //Assert.IsTrue(modelRunnerAndResultComparer.IsWaterLevelOK, testDirectory + " " + modelRunnerAndResultComparer.WaterLevelReport);
                //Assert.IsTrue(modelRunnerAndResultComparer.IsWaterDepthOK, testDirectory + " " + modelRunnerAndResultComparer.WaterDepthReport);
                //Assert.IsTrue(modelRunnerAndResultComparer.IsWaterFlowOK, testDirectory + " " + modelRunnerAndResultComparer.WaterFlowReport);
                //Assert.IsTrue(modelRunnerAndResultComparer.IsWaterVelocityOK, testDirectory + " " + modelRunnerAndResultComparer.WaterVelocityReport);
            }
        }

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [TearDown]
        public void TearDown()
        {
            if (modelRunnerAndResultComparer != null)
            {
                modelRunnerAndResultComparer.Dispose();
            }
        }

       
         #region Generated tests code..replace with nunit parameter if environment is ready for it (resharper+teamcity)

        [Test]
        public void RunTest001()
        {
            RunTest(1);
        }
        [Test]
        public void RunTest002()
        {
            RunTest(2);
        }
        [Test]
        public void RunTest003()
        {
            RunTest(3);
        }
        [Test]
        public void RunTest004()
        {
            RunTest(4);
        }
        [Test]
        public void RunTest005()
        {
            RunTest(5);
        }
        [Test]
        public void RunTest006()
        {
            RunTest(6);
        }
        [Test]
        public void RunTest007()
        {
            RunTest(7);
        }
        [Test]
        public void RunTest008()
        {
            RunTest(8);
        }
        [Test]
        public void RunTest009()
        {
            RunTest(9);
        }
        [Test]
        public void RunTest010()
        {
            RunTest(10);
        }
        [Test]
        public void RunTest011()
        {
            RunTest(11);
        }
        [Test]
        public void RunTest012()
        {
            RunTest(12);
        }
        [Test]
        public void RunTest013()
        {
            RunTest(13);
        }
        [Test]
        public void RunTest014()
        {
            RunTest(14);
        }
        [Test]
        public void RunTest015()
        {
            RunTest(15);
        }
        [Test]
        public void RunTest016()
        {
            RunTest(16);
        }
        [Test]
        public void RunTest017()
        {
            RunTest(17);
        }
        [Test]
        public void RunTest018()
        {
            RunTest(18);
        }
        [Test]
        public void RunTest019()
        {
            RunTest(19);
        }
        [Test]
        public void RunTest020()
        {
            RunTest(20);
        }
        [Test]
        public void RunTest021()
        {
            RunTest(21);
        }
        [Test]
        public void RunTest022()
        {
            RunTest(22);
        }
        [Test]
        public void RunTest023()
        {
            RunTest(23);
        }
        [Test]
        public void RunTest024()
        {
            RunTest(24);
        }
        [Test]
        public void RunTest025()
        {
            RunTest(25);
        }
        [Test]
        public void RunTest026()
        {
            RunTest(26);
        }
        [Test]
        public void RunTest027()
        {
            RunTest(27);
        }
        [Test]
        public void RunTest028()
        {
            RunTest(28);
        }
        [Test]
        public void RunTest029()
        {
            RunTest(29);
        }
        [Test]
        public void RunTest030()
        {
            RunTest(30);
        }
        [Test]
        public void RunTest031()
        {
            RunTest(31);
        }
        [Test]
        public void RunTest032()
        {
            RunTest(32);
        }
        [Test]
        public void RunTest033()
        {
            RunTest(33);
        }
        [Test]
        public void RunTest034()
        {
            RunTest(34);
        }
        [Test]
        public void RunTest035()
        {
            RunTest(35);
        }
        [Test]
        public void RunTest036()
        {
            RunTest(36);
        }
        [Test]
        public void RunTest037()
        {
            RunTest(37);
        }
        [Test]
        public void RunTest038()
        {
            RunTest(38);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest039()
        {
            RunTest(39);
        }
        [Test]
        public void RunTest040()
        {
            RunTest(40);
        }
        [Test]
        public void RunTest041()
        {
            RunTest(41);
        }
        [Test]
        public void RunTest042()
        {
            RunTest(42);
        }
        [Test]
        public void RunTest043()
        {
            RunTest(43);
        }
        [Test]
        public void RunTest044()
        {
            RunTest(44);
        }
        [Test]
        public void RunTest045()
        {
            RunTest(45);
        }
        [Test]
        public void RunTest046()
        {
            RunTest(46);
        }
        [Test]
        public void RunTest047()
        {
            RunTest(47);
        }
        [Test]
        public void RunTest048()
        {
            RunTest(48);
        }
        [Test]
        public void RunTest049()
        {
            RunTest(49);
        }
        [Test]
        public void RunTest050()
        {
            RunTest(50);
        }
        [Test]
        public void RunTest051()
        {
            RunTest(51);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest052()
        {
            RunTest(52);
        }
        [Test]
        public void RunTest053()
        {
            RunTest(53);
        }
        [Test]
        public void RunTest054()
        {
            RunTest(54);
        }
        [Test]
        public void RunTest055()
        {
            RunTest(55);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest056()
        {
            RunTest(56);
        }
        [Test]
        public void RunTest057()
        {
            RunTest(57);
        }
        [Test]
        public void RunTest058()
        {
            RunTest(58);
        }
        [Test]
        public void RunTest059()
        {
            RunTest(59);
        }
        [Test]
        public void RunTest060()
        {
            RunTest(60);
        }
        [Test]
        public void RunTest061()
        {
            RunTest(61);
        }
        [Test]
        public void RunTest062()
        {
            RunTest(62);
        }
        [Test]
        public void RunTest063()
        {
            RunTest(63);
        }
        [Test]
        public void RunTest064()
        {
            RunTest(64);
        }
        [Test]
        public void RunTest065()
        {
            RunTest(65);
        }
        [Test]
        public void RunTest066()
        {
            RunTest(66);
        }
        [Test]
        public void RunTest067()
        {
            RunTest(67);
        }
        [Test]
        public void RunTest068()
        {
            RunTest(68);
        }
        [Test]
        public void RunTest069()
        {
            RunTest(69);
        }
        [Test]
        public void RunTest070()
        {
            RunTest(70);
        }
        [Test]
        public void RunTest071()
        {
            RunTest(71);
        }
        [Test]
        public void RunTest072()
        {
            RunTest(72);
        }
        [Test]
        public void RunTest073()
        {
            RunTest(73);
        }
        [Test]
        public void RunTest074()
        {
            RunTest(74);
        }
        [Test]
        public void RunTest075()
        {
            RunTest(75);
        }
        [Test]
        public void RunTest076()
        {
            RunTest(76);
        }
        [Test]
        public void RunTest077()
        {
            RunTest(77);
        }
        [Test]
        public void RunTest078()
        {
            RunTest(78);
        }
        [Test]
        public void RunTest079()
        {
            RunTest(79);
        }
        [Test]
        public void RunTest080()
        {
            RunTest(80);
        }
        [Test]
        public void RunTest081()
        {
            RunTest(81);
        }
        [Test]
        public void RunTest082()
        {
            RunTest(82);
        }
        [Test]
        public void RunTest083()
        {
            RunTest(83);
        }
        [Test]
        public void RunTest084()
        {
            RunTest(84);
        }
        [Test]
        public void RunTest085()
        {
            RunTest(85);
        }
        [Test]
        public void RunTest086()
        {
            RunTest(86);
        }
        [Test]
        public void RunTest087()
        {
            RunTest(87);
        }
        [Test]
        public void RunTest088()
        {
            RunTest(88);
        }
        [Test]
        public void RunTest089()
        {
            RunTest(89);
        }
        [Test]
        public void RunTest090()
        {
            RunTest(90);
        }
        [Test]
        public void RunTest091()
        {
            RunTest(91);
        }
        [Test]
        public void RunTest092()
        {
            RunTest(92);
        }
        [Test]
        public void RunTest093()
        {
            RunTest(93);
        }
        [Test]
        public void RunTest094()
        {
            RunTest(94);
        }
        [Test]
        public void RunTest095()
        {
            RunTest(95);
        }
        [Test]
        public void RunTest096()
        {
            RunTest(96);
        }
        [Test]
        public void RunTest097()
        {
            RunTest(97);
        }
        [Test]
        public void RunTest098()
        {
            RunTest(98);
        }
        [Test]
        public void RunTest099()
        {
            RunTest(99);
        }
        [Test]
        public void RunTest100()
        {
            RunTest(100);
        }
        [Test]
        public void RunTest101()
        {
            RunTest(101);
        }
        [Test]
        public void RunTest102()
        {
            RunTest(102);
        }
        [Test]
        public void RunTest103()
        {
            RunTest(103);
        }
        [Test]
        public void RunTest104()
        {
            RunTest(104);
        }
        [Test]
        public void RunTest105()
        {
            RunTest(105);
        }
        [Test]
        public void RunTest()
        {
            RunTest(106);
        }
        [Test]
        public void RunTest107()
        {
            RunTest(107);
        }
        [Test]
        public void RunTest108()
        {
            RunTest(108);
        }
        [Test]
        public void RunTest109()
        {
            RunTest(109);
        }
        [Test]
        public void RunTest110()
        {
            RunTest(110);
        }
        [Test]
        public void RunTest111()
        {
            RunTest(111);
        }
        [Test]
        // Is tested in DIMR-Testbench, may be return as import test only
        [Category(TestCategory.WorkInProgress)]
        public void RunTest112()
        {
            RunTest(112);
        }
        [Test]
        public void RunTest113()
        {
            RunTest(113);
        }
        [Test]
        public void RunTest114()
        {
            RunTest(114);
        }
        [Test]
        public void RunTest115()
        {
            RunTest(115);
        }
        [Test]
        // Is tested in DIMR-Testbench, may be return as import test only
        [Category(TestCategory.WorkInProgress)]
        public void RunTest116()
        {
            RunTest(116);
        }
        [Test]
        public void RunTest117()
        {
            RunTest(117);
        }
        [Test]
        public void RunTest118()
        {
            RunTest(118);
        }
        [Test]
        public void RunTest119()
        {
            RunTest(119);
        }
        [Test]
        public void RunTest120()
        {
            RunTest(120);
        }
        [Test]
        public void RunTest121()
        {
            RunTest(121);
        }
        [Test]
        public void RunTest122()
        {
            RunTest(122);
        }
        [Test]
        public void RunTest123()
        {
            RunTest(123);
        }
        [Test]
        public void RunTest124()
        {
            RunTest(124);
        }
        [Test]
        public void RunTest125()
        {
            RunTest(125);
        }
        [Test]
        public void RunTest126()
        {
            RunTest(126);
        }
        [Test]
        public void RunTest127()
        {
            RunTest(127);
        }
        [Test]
        public void RunTest128()
        {
            RunTest(128);
        }
        [Test]
        public void RunTest129()
        {
            RunTest(129);
        }
        [Test]
        public void RunTest130()
        {
            RunTest(130);
        }
        [Test]
        public void RunTest131()
        {
            RunTest(131);
        }
        [Test]
        public void RunTest132()
        {
            RunTest(132);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest133()
        {
            RunTest(133);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest134()
        {
            RunTest(134);
        }
        [Test]
        public void RunTest135()
        {
            RunTest(135);
        }
        [Test]
        public void RunTest136()
        {
            RunTest(136);
        }
        [Test]
        public void RunTest137()
        {
            RunTest(137);
        }
        [Test]
        public void RunTest138()
        {
            RunTest(138);
        }
        [Test]
        public void RunTest139()
        {
            RunTest(139);
        }
        [Test]
        public void RunTest140()
        {
            RunTest(140);
        }
        [Test]
        public void RunTest141()
        {
            RunTest(141);
        }
        [Test]
        public void RunTest142()
        {
            RunTest(142);
        }
        [Test]
        public void RunTest143()
        {
            RunTest(143);
        }
        [Test]
        public void RunTest144()
        {
            RunTest(144);
        }
        [Test]
        public void RunTest145()
        {
            RunTest(145);
        }
        [Test]
        public void RunTest146()
        {
            RunTest(146);
        }
        [Test]
        public void RunTest147()
        {
            RunTest(147);
        }
        [Test]
        public void RunTest148()
        {
            RunTest(148);
        }
        [Test]
        public void RunTest149()
        {
            RunTest(149);
        }
        [Test]
        public void RunTest150()
        {
            RunTest(150);
        }
        [Test]
        public void RunTest151()
        {
            RunTest(151);
        }
        [Test]
        public void RunTest152()
        {
            RunTest(152);
        }
        [Test]
        public void RunTest153()
        {
            RunTest(153);
        }
        [Test]
        public void RunTest154()
        {
            RunTest(154);
        }
        [Test]
        public void RunTest155()
        {
            RunTest(155);
        }
        [Test]
        public void RunTest156()
        {
            RunTest(156);
        }
        [Test]
        public void RunTest157()
        {
            RunTest(157);
        }
        [Test]
        public void RunTest158()
        {
            RunTest(158);
        }
        [Test]
        public void RunTest159()
        {
            RunTest(159);
        }
        [Test]
        public void RunTest160()
        {
            RunTest(160);
        }
        [Test]
        public void RunTest161()
        {
            RunTest(161);
        }
        [Test]
        public void RunTest162()
        {
            RunTest(162);
        }
        [Test]
        public void RunTest163()
        {
            RunTest(163);
        }
        [Test]
        public void RunTest164()
        {
            RunTest(164);
        }
        [Test]
        public void RunTest165()
        {
            RunTest(165);
        }
        [Test]
        public void RunTest166()
        {
            RunTest(166);
        }
        [Test]
        public void RunTest167()
        {
            RunTest(167);
        }
        [Test]
        public void RunTest168()
        {
            RunTest(168);
        }
        [Test]
        public void RunTest169()
        {
            RunTest(169);
        }
        [Test]
        public void RunTest170()
        {
            RunTest(170);
        }
        [Test]
        public void RunTest171()
        {
            RunTest(171);
        }
        [Test]
        public void RunTest172()
        {
            RunTest(172);
        }
        [Test]
        public void RunTest173()
        {
            RunTest(173);
        }
        [Test]
        public void RunTest174()
        {
            RunTest(174);
        }
        [Test]
        public void RunTest175()
        {
            RunTest(175);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest176()
        {
            RunTest(176);
        }
        [Test]
        public void RunTest177()
        {
            RunTest(177);
        }
        [Test]
        public void RunTest178()
        {
            RunTest(178);
        }
        [Test]
        public void RunTest179()
        {
            RunTest(179);
        }
        [Test]
        public void RunTest180()
        {
            RunTest(180);
        }
        [Test]
        public void RunTest181()
        {
            RunTest(181);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest182()
        {
            RunTest(182);
        }
        [Test]
        public void RunTest183()
        {
            RunTest(183);
        }
        [Test]
        public void RunTest184()
        {
            RunTest(184);
        }
        [Test]
        public void RunTest185()
        {
            RunTest(185);
        }
        [Test]
        public void RunTest186()
        {
            RunTest(186);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest187()
        {
            RunTest(187);
        }
        [Test]
        public void RunTest188()
        {
            RunTest(188);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest189()
        {
            RunTest(189);
        }
        [Test]
        public void RunTest190()
        {
            RunTest(190);
        }
        [Test]
        public void RunTest191()
        {
            RunTest(191);
        }
        [Test]
        public void RunTest192()
        {
            RunTest(192);
        }
        [Test]
        public void RunTest193()
        {
            RunTest(193);
        }
        [Test]
        public void RunTest194()
        {
            RunTest(194);
        }
        [Test]
        public void RunTest195()
        {
            RunTest(195);
        }
        [Test]
        public void RunTest196()
        {
            RunTest(196);
        }
        [Test]
        public void RunTest197()
        {
            RunTest(197);
        }
        [Test]
        public void RunTest198()
        {
            RunTest(198);
        }
        [Test]
        public void RunTest199()
        {
            RunTest(199);
        }
        [Test]
        public void RunTest200()
        {
            RunTest(200);
        }
        [Test]
        public void RunTest201()
        {
            RunTest(201);
        }
        [Test]
        public void RunTest202()
        {
            RunTest(202);
        }
        [Test]
        public void RunTest203()
        {
            RunTest(203);
        }
        [Test]
        public void RunTest204()
        {
            RunTest(204);
        }
        [Test]
        public void RunTest205()
        {
            RunTest(205);
        }
        [Test]
        public void RunTest206()
        {
            RunTest(206);
        }
        [Test]
        public void RunTest207()
        {
            RunTest(207);
        }
        [Test]
        public void RunTest208()
        {
            RunTest(208);
        }
        [Test]
        public void RunTest209()
        {
            RunTest(209);
        }
        [Test]
        public void RunTest210()
        {
            RunTest(210);
        }
        [Test]
        public void RunTest211()
        {
            RunTest(211);
        }
        [Test]
        public void RunTest212()
        {
            RunTest(212);
        }
        [Test]
        public void RunTest213()
        {
            RunTest(213);
        }
        [Test]
        public void RunTest214()
        {
            RunTest(214);
        }
        [Test]
        public void RunTest215()
        {
            RunTest(215);
        }
        [Test]
        public void RunTest216()
        {
            RunTest(216);
        }
        [Test]
        public void RunTest217()
        {
            RunTest(217);
        }
        [Test]
        public void RunTest218()
        {
            RunTest(218);
        }
        [Test]
        public void RunTest219()
        {
            RunTest(219);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest220()
        {
            RunTest(220);
        }
        [Test]
        // Work in Progress until issue SOBEK-50139 has been solved
        [Category(TestCategory.WorkInProgress)]
        public void RunTest221()
        {
            RunTest(221);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest222()
        {
            RunTest(222);
        }
        [Test]
        public void RunTest223()
        {
            RunTest(223);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest224()
        {
            RunTest(224);
        }
        [Test]
        public void RunTest225()
        {
            RunTest(225);
        }
        [Test]
        public void RunTest226()
        {
            RunTest(226);
        }
        [Test]
        public void RunTest227()
        {
            RunTest(227);
        }
        [Test]
        public void RunTest228()
        {
            RunTest(228);
        }
        [Test]
        public void RunTest229()
        {
            RunTest(229);
        }
        [Test]
        public void RunTest230()
        {
            RunTest(230);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest231()
        {
            RunTest(231);
        }
        [Test]
        public void RunTest232()
        {
            RunTest(232);
        }
        [Test]
        public void RunTest233()
        {
            RunTest(233);
        }
        [Test]
        public void RunTest234()
        {
            RunTest(234);
        }
        [Test]
        public void RunTest235()
        {
            RunTest(235);
        }
        [Test]
        public void RunTest236()
        {
            RunTest(236);
        }
        [Test]
        public void RunTest237()
        {
            RunTest(237);
        }
        [Test]
        public void RunTest238()
        {
            RunTest(238);
        }
        [Test]
        public void RunTest239()
        {
            RunTest(239);
        }
        [Test]
        public void RunTest240()
        {
            RunTest(240);
        }
        [Test]
        public void RunTest241()
        {
            RunTest(241);
        }
        [Test]
        public void RunTest242()
        {
            RunTest(242);
        }
        [Test]
        public void RunTest243()
        {
            RunTest(243);
        }
        [Test]
        public void RunTest244()
        {
            RunTest(244);
        }
        [Test]
        public void RunTest245()
        {
            RunTest(245);
        }
        [Test]
        public void RunTest246()
        {
            RunTest(246);
        }
        [Test]
        public void RunTest247()
        {
            RunTest(247);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest248()
        {
            RunTest(248);
        }
        [Test]
        public void RunTest249()
        {
            RunTest(249);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest250()
        {
            RunTest(250);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest251()
        {
            RunTest(251);
        }
        [Test]
        public void RunTest252()
        {
            RunTest(252);
        }
        [Test]
        public void RunTest253()
        {
            RunTest(253);
        }
        [Test]
        public void RunTest254()
        {
            RunTest(254);
        }
        [Test]
        public void RunTest255()
        {
            RunTest(255);
        }
        [Test]
        public void RunTest256()
        {
            RunTest(256);
        }
        [Test]
        public void RunTest257()
        {
            RunTest(257);
        }
        [Test]
        public void RunTest258()
        {
            RunTest(258);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest259()
        {
            RunTest(259);
        }
        [Test]
        public void RunTest260()
        {
            RunTest(260);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest261()
        {
            RunTest(261);
        }
        [Test]
        public void RunTest262()
        {
            RunTest(262);
        }
        [Test]
        public void RunTest263()
        {
            RunTest(263);
        }
        [Test]
        public void RunTest264()
        {
            RunTest(264);
        }
        [Test]
        public void RunTest265()
        {
            RunTest(265);
        }
        [Test]
        public void RunTest266()
        {
            RunTest(266);
        }
        [Test]
        public void RunTest267()
        {
            RunTest(267);
        }
        [Test]
        public void RunTest268()
        {
            RunTest(268);
        }
        [Test]
        public void RunTest269()
        {
            RunTest(269);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest270()
        {
            RunTest(270);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest271()
        {
            RunTest(271);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest272()
        {
            RunTest(272);
        }
        [Test]
        public void RunTest273()
        {
            RunTest(273);
        }
        [Test]
        public void RunTest274()
        {
            RunTest(274);
        }
        [Test]
        public void RunTest275()
        {
            RunTest(275);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest276()
        {
            RunTest(276);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest277()
        {
            RunTest(277);
        }
        [Test]
        public void RunTest278()
        {
            RunTest(278);
        }
        [Test]
        public void RunTest279()
        {
            RunTest(279);
        }
        [Test]
        public void RunTest280()
        {
            RunTest(280);
        }
        [Test]
        public void RunTest281()
        {
            RunTest(281);
        }
        [Test]
        public void RunTest282()
        {
            RunTest(282);
        }
        [Test]
        public void RunTest283()
        {
            RunTest(283);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest284()
        {
            RunTest(284);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest285()
        {
            RunTest(285);
        }
        [Test]
        public void RunTest286()
        {
            RunTest(286);
        }
        [Test]
        public void RunTest287()
        {
            RunTest(287);
        }
        [Test]
        public void RunTest288()
        {
            RunTest(288);
        }
        [Test]
        public void RunTest289()
        {
            RunTest(289);
        }
        [Test]
        public void RunTest290()
        {
            RunTest(290);
        }
        [Test]
        public void RunTest291()
        {
            RunTest(291);
        }
        [Test]
        public void RunTest292()
        {
            RunTest(292);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest293()
        {
            RunTest(293);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest294()
        {
            RunTest(294);
        }
        [Test]
        public void RunTest295()
        {
            RunTest(295);
        }
        [Test]
        public void RunTest296()
        {
            RunTest(296);
        }
        [Test]
        public void RunTest297()
        {
            RunTest(297);
        }
        [Test]
        // Work in Progress until issue SOBEK-50156 has been solved
        [Category(TestCategory.WorkInProgress)]
        public void RunTest298()
        {
            RunTest(298);
        }
        [Test]
        public void RunTest299()
        {
            RunTest(299);
        }
        [Test]
        public void RunTest300()
        {
            RunTest(300);
        }
        [Test]
        public void RunTest301()
        {
            RunTest(301);
        }
        [Test]
        public void RunTest302()
        {
            RunTest(302);
        }
        [Test]
        public void RunTest303()
        {
            RunTest(303);
        }
        [Test]
        public void RunTest304()
        {
            RunTest(304);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest305()
        {
            RunTest(305);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest306()
        {
            RunTest(306);
        }
        [Test]
        public void RunTest307()
        {
            RunTest(307);
        }
        [Test]
        public void RunTest308()
        {
            RunTest(308);
        }
        [Test]
        public void RunTest309()
        {
            RunTest(309);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest310()
        {
            RunTest(310);
        }
        [Test]
        public void RunTest311()
        {
            RunTest(311);
        }
        [Test]
        public void RunTest312()
        {
            RunTest(312);
        }
        [Test]
        public void RunTest313()
        {
            RunTest(313);
        }
        [Test]
        public void RunTest314()
        {
            RunTest(314);
        }
        [Test]
        public void RunTest315()
        {
            RunTest(315);
        }
        [Test]
        public void RunTest316()
        {
            RunTest(316);
        }
        [Test]
        public void RunTest317()
        {
            RunTest(317);
        }
        [Test]
        public void RunTest318()
        {
            RunTest(318);
        }
        [Test]
        public void RunTest319()
        {
            RunTest(319);
        }
        [Test]
        public void RunTest320()
        {
            RunTest(320);
        }
        [Test]
        public void RunTest321()
        {
            RunTest(321);
        }
        [Test]
        public void RunTest322()
        {
            RunTest(322);
        }
        [Test]
        public void RunTest323()
        {
            RunTest(323);
        }
        [Test]
        public void RunTest324()
        {
            RunTest(324);
        }
        [Test]
        public void RunTest325()
        {
            RunTest(325);
        }
        [Test]
        public void RunTest326()
        {
            RunTest(326);
        }
        [Test]
        public void RunTest327()
        {
            RunTest(327);
        }
        [Test]
        public void RunTest328()
        {
            RunTest(328);
        }
        [Test]
        public void RunTest329()
        {
            RunTest(329);
        }
        [Test]
        public void RunTest330()
        {
            RunTest(330);
        }
        [Test]
        public void RunTest331()
        {
            RunTest(331);
        }
        [Test]
        public void RunTest332()
        {
            RunTest(332);
        }
        [Test]
        public void RunTest333()
        {
            RunTest(333);
        }
        [Test]
        public void RunTest334()
        {
            RunTest(334);
        }
        [Test]
        public void RunTest335()
        {
            RunTest(335);
        }
        [Test]
        public void RunTest336()
        {
            RunTest(336);
        }
        [Test]
        public void RunTest337()
        {
            RunTest(337);
        }
        [Test]
        public void RunTest338()
        {
            RunTest(338);
        }
        [Test]
        public void RunTest339()
        {
            RunTest(339);
        }
        [Test]
        public void RunTest340()
        {
            RunTest(340);
        }
        [Test]
        public void RunTest341()
        {
            RunTest(341);
        }
        [Test]
        public void RunTest342()
        {
            RunTest(342);
        }
        [Test]
        public void RunTest343()
        {
            RunTest(343);
        }
        [Test]
        public void RunTest344()
        {
            RunTest(344);
        }
        [Test]
        public void RunTest345()
        {
            RunTest(345);
        }
        [Test]
        public void RunTest346()
        {
            RunTest(346);
        }
        [Test]
        public void RunTest347()
        {
            RunTest(347);
        }
        [Test]
        public void RunTest348()
        {
            RunTest(348);
        }
        [Test]
        public void RunTest349()
        {
            RunTest(349);
        }
        [Test]
        public void RunTest350()
        {
            RunTest(350);
        }
        [Test]
        public void RunTest351()
        {
            RunTest(351);
        }
        [Test]
        public void RunTest352()
        {
            RunTest(352);
        }
        [Test]
        public void RunTest353()
        {
            RunTest(353);
        }
        [Test]
        public void RunTest354()
        {
            RunTest(354);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest355()
        {
            RunTest(355);
        }
        [Test]
        // Waiting for fix of SOBEK3-48
        [Category(TestCategory.WorkInProgress)]
        public void RunTest356()
        {
            RunTest(356);
        }
        [Test]
        public void RunTest357()
        {
            RunTest(357);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest358()
        {
            RunTest(358);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest359()
        {
            RunTest(359);
        }
        [Test]
        public void RunTest360()
        {
            RunTest(360);
        }
        [Test]
        public void RunTest361()
        {
            RunTest(361);
        }
        [Test]
        public void RunTest362()
        {
            RunTest(362);
        }
        [Test]
        public void RunTest363()
        {
            RunTest(363);
        }
        [Test]
        public void RunTest364()
        {
            RunTest(364);
        }
        [Test]
        public void RunTest365()
        {
            RunTest(365);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest366()
        {
            RunTest(366);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest367()
        {
            RunTest(367);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest368()
        {
            RunTest(368);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest369()
        {
            RunTest(369);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest370()
        {
            RunTest(370);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest371()
        {
            RunTest(371);
        }
        [Test]
        // SOBEK2 Reference data has two unlikely spikes
        [Category(TestCategory.WorkInProgress)]
        public void RunTest372()
        {
            RunTest(372);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest373()
        {
            RunTest(373);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest374()
        {
            RunTest(374);
        }
        [Test]
        public void RunTest375()
        {
            RunTest(375);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest376()
        {
            RunTest(376);
        }
        [Test]
        public void RunTest377()
        {
            RunTest(377);
        }
        [Test]
        // Sequential RR-Flow Test, not supported in DIMR
        [Category(TestCategory.WorkInProgress)]
        public void RunTest378()
        {
            RunTest(378);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest379()
        {
            RunTest(379);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest380()
        {
            RunTest(380);
        }
        [Test]
        public void RunTest381()
        {
            RunTest(381);
        }
        [Test]
        public void RunTest382()
        {
            RunTest(382);
        }
        [Test]
        public void RunTest383()
        {
            RunTest(383);
        }
        [Test]
        public void RunTest384()
        {
            RunTest(384);
        }
        [Test]
        public void RunTest385()
        {
            RunTest(385);
        }
        [Test]
        public void RunTest386()
        {
            RunTest(386);
        }
        [Test]
        public void RunTest387()
        {
            RunTest(387);
        }
        [Test]
        public void RunTest388()
        {
            RunTest(388);
        }
        [Test]
        public void RunTest389()
        {
            RunTest(389);
        }
        [Test]
        public void RunTest390()
        {
            RunTest(390);
        }
        [Test]
        public void RunTest391()
        {
            RunTest(391);
        }
        [Test]
        public void RunTest392()
        {
            RunTest(392);
        }
        [Test]
        public void RunTest393()
        {
            RunTest(393);
        }
        [Test]
        public void RunTest394()
        {
            RunTest(394);
        }
        [Test]
        public void RunTest395()
        {
            RunTest(395);
        }
        [Test]
        public void RunTest396()
        {
            RunTest(396);
        }
        [Test]
        public void RunTest397()
        {
            RunTest(397);
        }
        [Test]
        public void RunTest398()
        {
            RunTest(398);
        }
        [Test]
        public void RunTest399()
        {
            RunTest(399);
        }
        [Test]
        public void RunTest400()
        {
            RunTest(400);
        }
        [Test]
        public void RunTest401()
        {
            RunTest(401);
        }
        [Test]
        public void RunTest402()
        {
            RunTest(402);
        }
        [Test]
        public void RunTest403()
        {
            RunTest(403);
        }
        [Test]
        public void RunTest404()
        {
            RunTest(404);
        }
        [Test]
        public void RunTest405()
        {
            RunTest(405);
        }
        [Test]
        public void RunTest406()
        {
            RunTest(406);
        }
        [Test]
        public void RunTest407()
        {
            RunTest(407);
        }
        [Test]
        public void RunTest408()
        {
            RunTest(408);
        }
        [Test]
        public void RunTest409()
        {
            RunTest(409);
        }
        [Test]
        public void RunTest410()
        {
            RunTest(410);
        }
        [Test]
        public void RunTest411()
        {
            RunTest(411);
        }
        [Test]
        public void RunTest412()
        {
            RunTest(412);
        }
        [Test]
        public void RunTest413()
        {
            RunTest(413);
        }
        [Test]
        public void RunTest414()
        {
            RunTest(414);
        }
        [Test]
        public void RunTest415()
        {
            RunTest(415);
        }
        [Test]
        public void RunTest416()
        {
            RunTest(416);
        }
        [Test]
        public void RunTest417()
        {
            RunTest(417);
        }
        [Test]
        public void RunTest418()
        {
            RunTest(418);
        }
        [Test]
        public void RunTest419()
        {
            RunTest(419);
        }
        [Test]
        public void RunTest420()
        {
            RunTest(420);
        }
        [Test]
        public void RunTest421()
        {
            RunTest(421);
        }
        [Test]
        public void RunTest422()
        {
            RunTest(422);
        }
        [Test]
        public void RunTest423()
        {
            RunTest(423);
        }
        [Test]
        public void RunTest424()
        {
            RunTest(424);
        }
        [Test]
        public void RunTest425()
        {
            RunTest(425);
        }
        [Test]
        public void RunTest426()
        {
            RunTest(426);
        }
        [Test]
        public void RunTest427()
        {
            RunTest(427);
        }
        [Test]
        public void RunTest428()
        {
            RunTest(428);
        }
        [Test]
        public void RunTest429()
        {
            RunTest(429);
        }
        [Test]
        public void RunTest430()
        {
            RunTest(430);
        }
        [Test]
        public void RunTest431()
        {
            RunTest(431);
        }
        [Test]
        public void RunTest432()
        {
            RunTest(432);
        }
        [Test]
        public void RunTest433()
        {
            RunTest(433);
        }
        [Test]
        public void RunTest434()
        {
            RunTest(434);
        }
        [Test]
        public void RunTest435()
        {
            RunTest(435);
        }
        [Test]
        public void RunTest436()
        {
            RunTest(436);
        }
        [Test]
        public void RunTest437()
        {
            RunTest(437);
        }
        [Test]
        public void RunTest438()
        {
            RunTest(438);
        }
        [Test]
        public void RunTest439()
        {
            RunTest(439);
        }
        [Test]
        public void RunTest440()
        {
            RunTest(440);
        }
        [Test]
        public void RunTest441()
        {
            RunTest(441);
        }
        [Test]
        public void RunTest442()
        {
            RunTest(442);
        }
        [Test]
        public void RunTest443()
        {
            RunTest(443);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest444()
        {
            RunTest(444);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunTest445()
        {
            RunTest(445);
        }
        [Test]
        [Category(TestCategory.WorkInProgress)] // At request of Jan N. (after changes to Boundary locations: SOBEK3-1035)
        public void RunTest446()
        {
            RunTest(446);
        }
        [Test]
        public void RunTest447()
        {
            RunTest(447);
        }
        [Test]
        public void RunTest448()
        {
            RunTest(448);
        }
        [Test]
        public void RunTest449()
        {
            RunTest(449);
        }
        [Test]
        public void RunTest450()
        {
            RunTest(450);
        }
        [Test]
        public void RunTest451()
        {
            RunTest(451);
        }
        [Test]
        public void RunTest452()
        {
            RunTest(452);
        }
        [Test]
        public void RunTest453()
        {
            RunTest(453);
        }
        [Test]
        public void RunTest454()
        {
            RunTest(454);
        }
        [Test]
        public void RunTest455()
        {
            RunTest(455);
        }
        [Test]
        public void RunTest456()
        {
            RunTest(456);
        }
        [Test]
        public void RunTest457()
        {
            RunTest(457);
        }
        [Test]
        public void RunTest458()
        {
            RunTest(458);
        }
        [Test]
        public void RunTest459()
        {
            RunTest(459);
        }
        [Test]
        public void RunTest460()
        {
            RunTest(460);
        }
        [Test]
        public void RunTest461()
        {
            RunTest(461);
        }
        [Test]
        public void RunTest462()
        {
            RunTest(462);
        }
        [Test]
        public void RunTest463()
        {
            RunTest(463);
        }
        [Test]
        public void RunTest464()
        {
            RunTest(464);
        }
        [Test]
        public void RunTest465()
        {
            RunTest(465);
        }
        [Test]
        public void RunTest466()
        {
            RunTest(466);
        }
        [Test]
        public void RunTest467()
        {
            RunTest(467);
        }
        [Test]
        public void RunTest468()
        {
            RunTest(468);
        }
        [Test]
        public void RunTest469()
        {
            RunTest(469);
        }
        [Test]
        public void RunTest470()
        {
            RunTest(470);
        }
        [Test]
        public void RunTest471()
        {
            RunTest(471);
        }
        [Test]
        public void RunTest472()
        {
            RunTest(472);
        }
        [Test]
        public void RunTest473()
        {
            RunTest(473);
        }
        [Test]
        public void RunTest474()
        {
            RunTest(474);
        }
        [Test]
        public void RunTest475()
        {
            RunTest(475);
        }
        [Test]
        public void RunTest476()
        {
            RunTest(476);
        }
        [Test]
        public void RunTest477()
        {
            RunTest(477);
        }
        [Test]
        public void RunTest478()
        {
            RunTest(478);
        }
        [Test]
        public void RunTest479()
        {
            RunTest(479);
        }
        [Test]
        public void RunTest480()
        {
            RunTest(480);
        }
        [Test]
        public void RunTest481()
        {
            RunTest(481);
        }
        [Test]
        public void RunTest482()
        {
            RunTest(482);
        }
        [Test]
        public void RunTest483()
        {
            RunTest(483);
        }
        [Test]
        public void RunTest484()
        {
            RunTest(484);
        }
        [Test]
        public void RunTest485()
        {
            RunTest(485);
        }
        [Test]
        public void RunTest486()
        {
            RunTest(486);
        }
        [Test]
        public void RunTest487()
        {
            RunTest(487);
        }
        [Test]
        public void RunTest488()
        {
            RunTest(488);
        }
        [Test]
        public void RunTest489()
        {
            RunTest(489);
        }
        [Test]
        public void RunTest490()
        {
            RunTest(490);
        }
        [Test]
        public void RunTest491()
        {
            RunTest(491);
        }
        [Test]
        public void RunTest492()
        {
            RunTest(492);
        }
        [Test]
        public void RunTest493()
        {
            RunTest(493);
        }
        [Test]
        public void RunTest494()
        {
            RunTest(494);
        }
        [Test]
        public void RunTest495()
        {
            RunTest(495);
        }
        [Test]
        public void RunTest496()
        {
            RunTest(496);
        }
        [Test]
        public void RunTest497()
        {
            RunTest(497);
        }
        [Test]
        public void RunTest498()
        {
            RunTest(498);
        }
        [Test]
        public void RunTest499()
        {
            RunTest(499);
        }
        [Test]
        public void RunTest500()
        {
            RunTest(500);
        }
        #endregion

    }
}
