using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveMeteoDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var meteoData = new WaveMeteoData();

            // Assert
            Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXY));
        }

        private static IEnumerable<TestCaseData> GetPropertyChangedData()
        {
            void UpdateFileType(WaveMeteoData data) => data.FileType = WindDefinitionType.SpiderWebGrid;
            yield return new TestCaseData((Action<WaveMeteoData>)UpdateFileType,
                                          nameof(WaveMeteoData.FileType));

            void UpdateXYVectorPath(WaveMeteoData data) => data.XYVectorFilePath = "somePath";
            yield return new TestCaseData((Action<WaveMeteoData>)UpdateXYVectorPath,
                                          nameof(WaveMeteoData.XYVectorFilePath));

            void UpdateXComponentFilePath(WaveMeteoData data) => data.XComponentFilePath = "somePath";
            yield return new TestCaseData((Action<WaveMeteoData>)UpdateXComponentFilePath,
                                          nameof(WaveMeteoData.XComponentFilePath));

            void UpdateYComponentFilePath(WaveMeteoData data) => data.YComponentFilePath = "somePath";
            yield return new TestCaseData((Action<WaveMeteoData>)UpdateYComponentFilePath,
                                          nameof(WaveMeteoData.YComponentFilePath));


            void UpdateHasSpiderWeb(WaveMeteoData data) => data.HasSpiderWeb = true;
            yield return new TestCaseData((Action<WaveMeteoData>)UpdateHasSpiderWeb,
                                          nameof(WaveMeteoData.HasSpiderWeb));

            void UpdateSpiderWebFilePath(WaveMeteoData data) => data.SpiderWebFilePath = "somePath";
            yield return new TestCaseData((Action<WaveMeteoData>)UpdateSpiderWebFilePath,
                                          nameof(WaveMeteoData.SpiderWebFilePath));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyChangedData))]
        public void PropertyChanged_RaisesEvent(Action<WaveMeteoData> UpdateProperty,
                                                string expectedParameterName)
        {
            // Setup
            var meteoData = new WaveMeteoData();
            var observer = new EventTestObserver<PropertyChangedEventArgs>();

            ((INotifyPropertyChanged)meteoData).PropertyChanged += observer.OnEventFired;

            // Call
            UpdateProperty.Invoke(meteoData);

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders[0], Is.SameAs(meteoData));
            Assert.That(observer.EventArgses[0].PropertyName,
                        Is.EqualTo(expectedParameterName));
        }
    }
}