using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    public class RainfallRunoffModelExporter : IFileExporter
    {
        private readonly IBasinGeometrySerializer serializer;

        public RainfallRunoffModelExporter(): this(new BasinGeometryShapeFileSerializer())
        {
            
        }

        public RainfallRunoffModelExporter(IBasinGeometrySerializer serializer)
        {
            Ensure.NotNull(serializer, nameof(serializer));
            this.serializer = serializer;
        }

        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return "Rainfall Runoff Model exporter"; }
        }

        [ExcludeFromCodeCoverage]
        public string Category
        {
            get { return ""; }
        }

        public string Description
        {
            get { return Name; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(RainfallRunoffModel);
        }
        
        public string FileFilter
        {
            get { return "RR file folder name export|*."; }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }

        [ExcludeFromCodeCoverage]
        public bool CanExportFor(object item)
        {
            return true;
        }

        public bool Export(object item, string path)
        {
            var model = item as RainfallRunoffModel;
            if (model == null) return false;
            var bcWriter = new RainfallRunoffBoundaryDataFileWriter();
            bcWriter.WriteFile(Path.Combine(Path.GetFullPath(path), "BoundaryConditions.bc"), model);
            
            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new Nwrw3BComponentFileWriter(model),
                new NwrwAlgComponentFileWriter(model),
                new NwrwDwfComponentFileWriter(model), 
                new NwrwTpComponentFileWriter(model), 
            });
            nwrwWriter.WriteNwrwFiles(path);
            model.ModelController.GetWorkingDirectoryDelegate = () => Path.GetFullPath(path);
            model.ModelController.WriteFiles();

            var meteoWriter = new MeteoDataExporter();
            if (model.Evaporation.DataDistributionType != MeteoDataDistributionType.Global)
            {
                WriteToFile(path, "default.evp", p => meteoWriter.Export(model.Evaporation, p));
            }
            
            WriteToFile(path, "default.bui", p => meteoWriter.Export(model.Precipitation, p));
            WriteToFile(path, "default.tmp", p => meteoWriter.Export(model.Temperature, p));
            WriteToFile(path, "basinGeometry.shp", p =>  serializer.WriteCatchmentGeometry(model.Basin, p));
            
            return true;
        }

        private static void WriteToFile(string path, string fileName, Action<string> writeAction)
        {
            var filePath = Path.Combine(Path.GetFullPath(path), fileName);
            FileUtils.DeleteIfExists(filePath);
            writeAction(filePath);
        }
    }
}