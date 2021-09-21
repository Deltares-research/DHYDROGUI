using System.Threading;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Polder;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Polder.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class PolderConceptViewTest
    {

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var polderConceptView = new PolderConceptView { Data = null };
            WindowsFormsTestHelper.ShowModal(polderConceptView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithData()
        {
            var catchment = new Catchment();
            catchment.IsGeometryDerivedFromAreaSize = true;
            catchment.SetAreaSize(1000);
            var polderConcept = new PolderConcept(catchment)
                {
                    CalculationArea = 300000,
                    SubCatchmentModelData =
                        {
                            new PavedData(new Catchment()),
                            new UnpavedData(new Catchment()),
                            new GreenhouseData(new Catchment()),
                            new OpenWaterData(new Catchment())
                        },
                    PavedArea = 100000,
                    UnpavedArea = 200000,
                    GreenhouseArea = 300000,
                    OpenWaterArea = 400000,
                };
            var polderConceptView = new PolderConceptView
                                             {
                                                 AreaUnit = RainfallRunoffEnums.AreaUnit.km2,
                                                 Data = polderConcept
                                             };

            WindowsFormsTestHelper.ShowModal(polderConceptView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithSomeData()
        {
            var catchment = new Catchment {CatchmentType = CatchmentType.Polder, IsGeometryDerivedFromAreaSize = true};
            catchment.SetAreaSize(1000);
            var polderConcept = new PolderConcept(catchment)
            {
                CalculationArea = 300000,
                SubCatchmentModelData =
                        {
                            new PavedData(new Catchment()),
                            new OpenWaterData(new Catchment())
                        },
                PavedArea = 100000,
                OpenWaterArea = 400000,
            };
            var polderConceptView = new PolderConceptView
            {
                AreaUnit = RainfallRunoffEnums.AreaUnit.km2,
                Data = polderConcept
            };

            WindowsFormsTestHelper.ShowModal(polderConceptView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void OnlyGreenhouseDataIsAvailableCheckSelectedTabIsGreenhouse()
        {
            var catchment = new Catchment();
            catchment.IsGeometryDerivedFromAreaSize = true;
            catchment.SetAreaSize(1000);
            var polderConcept = new PolderConcept(catchment)
            {
                CalculationArea = 300,
                SubCatchmentModelData = {new  GreenhouseData(new Catchment())},
                GreenhouseArea = 800,
            };
            var polderConceptView = new PolderConceptView
            {
                AreaUnit = RainfallRunoffEnums.AreaUnit.km2,
                Data = polderConcept
            };

            WindowsFormsTestHelper.Show(polderConceptView);

            //ugly wait hack:
            Thread.Sleep(500);
            Application.DoEvents();

            var tabControl = polderConceptView.Controls.Find("tabControl", true);
            var tabGreenhouse = polderConceptView.Controls.Find("tabPageGreenhouse", true);

            Assert.AreEqual(1,tabControl.Length);
            Assert.AreEqual(1,tabGreenhouse.Length);
            Assert.AreSame(tabGreenhouse[0], ((TabControl)tabControl[0]).SelectedTab);
        }
    }
}
