using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.Common.Toolboxes
{
    public static class ToolboxCommands
    {
        public static IEnumerable<ScriptCommand> LoadFrom(string toolboxPath)
        {
            var commands = new List<ScriptCommand>();

            foreach (string scriptPath in Directory.GetFiles(toolboxPath, "*.py"))
            {
                string title = Path.GetFileNameWithoutExtension(scriptPath);
                Bitmap image = LoadImageWithSameName(scriptPath, title);

                commands.Add(new ScriptCommand(title, image, scriptPath));
            }

            return commands;
        }

        private static Bitmap LoadImageWithSameName(string scriptPath, string title)
        {
            string directory = Path.GetDirectoryName(scriptPath) ?? "";
            string imagePath = Path.Combine(directory, title + ".png");
            if (File.Exists(imagePath))
            {
                return (Bitmap) Image.FromFile(imagePath);
            }

            return null;
        }
    }
}