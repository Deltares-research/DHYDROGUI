using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Remoting;
using NUnit.Framework;
using RainfallRunoffModelEngine;
using Rhino.Mocks;

namespace RainfallRunoffModelEngineTest
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class RRModelHybridEngineTest
    {
        private MockRepository mocks = new MockRepository();
        private IRRModelEngineDll mockedDll;
        private IRRModelApi api;
        private IRRModelHybridFileWriter writer;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            WriteFixedFile("CROPFACT");
            WriteFixedFile("BERGCOEF");
            WriteFixedFile("KASGEBR");
            WriteFixedFile("KASINIT");
            WriteFixedFile("KASKLASS");
            WriteFixedFile("CROP_OW.PRN");
        }

        private void WriteFixedFile(string fileName)
        {
            var stream = AssemblyUtils.GetAssemblyResourceStream(GetType().Assembly, fileName);
            File.WriteAllText(fileName, new StreamReader(stream).ReadToEnd());
        }

        [SetUp]
        public void SetUp()
        {
            mockedDll = mocks.DynamicMock<IRRModelEngineDll>();
            writer = RRModelHybridFileWriterFactory.GetWriter(); //mocks.DynamicMock<IRRModelHybridFileWriter>();
            api = new RRModelHybridEngine(mockedDll);
        }

        [TearDown]
        public void TearDown()
        {
            CleanupRemoteApi();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "waterUsePerCapitaPerHourInDay should be length 24")]
        public void PavedToEngineWrongArray()
        {
            writer.AddPaved("paved1", 1000, 1, 1, 2, 3, 4, 5, 6, 0, true, 1, 2, LinkType.Boundary,
                         LinkType.WasteWaterTreatmentPlant, 5000, 0, new double[] { 0, 0, 0, 0, 0, 0 }, 0.0, "meteostationId", 1.0);
        }

        [Test]
        public void PavedToEngine()
        {
            var pavedId = writer.AddPaved("paved1", 1000, 1, 1, 2, 3, 4, 5, 6, 0, true, 1, 2, LinkType.Boundary, LinkType.WasteWaterTreatmentPlant, 5000, DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                                       new double[]
                                           {
                                               1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 10, 10, 5, 6, 7, 8, 9, 10, 10, 10
                                               , 10
                                           }, 0.0, "meteostationId", 1.0);
            var paved2Id = writer.AddPaved("paved2", 1000, 1, 1, 2, 3, 4, 5, 6, 0, true, 1, 2, LinkType.Boundary, LinkType.Boundary, 10000, DwfComputationOption.VariableDWF,
                                       new double[]
                                           {
                                               1, 2, 3, 88, 5, 6, 7, 8, 9, 10, 10, 10, 10, 10, 10, 5, 6, 7, 8, 9, 10, 10, 10
                                               , 10
                                           }, 0.0, "meteostationId", 1.0);
            Assert.AreEqual(1, pavedId);
            Assert.AreEqual(2, paved2Id);
            
            writer.WriteFiles();//writes files
            api.ModelInitialize(); 

            Assert.AreEqual(
                "PAVE id 'paved1' ar 1000 lv 1 sd 'paved1_storage' ss 0 qc 0 1 2 qo 2 0 ms 'meteostationId' is 0 np 5000 dw 'paved1_dwf' ro 0 ru 0 pave\r\n" +
                "PAVE id 'paved2' ar 1000 lv 1 sd 'paved2_storage' ss 0 qc 0 1 2 qo 0 0 ms 'meteostationId' is 0 np 10000 dw 'paved2_dwf' ro 0 ru 0 pave",
                ReadAllText("Paved.3b"));

            Assert.AreEqual(
                "STDF id 'paved1_storage' nm 'paved1_storage' ms 2 is 1 mr 4 6 ir 3 5 stdf\r\n" +
                "STDF id 'paved2_storage' nm 'paved2_storage' ms 2 is 1 mr 4 6 ir 3 5 stdf",
                ReadAllText("Paved.sto"));

            Assert.AreEqual(
                "DWA id 'paved1_dwf' nm 'paved1_dwf' do 1 wc 7.5 wd 0 wh 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 dwa\r\n" +
                "DWA id 'paved2_dwf' nm 'paved2_dwf' do 4 wc 0 wd 264 wh 0.378787878787879 0.757575757575758 1.13636363636364 33.3333333333333 1.89393939393939 2.27272727272727 2.65151515151515 3.03030303030303 3.40909090909091 3.78787878787879 3.78787878787879 3.78787878787879 3.78787878787879 3.78787878787879 3.78787878787879 1.89393939393939 2.27272727272727 2.65151515151515 3.03030303030303 3.40909090909091 3.78787878787879 3.78787878787879 3.78787878787879 3.78787878787879 dwa",
                ReadAllText("Paved.dwa"));

            Assert.AreEqual("", ReadAllText("Paved.tbl"));
        }

        [Test]
        public void PavedToEngineWithVariableCapacity()
        {
            var pavedId = writer.AddPaved("paved1", 1000, 1, 1, 2, 3, 4, 5, 6, SewerType.Separated, false, 1, 2, LinkType.Boundary, LinkType.WasteWaterTreatmentPlant, 5000, DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                                       new double[]
                                           {
                                               1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 10, 10, 5, 6, 7, 8, 9, 10, 10, 10
                                               , 10
                                           }, 0.5, "meteostationId", 1.0);
            Assert.AreEqual(1, pavedId);

                        var dateTimes = new[]
                            {
                                new DateTime(2001, 1, 1), new DateTime(2002, 2, 2), new DateTime(2003, 3, 3),
                                new DateTime(2004, 5, 6), new DateTime(2005, 6, 7), new DateTime(2008, 9, 10), 
                            };
            var dates = dateTimes.Select(RRModelEngineHelper.DateToInt).ToArray();
            var times = dateTimes.Select(RRModelEngineHelper.TimeToInt).ToArray();
            writer.SetPavedVariablePumpCapacities(pavedId, dates, times, dates.Select((d, i) => i*2.0).ToArray(),
                                               dates.Select((d, i) => i*3.0).ToArray());
            writer.WriteFiles();//writes files
            api.ModelInitialize(); 

            Assert.AreEqual(
                "PAVE id 'paved1' ar 1000 lv 1 sd 'paved1_storage' ss 1 qc 1 'paved1_qc' qo 2 0 ms 'meteostationId' is 0 np 5000 dw 'paved1_dwf' ro 1 ru 0.5 pave",
                ReadAllText("Paved.3b"));

            Assert.AreEqual(
                "STDF id 'paved1_storage' nm 'paved1_storage' ms 2 is 1 mr 4 6 ir 3 5 stdf",
                ReadAllText("Paved.sto"));

            Assert.AreEqual(
                "DWA id 'paved1_dwf' nm 'paved1_dwf' do 1 wc 7.5 wd 0 wh 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 dwa",
                ReadAllText("Paved.dwa"));

            Assert.AreEqual(
                "QC_T id 'paved1_qc' PDIN 1 1 '31536000' pdin\n" +
                "    TBLE\n" +
                "        '2001/01/01;00:00:00' 0    0 <\n" +
                "        '2002/02/02;00:00:00' 2    3 <\n" +
                "        '2003/03/03;00:00:00' 4    6 <\n" +
                "        '2004/05/06;00:00:00' 6    9 <\n" +
                "        '2005/06/07;00:00:00' 8    12 <\n" +
                "        '2008/09/10;00:00:00' 10    15 <\n" +
                "    tble\n" +
                "qc_t",
                ReadAllText("Paved.tbl"));
        }

        [Test]
        public void UnpavedHellingaToEngine()
        {
            var unpavedId = writer.AddUnpaved("unpaved1", new [] {1.2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 100.4, 1,
                                         DrainageComputationOption.DeZeeuwHellinga, 0, 1.3, 2, 1.2, 1, 1, 2, 2, "meteostationId", 1.0);
            writer.SetDeZeeuwHellinga(unpavedId, 1, 2.4, 3, new[] {0.2, 0.4, 0.8}, new double[] {2, 3, 4});
            writer.SetUnpavedConstantSeepage(unpavedId, 1);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(1, unpavedId);

            Assert.AreEqual(
                "UNPV id 'unpaved1' na 16 ar 1.2 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 lv 1 ga 100.4 co 1 su 0 sd 'unpaved1_storage' rc 0 ad 'unpaved1_hellinga' sp 'unpaved1_seepage' ic 'unpaved1_infilt' bt 1 ig 0 1 mg 2 gl 2 ms 'meteostationId' is 0 unpv",
                ReadAllText("Unpaved.3b"));
            Assert.AreEqual("ALFA id 'unpaved1_hellinga' nm 'unpaved1_hellinga' af 1 2 3 4 2.4 3 lv 0.2 0.4 0.8 alfa",
                            ReadAllText("Unpaved.alf"));
            Assert.AreEqual("STDF id 'unpaved1_storage' nm 'unpaved1_storage' ml 2 il 1.3 stdf", ReadAllText("Unpaved.sto"));
            Assert.AreEqual("SEEP id 'unpaved1_seepage' nm 'unpaved1_seepage' co 1 sp 1 ss 0 seep", ReadAllText("Unpaved.sep"));
            Assert.AreEqual("INFC id 'unpaved1_infilt' nm 'unpaved1_infilt' ic 1.2 infc", ReadAllText("Unpaved.inf"));
        }

        [Test]
        public void UnpavedErnstToEngine()
        {
            var unpavedId = writer.AddUnpaved("unpaved1", new[] { 1.2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 100.4, 1,
                                         DrainageComputationOption.Ernst, 0, 1.3, 2, 1.2, 1, 1, 2, 2, "meteostationId", 1.0);
            writer.SetErnst(unpavedId, 1, 2.4, 3, new[] { 0.2, 0.4, 0.8 }, new double[] { 2, 3, 4 });
            writer.SetUnpavedConstantSeepage(unpavedId, 1);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(1, unpavedId);

            Assert.AreEqual(
                "UNPV id 'unpaved1' na 16 ar 1.2 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 lv 1 ga 100.4 co 3 su 0 sd 'unpaved1_storage' rc 0 ed 'unpaved1_ernst' sp 'unpaved1_seepage' ic 'unpaved1_infilt' bt 1 ig 0 1 mg 2 gl 2 ms 'meteostationId' is 0 unpv",
                ReadAllText("Unpaved.3b"));
            Assert.AreEqual(
                "ERNS id 'unpaved1_ernst' nm 'unpaved1_ernst' cvs 1 cvo 2 3 4 2.4 cvi 3 lv 0.2 0.4 0.8 erns",
                ReadAllText("Unpaved.alf"));
            Assert.AreEqual("", ReadAllText("Unpaved.tbl"));
            Assert.AreEqual("STDF id 'unpaved1_storage' nm 'unpaved1_storage' ml 2 il 1.3 stdf", ReadAllText("Unpaved.sto"));
            Assert.AreEqual("SEEP id 'unpaved1_seepage' nm 'unpaved1_seepage' co 1 sp 1 ss 0 seep", ReadAllText("Unpaved.sep"));
            Assert.AreEqual("INFC id 'unpaved1_infilt' nm 'unpaved1_infilt' ic 1.2 infc", ReadAllText("Unpaved.inf"));
        }

        [Test]
        public void UnpavedKrayenhoffToEngine()
        {
            var unpavedId = writer.AddUnpaved("unpaved1", new[] { 1.2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 100.4, 1,
                                         DrainageComputationOption.KrayenhoffVdLeur, 1.9, 1.3, 2, 1.2, 1, 1, 2, 2, "meteostationId", 1.0);
            writer.SetUnpavedConstantSeepage(unpavedId, 1);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(1, unpavedId);

            Assert.AreEqual(
                "UNPV id 'unpaved1' na 16 ar 1.2 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 lv 1 ga 100.4 co 2 su 0 sd 'unpaved1_storage' rc 1.9 sp 'unpaved1_seepage' ic 'unpaved1_infilt' bt 1 ig 0 1 mg 2 gl 2 ms 'meteostationId' is 0 unpv",
                ReadAllText("Unpaved.3b"));
            Assert.AreEqual("", ReadAllText("Unpaved.alf"));
            Assert.AreEqual("STDF id 'unpaved1_storage' nm 'unpaved1_storage' ml 2 il 1.3 stdf", ReadAllText("Unpaved.sto"));
            Assert.AreEqual("SEEP id 'unpaved1_seepage' nm 'unpaved1_seepage' co 1 sp 1 ss 0 seep", ReadAllText("Unpaved.sep"));
            Assert.AreEqual("INFC id 'unpaved1_infilt' nm 'unpaved1_infilt' ic 1.2 infc", ReadAllText("Unpaved.inf"));
        }

        [Test]
        public void UnpavedH0SeepageToEngine()
        {
            var unpavedId = writer.AddUnpaved("unpaved1", new[] { 1.2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 100.4, 1,
                                         DrainageComputationOption.KrayenhoffVdLeur, 1.9, 1.3, 2, 1.2, 1, 1, 2, 2, "meteostationId", 1.0);

            var dateTimes = new[]
                            {
                                new DateTime(2001, 1, 1), new DateTime(2002, 2, 2), new DateTime(2003, 3, 3),
                                new DateTime(2004, 5, 6), new DateTime(2005, 6, 7), new DateTime(2008, 9, 10), 
                            };
            var dates = dateTimes.Select(RRModelEngineHelper.DateToInt).ToArray();
            var times = dateTimes.Select(RRModelEngineHelper.TimeToInt).ToArray();

            var values = new[] {1, 2, 3, 0.5, 0.8, 0.1};

            writer.SetUnpavedVariableSeepage(unpavedId, SeepageComputationOption.VariableWithH0, 55, dates, times, values);
            
            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(1, unpavedId);

            Assert.AreEqual(
                "UNPV id 'unpaved1' na 16 ar 1.2 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 lv 1 ga 100.4 co 2 su 0 sd 'unpaved1_storage' rc 1.9 sp 'unpaved1_seepage' ic 'unpaved1_infilt' bt 1 ig 0 1 mg 2 gl 2 ms 'meteostationId' is 0 unpv",
                ReadAllText("Unpaved.3b"));
            Assert.AreEqual("", ReadAllText("Unpaved.alf"));
            Assert.AreEqual("STDF id 'unpaved1_storage' nm 'unpaved1_storage' ml 2 il 1.3 stdf", ReadAllText("Unpaved.sto"));
            Assert.AreEqual("SEEP id 'unpaved1_seepage' nm 'unpaved1_seepage' co 2 cv 55 h0 'unpaved1_h0table' ss 0 seep", ReadAllText("Unpaved.sep"));
            Assert.AreEqual("INFC id 'unpaved1_infilt' nm 'unpaved1_infilt' ic 1.2 infc", ReadAllText("Unpaved.inf"));
            Assert.AreEqual(
                "H0_T id 'unpaved1_h0table' PDIN 1 1 '31536000' pdin\n" +
                "    TBLE\n        '2001/01/01;00:00:00' 1 <\n        '2002/02/02;00:00:00' 2 <\n        '2003/03/03;00:00:00' 3 <\n" +
                "        '2004/05/06;00:00:00' 0.5 <\n        '2005/06/07;00:00:00' 0.8 <\n        '2008/09/10;00:00:00' 0.1 <\n" +
                "    tble\nh0_t", 
                ReadAllText("Unpaved.tbl"));
        }

        [Test]
        public void WwtpToEngine()
        {
            var lateralId = writer.AddBoundaryNode("lt1", -99.0);
            var boundaryId = writer.AddBoundaryNode("bd1", -99.0);
            var wwtpId = writer.AddWasteWaterTreatmentPlant("wwtp1");
            
            Assert.AreEqual(1, wwtpId);
            Assert.AreEqual(1, lateralId);
            Assert.AreEqual(2, boundaryId);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual("NODE id 'lt1' nm 'lt1' ri '-1' mt 1 '6' nt 78 ObID 'SBK_SBK-3B-NODE' px 0.0 py 0.0 node\r\n" +
                            "NODE id 'bd1' nm 'bd1' ri '-1' mt 1 '6' nt 78 ObID 'SBK_SBK-3B-NODE' px 0.0 py 0.0 node\r\n" +
                            "NODE id 'wwtp1' nm 'wwtp1' ri '-1' mt 1 '14' nt 56 ObID '3B_WWTP' px 0.0 py 0.0 node",
                            ReadAllText("3B_NOD.TP"));

            Assert.AreEqual("WWTP id 'wwtp1' tb 0 wwtp", ReadAllText("WWTP.3B"));
        }

        [Test]
        public void BoundariesToEngine()
        {
            var lateralId = writer.AddBoundaryNode("lt1", -99.0);
            var boundaryId = writer.AddBoundaryNode("bd1", -99.0);

            Assert.AreEqual(1, lateralId);
            Assert.AreEqual(2, boundaryId);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual("BOUN id 'lt1' bl 0 -99 is 0 boun\r\n" +
                            "BOUN id 'bd1' bl 0 -99 is 0 boun",
                            ReadAllText("Bound3B.3B"));
        }

        [Test]
        public void OpenWaterToEngine()
        {
            var openWaterId = writer.AddOpenWater("openwaterId", 123.45, "meteostationId", 1.0);
            
            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(1, openWaterId);

            Assert.AreEqual(
                "OWRR id 'openwaterId' ar 123.45 ms 'meteostationId' owrr",
                ReadAllText("Openwate.3b"));
        }

        [Test]
        public void GreenhouseToEngine()
        {
            var greenhouseId = writer.AddGreenhouse("greenhouseId", new[] { 1.1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 11.1, 12.2, 13.3, 14.4, 15, true, 16, "meteostationId", 1.0);

            Assert.AreEqual(1, greenhouseId);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(
                "GRHS id 'greenhouseId' na 10  ar 1.1 2 3 4 5 6 7 8 9 10 sl 11.1 as 16 si 'greenhouseId' sd 'greenhouseId' ms 'meteostationId' is 0.0 grhs",
                ReadAllText("Greenhse.3b"));

            Assert.AreEqual("SILO id 'greenhouseId' nm 'greenhouseId' sc 14.4 pc 15 silo", ReadAllText("Greenhse.sil"));

            Assert.AreEqual("STDF id 'greenhouseId' nm 'greenhouseId' mk 13.3 ik 12.2 stdf", ReadAllText("Greenhse.rf"));
        }

        [Test]
        //Verify test expectation!!!! ToDo
        public void GreenhouseDontUseSiloAreaToEngine()
        {
            var greenhouseId = writer.AddGreenhouse("greenhouseId", new[] { 1.1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 11.1, 12.2, 13.3, 14.4, 15, false, 16, "meteostationId", 1.0);

            Assert.AreEqual(1, greenhouseId);

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(
                "GRHS id 'greenhouseId' na 10  ar 1.1 2 3 4 5 6 7 8 9 10 sl 11.1 as 0 si '-1' sd 'greenhouseId' ms 'meteostationId' is 0.0 grhs",
                ReadAllText("Greenhse.3b"));

            Assert.AreEqual("", ReadAllText("Greenhse.sil"));

            Assert.AreEqual("STDF id 'greenhouseId' nm 'greenhouseId' mk 13.3 ik 12.2 stdf", ReadAllText("Greenhse.rf"));
        }

        [Test]
        public void Initialize()
        {
            var numTimeSteps = 1;
            var timeStep = 60 * 60; //1h
            var start = new DateTime(1951, 1, 1, 0, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000*start.Year + 100*start.Month + start.Day;
            var starttime = 10000*start.Hour + 100*start.Minute + start.Second;
            var enddate = 10000*end.Year + 100*end.Month + end.Day;
            var endtime = 10000*end.Hour + 100*end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, 60, 120);
            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.GreaterOrEqual(ReadAllText("DELFT_3B.INI").Length, 700);
        }

        [Test]
        public void InitializeAndFinalizeWithRealDll()
        {
            var modelApi = GetRemoteApi();

            var numTimeSteps = 1;
            var timeStep = 60; //1h
            var start = new DateTime(2012, 1, 1, 0, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, 60, 120);

            SetMeteo(numTimeSteps, startdate,starttime, timeStep, null, 0.0);

            writer.WriteFiles();//writes files
            var success = modelApi.ModelInitialize() && modelApi.ModelFinalize();
            
            Assert.IsTrue(success);
        }

        [Test]
        public void RunUnpavedNode()
        {
            var modelApi = GetRemoteApi();
            var numTimeSteps = 10;
            var timeStep = 60*60; //1h
            var start = new DateTime(1951, 1, 1, 0, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);
            
            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, timeStep, timeStep);

            SetMeteo(numTimeSteps, startdate, starttime, timeStep, null, 0.0);

            var unpavedName = "unpaved1";
            var unpavedId = writer.AddUnpaved(unpavedName, new[] { 5000.0, 0.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 5000.0, 1,
                                           DrainageComputationOption.Ernst, 0, 0, 2, 1.2, 5, 1.0, 3, 2, "meteostationId", 1.0);
            writer.SetErnst(unpavedId, 1, 2.4, 3, new[] { 0.2, 0.4, 0.8 }, new double[] { 2, 2, 2 });
            writer.SetUnpavedConstantSeepage(unpavedId, 1);
            var nodeName = "node1";
            writer.AddBoundaryNode(nodeName, -99.0);
            writer.AddLink("1",unpavedName, nodeName);

            writer.WriteFiles();//writes files
            var initialized = modelApi.ModelInitialize();
            
            var runoffs = new List<double>();

            var runSuccess = true;
            for (int i = 0; i < numTimeSteps; i++)
            {
                runSuccess &= modelApi.ModelPerformTimeStep();
                var runoff = modelApi.GetValue(QuantityType.Flow, ElementSet.BoundaryElmSet, 1);
                Console.WriteLine("Runoff: {0}", runoff);
                runoffs.Add(runoff);
            }

            Assert.IsTrue(runoffs.Skip(1).All(r => r > 0 && r <= 3)); //all values within some (random) range

            var finished = modelApi.ModelFinalize();

            Assert.IsTrue(initialized);
            Assert.IsTrue(runSuccess);
            Assert.IsTrue(finished);
        }


        [Test]
        public void MultiplePrecipitationStations()
        {
            writer.SetMeteoDataStartTimeAndInterval(20120227,123400,3600);
            writer.AddPrecipitationStation("StationA", new double[]{1,2,3,4,5,6,7,8,9,10});
            writer.AddPrecipitationStation("StationB", new [] { 11.1, 22.2, 33.3, 44.4, 55.5, 66.6, 77.7, 88.8, 99.9, 100 });

            var expectationFileString = "1" + Environment.NewLine +
                                        "*Aantal stations" + Environment.NewLine +
                                        "2" + Environment.NewLine +
                                        "*Namen van stations" + Environment.NewLine +
                                        "'StationA'" + Environment.NewLine +
                                        "'StationB'" + Environment.NewLine + 
                                        "*Aantal gebeurtenissen (omdat het 1 bui betreft is dit altijd 1)" + Environment.NewLine +
                                        "*en het aantal seconden per waarnemingstijdstap" + Environment.NewLine +
                                        "1 3600" + Environment.NewLine +
                                        "*Elke commentaarregel wordt begonnen met een * (asteriks)." + Environment.NewLine +
                                        "2012 2 27 12 34 0 0 10 0 0" + Environment.NewLine +
                                        "1 11.1 " + Environment.NewLine +
                                        "2 22.2 " + Environment.NewLine +
                                        "3 33.3 " + Environment.NewLine +
                                        "4 44.4 " + Environment.NewLine +
                                        "5 55.5 " + Environment.NewLine +
                                        "6 66.6 " + Environment.NewLine +
                                        "7 77.7 " + Environment.NewLine +
                                        "8 88.8 " + Environment.NewLine +
                                        "9 99.9 " + Environment.NewLine +
                                        "10 100 " + Environment.NewLine + 
                                        "0 0 ";

            writer.WriteFiles();//writes files
            api.ModelInitialize();

            Assert.AreEqual(expectationFileString, ReadAllText("DEFAULT.BUI"));

        }


        [Test]
        public void RunUnpavedNodeAndSetBoundaryWaterLevel()
        {
            var modelApi = GetRemoteApi();

            var numTimeSteps = 10;
            var timeStep = 60 * 60; //1h
            var start = new DateTime(1951, 1, 1, 0, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, timeStep, timeStep);

            SetMeteo(numTimeSteps, startdate, starttime, timeStep, null, 0.0);

            var unpavedName = "unpaved1";
            var unpavedId = writer.AddUnpaved(unpavedName, new[] { 5000000000.0, 0.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 5000000000.0, 1,
                                           DrainageComputationOption.Ernst, 0, 0, 2, 1.2, 5, 1.0, 3, 2, "meteostationId", 1.0);
            writer.SetErnst(unpavedId, 1, 2.4, 3, new[] { 0.2, 0.4, 0.8 }, new double[] { 2, 2, 2 });
            writer.SetUnpavedConstantSeepage(unpavedId, 1);

            var nodeName = "node1";
            writer.AddBoundaryNode(nodeName, -99.0);
            writer.AddLink("1", unpavedName, nodeName);

            writer.WriteFiles();//writes files
            var initialized = modelApi.ModelInitialize();

            modelApi.SetValues(QuantityType.BndLevels, ElementSet.BoundaryElmSet, new[] { 1.0 });
            
            var runoffs = new List<double>();

            var runSuccess = true;
            for (int i = 0; i < numTimeSteps; i++)
            {
                runSuccess &= modelApi.ModelPerformTimeStep();
                var runoff = modelApi.GetValue(QuantityType.Flow, ElementSet.BoundaryElmSet, 1);
                Console.WriteLine("Runoff: {0}", runoff);
                runoffs.Add(runoff);
            }

            for (int i = 0; i < runoffs.Count; i++)
            {
                // all positive Q's: 
                // boundary water level is equal service and ground water level, 
                // rain fall is added (but after each step a litte bit less) 
                // so it produces discharge to the boundary (unpaved -> boundary == Q positive)
                Assert.IsTrue(runoffs.ElementAt(i) > 0); 
            }
            var finished = modelApi.ModelFinalize();

            Assert.IsTrue(initialized);
            Assert.IsTrue(runSuccess);
            Assert.IsTrue(finished);
        }

        [Test]
        public void RunPavedNode()
        {
            var modelApi = GetRemoteApi();
            
            var numTimeSteps = 10;
            var timeStep = 60 * 60; //1h
            var start = new DateTime(1951, 1, 1, 0, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, timeStep, timeStep);

            SetMeteo(numTimeSteps, startdate,starttime, timeStep, null, 0.0);

            var pavedName = "paved1";
            var pavedId = writer.AddPaved(pavedName, 10000, 0, 0, 1, 0, 1, 0, 1, SewerType.Mixed, true, 100, 100, LinkType.Boundary, LinkType.Boundary, 1000,
                                       DwfComputationOption.VariableDWF, Enumerable.Range(0, 24).Select(i => 30.0).ToArray(), 0.0, "meteostationId", 1.0);

            var nodeName = "node1";
            writer.AddBoundaryNode(nodeName, -99.0);
            writer.AddLink("1", pavedName, nodeName);

            writer.WriteFiles();//writes files
            var initialized = modelApi.ModelInitialize();

            var runoffs = new List<double>();

            var runSuccess = true;
            for (int i = 0; i < numTimeSteps; i++)
            {
                runSuccess &= modelApi.ModelPerformTimeStep();
                var runoff = modelApi.GetValue(QuantityType.Flow, ElementSet.BoundaryElmSet, 1);
                Console.WriteLine("Runoff: {0}", runoff);
                runoffs.Add(runoff);
            }

            Assert.IsTrue(runoffs.All(r => r > 0 && r <= 0.1)); //all values within >0 and <=0.1

            var finished = modelApi.ModelFinalize();

            Assert.IsTrue(initialized);
            Assert.IsTrue(runSuccess);
            Assert.IsTrue(finished); 
        }
        
        [Test]
        public void RunOpenWaterNode()
        {
            var modelApi = GetRemoteApi();

            var numTimeSteps = 10;
            var timeStep = 60 * 60; //1h
            var start = new DateTime(1951, 1, 1, 0, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, timeStep, timeStep);

            SetMeteo(numTimeSteps, startdate, starttime, timeStep, null, 0.0);

            var openWaterName = "openWater1";
            var openWaterId = writer.AddOpenWater(openWaterName, 54321, "meteostationId", 1.0);

            var nodeName = "node1";
            writer.AddBoundaryNode(nodeName, -99.0);
            writer.AddLink("1", openWaterName, nodeName);

            writer.WriteFiles();//writes files
            var initialized = modelApi.ModelInitialize();

            var linkFlows = new List<double>();
            var precipitations = new List<double>();
            var evaporations = new List<double>();

            var runSuccess = true;
            for (int i = 0; i < numTimeSteps; i++)
            {
                runSuccess &= modelApi.ModelPerformTimeStep();
                var runoff = modelApi.GetValue(QuantityType.Flow, ElementSet.LinkElmSet, 1);
                Console.WriteLine("Runoff: {0}", runoff);
                linkFlows.Add(runoff);

                var precipitation = modelApi.GetValue(QuantityType.Rainfall, ElementSet.OpenWaterElmSet, 1);
                Console.WriteLine("Rainfall: {0}", precipitation);
                precipitations.Add(precipitation);

                var evaporation = modelApi.GetValue(QuantityType.EvaporationSurface, ElementSet.OpenWaterElmSet, 1);
                Console.WriteLine("Evaporation: {0}", evaporation);
                evaporations.Add(evaporation);
            }

            Assert.IsTrue(linkFlows.All(r => r > 0 && r <= 0.2)); //all values within >0 and <=0.2
            Assert.IsTrue(precipitations.All(r => r > 0 && r <= 0.2)); //all values within >0 and <=0.2
            Assert.IsTrue(evaporations.All(r => r == 0)); //no evaporation

            var finished = modelApi.ModelFinalize();

            Assert.IsTrue(initialized);
            Assert.IsTrue(runSuccess);
            Assert.IsTrue(finished);
        }

        [Test]
        public void RunOpenWaterNodeWithEvaporationActivePeriod()
        {
            var modelApi = GetRemoteApi();

            var numTimeSteps = 10;
            var timeStep = 60 * 60; //1h
            var start = new DateTime(1951, 1, 1, 7, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, timeStep, timeStep);

            SetMeteo(numTimeSteps, startdate, starttime, timeStep, null, 1.0);

            var openWaterName = "openWater1";
            var openWaterId = writer.AddOpenWater(openWaterName, 54321, "meteostationId", 1.0);

            var nodeName = "node1";
            writer.AddBoundaryNode(nodeName, -99.0);
            writer.AddLink("1", openWaterName, nodeName);

            writer.WriteFiles();//writes files
            var initialized = modelApi.ModelInitialize();

            var linkFlows = new List<double>();
            var precipitations = new List<double>();
            var evaporations = new List<double>();

            var runSuccess = true;
            for (int i = 0; i < numTimeSteps; i++)
            {
                runSuccess &= modelApi.ModelPerformTimeStep();
                var runoff = modelApi.GetValue(QuantityType.Flow, ElementSet.LinkElmSet, 1);
                Console.WriteLine("Runoff: {0}", runoff);
                linkFlows.Add(runoff);

                var precipitation = modelApi.GetValue(QuantityType.Rainfall, ElementSet.OpenWaterElmSet, 1);
                Console.WriteLine("Rainfall: {0}", precipitation);
                precipitations.Add(precipitation);

                var evaporation = modelApi.GetValue(QuantityType.EvaporationSurface, ElementSet.OpenWaterElmSet, 1);
                Console.WriteLine("Evaporation: {0}", evaporation);
                evaporations.Add(evaporation);
            }

            Assert.IsTrue(linkFlows.All(r => r > 0));
            Assert.IsTrue(precipitations.All(r => r > 0));
            Assert.IsTrue(evaporations.All(r => r > 0.0));

            var finished = modelApi.ModelFinalize();

            Assert.IsTrue(initialized);
            Assert.IsTrue(runSuccess);
            Assert.IsTrue(finished);
        }

        [Test]
        public void RunOpenWaterNodeWithEvaporationNoPrecipitationActivePeriod()
        {
            var modelApi = GetRemoteApi();

            var numTimeSteps = 10;
            var timeStep = 60 * 60; //1h
            var start = new DateTime(1951, 1, 1, 7, 0, 0);
            var end = start.AddSeconds(numTimeSteps * timeStep);

            var startdate = 10000 * start.Year + 100 * start.Month + start.Day;
            var starttime = 10000 * start.Hour + 100 * start.Minute + start.Second;
            var enddate = 10000 * end.Year + 100 * end.Month + end.Day;
            var endtime = 10000 * end.Hour + 100 * end.Minute + end.Second;

            writer.SetSimulationTimesAndGenerateIniFile(startdate, starttime, enddate, endtime, timeStep, timeStep);

            SetMeteo(numTimeSteps, startdate, starttime, timeStep, 0.0, 1.0);

            var openWaterName = "openWater1";
            var openWaterId = writer.AddOpenWater(openWaterName, 54321, "meteostationId", 1.0);

            var nodeName = "node1";
            writer.AddBoundaryNode(nodeName, -99.0);
            writer.AddLink("1", openWaterName, nodeName);

            writer.WriteFiles();//writes files
            var initialized = modelApi.ModelInitialize();

            var linkFlows = new List<double>();
            var precipitations = new List<double>();
            var evaporations = new List<double>();

            var runSuccess = true;
            for (int i = 0; i < numTimeSteps; i++)
            {
                runSuccess &= modelApi.ModelPerformTimeStep();
                var runoff = modelApi.GetValue(QuantityType.Flow, ElementSet.LinkElmSet, 1);
                Console.WriteLine("Runoff: {0}", runoff);
                linkFlows.Add(runoff);

                var precipitation = modelApi.GetValue(QuantityType.Rainfall, ElementSet.OpenWaterElmSet, 1);
                Console.WriteLine("Rainfall: {0}", precipitation);
                precipitations.Add(precipitation);

                var evaporation = modelApi.GetValue(QuantityType.EvaporationSurface, ElementSet.OpenWaterElmSet, 1);
                Console.WriteLine("Evaporation: {0}", evaporation);
                evaporations.Add(evaporation);
            }

            Assert.IsTrue(linkFlows.All(r => r < 0));
            Assert.IsTrue(precipitations.All(r => r == 0));
            Assert.IsTrue(evaporations.All(r => r > 0.0));

            var finished = modelApi.ModelFinalize();

            Assert.IsTrue(initialized);
            Assert.IsTrue(runSuccess);
            Assert.IsTrue(finished);
        }

        private void SetMeteo(int numTimesteps, int sobekStartDate, int sobekStartTime, int timeStepInSeconds,
            double? precipitationValue, double evaporationValue)
        {
            writer.SetMeteoDataStartTimeAndInterval(sobekStartDate, sobekStartTime, timeStepInSeconds);
            if (precipitationValue != null)
            {
                writer.AddPrecipitationStation("precip", Enumerable.Range(0, numTimesteps + 1).Select(v => precipitationValue.Value).ToArray());
            }
            else
            {
                writer.AddPrecipitationStation("precip", Enumerable.Range(0, numTimesteps + 1).Select(v => (double)numTimesteps - v).ToArray());
            }
            writer.AddEvaporationStation("evap", Enumerable.Range(0, numTimesteps + 1).Select(v => evaporationValue).ToArray());
        }

        private IRRModelApi remoteApi;
        private IRRModelApi GetRemoteApi()
        {
            remoteApi = RemoteInstanceContainer.CreateInstance<IRRModelApi, RRModelHybridEngine>();
            //remoteApi = new RRModelHybridEngine();
            return remoteApi;
        }

        private void CleanupRemoteApi()
        {
            if (remoteApi != null)
            {
                RemoteInstanceContainer.RemoveInstance(remoteApi);
                remoteApi = null;
            }
        }

        [Test]
        public void GetError()
        {
            var writer = GetRemoteApi();
            var error = writer.GetError(-77);
            Assert.AreEqual("WLDelft OpenMI Component Demo License expired", error);
        }

        private string ReadAllText(string sobekFile)
        {
            return File.ReadAllText(sobekFile);
        }
    }
}
