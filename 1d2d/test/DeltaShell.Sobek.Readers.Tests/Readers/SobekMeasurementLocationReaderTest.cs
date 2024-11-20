using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekMeasurementLocationReaderTest
    {
        [Test]
        public void ReadFromStringSobek212Format()
        {
            var fileText = @"ME_1.0" + Environment.NewLine +
                       @"MEAS id '28_200' nm '28_200_name' ObID 'SBK_MEASSTAT' ci 'R_28' lc 199.999999999806 meas" +
                       Environment.NewLine +
                       @"MEAS id 'P_003_0' nm 'P_003_0' ObID 'SBK_MEASSTAT' ci 'R_P_003' lc 9.99999999979503 meas" +
                       Environment.NewLine +
                       @"MEAS id 'P_022_0' nm 'P_022_0' ObID 'SBK_MEASSTAT' ci 'R_P_022' lc 10.0000000000753 meas" +
                       Environment.NewLine +
                       @"MEAS id 'P_P_P_5686151_0' nm 'P_P_P_5686151_0' ObID 'SBK_MEASSTAT' ci 'R_P_P_P_5686151' lc 9.99999999981218 meas";

            var measurementLocations = new SobekMeasurementLocationReader().Parse(fileText).ToArray();

            Assert.AreEqual(4, measurementLocations.Length);
            Assert.AreEqual("28_200",measurementLocations[0].Id);
            Assert.AreEqual("28_200_name",measurementLocations[0].Name);
            Assert.AreEqual("R_28",measurementLocations[0].BranchId);
            Assert.AreEqual(199.99,measurementLocations[0].Chainage,0.01);
        }

        [Test]
        public void ReadFromStringSobekTriggerFormat()
        {
            var fileText = @"TRGR id '0' nm 'Hvl_OpenVerval_Trigger' ty 1 tp 1 tb '-1' tl 9.9999e+009 ts '13' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr" + Environment.NewLine +
                            @"TRGR id '1' nm 'Hvl_SluitVerval_Trigger' ty 1 tp 1 tb '-1' tl 9.9999e+009 ts '13' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 0 0 <" + Environment.NewLine + 
                            @"tble" + Environment.NewLine +
                            @"trgr" + Environment.NewLine +
                            @"TRGR id '4' nm 'HY-SLUIT-TR' ty 1 tp 0 tb '28' tl 200 ts '-1' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 1.8 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr" + Environment.NewLine +
                            @"TRGR id '5' nm 'HY-OPEN-TR' ty 1 tp 0 tb '28' tl 200 ts '24' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 0 1.8 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr" + Environment.NewLine +
                            @"TRGR id '18976' nm 'Svk_tijdreeks' ty 0 tp 0 tb '-1' tl 9.9999e+009 ts '-1' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1991/01/01;00:00:00' 0 -1 -1 9.9999e+009 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr";

            //tb '28' tl 200 = branch and location

            var measurementLocations = new SobekMeasurementLocationReader().Parse(fileText).ToArray();

            Assert.AreEqual(5, measurementLocations.Length);
            Assert.AreEqual("28_200", measurementLocations[2].Id);
            Assert.AreEqual("28_200", measurementLocations[2].Name);
            Assert.AreEqual("28", measurementLocations[2].BranchId);
            Assert.AreEqual(200, measurementLocations[2].Chainage, 0.01);

            Assert.AreEqual(2, measurementLocations.Select(m => m.Id).Distinct().Count()); //Duplicate locations: "tb -1 tl 9.9999e+009" and "tb 28 tl 200"
        }

        [Test]
        public void ReadFromStringSobekControllerFormat()
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
                            @" mp 0 mc 0 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl" + Environment.NewLine +
                            @"CNTL id '22128' nm 'Hvl_schuif1' ta 1 0 0 0 gi '0' '1' '-1' '-1' ao 1 1 1 1 ct 1 ac 1 ca 2 cf 0 cb 'P_003' 'P_003' '-1' '-1' '-1' cl 0 200 9.9999e+009 9.9999e+009 9.9999e+009 cp 1 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'2001/12/14;16:55:00' 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @" bl 1 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl";

            //tb '28' tl 200 = branch and location

            var measurementLocations = new SobekMeasurementLocationReader().Parse(fileText).ToArray();

            Assert.AreEqual(3, measurementLocations.Length);
            Assert.AreEqual("28_0", measurementLocations[0].Id);
            Assert.AreEqual("28_0", measurementLocations[0].Name);
            Assert.AreEqual("28", measurementLocations[0].BranchId);
            Assert.AreEqual(0, measurementLocations[0].Chainage, 0.01);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobek212File()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\NETWORK.ME");
            var measurementLocations = new SobekMeasurementLocationReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(4, measurementLocations.Length);
            Assert.AreEqual("28_200", measurementLocations[0].Id);
            Assert.AreEqual("28_200", measurementLocations[0].Name);
            Assert.AreEqual("R_28", measurementLocations[0].BranchId);
            Assert.AreEqual(199.99, measurementLocations[0].Chainage, 0.01);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobekREFileDEFSTR4()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\DEFSTR.4");
            var measurementLocations = new SobekMeasurementLocationReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(19, measurementLocations.Length);

            Assert.AreEqual(3, measurementLocations.Select(m => m.Id).Distinct().Count()); //3 times same location
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobekREFileDEFSTR5()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\DEFSTR.5");
            var measurementLocations = new SobekMeasurementLocationReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(28, measurementLocations.Length);

            Assert.AreEqual(4, measurementLocations.Select(m => m.Id).Distinct().Count()); //3 times same location
        }
    }
}
