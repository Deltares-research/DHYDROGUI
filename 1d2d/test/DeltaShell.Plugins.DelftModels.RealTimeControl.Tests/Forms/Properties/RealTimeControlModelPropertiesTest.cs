using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class RealTimeControlModelPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(
                new RealTimeControlModelProperties {Data = new RealTimeControlModel()});
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetWriteRestart_ThenOutputMarkedOutOfSync()
        {
            // Given
            var model = Substitute.For<IRealTimeControlModel>();
            var properties = new RealTimeControlModelProperties() {Data = model};

            // When
            properties.WriteRestart = true;

            // Then
            model.Received().MarkOutputOutOfSync();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetSaveStateStopTime_ThenOutputMarkedOutOfSync()
        {
            // Given
            var model = Substitute.For<IRealTimeControlModel>();
            var properties = new RealTimeControlModelProperties() {Data = model};

            // When
            properties.SaveStateStopTime = DateTime.Now;

            // Then
            model.Received().MarkOutputOutOfSync();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetSaveStateStartTime_ThenOutputMarkedOutOfSync()
        {
            // Given
            var model = Substitute.For<IRealTimeControlModel>();
            var properties = new RealTimeControlModelProperties() {Data = model};

            // When
            properties.SaveStateStartTime = DateTime.Now;

            // Then
            model.Received().MarkOutputOutOfSync();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetSaveStateTimeStep_ThenOutputMarkedOutOfSync()
        {
            // Given
            var model = Substitute.For<IRealTimeControlModel>();
            var properties = new RealTimeControlModelProperties() {Data = model};

            // When
            properties.SaveStateTimeStep = TimeSpan.FromDays(2);

            // Then
            model.Received().MarkOutputOutOfSync();
        }
    }
}