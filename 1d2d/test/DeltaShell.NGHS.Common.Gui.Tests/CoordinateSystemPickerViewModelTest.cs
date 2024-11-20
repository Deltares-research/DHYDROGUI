using GeoAPI.Extensions.CoordinateSystems;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests
{
    [TestFixture]
    public class CoordinateSystemPickerViewModelTest
    {
        [Test]
        public void GivenCoordinateSystemPickerViewModel_Filtering_ShouldWork()
        {
            //Arrange
            var viewModel = new CoordinateSystemPickerViewModel();

            // Act
            Assert.AreNotEqual(1, viewModel.CoordinateSystems.Count);

            var viewModelFilterText = "rd n";
            viewModel.FilterText = viewModelFilterText;

            // Assert
            Assert.AreEqual(1, viewModel.CoordinateSystems.Count);
            Assert.AreEqual(viewModelFilterText, viewModel.FilterText);
        }

        [Test]
        public void GivenCoordinateSystemPickerViewModel_ChangingSelectedCoordinateSystem_ShouldTriggerPropertyChanged()
        {
            //Arrange
            var viewModel = new CoordinateSystemPickerViewModel();

            // Act
            int propertyChangedCount = 0;
            viewModel.PropertyChanged += (s,e) => propertyChangedCount++;

            var coordinateSystem = (ICoordinateSystem) viewModel.CoordinateSystems.GetItemAt(9);
            viewModel.SelectedCoordinateSystem = coordinateSystem;

            // Assert
            Assert.AreEqual(1, propertyChangedCount);
            Assert.AreEqual(coordinateSystem, viewModel.SelectedCoordinateSystem);
        }
    }
}