using System.Linq;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RealTimeControl.NHibernate;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.NHibernate
{
    [TestFixture]
    public class RealTimeControlDataAccessListenersProviderTest
    {
        [Test]
        public void CreateDataAccessListeners_ReturnsCorrectCollectionOfDataAccessListeners()
        {
            var dalProvider = new RealTimeControlDataAccessListenersProvider();

            IDataAccessListener[] result = dalProvider.CreateDataAccessListeners().ToArray();

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<RtcDataAccessListener>());
        }
    }
}