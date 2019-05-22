using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class NHibernateWaterFlowModel1DWeirsTest
    {
        private NHibernateProjectRepository projectRepository;
        private NHibernateProjectRepositoryFactory factory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.SetLoggingLevel(Level.Off);
            factory = new NHibernateProjectRepositoryFactory();
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
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
        public void SaveLoadSimpleWeirFormula()
        {
            TestSimplePropertiesAreSavedForWeirFormula<SimpleWeirFormula>();
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadFreeFormWeirFormula()
        {
            TestSimplePropertiesAreSavedForWeirFormula<FreeFormWeirFormula>();

            //test the shape..since it is not 'simple' use this ...
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            var y = new[] { 2.0, 3.0, 4.0 };
            var z = new[] { 2.0, 4.0, 6.0 };
            var formula = new FreeFormWeirFormula();
            formula.SetShape(y, z);

            var retrievedFormula = GetRetrievedWeirFormula(formula, path);
            for (int i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i], retrievedFormula.Y.ToArray()[i]);
                Assert.AreEqual(z[i], retrievedFormula.Z.ToArray()[i]);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadGatedWeirFormula()
        {
            TestSimplePropertiesAreSavedForWeirFormula<GatedWeirFormula>();
        }

        
        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadPierWeirFormula()
        {
            TestSimplePropertiesAreSavedForWeirFormula<PierWeirFormula>();
        }
        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadRiverWeirFormula()
        {
            TestSimplePropertiesAreSavedForWeirFormula<RiverWeirFormula>();
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadGeneralStructureWeirFormula()
        {
            TestSimplePropertiesAreSavedForWeirFormula<GeneralStructureWeirFormula>();
        }

        private void TestSimplePropertiesAreSavedForWeirFormula<TFormulaType>() where TFormulaType : IWeirFormula
        {
            var formula = Activator.CreateInstance(typeof(TFormulaType));
            //get some random stuff in there
            if (typeof (TFormulaType) == typeof (GatedWeirFormula))
            {
                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(formula, new[] { "UseLowerEdgeLevelTimeSeries", "UseHorizontalDoorOpeningWidthTimeSeries", "DoorHeight", "HorizontalDoorOpeningWidth","LowerEdgeLevel", "HorizontalDoorOpeningDirection" });
            }
            else if (typeof(TFormulaType) == typeof(GeneralStructureWeirFormula))
            {
                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(formula, new[] { "UseLowerEdgeLevelTimeSeries", "UseHorizontalDoorOpeningWidthTimeSeries", "DoorHeight", "HorizontalDoorOpeningWidth", "LowerEdgeLevel", "HorizontalDoorOpeningDirection" });
            }
            else
            {
                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(formula);
            }

            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            var retrievedFormula = GetRetrievedWeirFormula((IWeirFormula)formula, path);

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(formula, retrievedFormula);
        }

        private TFormula GetRetrievedWeirFormula<TFormula>(TFormula formula, string path) where TFormula : IWeirFormula
        {
            var weir = new Weir() { WeirFormula = formula };

            //setup repo.
            projectRepository.Create(path);
            projectRepository.SaveOrUpdate(NHibernateWaterFlowModel1DTest.GetProjectFor(weir));
            projectRepository.Close();

            var retievedProject = projectRepository.Open(path);
            IWeir retrievedWeir = retievedProject.GetAllItemsRecursive().OfType<IWeir>().First();

            return (TFormula)retrievedWeir.WeirFormula;
        }

        
    }
}