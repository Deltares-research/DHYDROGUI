using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using DelftTools.TestUtils;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterFlowModel1DMergeIntegrationTest
    {
        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreTestedIfTheyCanBeMergedWhenCanMergeThenTrue()
        {
            var sourceWFM1D = new WaterFlowModel1D();
            var destinationWFM1D = new WaterFlowModel1D();
            Assert.That(destinationWFM1D.CanMerge(sourceWFM1D), Is.True);
        }

        [Test]
        public void GivenSourceFMModelAndDestinationWFM1DAreTestedIfTheyCanBeMergedWhenCanMergeThenFalse()
        {
            var fmModel = new WaterFlowFMModel();
            var flowModel1D = new WaterFlowModel1D();
            Assert.That(flowModel1D.CanMerge(fmModel), Is.False);
        }
        
        [Test]
        public void GivenSourceHydroModelAndDestinationWFM1DAreTestedIfTheyCanBeMergedWhenCanMergeThenFalse()
        {
            var hydroModel = new HydroModel();
            var flowModel1D = new WaterFlowModel1D();
            Assert.That(flowModel1D.CanMerge(hydroModel), Is.False);
        }

        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenStructureWeirFormulaDuplicateNamesAreRenamed()
        {
            using (var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true})
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Run();

                var destinationWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
                destinationWFM1D.Name = "Destination";

                var channel = destinationWFM1D.Network.Channels.LastOrDefault();
                Assert.IsNotNull(channel);

                var weir = new Weir() {Name = "weir"};
                channel.BranchFeatures.Add(weir);
                var weirFormula = new GeneralStructureWeirFormula();
                weir.WeirFormula = weirFormula;
                weirFormula.GateOpening = 15;
                weir = new Weir() { Name = "weir1" };
                channel.BranchFeatures.Add(weir);
                weirFormula = new GeneralStructureWeirFormula();
                weir.WeirFormula = weirFormula;
                weirFormula.GateOpening = 10;

                app.Project.RootFolder.Add(destinationWFM1D);
                Assert.DoesNotThrow(() => app.SaveProjectAs("test.dsproj"));
                
                var sourceWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 250);
                sourceWFM1D.Name = "Source";
                channel = sourceWFM1D.Network.Channels.LastOrDefault();
                Assert.IsNotNull(channel);
                weir = new Weir
                {
                    Name = "weir",
                    WeirFormula = new GeneralStructureWeirFormula {GateOpening = 15}
                };

                channel.BranchFeatures.Add(weir);

                weir = new Weir
                {
                    Name = "weir1",
                    WeirFormula = new GeneralStructureWeirFormula {GateOpening = 10}
                };

                channel.BranchFeatures.Add(weir);

                app.Project.RootFolder.Add(sourceWFM1D);
                Assert.DoesNotThrow(() => app.SaveProject());
                
                destinationWFM1D.Merge(sourceWFM1D, null);
                
                weir = (Weir) destinationWFM1D.Network.Weirs.LastOrDefault();
                Assert.That(weir, Is.Not.Null);
                Assert.That(weir.Name, Is.EqualTo("Source0_weir1"));
                
                Assert.DoesNotThrow(() =>  app.SaveProject(), "Cannot save because weirformulas have same nhibernate ids");
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenDuringSaveNoExceptionShouldThrow()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Run();

                var destinationWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
                destinationWFM1D.Name = "Destination";
                var channel = destinationWFM1D.Network.Channels.LastOrDefault();
                Assert.IsNotNull(channel);

                app.Project.RootFolder.Add(destinationWFM1D);
                Assert.DoesNotThrow(() => app.SaveProjectAs("ralph.dsproj"));
                Assert.AreEqual(3,channel.Id);

               //MessageBox.Show("Before second model");
                var sourceWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 250);
                sourceWFM1D.Name = "Source";
                channel = sourceWFM1D.Network.Channels.LastOrDefault();
                Assert.IsNotNull(channel);
                channel.Id = 3;
                //MessageBox.Show("after second model");

                destinationWFM1D.Merge(sourceWFM1D, null);

                //MessageBox.Show("after clone");
                sourceWFM1D.Dispose();
                GC.Collect();
                //MessageBox.Show("after dispose");

                //app.Project.RootFolder.Add(destinationWFM1D);
                Assert.DoesNotThrow(() => app.SaveProject());
            }
        }
    }
}