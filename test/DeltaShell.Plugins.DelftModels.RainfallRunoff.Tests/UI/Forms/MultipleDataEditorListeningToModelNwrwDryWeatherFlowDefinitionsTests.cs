using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitionsTests
    {
        [Test]
        public void ShowEmpty()
        {
            var mde = new MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions();
            WindowsFormsTestHelper.ShowModal(mde);
        }

        [Test]
        public void ShowWithGreenhouseAndUnpavedAndPavedAndOpenWaterAndNwrw()
        {
            var mde = new MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions();

            var rrmodel = new RainfallRunoffModel();
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c01", CatchmentType = CatchmentType.Unpaved });
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c02", CatchmentType = CatchmentType.GreenHouse });
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c03", CatchmentType = CatchmentType.Paved });
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c04", CatchmentType = CatchmentType.OpenWater });
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c05", CatchmentType = CatchmentType.NWRW});

            rrmodel.Basin.WasteWaterTreatmentPlants.Add(new WasteWaterTreatmentPlant { Name = "wwtp1" });

            var providerOne = new ConceptDataRowProvider<UnpavedData, UnpavedDataRow>(rrmodel, "Unpaved");
            var providerTwo = new ConceptDataRowProvider<GreenhouseData, GreenhouseDataRow>(rrmodel, "Greenhouse");
            var providerThree = new ConceptDataRowProvider<PavedData, PavedDataRow>(rrmodel, "Paved");
            var providerFour = new ConceptDataRowProvider<OpenWaterData, OpenWaterDataRow>(rrmodel, "Openwater");
            var providerFive = new ConceptDataRowProvider<NwrwData, NwrwDataRow>(rrmodel, "Nwrw");

            mde.Data = new IDataRowProvider[] { providerOne, providerTwo, providerThree, providerFour, providerFive };

            WindowsFormsTestHelper.ShowModal(mde);
        }
        [Test]
        public void ShowWithNwrwAndTriggerModelNwrwDryWeatherFlowDefinitions()
        {
            var mde = new MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions();
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var rrmodel = new RainfallRunoffModel();
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c01", CatchmentType = CatchmentType.NWRW});
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c02", CatchmentType = CatchmentType.NWRW});

            var provider = new ConceptDataRowProvider<NwrwData, NwrwDataRow>(rrmodel, "Nwrw");

            mde.Data = new IDataRowProvider[] { provider };
            
            WindowsFormsTestHelper.ShowModal(mde, f =>
            {
                var definitions = TypeUtils.GetField(mde, "nwrwDryWeatherFlowDefinitions") as IEventedList<NwrwDryWeatherFlowDefinition>;
                Assert.That(definitions, Is.Not.Null);
                Assert.That(definitions.Count, Is.EqualTo(2));
                var nwrwDryWeatherFlowDefinition = new NwrwDryWeatherFlowDefinition() { Name = "test" };
                var nwrwDryWeatherFlowDefinition2 = new NwrwDryWeatherFlowDefinition() { Name = "test2" };
                rrmodel.NwrwDryWeatherFlowDefinitions.Add(nwrwDryWeatherFlowDefinition);
                rrmodel.NwrwDryWeatherFlowDefinitions.Add(nwrwDryWeatherFlowDefinition2);
                definitions = TypeUtils.GetField(mde, "nwrwDryWeatherFlowDefinitions") as IEventedList<NwrwDryWeatherFlowDefinition>;
                Assert.That(definitions, Is.Not.Null);
                Assert.That(definitions.Count, Is.EqualTo(4));
                var nwrwData = rrmodel.ModelData.FirstOrDefault() as NwrwData;
                var otherNwrwData = rrmodel.ModelData.ElementAtOrDefault(1) as NwrwData;
                nwrwData.DryWeatherFlows[0].DryWeatherFlowId = "test";
                const string othertest = "otherTest";
                nwrwDryWeatherFlowDefinition.Name = othertest;
                Assert.That(nwrwData.DryWeatherFlows[0].DryWeatherFlowId, Is.EqualTo(othertest));
                Assert.That(otherNwrwData.DryWeatherFlows[0].DryWeatherFlowId, Is.EqualTo(NwrwDryWeatherFlowDefinition.DefaultDwaId));
                definitions = TypeUtils.GetField(mde, "nwrwDryWeatherFlowDefinitions") as IEventedList<NwrwDryWeatherFlowDefinition>;
                Assert.That(definitions, Is.Not.Null);
                Assert.That(definitions.Count, Is.EqualTo(4));
                Assert.That(definitions.Any(d => d.Name.Equals(othertest)), Is.True);
                nwrwData.DryWeatherFlows[1].DryWeatherFlowId = "test2";
                rrmodel.NwrwDryWeatherFlowDefinitions.Remove(nwrwDryWeatherFlowDefinition);
                definitions = TypeUtils.GetField(mde, "nwrwDryWeatherFlowDefinitions") as IEventedList<NwrwDryWeatherFlowDefinition>;
                Assert.That(definitions, Is.Not.Null);
                Assert.That(definitions.Count, Is.EqualTo(3));
                Assert.That(nwrwData.DryWeatherFlows[0].DryWeatherFlowId, Is.EqualTo(NwrwDryWeatherFlowDefinition.DefaultDwaId));
                Assert.That(nwrwData.DryWeatherFlows[1].DryWeatherFlowId, Is.EqualTo("test2"));
            });
            
        }
        [Test]
        public void GivenNwrwCatchmentModelData_ChangingTheClosedPavedFlatNwrwCatchmentDataOfModelViaMDEDataRow_ShouldUpdateCatchmentGeometryArea()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var catchment = new Catchment();
            if (string.IsNullOrWhiteSpace(NwrwDryWeatherFlowDefinition.DefaultDwaId))
            {
                //NwrwDryWeatherFlowDefinition.DefaultDwaId = NwrwDryWeatherFlowDefinitionTest.ORIGINAL_DEFAULT_DWF_ID;
                TypeUtils.SetPrivatePropertyValue(new NwrwDryWeatherFlowDefinition(), "DefaultDwaId", "Default_DWA");
            }

            var nwrwData = new NwrwData(catchment);
            model.GetAllModelData().OfType<NwrwData>().Returns(Enumerable.Repeat(nwrwData, 1));
            var rowProvider = new ConceptDataRowProvider<NwrwData, NwrwDataRow>(model, "NWRW") { Filter = Enumerable.Repeat(catchment, 1) };

            // Act & Assert
            catchment.SetAreaSize(1000);
            Assert.That(catchment.GeometryArea, Is.EqualTo(1000).Within(0.1)); 
            
            var nwrwDataRow = rowProvider.Rows.FirstOrDefault() as NwrwDataRow;
            Assert.That(nwrwDataRow, Is.Not.Null);
            nwrwDataRow.ClosedPavedFlat = 3000;
            Assert.That(catchment.GeometryArea, Is.EqualTo(3000).Within(0.1));
        }
    }
}