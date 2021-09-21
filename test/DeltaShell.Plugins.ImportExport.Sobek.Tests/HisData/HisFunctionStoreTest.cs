using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.ImportExport.Sobek.HisData;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;
using HisFileReader = DeltaShell.Sobek.Readers.Readers.HisFileReader;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.HisData
{
    [TestFixture]
    public class HisFunctionStoreTest
    {

        [Test]
        public void HisFunctionStoreFunctions()
        {
            string uri = Path.Combine("HisData", "flowhis.his");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);

            var hisFileReader = new HisFileReader(path);
            var nComponents = hisFileReader.GetHisFileHeader.Components.Count;
            hisFileReader.Close();

           using (var hisFunctionStore = new HisFunctionStore(path))
           {
                var function = hisFunctionStore.Functions.First(f => f.Name == "flowhis As Function");

                Assert.AreEqual(2, function.Arguments.Count); //time and locations
                Assert.AreEqual(nComponents, function.Components.Count);
           }
        }

        [Test]
        public void GetArgumentValues()
        {
            string uri = Path.Combine("HisData", "flowhis.his");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);

            var hisFileReader = new HisFileReader(path);
            var timeSteps = hisFileReader.GetHisFileHeader.TimeSteps;
            var locations = hisFileReader.GetHisFileHeader.Locations;
            hisFileReader.Close();

            using (var hisFunctionStore = new HisFunctionStore(path))
            {

                var function = hisFunctionStore.Functions.First(f => f.Name == "flowhis As Function");
                var variableTime = function.Arguments.First(a => a.Name == "time");
                var variableLocations = function.Arguments.First(a => a.Name == "locations");

                Assert.AreEqual(timeSteps, variableTime.Values);
                Assert.AreEqual(locations, variableLocations.Values);
            }
        }

        [Test]
        public void GetValues()
        {
            string uri = Path.Combine("HisData", "flowhis.his");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);
            var componentName = "Water level";

            var hisFileReader = new HisFileReader(path);
            var waterLevelValues = hisFileReader.ReadAllData(componentName).Select(row => row.Value).ToArray();
            hisFileReader.Close();

            using (var hisFunctionStore = new HisFunctionStore(path))
            {
                var function = hisFunctionStore.Functions.First(f => f.Name == "flowhis As Function");
                var variableWaterLevel = function.Components.First(c => c.Name == componentName);

                Assert.AreEqual(waterLevelValues, variableWaterLevel.Values);
            }
        }

        [Test]
        public void GetValuesOfOneTimeStep()
        {
            string uri = Path.Combine("HisData", "flowhis.his");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);
            var componentName = "Water level";

            var hisFileReader = new HisFileReader(path);
            var timestep = hisFileReader.GetHisFileHeader.TimeSteps[5];
            var values = hisFileReader.ReadTimeStep(timestep,componentName).Select(row => row.Value).ToArray();
            hisFileReader.Close();

            using (var hisFunctionStore = new HisFunctionStore(path))
            {
                var function = hisFunctionStore.Functions.First(f => f.Name == "flowhis As Function");
                var variableTimeSteps = function.Arguments.First(c => c.Name == "time");
                var variableWaterLevel = function.Components.First(c => c.Name == componentName);
                var timestepFilter = new VariableValueFilter<DateTime>(variableTimeSteps, timestep);

                Assert.AreEqual(values, variableWaterLevel.GetValues(timestepFilter));
            }
        }

        [Test]
        public void GetValuesOfOneLocation()
        {
            string uri = Path.Combine("HisData", "flowhis.his");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);
            var componentName = "Water level";

            var hisFileReader = new HisFileReader(path);
            var location = hisFileReader.GetHisFileHeader.Locations[3];
            var values = hisFileReader.ReadLocation(location, componentName).Select(row => row.Value).ToArray();
            hisFileReader.Close();

            using (var hisFunctionStore = new HisFunctionStore(path))
            {
                var function = hisFunctionStore.Functions.First(f => f.Name == "flowhis As Function");
                var variableLocations = function.Arguments.First(c => c.Name == "locations");
                var variableWaterLevel = function.Components.First(c => c.Name == componentName);
                var locationFilter = new VariableValueFilter<string>(variableLocations, location);

                Assert.AreEqual(values, variableWaterLevel.GetValues(locationFilter));
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void ShowInFunctionView()
        {
            string uri = Path.Combine("HisData", "flowhis.his");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);

            using (var hisFunctionStore = new HisFunctionStore(path))
            {
                var function = hisFunctionStore.Functions.First(f => f.Name == "flowhis As Function");
                var view = new FunctionView() {Data = function};

                //WindowsFormsTestHelper.ShowModal(view);
            }

        }

        [Test, Category(TestCategory.WorkInProgress)] // crashes in Test All System.IO.IOException : The process cannot access the file 'C:\BuildAgent\work\DeltaShell\test-d
        public void HisFunctionStoreWithNetworkCoverage()
        {
            string uri = Path.Combine("HisData", "HisAndNetwork");
            uri = Path.Combine(uri, "CALCPNT.HIS");
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), uri);
            var componentName = "Waterlevel  (m AD)";
            var indexLocation = 5;

            var hisFileReader = new HisFileReader(path);
            var location = hisFileReader.GetHisFileHeader.Locations[indexLocation];
            var values = hisFileReader.ReadLocation(location, componentName).Select(row => row.Value).ToArray();
            hisFileReader.Close();

            using (var hisFunctionStore = new HisFunctionStore(path))
            {
                var function = hisFunctionStore.Functions.OfType<INetworkCoverage>().First();
                var variableLocations = function.Arguments.First(c => c.Name == "locations");
                var variableWaterLevel = function.Components.First(c => c.Name == componentName);
                var locationFilter = new VariableValueFilter<INetworkLocation>(variableLocations,
                                                                              (INetworkLocation)
                                                                              variableLocations.Values[indexLocation]);

                Assert.AreEqual(values, variableWaterLevel.GetValues(locationFilter));
            }
        }
    }
}
