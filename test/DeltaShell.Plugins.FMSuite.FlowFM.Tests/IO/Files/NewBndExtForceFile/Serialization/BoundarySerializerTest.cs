using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Serialization
{
    [TestFixture]
    public class BoundarySerializerTest
    {
        [Test]
        public void Serialize_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundarySerializer = new BoundarySerializer();

            // Call
            void Call() => boundarySerializer.Serialize(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Serialize_ReturnTimeHasValue_SerializesTheBoundaryDTOToADelftINICategory()
        {
            // Setup
            var boundarySerializer = new BoundarySerializer();
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 1.23);

            // Call
            DelftIniCategory delftIniCategory = boundarySerializer.Serialize(boundaryDTO);

            // Assert
            Assert.That(delftIniCategory.Name, Is.EqualTo("boundary"));
            Assert.That(delftIniCategory.Properties, Has.Count.EqualTo(5));
            Assert.That(delftIniCategory.GetPropertyValue("quantity"), Is.EqualTo("some_quantity"));
            Assert.That(delftIniCategory.GetPropertyValue("locationFile"), Is.EqualTo("some_location_file"));
            Assert.That(delftIniCategory.GetPropertyValues("forcingFile"), Is.EqualTo(forcingFiles));
            Assert.That(delftIniCategory.GetPropertyValue("returnTime"), Is.EqualTo("1.2300000e+000"));
        }

        [Test]
        public void Serialize_ReturnTimeHasNoValue_SerializesTheBoundaryDTOToADelftINICategory()
        {
            // Setup
            var boundarySerializer = new BoundarySerializer();
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, null);

            // Call
            DelftIniCategory delftIniCategory = boundarySerializer.Serialize(boundaryDTO);

            // Assert
            Assert.That(delftIniCategory.Name, Is.EqualTo("boundary"));
            Assert.That(delftIniCategory.Properties, Has.Count.EqualTo(4));
            Assert.That(delftIniCategory.GetPropertyValue("quantity"), Is.EqualTo("some_quantity"));
            Assert.That(delftIniCategory.GetPropertyValue("locationFile"), Is.EqualTo("some_location_file"));
            Assert.That(delftIniCategory.GetPropertyValues("forcingFile"), Is.EqualTo(forcingFiles));
        }
    }
}