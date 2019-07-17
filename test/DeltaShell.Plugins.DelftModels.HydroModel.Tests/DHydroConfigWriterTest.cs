using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DHydroConfigWriterTest
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

        [Test]
        public void CreateConfigDocumentWithCouplerDataTest()
        {
            /**
             * Testing rtc + {1d2d} (or rtc + {fm + f1d}) model with links but then generic way!
             * 
             * 
             **/

            var mocks1 = new MockRepository();
            var dataItemFM_to_RTCInput = mocks1.StrictMock<IDataItem>();
            dataItemFM_to_RTCInput.Expect(di => di.Name).Return("FM_to_RTC").Repeat.Any();
            dataItemFM_to_RTCInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemF1D_to_RTCInput = mocks1.StrictMock<IDataItem>();
            dataItemF1D_to_RTCInput.Expect(di => di.Name).Return("F1D_to_RTC").Repeat.Any();
            dataItemF1D_to_RTCInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemRTC_to_FMOutput = mocks1.StrictMock<IDataItem>();
            dataItemRTC_to_FMOutput.Expect(di => di.Name).Return("RTC_to_FM").Repeat.Any();
            dataItemRTC_to_FMOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            var dataItemRTC_to_F1DOutput = mocks1.StrictMock<IDataItem>();
            dataItemRTC_to_F1DOutput.Expect(di => di.Name).Return("RTC_to_F1D").Repeat.Any();
            dataItemRTC_to_F1DOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            var dataItemF1dInput = mocks1.StrictMock<IDataItem>();
            dataItemF1dInput.Expect(di => di.Name).Return("f1d input").Repeat.Any();
            dataItemF1dInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemFmInput = mocks1.StrictMock<IDataItem>();
            dataItemFmInput.Expect(di => di.Name).Return("fm input").Repeat.Any();
            dataItemFmInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemF1dOutput = mocks1.StrictMock<IDataItem>();
            dataItemF1dOutput.Expect(di => di.Name).Return("f1d output").Repeat.Any();
            dataItemF1dOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();

            var dataItemFmOutput = mocks1.StrictMock<IDataItem>();
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

            IModel mockedF1DModel = mocks1.StrictMultiMock<ITimeDependentModel>(typeof(IModel), typeof(IDimrModel));
            var sourceDataItems = new List<IDataItem> { dataItemF1dInput, dataItemF1dOutput };
            mockedF1DModel.Expect(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            mockedF1DModel.Expect(m => m.Name).Return(sourcemodelText).Repeat.Any();
            mockedF1DModel.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            mockedF1DModel.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)mockedF1DModel).Expect(m => m.ShortName).Return("mockedF1dModel").Repeat.Any();
            ((IDimrModel)mockedF1DModel).Expect(dm => dm.GetItemString(dataItemF1dOutput)).Return(f1DDataOutputitemText).Repeat.Any();
            ((IDimrModel)mockedF1DModel).Expect(dm => dm.GetItemString(dataItemF1dInput)).Return(f1DDataInputitemText).Repeat.Any();
            ((IDimrModel)mockedF1DModel).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            var now = new DateTime(1981, 7, 12);
            ((ITimeDependentModel)mockedF1DModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
            ((ITimeDependentModel)mockedF1DModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
            ((ITimeDependentModel)mockedF1DModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

            IModel mockedFmModel = mocks1.StrictMultiMock<ITimeDependentModel>(typeof(IModel), typeof(IDimrModel));
            var extraSourceDataItems = new List<IDataItem> { dataItemFmInput, dataItemFmOutput };
            mockedFmModel.Expect(m => m.AllDataItems).Return(extraSourceDataItems).Repeat.Any();
            mockedFmModel.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            mockedFmModel.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();

            mockedFmModel.Expect(m => m.Name).Return(otherSourceModelText).Repeat.Any();
            ((IDimrModel)mockedFmModel).Expect(dm => dm.ShortName).Return("source").Repeat.Any();
            ((IDimrModel)mockedFmModel).Expect(dm => dm.GetItemString(dataItemFmOutput)).Return(fmDataOutputitemText).Repeat.Any();
            ((IDimrModel)mockedFmModel).Expect(dm => dm.GetItemString(dataItemFmInput)).Return(fmDataInputitemText).Repeat.Any();
            ((IDimrModel)mockedFmModel).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            ((ITimeDependentModel)mockedFmModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
            ((ITimeDependentModel)mockedFmModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
            ((ITimeDependentModel)mockedFmModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

            IModel mockedRtcModel = mocks1.StrictMultiMock<ITimeDependentModel>(typeof(ITimeDependentModel), typeof(IDimrModel));
            var targetDataItems = new List<IDataItem> { dataItemF1D_to_RTCInput, dataItemFM_to_RTCInput, dataItemRTC_to_F1DOutput, dataItemRTC_to_FMOutput };
            mockedRtcModel.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            mockedRtcModel.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((ITimeDependentModel)mockedRtcModel).Expect(m => m.Name).Return(targetModelText).Repeat.Any();
            ((ITimeDependentModel)mockedRtcModel).Expect(m => m.Owner).Return(null).Repeat.Any();
            ((ITimeDependentModel)mockedRtcModel).Expect(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.ShortName).Return("mockedRTCModel").Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.GetItemString(dataItemF1D_to_RTCInput)).Return(targetDataF1dInputitemText).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.GetItemString(dataItemFM_to_RTCInput)).Return(targetDataFMInputitemText).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.GetItemString(dataItemRTC_to_F1DOutput)).Return(targetDataF1dOutputitemText).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.GetItemString(dataItemRTC_to_FMOutput)).Return(targetDataFMOutputitemText).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.LibraryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.DirectoryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel)mockedRtcModel).Expect(dm => dm.InputFile).Return(string.Empty).Repeat.Any();
            ((ITimeDependentModel)mockedRtcModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
            ((ITimeDependentModel)mockedRtcModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
            ((ITimeDependentModel)mockedRtcModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

            var mocked1D2DCouplerActivities = new EventedList<IActivity>();
            Iterative1D2DCoupler mocked1D2DCoupler = mocks1.StrictMultiMock<Iterative1D2DCoupler>(typeof(IModel), typeof(ICompositeActivity), typeof(IDimrModel));
            ((IDimrModel)mocked1D2DCoupler).Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)mocked1D2DCoupler).Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.Activities).Return(mocked1D2DCouplerActivities).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.Flow1DModel).Return(mockedF1DModel as ITimeDependentModel).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.Flow2DModel).Return(mockedFmModel as ITimeDependentModel).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.DeepClone()).Return(mocked1D2DCoupler).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.Name).Return(couplerNameText).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.AllDataItems).Return(Enumerable.Empty<IDataItem>()).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.GetHashCode()).Return(1).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.Equals(Arg<object>.Is.Anything)).IgnoreArguments().Return(false).Repeat.Any();
            ((IDimrModel)mocked1D2DCoupler).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            ((IDimrModel)mocked1D2DCoupler).Expect(dm => dm.ShortName).Return(couplerText).Repeat.Any();
            ((IDimrModel)mocked1D2DCoupler).Expect(dm => dm.LibraryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel)mocked1D2DCoupler).Expect(dm => dm.DirectoryName).Return(string.Empty).Repeat.Any();
            ((IDimrModel)mocked1D2DCoupler).Expect(dm => dm.InputFile).Return(string.Empty).Repeat.Any();


            var workflow1Activities = new EventedList<IActivity>();
            ICompositeActivity workflow1 = mocks1.StrictMock<ParallelActivity>();
            workflow1.Expect(wf => wf.CurrentWorkflow).Return(workflow1).Repeat.Any();
            workflow1.Expect(wf => wf.Activities).Return(workflow1Activities).Repeat.Any();
            mocked1D2DCoupler.Expect(wf => wf.CurrentWorkflow).Return(workflow1).Repeat.Any();
            mockedF1DModel.Expect(m => m.Owner).Return(mocked1D2DCoupler).Repeat.Any();
            mockedFmModel.Expect(m => m.Owner).Return(mocked1D2DCoupler).Repeat.Any();

            mocks1.ReplayAll();

            mocked1D2DCouplerActivities.AddRange(new List<IActivity> { new ActivityWrapper(mockedF1DModel), new ActivityWrapper(mockedFmModel) });
            workflow1Activities.AddRange(new List<IActivity> { new ActivityWrapper(mockedRtcModel), new ActivityWrapper(mocked1D2DCoupler) });

            var expectedXmlDoc = TestHelper.GetTestFilePath(@"dimrExport\expected.xml");
            expectedXmlDoc = TestHelper.CreateLocalCopy(expectedXmlDoc);

            var writer = new DHydroConfigWriter();
            //initialize writer
            TypeUtils.TrySetValueAnyVisibility(writer, typeof(DHydroConfigWriter), "modelCouplers", new List<IDimrConfigModelCoupler>());
            TypeUtils.TrySetValueAnyVisibility(writer, typeof(DHydroConfigWriter), "CouplerModelsDictionary", new Dictionary<IModel, IDimrModel>());
            TypeUtils.TrySetValueAnyVisibility(writer, typeof(DHydroConfigWriter), "CoreCountDictionary", new Dictionary<IDimrModel, int>());

            var xml = writer.CreateConfigDocument(workflow1);
            XDocument expectedXml = XDocument.Load(expectedXmlDoc);
            var creationDate = xml.Descendants().SingleOrDefault(p => p.Name.LocalName == "creationDate");
            Assert.NotNull(creationDate);
            creationDate.Value = "";
            Assert.That(xml.ToString(), Is.EqualTo(expectedXml.ToString()));

            mocks1.VerifyAll();

        }

        [Test]
        [Category(TestCategory.Slow)]
        public void WriteAndCheckEmptyDocumentIsNotValid()
        {
            var model = new HydroModel();
            var configWriter = new DHydroConfigWriter();
            bool exportFailed = true;
            try
            {
                configWriter.CreateConfigDocument(model);
                exportFailed = false;
            }
            catch
            {
                Assert.That(exportFailed, Is.True);
            }         
        }


        [Test]
        public void WriteDocumentWithComponents()
        {
            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(new WaterFlowFMModel());
            hydroModel.Activities.Add(new WaveModel());
            hydroModel.Activities.Add(new RealTimeControlModel());
            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();
            var xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }

        [Test]
        public void WriteDocumentWithCouplings()
        {
            var hydroModel = BuildCoupledDemoModel();
            var xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }

        [Test]
        public void WriteDocument_RTC_FM_HasLoggerElement()
        {
            var hydroModel = BuildCoupledDemoModel();
            CheckCouplerXml(hydroModel);
        }

        #region PrivateHelperMethods
        private static HydroModel BuildCoupledDemoModel()
        {
            var hydroModel = new HydroModel();
            var waterFlowFMModel = new WaterFlowFMModel();
            var realTimeControlModel = new RealTimeControlModel();

            hydroModel.Activities.Add(waterFlowFMModel);
            hydroModel.Activities.Add(new WaveModel());
            hydroModel.Activities.Add(realTimeControlModel);

            var pump = new Pump2D("pomp")
            {
                Geometry = new LineString(new[] { new Coordinate(-50, -100), new Coordinate(50, -100) })
            };
            var gate = new Weir2D("poort")
            {
                Geometry = new LineString(new[] { new Coordinate(-50, 100), new Coordinate(50, 100) })
            };
            var obserVationPoint = new GroupableFeature2DPoint
            {
                Name = "station",
                Geometry = new Point(0, 0)
            };

            waterFlowFMModel.Area.Pumps.Add(pump);
            waterFlowFMModel.Area.Weirs.Add(gate);
            waterFlowFMModel.Area.ObservationPoints.Add(obserVationPoint);

            realTimeControlModel.ControlGroups.Add(RealTimeControlModelHelper.CreateStandardControlGroup("InvertorRule"));

            var waterDepth = waterFlowFMModel.GetChildDataItems(obserVationPoint).ElementAt(1);
            var pumpCapacity = waterFlowFMModel.GetChildDataItems(pump).First();

            var controlGroupDataItem = realTimeControlModel.DataItems.First(di => di.Value is ControlGroup);
            var rtcInput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var rtcOutput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            rtcInput.LinkTo(waterDepth);
            pumpCapacity.LinkTo(rtcOutput);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();

            return hydroModel;
        }

        /*  **** DIMR **** 15/8/2016
         * XSD for Dimr does not consider an empty model. Thus for any model is expecting
         * the nodes control (with its children) and component (with its attributes / children).
         * In our opinion this should not be the case, however, if this changes in the future
         * we will be able to detect it, (discuss it) and fix it.
         */

        private static void CheckCouplerXml(HydroModel hydroModel)
        {
            var xml = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var couplers = xml.Descendants().Where(p => p.Name.LocalName == "coupler" && p.HasElements).ToList();

            Assert.IsTrue(couplers.Any());
            foreach (var coupler in couplers)
            {
                var couplerName = coupler.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "name");
                Assert.IsNotNull(couplerName);
                Assert.IsNotNull(couplerName.Value);

                var logger = coupler.Descendants().SingleOrDefault(c => c.Name.LocalName == "logger");
                Assert.IsNotNull(logger);
                Assert.IsTrue(logger.HasElements);

                var outputFileElement = logger.Descendants().SingleOrDefault(l => l.Name.LocalName == "outputFile");
                Assert.IsNotNull(outputFileElement);

                var couplerNameWithExtension = string.Concat(couplerName.Value, ".nc");
                Assert.AreEqual(couplerNameWithExtension, outputFileElement.Value);
            }
            ValidateXml(xml);
        }

        private static void ValidateXml(XDocument xmlDocument, bool expectedToFail = false, Action<string> assertFailMessage = null)
        {
            if (assertFailMessage == null)
                assertFailMessage = (s) => Assert.Fail("Couldn't validate dimr xml!!" + Environment.NewLine + s);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            settings.ValidationEventHandler +=
                new ValidationEventHandler(
                    (s, e) =>
                    {
                        if (!expectedToFail) assertFailMessage(e.Message);
                    });
            try
            {
                FileUtils.DeleteIfExists("mytest.xml");
                xmlDocument.Save("mytest.xml");
                // Create the XmlReader object.
                using (var xmlReader = XmlReader.Create("mytest.xml", settings))
                {
                    // Parse the file. 
                    while (xmlReader.Read())
                    {
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists("mytest.xml");
            }
        }
        
        #endregion

    }

}