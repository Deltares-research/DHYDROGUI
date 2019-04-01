using System;
using System.Collections.Specialized;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessFromCsvFileExporterTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void WriteAndReadDemoModelWithSimpleRoughnessCsv()
        {
            // export test data 
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            var model = ExportRoughnessForDemoModel(path, 12.21, RoughnessType.StricklerKn);

            // import and check for changes
            var imports = new RoughnessBranchDataCsvReader().ReadCsvRecords(path);
            Assert.AreEqual(1, imports.Count);

            var branch = model.Network.Branches[0];
            Assert.AreEqual(branch.Name, imports[0].BranchName);
            Assert.AreEqual(10.0, imports[0].Chainage, 1.0e-6);
            Assert.AreEqual(12.21, imports[0].PositiveConstant, 1.0e-6);
            Assert.AreEqual(RoughnessType.StricklerKn, imports[0].RoughnessType);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WriteAndReadDemoModelWithSimpleRoughnessAndMergeWithSections()
        {
            // export test data 
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            var model = ExportRoughnessForDemoModel(path, 12.21, RoughnessType.StricklerKn);
            Assert.AreEqual(1, model.RoughnessSections[0].RoughnessNetworkCoverage.Locations.Values.Count);

            model.RoughnessSections.ForEach(roughnessSection => roughnessSection.Clear());
            Assert.AreEqual(0, model.RoughnessSections[0].RoughnessNetworkCoverage.Locations.Values.Count);

            // import 
            var importer = new RoughnessFromCsvToSectionImporter();
            importer.ImportItem(path, model.RoughnessSections);

            // merge into RoughnessSections of model
            Assert.AreEqual(1, model.RoughnessSections[0].RoughnessNetworkCoverage.Locations.Values.Count);
            var networkLocation = (NetworkLocation)model.RoughnessSections[0].RoughnessNetworkCoverage.Locations.Values[0];
            Assert.AreEqual(RoughnessType.StricklerKn, model.RoughnessSections[0].RoughnessNetworkCoverage.EvaluateRoughnessType(networkLocation));
            Assert.AreEqual(12.21 ,model.RoughnessSections[0].RoughnessNetworkCoverage.EvaluateRoughnessValue(networkLocation), 1.0e-6);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WriteAndReadDemoModelWithQAndHRoughnessCheckParsedData()
        {
            IBranch branch0, branch1;
            WaterFlowModel1D model;
            string path;
            ExportDemoModelWithFunctionOfQAndFunctionOfH(out model, out branch0, out branch1, out path);
            var exporter = new RoughnessFromCsvFileExporter();
            exporter.Export(model.RoughnessSections, path);
            
            var imports = new RoughnessBranchDataCsvReader().ReadCsvRecords(path);
            // imports will per branch (2) a record for each chainage (2 + 2)
            Assert.AreEqual(8, imports.Count);
            Assert.AreEqual(branch0.Name, imports[0].BranchName);
            Assert.AreEqual(0.0, imports[0].Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, imports[0].RoughnessFunction);

            Assert.AreEqual(branch0.Name, imports[1].BranchName);
            Assert.AreEqual(0.0, imports[1].Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, imports[1].RoughnessFunction);

            Assert.AreEqual(branch1.Name, imports[4].BranchName);
            Assert.AreEqual(2.0, imports[4].Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessFunction.FunctionOfH, imports[4].RoughnessFunction);

            Assert.AreEqual(branch1.Name, imports[5].BranchName);
            Assert.AreEqual(2.0, imports[5].Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessFunction.FunctionOfH, imports[5].RoughnessFunction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WriteAndReadDemoModelWithQAndHRoughnessAndWhiteColebrookType()
        {
            string path = TestHelper.GetCurrentMethodName() + ".csv";
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            // Add constant value to roughnessSection
            var roughnessSections = model.RoughnessSections;
            var roughnessSection = roughnessSections[0];
            var branch0 = model.Network.Branches[0];

            roughnessSection.RoughnessNetworkCoverage.DefaultRoughnessType = RoughnessType.WhiteColebrook;

            var q = roughnessSection.AddQRoughnessFunctionToBranch(branch0);
            q[0.0, 0.0] = 12.0; // q[chainage, q] = roughness
            q[1.0, 0.0] = 14.0;
            q[0.0, 10.0] = 15.0;

            var exporter = new RoughnessFromCsvFileExporter();
            exporter.Export(model.RoughnessSections, path);

            var imports = new RoughnessBranchDataCsvReader().ReadCsvRecords(path);
            // imports will per branch (2) a record for each chainage (2 + 2)
            Assert.AreEqual(4, imports.Count);
            Assert.AreEqual(branch0.Name, imports[0].BranchName);
            Assert.AreEqual(0.0, imports[0].Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, imports[0].RoughnessFunction);

            Assert.AreEqual(branch0.Name, imports[1].BranchName);
            Assert.AreEqual(0.0, imports[1].Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, imports[1].RoughnessFunction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WriteAndReadDemoModelWithQAndHRoughnessCheckResultingCoverages()
        {
            IBranch branch0, branch1;
            WaterFlowModel1D model;
            string path;
            ExportDemoModelWithFunctionOfQAndFunctionOfH(out model, out branch0, out branch1, out path);
            var exporter = new RoughnessFromCsvFileExporter();
            exporter.Export(model.RoughnessSections, path);

            // clear roughness sections
            model.RoughnessSections.ForEach(rs => rs.Clear());

            var importer = new RoughnessFromCsvToSectionImporter();
            importer.ImportItem(path, model.RoughnessSections);
            
            var functionsOfQ = model.RoughnessSections[0].FunctionOfQ(branch0);
            var functionsOfH = model.RoughnessSections[0].FunctionOfH(branch1);

            Assert.AreEqual(12.0, (double)functionsOfQ[0.0, 0.0], 1.0e-6);
            Assert.AreEqual(14.0, (double)functionsOfQ[1.0, 0.0], 1.0e-6);
            Assert.AreEqual(15.0, (double)functionsOfQ[0.0, 10.0], 1.0e-6);
 
            Assert.AreEqual(22.0, (double)functionsOfH[2.0, 101.0], 1.0e-6);
            Assert.AreEqual(24.0, (double)functionsOfH[3.0, 101.0], 1.0e-6);
            Assert.AreEqual(25.0, (double)functionsOfH[2.0, 111.0], 1.0e-6);
        }

        [Test]
        public void GivenRoughnessFromCsvFileExporterObjectWhenCallingCategoryThenCategoryShouldBeGeneral()
        {
            var exporter = new RoughnessFromCsvFileExporter();
            Assert.That(exporter.Category, Is.EqualTo("General"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "RoughnessFromCsvFileExporter can only export items of type RoughnessSections")]
        public void GivenANullObjectToExportedWhenExportingRoughnessFromCsvFileThenExceptionShouldBeThrown()
        {
            var exporter = new RoughnessFromCsvFileExporter();
            exporter.Export(null, string.Empty);

        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenARoughnessSectionWithIllegalExportPathWhenExportingRoughnessFromCsvFileThenArgumentExceptionShouldBeThrown()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMultiMock<INetwork>(typeof(INotifyCollectionChanged));
            network.Expect(n => n.Branches).Return(new EventedList<IBranch>()).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged) network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Any();
            mocks.ReplayAll();
            
            var exporter = new RoughnessFromCsvFileExporter();
            var roughnessSection = new RoughnessSection(new CrossSectionSectionType(), network);
            exporter.Export(roughnessSection, string.Empty);
            mocks.VerifyAll();
        }

        [Test]
        public void GivenARoughnessSectionWithALegalExportPathWhenExportingRoughnessFromCsvFileThenExportIsSucces()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMultiMock<INetwork>(typeof(INotifyCollectionChanged));
            network.Expect(n => n.Branches).Return(new EventedList<IBranch>()).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged)network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Any();
            mocks.ReplayAll();

            var exporter = new RoughnessFromCsvFileExporter();
            var roughnessSection = new RoughnessSection(new CrossSectionSectionType(), network);
            var exportDir = FileUtils.CreateTempDirectory();
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            try
            {
                Assert.That(exporter.Export(roughnessSection, Path.Combine(exportDir, "myFile.csv")), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(exportDir); 
            }
            
            mocks.VerifyAll();
        }

        private static void ExportDemoModelWithFunctionOfQAndFunctionOfH(out WaterFlowModel1D model, out IBranch branch0, out IBranch branch1, out string path)
        {
            path = TestHelper.GetCurrentMethodName() + ".csv";
            model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            // Add constant value to roughnessSection
            var roughnessSections = model.RoughnessSections;
            var roughnessSection = roughnessSections[0];
            branch0 = model.Network.Branches[0];
            branch1 = model.Network.Branches[1];
            
            var q = roughnessSection.AddQRoughnessFunctionToBranch(branch0);
            q[0.0, 0.0] = 12.0; // q[chainage, q] = roughness
            q[1.0, 0.0] = 14.0;
            q[0.0, 10.0] = 15.0;
            
            var h = roughnessSection.AddHRoughnessFunctionToBranch(branch1);
            h[2.0, 101.0] = 22.0; // q[chainage, q] = roughness
            h[3.0, 101.0] = 24.0;
            h[2.0, 111.0] = 25.0;
        }

        private static WaterFlowModel1D ExportRoughnessForDemoModel(string path, double value, RoughnessType roughnessType)
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            // Add constant value to roughnessSection
            var roughnessSections = model.RoughnessSections;
            var roughnessSection = roughnessSections[0];
            var branch = model.Network.Branches[0];
            roughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 10.0)] = new object[] { value, roughnessType };
            var exporter = new RoughnessFromCsvFileExporter();
            exporter.Export(model.RoughnessSections, path);
            return model;
        }
    }
}

