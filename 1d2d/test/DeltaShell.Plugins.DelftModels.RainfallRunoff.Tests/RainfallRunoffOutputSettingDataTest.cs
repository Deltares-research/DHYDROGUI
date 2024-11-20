using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffOutputSettingDataTest
    {

        [Test]
        public void FireIsEnabledEvent()
        {
            var count = 0;
            var outputSettings = new RainfallRunoffOutputSettingData();

            ((INotifyPropertyChanged) outputSettings).PropertyChanged += (s, e) =>
                                                                             {
                                                                                 count++;
                                                                             };

            var firstParameterOnNone = outputSettings.EngineParameters.FirstOrDefault(ep => !ep.IsEnabled);

            Assert.IsNotNull(firstParameterOnNone);

            firstParameterOnNone.IsEnabled = true;

            Assert.AreEqual(1,count);

        }
    }
}
