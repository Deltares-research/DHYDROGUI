using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class IncludeFileFactoryBaseTest
    {
        private class TestFactory : IncludeFileFactoryBase
        {
            public override string CreateParametersInclude(WaqInitializationSettings initializationSettings)
            {
                throw new NotImplementedException();
            }

            protected override string CreateSpatialInitialConditionsFileContents(WaqInitializationSettings initializationSettings)
            {
                return "<insert spatial data here>";
            }
        }

        #region Block 1

        [Test]
        public void TestCreateT0Include()
        {
            const string expectedString = "'T0: 2010.01.01 13:12:11  (scu=       1s)'";

            TestFactory factory = new TestFactory();
            var dispersionInclude = factory.CreateT0Include(new DateTime(2010, 1, 1, 13, 12, 11));
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        [Test]
        public void TestCreateSubstanceListInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();
            substanceProcessLib.Substances.Add(new WaterQualitySubstance
            {
                Name = "ActiveSubstance",
                Active = true,
                Description = "An active substance"
            });
            substanceProcessLib.Substances.Add(new WaterQualitySubstance
            {
                Name = "InActiveSubstance",
                Active = false,
                Description = "An inactive substance"
            });

            string expectedString = "; number of active and inactive substances" + Environment.NewLine +
                                    "1             1" + Environment.NewLine +
                                    "        ; active substances" + Environment.NewLine +
                                    "1            'ActiveSubstance' ;An active substance" + Environment.NewLine +
                                    "        ; passive substances" + Environment.NewLine +
                                    "2            'InActiveSubstance' ;An inactive substance" + Environment.NewLine;

            TestFactory factory = new TestFactory();
            var dispersionInclude = factory.CreateSubstanceListInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        #endregion Block 1
        #region Block 2

        [Test]
        public void TestCreateNumSettingsInclude()
        {
            var expectedString = "22.63 ; integration option" + Environment.NewLine +
                                 "; detailed balance options" + Environment.NewLine +
                                 "BAL_LUMPPROCESSES BAL_NOLUMPTRANSPORT BAL_LUMPLOADS" + Environment.NewLine +
                                 "BAL_NOSUPPRESSSPACE BAL_SUPPRESSTIME" + Environment.NewLine;

            var waqSettings = new WaterQualityModelSettings
            {
                Balance = true,
                BalanceUnit = BalanceUnit.Gram,
                LumpLoads = true,
                LumpTransport = false,
                LumpProcesses = true,
                SuppressSpace = false,
                SuppressTime = true,
                NumericalScheme = NumericalScheme.Scheme22,
                UseFirstOrder = false,
                NoDispersionOverOpenBoundaries = true,
                NoDispersionIfFlowIsZero = false
            };

            TestFactory factory = new TestFactory();
            var dispersionInclude = factory.CreateNumSettingsInclude(waqSettings);
            Assert.AreEqual(expectedString, dispersionInclude);

            waqSettings.BalanceUnit = BalanceUnit.GramPerSquareMeter;
            expectedString = "22.63 ; integration option" + Environment.NewLine +
                             "; detailed balance options" + Environment.NewLine +
                             "BAL_UNITAREA" + Environment.NewLine +
                             "BAL_LUMPPROCESSES BAL_NOLUMPTRANSPORT BAL_LUMPLOADS" + Environment.NewLine +
                             "BAL_NOSUPPRESSSPACE BAL_SUPPRESSTIME" + Environment.NewLine;
            dispersionInclude = factory.CreateNumSettingsInclude(waqSettings);
            Assert.AreEqual(expectedString, dispersionInclude);

            waqSettings.BalanceUnit = BalanceUnit.GramPerCubicMeter;
            expectedString = "22.63 ; integration option" + Environment.NewLine +
                             "; detailed balance options" + Environment.NewLine +
                             "BAL_UNITVOLUME" + Environment.NewLine +
                             "BAL_LUMPPROCESSES BAL_NOLUMPTRANSPORT BAL_LUMPLOADS" + Environment.NewLine +
                             "BAL_NOSUPPRESSSPACE BAL_SUPPRESSTIME" + Environment.NewLine;
            dispersionInclude = factory.CreateNumSettingsInclude(waqSettings);
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        [Test]
        public void TestCreateOutputTimersInclude()
        {
            string expectedString = "; output control (see DELWAQ-manual)" + Environment.NewLine +
                                    "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss  dddhhmmss" + Environment.NewLine +
                                    "  2010/01/01-00:00:00  2010/01/10-00:00:00  001000000 ;  start, stop and step for balance output" + Environment.NewLine +
                                    "  2010/02/01-00:00:00  2010/02/10-00:00:00  000010000 ;  start, stop and step for map output" + Environment.NewLine +
                                    "  2010/03/01-00:00:00  2010/03/10-00:00:00  000000100 ;  start, stop and step for his output" + Environment.NewLine;

            var waqSettings = new WaterQualityModelSettings
            {
                HisStartTime = new DateTime(2010, 3, 1),
                HisStopTime = new DateTime(2010, 3, 10),
                HisTimeStep = new TimeSpan(0, 0, 1, 0),
                MapStartTime = new DateTime(2010, 2, 1),
                MapStopTime = new DateTime(2010, 2, 10),
                MapTimeStep = new TimeSpan(0, 1, 0, 0),
                BalanceStartTime = new DateTime(2010, 1, 1),
                BalanceStopTime = new DateTime(2010, 1, 10),
                BalanceTimeStep = new TimeSpan(1, 0, 0, 0),
            };

            TestFactory factory = new TestFactory();
            var outputTimersInclude = factory.CreateOutputTimersInclude(waqSettings);
            Assert.AreEqual(expectedString, outputTimersInclude);
        }

        [Test]
        public void TestCreateSimTimersInclude()
        {
            string expectedString = "  2010/01/01-00:00:00 ; start time" + Environment.NewLine +
                                    "  2010/01/05-00:00:00 ; stop time" + Environment.NewLine +
                                    "  0 ; timestep constant" + Environment.NewLine +
                                    "  001000000 ; timestep";

            var initializationSettings = new WaqInitializationSettings
            {
                SimulationStartTime = new DateTime(2010, 1, 1),
                SimulationStopTime = new DateTime(2010, 1, 5),
                SimulationTimeStep = new TimeSpan(1, 0, 0, 0)
            };

            TestFactory factory = new TestFactory();
            var simTimersInclude = factory.CreateSimTimersInclude(initializationSettings);
            Assert.AreEqual(expectedString, simTimersInclude);
        }

        #endregion Block 2
        #region Block 7

        [Test]
        public void TestCreateProcessesInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();
            substanceProcessLib.Processes.Add(new WaterQualityProcess { Name = "Process A" });
            substanceProcessLib.Processes.Add(new WaterQualityProcess { Name = "Process B" });

            string expectedString = "CONSTANTS 'ACTIVE_Process A' DATA 0" + Environment.NewLine +
                                    "CONSTANTS 'ACTIVE_Process B' DATA 0" + Environment.NewLine;

            var factory = new TestFactory();
            var dispersionInclude = factory.CreateProcessesInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        [Test]
        public void TestCreateConstantsInclude()
        {
            var processCoefficients = new[]
            {
                WaterQualityFunctionFactory.CreateConst("A", 2, "A", "mg/L", "A"),
                WaterQualityFunctionFactory.CreateTimeSeries("B", 5.5, "B", "g/L", "B"),
                WaterQualityFunctionFactory.CreateNetworkCoverage("C", 10.5, "C", "mg/mL", "C"),
                WaterQualityFunctionFactory.CreateConst("D", 3.5, "D", "g/mL", "D")
            };

            string expectedString = "CONSTANTS 'A' DATA 2" + Environment.NewLine +
                                    "CONSTANTS 'D' DATA 3.5" + Environment.NewLine;

            var factory = new TestFactory();
            var createConstantsInclude = factory.CreateConstantsInclude(processCoefficients);
            Assert.AreEqual(expectedString, createConstantsInclude);
        }

        [Test]
        public void TestCreateFunctionsInclude()
        {
            var timeSeries1 = WaterQualityFunctionFactory.CreateTimeSeries("A", 2, "A", "mg/L", "A");
            timeSeries1.Arguments[0].InterpolationType = InterpolationType.Linear;
            timeSeries1[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.12;
            timeSeries1[new DateTime(2010, 1, 1, 0, 10, 0)] = 0.16;

            var timeSeries2 = WaterQualityFunctionFactory.CreateTimeSeries("D", 3.5, "D", "g/mL", "D");
            timeSeries2.Arguments[0].InterpolationType = InterpolationType.Constant;
            timeSeries2[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.12;
            timeSeries2[new DateTime(2010, 1, 1, 0, 10, 0)] = 0.18;
            timeSeries2[new DateTime(2010, 1, 1, 0, 20, 0)] = 0.14;

            var processCoefficients = new[]
            {
                timeSeries1,
                WaterQualityFunctionFactory.CreateConst("B", 5.5, "B", "g/L", "B"),
                WaterQualityFunctionFactory.CreateNetworkCoverage("C", 10.5, "C", "mg/mL", "C"),
                timeSeries2
            };

            string expectedString = "FUNCTIONS" + Environment.NewLine +
                                    "A" + Environment.NewLine +
                                    "LINEAR DATA" + Environment.NewLine +
                                    "2010/01/01-00:00:00 0.12" + Environment.NewLine +
                                    "2010/01/01-00:10:00 0.16" + Environment.NewLine +
                                    Environment.NewLine +
                                    "FUNCTIONS" + Environment.NewLine +
                                    "D" + Environment.NewLine +
                                    "DATA" + Environment.NewLine +
                                    "2010/01/01-00:00:00 0.12" + Environment.NewLine +
                                    "2010/01/01-00:10:00 0.18" + Environment.NewLine +
                                    "2010/01/01-00:20:00 0.14" + Environment.NewLine +
                                    Environment.NewLine;

            var factory = new TestFactory();
            var functionsInclude = factory.CreateFunctionsInclude(processCoefficients);
            Assert.AreEqual(expectedString, functionsInclude);
        }

        #endregion Block 7
        #region Block 8

        [Test]
        public void TestCreateInitialConditionsIncludeWithoutInitialConditionsAvailable()
        {
            // setup
            var settings = new WaqInitializationSettings { InitialConditions = new List<IFunction>() };

            var factory = new TestFactory();

            // call
            var fileContents = factory.CreateInitialConditionsInclude(settings);

            // assert
            Assert.AreEqual(string.Empty, fileContents);
        }

        [Test]
        public void TestCreateInitialConditionsWithConstantInitialConditions()
        {
            // setup
            var settings = new WaqInitializationSettings
            {
                InitialConditions = new List<IFunction>
                {
                    WaterQualityFunctionFactory.CreateConst("A", 1.5, "A", "mg/L", "A"),
                    WaterQualityFunctionFactory.CreateConst("B", 2.9, "B", "g/L", "B"),
                    WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("C", 99.33, "C", "test", "C")
                }
            };

            var factory = new TestFactory();

            string expectedString2 = "MASS/M2" + Environment.NewLine +
                                     "INITIALS" + Environment.NewLine +
                                     "'A' 'B'" + Environment.NewLine +
                                     "DEFAULTS" + Environment.NewLine +
                                     "1.5 2.9" + Environment.NewLine +
                                     "<insert spatial data here>";

            // call
            var fileContents = factory.CreateInitialConditionsInclude(settings);

            // assert
            Assert.AreEqual(expectedString2, fileContents);
        }
        
        #endregion Block 8
        #region Block 9

        [Test]
        public void TestCreateMapVarInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();

            substanceProcessLib.OutputParameters.AddRange(
                new[]
                    {
                        new WaterQualityOutputParameter { Name = "Winddir", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "Vwind", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "Temp", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "Rad", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "Volume", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "Surf", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "Theta", ShowInMap = true },
                        new WaterQualityOutputParameter { Name = "NotSelected1" },
                        new WaterQualityOutputParameter { Name = "NotSelected2" }
                    });

            string expectedString = "2 ; perform default output and extra parameters listed below" + Environment.NewLine +
                                    "7 ; number of parameters listed" + Environment.NewLine +
                                    " 'Winddir'" + Environment.NewLine +
                                    " 'Vwind'" + Environment.NewLine +
                                    " 'Temp'" + Environment.NewLine +
                                    " 'Rad'" + Environment.NewLine +
                                    " 'Volume'" + Environment.NewLine +
                                    " 'Surf'" + Environment.NewLine +
                                    " 'Theta'" + Environment.NewLine;

            var factory = new IncludeFileFactory();
            var mapVarInclude = factory.CreateMapVarInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, mapVarInclude);
        }

        [Test]
        public void TestCreateHisVarInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();

            substanceProcessLib.OutputParameters.AddRange(
                new[]
                    {
                        new WaterQualityOutputParameter { Name = "Winddir", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "Vwind", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "Temp", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "Rad", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "Volume", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "Surf", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "Theta", ShowInHis = true },
                        new WaterQualityOutputParameter { Name = "NotSelected1" },
                        new WaterQualityOutputParameter { Name = "NotSelected2" }
                    });

            string expectedString = "2 ; perform default output and extra parameters listed below" + Environment.NewLine +
                                    "7 ; number of parameters listed" + Environment.NewLine +
                                    " 'Winddir' 'volume'" + Environment.NewLine +
                                    " 'Vwind' 'volume'" + Environment.NewLine +
                                    " 'Temp' 'volume'" + Environment.NewLine +
                                    " 'Rad' 'volume'" + Environment.NewLine +
                                    " 'Volume' ' '" + Environment.NewLine +
                                    " 'Surf' ' '" + Environment.NewLine +
                                    " 'Theta' 'volume'" + Environment.NewLine;

            var factory = new IncludeFileFactory();
            var hisVarInclude = factory.CreateHisVarInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, hisVarInclude);
        }

        #endregion Block 9
    }
}