using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.NetworkEditor;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NGHS.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    internal class HydroNetworkNHibernateTest : NHibernateIntegrationTestBase
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
        }

        [Test]
        public void SaveLoadFeatureCoverageWithBranchFeatures()
        {
            //relates to issue 3633

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            var dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
            // create network
            INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
            
            //IFeatureCoverage featureCoverage = new FeatureCoverage(); { HydroNetwork = network };
            var featureCoverage = new FeatureCoverage("Test");
            IVariable timeVariable = new Variable<DateTime>("time");
            var featureVariable = new Variable<IBranchFeature>("feature");

            featureCoverage.Arguments.Add(timeVariable);
            featureCoverage.Arguments.Add(featureVariable);
            featureCoverage.Components.Add(new Variable<double>("value"));

            //save 
            using (NHibernateProjectRepository projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);

                var project = new Project();
                project.RootFolder.Add(new DataItem(network));
                project.RootFolder.Add(new DataItem(featureCoverage));

                projectRepository.SaveOrUpdate(project);
            }

            using (NHibernateProjectRepository projectRepository = factory.CreateNew())
            {
                //reload
                Project retrievedProject = projectRepository.Open(path);
                IFeatureCoverage retrievedFeatureCoverage = retrievedProject.GetAllItemsRecursive().OfType<IFeatureCoverage>().FirstOrDefault();

                //compare
                Assert.AreEqual(featureCoverage.Components[0].Values.Count, retrievedFeatureCoverage.Components[0].Values.Count);
            }
        }
    }
}