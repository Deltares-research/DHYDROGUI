using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.Extensions;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelIntegrationTest
    {
        [Test]
        [TestCaseSource(nameof(GetTestCases))]
        public void GivenAnIntegratedModelProject_WhenTheProjectIsOpened_ThenTheDataItemsShouldBeLinked(string testcaseDir)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (IApplication application = GetApplication())
            {
                string testcasePath = TestHelper.GetTestFilePath(testcaseDir);
                FileUtils.CopyDirectory(testcasePath, tempDir.Path);

                string projPath = Path.Combine(tempDir.Path, "testProj", "Project1.dsproj");

                // Call
                Project project = application.ProjectService.OpenProject(projPath);

                // Assert
                RealTimeControlModel rtcModel =
                    project.RootFolder.Items
                           .OfType<HydroModel>().Single()
                           .Activities
                           .OfType<RealTimeControlModel>().Single();

                IDataItem relevantDataItem = rtcModel.DataItems.Single(x => x.Name == "Control Group 1")
                                                     .Children.Single(x => x.Role == DataItemRole.Output);

                IDataItem linkedStructure = relevantDataItem.LinkedBy?.FirstOrDefault();

                Assert.That(linkedStructure, Is.Not.Null);
                Assert.That(linkedStructure.Tag, Is.EqualTo("GateLowerEdgeLevel"));
            }
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            yield return new TestCaseData("relinkDataItemsProject").SetName("Legacy ModelExchange file");
            yield return new TestCaseData("relinkDataItemsProjectDimrFormat").SetName("ModelExchange file with DIMR strings");
        }

        [Test]
        public void GivenAnIntegratedModelWithTwoFMDataItemsWithSameNameButDifferentQuantitiesLinkedToRTC_WhenSavedAndLoaded_ThenDataItemLinksAreCorrectlyRestored()
        {
            // Setup
            const string dataItemName = "randomName";

            using (var tempDir = new TemporaryDirectory())
            using (IApplication application = GetApplication())
            using (HydroModel hydroModel = CreateHydroModelWithFMAndRTCAndDataItemsWithSameName(dataItemName))
            {
                IProjectService projectService = application.ProjectService;
                Project project = projectService.CreateProject();
                project.RootFolder.Add(hydroModel);

                // Call
                string projPath = Path.Combine(tempDir.Path, "testProj", "Project1.dsproj");
                projectService.SaveProjectAs(projPath);
                projectService.CloseProject();
                project = application.ProjectService.OpenProject(projPath);

                // Assert
                Assert.That(project, Is.Not.Null);

                HydroModel loadedHydroModel = project.RootFolder.Items.OfType<HydroModel>().Single();
                AssertThatDataItemsAreLinkedCorrectly(loadedHydroModel);
            }
        }

        private static HydroModel CreateHydroModelWithFMAndRTCAndDataItemsWithSameName(string dataItemName)
        {
            var hydroModel = new HydroModel();
            var fmModel = new WaterFlowFMModel();
            var rtcModel = new RealTimeControlModel();
            hydroModel.Activities.Add(fmModel);
            hydroModel.Activities.Add(rtcModel);

            AddLinkedDataItemsWithSameNameToModels(fmModel, rtcModel, dataItemName);

            return hydroModel;
        }

        private static void AddLinkedDataItemsWithSameNameToModels(
            WaterFlowFMModel fmModel, RealTimeControlModel rtcModel, string dataItemName)
        {
            // Create FM features and add them to FM model
            GroupableFeature2DPoint observationPoint = Create2DPointFeatureWithName(dataItemName);
            var observationCrossSection = CreateGroupableFeature<ObservationCrossSection2D>(dataItemName);
            var pump = CreateGroupableFeature<Pump>(dataItemName);

            fmModel.Area.ObservationPoints.Add(observationPoint);
            fmModel.Area.ObservationCrossSections.Add(observationCrossSection);
            fmModel.Area.Pumps.Add(pump);

            // Create RTC control group with two inputs and one output
            ControlGroup controlGroup = AddPIDControlGroupToRTCModel(rtcModel);

            // Get RTC data items
            IDataItem rtcInput0DataItem = GetFirstRTCInputDataItem(rtcModel, controlGroup);
            IDataItem rtcInput1DataItem = GetSecondRTCInputDataItem(rtcModel, controlGroup);
            IDataItem rtcOutput0DataItem = GetFirstRTCOutputDataItem(rtcModel, controlGroup);

            // Get FM data items
            IDataItem observationCrossSectionDataItem = GetSingleFMDataItemByTag<ObservationCrossSection2D>(fmModel, "discharge");
            IDataItem observationPointDataItem = GetSingleFMDataItemByTag<GroupableFeature2DPoint>(fmModel, "water_level");
            IDataItem pumpDataItem = GetSingleFMDataItemByTag<Pump>(fmModel, "capacity");

            // Link RTC and FM data items
            rtcInput0DataItem.LinkTo(observationCrossSectionDataItem);
            rtcInput1DataItem.LinkTo(observationPointDataItem);
            pumpDataItem.LinkTo(rtcOutput0DataItem);
        }

        private static GroupableFeature2DPoint Create2DPointFeatureWithName(string dataItemName)
        {
            return new GroupableFeature2DPoint
            {
                Name = dataItemName,
                Geometry = new Point(1, 1)
            };
        }

        private static T CreateGroupableFeature<T>(string dataItemName) where T : IGroupableFeature, INameable, new()
        {
            return new T()
            {
                Name = dataItemName,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })
            };
        }

        private static ControlGroup AddPIDControlGroupToRTCModel(RealTimeControlModel rtcModel)
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.GenerateControlGroup();
            rtcModel.ControlGroups.Add(controlGroup);
            return controlGroup;
        }

        private static IDataItem GetFirstRTCInputDataItem(RealTimeControlModel rtcModel, ControlGroup controlGroup)
        {
            Input firstInput = controlGroup.Inputs.First();
            return rtcModel.GetDataItemByValue(firstInput);
        }

        private static IDataItem GetSecondRTCInputDataItem(RealTimeControlModel rtcModel, ControlGroup controlGroup)
        {
            Input secondInput = controlGroup.Inputs.Last();
            return rtcModel.GetDataItemByValue(secondInput);
        }

        private static IDataItem GetFirstRTCOutputDataItem(RealTimeControlModel rtcModel, ControlGroup controlGroup)
        {
            Output output = controlGroup.Outputs.Single();
            return rtcModel.GetDataItemByValue(output);
        }

        private static IDataItem GetSingleFMDataItemByTag<T>(WaterFlowFMModel fmModel, string tag)
        {
            return fmModel.AllDataItems.Single(di => di.ComposedValue is T && di.Tag.EqualsCaseInsensitive(tag));
        }

        private static void AssertThatDataItemsAreLinkedCorrectly(HydroModel hydroModel)
        {
            RealTimeControlModel rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().Single();
            IDataItem input0 = GetRTCDataItemByName(rtcModel, "Control group.input0");
            IDataItem input1 = GetRTCDataItemByName(rtcModel, "Control group.input1");
            IDataItem output0 = GetRTCDataItemByName(rtcModel, "Control group.output0");

            WaterFlowFMModel fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().Single();
            IDataItem observationCrossSectionDataItem = GetSingleFMDataItemByTag<ObservationCrossSection2D>(fmModel, "discharge");
            IDataItem observationPointDataItem = GetSingleFMDataItemByTag<GroupableFeature2DPoint>(fmModel, "water_level");
            IDataItem pumpDataItem = GetSingleFMDataItemByTag<Pump>(fmModel, "capacity");

            Assert.That(input0.LinkedTo, Is.EqualTo(observationCrossSectionDataItem));
            Assert.That(input1.LinkedTo, Is.EqualTo(observationPointDataItem));
            Assert.That(pumpDataItem.LinkedTo, Is.EqualTo(output0));
        }

        private static IDataItem GetRTCDataItemByName(RealTimeControlModel rtcModel, string name)
        {
            return rtcModel.AllDataItems.Single(di => di.Name.EqualsCaseInsensitive(name));
        }

        private static IApplication GetApplication()
        {
            var app = new DHYDROApplicationBuilder()
                      .WithFlowFM()
                      .WithHydroModel()
                      .WithRealTimeControl()
                      .Build();
            app.Run();

            return app;
        }
    }
}