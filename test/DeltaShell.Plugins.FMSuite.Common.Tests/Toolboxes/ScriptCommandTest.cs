using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Toolboxes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Toolboxes
{
    [TestFixture]
    public class ScriptCommandTest
    {
        [Test]
        public void RunScriptAddScopeAndVerifyLog()
        {
            // load all test scripts
            string toolboxDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "toolboxes");
            var bathyCommand = new ScriptCommand("Bathy", null,
                                                 Path.Combine(toolboxDirectory, "Bathymetry from Gebco.py"));
            var logger = new StubLogger();

            // prepare a scope variable
            var uri = new Uri("file://test-uri");
            var scope = new Dictionary<string, object> {{"uri", uri}};

            // run the script
            bathyCommand.Execute(logger, scope);

            // for debugging purposes, show what we got
            logger.DumpToConsole();

            // verify the script works & got the uri variable:
            Assert.IsTrue(logger.Infos.Any(i => i.StartsWith("Bathymetry script got uri: file://test-uri")));
        }

        private class StubLogger : IScriptLogger
        {
            public readonly List<string> Infos = new List<string>();
            public readonly List<string> Errors = new List<string>();

            public void DumpToConsole()
            {
                Console.WriteLine("Infos:");
                foreach (string info in Infos)
                {
                    Console.WriteLine(info);
                }

                Console.WriteLine("Errors:");
                foreach (string error in Errors)
                {
                    Console.WriteLine(error);
                }
            }

            public void Info(string format, params string[] args)
            {
                Infos.Add(string.Format(format, args));
            }

            public void Error(string format, params string[] args)
            {
                Errors.Add(string.Format(format, args));
            }
        }
    }
}