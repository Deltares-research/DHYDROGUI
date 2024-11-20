using System.Linq;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    public class WaterQualityDataAccessListenersProviderTest
    {
        [Test]
        public void CreateDataAccessListeners_ReturnsCorrectCollectionOfDataAccessListeners()
        {
            var dalProvider = new WaterQualityDataAccessListenersProvider();

            IDataAccessListener[] result = dalProvider.CreateDataAccessListeners().ToArray();

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<WaterQualityModelDataAccessListener>());
        }
    }
}