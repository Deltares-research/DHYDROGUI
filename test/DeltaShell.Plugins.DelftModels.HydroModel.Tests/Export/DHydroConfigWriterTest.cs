using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Is = NUnit.Framework.Is;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Export
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DHydroConfigWriterTest
    {
        private MockRepository mocks;
        private IModel mockedF1dModel;
        private IModel mockedFMModel;
        private IModel mockedRTCModel;
        private Iterative1D2DCoupler mocked1d2dCoupler;
        private ICompositeActivity workflow;

        [Test]
        public void CreateConfigDocumentWithCouplerDataTest()
        {
            mocks.ReplayAll();
            var expectedXmlDoc = TestHelper.GetTestFilePath(@"dimrExport\expected.xml");
            expectedXmlDoc = TestHelper.CreateLocalCopy(expectedXmlDoc);

            var writer = new DHydroConfigWriter();
            //initialize writer
            TypeUtils.TrySetValueAnyVisibility(writer, typeof(DHydroConfigWriter), "modelCouplers", new List<IDimrConfigModelCoupler>());
            TypeUtils.TrySetValueAnyVisibility(writer, typeof(DHydroConfigWriter), "CouplerModelsDictionary", new Dictionary<IModel, IDimrModel>());
            TypeUtils.TrySetValueAnyVisibility(writer, typeof(DHydroConfigWriter), "CoreCountDictionary", new Dictionary<IDimrModel, int>());
            
            var xml = writer.CreateConfigDocument(workflow);
            XDocument expectedXml = XDocument.Load(expectedXmlDoc);
            var creationDate = xml.Descendants().SingleOrDefault(p => p.Name.LocalName == "creationDate");
            Assert.NotNull(creationDate);
            creationDate.Value = "";
            Assert.That(xml.ToString(), Is.EqualTo(expectedXml.ToString()));

            mocks.VerifyAll();

        }


        [SetUp]
        public void Setup()
        {
            const string f1DDataOutputitemText = "f1d_data_output_item";
            const string f1DDataInputitemText = "f1d_data_input_item";
            const string fmDataOutputitemText = "fm_data_output_item";
            const string fmDataInputitemText = "fm_data_input_item";
            const string targetDataF1dInputitemText = "RTC_F1D_input_dataitem";
            const string targetDataFMInputitemText = "RTC_FM_input_dataitem";
            const string targetDataF1dOutputitemText = "RTC_F1D_output_dataitem";
            const string targetDataFMOutputitemText = "RTC_FM_output_dataitem";
            const string sourcemodelText = "My F1dModel";
            const string otherSourceModelText = "My FMModel";
            const string targetModelText = "My RTCModel";
            const string couplerText = "mocked1d2dCoupler";
            const string couplerNameText = "1d2dCoupler";

            /**
             * Testing rtc + {1d2d} (or rtc + {fm + f1d}) model with links but then generic way!
             * 
             * 
             **/

            mocks = new MockRepository();
            var dataItemFM_to_RTCInput = mocks.StrictMock<IDataItem>();
            dataItemFM_to_RTCInput.Expect(di => di.Name).Return("FM_to_RTC").Repeat.Any();
            dataItemFM_to_RTCInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemF1D_to_RTCInput = mocks.StrictMock<IDataItem>();
            dataItemF1D_to_RTCInput.Expect(di => di.Name).Return("F1D_to_RTC").Repeat.Any();
            dataItemF1D_to_RTCInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemRTC_to_FMOutput = mocks.StrictMock<IDataItem>();
            dataItemRTC_to_FMOutput.Expect(di => di.Name).Return("RTC_to_FM").Repeat.Any();
            dataItemRTC_to_FMOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            var dataItemRTC_to_F1DOutput = mocks.StrictMock<IDataItem>();
            dataItemRTC_to_F1DOutput.Expect(di => di.Name).Return("RTC_to_F1D").Repeat.Any();
            dataItemRTC_to_F1DOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            var dataItemF1dInput = mocks.StrictMock<IDataItem>();
            dataItemF1dInput.Expect(di => di.Name).Return("f1d input").Repeat.Any();
            dataItemF1dInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemFmInput = mocks.StrictMock<IDataItem>();
            dataItemFmInput.Expect(di => di.Name).Return("fm input").Repeat.Any();
            dataItemFmInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemF1dOutput = mocks.StrictMock<IDataItem>();
            dataItemF1dOutput.Expect(di => di.Name).Return("f1d output").Repeat.Any();
            dataItemF1dOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            var dataItemFmOutput = mocks.StrictMock<IDataItem>();
            dataItemFmOutput.Expect(di => di.Name).Return("fm output").Repeat.Any();
            dataItemFmOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            dataItemF1dInput.Expect(di => di.LinkedTo).Return(dataItemRTC_to_F1DOutput).Repeat.Any();
            dataItemFmInput.Expect(di => di.LinkedTo).Return(dataItemRTC_to_FMOutput).Repeat.Any();

            dataItemF1dOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();
            dataItemFmOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();

            dataItemF1D_to_RTCInput.Expect(di => di.LinkedTo).Return(dataItemF1dOutput).Repeat.Any();
            dataItemRTC_to_F1DOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();

            dataItemFM_to_RTCInput.Expect(di => di.LinkedTo).Return(dataItemFmOutput).Repeat.Any();
            dataItemRTC_to_FMOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();
            
            mockedF1dModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(IModel), typeof(IDimrModel));
            var sourceDataItems = new List<IDataItem> { dataItemF1dInput, dataItemF1dOutput };
            mockedF1dModel.Expect(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            mockedF1dModel.Expect(m => m.Name).Return(sourcemodelText).Repeat.Any();
            ((IDimrModel) mockedF1dModel).Expect(m => m.ShortName).Return("mockedF1dModel").Repeat.Any();
            ((IDimrModel) mockedF1dModel).Expect(dm => dm.GetItemString(dataItemF1dOutput)).Return(f1DDataOutputitemText).Repeat.Any();
            ((IDimrModel) mockedF1dModel).Expect(dm => dm.GetItemString(dataItemF1dInput)).Return(f1DDataInputitemText).Repeat.Any();
            ((IDimrModel) mockedF1dModel).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            var now = new DateTime(1981, 7, 12);
            ((ITimeDependentModel) mockedF1dModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
            ((ITimeDependentModel) mockedF1dModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
            ((ITimeDependentModel) mockedF1dModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

            mockedFMModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(IModel), typeof(IDimrModel));
            var extraSourceDataItems = new List<IDataItem> { dataItemFmInput, dataItemFmOutput };
            mockedFMModel.Expect(m => m.AllDataItems).Return(extraSourceDataItems).Repeat.Any();

            mockedFMModel.Expect(m => m.Name).Return(otherSourceModelText).Repeat.Any();
            ((IDimrModel) mockedFMModel).Expect(dm => dm.ShortName).Return("source").Repeat.Any();
            ((IDimrModel) mockedFMModel).Expect(dm => dm.GetItemString(dataItemFmOutput)).Return(fmDataOutputitemText).Repeat.Any();
            ((IDimrModel) mockedFMModel).Expect(dm => dm.GetItemString(dataItemFmInput)).Return(fmDataInputitemText).Repeat.Any();
            ((IDimrModel) mockedFMModel).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            ((ITimeDependentModel) mockedFMModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
            ((ITimeDependentModel) mockedFMModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
            ((ITimeDependentModel) mockedFMModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

            mockedRTCModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(ITimeDependentModel), typeof(IDimrModel));
            var targetDataItems = new List<IDataItem> {dataItemF1D_to_RTCInput, dataItemFM_to_RTCInput, dataItemRTC_to_F1DOutput, dataItemRTC_to_FMOutput };
            ((ITimeDependentModel) mockedRTCModel).Expect(m => m.Name).Return(targetModelText).Repeat.Any();
            ((ITimeDependentModel) mockedRTCModel).Expect(m => m.Owner).Return(null).Repeat.Any();
            ((ITimeDependentModel) mockedRTCModel).Expect(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.ShortName).Return("mockedRTCModel").Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.GetItemString(dataItemF1D_to_RTCInput)).Return(targetDataF1dInputitemText).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.GetItemString(dataItemFM_to_RTCInput)).Return(targetDataFMInputitemText).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.GetItemString(dataItemRTC_to_F1DOutput)).Return(targetDataF1dOutputitemText).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.GetItemString(dataItemRTC_to_FMOutput)).Return(targetDataFMOutputitemText).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.LibraryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.DirectoryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel) mockedRTCModel).Expect(dm => dm.InputFile).Return(string.Empty).Repeat.Any();
            ((ITimeDependentModel) mockedRTCModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
            ((ITimeDependentModel) mockedRTCModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
            ((ITimeDependentModel) mockedRTCModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

            mocked1d2dCoupler = mocks.StrictMultiMock <Iterative1D2DCoupler>(typeof(IModel), typeof(ICompositeActivity), typeof(IDimrModel));
            mocked1d2dCoupler.Expect(wf => wf.Activities).Return(new EventedList<IActivity> { new ActivityWrapper(mockedF1dModel), new ActivityWrapper(mockedFMModel)}).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.Flow1DModel).Return(mockedF1dModel as ITimeDependentModel).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.Flow2DModel).Return(mockedFMModel as ITimeDependentModel).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.DeepClone()).Return(mocked1d2dCoupler).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.Name).Return(couplerNameText).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.AllDataItems).Return(Enumerable.Empty<IDataItem>()).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.GetHashCode()).Return(1).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.Equals(Arg<object>.Is.Anything)).IgnoreArguments().Return(false).Repeat.Any();
            ((IDimrModel) mocked1d2dCoupler).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            ((IDimrModel) mocked1d2dCoupler).Expect(dm => dm.ShortName).Return(couplerText).Repeat.Any();
            ((IDimrModel) mocked1d2dCoupler).Expect(dm => dm.LibraryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel) mocked1d2dCoupler).Expect(dm => dm.DirectoryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel) mocked1d2dCoupler).Expect(dm => dm.InputFile).Return(string.Empty).Repeat.Any();


            /*var mocked1d2dWorkFlow = mocks.StrictMock<ParallelActivity>();
            mocked1d2dWorkFlow.Expect(wf => wf.Activities).Return(
                new EventedList<IActivity> { new ActivityWrapper(mockedF1dModel), new ActivityWrapper(mockedFMModel) }
            ).Repeat.Any();*/
            //mocked1d2dCoupler.Expect(wf => wf.CurrentWorkflow).Return(mocked1d2dWorkFlow).Repeat.Any();

            workflow = mocks.StrictMock<ParallelActivity>(); // similar to rtc + {fm + f1d} workflow
            workflow.Expect(wf => wf.CurrentWorkflow).Return(workflow).Repeat.Any();
            workflow.Expect(wf => wf.Activities).Return(
                new EventedList<IActivity> {new ActivityWrapper(mockedRTCModel), new ActivityWrapper(mocked1d2dCoupler)}
            ).Repeat.Any();
            mocked1d2dCoupler.Expect(wf => wf.CurrentWorkflow).Return(workflow).Repeat.Any();
            mockedF1dModel.Expect(m => m.Owner).Return(mocked1d2dCoupler).Repeat.Any();
            mockedFMModel.Expect(m => m.Owner).Return(mocked1d2dCoupler).Repeat.Any();
        }
    }
}