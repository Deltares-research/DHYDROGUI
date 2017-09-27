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

namespace Sobek.IntegrationTests
{
    [TestFixture]
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
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Run();

                var destinationWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
                destinationWFM1D.Name = "Destination";
                var channel = destinationWFM1D.Network.Channels.LastOrDefault();
                Assert.IsNotNull(channel);

                var weir = new Weir() {Name = "weir"};
                channel.BranchFeatures.Add(weir);
                var weirFormula = new GeneralStructureWeirFormula() {Id = 1};
                weir.WeirFormula = weirFormula;
                weirFormula.GateOpening = 15;


                var sourceWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 250);
                sourceWFM1D.Name = "Source";
                channel = sourceWFM1D.Network.Channels.LastOrDefault();
                Assert.IsNotNull(channel);
                weir = new Weir() {Name = "weir"};
                channel.BranchFeatures.Add(weir);
                weirFormula = new GeneralStructureWeirFormula() {Id = 1 };
                weir.WeirFormula = weirFormula;
                weirFormula.GateOpening = 15;


                destinationWFM1D.Merge(sourceWFM1D, null);
                weir = (Weir) destinationWFM1D.Network.Weirs.LastOrDefault();
                Assert.That(weir, Is.Not.Null);
                Assert.That(weir.Name, Is.EqualTo("Source0_weir"));
                app.Project.RootFolder.Add(destinationWFM1D);
                Assert.DoesNotThrow(() =>  app.SaveProjectAs("mick.dsproj"), "Cannot save because weirformulas have same nhibernate ids");
            }
        }
    }
}