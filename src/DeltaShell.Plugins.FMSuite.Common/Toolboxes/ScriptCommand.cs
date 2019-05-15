using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DeltaShell.Plugins.Scripting;

namespace DeltaShell.Plugins.FMSuite.Common.Toolboxes
{
    public class ScriptCommand
    {
        public string Title { get; private set; }
        public Image Image { get; private set; }
        private string ScriptPath { get; set; }

        public ScriptCommand(string title, Image image, string scriptPath)
        {
            Title = title;
            Image = image;
            ScriptPath = scriptPath;
        }

        public void Execute(IScriptLogger scriptLogger = null, Dictionary<string, object> predefVariables = null)
        {
            var host = new ScriptHost(true, true);

            // add any additional variables to scope
            if (predefVariables != null)
            {
                foreach (KeyValuePair<string, object> keyValue in predefVariables)
                {
                    host.Scope.SetVariable(keyValue.Key, keyValue.Value);
                }
            }

            // execute
            host.Execute(File.ReadAllText(ScriptPath));

            RedirectOutputToLogger(host, scriptLogger);
        }

        private void RedirectOutputToLogger(ScriptHost host, IScriptLogger scriptLogger)
        {
            if (scriptLogger == null)
            {
                return;
            }

            string output = host.ReadOutput();

            if (!string.IsNullOrEmpty(output))
            {
                foreach (string str in output.Split('\n'))
                {
                    scriptLogger.Info(str);
                }
            }

            string errors = host.ReadError();

            if (!string.IsNullOrEmpty(errors))
            {
                foreach (string str in errors.Split('\n'))
                {
                    scriptLogger.Error(str);
                }
            }

            if (!string.IsNullOrEmpty(host.ErrorFromLastExecution))
            {
                scriptLogger.Error("Exception thrown while running command:");
                scriptLogger.Error(host.ErrorFromLastExecution);
            }

            scriptLogger.Info("Command '{0}' done.", Title);
        }
    }

    public interface IScriptLogger
    {
        void Info(string format, params string[] args);
        void Error(string format, params string[] args);
    }
}