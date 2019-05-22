using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Polder.Forms
{
    [TestFixture]
    public class PolderConceptViewDataTest
    {
        [Test]
        public void CheckPercentage()
        {
            using (var polderConceptViewData = GetPolderConceptViewData(20, 40, 60, 80, 200, RainfallRunoffEnums.AreaUnit.m2))
            {
                Assert.AreEqual(10.0, polderConceptViewData.PavedPercentage, 0.01);
                Assert.AreEqual(20.0, polderConceptViewData.UnpavedPercentage, 0.01);
                Assert.AreEqual(30.0, polderConceptViewData.GreenhousePercentage, 0.01);
                Assert.AreEqual(40.0, polderConceptViewData.OpenwaterPercentage, 0.01);

                Assert.AreEqual(100.0, polderConceptViewData.SumPercentages, 0.01);
            }
        }

        [Test]
        public void CheckPropertyChangeForDataBinding()
        {
            var polderConcept = new PolderConcept(new Catchment());
            using (var polderConceptViewData = new PolderConceptViewData(polderConcept, RainfallRunoffEnums.AreaUnit.m2))
            {
                var changeNumber = 0;
                polderConceptViewData.PropertyChanged += (s, a) =>
                    {
                        // sender needs to be binding object
                        Assert.AreEqual(polderConceptViewData, s);

                        switch (changeNumber)
                        {
                            case 0 :
                                Assert.AreEqual("HasGreenhouse", a.PropertyName);
                                break;
                            case 1 : 
                                Assert.AreEqual("HasPaved", a.PropertyName);
                                break;
                            case 2:
                                Assert.AreEqual("AreaUnit", a.PropertyName);
                                break;
                            case 3:
                                Assert.AreEqual("CalculationArea", a.PropertyName);
                                break;
                        }
                        changeNumber++;
                    };

                polderConcept.SubCatchmentModelData.Add(new GreenhouseData(new Catchment())); // 1
                polderConcept.SubCatchmentModelData.Add(new PavedData(new Catchment())); // 2
                polderConceptViewData.AreaUnit = RainfallRunoffEnums.AreaUnit.km2; // 3
            }
        }

        [Test]
        public void CheckPercentageAfterAreaChanges()
        {
            using (var polderConceptViewData = GetPolderConceptViewData(10, 20, 30, 40, 100, RainfallRunoffEnums.AreaUnit.m2))
            {
                polderConceptViewData.PavedArea = 20;
                polderConceptViewData.UnpavedArea = 15;
                polderConceptViewData.GreenhouseArea = 10;
                polderConceptViewData.OpenwaterArea = 5;

                Assert.AreEqual(20.0, polderConceptViewData.PavedPercentage, 0.01);
                Assert.AreEqual(15.0, polderConceptViewData.UnpavedPercentage, 0.01);
                Assert.AreEqual(10.0, polderConceptViewData.GreenhousePercentage, 0.01);
                Assert.AreEqual(5.0, polderConceptViewData.OpenwaterPercentage, 0.01);

                Assert.AreEqual(50.0, polderConceptViewData.SumPercentages, 0.01);
                var catchment = polderConceptViewData.Catchment;
                catchment.SetAreaSize(200);

                Assert.AreEqual(10.0, polderConceptViewData.PavedPercentage, 0.01);
                Assert.AreEqual(7.5, polderConceptViewData.UnpavedPercentage, 0.01);
                Assert.AreEqual(5.0, polderConceptViewData.GreenhousePercentage, 0.01);
                Assert.AreEqual(2.5, polderConceptViewData.OpenwaterPercentage, 0.01);

                Assert.AreEqual(25.0, polderConceptViewData.SumPercentages, 0.01);
            }
        }

        [Test]
        public void CheckPercentageAfterOneAreaChange()
        {
            using (var polderConceptViewData = GetPolderConceptViewData(10, 20, 30, 40, 100, RainfallRunoffEnums.AreaUnit.m2))
            {
                polderConceptViewData.PavedArea = 110;


                Assert.AreEqual(110.0, polderConceptViewData.PavedPercentage, 0.01);
                Assert.AreEqual(20.0, polderConceptViewData.UnpavedPercentage, 0.01);
                Assert.AreEqual(30.0, polderConceptViewData.GreenhousePercentage, 0.01);
                Assert.AreEqual(40.0, polderConceptViewData.OpenwaterPercentage, 0.01);

                Assert.AreEqual(200.0, polderConceptViewData.SumAreas, 0.01);
                Assert.AreEqual(200.0, polderConceptViewData.SumPercentages, 0.01);
            }
        }


        [Test]
        public void CheckPropertyChangeAfterCatchmentChange()
        {
            using (var polderConceptViewData = GetPolderConceptViewData(10, 20, 30, 40, 100, RainfallRunoffEnums.AreaUnit.m2))
            {
                var counter = 0;
                polderConceptViewData.PropertyChanged += ((sender, args) => counter++);
                var catchment = polderConceptViewData.Catchment;
                catchment.SetAreaSize(200);
                catchment.Name = "new name";

                Assert.AreEqual(2, counter);
            }
        }
        
        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void AreaUnitChange()
        {
            using (var polderConceptViewData = GetPolderConceptViewData(10, 20, 30, 40, 100, RainfallRunoffEnums.AreaUnit.m2))
            {
                polderConceptViewData.AreaUnit = RainfallRunoffEnums.AreaUnit.m2;
            }
        }

        private static PolderConceptViewData GetPolderConceptViewData(double pavedArea, double unpavedArea, double greenhouseArea, double openwaterArea, double totalArea, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            var catchment = new Catchment {IsGeometryDerivedFromAreaSize = true};
            catchment.SetAreaSize(totalArea);

            return new PolderConceptViewData(new PolderConcept(catchment)
                {
                    CalculationArea = 1000,
                    SubCatchmentModelData = { new GreenhouseData(new Catchment()), new UnpavedData(new Catchment()), new PavedData(new Catchment()), new OpenWaterData(new Catchment())},
                    PavedArea = pavedArea,
                    UnpavedArea = unpavedArea,
                    GreenhouseArea = greenhouseArea,
                    OpenWaterArea = openwaterArea
                },areaUnit);
        }
    }
}
