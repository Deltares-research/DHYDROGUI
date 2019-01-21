using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekBoundaryConditionsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportBoundaryConditions()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter() });

            importer.Import();

            var boundaryConditions = waterFlowModel1DModel.BoundaryConditions;
            Assert.AreEqual(34, boundaryConditions.Count());
            Assert.AreEqual(9, boundaryConditions.Count(bc => bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadBoundaryConditionsAndBuildWithInterpolationTypeConstant()
        {
            // PDIN ..pdin = period and interpolation method, 0 0 or 0 1 = interpolation continuous, 1 0 or 1 1 = interpolation block 
            var initialConditionsText = @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 1 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;18:00:00' 20 < " + Environment.NewLine +
                @"'1996/01/02;00:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();
            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);
            var flowBoundaryCondition = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(sobekFlowBoundaryCondition);
            
            Assert.AreEqual(InterpolationType.Constant, flowBoundaryCondition.Data.Arguments[0].InterpolationType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadBoundaryConditionsAndBuildWithPeriodicExtrapolation()
        {
            //PDIN 0 1 '43200' pdin = extrapolation periodic 43200 sec (12 hours) (source sobek 2.12)
            var initialConditionsText = @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 0 1 '43200' pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();
            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);
            var flowBoundaryCondition = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(sobekFlowBoundaryCondition);
            
            Assert.AreEqual(ExtrapolationType.Periodic, flowBoundaryCondition.Data.Arguments[0].ExtrapolationType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadBoundaryConditionsAndBuildWithPeriodicExtrapolation2()
        {
            //PDIN 0 1 365;00:00:00 means linear interpolation, period one year (source manual)
            var initialConditionsText = @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 0 1 365;00:00:00 pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();
            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);
            var flowBoundaryCondition = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(sobekFlowBoundaryCondition);
            
            Assert.AreEqual(ExtrapolationType.Periodic, flowBoundaryCondition.Data.Arguments[0].ExtrapolationType);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportInitialConditionsLandelijkSobekModel()
        {
            var zipPath = TestHelper.GetDataDir() + @"\LSM1_0.lit\12.zip";

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Unzip LSM1_0.lit/12/ into temporary directory
                ZipFileUtils.Extract(zipPath, tempDir);

                // Actual test
                var pathToSobekNetwork = Path.Combine(tempDir, "12", "NETWORK.TP");

                var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");

                var partialSobekImporters = new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter(), // 7,5 sec
                    new SobekBoundaryConditionsImporter()
                };

                var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, partialSobekImporters);
                TestHelper.AssertIsFasterThan(80000, importer.Import);
            });
        }
    }
}
