using System;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Layers;

namespace DeltaShell.NGHS.Common.Gui.Tests.MapLayers
{
    [TestFixture]
    public class LayerExtensionsTest
    {
        [Test]
        public void SetName_ArgumentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((Layer) null).SetName("new_name");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("layer"));
        }

        [Test]
        public void SetName_SetsNameCorrectly_AndRestoresProperty([Values] bool nameIsReadOnly)
        {
            // Setup
            var layer = Substitute.For<Layer>();
            layer.NameIsReadOnly = nameIsReadOnly;

            // Call
            layer.SetName("new_name");

            // Assert
            Assert.That(layer.NameIsReadOnly, Is.EqualTo(nameIsReadOnly));
            Assert.That(layer.Name, Is.EqualTo("new_name"));
        }
    }
}