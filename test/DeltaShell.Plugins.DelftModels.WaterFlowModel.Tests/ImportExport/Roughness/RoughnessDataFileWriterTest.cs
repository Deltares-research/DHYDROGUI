using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessDataFileWriterTest
    {
        private const string RelativeTargetDirectory = "./FileWriters";

        [Test]
        public void TestRoughnessDataFileWriter_DuitseRijnBranch24_B_A() // see issue SOBEK3-417
        {
            var targetPath = Path.Combine(Environment.CurrentDirectory, RelativeTargetDirectory);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var dischargeRoughnessSection = new RoughnessSection(crossSectionSectionType, network);

            var dischargeFunction = RoughnessSection.DefineFunctionOfQ();
            dischargeFunction[0.0, 0.0] = 1.1;
            dischargeFunction[0.0, 1000.0] = 2.1;
            dischargeFunction[0.0, 5000.0] = 3.1;
            dischargeFunction[0.0, 10000.0] = 2.1;

            var waterLevelfunction = RoughnessSection.DefineFunctionOfH();
            waterLevelfunction[0.0, 0.0] = 4.1;
            waterLevelfunction[0.0, 1000.0] = 5.1;
            waterLevelfunction[0.0, 5000.0] = 6.1;
            waterLevelfunction[0.0, 10000.0] = 5.1;

            // remove values for Chainage and Roughness, but keep values for Q (as is the case for branch 24_B_A in DuitseRijn)
            dischargeFunction.Arguments[0].Values.Clear();
            dischargeFunction.Components[0].Values.Clear();
            
            dischargeRoughnessSection.AddQRoughnessFunctionToBranch(branch1, dischargeFunction);
            dischargeRoughnessSection.AddHRoughnessFunctionToBranch(branch2, waterLevelfunction);

            var expectedFile = TestHelper.GetTestFilePath(@"FileWriters/roughness-SOBEK3-417_expected.txt");
            var relativePathActualFile = Path.Combine(targetPath, "roughness-SOBEK3-417.ini");
            RoughnessDataFileWriter.WriteFile(relativePathActualFile, dischargeRoughnessSection);

            // In this case, we expect to see BranchProperties written to file for branch2 but not branch1

            string errorMessage;
            Assert.IsTrue(FileComparer.Compare(expectedFile, relativePathActualFile, out errorMessage, true),
                          string.Format("Generated Roughness main file does not match template!{0}{1}", Environment.NewLine, errorMessage));
        }

        [Test]
        public void TestRoughnessDataFileWriter()
        {
            //var expectedFile = TestHelper.GetTestFilePath(@"FileWriters/RoughnessData_expected.txt");
            var targetPath = Path.Combine(Environment.CurrentDirectory, RelativeTargetDirectory);
            
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var dischargeRoughnessSection = new RoughnessSection(crossSectionSectionType, network);

            var dischargeFunction = RoughnessSection.DefineFunctionOfQ();
            dischargeFunction[0.0, 0.0] = 1.1;
            dischargeFunction[0.0, 1000.0] = 2.1;
            dischargeFunction[0.0, 5000.0] = 3.1;
            dischargeFunction[0.0, 10000.0] = 2.1;

            dischargeFunction[2500.0, 0.0] = 11.1;
            dischargeFunction[2500.0, 8000.0] = 13.1;
            dischargeFunction[2500.0, 10000.0] = 12.1;

            dischargeRoughnessSection.AddQRoughnessFunctionToBranch(branch2, dischargeFunction);
            var locationOne = new NetworkLocation(branch2, 0);
            dischargeRoughnessSection.RoughnessNetworkCoverage[locationOne] = new object[] { 50.0, RoughnessType.WhiteColebrook };
            dischargeRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch1, 50)] = new object[] { 60.0, RoughnessType.DeBosAndBijkerk };
            dischargeRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch1, 0)] = new object[] { 70.0, RoughnessType.DeBosAndBijkerk };

            RoughnessDataFileWriter.WriteFile(Path.Combine(targetPath,"roughness-FunctionOfQ.ini"), dischargeRoughnessSection);
            
            var waterLevelRoughnessSection = new RoughnessSection(crossSectionSectionType, network);
            var waterLevelfunction = RoughnessSection.DefineFunctionOfH();
            waterLevelfunction[0.0, 0.0] = 4.1;
            waterLevelfunction[0.0, 1000.0] = 5.1;
            waterLevelfunction[0.0, 5000.0] = 6.1;
            waterLevelfunction[0.0, 10000.0] = 5.1;

            waterLevelfunction[2000.0, 0.0] = 14.1;
            waterLevelfunction[2000.0, 8000.0] = 16.1;
            waterLevelfunction[2000.0, 10000.0] = 15.1;

            waterLevelRoughnessSection.AddHRoughnessFunctionToBranch(branch1, waterLevelfunction);
            var locationTwo = new NetworkLocation(branch1, 0);
            waterLevelRoughnessSection.RoughnessNetworkCoverage[locationTwo] = new object[] { 50.0, RoughnessType.Manning };

            RoughnessDataFileWriter.WriteFile(Path.Combine(targetPath,"roughness-FunctionOfH.ini"), waterLevelRoughnessSection);

            var constantRoughnessSection =  new ReverseRoughnessSection(new RoughnessSection(crossSectionSectionType, network));
            var roughnessFunction = RoughnessBranchDataMerger.DefineConstantFunction();

            roughnessFunction[10.0] = 10.0;
            roughnessFunction[99.0] = 99.0;
            roughnessFunction[10000.0] = 10000.0;

            constantRoughnessSection.UpdateCoverageForFunction(network.Branches[0], roughnessFunction, RoughnessType.Chezy);

            RoughnessDataFileWriter.WriteFile(Path.Combine(targetPath,"roughness-constant.ini"), constantRoughnessSection);
        }

        [Test]
        [TestCase("Test", RoughnessType.StricklerKn,25.2, InterpolationType.None)]
        [TestCase("Test2", RoughnessType.Manning, 2.2, InterpolationType.Linear)]
        [TestCase("Test3", RoughnessType.WhiteColebrook, 2.45, InterpolationType.Constant)]
        public void ReverseRoughnessFileContentShouldBeCorrect(string orginalSectionName, RoughnessType type, double defaultValue, InterpolationType interpolationType)
        {
            var network = (INetwork) MockRepository.GenerateStrictMock(typeof(INetwork), new []{typeof(INotifyPropertyChanged), typeof(INotifyCollectionChanged) });

            network.Expect(n => n.Branches).Return(new EventedList<IBranch>()).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged) network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Twice();

            network.Replay();

            var roughnessSection = new RoughnessSection(new CrossSectionSectionType{Name = orginalSectionName}, network);
            var reverseRoughnessSection = new ReverseRoughnessSection(roughnessSection){ UseNormalRoughness = false};
            var coverage = reverseRoughnessSection.RoughnessNetworkCoverage;

            // values to check
            coverage.DefaultRoughnessType = type;
            coverage.DefaultValue = defaultValue;
            coverage.Arguments[0].InterpolationType = interpolationType;

            var expectedFile = FileUtils.CreateTempDirectory() + @"roughness.txt";
            
            try
            {
                RoughnessDataFileWriter.WriteFile(expectedFile, reverseRoughnessSection);

                Assert.IsTrue(File.Exists(expectedFile));

                var data = File.ReadAllLines(expectedFile);

                Assert.AreEqual("[Content]", data[5]);
                Assert.AreEqual("sectionId             = " + orginalSectionName, data[6].Trim());
                Assert.AreEqual("flowDirection         = True", data[7].Trim());
                Assert.AreEqual("interpolate           = " + (interpolationType == InterpolationType.Linear? 1 :0), data[8].Trim());
                Assert.AreEqual("globalType            = " + (int)FrictionTypeConverter.ConvertFrictionType(type), data[9].Trim());
                Assert.AreEqual("globalValue           = " + defaultValue.ToString("F3", CultureInfo.InvariantCulture), data[10].Trim());
            }
            finally
            {
                FileUtils.DeleteIfExists(expectedFile);
            }
        }

        [Test]
        public void TestRoughnessDataFileWriter_STEF()
        {
            //var expectedFile = TestHelper.GetTestFilePath(@"FileWriters/RoughnessData_expected.txt");
            var targetPath = Path.Combine(Environment.CurrentDirectory, RelativeTargetDirectory);
            
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            //var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];
            var branch3 = network.Branches[2];
            //var branch4 = network.Branches[3];

            var mainCrossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var mainRoughnessSection = new RoughnessSection(mainCrossSectionSectionType, network);
            var floodPlain1CrossSectionSectionType = new CrossSectionSectionType { Name = "floodplain1" };
            var floodPlain1RoughnessSection = new RoughnessSection(floodPlain1CrossSectionSectionType, network);

            var dischargeFunctionBranch2Main = RoughnessSection.DefineFunctionOfQ();
            dischargeFunctionBranch2Main[0.0, 0.0] = 1.1;
            dischargeFunctionBranch2Main[0.0, 10000.0] = 2.1;

            dischargeFunctionBranch2Main[2500.0, 0.0] = 11.1;
            dischargeFunctionBranch2Main[2500.0, 10000.0] = 12.1;
            mainRoughnessSection.AddQRoughnessFunctionToBranch(branch2, dischargeFunctionBranch2Main);

            var waterLevelFunctionBranch3FloodPlain1 = RoughnessSection.DefineFunctionOfH();
            waterLevelFunctionBranch3FloodPlain1[0.0, 1.0] = 31.1;
            waterLevelFunctionBranch3FloodPlain1[0.0, 4.0] = 41.1;

            waterLevelFunctionBranch3FloodPlain1[3500.0, 1.0] = 111.1;
            waterLevelFunctionBranch3FloodPlain1[3500.0, 4.0] = 112.1;

            floodPlain1RoughnessSection.AddHRoughnessFunctionToBranch(branch3, waterLevelFunctionBranch3FloodPlain1);

            /*var mainReverseRoughnessSection = new ReverseRoughnessSection(mainRoughnessSection);
            var mainReverseFunctionBranch4 = (IFunction)mainReverseRoughnessSection.FunctionOfQ(branch2).Clone();
            mainReverseFunctionBranch4[0.0, 10000.0] = 80.1;
            var mainReverseFunctionBranch4 = RoughnessSection.DefineFunctionOfQ();
            mainReverseFunctionBranch4[0.0, 0.0] = 31.1;
            mainReverseFunctionBranch4[0.0, 10000.0] = 32.1;

            mainReverseFunctionBranch4[2500.0, 0.0] = 311.1;
            mainReverseFunctionBranch4[2500.0, 10000.0] = 312.1;
            //mainReverseRoughnessSection.AddQRoughnessFunctionToBranch(branch4, mainReverseFunctionBranch4);
            mainReverseRoughnessSection.UpdateCoverageForFunction(branch4, mainReverseFunctionBranch4,RoughnessType.Manning);*/
            /*
            //generate contant
            var floodPlain2CrossSectionSectionType = new CrossSectionSectionType { Name = "floodplain2" };
            var constantRoughnessSection = new RoughnessSection(floodPlain2CrossSectionSectionType, network);
            var roughnessFunction = RoughnessBranchDataMerger.DefineConstantFunction();

            roughnessFunction[10.0] = 10.0;
            roughnessFunction[99.0] = 99.0;
            
            constantRoughnessSection.UpdateCoverageForFunction(branch1, roughnessFunction, RoughnessType.Chezy);
            */
            RoughnessDataFileWriter.WriteFile(Path.Combine(targetPath, "roughness-main.ini"), mainRoughnessSection);
            var expectedFile = TestHelper.GetTestFilePath(@"FileWriters/roughness-main_expected.txt");
            string errorMessage;
            var relativePathActualFile = Path.Combine(targetPath, "roughness-main.ini");
            Assert.IsTrue(FileComparer.Compare(expectedFile, relativePathActualFile, out errorMessage, true),
                          string.Format("Generated Roughness main file does not match template!{0}{1}", Environment.NewLine, errorMessage));
            /*roughnessDataFileWriter.WriteFile("roughness-main-reverse.ini", mainReverseRoughnessSection);*/
            RoughnessDataFileWriter.WriteFile(Path.Combine(targetPath,"roughness-floodplain1.ini"), floodPlain1RoughnessSection);
            expectedFile = TestHelper.GetTestFilePath(@"FileWriters/roughness-floodplain1_expected.txt");
            relativePathActualFile = Path.Combine(targetPath, "roughness-floodplain1.ini");
            Assert.IsTrue(FileComparer.Compare(expectedFile, relativePathActualFile, out errorMessage, true),
                          string.Format("Generated Roughness floodplain1 file does not match template!{0}{1}", Environment.NewLine, errorMessage));
        } 
    }
}