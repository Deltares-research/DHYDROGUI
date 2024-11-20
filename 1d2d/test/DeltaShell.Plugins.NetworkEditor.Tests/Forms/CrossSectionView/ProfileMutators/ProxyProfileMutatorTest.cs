using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView.ProfileMutators
{
    [TestFixture]
    public class ProxyProfileMutatorTest
    {
        private static readonly MockRepository mocks = new MockRepository();

        [Test]
        public void AddPointAddsPointToInnerMutator()
        {
            var crossSectionDefinitionYZ = new CrossSectionDefinitionYZ();

            var proxyDefinition = new CrossSectionDefinitionProxy(crossSectionDefinitionYZ);
            var innerMutator = mocks.StrictMock<ICrossSectionProfileMutator>();

            var proxy = new ProxyProfileMutator(proxyDefinition, innerMutator);

            Expect.Call(()=>innerMutator.AddPoint(2, 2));
            mocks.ReplayAll();
            proxy.AddPoint(2,2);
            mocks.VerifyAll();

        }
    }
}