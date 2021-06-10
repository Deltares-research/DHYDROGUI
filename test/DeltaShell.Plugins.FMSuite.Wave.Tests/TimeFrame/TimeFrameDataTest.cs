using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class TimeFrameDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var data = new TimeFrameData();

            // Assert
            Assert.That(data, Is.InstanceOf<ITimeFrameData>());

            Assert.That(data.HydrodynamicsConstantData, Is.Not.Null);
            Assert.That(data.HydrodynamicsConstantData.WaterLevel, Is.EqualTo(0.0));
            Assert.That(data.HydrodynamicsConstantData.VelocityX, Is.EqualTo(0.0));
            Assert.That(data.HydrodynamicsConstantData.VelocityY, Is.EqualTo(0.0));

            Assert.That(data.WindConstantData, Is.Not.Null);
            Assert.That(data.WindConstantData.Speed, Is.EqualTo(0.0));
            Assert.That(data.WindConstantData.Direction, Is.EqualTo(0.0));

            Assert.That(data.WindFileData, Is.Not.Null);
            Assert.That(data.WindFileData.FileType, Is.EqualTo(WindDefinitionType.WindXY));

            Assert.That(data.HydrodynamicsInputDataType, Is.EqualTo(HydrodynamicsInputDataType.Constant));
            Assert.That(data.WindInputDataType, Is.EqualTo(WindInputDataType.Constant));

            Assert.That(data.TimeVaryingData, Is.Not.EqualTo(null));
            Assert.That(data.TimeVaryingData.Arguments.Count, Is.EqualTo(1));
            Assert.That(data.TimeVaryingData.Arguments[0].Name, Is.EqualTo("Time"));
            Assert.That(data.TimeVaryingData.Arguments[0].DefaultValue, Is.EqualTo(DateTime.Today));

            Assert.That(data.TimeVaryingData.Components.Count, Is.EqualTo(5));
            AssertCorrectVariable(data.TimeVaryingData.Components[0], "Water Level", "meter", "m");
            AssertCorrectVariable(data.TimeVaryingData.Components[1], "Velocity X", "meter per second", "m/s");
            AssertCorrectVariable(data.TimeVaryingData.Components[2], "Velocity Y", "meter per second", "m/s");
            AssertCorrectVariable(data.TimeVaryingData.Components[3], "Wind Speed", "meter per second", "m/s");
            AssertCorrectVariable(data.TimeVaryingData.Components[4], "Wind Direction", "degrees", "deg");

            Assert.That(data.TimePoints, Is.Empty);
        }

        private static void AssertCorrectVariable(IVariable variable,
                                                  string expectedName,
                                                  string expectedUnitName,
                                                  string expectedUnitSymbol)
        {
            Assert.That(variable.Name, Is.EqualTo(expectedName));
            Assert.That(variable.Unit.Name, Is.EqualTo(expectedUnitName));
            Assert.That(variable.Unit.Symbol, Is.EqualTo(expectedUnitSymbol));
        }

        public static IEnumerable<TestCaseData> GetPropertyChangedData()
        {
            object Identity(ITimeFrameData data) => data;
            void UpdateHydrodynamicsInputDataType(ITimeFrameData data) => data.HydrodynamicsInputDataType = HydrodynamicsInputDataType.TimeVarying;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateHydrodynamicsInputDataType,
                                          (Func<ITimeFrameData, object>)Identity,
                                          nameof(ITimeFrameData.HydrodynamicsInputDataType));
            void UpdateWindInputDataType(ITimeFrameData data) => data.WindInputDataType = WindInputDataType.TimeVarying;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateWindInputDataType,
                                          (Func<ITimeFrameData, object>)Identity,
                                          nameof(ITimeFrameData.WindInputDataType));

            object Hydrodynamics(ITimeFrameData data) => data.HydrodynamicsConstantData;
            void UpdateWaterLevel(ITimeFrameData data) => data.HydrodynamicsConstantData.WaterLevel = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateWaterLevel,
                                          (Func<ITimeFrameData, object>)Hydrodynamics,
                                          nameof(HydrodynamicsConstantData.WaterLevel));
            void UpdateVelocityX(ITimeFrameData data) => data.HydrodynamicsConstantData.VelocityX = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateVelocityX,
                                          (Func<ITimeFrameData, object>)Hydrodynamics,
                                          nameof(HydrodynamicsConstantData.VelocityX));
            void UpdateVelocityY(ITimeFrameData data) => data.HydrodynamicsConstantData.VelocityY = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateVelocityY,
                                          (Func<ITimeFrameData, object>)Hydrodynamics,
                                          nameof(HydrodynamicsConstantData.VelocityY));

            object Wind(ITimeFrameData data) => data.WindConstantData;
            void UpdateSpeed(ITimeFrameData data) => data.WindConstantData.Speed = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateSpeed,
                                          (Func<ITimeFrameData, object>)Wind,
                                          nameof(WindConstantData.Speed));
            void UpdateDirection(ITimeFrameData data) => data.WindConstantData.Direction = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateDirection,
                                          (Func<ITimeFrameData, object>)Wind,
                                          nameof(WindConstantData.Direction));

            object WindFiles(ITimeFrameData data) => data.WindFileData;
            void UpdateFileType(ITimeFrameData data) => data.WindFileData.FileType = WindDefinitionType.SpiderWebGrid;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateFileType,
                                          (Func<ITimeFrameData, object>)WindFiles,
                                          nameof(WaveMeteoData.FileType));

            void UpdateXYVectorPath(ITimeFrameData data) => data.WindFileData.XYVectorFilePath = "somePath";
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateXYVectorPath,
                                          (Func<ITimeFrameData, object>)WindFiles,
                                          nameof(WaveMeteoData.XYVectorFilePath));

            void UpdateXComponentFilePath(ITimeFrameData data) => data.WindFileData.XComponentFilePath = "somePath";
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateXComponentFilePath,
                                          (Func<ITimeFrameData, object>)WindFiles,
                                          nameof(WaveMeteoData.XComponentFilePath));

            void UpdateYComponentFilePath(ITimeFrameData data) => data.WindFileData.YComponentFilePath = "somePath";
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateYComponentFilePath,
                                          (Func<ITimeFrameData, object>)WindFiles,
                                          nameof(WaveMeteoData.YComponentFilePath));


            void UpdateHasSpiderWeb(ITimeFrameData data) => data.WindFileData.HasSpiderWeb = true;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateHasSpiderWeb,
                                          (Func<ITimeFrameData, object>)WindFiles,
                                          nameof(WaveMeteoData.HasSpiderWeb));

            void UpdateSpiderWebFilePath(ITimeFrameData data) => data.WindFileData.SpiderWebFilePath = "somePath";
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateSpiderWebFilePath,
                                          (Func<ITimeFrameData, object>)WindFiles,
                                          nameof(WaveMeteoData.SpiderWebFilePath));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyChangedData))]
        public void PropertyChanged_CallsNotifyPropertyChangedCorrectly(Action<ITimeFrameData> updateProperty,
                                                                        Func<ITimeFrameData, object> getSender,
                                                                        string expectedPropertyName)
        {
            // Setup
            var data = new TimeFrameData();
            var observer = new EventTestObserver<PropertyChangedEventArgs>();

            ((INotifyPropertyChanged)data).PropertyChanged += observer.OnEventFired;

            // Call
            updateProperty(data);

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders.First(), Is.SameAs(getSender(data)));

            PropertyChangedEventArgs args = observer.EventArgses.First();
            Assert.That(args.PropertyName, Is.EqualTo(expectedPropertyName));
        }
    }
}