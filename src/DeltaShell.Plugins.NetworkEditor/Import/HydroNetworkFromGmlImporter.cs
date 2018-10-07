using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using log4net;
using NetTopologySuite.IO.GML2;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class HydroNetworkFromGmlImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroNetworkFromGmlImporter));

        #region IFileImporter Members

        public virtual string Name
        {
            get { return "Model features from GML"; }
        }

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

            //if (hydroNetwork == null) return null;
            var reader = new GMLReader();
            //var document = new XmlDocument();
            //document.Load(path);
            //string gml = document.InnerXml;
            //gml = gml.Replace("gml:", "");
            var result = reader.Read(gmlNode);
            //var geometry = gmlReader.(document.InnerXml);
            return hydroNetwork;
        }

        #endregion

    }
}
