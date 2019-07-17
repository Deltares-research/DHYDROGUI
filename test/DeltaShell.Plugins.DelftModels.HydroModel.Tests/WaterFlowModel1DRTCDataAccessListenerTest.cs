using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DRTCDataAccessListenerTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestOnPreLoad_RemovingInterpolationNoneForTimeRulesIfSetInDatabase()
        {
            var testProjectPath = TestHelper.GetTestFilePath(@"RTCInterpolationNoneForTimeRules\Project3.dsproj");
            Assert.IsTrue(File.Exists(testProjectPath));

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                app.Run();

                app.OpenProject(testProjectPath);
                Assert.NotNull(app.Project);

                var integratedModel = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                Assert.NotNull(integratedModel);

                var realTimeControlModel = integratedModel.Activities
                        .OfType<RealTimeControlModel>().FirstOrDefault();
                Assert.NotNull(realTimeControlModel);

                Assert.AreEqual(InterpolationType.Linear,
                    ((TimeRule) realTimeControlModel.ControlGroups[0].Rules[0]).InterpolationOptionsTime);
            }
        }
    }
}