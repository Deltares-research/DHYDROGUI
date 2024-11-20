using System;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.TestUtils
{
    /// <summary>
    /// <see cref="TemporaryDirectoryBaseFixture"/> defines a setup which provides the <see cref="TempDir"/>
    /// <see cref="TemporaryDirectory"/> property, which is setup and tore down before each test.
    /// </summary>
    public class TemporaryDirectoryBaseFixture
    {
        protected TemporaryDirectory TempDir { get; private set; }

        [SetUp]
        public void BaseSetUp() => TempDir = new TemporaryDirectory();

        [TearDown]
        public void BaseTearDown() => (TempDir as IDisposable)?.Dispose();
    }
}