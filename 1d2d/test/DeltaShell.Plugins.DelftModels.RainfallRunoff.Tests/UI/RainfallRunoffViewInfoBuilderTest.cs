﻿using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.Gui.Forms.ViewManager;
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

        private static IEnumerable<TestCaseData> GetBuildViewInfoModelDataObjectData()
        {
            TestCaseData ToData<TData, TView>(string description) =>
                new TestCaseData(description, typeof(TData), typeof(TView))
                    .SetName($"{description} | {typeof(TData).Name}");

            
            yield return ToData<PavedData, PavedDataView>("Paved view");
            yield return ToData<UnpavedData, UnpavedDataView>("Unpaved view");
            yield return ToData<OpenWaterData, OpenWaterDataView>("Open water view");
            yield return ToData<GreenhouseData, GreenhouseDataView>("Greenhouse view");
            yield return ToData<SacramentoData, SacramentoDataView>("Sacramento view");
            yield return ToData<HbvData, HbvDataView>("HBV view");
        }
        private static IEnumerable<TestCaseData> GetBuildViewInfoModelDataObjectDataUsedInModel()
        {
            TestCaseData ToData<TData, TView>(string description, CatchmentType catchmentType) =>
                new TestCaseData(typeof(TData), catchmentType, typeof(TView))
                    .SetName($"{description} | {typeof(TData).Name}");

            
            yield return ToData<PavedData, PavedDataView>("Paved view", CatchmentType.Paved);
            yield return ToData<UnpavedData, UnpavedDataView>("Unpaved view", CatchmentType.Unpaved);
            yield return ToData<OpenWaterData, OpenWaterDataView>("Open water view", CatchmentType.OpenWater);
            yield return ToData<GreenhouseData, GreenhouseDataView>("Greenhouse view", CatchmentType.GreenHouse);
            yield return ToData<SacramentoData, SacramentoDataView>("Sacramento view", CatchmentType.Sacramento);
            yield return ToData<HbvData, HbvDataView>("HBV view", CatchmentType.Hbv);
        }

        private static IEnumerable<TestCaseData> GetBuildViewInfoModelDataCatchmentWrapperObjectData()
        {
            TestCaseData ToDataWithViewData<TData, TViewData, TView>(string description) =>
                new TestCaseData(description, typeof(TData), typeof(TViewData), typeof(TView))
                    .SetName($"Catchment wrapper {description} | {typeof(TData).Name} | {typeof(TViewData).Name}");

            yield return ToDataWithViewData<Catchment, PavedData, PavedDataView>("Paved view");
            yield return ToDataWithViewData<Catchment, UnpavedData, UnpavedDataView>("Unpaved view");
            yield return ToDataWithViewData<Catchment, OpenWaterData, OpenWaterDataView>("Open water view");
            yield return ToDataWithViewData<Catchment, GreenhouseData, GreenhouseDataView>("Greenhouse view");
            yield return ToDataWithViewData<Catchment, SacramentoData, SacramentoDataView>("Sacramento view");
            yield return ToDataWithViewData<Catchment, HbvData, HbvDataView>("HBV view");
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
        [TestCaseSource(nameof(GetBuildViewInfoModelDataCatchmentWrapperObjectData))]
        public void BuildViewInfoModelDataCatchmentWrapperObject_ContainsExpectedViewInfo(string description,
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
            var catchment = Substitute.For<Catchment>();
            var data = Substitute.For(new [] { expectedViewDataType },new object[] { catchment });
            var view = (IView)Activator.CreateInstance(expectedViewType);
            Assert.That(relevantViewInfo.CloseForData(view, data), Is.False, $"View data is not yet coupled to view, this view data of type {expectedViewType} should not be closed for this data.");
            view.Data = data;
            Assert.That(relevantViewInfo.CloseForData(view, data), Is.True, $"View data is coupled to view, this view data of type {expectedViewType} should be closed for this data of type {expectedViewDataType} when it is in the object {expectedDataType}.");
        }

        [Test]
        [TestCaseSource(nameof(GetBuildViewInfoModelDataObjectData))]
        public void BuildViewInfoModelDataObject_ContainsExpectedViewInfo(string description,
                                                                 Type expectedDataType,
                                                                 Type expectedViewType)

        {
            // Setup
            var plugin = Substitute.For<IRainfallRunoffGuiPlugin>();

            // Call
            IEnumerable<ViewInfo> viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);

            // Assert
            bool IsRelevantViewInfo(ViewInfo info) =>
                info.DataType == expectedDataType &&
                info.ViewType == expectedViewType;

            ViewInfo relevantViewInfo = viewInfos.FirstOrDefault(IsRelevantViewInfo);
            Assert.That(relevantViewInfo, Is.Not.Null);
            Assert.That(relevantViewInfo.Description, Is.EqualTo(description));
            var catchment = Substitute.For<Catchment>();
            var data = Substitute.For(new [] { expectedDataType },new object[] { catchment });
            var view = (IView)Activator.CreateInstance(expectedViewType);
            Assert.That(relevantViewInfo.CloseForData(view, data), Is.True, $"View data is not yet coupled to view, this view data of type {expectedViewType} should still be closed.");
            view.Data = data;
            Assert.That(relevantViewInfo.CloseForData(view, data), Is.True, $"View data is coupled to view, this view data of type {expectedViewType} should be closed for this data of type {expectedDataType}.");
        }
        
        [Test]
        [TestCaseSource(nameof(GetBuildViewInfoModelDataObjectDataUsedInModel))]
        public void BuildViewInfoModelDataObjectWithModel_WhenOpeningViewForCatchmentOfModelData_ContainsExpectedView_AndAfterGuiRemoveAllViewsCalledOnModelViewIsRemoved(Type expectedDataType,
                                                                                                                                                                          CatchmentType expectedCatchmentDataType,
                                                                                                                                                                          Type expectedViewType)

        {
            // Setup
            var gui = Substitute.For<IGui>();
            var projectFileDialogService = Substitute.For<IProjectFileDialogService>();
            var projectService = Substitute.For<IProjectService>();
            var rainfallRunoffModel = new RainfallRunoffModel();
            ViewResolver viewResolver = GenerateViewResolver(expectedCatchmentDataType, gui, rainfallRunoffModel, out Catchment catchment, out CatchmentModelData _);
            var guiCommandHandler = new GuiCommandHandler(gui, projectFileDialogService, projectService);

            // Call
            bool viewForDataOpened = viewResolver.OpenViewForData(catchment);
            var viewsForDataOpened = viewResolver.GetViewsForData(catchment);

            // Assert
            Assert.That(catchment, Is.TypeOf<Catchment>());
            Assert.That(rainfallRunoffModel.GetCatchmentModelData(catchment), Is.TypeOf(expectedDataType));
            Assert.That(viewForDataOpened, Is.True);
            Assert.That(viewsForDataOpened, Is.Not.Empty);
            Assert.That(viewsForDataOpened.ElementAtOrDefault(0), Is.TypeOf(expectedViewType));

            // Call
            guiCommandHandler.RemoveAllViewsForItem(rainfallRunoffModel);

            // Assert
            Assert.That(viewResolver.GetViewsForData(catchment), Is.Empty, $"View data of type {expectedDataType} is coupled to view, this view data of type {expectedViewType} should be closed for this data of type {expectedDataType}.");
        }

        [Test]
        [TestCaseSource(nameof(GetBuildViewInfoModelDataObjectDataUsedInModel))]
        public void BuildViewInfoModelDataObjectWithModel_WhenOpeningViewForModelData_ContainsExpectedView_AndAfterGuiRemoveAllViewsCalledOnModelViewIsRemoved(Type expectedDataType,
                                                                                                                                                               CatchmentType expectedCatchmentDataType,
                                                                                                                                                               Type expectedViewType)

        {
            // Setup
            var gui = Substitute.For<IGui>();
            var projectFileDialogService = Substitute.For<IProjectFileDialogService>();
            var projectService = Substitute.For<IProjectService>();
            var rainfallRunoffModel = new RainfallRunoffModel();
            ViewResolver viewResolver = GenerateViewResolver(expectedCatchmentDataType, gui, rainfallRunoffModel, out Catchment _, out CatchmentModelData modelDataObject);
            var guiCommandHandler = new GuiCommandHandler(gui, projectFileDialogService, projectService);

            // Call
            bool viewForDataOpened = viewResolver.OpenViewForData(modelDataObject);
            var viewsForDataOpened = viewResolver.GetViewsForData(modelDataObject);

            // Assert
            Assert.That(modelDataObject, Is.TypeOf(expectedDataType));
            Assert.That(viewForDataOpened, Is.True);
            Assert.That(viewsForDataOpened, Is.Not.Empty);
            Assert.That(viewsForDataOpened.ElementAtOrDefault(0), Is.TypeOf(expectedViewType));

            // Call
            guiCommandHandler.RemoveAllViewsForItem(rainfallRunoffModel);

            // Assert
            Assert.That(viewResolver.GetViewsForData(modelDataObject), Is.Empty, $"View data of type {expectedDataType} is coupled to view, this view data of type {expectedViewType} should be closed for this data of type {expectedDataType}.");
        }

        private static ViewResolver GenerateViewResolver(CatchmentType expectedCatchmentDataType, IGui gui, RainfallRunoffModel rainfallRunoffModel, out Catchment catchment, out CatchmentModelData data)
        {

            var plugin = Substitute.For<IRainfallRunoffGuiPlugin>();
            plugin.Gui.Returns(gui);
            var dockingManager = Substitute.For<IDockingManager>();
            var guiContextManager = Substitute.For<IGuiContextManager>();

            var documentViewManager = new ViewList(dockingManager, ViewLocation.Document, () => guiContextManager)
            {
                IgnoreActivation = true,
            };

            // Builds setup
            var viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);
            var viewResolver = Substitute.For<ViewResolver>(documentViewManager, viewInfos);
            gui.DocumentViewsResolver.Returns(viewResolver);
            gui.DocumentViews.Returns(documentViewManager);

            catchment = new Catchment() { CatchmentType = expectedCatchmentDataType };
            data = catchment.CreateDefaultModelData();

            rainfallRunoffModel.ModelData.Add(data);
            catchment.Basin = rainfallRunoffModel.Basin;
            AddToProject(rainfallRunoffModel, gui.Application.ProjectService);
            return viewResolver;
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
            TestCaseData ToData(Func<RainfallRunoffModel, IMeteoData> fRetrieveMeteoData, Type meteoDataType,
                                string description) =>
                new TestCaseData(fRetrieveMeteoData, meteoDataType).SetDescription(description);

            yield return ToData(model => model.Precipitation, typeof(IMeteoData), "Precipitation");
            yield return ToData(model => model.Evaporation, typeof(IEvaporationMeteoData), "Evaporation");
            yield return ToData(model => model.Temperature, typeof(IMeteoData), "Temperature");
        }

        [Test]
        [TestCaseSource(nameof(GetMeteoDataViewInfoData))]
        [Category(TestCategory.Integration)]
        public void BuildViewInfoObject_MeteoData_HasACorrectViewForSpecificMeteoData(Func<RainfallRunoffModel, IMeteoData> GetMeteoDataFunc, Type meteoDataType)
        {
            // Setup
            var model = new RainfallRunoffModel();

            var gui = Substitute.For<IGui>();
            AddToProject(model, gui.Application.ProjectService);

            var plugin = Substitute.For<IRainfallRunoffGuiPlugin>();
            plugin.Gui.Returns(gui);

            IMeteoData meteoData = GetMeteoDataFunc.Invoke(model);
            
            // Call
            IEnumerable<ViewInfo> viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);
            
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

        private static void AddToProject(object obj, IProjectService projectService)
        {
            var project = new Project();
            project.RootFolder.Add(obj);
            projectService.Project.Returns(project);
        }
    }
}