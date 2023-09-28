using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class PipeTest
    {
        [Test]
        public void PipeSewerConnectionIsPipe()
        {
            var pipe = new Pipe();
            Assert.IsNotNull(pipe);
            Assert.IsTrue(pipe is SewerConnection);
            Assert.IsTrue(pipe.IsPipe());
        }

        [Test]
        public void PipeDoesNotAcceptAnyBranchFeature()
        {
            var pipe = new Pipe();
            Assert.IsNotNull(pipe);

            Assert.IsNotNull(pipe.BranchFeatures);
            Assert.IsFalse(pipe.BranchFeatures.Any());

            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var expectedLogMessage = string.Format("Pipe {0} does not allow any branch feature on it.", pipe.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => pipe.BranchFeatures.Add(featureOne), expectedLogMessage);
            Assert.IsFalse(pipe.BranchFeatures.Any());
        }

        [Test]
        public void ReplacingPipeFeatureBranchesReturnsLogMessage()
        {
            var pipe = new Pipe();
            Assert.IsNotNull(pipe);

            Assert.IsNotNull(pipe.BranchFeatures);
            Assert.IsFalse(pipe.BranchFeatures.Any());

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureList = new EventedList<IBranchFeature> { featureOne, featureTwo };
            Assert.IsNotNull(featureList);
            Assert.IsTrue(featureList.Any());
            #endregion

            var expectedLogMessage = string.Format("Pipe {0} does not allow any branch feature on it.", pipe.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => pipe.BranchFeatures = featureList, expectedLogMessage);
        }

        [Test]
        public void GivenPipe_WhenSettingCrossSectionDefinition_ThenCrossSectionDefinitionNameIsSetAsWell()
        {
            var pipe = new Pipe();
            var csDefinitionName = "myCrossSectionDefinition";
            pipe.CrossSection = new CrossSection(new CrossSectionDefinitionStandard
            {
                Name = csDefinitionName
            });

            Assert.That(pipe.CrossSectionDefinitionName, Is.EqualTo(csDefinitionName));
        }
        [Test]
        public void GivenPipe_WhenNoCrossSectionNull_ThenAlwaysCreateDefaultPipeCrossSectionDefinitionWithDefaultLevels()
        {
            // arrange
            IHydroNetwork hydroNetwork = new HydroNetwork();
            var pipe = new Pipe() { Network = hydroNetwork };

            // act 
            pipe.GenerateDefaultProfileForSewerConnections();

            //asserts
            Assert.That(pipe.CrossSection.Definition.Name, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultPipeProfileName));
            Assert.That(pipe.HydroNetwork.SharedCrossSectionDefinitions.Select(def => def.Name).Any(name => name.Equals(SewerCrossSectionDefinitionFactory.DefaultPipeProfileName)), Is.True);
            Assert.That(pipe.LevelSource, Is.EqualTo(-10.0d));
            Assert.That(pipe.LevelTarget, Is.EqualTo(-10.0d));

        }
    }
}