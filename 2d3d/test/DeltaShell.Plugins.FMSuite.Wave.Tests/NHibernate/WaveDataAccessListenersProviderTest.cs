using System.Linq;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.FMSuite.Wave.NHibernate;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.NHibernate
{
    [TestFixture]
    public class WaveDataAccessListenersProviderTest
    {
        [Test]
        public void CreateDataAccessListeners_ReturnsCorrectCollectionOfDataAccessListeners()
        {
            var dalProvider = new WaveDataAccessListenersProvider();

            IDataAccessListener[] result = dalProvider.CreateDataAccessListeners().ToArray();

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<WaveDataAccessListener>());
        }
    }
}