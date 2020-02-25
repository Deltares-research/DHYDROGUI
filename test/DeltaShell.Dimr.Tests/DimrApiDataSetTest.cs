using System;
using System.Linq;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrApiDataSetTest
    {
        [Test]
        public void SetSharedPath_NotContained_PathContainsSharedPathAtTheEnd()
        {
            // Setup
            string oldPath = Environment.GetEnvironmentVariable("PATH");
            Assert.That(oldPath.Contains(DimrApiDataSet.SharedDllPath), Is.False, "Precondition violated.");

            // Call
            DimrApiDataSet.SetSharedPath();

            // Assert
            string newPath = Environment.GetEnvironmentVariable("PATH");
            string[] paths = newPath.Split(';');
            Assert.That(paths.Last(), Is.EqualTo(DimrApiDataSet.SharedDllPath));

            // Clean up
            Environment.SetEnvironmentVariable("PATH", oldPath);
        }

        [Test]
        public void SetSharedPath_Contained_DoesNotAddSecondSharedPath()
        {
            // Setup
            string oldPath = Environment.GetEnvironmentVariable("PATH");

            Environment.SetEnvironmentVariable("PATH", DimrApiDataSet.SharedDllPath + ";" + oldPath);
            string modifiedPath = Environment.GetEnvironmentVariable("PATH");
            Assert.That(modifiedPath.Contains(DimrApiDataSet.SharedDllPath), Is.True, "Precondition violated.");

            // Call
            DimrApiDataSet.SetSharedPath();

            // Assert
            string newPath = Environment.GetEnvironmentVariable("PATH");
            string[] paths = newPath.Split(';');
            Assert.That(paths.Count(x => x == DimrApiDataSet.SharedDllPath), Is.EqualTo(1));

            // Clean up
            Environment.SetEnvironmentVariable("PATH", oldPath);
        }
    }
}