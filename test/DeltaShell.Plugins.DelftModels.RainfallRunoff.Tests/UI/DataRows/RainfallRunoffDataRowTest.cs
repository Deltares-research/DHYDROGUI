using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.DataRows
{
    [TestFixture]
    public class RainfallRunoffDataRowTest
    {
        [Test]
        public void DataRowSendsPropertyChanged()
        {
            var pavedData = new PavedData(new Catchment());
            var dataRow = new PavedDataRow();
            dataRow.Initialize(pavedData);

            var called = 0;
            dataRow.PropertyChanged += (s, e) => called++;

            pavedData.CalculationArea = 500;
            Assert.AreEqual(1, called);

            pavedData.InitialStreetStorage = 5;
            Assert.AreEqual(2, called);
        }

        [Test]
        public void UnpavedAreaDictionarySendsPropertyChanged()
        {
            var unpavedData = new UnpavedData(new Catchment());
            var dataRow = new UnpavedDataRow();
            dataRow.Initialize(unpavedData);

            var called = 0;
            dataRow.PropertyChanged += (s, e) => called++;

            unpavedData.CalculationArea = 500;
            Assert.AreEqual(2, called);

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Corn] = 15.0;
            Assert.AreEqual(3, called);
        }
    }
}