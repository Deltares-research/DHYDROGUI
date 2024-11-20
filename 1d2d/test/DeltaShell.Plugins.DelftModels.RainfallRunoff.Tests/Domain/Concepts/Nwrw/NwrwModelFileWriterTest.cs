using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    [TestFixture]
    public class NwrwModelFileWriterTest
    {
        [Test]
        public void Write_WithWriters_CallWritersWithCorrectArguments()
        {
            // Setup
            const string path = "just path";

            var mocks = new MockRepository();
            var writerOne = mocks.StrictMock<INwrwComponentFileWriterBase>();
            writerOne.Expect(writer => writer.Write(path)).Return(true);

            var writerTwo = mocks.StrictMock<INwrwComponentFileWriterBase>();
            writerTwo.Expect(writer => writer.Write(path)).Return(true);
            mocks.ReplayAll();

            var modelWriter = new NwrwModelFileWriter(new []
            {
                writerOne,
                writerTwo
            });

            // Call
            modelWriter.WriteNwrwFiles(path);

            // Assert
            mocks.VerifyAll(); // Verifies whether the write methods are called accordingly
            
        }
    }
}