using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.DataRows
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class RainfallRunoffDataRowProviderFactoryTest
    {
        [Test]
        public void CreateAllRows()
        {
            var model = new RainfallRunoffModel();
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Paved});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.GreenHouse});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.OpenWater});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Sacramento});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Hbv});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.NWRW});

            foreach(var modelData in model.GetAllModelData())
            {
                modelData.CalculationArea = 1000;
            }

            var allProviders = RainfallRunoffDataRowProviderFactory.GetDataRowProviders(model, new Catchment[] {});

            Assert.GreaterOrEqual(allProviders.Count(), 7);

            foreach(var provider in allProviders)
            {
                Assert.AreEqual(1, provider.Rows.Count()); //each one row
            }

            var mdeView = new MultipleDataEditor {Data = allProviders}; //triggers some getters
        }
    }
}