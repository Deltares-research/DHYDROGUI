using System.IO;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.TestUtils
{
    public class DeltaShellFrameworkSimulator
    {
        private readonly IFileBased model;

        public DeltaShellFrameworkSimulator(IFileBased model)
        {
            this.model = model;
        }
        public void NewProject(string filePath)
        {
            model.CreateNew(filePath);
        }

        public void SaveAs(string filePath)
        {
            model.CopyTo(filePath);
            model.SwitchTo(filePath);

            model.Path = filePath;
            model.SwitchTo(filePath);
        }

        public void FirstSave(string filePath)
        {
            SaveAs(filePath);
        }

        public void Save(string filePath)
        {
            model.Path = "$" + filePath;
            model.SwitchTo(filePath);
        }

        public void OpenProject(string filePath)
        {
            string relativePath = Path.GetFileName(filePath);
            model.Path = relativePath;
            model.SwitchTo(filePath);
        }

        public void RemoveModelFromProject()
        {
            model.Delete();
        }
    }
}