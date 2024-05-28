using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.IntegrationTestUtils;
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
    class HydroNetworkNHibernateTest : NHibernateIntegrationTestBase
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
        }

        [Test]
        public void SaveLoadFeatureCoverageWithBranchFeatures()
        {
            //relates to issue 3633

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            var dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
            // create network
            var network = NHibernateTestsHelper.CreateDummyNetwork();
            var crossSectionDefinition = new CrossSectionDefinitionYZ("Cross Section");
            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(network.Branches[0], crossSectionDefinition, 0);

            //IFeatureCoverage featureCoverage = new FeatureCoverage(); { HydroNetwork = network };
            var featureCoverage = new FeatureCoverage("Test");
            IVariable timeVariable = new Variable<DateTime>("time");
            var featureVariable = new Variable<IBranchFeature>("feature");

            featureCoverage.Arguments.Add(timeVariable);
            featureCoverage.Arguments.Add(featureVariable);
            featureCoverage.Components.Add(new Variable<double>("value"));
            featureCoverage[dateTime, cs] = 17.0;

            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);

                var project = new Project();
                project.RootFolder.Add(new DataItem(network));
                project.RootFolder.Add(new DataItem(featureCoverage));

                projectRepository.SaveOrUpdate(project);
            }

            using (var projectRepository = factory.CreateNew())
            {
                //reload
                var retrievedProject = projectRepository.Open(path);
                var retrievedNetwork = retrievedProject.GetAllItemsRecursive().OfType<INetwork>().FirstOrDefault();
                var retrievedFeatureCoverage = retrievedProject.GetAllItemsRecursive().OfType<IFeatureCoverage>().FirstOrDefault();
                var retrievedCrossSection = retrievedNetwork.Branches[0].BranchFeatures[0];

                //compare
                Assert.AreEqual(retrievedCrossSection, retrievedFeatureCoverage.FeatureVariable.Values[0]);
                Assert.AreEqual(featureCoverage.Components[0].Values.Count, retrievedFeatureCoverage.Components[0].Values.Count);
                Assert.AreEqual((double)retrievedFeatureCoverage[dateTime, retrievedCrossSection], 17.0, 1.0e-6);
            }
        }

        [Test]
        public void RemovingFeatureCoverageDoesNotRemoveFeatures()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            // create network
            var network = NHibernateTestsHelper.CreateDummyNetwork();
            var crossSectionDefinition = CrossSectionDefinitionYZ.CreateDefault();
            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(network.Branches[0], crossSectionDefinition, 0);

            //IFeatureCoverage featureCoverage = new FeatureCoverage(); { HydroNetwork = network };
            var featureCoverage = new FeatureCoverage("Test");
            var featureVariable = new Variable<IBranchFeature>("feature");

            featureCoverage.Arguments.Add(featureVariable);
            featureCoverage.Components.Add(new Variable<double>("value"));
            featureCoverage[cs] = 17.0;

            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);

                var project = new Project();
                project.RootFolder.Add(new DataItem(network));
                var featureCoverageDataItem = new DataItem(featureCoverage);
                project.RootFolder.Add(featureCoverageDataItem);

                projectRepository.SaveOrUpdate(project);

                //remove featurecoverage (shouln't cascade remove features from network
                project.RootFolder.Items.Remove(featureCoverageDataItem);

                projectRepository.SaveOrUpdate(project);
            }

            using (var projectRepository = factory.CreateNew())
            {
                //reload
                var retrievedProject = projectRepository.Open(path);
                var retrievedNetwork = retrievedProject.GetAllItemsRecursive().OfType<INetwork>().FirstOrDefault();

                //check the crossection is still network
                Assert.AreEqual(1, retrievedNetwork.BranchFeatures.Count());
            }
        }
    }
}
