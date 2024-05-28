using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekTriggerReaderTest
    {
        [Test]
        public void ReadFromStringSobek212Format()
        {
            var fileText = @"TRGR id '0' nm 'Hvl_OpenVerval_Trigger' ty 1 t1 0 tp 1 ts 'S_13' tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid "  + Environment.NewLine +
                            @"TBLE "  + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 0 <"  + Environment.NewLine +
                            @"tble trgr"  + Environment.NewLine +
                            @"TRGR id '1' nm 'Hvl_SluitVerval_Trigger' ty 1 t1 1 tp 1 ts 'S_13' tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid "  + Environment.NewLine +
                            @"TBLE "  + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 0 0 <"  + Environment.NewLine +
                            @"tble trgr"  + Environment.NewLine +
                            @"TRGR id '4' nm 'HY-SLUIT-TR' ty 1 t1 0 tp 0 ml '28_200' tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid "  + Environment.NewLine +
                            @"TBLE "  + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 1.8 <"  + Environment.NewLine +
                            @"tble trgr"  + Environment.NewLine +
                            @"TRGR id '5' nm 'HY-OPEN-TR' ty 1 t1 0 tp 0 ts 'S_24' ml '28_200' tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid "  + Environment.NewLine +
                            @"TBLE "  + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 0 1.8 <"  + Environment.NewLine +
                            @"tble trgr"  + Environment.NewLine +
                            @"TRGR id '18976' nm 'Svk_tijdreeks' ty 0 t1 0 tp 0 tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid "  + Environment.NewLine +
                            @"TBLE "  + Environment.NewLine +
                            @"'1991/01/01;00:00:00' 0 -1 -1 9.9999e+009 <" + Environment.NewLine +
                            @"tble trgr";

            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();

            Assert.AreEqual(5, sobekTriggers.Length);
            Assert.AreEqual("TRG_0", sobekTriggers[0].Id);
            Assert.AreEqual("Hvl_OpenVerval_Trigger", sobekTriggers[0].Name);
            Assert.AreEqual(SobekTriggerType.Hydraulic, sobekTriggers[0].TriggerType);
            Assert.AreEqual(false, sobekTriggers[0].OnceHydraulicTrigger);
            Assert.AreEqual(SobekTriggerParameterType.HeadDifferenceStructure, sobekTriggers[0].TriggerParameterType);
            Assert.AreEqual("S_13", sobekTriggers[0].StructureId);
            Assert.AreEqual("", sobekTriggers[0].MeasurementStationId);
            Assert.AreEqual(SobekTriggerCheckOn.Value, sobekTriggers[0].CheckOn);
            Assert.AreEqual(1, sobekTriggers[0].TriggerTable.Rows.Count);
            Assert.AreEqual(true, sobekTriggers[1].OnceHydraulicTrigger);
        }

        [Test]
        public void ReadFromStringSobekREFormat()
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


            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();

            Assert.AreEqual(5, sobekTriggers.Length);
            Assert.AreEqual("TRG_0", sobekTriggers[0].Id);
            Assert.AreEqual("Hvl_OpenVerval_Trigger", sobekTriggers[0].Name);
            Assert.AreEqual(SobekTriggerType.Hydraulic, sobekTriggers[0].TriggerType);
            Assert.AreEqual(SobekTriggerParameterType.HeadDifferenceStructure, sobekTriggers[0].TriggerParameterType);
            Assert.AreEqual("13", sobekTriggers[0].StructureId);
            Assert.AreEqual("", sobekTriggers[0].MeasurementStationId);
            Assert.AreEqual(SobekTriggerCheckOn.Value, sobekTriggers[0].CheckOn);
            Assert.AreEqual(1, sobekTriggers[0].TriggerTable.Rows.Count);

        }

        [Test]
        public void ReadFromStringSobekTimeTriggerPeriodicExtrapolation()
        {
            var fileText = @"TRGR id '0' nm 'TimeTriggerExtrapolationBlock' ty 1 t1 0 tp 1 tb '-1' tl 9.9999e+009 ts '13' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 1 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 0 < " + Environment.NewLine +
                            @"'1975/12/25;01:00:00' -1 -1 0 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr" + Environment.NewLine +
                            @"TRGR id '1' nm 'TimeTriggerExtrapolationPeriodic' ty 1 t1 0 tp 1 tb '-1' tl 9.9999e+009 ts '13' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 1 '3600' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 0 < " + Environment.NewLine +
                            @"'1975/12/25;01:00:00' -1 -1 0 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr";


            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();
            Assert.AreEqual(2, sobekTriggers.Length);
            Assert.AreEqual("",sobekTriggers[0].PeriodicExtrapolationPeriod);
            Assert.AreEqual("3600", sobekTriggers[1].PeriodicExtrapolationPeriod);
        }

        [Test]
        public void ReadFromStringSobekTimeTriggerPeriodicExtrapolationDirectional()
        {
            var fileText = @"TRGR id '0' nm 'TimeTriggerExtrapolationBlock' ty 1 t1 0 tp 1 tb '-1' tl 9.9999e+009 ts '13' ch 1 ql '-1' tt tr 'Trigger' PDIN 1 1 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 0 < " + Environment.NewLine +
                            @"'1975/12/25;01:00:00' -1 -1 0 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr" + Environment.NewLine +
                            @"TRGR id '1' nm 'TimeTriggerExtrapolationPeriodic' ty 1 t1 0 tp 1 tb '-1' tl 9.9999e+009 ts '13' ch 1 ql '-1' tt tr 'Trigger' PDIN 1 1 '3600' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " + Environment.NewLine +
                            @"'1975/12/25;00:00:00' -1 -1 1 0 < " + Environment.NewLine +
                            @"'1975/12/25;01:00:00' -1 -1 0 0 < " + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"trgr";
            
            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();
            Assert.AreEqual(2, sobekTriggers.Length);
            Assert.AreEqual(SobekTriggerCheckOn.Direction, sobekTriggers[0].CheckOn);
            Assert.AreEqual("", sobekTriggers[0].PeriodicExtrapolationPeriod);
            Assert.AreEqual("3600", sobekTriggers[1].PeriodicExtrapolationPeriod);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobek212File()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\Trigger.def");
            var sobekTriggers = new SobekTriggerReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(28, sobekTriggers.Length);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromSobekREFile()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\DEFSTR.5");
            var sobekTriggers = new SobekTriggerReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(28, sobekTriggers.Length);
        }

        [Test]
        public void ReadTriggerWithInputLocation()
        {
            var fileText = @"TRGR id '##1' nm 'Trigger 1' ty 1 t1 0 tp 0 ml '7' tt tr PDIN 1 0  pdin" + Environment.NewLine +
                           @"TBLE" + Environment.NewLine +
                           @"'2003/01/01;00:00:00' 0 0 1 5 < " + Environment.NewLine +
                           @"tble trgr";

            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();
            Assert.AreEqual(1, sobekTriggers.Length);
            Assert.AreEqual("7", sobekTriggers[0].MeasurementStationId);
        }

        [Test]
        public void ReadTriggerNoFiveFromNDB_measurementStationId_And_structureID()
        {
            var fileText =
                @"TRGR id '5' nm 'HY-OPEN-TR' ty 1 t1 0 tp 0 ts 'S_24' ml '28_200' tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1975/12/25;00:00:00' -1 -1 0 1.8 < " + Environment.NewLine +
                @"tble trgr";
            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();
            Assert.AreEqual(1, sobekTriggers.Length);
            var trigger = sobekTriggers[0];
            Assert.AreEqual("S_24", trigger.StructureId);
            Assert.AreEqual("28_200", trigger.MeasurementStationId);
        }

        [Test]
        public void ReadTriggerNoZeroFromNDB()
        {
            //seems also a cause for issue TOOLS-5547

            var fileText =
                @"TRGR id '0' nm 'Hvl_OpenVerval_Trigger' ty 1 t1 0 tp 1 ts 'C_2##S_13' tt tr PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Head Difference [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid " +
                Environment.NewLine +
                @"TBLE " + Environment.NewLine +
                @"'1975/12/25;00:00:00' -1 -1 1 0 < " + Environment.NewLine +
                @"tble trgr";

            var sobekTriggers = new SobekTriggerReader().Parse(fileText).ToArray();
            Assert.AreEqual(1, sobekTriggers.Length);


        }
    }
}
