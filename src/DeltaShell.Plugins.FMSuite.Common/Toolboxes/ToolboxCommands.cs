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

            foreach (var scriptPath in Directory.GetFiles(toolboxPath, "*.py"))
            {
                var title = Path.GetFileNameWithoutExtension(scriptPath);
                var image = LoadImageWithSameName(scriptPath, title);

                commands.Add(new ScriptCommand(title, image, scriptPath));
            }

            return commands;
        }

        private static Bitmap LoadImageWithSameName(string scriptPath, string title)
        {
            var directory = Path.GetDirectoryName(scriptPath) ?? "";
            var imagePath = Path.Combine(directory, title + ".png");
            if (File.Exists(imagePath))
                return (Bitmap) Image.FromFile(imagePath);
            return null;
        }
    }
}