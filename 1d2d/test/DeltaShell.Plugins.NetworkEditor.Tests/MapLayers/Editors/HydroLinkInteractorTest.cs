using System.Drawing;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    [TestFixture]
    public class HydroLinkInteractorTest
    {
        [Test]
        public void PropertyTest()
        {
            var map = new Map {Size = new Size(1000, 1000)};
            var hydroLink = new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>());
            var hydroLinkInteractor = new HydroLinkInteractor(new VectorLayer { Map = map }, hydroLink, null, null);
            
            Assert.AreEqual(true, hydroLinkInteractor.AllowDeletion());
            Assert.AreEqual(false, hydroLinkInteractor.AllowMove());
        }
    }
}
