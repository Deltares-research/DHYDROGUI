using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqModelApiTest
    {
        private IWaqModelApi modelApi;
        private string log = "";

        [SetUp]
        public void SetUp()
        {
            modelApi = new WaqModelApi();
        }

        [TearDown]
        public void TearDown()
        {
            Trace.WriteLine(log);
            log = "";

            modelApi.WaqFinalize();
            modelApi = null;
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [Ignore("TODO: Api should always be ran via a remote instance container")]
        public void InitializeByWorkFiles()
        {
            var workdir = Path.Combine(TestHelper.GetDataDir(), "TestModelApi");

            try
            {
                ConsoleRedirector.Attach(HandleApiConsoleMessages, HandleApiConsoleMessages, true);
                Directory.SetCurrentDirectory(workdir);

                modelApi.WaqInitialize_By_Id("deltashell");
                modelApi.WaqPerformTimeStep();
                modelApi.GetWQCurrentValue("OXY", 20);
            }
            catch (Exception ex)
            {
                Assert.Fail("An error has occurred :" + ex.Message);
            }
            finally
            {
                ConsoleRedirector.Detatch();
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [Ignore("TODO: Api should always be ran via a remote instance container")]
        public void RunDelwaq2ApiTwice()
        {
            var workdir = Path.Combine(TestHelper.GetDataDir(), "TestModelApi");

            var oxyValuesRun1 = new double[20];
            var oxyValuesRun2 = new double[20];

            try
            {
                ConsoleRedirector.Attach(HandleApiConsoleMessages, HandleApiConsoleMessages, true);
                Directory.SetCurrentDirectory(workdir);

                modelApi.WaqInitialize_By_Id("deltashell");
                modelApi.WaqPerformTimeStep();
                oxyValuesRun1 = modelApi.GetWQCurrentValue("OXY", 20);
                modelApi.WaqFinalize();
                
                Directory.SetCurrentDirectory(workdir);

                modelApi.WaqInitialize_By_Id("deltashell");
                modelApi.WaqPerformTimeStep();
                oxyValuesRun2 = modelApi.GetWQCurrentValue("OXY", 20);
            }
            catch (Exception ex)
            {
                Assert.Fail("An error has occurred :" + ex.Message);
            }
            finally
            {
                ConsoleRedirector.Detatch();
            }

            Assert.AreEqual(oxyValuesRun1, oxyValuesRun2);
        }

        private void HandleApiConsoleMessages(object sender, ProgressChangedEventArgs eventArgs)
        {
            log += (string) eventArgs.UserState;
        }

        [Test]
        [Ignore("Model api crashes during initialize")]
        public void SimpleTestCase()
        {
            // segments: 
            //
            //          10 km
            //       <---------->
            //        __________  __________  __________  __________  __________ 
            //     ^ |          ||          ||          ||          ||          | 
            // 1km | |    -1    ||     1    ||     2    ||     3    ||    -2    | 
            //     v |__________||__________||__________||__________||__________| 
            //
            // Segment dimensions : height = 10 km 
            //                      width = 1 km 
            //                      depth = 10 m
            //                      volume = 1.0e4 m3
            //                      exchange area = 1.0e4 m2
            //
            // Flow rate: 0.2e4 m3/s (0.2 m/s x area)
            // Lengths: 5 km on both sides
            // Residence time: 1.0e8/0.2e4 = 50000 s

            try
            {
                ConsoleRedirector.Attach(HandleApiConsoleMessages, HandleApiConsoleMessages, true);
                var pointers = new[]
                               {
                                 //from, to, from-1, to+1
                                   -1  ,  1,    0  ,   2,
                                    1  ,  2,   -1  ,   3, 
                                    2  ,  3,    1  ,  -2, 
                                    3  , -2,    2  ,   0 
                               };

                // four exchanges in first direction (pointers/4), others are 0 (no other directions distinguished)
                Assert.IsTrue(modelApi.DefineWQSchematisation(3, pointers, new[] { 4, 0, 0, 0 }));

                // define boundary locations
                Assert.IsTrue(modelApi.DefineWQDischargeLocations(new[] { 1, 2 }, 2));

                var lengths = new[]
                              {
                                //from length, to length --> (length = segment length/2)
                                     5000.0  , 5000.0, 
                                     5000.0  , 5000.0, 
                                     5000.0  , 5000.0, 
                                     5000.0  , 5000.0
                              };

                Assert.IsTrue(modelApi.DefineWQDispersion(new[] { 100.0, 0.0, 0.0 }, lengths));

                // Important: for initializing the mass per segment
                var initialVolume = Enumerable.Repeat(1.0e8, 3).ToArray();
                Assert.IsTrue(modelApi.SetWQInitialVolume(initialVolume));

                // integration method 5
                // use flows and dispersion
                // use dispersion over open boundaries
                // use first order approximation over open boundaries
                // No Forester filter
                // No Anticreep filter
                Assert.IsTrue(modelApi.SetWQIntegrationOptions(5, true, true, true, false, false));

                const int startTimeTimers = 0;
                const int stopTimeTimers = 1000000;
                const int timeStepTimers = 10000;

                Assert.IsTrue(modelApi.SetWQSimulationTimes(startTimeTimers, stopTimeTimers, timeStepTimers));

                // set timers for his, map and balance files
                Assert.IsTrue(modelApi.DefineWQMonitoringLocations(new[] { 1, 2, 3 }, new[] { "1", "2", "3" }, 3));
                Assert.IsTrue(modelApi.SetWQOutputTimers(1, startTimeTimers, stopTimeTimers, timeStepTimers));
                Assert.IsTrue(modelApi.SetWQOutputTimers(2, startTimeTimers, stopTimeTimers, timeStepTimers));
                Assert.IsTrue(modelApi.SetWQOutputTimers(3, startTimeTimers, stopTimeTimers, timeStepTimers));

                var substances = new[] { "Salinity", "Temperature", "OXY" };
                var parameters = new[] { "OXYSAT" };
                var processes = new[] { "SaturOXY", "ReaerOXY" };

                Assert.IsTrue(modelApi.DefineWQProcesses(substances, 3, 3, parameters, 1, processes, 2));

                // set initial values for substances
                Assert.IsTrue(modelApi.SetWQCurrentValueScalarInit("Salinity", 30.0));
                Assert.IsTrue(modelApi.SetWQCurrentValueScalarInit("Temperature", 20.0));
                Assert.IsTrue(modelApi.SetWQCurrentValueScalarInit("OXY", 3.0));

                Assert.IsTrue(modelApi.WaqInitialize());

                var flow = Enumerable.Repeat(0.2e4, 3).ToArray();
                var area = Enumerable.Repeat(1.0e4, 3).ToArray();

                Assert.IsTrue(modelApi.SetWQFlowData(initialVolume, area, flow));

                // set boundary discharge (discharge rate (m3/s),other values are concentration of substances)
                Assert.IsTrue(modelApi.SetWQWasteLoadValues(0, new[] { 1.0, 0.0, 1000000.0, 0.0 }));
                Assert.IsTrue(modelApi.SetWQWasteLoadValues(1, new[] { 1.0, 1000000.0, 0.0, 0.0 }));

                // active substances concentrations for boundaries : Salinity, Temperature, OXY
                Assert.IsTrue(modelApi.SetWQBoundaryConditions(1, new[] { 30.0, 10.0, 3.0 }));
                Assert.IsTrue(modelApi.SetWQBoundaryConditions(2, new[] { 0.0, 20.0, 3.0 }));

                // do all 10 timesteps
                for (var i = 0; i < 10; i++)
                {
                    var salinityValue = new double[3];
                    var temperatureValue = new double[3];
                    var oxygenValue = new double[3];


                    Assert.IsTrue(modelApi.WaqPerformTimeStep());
                    modelApi.GetWQCurrentValue("Salinity", 3);
                    modelApi.GetWQCurrentValue("Temperature", 3);
                    modelApi.GetWQCurrentValue("OXY", 3);

                    Console.WriteLine("Salinity = " + salinityValue);
                    Console.WriteLine("Temperature = " + temperatureValue);
                    Console.WriteLine("OXY = " + oxygenValue);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("An error has occurred :" + ex.Message);
            }
            finally
            {
                ConsoleRedirector.Detatch();
            }
        }
    }
}