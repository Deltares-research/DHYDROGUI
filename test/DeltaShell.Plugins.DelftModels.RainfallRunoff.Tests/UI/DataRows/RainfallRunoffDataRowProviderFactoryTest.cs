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
            var catchment = new Catchment {CatchmentType = CatchmentType.Polder};
            catchment.SubCatchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved});
            catchment.SubCatchments.Add(new Catchment {CatchmentType = CatchmentType.Paved});
            catchment.SubCatchments.Add(new Catchment {CatchmentType = CatchmentType.GreenHouse});
            catchment.SubCatchments.Add(new Catchment {CatchmentType = CatchmentType.OpenWater});
            catchment.SubCatchments.Add(new Catchment {CatchmentType = CatchmentType.Sacramento});
            catchment.SubCatchments.Add(new Catchment {CatchmentType = CatchmentType.Hbv});
            model.Basin.Catchments.Add(catchment);

            foreach(var modelData in model.GetAllModelData())
            {
                modelData.CalculationArea = 1000;
            }

            var allProviders = RainfallRunoffDataRowProviderFactory.GetDataRowProviders(model, new Catchment[] {});

            Assert.GreaterOrEqual(allProviders.Count(), 4); //or 5?

            foreach(var provider in allProviders)
            {
                Assert.AreEqual(1, provider.Rows.Count()); //each one row
            }

            var mdeView = new MultipleDataEditor {Data = allProviders}; //triggers some getters
        }
    }
}