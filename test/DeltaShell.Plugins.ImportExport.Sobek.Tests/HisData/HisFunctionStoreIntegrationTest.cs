using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.ImportExport.Sobek.HisData;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Coverages;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.HisData
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class HisFunctionStoreIntegrationTest
    {
        private NHibernateProjectRepository projectRepository;
        private NHibernateProjectRepositoryFactory factory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.SetLoggingLevel(Level.Off);

            // register data types to be serialized
            factory = new NHibernateProjectRepositoryFactory();
            
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new SobekImportApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
        }
        
        [SetUp]
        public void SetUp()
        {
            projectRepository = factory.CreateNew();
        }

        [TearDown]
        public void TearDown()
        {
            projectRepository.Dispose();
        }
      
        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveFunctionWithHisFunctionStore()
        {
            var pathProject = TestHelper.GetCurrentMethodName() + ".dsproj";
            var pathHis = Path.Combine(TestHelper.GetTestDataDirectory(), "HisData", "flowhis.his");
            var hisFunctionStore = new HisFunctionStore(pathHis);
            
            Assert.AreEqual(6,hisFunctionStore.Functions.Count);

            var function = hisFunctionStore.Functions.Last();
            var values = function.GetValues<double>();
            hisFunctionStore.Close();

            projectRepository.Create(pathProject);

            var project = new Project();
            project.RootFolder.Add(new DataItem(function));
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();
            
            hisFunctionStore.Close(); // since we don't use FileBasedItemRepository here - close item manually

            //reload
            var retrievedProject = projectRepository.Open(pathProject);

            Assert.AreEqual(1, retrievedProject.RootFolder.DataItems.Count());

            var retrievedFunction = retrievedProject.RootFolder.DataItems.First().Value as Function;

            Assert.IsNotNull(retrievedFunction);
            Assert.AreEqual(values, retrievedFunction.GetValues<double>());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveNetworkCoverageWithHisFunctionStore()
        {
            var pathProject = TestHelper.GetCurrentMethodName() + ".dsproj";
            var pathHis = Path.Combine(TestHelper.GetTestDataDirectory(), "HisData", "HisAndNetwork", "CALCPNT.HIS");
            var hisFunctionStore = new HisFunctionStore(pathHis);
            var networkCoverage = hisFunctionStore.Functions.OfType<INetworkCoverage>().First();
            var name = networkCoverage.Name;
            var nLocations = networkCoverage.Locations.Values.Count;
            hisFunctionStore.Close();

            projectRepository.Create(pathProject);

            var project = new Project();
            project.RootFolder.Add(new DataItem(networkCoverage));
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            hisFunctionStore.Close(); // since we don't use FileBasedItemRepository here - close item manually

            //reload
            var retrievedProject = projectRepository.Open(pathProject);

            Assert.AreEqual(1, retrievedProject.RootFolder.DataItems.Count());

            var retrievedNetworkCoverage = retrievedProject.RootFolder.DataItems.First().Value as INetworkCoverage;

            Assert.IsNotNull(retrievedNetworkCoverage);
            Assert.AreEqual(name,retrievedNetworkCoverage.Name);
            Assert.AreEqual(nLocations, retrievedNetworkCoverage.Locations.Values.Count);

        }
    }
}

