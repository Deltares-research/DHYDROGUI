using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using NUnit.Framework;
using SobekCompare.Tests.Helpers;

namespace SobekCompare.Tests
{
    [TestFixture]
    [Category(TestCategorySobekValidation.WaterQuality1D)]
    public class WAQTestBench
    {
        private static string SobekWaqSubstateSubFile = "SUBSTATE.SUB";
        private static string SobekWaqBoundWQTypFile = "BOUNDWQ.TYP";
        private static string SobekWaqBoundWQDatFile = "BOUNDWQ.DAT";
        private static string SobekWaqBoundWQGlbFile = "BOUNDWQ.GLB";
        private static string SobekWaqConstantDwqFile = "CONSTANT.DWQ";
        private static string SobekWaqCaseCoefxDatFile = "COEFX.DAT";
        private static string SobekWaqDelwaq1InpFile = "DELWAQ1.INP";
        private static string SobekWaqDelwaq2InpFile = "DELWAQ2.INP";
        private static string SobekWaqDelwaq3InpFile = "DELWAQ3.INP";
        private static string SobekWaqCaseDescCmtFile = "casedesc.cmt";
        private static string SobekCaseSettingFileName = "SETTINGS.DAT" ;

        private static string testCasesDirectory;
        private static IDictionary<int, string> testDictionary;
        private static ModelRunnerAndResultComparer modelRunnerAndResultComparer;

        # region SetUp / TearDown / Constructor

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            // Set the testcases dir
            testCasesDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "testbench\\testcases_waq");

            // Create a lookup of nr (=> testdirectory which we can use in the separate tests)
            testDictionary = SobekTestBenchHelper.GetTestsDictionary(testCasesDirectory);

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
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

        # endregion

        [Test]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        [TestCase(12)]
        [TestCase(13)]
        [TestCase(14)]
        [TestCase(15)]
        [TestCase(16)]
        [TestCase(17)]
        [TestCase(18)]
        [TestCase(19)]
        public void RunTestCase(int caseNumber)
        {
            var testInfo = SobekTestBenchHelper.GetTestInfo(caseNumber, testCasesDirectory, testDictionary);
            if (testInfo == null) return;

            var currentDir = Environment.CurrentDirectory;

            modelRunnerAndResultComparer = new ModelRunnerAndResultComparer(testInfo.CaseDirectory);

            var flowModel = modelRunnerAndResultComparer.waterFlowModel1D;
            flowModel.HydFileOutput = true;

            modelRunnerAndResultComparer.RunModels();

            var waqWorkDir = CreateWorkingDirectory();
            var waqModel = new WaterQualityModel
                {
                    ExplicitWorkingDirectory = Path.GetTempPath() + "waq",
                    ModelSettings =
                        {
                            WorkDirectory = waqWorkDir,
                            OutputDirectory = waqWorkDir
                        }
                };

            var data = HydFileReader.ReadAll(new FileInfo(flowModel.HydFilePath));

            // import data (hyd file, sub file)
            waqModel.ImportHydroData(data);

            new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, Path.Combine(testInfo.CaseDirectory, SobekWaqSubstateSubFile));
            
            // set duflow dll's
            waqModel.SubstanceProcessLibrary.ProcessDllFilePath = SubstanceProcessLibrary.DefaultDuflowProcessDllFilePath;
            waqModel.SubstanceProcessLibrary.ProcessDefinitionFilesPath = SubstanceProcessLibrary.DefaultDuflowProcessDefinitionFilesPath;

            ImportFromSobek212(waqModel, testInfo.CaseDirectory);
            
            ActivityRunner.RunActivity(waqModel);

            modelRunnerAndResultComparer.WorkingDirHisFilesPath = waqModel.ModelSettings.OutputDirectory;

            var error = modelRunnerAndResultComparer.CompareDeltaShellHisWithSobek212His(testInfo.Name, testCasesDirectory, testInfo.TestDirectory, testInfo.CaseDirectory);

            Environment.CurrentDirectory = currentDir;

/*            if (!String.IsNullOrEmpty(error))
            {
                Assert.Fail(error);
            }*/
        }

        private static string CreateWorkingDirectory()
        {
            var workDirectory = Path.GetTempFileName();
            File.Delete(workDirectory);
            Directory.CreateDirectory(workDirectory);
            return workDirectory;
        }

        private void ImportFromSobek212(WaterQualityModel waqModel, string directory)
        {
            var sobekCaseSettings = SobekCaseSettingsReader.GetSobekCaseSettings(Path.Combine(directory, SobekCaseSettingFileName) );
            waqModel.ModelSettings.ProcessesActive = sobekCaseSettings.ActiveProcess;

            var delwaq1InpFile = Path.Combine(directory, SobekWaqDelwaq1InpFile);
            var sobekWaqNumericalSettings = SobekWaqNumericalSettingsReader.ReadNumericalSettingsFromSobek212(delwaq1InpFile);

            waqModel.ModelSettings.NumericalScheme = SetNumericalScheme1D(sobekWaqNumericalSettings.NumericalScheme1D);
            waqModel.ModelSettings.NoDispersionIfFlowIsZero = sobekWaqNumericalSettings.NoDispersionIfFlowIsZero;
            waqModel.ModelSettings.NoDispersionOverOpenBoundaries = sobekWaqNumericalSettings.NoDispersionOverOpenBoundaries;
            waqModel.ModelSettings.UseFirstOrder = sobekWaqNumericalSettings.UseFirstOrder;

            var sobekWaqSimulationTimer = SobekWaqSimulationTimerReader.ReadSimulationTimerFromSobek212(delwaq1InpFile);

            waqModel.StartTime = sobekWaqSimulationTimer.StartTime;
            waqModel.StopTime = sobekWaqSimulationTimer.StopTime;
            waqModel.TimeStep = sobekWaqSimulationTimer.TimeStep;

            var boundaryFile = Path.Combine(directory, SobekWaqBoundWQDatFile);
            var constantBoundaryData = SobekWaqBoundaryConditionsReader.ReadConstantBoundaryValuesFromSobek212(boundaryFile);
            var timeDependentBoundaryDataBlock = SobekWaqBoundaryConditionsReader.ReadTimeDependentBoundaryValuesWithBlockInterpolationFromSobek212(boundaryFile);
            var timeDependentBoundaryDataLinear = SobekWaqBoundaryConditionsReader.ReadTimeDependentBoundaryValuesWithLinearInterpolationFromSobek212(boundaryFile);

            var activeSubstances = waqModel.SubstanceProcessLibrary.ActiveSubstances;

            CheckActiveSubstances(constantBoundaryData, timeDependentBoundaryDataBlock, timeDependentBoundaryDataLinear, activeSubstances, "boundaries");

            //waqModel.BoundaryDataManager.CreateNewDataTable("FractionData", fractionDataContent, "Fractions.userfor", "", true);
/*
            SetConstantDataForBoundaries(constantBoundaryData);
            SetTimeDependentDataForBoundaries(timeDependentBoundaryDataBlock, boundaryDataCollection, InterpolationType.Constant);
            SetTimeDependentDataForBoundaries(timeDependentBoundaryDataLinear, boundaryDataCollection, InterpolationType.Linear);
*/
        }

        private static NumericalScheme SetNumericalScheme1D(int numericalScheme1D)
        {
            switch (numericalScheme1D)
            {
                case 1: return NumericalScheme.Scheme1;
                case 5: return NumericalScheme.Scheme5;
                case 10: return NumericalScheme.Scheme10;
                case 15: return NumericalScheme.Scheme15;
                case 22: return NumericalScheme.Scheme22;
            }

            throw  new ArgumentOutOfRangeException("Schema " + numericalScheme1D + " not supported");
        }

        private static void CheckActiveSubstances(Dictionary<string, Dictionary<string, double>> constantData, Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> timeDependentDataBlock, Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> timeDependentDataLinear, IEnumerable<WaterQualitySubstance> activeSubstances, string typeDescription)
        {
            var substanceNames = constantData.Values.SelectMany(v => v.Keys)
                .Union(timeDependentDataBlock.Values.SelectMany(v => v.Values).SelectMany(v => v.Keys))
                .Union(timeDependentDataLinear.Values.SelectMany(v => v.Values).SelectMany(v => v.Keys))
                .Distinct();

            foreach (var substanceName in substanceNames.Where(substanceName => !activeSubstances.Any(s => s.Name.Equals(substanceName))))
            {
                Console.WriteLine(String.Format("Concentration values on {0} for the substance '{1}' will be ignored: active substance does not exist", typeDescription, substanceName));
            }
        }
/*
        private static void SetConstantDataForFractions(Dictionary<string, Dictionary<string, double>> constantFractionData, WaterQualityModel1D waterQualityModel1D)
        {
            var fractionDataCollection = waterQualityModel1D.BoundaryData.Where(bd => bd.Feature == null).ToList();

            foreach (var fractionName in constantFractionData.Keys)
            {
                // Obtain or create a new fraction data item
                var fractionDataItem = fractionDataCollection.FirstOrDefault(fd => fd.Fraction.Equals(fractionName));
                if (fractionDataItem == null)
                {
                    fractionDataItem = new WaterQualityBoundaryData(new EventedList<WaterQualitySubstance>(waterQualityModel1D.SubstanceProcessLibrary.ActiveSubstances)) { Fraction = fractionName };

                    waterQualityModel1D.BoundaryData.Add(fractionDataItem);
                    fractionDataCollection.Add(fractionDataItem);
                }

                SetConstantData(constantFractionData[fractionName], fractionDataItem);
            }
        }

        private static void SetConstantDataForBoundaries(Dictionary<string, Dictionary<string, double>> constantBoundaryData, List<WaterQualityBoundaryData> boundaryDataCollection)
        {
            foreach (var boundaryName in constantBoundaryData.Keys)
            {
                // Obtain the corresponding boundary data item
                var boundaryDataItem = GetDataItemByBoundaryName(boundaryDataCollection, boundaryName);
                if (boundaryDataItem == null) continue;

                SetConstantData(constantBoundaryData[boundaryName], boundaryDataItem);
            }
        }

        private static void SetConstantData(Dictionary<string, double> substanceConcentrationDictionary, WaterQualityBoundaryData boundaryDataItem)
        {
            // If necessary, transform the boundary data item into a constant boundary data item
            if (boundaryDataItem.DataType != WaterQualityBoundaryDataType.Constant)
            {
                boundaryDataItem.DataType = WaterQualityBoundaryDataType.Constant;
            }

            // Set the imported data to the constant boundary data item
            foreach (var substanceName in substanceConcentrationDictionary.Keys)
            {
                var substance = boundaryDataItem.ActiveSubstances.FirstOrDefault(s => s.Name.Equals(substanceName));
                if (substance == null) continue; // Note: A warning message for missing substances should already be logged (=> via the CheckActiveSubstances method)

                boundaryDataItem.Data[substance] = substanceConcentrationDictionary[substanceName];
            }
        }

        private static void SetTimeDependentDataForFractions(Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> timeDependentFractionData, WaterQualityModel1D waterQualityModel1D, InterpolationType interpolationType)
        {
            var fractionDataCollection = waterQualityModel1D.BoundaryData.Where(bd => bd.Feature == null).ToList();

            foreach (var fractionName in timeDependentFractionData.Keys)
            {
                // Obtain or create a new fraction data item
                var fractionDataItem = fractionDataCollection.FirstOrDefault(fd => fd.Fraction.Equals(fractionName));
                if (fractionDataItem == null)
                {
                    fractionDataItem = new WaterQualityBoundaryData { Fraction = fractionName };

                    waterQualityModel1D.BoundaryData.Add(fractionDataItem);
                    fractionDataCollection.Add(fractionDataItem);
                }

                SetTimeDependentData(timeDependentFractionData[fractionName], fractionDataItem, interpolationType);
            }
        }

        private static void SetTimeDependentDataForBoundaries(Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> timeDependentBoundaryData, List<WaterQualityBoundaryData> boundaryDataCollection, InterpolationType interpolationType)
        {
            var boundaryNames = timeDependentBoundaryData.Keys.ToList();
            foreach (var boundaryName in boundaryNames)
            {
                var dependentBoundaryData = timeDependentBoundaryData[boundaryName];

                // remove processed value (memory optimization)
                timeDependentBoundaryData.Remove(boundaryName);

                // Obtain the corresponding boundary data item
                var boundaryDataItem = GetDataItemByBoundaryName(boundaryDataCollection, boundaryName);
                if (boundaryDataItem == null) continue;

                // Set the time dependent data for the corresponding boundary data item

                SetTimeDependentData(dependentBoundaryData, boundaryDataItem, interpolationType);
            }
        }

        private static void SetTimeDependentData(Dictionary<DateTime, Dictionary<string, double>> timeDependentBoundaryData, WaterQualityBoundaryData boundaryData, InterpolationType interpolationType)
        {
            if (!timeDependentBoundaryData.Values.Any())
            {
                return;
            }

            // If necessary, transform the boundary data item into a time dependent boundary data item
            if (boundaryData.DataType != WaterQualityBoundaryDataType.TimeSeries)
            {
                boundaryData.DataType = WaterQualityBoundaryDataType.TimeSeries;
            }

            var timeSeries = (TimeSeries)boundaryData.Data;
            var substanceComponentLookup = timeSeries.Components.ToDictionaryWithErrorDetails("waq timeseries", c => c.Name, c => c);

            // If necessary, adapt the interpolation type of the boundary data item)
            if (timeSeries.Time.InterpolationType != interpolationType)
            {
                timeSeries.Time.InterpolationType = interpolationType;
            }

            // set times
            timeSeries.Time.SetValues(timeDependentBoundaryData.Keys);

            // set component values
            foreach (var substanceName in timeDependentBoundaryData.Values.First().Keys)
            {
                if (!substanceComponentLookup.ContainsKey(substanceName)) continue;

                var component = substanceComponentLookup[substanceName];
                var values = timeDependentBoundaryData.Values.Select(v => v[substanceName]);

                component.Values.FireEvents = false;
                component.SetValues(values);
                component.Values.FireEvents = true;
            }
        }*/
    }
}