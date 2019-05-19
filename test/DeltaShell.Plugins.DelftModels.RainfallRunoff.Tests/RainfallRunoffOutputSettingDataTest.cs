using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffOutputSettingDataTest
    {

        [Test]
        public void FireAggregationChangeEvent()
        {
            var count = 0;
            var outputSettings = new RainfallRunoffOutputSettingData();

            ((INotifyPropertyChanged) outputSettings).PropertyChanged += (s, e) =>
                                                                             {
                                                                                 count++;
                                                                             };

            var firstParameterOnNone =
                outputSettings.EngineParameters.FirstOrDefault(ep => ep.AggregationOptions == AggregationOptions.None);

            Assert.IsNotNull(firstParameterOnNone);

            firstParameterOnNone.AggregationOptions = AggregationOptions.Current;

            Assert.AreEqual(1,count);

        }
    }
}
