using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.CustomRenderers
{
    [TestFixture]
    public class DiffuseLateralSourceRendererTest
    {
        [Test]
        public void TestClone()
        {
            var diffuseLateralSourceRenderer = new DiffuseLateralSourceRenderer();
            var clone = diffuseLateralSourceRenderer.Clone() as DiffuseLateralSourceRenderer;

            Assert.IsNotNull(clone);
            Assert.AreNotSame(diffuseLateralSourceRenderer, clone);
        }
    }
}
