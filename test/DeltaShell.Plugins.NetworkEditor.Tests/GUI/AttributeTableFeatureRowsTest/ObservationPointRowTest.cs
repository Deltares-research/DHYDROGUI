using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class ObservationPointRowTest
    {
        [Test]
        public void Constructor_WithNullObservationPoint_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new ObservationPointRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsObservationPoint()
        {
            // Arrange
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(observationPoint));
        }

        [Test]
        public void WhenObservationPointPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            observationPoint.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsObservationPointName()
        {
            // Arrange
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(observationPoint.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsObservationPointName()
        {
            // Arrange
            var observationPoint = new ObservationPoint { Name = "some_name" };
            var row = new ObservationPointRow(observationPoint);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void SetLongName_SetsObservationPointLongName()
        {
            // Arrange
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint);

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(observationPoint.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsObservationPointLongName()
        {
            // Arrange
            var observationPoint = new ObservationPoint { LongName = "some_long_name" };
            var row = new ObservationPointRow(observationPoint);

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_GetsObservationPointBranchName()
        {
            // Arrange
            var branch = new Branch { Name = "some_branch_name" };
            var observationPoint = new ObservationPoint { Branch = branch };
            var row = new ObservationPointRow(observationPoint);

            // Act
            string result = row.Branch;

            // Assert
            Assert.AreEqual(result, "some_branch_name");
        }

        [Test]
        public void GetChainage_GetsObservationPointChainage()
        {
            // Arrange
            var observationPoint = new ObservationPoint { Chainage = 123.45 };
            var row = new ObservationPointRow(observationPoint);

            // Act
            double result = row.Chainage;

            // Assert
            Assert.That(result, Is.EqualTo(123.45));
        }
    }
}