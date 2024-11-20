using System.Linq;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.NHibernate;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.NHibernate
{
    [TestFixture]
    public class RainfallRunoffDataAccessListenersProviderTest
    {
        [Test]
        public void CreateDataAccessListeners_ReturnsCorrectCollectionOfDataAccessListeners()
        {
            var dalProvider = new RainfallRunoffDataAccessListenersProvider();

            IDataAccessListener[] result = dalProvider.CreateDataAccessListeners().ToArray();

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<RainfallRunoffDataAccessListener>());
        }
    }
}