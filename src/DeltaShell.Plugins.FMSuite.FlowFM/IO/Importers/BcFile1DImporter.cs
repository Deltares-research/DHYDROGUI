using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Boundary;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class BcFile1DImporter :  IFileImporter
    {
        private object ImportOnModel1DBoundaryNodeData(string filePath, Model1DBoundaryNodeData model1DBoundaryNodeData)
        {
            if (ShouldCancel) return model1DBoundaryNodeData;
            BoundaryFileReader.ReadFile(filePath, new[] {model1DBoundaryNodeData});
            OpenViewAfterImport = false;
            return model1DBoundaryNodeData;
        }

        private object ImportOnListModel1DBoundaryNodeData(string filePath, IEnumerable<Model1DBoundaryNodeData> model1DBoundaryNodeDatas)
        {
            if (ShouldCancel) return model1DBoundaryNodeDatas;
            BoundaryFileReader.ReadFile(filePath, model1DBoundaryNodeDatas);
            OpenViewAfterImport = false;
            return model1DBoundaryNodeDatas;
        }

        #region IFileImporter

        public string Name
        {
            get { return "Boundary data 1D from .bc file"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Boundary data 1D"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<Model1DBoundaryNodeData>);
                yield return typeof(Model1DBoundaryNodeData);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "Boundary conditions 1d file|*.bc"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public object ImportItem(string filePath, object target = null)
        {
            switch (target)
            {
                case IEnumerable<Model1DBoundaryNodeData> listModel1DBoundaryNodeData:
                    return ImportOnListModel1DBoundaryNodeData(filePath, listModel1DBoundaryNodeData);
                case Model1DBoundaryNodeData model1DBoundaryNodeData:
                    return ImportOnModel1DBoundaryNodeData(filePath, model1DBoundaryNodeData);
                default:
                    throw new ArgumentException("Boundary condition 1D bc-file importer could not import data onto given target");
            }
        }

        #endregion

    }
}