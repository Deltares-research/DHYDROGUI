using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffAreaSynchronizerTest
    {
        private MockRepository mocks = new MockRepository();
        private int addCalled;
        private int removeCalled;
        private CatchmentModelData area;
        private IEventRaiser areaAddedEvent;
        private IEventRaiser areaRemovedEvent;

        [SetUp]
        public void SetUp()
        {
            addCalled = 0;
            removeCalled = 0;
            area = new UnpavedData(new Catchment());
            var model = mocks.Stub<IRainfallRunoffModel>();

            model.ModelDataAdded += null;
            LastCall.IgnoreArguments();
            areaAddedEvent = LastCall.GetEventRaiser();

            model.ModelDataRemoved += null;
            LastCall.IgnoreArguments();
            areaRemovedEvent = LastCall.GetEventRaiser();

            mocks.ReplayAll();

            new CatchmentModelDataSynchronizer<CatchmentModelData>(model)
            {
                OnAreaAddedOrModified = a => addCalled++,
                OnAreaRemoved = a => removeCalled++
            };
        }

        [Test]
        public void AreaAdded()
        {
            areaAddedEvent.Raise(null, new EventArgs<CatchmentModelData>(area));
            Assert.AreEqual(1, addCalled);
            Assert.AreEqual(0, removeCalled);
        }

        [Test]
        public void AreaRemoved()
        {
            areaRemovedEvent.Raise(null, new EventArgs<CatchmentModelData>(area));
            Assert.AreEqual(0, addCalled);
            Assert.AreEqual(1, removeCalled);
        }
    }
}
