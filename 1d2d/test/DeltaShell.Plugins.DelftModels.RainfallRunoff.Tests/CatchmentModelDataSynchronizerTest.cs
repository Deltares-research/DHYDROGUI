using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class CatchmentModelDataSynchronizerTest
    {
        [Test]
        public void GivenCatchmentModelDataSynchronizer_ChangingTheSpecifiedCatchmentDataOfModel_ShouldBeRaisingCorrectEvent()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var catchment = new Catchment();
            var pavedData = new PavedData(catchment);

            var dataSynchronizer = new CatchmentModelDataSynchronizer<PavedData>(model);

            var addedOrModified = 0;
            var removed = 0;

            dataSynchronizer.OnAreaAddedOrModified = d => addedOrModified++;
            dataSynchronizer.OnAreaRemoved = d => removed++;

            // Act & Assert
            model.ModelDataAdded += Raise.EventWith(new EventArgs<CatchmentModelData>(pavedData));
            Assert.AreEqual(1, addedOrModified);

            model.ModelDataRemoved += Raise.EventWith(new EventArgs<CatchmentModelData>(pavedData));
            Assert.AreEqual(1, removed);

            model.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(pavedData, new PropertyChangedEventArgs(nameof(pavedData.IsSewerPumpCapacityFixed)));
            Assert.AreEqual(2, addedOrModified);
        }

        [Test]
        public void GivenCatchmentModelDataSynchronizer_Disconnect_ShouldReleaseEventSubscriptions()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var dataSynchronizer = new CatchmentModelDataSynchronizer<PavedData>(model);
            
            // Act & Assert
            model.Received().PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
            model.Received().ModelDataAdded += Arg.Any<EventHandler<EventArgs<CatchmentModelData>>>();
            model.Received().ModelDataRemoved += Arg.Any<EventHandler<EventArgs<CatchmentModelData>>>();

            dataSynchronizer.Disconnect();

            model.Received().PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            model.Received().ModelDataAdded -= Arg.Any<EventHandler<EventArgs<CatchmentModelData>>>();
            model.Received().ModelDataRemoved -= Arg.Any<EventHandler<EventArgs<CatchmentModelData>>>();
        }
    }
}