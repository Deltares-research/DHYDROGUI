using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessBranchDataCsvReaderTest
    {
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportMaasCsvPerBranch()
        {
            var fileName = TestHelper.GetTestFilePath("roughness_voorbeeld_Maas.csv");
            var importer = new RoughnessBranchDataCsvReader();


            var importedData = importer.GetBranchData(fileName);
            // 5 branches
            Assert.AreEqual(15, importedData.Count);
            Assert.AreEqual("001", importedData[0].BranchName);
            Assert.AreEqual("Main", importedData[0].SectionType);
            var functionOfQ = importedData[0].RoughnessFunctionOfQ;
            Assert.IsNotNull(functionOfQ);
            // 2 chainages
            Assert.AreEqual(2, functionOfQ.Arguments[0].Values.Count);
            // at chainage 0 12 Q's
            Assert.AreEqual(13, functionOfQ.Arguments[1].Values.Count);
            Assert.AreEqual(new[]
                                {
                                    0.0, 50.0, 125.0,250.0, 500.0, 
                                    750.0, 1000.0, 1500.0, 2000.0, 2400.0, 
                                    2800.0, 4000.0, 4600.0
                                }, functionOfQ.Arguments[1].Values);
            Assert.AreEqual(24.19, (double)functionOfQ[0.0, 50.0], 1.0e-6);

            var floodplain2hannel20 = importedData.Where(id => id.SectionType == "Floodplain 2" & id.BranchName == "020").FirstOrDefault();
            Assert.AreEqual(RoughnessType.WhiteColebrook, floodplain2hannel20.RoughnessType);
            Assert.AreEqual(2, floodplain2hannel20.ConstantRoughness.Arguments[0].Values.Count);
            Assert.AreEqual(0.5, (double)floodplain2hannel20.ConstantRoughness[0.0], 1.0e-6);
            Assert.AreEqual(0.5, (double)floodplain2hannel20.ConstantRoughness[4084.0], 1.0e-6);
        }
        
        [Test]
        public void ImportEmptyFile()
        {
            //bit of an edge case but it should not crash the app anyway.
            var fileName = TestHelper.GetTestFilePath("EmptyRoughness.csv");
            var importer = new RoughnessBranchDataCsvReader();

            var importedData = importer.GetBranchData(fileName);
            //data for 0 branches
            Assert.AreEqual(0,importedData.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaasWithWrongColumnHeaders()
        {
            // Cheenage vs Chainage
            var fileName = TestHelper.GetTestFilePath("MaasWithWrongColumns.csv");
            new RoughnessBranchDataCsvReader().ReadCsvRecords(fileName);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaasWithWrongEnumValue()
        {
            // Unknown vs Checy
            var fileName = TestHelper.GetTestFilePath("MaasWithWrongEnumValue.csv");
            

            var importedData = new RoughnessBranchDataCsvReader().ReadCsvRecords(fileName);
            Assert.AreEqual(1, importedData.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaasWithWrongType()
        {
            // Unknown vs Checy
            var fileName = TestHelper.GetTestFilePath("MaasWithWrongType.csv");
            var importedData = new RoughnessBranchDataCsvReader().ReadCsvRecords(fileName);
            Assert.AreEqual(1, importedData.Count);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMaasCsvRecords()
        {
            var fileName = TestHelper.GetTestFilePath("roughness_voorbeeld_Maas.csv");
            

            var importedData = new RoughnessBranchDataCsvReader().ReadCsvRecords(fileName);
            Assert.AreEqual(152, importedData.Count);

            // test record at line 147; start at 0, header -> -2
            // 020;4084;Chezy;Main;Discharge;Linear;Same;;2000;50
            var record145 = importedData[145];
            Assert.AreEqual("020", record145.BranchName);
            Assert.AreEqual(4084.0, record145.Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessType.Chezy, record145.RoughnessType);
            Assert.AreEqual("Main", record145.SectionType);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, record145.RoughnessFunction);
            Assert.AreEqual(InterpolationType.Linear, record145.InterpolationType);
            Assert.IsTrue(record145.NegativeIsPositive);
            Assert.AreEqual(2000.0, record145.PositiveQ, 1.0e-6);
            Assert.AreEqual(50.0, record145.PositiveQRoughness, 1.0e-6);

            // test record at line 148
            // 020;0;Chezy;Floodplain 1;Constant;Linear;Same;35;;
            var record146 = importedData[146];
            Assert.AreEqual("020", record146.BranchName);
            Assert.AreEqual(0.0, record146.Chainage, 1.0e-6);
            Assert.AreEqual(RoughnessType.Chezy, record146.RoughnessType);
            Assert.AreEqual("Floodplain 1", record146.SectionType);
            Assert.AreEqual(RoughnessFunction.Constant, record146.RoughnessFunction);
            Assert.AreEqual(InterpolationType.Linear, record146.InterpolationType);
            Assert.IsTrue(record146.NegativeIsPositive);
            Assert.AreEqual(35, record146.PositiveConstant, 1.0e-6);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaasWithMissingColumns()
        {
            // Unknown vs Checy
            var fileName = TestHelper.GetTestFilePath("MaasWithMissingColumns.csv");
            var importer = new RoughnessBranchDataCsvReader();

            var importedData = new RoughnessBranchDataCsvReader().ReadCsvRecords(fileName);
            Assert.AreEqual(1, importedData.Count);
        }

    }
}