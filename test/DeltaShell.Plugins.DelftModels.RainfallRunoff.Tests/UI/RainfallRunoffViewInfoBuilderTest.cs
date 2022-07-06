using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Views;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture]
    public class RainfallRunoffViewInfoBuilderTest
    {
        private static IEnumerable<TestCaseData> GetBuildViewInfoObjectData()
        {
            TestCaseData ToData<TData, TView>(string description) =>
                ToDataWithViewData<TData, TData, TView>(description);

            TestCaseData ToDataWithViewData<TData, TViewData, TView>(string description) =>
                new TestCaseData(description, typeof(TData), typeof(TViewData), typeof(TView))
                    .SetName($"{description} | {typeof(TData).Name} | {typeof(TViewData).Name}");

            yield return ToData<IEventedList<NwrwDryWeatherFlowDefinition>, NwrwDryWeatherFlowDefinitionView>("Dryweather flow view");
            yield return ToData<IEventedList<NwrwDefinition>, NwrwDefinitionView>("Nwrw surface settings view");
            yield return ToData<UnpavedData, UnpavedDataView>("Unpaved view");
            yield return ToData<PavedData, PavedDataView>("Paved view");
            yield return ToData<OpenWaterData, OpenWaterDataView>("Open water view");
            yield return ToData<GreenhouseData, GreenhouseDataView>("Greenhouse view");
            yield return ToData<SacramentoData, SacramentoDataView>("Sacramento view");
            yield return ToData<HbvData, HbvDataView>("HBV view");
            yield return ToDataWithViewData<RunoffBoundary, RunoffBoundaryData, RunoffBoundaryDataView>("Runoff boundary data");
            yield return ToDataWithViewData<Catchment, UnpavedData, UnpavedDataView>("Unpaved view");
            yield return ToDataWithViewData<Catchment, PavedData, PavedDataView>("Paved view");
            yield return ToDataWithViewData<Catchment, OpenWaterData, OpenWaterDataView>("Open water view");
            yield return ToDataWithViewData<Catchment, GreenhouseData, GreenhouseDataView>("Greenhouse view");
            yield return ToDataWithViewData<Catchment, SacramentoData, SacramentoDataView>("Sacramento view");
            yield return ToDataWithViewData<Catchment, HbvData, HbvDataView>("HBV view");
            yield return ToData<IMeteoData, MeteoEditorView>("Meteorological data viewer");
            yield return ToDataWithViewData<RRInitialConditionsWrapper, IEnumerable<IDataRowProvider>, MultipleDataEditor>("Multiple data editor (D-RR)");
            yield return ToDataWithViewData<IEnumerable<Catchment>, IEnumerable<IDataRowProvider>, MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions>("Multiple data editor (D-RR)");
            yield return ToDataWithViewData<TreeFolder, IEnumerable<IDataRowProvider>, MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions>("Catchment attribute viewer");
            yield return ToData<RainfallRunoffModel, ValidationView>("Validation Report");
            yield return ToData<IFeatureCoverage, CoverageTableView>("Output");
        }


        [Test]
        [TestCaseSource(nameof(GetBuildViewInfoObjectData))]
        public void BuildViewInfoObject_ContainsExpectedViewInfo(string description,
                                                                 Type expectedDataType,
                                                                 Type expectedViewDataType,
                                                                 Type expectedViewType)

        {
            // Setup
            var plugin = Substitute.For<IRainfallRunoffGuiPlugin>();

            // Call
            IEnumerable<ViewInfo> viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);

            // Assert
            bool IsRelevantViewInfo(ViewInfo info) =>
                info.DataType == expectedDataType &&
                info.ViewDataType == expectedViewDataType &&
                info.ViewType == expectedViewType;

            ViewInfo relevantViewInfo = viewInfos.FirstOrDefault(IsRelevantViewInfo);
            Assert.That(relevantViewInfo, Is.Not.Null);
            Assert.That(relevantViewInfo.Description, Is.EqualTo(description));
        }

        [Test]
        public void BuildViewInfoObject_ContainsTwoViewInfosRelatedToMeteoData()
        {
            // Setup
            var plugin = Substitute.For<IRainfallRunoffGuiPlugin>();

            // Call
            IEnumerable<ViewInfo> viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);

            // Assert
            Type meteoDataType = typeof(IMeteoData);
            Type viewType = typeof(MeteoEditorView);

            bool IsRelevantViewInfo(ViewInfo info) =>
                info.DataType == meteoDataType &&
                info.ViewDataType == meteoDataType &&
                info.ViewType == viewType ;

            ViewInfo[] relevantViewInfos = viewInfos.Where(IsRelevantViewInfo).ToArray();
            Assert.That(relevantViewInfos, Has.Length.EqualTo(2));
        }

        private static IEnumerable<TestCaseData> GetMeteoDataViewInfoData()
        {
            TestCaseData ToData(Func<RainfallRunoffModel, MeteoData> fRetrieveMeteoData,
                                string description) =>
                new TestCaseData(fRetrieveMeteoData).SetDescription(description);

            yield return ToData(model => model.Precipitation, "Precipitation");
            yield return ToData(model => model.Evaporation, "Evaporation");
            yield return ToData(model => model.Temperature, "Temperature");
        }

        [Test]
        [TestCaseSource(nameof(GetMeteoDataViewInfoData))]
        [Category(TestCategory.Integration)]
        public void BuildViewInfoObject_MeteoData_HasACorrectViewForSpecificMeteoData(Func<RainfallRunoffModel, MeteoData> GetMeteoDataFunc)
        {
            // Setup
            var model = new RainfallRunoffModel();
            IModel[] models = { model };

            var application = Substitute.For<IApplication>();
            application.GetAllModelsInProject().Returns(models);

            var gui = Substitute.For<IGui>();
            gui.Application.Returns(application);

            var plugin = Substitute.For<IRainfallRunoffGuiPlugin>();
            plugin.Gui.Returns(gui);

            MeteoData meteoData = GetMeteoDataFunc.Invoke(model);


            // Call
            IEnumerable<ViewInfo> viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);

            Type meteoDataType = typeof(IMeteoData);
            Type viewType = typeof(MeteoEditorView);

            bool IsRelevantViewInfo(ViewInfo info) =>
                info.DataType == meteoDataType &&
                info.ViewDataType == meteoDataType &&
                info.ViewType == viewType &&
                info.AdditionalDataCheck.Invoke(meteoData);

            ViewInfo[] relevantViewInfos = viewInfos.Where(IsRelevantViewInfo).ToArray();

            // Assert: Relevant view info is retrieved.
            Assert.That(relevantViewInfos, Has.Length.EqualTo(1));


            using (var view = new MeteoEditorView())
            {
                relevantViewInfos.FirstOrDefault()?.AfterCreate(view, meteoData);
            
                // Assert: DataContext has been set.
                Assert.That(view.DataContext, Is.Not.Null);
            }
        }
    }
}