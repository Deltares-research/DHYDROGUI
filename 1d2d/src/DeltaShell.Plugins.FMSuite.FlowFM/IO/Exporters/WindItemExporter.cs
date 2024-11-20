using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    class WindItemExporter: IFileExporter
    {
        public static IEnumerable<WindItemExporter> CreateExporters()
        {
            yield return
                new WindItemExporter
                {
                    Category = "Wind time series",
                    SupportedTypes = new[] {typeof (UniformWindField)},
                    SupportedQuantities =
                        new[]
                        {
                            WindQuantity.VelocityX, WindQuantity.VelocityY, WindQuantity.AirPressure,
                            WindQuantity.VelocityVector, WindQuantity.VelocityVectorAirPressure
                        },
                    FileFilter = "time series file|*.tim",
                    Icon = Properties.Resources.TimeSeries
                };
            yield return
                new WindItemExporter
                {
                    Category = "Wind gridded data",
                    SupportedTypes = new[] { typeof(GriddedWindField) },
                    SupportedQuantities =
                        new[]
                        {
                            WindQuantity.VelocityX
                        },
                    FileFilter = "regular grid file|*.amu",
                    Icon = Properties.Resources.FunctionGrid2D
                };
            yield return
                new WindItemExporter
                {
                    Category = "Wind gridded data",
                    SupportedTypes = new[] { typeof(GriddedWindField) },
                    SupportedQuantities =
                        new[]
                        {
                            WindQuantity.VelocityY
                        },
                    FileFilter = "regular grid file|*.amv",
                    Icon = Properties.Resources.FunctionGrid2D
                };
            yield return
                new WindItemExporter
                {
                    Category = "Wind gridded data",
                    SupportedTypes = new[] {typeof (GriddedWindField)},
                    SupportedQuantities =
                        new[]
                        {
                            WindQuantity.AirPressure
                        },
                    FileFilter = "regular grid file|*.amp",
                    Icon = Properties.Resources.FunctionGrid2D
                };
            yield return
                new WindItemExporter
                {
                    Category = "Wind gridded data",
                    SupportedTypes = new[] {typeof (GriddedWindField)},
                    SupportedQuantities =
                        new[]
                        {
                            WindQuantity.VelocityVectorAirPressure
                        },
                    FileFilter = "curvi-grid file|*.apwxwy",
                    Icon = Properties.Resources.FunctionGrid2D
                };
            yield return
                new WindItemExporter
                {
                    Category = "Wind gridded data",
                    SupportedTypes = new[] { typeof(SpiderWebWindField) },
                    SupportedQuantities =
                        new[]
                        {
                            WindQuantity.VelocityVectorAirPressure
                        },
                    FileFilter = "spider web file|*.spw",
                    Icon = Properties.Resources.hurricane2
                };
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (WindItemExporter));

        public string Name
        {
            get { return "wind data exporter"; }
        }
        public string Description { get { return Name; } }
        private IList<Type> SupportedTypes { get; set; }

        private IList<WindQuantity> SupportedQuantities { get; set; }

        public Func<IWindField, DateTime> ReferenceDateGetter { private get; set; }

        public bool Export(object item, string path)
        {
            var uniformWindField = item as UniformWindField;
            if (uniformWindField != null)
            {
                var fileWriter = new TimFile();
                DateTime? referenceDate = null;
                if (ReferenceDateGetter != null)
                {
                    referenceDate = ReferenceDateGetter(uniformWindField);
                }
                fileWriter.Write(path, ((IWindField) item).Data, referenceDate);
            }
            var griddedWindField = item as GriddedWindField;
            if (griddedWindField != null)
            {
                if (griddedWindField.SeparateGridFile)
                {
                    var gridFilePath = Path.GetFullPath(griddedWindField.GridFilePath);
                    if (File.Exists(gridFilePath))
                    {
                        var newGridFilePath = GriddedWindField.GetCorrespondingGridFilePath(path);
                        if (newGridFilePath != gridFilePath)
                        {
                            File.Copy(gridFilePath, newGridFilePath, true);
                        }
                    }
                    else
                    {
                        Log.ErrorFormat("File not found : {0}, skipping export", griddedWindField.GridFilePath);
                        return false;
                    }
                }
                if (File.Exists(griddedWindField.WindFilePath))
                {
                    if (Path.GetFullPath(path) != Path.GetFullPath(griddedWindField.WindFilePath))
                    {
                        File.Copy(griddedWindField.WindFilePath, path, true);
                    }
                }
                else
                {
                    Log.ErrorFormat("File not found : {0}, skipping export", griddedWindField.WindFilePath);
                    return false;
                }
            }
            var spiderWebItem = item as SpiderWebWindField;
            if (spiderWebItem != null)
            {
                if (File.Exists(spiderWebItem.WindFilePath))
                {
                    if (Path.GetFullPath(path) != Path.GetFullPath(spiderWebItem.WindFilePath))
                    {
                        File.Copy(spiderWebItem.WindFilePath, path, true);
                    }
                }
                else
                {
                    Log.ErrorFormat("File not found : {0}, skipping export", spiderWebItem.WindFilePath);
                    return false;
                }
            }
            return true;
        }

        public string Category { get; private set; }

        public IEnumerable<Type> SourceTypes()
        {
            return SupportedTypes;
        }

        public string FileFilter { get; private set; }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }

        public bool CanExportFor(object item)
        {
            return SupportedTypes.Contains(item.GetType()) && SupportedQuantities.Contains(((IWindField) item).Quantity);
        }
    }
}
