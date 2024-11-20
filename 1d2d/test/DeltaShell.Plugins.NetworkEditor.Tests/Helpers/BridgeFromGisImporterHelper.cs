using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    public static class BridgeFromGisImporterHelper
    {
        /// <summary>
        /// TestName used for test class <see cref="TestBaseBridge"/>.
        /// </summary>
        public const string TestName = "TestName";

        /// <summary>
        /// TestClass to test the base of the bridgeFromGisImporterBase.
        /// </summary>
        public class TestBaseBridge : BridgeFromGisImporterBase
        {
            public override string Name => TestName;
            protected override BridgeType BridgeType { get; }
        }
    }
}