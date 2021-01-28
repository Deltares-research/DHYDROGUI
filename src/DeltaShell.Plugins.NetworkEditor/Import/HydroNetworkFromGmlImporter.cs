using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using DelftTools.Hydro;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class HydroNetworkFromGmlImporter : IFileImporter
    {
        #region IFileImporter Members

        public virtual string Name
        {
            get { return "Model features from GML"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Data Import"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.HydroRegion; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IHydroNetwork);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "gml"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get { return false; } }

        public virtual object ImportItem(string path, object target = null)
        {
            if (path == null) return null;
            var hydroNetwork = target as IHydroNetwork;

            var gml = File.ReadAllText(path);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gml);
            var gmlNode = xmlDoc.DocumentElement.FirstChild.NextSibling.FirstChild.LastChild.InnerXml;
            Console.WriteLine(gmlNode);

            //TODO : use GMLReader to import
            return hydroNetwork;
        }

        #endregion

    }
}
