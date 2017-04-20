using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using Deltares.OpenMI2.Oatc.Sdk.Backbone;
using Deltares.OpenMI2.Oatc.Sdk.Buffer;
using DeltaShell.OpenMIWrapper.Tests;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using OpenMI.Standard2;
using OpenMI.Standard2.TimeSpace;

namespace DeltaShell.OpenMI2Wrapper.Tests
{
    [TestFixture]
    public class OpenMI2WrapperTests
    {
        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void CheckOutputSimpleModelA()
        {
            try
            {
                DeltaShellOpenMI2TimeSpaceComponent.GetAdditionalPlugins = GetAdditionalPlugins;
                const string testDataName = "SimpleModelA";
                // use same testmodel as for OpenMI2
                string testDataDir = TestHelper.GetTestDataPath(typeof(OpenMIWrapperTests).Assembly, testDataName);
                const string testRunDir = testDataName + "-Out";

                ITimeSpaceComponent timeSpaceComponent = CreateTimeSpaceComponent(testDataDir, testRunDir);

                Assert.AreEqual(4, timeSpaceComponent.Inputs.Count, "#Inputs");
                Assert.AreEqual(201, timeSpaceComponent.Outputs.Count, "#Outputs");
                ITimeSpaceInput inputExchangeItem3 = (ITimeSpaceInput) timeSpaceComponent.Inputs[3];
                ITimeSpaceOutput outputExchangeItem172 = (ITimeSpaceOutput)timeSpaceComponent.Outputs[172];
                ITimeSpaceOutput outputExchangeItem195 = (ITimeSpaceOutput)timeSpaceComponent.Outputs[195];
                Assert.AreEqual("Node003", inputExchangeItem3.ElementSet().Caption);
                Assert.AreEqual("Water level", inputExchangeItem3.Quantity().Caption);
                Assert.AreEqual("grid_point.Channel2_963.630", outputExchangeItem172.ElementSet().Caption);
                Assert.AreEqual("Number of iterations", outputExchangeItem172.Quantity().Caption);
                Assert.AreEqual("reach_segment", outputExchangeItem195.ElementSet().Caption);
                Assert.AreEqual("FloodPlain2 Chezy values", outputExchangeItem195.Quantity().Caption);

                // check value(s) after initialization
                const double lateralExtraction = -9.0d;
                IBaseOutput lateralAout = FindOutputItem(timeSpaceComponent, "LateralSource - LatDisch_a - Discharge (l)");
                Assert.AreEqual(lateralExtraction, (double)lateralAout.Values.GetValue(new[] { 0, 0 }), 1e-6,
                                "correct value for lateral_a discharge");

                // set some input values: THIS DOES NOT WORK: uncommenting these lines have no effect on computation.  
                //IBaseInput lateralAIn = FindInputItem(timeSpaceComponent, "LateralSource - LatDisch_a - Discharge (l)");
                //const double lateralAInValue = -60000.0d;
                //lateralAIn.Values = new ValueSet(new List<IList> {new[] {lateralAInValue}}); // no provider -> direct input.

                // check value(s) after one time step

                timeSpaceComponent.Update();

                Assert.AreEqual(-8.997222d, (double)lateralAout.Values.GetValue(new[] { 0, 0 }), 1e-6, "realized value for lateral_A");

                timeSpaceComponent.Update();

                // check value(s) after one time step

                Assert.AreEqual(-8.994445d, (double)lateralAout.Values.GetValue(new[] { 0, 0 }), 1e-6, "realized value for lateral_A");

                IBaseOutput obsPointIIout = FindOutputItem(timeSpaceComponent, "ObservationPoint - ObsLoc_II - Water level (op)");
                Assert.AreEqual(0.236184759d, (double)obsPointIIout.Values.GetValue(new[] { 0, 0 }), 1e-6, "correct value for obs_II water_level");

                IBaseOutput waterLevelOnGrid = FindOutputItem(timeSpaceComponent, "Feature - grid_point - Water level");
                Assert.AreEqual(0.0464d, (double)waterLevelOnGrid.Values.GetValue(new[] { 0, 11 }), 1e-3, "correct value for network water_level");

            }
            finally
            {
                DeltaShellOpenMI2TimeSpaceComponent.GetAdditionalPlugins = null;
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("OpenMI2.0 adapted output not working in the expected way yet")]
        public void CheckGetValuesSimpleModelA()
        {
            try
            {
                DeltaShellOpenMI2TimeSpaceComponent.GetAdditionalPlugins = GetAdditionalPlugins;
                const string testDataName = "SimpleModelA";
                // use same testmodel as for OpenMI2
                var testDataDir = Path.Combine(TestHelper.GetDataDir() + "../../OpenMI/DeltaShell.OpenMIWrapper.Tests",
                                               testDataName);
                const string testRunDir = testDataName + "-GV";

                ITimeSpaceComponent timeSpaceComponent = CreateTimeSpaceComponent(testDataDir, testRunDir);

                IBaseOutput lateralAout = FindOutputItem(timeSpaceComponent, "LateralSource - LatDisch_a - Discharge (l)");

                IAdaptedOutputFactory adaptedOutputFactory = new TimeBufferFactory("Time Buffers");

                IIdentifiable timeInterpolatorId = adaptedOutputFactory.GetAvailableAdaptedOutputIds(lateralAout, null)[0];
                IBaseAdaptedOutput interpolatedLateralAout = adaptedOutputFactory.CreateAdaptedOutput(timeInterpolatorId, lateralAout, null);

                var querySpec = new Input("query", lateralAout.Quantity(), lateralAout.ElementSet());
                querySpec.TimeSet = new TimeSet();
                querySpec.TimeSet.SetSingleTime(timeSpaceComponent.TimeExtent.TimeHorizon.End());
                IBaseValueSet baseValueSet = interpolatedLateralAout.GetValues(querySpec);
                double lateralAoutValueAtLastTimeStep = (double)baseValueSet.GetValue(new[] { 0, 0 });
                Assert.AreEqual(100, lateralAoutValueAtLastTimeStep, 1e-6);
            }
            finally
            {
                DeltaShellOpenMI2TimeSpaceComponent.GetAdditionalPlugins = null;
            }
        }

        private static ITimeSpaceComponent CreateTimeSpaceComponent(string sourcePath, string testRunDir)
        {
            FileUtils.DeleteIfExists(testRunDir);
            Directory.CreateDirectory(testRunDir);
            FileUtils.CopyDirectory(sourcePath, testRunDir, ".svn");

            ITimeSpaceComponent timeSpaceComponent = new DeltaShellOpenMI2TimeSpaceComponent();

            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                foreach (IArgument argument in timeSpaceComponent.Arguments)
                {
                    if (argument.Id.Equals("DsProjFilePath"))
                    {
                        argument.Value = Path.Combine(testRunDir, "SimpleModelA.dsproj");
                    }
                    else if (argument.Id.Equals("ModelName"))
                    {
                        argument.Value = "simpleFlowModel";
                    }
                    else if (argument.Id.Equals("SplitSpecificElementSets"))
                    {
                        argument.Value = "grid_point";
                    }
                }
                timeSpaceComponent.Initialize();
                timeSpaceComponent.Prepare();
            }

            return timeSpaceComponent;
        }

        private IBaseInput FindInputItem(ITimeSpaceComponent timeSpaceComponent, string InputId)
        {
            return timeSpaceComponent.Inputs.FirstOrDefault(baseInput => baseInput.Id.Equals(InputId));
        }

        private IBaseOutput FindOutputItem(ITimeSpaceComponent timeSpaceComponent, string outputId)
        {
            return timeSpaceComponent.Outputs.FirstOrDefault(baseOutput => baseOutput.Id.Equals(outputId));
        }

        private IEnumerable<ApplicationPlugin> GetAdditionalPlugins()
        {
            return new ApplicationPlugin[]
                {
                    new NHibernateDaoApplicationPlugin(),
                    new NetCdfApplicationPlugin(),
                    new CommonToolsApplicationPlugin(),
                    new ScriptingApplicationPlugin(),
                    new SharpMapGisApplicationPlugin(),
                    new NetworkEditorApplicationPlugin(),
                    new HydroModelApplicationPlugin(),
                    new RainfallRunoffApplicationPlugin(),
                    new WaterFlowModel1DApplicationPlugin(),
                    new RealTimeControlApplicationPlugin(),
                    new WaterQualityModelApplicationPlugin()
                };
        }
    }
}
