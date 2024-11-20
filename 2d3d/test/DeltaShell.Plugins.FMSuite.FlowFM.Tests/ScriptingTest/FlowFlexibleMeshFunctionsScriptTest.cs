using System.IO;
using DeltaShell.Plugins.Scripting;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ScriptingTest
{
    [TestFixture]
    public class FlowFlexibleMeshFunctionsScriptTest
    {
        [OneTimeSetUp]
        public void TestFixture()
        {
            var standardLibPath = @"plugins\DeltaShell.Plugins.Scripting\Lib";
            string sitePackagesPath = Path.Combine(standardLibPath, "site-packages");

            ScriptHost.AdditionalSearchPaths.Add(standardLibPath);
            ScriptHost.AdditionalSearchPaths.Add(sitePackagesPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ScriptHost.AdditionalSearchPaths.Clear();
        }
    }
}