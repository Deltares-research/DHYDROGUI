using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public class Iterative1D2DCouplerExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Iterative1D2DCouplerExporter));

        #region Implementation of IFileExporter

        public bool Export(object item, string path)
        {
            var iterative1D2DCoupler = item as Iterative1D2DCoupler;
            if (iterative1D2DCoupler == null) return false;

            var iterative1D2DCouplerData = iterative1D2DCoupler.Data as Iterative1D2DCouplerData;
            if (iterative1D2DCouplerData == null) return false;

            var oldRefresh1D2DLinksValue = iterative1D2DCouplerData.Refresh1D2DLinks;
            
            var modelName = Path.GetFileNameWithoutExtension(path);
            try
            {
                iterative1D2DCouplerData.Refresh1D2DLinks = true; 
                Iterative1D2DCouplerFileWriter.Write(path, iterative1D2DCoupler);
                iterative1D2DCoupler.Name = modelName;
            }
            catch (Exception exception)
            {
                Log.Error("Model writing failed." + Environment.NewLine + exception.Message);
                return false;
            }
            finally
            {
                iterative1D2DCouplerData.Refresh1D2DLinks = oldRefresh1D2DLinksValue;
            }
            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(Iterative1D2DCoupler);
        }

        public bool CanExportFor(object item)
        {
            return true;
        }

        public string Name { get { return "Iterative1D2DCoupler Exporter"; } }
        public string Category
        {
            get { return "Iterative1D2DCoupler Models"; }
        }

        public string Description
        {
            get { return string.Empty; }
        }
        public string FileFilter { get { return "ini|*.ini"; } }
        public Bitmap Icon { get { return Resources.coupled_1d2d; } }

        #endregion
    }
}
