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
    [TestFixture]
    public class SobekControllerReaderTest
    {
        [Test]
        public void ReadFromStringSobek212Format()
        {
            var fileText = @"CNTL id '22128_1' nm 'Hvl_schuif1' ct 1 ca 2 ac 1 cf 0 ta 1 0 0 0 gi '0' '1' '-1' '-1' ao 1 1 1 1 cp 1 mp 86400 ml  'P_003_0' '-1' '-1' '-1' '-1' si '-1' hc ht PDIN 0 0 '' pdin CLTT 'Discharge [m3/s]' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid " + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"0 0 <" + Environment.NewLine +
                @"535 0 <" + Environment.NewLine +
                @"605 0 <" + Environment.NewLine +
                @"655 0 <" + Environment.NewLine +
                @"695 0 <" + Environment.NewLine +
                @"770 0 <" + Environment.NewLine +
                @"845 0 <" + Environment.NewLine +
                @"920 0.0251889 <" + Environment.NewLine +
                @"995 0.0251889 <" + Environment.NewLine +
                @"1025 0.0251889 <" + Environment.NewLine +
                @"1070 0.0251889 <" + Environment.NewLine +
                @"1105 0.0251889 <" + Environment.NewLine +
                @"1145 0.0251889 <" + Environment.NewLine +
                @"1190 0.0251889 <" + Environment.NewLine +
                @"1240 0.0534005 <" + Environment.NewLine +
                @"1290 0.088665 <" + Environment.NewLine +
                @"1350 0.126952 <" + Environment.NewLine +
                @"1415 0.166247 <" + Environment.NewLine +
                @"1475 0.207557 <" + Environment.NewLine +
                @"1540 0.250882 <" + Environment.NewLine +
                @"1605 0.296222 <" + Environment.NewLine +
                @"1675 0.343577 <" + Environment.NewLine +
                @"1740 0.392947 <" + Environment.NewLine +
                @"1805 0.443325 <" + Environment.NewLine +
                @"1875 0.495718 <" + Environment.NewLine +
                @"1940 0.550126 <" + Environment.NewLine +
                @"2010 0.606549 <" + Environment.NewLine +
                @"2145 0.725441 <" + Environment.NewLine +
                @"2275 0.850378 <" + Environment.NewLine +
                @"2405 0.984383 <" + Environment.NewLine +
                @"2535 1.12443 <" + Environment.NewLine +
                @"2665 1.24937 <" + Environment.NewLine +
                @"2795 1.31083 <" + Environment.NewLine +
                @"2925 1.37128 <" + Environment.NewLine +
                @"3055 1.43375 <" + Environment.NewLine +
                @"3185 1.49521 <" + Environment.NewLine +
                @"3315 1.55668 <" + Environment.NewLine +
                @"3445 1.61914 <" + Environment.NewLine +
                @"3570 1.68161 <" + Environment.NewLine +
                @"3705 1.74509 <" + Environment.NewLine +
                @"3835 1.80957 <" + Environment.NewLine +
                @"3970 1.87406 <" + Environment.NewLine +
                @"4300 2.05743 <" + Environment.NewLine +
                @"4625 2.267 <" + Environment.NewLine +
                @"4935 2.50882 <" + Environment.NewLine +
                @"5250 2.78086 <" + Environment.NewLine +
                @"5540 3.12343 <" + Environment.NewLine +
                @"5825 3.66751 <" + Environment.NewLine +
                @"6110 6.04534 <" + Environment.NewLine +
                @"6405 11 <" + Environment.NewLine +
                @"13000 11 <" + Environment.NewLine +
                @"20000 11 <" + Environment.NewLine +
                @"tble  ps 9999900000 ns 9999900000 cntl" + Environment.NewLine +
                @"CNTL id '18044_1' nm 'Hvl_Sluit_Contr' ct 0 ca 2 ac 1 cf 1 ta 1 0 0 0 gi '1' '-1' '-1' '-1' ao 1 1 1 1 mc 0 bl 0 ti tv PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1991/01/01;00:00:00' 0 <" + Environment.NewLine +
                @"tble  cntl" + Environment.NewLine +
                @"CNTL id '68_1' nm 'HY_Sluit_Contr' ct 0 ca 2 ac 1 cf 1 ta 1 1 0 0 gi '4' '20770' '-1' '-1' ao 1 1 1 1 mc 0.0025 bl 1 ti tv PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1991/01/01;00:00:00' 0 <" + Environment.NewLine +
                @"tble  cntl" + Environment.NewLine +
                @"CNTL id '69_1' nm 'HY_OPEN_CONTR' ct 0 ca 2 ac 1 cf 1 ta 1 1 0 0 gi '5' '20771' '-1' '-1' ao 1 0 1 1 mc 0.005 bl 0 ti tv PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1991/01/01;00:00:00' 9 <" + Environment.NewLine +
                @"tble  cntl;";

            var sobekControllers = new SobekControllerReader().Parse(fileText).ToArray();

            Assert.AreEqual(4, sobekControllers.Length);
            Assert.AreEqual("CTR_22128_1", sobekControllers[0].Id);
            Assert.AreEqual("Hvl_schuif1", sobekControllers[0].Name);
            Assert.AreEqual(SobekControllerType.HydraulicController,sobekControllers[0].ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekControllers[0].SobekControllerParameterType);
            Assert.IsTrue(sobekControllers[0].IsActive);
            Assert.AreEqual(4, sobekControllers[0].Triggers.Count);
            Assert.AreEqual("TRG_0", sobekControllers[0].Triggers[0].Id);
            Assert.AreEqual(true, sobekControllers[0].Triggers[0].Active);
            Assert.AreEqual(true, sobekControllers[0].Triggers[0].And);

            Assert.IsNotNull(sobekControllers[0].LookUpTable);
            Assert.AreEqual(52, sobekControllers[0].LookUpTable.Rows.Count);

        }

        [Test]
        public void ReadFromStringSobek212Format_ExtraSpaceAfter_ml() //Convert Sebek RE -> JAMM2010 -> Sobek 212
        {
            var fileText =
                @"CNTL id '08_1' nm 'Limmel_1' ct 1 ca 0 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 1 mp 0 ml  '002_0' '-1' '-1' '-1' '-1' si '-1' hc ht PDIN 0 0 '' pdin CLTT 'Discharge [m3/s]' 'Crest Level [m]' cltt CLID '(null)' '(null)' clid  " +
                Environment.NewLine +
                @"TBLE  " + Environment.NewLine +
                @"0 39.6 < " + Environment.NewLine +
                @"1250 39.6 < " + Environment.NewLine +
                @"1300 100 < " + Environment.NewLine +
                @"2000 100 < " + Environment.NewLine +
                @"5000 100 < " + Environment.NewLine +
                @"tble  ps 9999900000 ns 9999900000 cntl";

            var sobekControllers = new SobekControllerReader().Parse(fileText).ToArray();

            Assert.AreEqual("CTR_08_1", sobekControllers[0].Id);
            Assert.AreEqual("002_0", sobekControllers[0].MeasurementStationId);
        }

        [Test]
        public void ReadFromStringSobekREFormat()
        {
            var fileText = @"CNTL id '68' nm 'HY_Sluit_Contr' ta 1 1 0 0 gi '4' '20770' '-1' '-1' ao 1 1 1 1 ct 0 ac 1 ca 2 cf 1 cb '28' '-1' '-1' '-1' '-1' cl 0 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1991/01/01;00:00:00' 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @" mp 20000 mc 0.0025 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '24' hc ht 5 9.9999e+009 9.9999e+009 'Hydraulic Controller' PDIN 0 0 '' pdin CLTT 'Water Level [m]' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"-10 8 < " + Environment.NewLine +
                            @"2 8 < " + Environment.NewLine +
                            @"2 0 < " + Environment.NewLine +
                            @"10 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @" bl 1 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl" + Environment.NewLine +
                            @"CNTL id '69' nm 'HY_OPEN_CONTR' ta 1 1 0 0 gi '5' '20771' '-1' '-1' ao 1 0 1 1 ct 0 ac 1 ca 2 cf 1 cb '-1' '-1' '-1' '-1' '-1' cl 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1991/01/01;00:00:00' 9 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @" mp 500000 mc 0.005 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl" + Environment.NewLine +
                            @"CNTL id '18044' nm 'Hvl_Sluit_Contr' ta 1 0 0 0 gi '1' '-1' '-1' '-1' ao 1 1 1 1 ct 0 ac 1 ca 2 cf 1 cb '-1' '-1' '-1' '-1' '-1' cl 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1991/01/01;00:00:00' 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @" mp 0 mc 0 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";

 
            var sobekControllers = new SobekControllerReader().Parse(fileText).ToArray();

            Assert.AreEqual(3, sobekControllers.Length);
            Assert.AreEqual("CTR_68", sobekControllers[0].Id);
            Assert.AreEqual("HY_Sluit_Contr", sobekControllers[0].Name);
            Assert.AreEqual(SobekControllerType.TimeController, sobekControllers[0].ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekControllers[0].SobekControllerParameterType);
            Assert.IsTrue(sobekControllers[0].IsActive);
            Assert.AreEqual(4, sobekControllers[0].Triggers.Count);
            Assert.AreEqual("TRG_4", sobekControllers[0].Triggers[0].Id);
            Assert.AreEqual(true, sobekControllers[0].Triggers[0].Active);
            Assert.AreEqual(true, sobekControllers[0].Triggers[0].And);
        }

        [Test]
        public void ReadStringFromRE_NDB()
        {
            var fileText =
                @"CNTL id '22128' nm 'Hvl_schuif1' ta 1 0 0 0 gi '0' '1' '-1' '-1' ao 1 1 1 1 ct 1 ac 1 ca 2 cf 0 cb 'P_003' '-1' '-1' '-1' '-1' cl 0 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 1 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"'2002/01/03;07:01:00' 0 < " + Environment.NewLine +
                @"'2002/01/03;07:08:00' 1.21 < " + Environment.NewLine +
                @"'2002/01/03;10:00:00' 1.21 < " + Environment.NewLine +
                @"'2002/01/03;10:07:00' 0.75 < " + Environment.NewLine +
                @"'2002/01/03;16:25:00' 0.75 < " + Environment.NewLine +
                @"'2002/01/03;16:32:00' 0 < " + Environment.NewLine +
                @"'2002/01/03;19:39:00' 0 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" mc 0 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 'Hydraulic Controller' PDIN 0 0 '' pdin CLTT 'Discharge [m3/s]' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 0 < " + Environment.NewLine +
                @"535 0 < " + Environment.NewLine +
                @"845 0 < " + Environment.NewLine +
                @"920 0.0251889 < " + Environment.NewLine +
                @"1190 0.0251889 < " + Environment.NewLine +
                @"1240 0.0534005 < " + Environment.NewLine +
                @"1290 0.088665 < " + Environment.NewLine +
                @"1350 0.126952 < " + Environment.NewLine +
                @"1415 0.166247 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" bl 1 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 mp 86400 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl ";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual(SobekControllerType.HydraulicController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekController.SobekControllerParameterType);
            Assert.IsNotNull(sobekController.LookUpTable);
            Assert.AreEqual(9, sobekController.LookUpTable.Rows.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobek212File()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"ndb_controllertriggerfiles\Control.def");
            var sobekControlllers = new SobekControllerReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(43, sobekControlllers.Length);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobekREFile()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"ndb_controllertriggerfiles\DEFSTR.4");
            var sobekControllers = new SobekControllerReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(42, sobekControllers.Length);
        }

        [Test]
        public void TimeControllerSobekHelp1()
        {
            var fileText =
                @"CNTL id '24' nm 'RivCntrl' ct 0 ca 2 ac 1 cf 1 ta 1 1 0 0 gi '2' '3' '-1' '-1' ao 1 1 1 1 mc 0.0046 ti tv PDIN 0 0 pdin" + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1999/01/02;04:20:00' 0 < " + Environment.NewLine +
                @"'1999/01/02;04:28:00' 0.57 < " + Environment.NewLine +
                @"'1999/01/02;13:14:00' 0.57 < " + Environment.NewLine +
                @"'1999/01/02;13:17:00' 0 < " + Environment.NewLine +
                @"'1999/01/02;17:01:00' 0 < " + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_24", sobekController.Id);
            Assert.AreEqual("RivCntrl", sobekController.Name);
            Assert.AreEqual(SobekControllerType.TimeController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekController.SobekControllerParameterType);
            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual("TRG_2", sobekController.Triggers[0].Id);
            Assert.AreEqual(true, sobekController.Triggers[0].Active);
            Assert.AreEqual(true, sobekController.Triggers[0].And);
            Assert.AreEqual(0.0046, sobekController.MaxChangeVelocity);
            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);
            Assert.IsNotNull(sobekController.TimeTable);
            Assert.AreEqual(5, sobekController.TimeTable.Rows.Count);
        }

        [Test]
        public void TimeControllerSobekHelp2()
        {
            var fileText =
                @"CNTL id '60' nm 'UrbRur' ct 0 ac 1 ca 2 cf 1 mc 0 bl 1 ti tv " + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'2004/11/26;00:00:00' 2.2 < " + Environment.NewLine +
                @"'2004/11/26;01:00:00' 2.33 < " + Environment.NewLine +
                @"'2004/11/26;02:00:00' 2.12 < " + Environment.NewLine +
                @"'2004/11/26;03:00:00' 2.2 < " + Environment.NewLine +
                @"'1999/01/02;17:01:00' 0 < " + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_60", sobekController.Id);
            Assert.AreEqual("UrbRur", sobekController.Name);
            Assert.AreEqual(SobekControllerType.TimeController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekController.SobekControllerParameterType);
            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(0, sobekController.MaxChangeVelocity);
            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.IsNotNull(sobekController.TimeTable);
            Assert.AreEqual(5, sobekController.TimeTable.Rows.Count);
        }

        [Test]
        public void TimeControllerSobekHelp3()
        {
            var fileText =
                @"CNTL id '##9' nm '2DdambrkContr' ct 0 ca 5 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 mc 1110 ti tv PDIN 0 1 '365;00:00:00' pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'2000/01/01;00:00:00' 5 < " + Environment.NewLine +
                @"'2000/01/01;01:00:00' 4 < " + Environment.NewLine +
                @"'2000/01/01;03:30:00' 3 < " + Environment.NewLine +
                @"'2000/01/01;06:00:00' 1 < " + Environment.NewLine +
                @"tble cntl";
            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_##9", sobekController.Id);
            Assert.AreEqual("2DdambrkContr", sobekController.Name);
            Assert.AreEqual(SobekControllerType.TimeController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.BottomLevel2DGridCell, sobekController.SobekControllerParameterType);
            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(1110d, sobekController.MaxChangeVelocity);
            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Periodic, sobekController.ExtrapolationType);
            Assert.IsNotNull(sobekController.TimeTable);
            Assert.AreEqual(4, sobekController.TimeTable.Rows.Count);
        }

        [Test]
        public void HydraulicControllerSobekHelp1()
        {
            var fileText =
                @"CNTL id 'P_1' nm 'P_Amerongen' ct 1 ca 0 ac 1 cf 1 ta 1 0 0 0 gi '5' '-1' '-1' '-1' " + Environment.NewLine +
                @"ao 1 1 1 1 cp 1 mp 0 ml 'l_88' '61' '-1' '-1' '-1' hc ht PDIN 0 0 pdin" + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"-999 5.71 <" + Environment.NewLine +
                @"25 5.71 <" + Environment.NewLine +
                @"261 4.61 <" + Environment.NewLine +
                @"504 3.48 <" + Environment.NewLine +
                @"641 -1 <" + Environment.NewLine +
                @"6000 -1 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_P_1", sobekController.Id);
            Assert.AreEqual("P_Amerongen", sobekController.Name);
            Assert.AreEqual(SobekControllerType.HydraulicController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.CrestLevel, sobekController.SobekControllerParameterType);
            Assert.IsTrue(sobekController.IsActive);

            Assert.IsNotNull(sobekController.SpecificProperties);
            Assert.AreEqual(typeof(SobekHydraulicControllerProperties),sobekController.SpecificProperties.GetType());
            Assert.AreEqual(0,((SobekHydraulicControllerProperties)sobekController.SpecificProperties).TimeLag);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual("TRG_5", sobekController.Triggers[0].Id);
            Assert.AreEqual(true, sobekController.Triggers[0].Active);
            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);

            Assert.AreEqual("l_88", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.Discharge, sobekController.MeasurementLocationParameter);

            Assert.IsNotNull(sobekController.LookUpTable);
            Assert.AreEqual(6, sobekController.LookUpTable.Rows.Count);
        }

        [Test]
        public void HydraulicControllerSobekHelp2()
        {
            var fileText =
                @"CNTL id '60' nm 'ExampleCntrl' ct 1 ca 2 cf 1 ml 'l_88'  cp 1 bl 1 hc ht 1" + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"1 3.2 <" + Environment.NewLine +
                @"2 4.2 <" + Environment.NewLine +
                @"2.5 4.45 <" + Environment.NewLine +
                @"3 4.5 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_60", sobekController.Id);
            Assert.AreEqual("ExampleCntrl", sobekController.Name);
            Assert.AreEqual(SobekControllerType.HydraulicController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(false, sobekController.Triggers[1].Active);
            Assert.AreEqual(false, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);

            Assert.AreEqual("l_88", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.Discharge, sobekController.MeasurementLocationParameter);

            Assert.IsNotNull(sobekController.LookUpTable);
            Assert.AreEqual(4, sobekController.LookUpTable.Rows.Count);
        }

        [Test]
        public void IntervalControllerSobekHelp1()
        {
            var fileText =
                @"CNTL id '18066' nm 'Arjan' ct 2 ca 1 ac 1 cf 1 ta 1 0 1 0 gi '5' '-1' '4' '-1' ao 1 1 1 1 " + Environment.NewLine +
                @"cp 1 ml '89' ui 1.5 ua 2.5 cn 1 cv 0.05 dt 1 pe 20 di 200 da 205 sp tc 1 PDIN 0 0 pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1999/11/30;00:00:00' 1.5 <" + Environment.NewLine +
                @"'1999/11/30;01:00:00' 1 <" + Environment.NewLine +
                @"'1999/11/30;02:00:00' 1.25 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_18066", sobekController.Id);
            Assert.AreEqual("Arjan", sobekController.Name);
            Assert.AreEqual(SobekControllerType.IntervalController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.CrestWidth, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual("TRG_5", sobekController.Triggers[0].Id);
            Assert.AreEqual(true, sobekController.Triggers[0].Active);
            Assert.AreEqual(false, sobekController.Triggers[1].Active);
            Assert.AreEqual(true, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);

            Assert.AreEqual("89", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.Discharge, sobekController.MeasurementLocationParameter);

            Assert.IsNotNull(sobekController.TimeTable);
            Assert.AreEqual(3, sobekController.TimeTable.Rows.Count);

            //specific properties
            var specificProperties = (SobekIntervalControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(1.5, specificProperties.USminimum);
            Assert.AreEqual(2.5, specificProperties.USmaximum);
            Assert.AreEqual(0.05, specificProperties.ControlVelocity); //cv
            Assert.AreEqual(IntervalControllerDeadBandType.PercentageDischarge, specificProperties.DeadBandType);
            Assert.AreEqual(20.0, specificProperties.DeadBandPecentage); //pe

            Assert.AreEqual(IntervalControllerIntervalType.Variable, specificProperties.ControllerIntervalType);
            Assert.AreEqual(0.0, specificProperties.FixedInterval); //du is not defined in fileText
        }

        [Test]
        public void IntervalControllerSobekHelp2()
        {
            var fileText =
                @"CNTL id '60' nm 'IntervCntrl' ct 2 ac 1 ca 2 cf 1 ml '89' cp 0 ui 3.2 ua 2.7 cn 1 du 0.2 cv 0.025 dt 0 d_0.05 bl 1 sp tc 0 1.23 0 cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_60", sobekController.Id);
            Assert.AreEqual("IntervCntrl", sobekController.Name);
            Assert.AreEqual(SobekControllerType.IntervalController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(false, sobekController.Triggers[1].Active);
            Assert.AreEqual(false, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual("89", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.WaterLevel, sobekController.MeasurementLocationParameter);

            Assert.IsNull(sobekController.TimeTable);

            //specific properties
            var specificProperties = (SobekIntervalControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(3.2, specificProperties.USminimum);
            Assert.AreEqual(2.7, specificProperties.USmaximum);
            Assert.AreEqual(IntervalControllerDeadBandType.Fixed, specificProperties.DeadBandType);
            Assert.AreEqual(0.025, specificProperties.ControlVelocity);
            Assert.AreEqual(0.05, specificProperties.DeadBandFixedSize);
            Assert.AreEqual(1.23, specificProperties.ConstantSetPoint);

            Assert.AreEqual(IntervalControllerIntervalType.Fixed, specificProperties.ControllerIntervalType);
            Assert.AreEqual(0.2, specificProperties.FixedInterval);
        }

        [Test]
        public void PIDControllerSobekHelp1()
        {
            var fileText =
                @"CNTL id '18067' nm 'PIDCntrl' ct 3 ca 0 ac 1 cf 1 ta 1 1 1 0 gi '2' '7' '4' '-1' ao 1 1 1 1 cp 0  ml '116' u0 1.5 ui 1 ua 2.5 va 0.5 pf 1.5 if 0.05 df 0.5 sp tc 0 1.5 0 cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_18067", sobekController.Id);
            Assert.AreEqual("PIDCntrl", sobekController.Name);
            Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.CrestLevel, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual("TRG_2", sobekController.Triggers[0].Id);
            Assert.AreEqual(true, sobekController.Triggers[0].Active);

            Assert.AreEqual("116", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.WaterLevel, sobekController.MeasurementLocationParameter);

            Assert.IsNull(sobekController.TimeTable);

            //specific properties
            var specificProperties = (SobekPidControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(1.5, specificProperties.USinitial);
            Assert.AreEqual(1.0, specificProperties.USminimum);
            Assert.AreEqual(2.5, specificProperties.USmaximum);
            Assert.AreEqual(0.5, specificProperties.MaximumSpeed);
            Assert.AreEqual(1.5, specificProperties.KFactorProportional);
            Assert.AreEqual(0.05, specificProperties.KFactorIntegral);
            Assert.AreEqual(0.5, specificProperties.KFactorDifferential);
            Assert.AreEqual(1.5, specificProperties.ConstantSetPoint);
        }

        [Test]
        public void PIDControllerSobekHelp2()
        {
            var fileText =
                @"CNTL id '60' nm 'PIDCntrl' ct 3 ac 1 ca 2 cf 1 ml '116' cp 0 ui 0.7 ua 1.7 u0 1.25 pf 0.56 if 0.04 df 0.25 va 0.01 bl 1 sp tc 1" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1999/11/30;00:00:00' 1.25 <" + Environment.NewLine +
                @"'1999/11/30;01:00:00' 1.25 <" + Environment.NewLine +
                @"'1999/11/30;02:00:00' 1.75 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_60", sobekController.Id);
            Assert.AreEqual("PIDCntrl", sobekController.Name);
            Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.GateHeight, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(false, sobekController.Triggers[1].Active);
            Assert.AreEqual(false, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual("116", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.WaterLevel, sobekController.MeasurementLocationParameter);

            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);

            Assert.IsNotNull(sobekController.TimeTable);
            Assert.AreEqual(3, sobekController.TimeTable.Rows.Count);

            //specific properties
            var specificProperties = (SobekPidControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(1.25, specificProperties.USinitial);
            Assert.AreEqual(0.7, specificProperties.USminimum);
            Assert.AreEqual(1.7, specificProperties.USmaximum);
            Assert.AreEqual(0.01, specificProperties.MaximumSpeed);
            Assert.AreEqual(0.56, specificProperties.KFactorProportional);
            Assert.AreEqual(0.04, specificProperties.KFactorIntegral);
            Assert.AreEqual(0.25, specificProperties.KFactorDifferential);

        }

        [Test]
        public void ReadPIDControllerTestBrench172()
        {
            var fileText =
                @"CNTL id '##2' nm 'Controller 1' ct 3 ca 0 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 0 ml '8' u0 4 ui 3 ua 5.5 va 4 pf -2.5 if -5 df 0 sp tc  1 PDIN 1 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'2003/01/02;00:00:00' 3 < " + Environment.NewLine +
                @"'2003/01/02;06:00:00' 4 < " + Environment.NewLine +
                @"'2003/01/02;12:00:00' 3 < " + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_##2", sobekController.Id);
            Assert.AreEqual("Controller 1", sobekController.Name);
            Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);

            Assert.AreEqual(InterpolationType.Constant, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);


        }

        [Test]
        public void RelativeTimeControllerSobekHelp()
        {
            var fileText =
                @"CNTL id 'P_0' nm 'P_Driel' ct 4 ca 0 ac 1 cf 1 ta 0 1 0 0 gi '-1' '3' '-1' '-1' ao 1 1 1 1 mc 5 mp 0 ti vv PDIN 0 0 pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 1.5 <" + Environment.NewLine +
                @"240 2 <" + Environment.NewLine +
                @"600 1.5 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_P_0", sobekController.Id);
            Assert.AreEqual("P_Driel", sobekController.Name);
            Assert.AreEqual(SobekControllerType.RelativeTimeController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.CrestLevel, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(true, sobekController.Triggers[1].Active);
            Assert.AreEqual(false, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual(5.0, sobekController.MaxChangeVelocity);

            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);

            Assert.IsNotNull(sobekController.LookUpTable);
            Assert.AreEqual(3, sobekController.LookUpTable.Rows.Count);
        }

        [Test]
        public void RelativeFromValueControllerSobekHelp()
        {
            var fileText =
                @"CNTL  id 'P_0' nm 'P_Driel' ct 5 ca 0 ac 1 cf 1 ta 0 1 0 0 gi -1 3 -1 -1 ao 1 1 1 1 mc 5 mp 0 ti vv PDIN 0 0 pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 1.5 <" + Environment.NewLine +
                @"240 2 <" + Environment.NewLine +
                @"600 1.5 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_P_0", sobekController.Id);
            Assert.AreEqual("P_Driel", sobekController.Name);
            Assert.AreEqual(SobekControllerType.RelativeFromValueController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.CrestLevel, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(false, sobekController.Triggers[0].Active);
            Assert.AreEqual(true, sobekController.Triggers[1].Active);
            Assert.AreEqual(false, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual(5.0, sobekController.MaxChangeVelocity);

            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, sobekController.ExtrapolationType);

            Assert.IsNotNull(sobekController.LookUpTable);
            Assert.AreEqual(3, sobekController.LookUpTable.Rows.Count);
        }

        [Test]
        public void PIDController_ConstantValue_Sobek212()
        {
            var fileText = @"CNTL id '02_1' nm 'Linn_PID_1' ct 3 ca 0 ac 1 cf 1 ta 1 1 0 0 gi '5418' '5578308' '-1' '-1' ao 1 1 1 1 cp 0 ml '008_0' ui 1748 ua 2085 u0 20 pf 25 if 6 df 0 va 3 sp tc 0 2085 cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_02_1", sobekController.Id);
            Assert.AreEqual("Linn_PID_1", sobekController.Name);
            Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);
            Assert.AreEqual(SobekControllerParameter.CrestLevel, sobekController.SobekControllerParameterType);

            Assert.IsTrue(sobekController.IsActive);

            Assert.AreEqual(4, sobekController.Triggers.Count);
            Assert.AreEqual(true, sobekController.Triggers[0].Active);
            Assert.AreEqual(true, sobekController.Triggers[1].Active);
            Assert.AreEqual(false, sobekController.Triggers[2].Active);
            Assert.AreEqual(false, sobekController.Triggers[3].Active);

            Assert.AreEqual("008_0", sobekController.MeasurementStationId);
            Assert.AreEqual(SobekMeasurementLocationParameter.WaterLevel, sobekController.MeasurementLocationParameter);

            Assert.IsNull(sobekController.TimeTable);

            //specific properties
            var specificProperties = (SobekPidControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(20, specificProperties.USinitial);
            Assert.AreEqual(1748, specificProperties.USminimum);
            Assert.AreEqual(2085, specificProperties.USmaximum);
            Assert.AreEqual(3, specificProperties.MaximumSpeed);
            Assert.AreEqual(25, specificProperties.KFactorProportional);
            Assert.AreEqual(6, specificProperties.KFactorIntegral);
            Assert.AreEqual(0, specificProperties.KFactorDifferential);

            //constant value
            Assert.AreEqual(2085d, specificProperties.ConstantSetPoint);
        }

        [Test]
        public void ImportProblemWithNBAnalysisDotLit()
        {
            var fileText =
                @"CNTL id 'KST504' nm 'PIDDeValle' ct 3 ac 1 ca 0 cf 1 ml '197' cp 0 ui -1.2 ua -0.9 u0 -1 pf 1 if 0 df 0 va 0.01 bl 1 sp tc 0  -1 0  cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();

            Assert.AreEqual(-1.0, ((SobekPidControllerProperties)sobekController.SpecificProperties).ConstantSetPoint);
        }

        [Test]
        public void ReadExtraSpaceAfterSpTc()
        {
            // extra space following sp tc
            const string source = @"CNTL id '##4' nm 'Eefde_aflaatwerk_oud_dynamisch' ct 3 ca 2 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 0 ml 'Eefde_aflaatwerk_oud_debiet' u0 7 ui 7 ua 8.67 va 0.01 pf -4 if 0 df 2 sp tc  0 10 0 cntl";
            var sobekController = new SobekControllerReader().Parse(source).FirstOrDefault();
            Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);
            var specificProperties = (SobekPidControllerProperties)sobekController.SpecificProperties;
            Assert.IsFalse(double.IsNaN(specificProperties.ConstantSetPoint));
            //Assert.AreEqual(-1.0, ((SobekPidControllerProperties)sobekController.SpecificProperties).ConstantSetPoint);
        }

        [Test]
        public void ImportExceptionTestBrench_166_space_after_d_()
        {
            var fileText =
                @"CNTL idid '##1' nm 'gate control' ct 2 ca 2 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 0 ml '8' ui 5 ua 4.1 cn 0 du 0.001 dt 0 d_ 0.001 sp tc  1 PDIN 0 0  pdin " + Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'2003/01/02;00:00:00' 3 <  " + Environment.NewLine +
                @"'2003/01/03;12:00:00' 3.5 <  " + Environment.NewLine +
                @"'2003/01/05;00:00:00' 3 < " + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();
            Assert.AreEqual(SobekControllerType.IntervalController, sobekController.ControllerType);
            var specificProperties = (SobekIntervalControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(0.001, specificProperties.DeadBandFixedSize);
        }

        [Test]
        public void ImportPIDWithConstantValue()
        {
             var fileText = @"CNTL id '5577940' nm 'Lith_WKC' ta 1 0 0 0 gi '5577719' '-1' '-1' '-1' ao 1 1 1 1 ct 3 ac 1 ca 0 cf 1 cb '016' '-1' '-1' '-1' '-1' cl 34249 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 mp 0 mc 0 sp tc 0 4.9 9.9999e+009 ui 3 ua 4.85 u0 4.5 pf 0.25 if 1.5 df 0 va 0.03 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";
             var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();
             Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);
             var specificProperties = (SobekPidControllerProperties)sobekController.SpecificProperties;
             Assert.AreEqual(4.9, specificProperties.ConstantSetPoint);
        }

        [Test]
        public void ImportTimeRuleFromValue()
        {
            var fileText =
                @"CNTL id '5573291' nm 'Bosschebroek' ta 1 0 0 0 gi '5573289' '-1' '-1' '-1' ao 1 1 1 1 ct 5 ac 1 ca 2 cf 1 cb '-1' '-1' '-1' '-1' '-1' cl 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti vv 'Rel Time Controller' PDIN 0 0 '' pdin CLTT 'Value (s)' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE  " + Environment.NewLine +
                @"0 99 < " + Environment.NewLine +
                @"tble " + Environment.NewLine +
                @" mp 0 mc 0 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();
            Assert.AreEqual(SobekControllerType.RelativeFromValueController, sobekController.ControllerType);
        }

        [Test]
        public void HydraulicRuleOnFlowDirection()
        {
            var fileText =
                @"CNTL id '##4' nm 'Flow direction Cntrl 1' ct 1 ca 1 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 4 ml '7' '-1' '-1' '-1' '-1' ps 40 ns 50 cntl" + Environment.NewLine +
                @"CNTL id '##5' nm 'Velocity Cntrl 2' ct 1 ca 0 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 3 ml '7' '-1' '-1' '-1' '-1' hc ht PDIN 0 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"-.4 5 < " + Environment.NewLine +
                @"-.2 5.2 < " + Environment.NewLine +
                @"0 5.4 < " + Environment.NewLine +
                @".2 5.2 < " + Environment.NewLine +
                @".4 5 < " + Environment.NewLine +
                @"tble cntl";

            var sobekControllers = new SobekControllerReader().Parse(fileText).ToList();
            Assert.AreEqual(2, sobekControllers.Count);
            Assert.AreEqual(SobekControllerType.HydraulicController ,sobekControllers[0].ControllerType);
            Assert.AreEqual(SobekControllerType.HydraulicController, sobekControllers[1].ControllerType);

            var controllerOnFlowDirection = sobekControllers[0];
            Assert.AreEqual(40.0,controllerOnFlowDirection.PositiveStream);
            Assert.AreEqual(50.0,controllerOnFlowDirection.NegativeStream);

        }

        [Test]
        public void ReadHyddraulicRuleFromTestBenchCase175()
        {
            var fileText =
                @"CNTL id '##2' nm 'Weir crest width control' ct 1 ca 1 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 5 si '5##1' hc ht PDIN 0 0  pdin" +
                Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @" -20000 40 < " + Environment.NewLine +
                @" 0 30 < " + Environment.NewLine +
                @" 3000 35 < " + Environment.NewLine +
                @" 6000 40 < " + Environment.NewLine +
                @" 9000 45 < " + Environment.NewLine +
                @" 12000 50 < " + Environment.NewLine +
                @"15000 55 < " + Environment.NewLine +
                @" 18000 60 < " + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("5~~1",sobekController.StructureId);
        }

        [Test]
        public void ReadIntervalRuleFromTestBench169()
        {
            var fileText =
                @"CNTL id '##1' nm 'gate control' ct 2 ca 2 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 0 ml '8' ui 5 ua 4.1 cn 1 cv 0.000016667 dt 0 d_ 0.001 sp tc  0 3 0 cntl";

            var sobekController = new SobekControllerReader().Parse(fileText).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.IsNotNull(sobekController.SpecificProperties);
            Assert.AreEqual(3.0,((SobekIntervalControllerProperties)sobekController.SpecificProperties).ConstantSetPoint);
        }

        [Test]
        public void ReadExtendedCharacterset()
        {
            var text =
                @"CNTL id 'AL1_10301' nm 'AL1_retgebied_km669.3_Köln-Langel_1' ta 1 1 0 0 gi 'AL1_103011' 'AL1_103012' '-1' '-1' ao 1 0 1 1 ct 5 ac 1 ca 1 cf 1 cb '-1' '-1' '-1' '-1' '-1' cl 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti vv 'Rel Time Controller' PDIN 0 0 '' pdin CLTT 'Value (s)' 'Crest Width [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 200 < " + Environment.NewLine +
                @"3600 200 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" mp 0 mc 0 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 'Hydraulic Controller' PDIN 0 0 '' pdin CLTT 'Water Level [m]' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";


            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_AL1_10301", sobekController.Id);
            Assert.AreEqual("AL1_retgebied_km669.3_Köln-Langel_1", sobekController.Name);
        }

        [Test]
        public void ReadRelativeTimeRuleInterpolation()
        {
            var text =
                @"CNTL id '5576958' nm 'bosschebroek2' ta 1 0 0 0 gi '5576956' '-1' '-1' '-1' ao 1 1 1 1 ct 5 ac 1 ca 2 cf 1 cb '-1' '-1' '-1' '-1' '-1' cl 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti vv 'Rel Time Controller' PDIN 0 0 '' pdin CLTT 'Value (s)' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 0.3 < " + Environment.NewLine +
                @"3600 1 < " + Environment.NewLine +
                @"7200 10 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"mp 0 mc 0 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";

            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_5576958", sobekController.Id);
            Assert.AreEqual(InterpolationType.Linear, sobekController.InterpolationType);
        }

        [Test]
        public void ReadIntervalControllerWithExtrapolation()
        {
            var text = @"CNTL id '##1' nm 'gate control' ct 2 ca 2 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 0 ml '8' ui 5 ua 4.1 cn 0 du 0.001 dt 0 d_ 0.001 sp tc  1 PDIN 0 1 '7;00:00:00' pdin" + Environment.NewLine +
            @"TBLE" + Environment.NewLine +
            @"'2003/01/01;00:00:00' 3 < " + Environment.NewLine +
            @"'2003/01/02;00:00:00' 3 < " + Environment.NewLine +
            @"'2003/01/03;12:00:00' 3.5 < " + Environment.NewLine +
            @"'2003/01/05;00:00:00' 3 < " + Environment.NewLine +
            @"'2003/01/08;00:00:00' 3 < " + Environment.NewLine +
            @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_##1", sobekController.Id);
            Assert.AreEqual(ExtrapolationType.Periodic, sobekController.ExtrapolationType);
            Assert.AreEqual("'7;00:00:00'", sobekController.ExtrapolationPeriod);
        }

        [Test]
        //TOOLS-5576 
        public void HKV_REModel_Vecht_2_TwoTimeSeries_WhichOne()
        {
            var text =
                @"CNTL id '15883' nm 'Hardenberg_klep' ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 ct 2 ac 1 ca 0 cf 1 cb '20' '-1' '-1' '-1' '-1' cl 6950 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Crest Level [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"'1997/09/17;00:00:00' 6.79 < " + Environment.NewLine +
                @"'1997/09/17;01:00:00' 6.77 < " + Environment.NewLine +
                @"'1997/09/17;02:00:00' 6.77 < " + Environment.NewLine +
                @"'1997/09/17;03:00:00' 6.77 < " + Environment.NewLine +
                @"'1997/09/17;04:00:00' 6.77 < " + Environment.NewLine +
                @"'1997/09/17;05:00:00' 6.77 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" mp 0 mc 0 sp tc 1 9.9999e+009 9.9999e+009 'Steer Table' PDIN 0 0 '' pdin CLTT 'Time' 'Water Level [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"'1997/10/15;07:00:00' 7 < " + Environment.NewLine +
                @"'1997/10/15;08:00:00' 7 < " + Environment.NewLine +
                @"'1997/10/15;09:00:00' 7 < " + Environment.NewLine +
                @"'1997/10/15;10:00:00' 7 < " + Environment.NewLine +
                @"'1997/10/15;11:00:00' 7 < " + Environment.NewLine +
                @"'1997/10/15;12:00:00' 7 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" ui 5.21 ua 7 u0 6.8 pf 0.5 if 0.5 df 1 va 0.01 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 1 du 9.9999e+009 cv -3e-005 dt 0 pe 9.9999e+009 d_ 0.1 di 9.9999e+009 da 9.9999e+009 cntl";

            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_15883", sobekController.Id);

            //first table or second?????? Based on column name 'Crest Level [m]' the first one???
            Assert.AreEqual(7.0, (double)sobekController.TimeTable.Rows[0][1]);

            var properties = (SobekIntervalControllerProperties) sobekController.SpecificProperties;

            //extra check
            Assert.AreEqual(7.0, properties.USmaximum);
            Assert.AreEqual(5.21, properties.USminimum);
            Assert.AreEqual(-3e-005, properties.ControlVelocity); //cv
        }

        [Test]
        public void RE_Model_Rijntakken_PID_Controller_With_Table_Not_In_Use()
        {
            var text =
                @"CNTL id '02' nm 'Amero_PID' ta 1 1 0 0 gi '2457' '2455' '-1' '-1' ao 1 1 1 1 ct 3 ac 1 ca 2 cf 1 cb '005' '-1' '-1' '-1' '-1' cl 38152 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 mp 0 mc 0.00125 sp tc 0 6 9.9999e+009 ui 0 ua 16.7 u0 0 pf -0.5 if -0.5 df 0 va 1 si '-1' hc ht 5 9.9999e+009 9.9999e+009 'Hydraulic Controller' PDIN 0 0 '' pdin CLTT 'Discharge [m3/s]' 'Crest Level [m]' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 5.71 < " + Environment.NewLine +
                @"25 5.71 < " + Environment.NewLine +
                @"93 5.29 < " + Environment.NewLine +
                @"142 5.07 < " + Environment.NewLine +
                @"185 4.9 < " + Environment.NewLine +
                @"225 4.75 < " + Environment.NewLine +
                @"261 4.64 < " + Environment.NewLine +
                @"294 4.5 < " + Environment.NewLine +
                @"325 4.39 < " + Environment.NewLine +
                @"352 4.3 < " + Environment.NewLine +
                @"398 4.09 < " + Environment.NewLine +
                @"430 4.01 < " + Environment.NewLine +
                @"466 3.84 < " + Environment.NewLine +
                @"504 3.66 < " + Environment.NewLine +
                @"545 3.47 < " + Environment.NewLine +
                @"591 3.4 < " + Environment.NewLine +
                @"681 -1 < " + Environment.NewLine +
                @"6000 -1 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" bl 1 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";

            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_02", sobekController.Id);
            Assert.AreEqual(SobekControllerType.PIDController, sobekController.ControllerType);


            var properties = (SobekPidControllerProperties)sobekController.SpecificProperties;
            Assert.AreEqual(-0.5, properties.KFactorProportional);
            Assert.AreEqual(-0.5, properties.KFactorIntegral);
            Assert.AreEqual(0, properties.KFactorDifferential);
        }

        [Test]
        public void RE_Model_Rijntakken_Time_Controller_With_A_Lot_Of_Old_Data()
        {
            var text =
                @"CNTL id '2239' nm 'Driel_oml' ta 1 0 0 0 gi '2241' '-1' '-1' '-1' ao 1 1 1 1 ct 0 ac 1 ca 2 cf 1 cb '005' '-1' '-1' '-1' '-1' cl 12038 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE  " +
                Environment.NewLine +
                @"'1980/01/01;00:00:00' 0.65 <  " + Environment.NewLine +
                @"'2010/01/01;00:00:00' 0.65 <  " + Environment.NewLine +
                @"tble " + Environment.NewLine +
                @" mp 0 mc 0 sp tc 1 40 9.9999e+009 'Steer Table' PDIN 0 0 '' pdin CLTT 'Time' 'Water Level [m]' cltt CLID '(null)' '(null)' clid TBLE  " +
                Environment.NewLine +
                @"'2003/07/01;00:00:00' 7.71 <  " + Environment.NewLine +
                @"'2003/10/15;23:00:00' 8.25 <  " + Environment.NewLine +
                @"tble " + Environment.NewLine +
                @" ui 0.5 ua 1 u0 0 pf -0.5 if -1.5 df 0 va 0.001 si '-1' hc ht 5 9.9999e+009 9.9999e+009 'Hydraulic Controller' PDIN 0 0 '' pdin CLTT 'Discharge [m3/s]' 'Crest Level [m]' cltt CLID '(null)' '(null)' clid TBLE  " +
                Environment.NewLine +
                @"tble " + Environment.NewLine +
                @" bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du -2 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 20 di 9.9999e+009 da 9.9999e+009 cntl";

            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_2239", sobekController.Id);
            Assert.AreEqual(SobekControllerType.TimeController, sobekController.ControllerType);
            Assert.IsNotNull(sobekController.TimeTable);

            Assert.AreEqual(2,sobekController.TimeTable.Rows.Count);
            Assert.AreEqual(0.65, sobekController.TimeTable.Rows[0][1]);
            Assert.AreEqual(0.65, sobekController.TimeTable.Rows[1][1]);
        }

        [Test]
        public void MinimumPeriod()
        {
            var text =
                @"CNTL id '##1' nm 'Relative Time' ct 4 ca 0 ac 1 cf 1 ta 1 0 0 0 gi '##1' '-1' '-1' '-1' ao 1 1 1 1 mc 0 mp 172800 ti vv PDIN 0 0 pdin" +
                Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 5 <" + Environment.NewLine +
                @"21600 6 <" + Environment.NewLine +
                @"43200 5 <" + Environment.NewLine +
                @"tble cntl";

            var sobekController = new SobekControllerReader().Parse(text).FirstOrDefault();
            Assert.IsNotNull(sobekController);
            Assert.AreEqual("CTR_##1", sobekController.Id);
            Assert.AreEqual(172800, sobekController.MinimumPeriod);
        }

    }
}

