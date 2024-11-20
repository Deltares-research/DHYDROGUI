using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    [Entity(FireOnCollectionChange = false)]
    public class FileBasedModelItem
    {
        public FileBasedModelItem(string propertyName, string filePath)
        {
            PropertyName = propertyName;
            FilePath = filePath;
            DirectChildren = new EventedList<FileBasedModelItem>();
        }

        public FileBasedModelItem Parent { get; private set; }

        public IEventedList<FileBasedModelItem> DirectChildren { get; private set; }

        public string PropertyName { get; private set; }

        public string FilePath { get; private set; }

        public string FileName => Path.GetFileName(FilePath);

        public string Directory => Path.GetDirectoryName(FilePath);

        public bool FileExists => File.Exists(FilePath);

        public FileBasedModelItem AddChildItem(string property, string relativePath)
        {
            if (property == null || relativePath == null)
            {
                return null;
            }

            string absolutePath = Path.Combine(Directory, relativePath);
            var fileBasedModelItem = new FileBasedModelItem(property, absolutePath) {Parent = this};
            DirectChildren.Add(fileBasedModelItem);
            return fileBasedModelItem;
        }

        public void Clear()
        {
            foreach (FileBasedModelItem child in DirectChildren)
            {
                child.Clear();
            }

            DirectChildren.Clear();
        }
    }
}