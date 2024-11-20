using System.Linq;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.FMSuite.FlowFM.NHibernate;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.NHibernate
{
    [TestFixture]
    public class FlowFMDataAccessListenersProviderTest
    {
        [Test]
        public void CreateDataAccessListeners_ReturnsCorrectCollectionOfDataAccessListeners()
        {
            var dalProvider = new FlowFMDataAccessListenersProvider();

            IDataAccessListener[] result = dalProvider.CreateDataAccessListeners().ToArray();

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<WaterFlowFMDataAccessListener>());
        }
    }
}