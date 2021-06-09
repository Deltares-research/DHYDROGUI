using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekWaterFlowModel1DImporterNetworkTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBridgeAndCulvertFrictionsTest()
        {
            //STFR id '5' ci '5' mf 3 mt cp 0 35 0 mr cp 0 35 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 stfr
            //STFR id '4' ci '4' mf 1 mt cp 0 0.022 0 mr cp 0 0.022 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 stfr
            var path = TestHelper.GetTestDataDirectory() + @"\StrucFr2.lit\2\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(path);

            Assert.AreEqual(1, network.Bridges.Count());
            var bridge = network.Structures.Where(s => s.Name == "4").First(); // river bridge id != def id
            Assert.IsInstanceOf<Bridge>(bridge);
            Assert.AreEqual(BridgeFrictionType.Manning, ((Bridge)bridge).FrictionType);
            Assert.AreEqual(0.022, ((Bridge)bridge).Friction);

            Assert.AreEqual(1, network.Culverts.Count());
            var culvert = network.Structures.Where(s => s.Name == "5").First(); // river culvert id != def id
            Assert.IsInstanceOf<Culvert>(culvert);
            Assert.AreEqual(CulvertFrictionType.StricklerKs, ((Culvert)culvert).FrictionType);
            Assert.AreEqual(35, ((Culvert)culvert).Friction);
        }

        [Test]
        public void ReadAndConvertLateralFlowWithInterpolationTypeConstant()
        {
            // PDIN ..pdin = period and interpolation method, 0 0 or 0 1 = interpolation continuous, 1 0 or 1 1 = interpolation block 
            string initialConditionsText =
                @"FLBR id '5' sc 0 lt 0 dc lt 0 50 0 flbr" + Environment.NewLine +
                @"FLBR id '6' sc 0 lt 0 dc lt 1 0 0 PDIN 1 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;18:00:00' 20 < " + Environment.NewLine +
                @"'1996/01/02;00:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";
            var reader = new SobekLateralFlowReader();

            var lateralFlow = reader.GetLateralFlow(initialConditionsText, new Dictionary<string, IList<string>>());

            Model1DLateralSourceData model1DLateralSourceData = new Model1DLateralSourceData();
            SobekLateralSourcesDataImporter.ConvertToLateralSourceData(lateralFlow, model1DLateralSourceData);
            Assert.AreEqual(InterpolationType.Constant, model1DLateralSourceData.Data.Arguments[0].InterpolationType);
        }

        [Test]
        public void ReadAndConvertLateralFlowWithQhTable()
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

            var lateralFlow = reader.GetLateralFlow(source, new Dictionary<string, IList<string>>());

            var waterFlowModel1DLateralSourceData = new Model1DLateralSourceData();

            SobekLateralSourcesDataImporter.ConvertToLateralSourceData(lateralFlow, waterFlowModel1DLateralSourceData);

            Assert.AreEqual(Model1DLateralDataType.FlowWaterLevelTable,
                            waterFlowModel1DLateralSourceData.DataType);
            Assert.AreEqual(0.0, (double)waterFlowModel1DLateralSourceData.Data[49.86], 1.0e-6);
            Assert.AreEqual(-10.0, (double)waterFlowModel1DLateralSourceData.Data[49.96], 1.0e-6);
            Assert.AreEqual(-952.0, (double)waterFlowModel1DLateralSourceData.Data[52.44], 1.0e-6);
            Assert.AreEqual(ExtrapolationType.Constant, waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType);
        }
    }
}