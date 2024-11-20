using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.ViewModels
{
    [TestFixture]
    public class MeteoStationViewModelTest
    {
        private MeteoStationViewModel ViewModel { get; set; }

        [SetUp]
        public void SetUp()
        {
            ViewModel = new MeteoStationViewModel();
        }

        [Test]
        public void ImplementsINotifyPropertyChanged()
        {
            Assert.That(ViewModel, Is.InstanceOf<INotifyPropertyChanged>());
        }

        [Test]
        public void Name_NotifiesPropertyChanged()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            ViewModel.Name = "station";

            // Assert
            Assert.That(callBacks, Has.Count.EqualTo(1));

            Assert.Multiple(() =>
            {
                (object s, PropertyChangedEventArgs args) = callBacks.First();
                Assert.That(s, Is.SameAs(ViewModel));
                Assert.That(args.PropertyName, Is.EqualTo(nameof(ViewModel.Name)));
            });

            // Clean up
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }


        [Test]
        public void IsSelected_NotifiesPropertyChanged()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            ViewModel.IsSelected = !ViewModel.IsSelected;

            // Assert
            Assert.That(callBacks, Has.Count.EqualTo(1));

            Assert.Multiple(() =>
            {
                (object s, PropertyChangedEventArgs args) = callBacks.First();
                Assert.That(s, Is.SameAs(ViewModel));
                Assert.That(args.PropertyName, Is.EqualTo(nameof(ViewModel.IsSelected)));
            });

            // Clean up
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }
    }
}