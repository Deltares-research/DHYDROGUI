using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class StructureFile
    {
        private static readonly Dictionary<string, Action<string, WaterFlowFMModel, IStructure2D>> StructureWriteActions = new Dictionary<string, Action<string, WaterFlowFMModel, IStructure2D>>
        {
            {StructureRegion.PolylineFile.Key, WritePolylineFile},
            {StructureRegion.Capacity.Key, WriteTimeSeriesFile},
            {StructureRegion.CrestLevel.Key, WriteTimeSeriesFile },
            {StructureRegion.GateSillLevel.Key, WriteTimeSeriesFile },
            {StructureRegion.GateLowerEdgeLevel.Key, WriteTimeSeriesFile },
            {StructureRegion.GateOpeningWidth.Key, WriteTimeSeriesFile },
            {StructureRegion.TimeFilePath.Key, WriteTimeSeriesFile }
        };

        public static IEnumerable<DelftIniCategory> Generate2DStructureCategoriesFromFMModel(IModel model)
        {
            var fmModel = model as WaterFlowFMModel;
            if (fmModel?.Network == null) return Enumerable.Empty<DelftIniCategory>();

            var categories1D = NetworkEditor.IO.StructureFile.ExtractFunctionStructuresOfNetworkGenerator(fmModel.Network);
            var categories2D = ExtractFunctionStructuresOfAreaGenerator(fmModel.Area).ToList();

            Action<string, WaterFlowFMModel, IStructure2D> myAction;
            foreach (var category2D in categories2D)
            {
                var structureName = category2D.GetPropertyValue(StructureRegion.Id.Key);
                if (string.IsNullOrEmpty(structureName)) continue;

                foreach (var propertyName in category2D.Properties.Select(property => property.Name))
                {
                    if (StructureWriteActions.TryGetValue(propertyName, out myAction))
                    {
                        var fileNameProperty = category2D.Properties.FirstOrDefault(p => p.Name == propertyName);
                        var structure2D = fmModel.Area.AllHydroObjects.Cast<IStructure2D>().FirstOrDefault(o => o.Name == structureName);
                        if (fileNameProperty != null) myAction(fileNameProperty.Value, fmModel, structure2D);
                    }
                }
            }

            return categories1D.Concat(categories2D);
        }

        private static IEnumerable<DelftIniCategory> ExtractFunctionStructuresOfAreaGenerator(HydroArea area)
        {
            foreach (var structure2D in area.AllHydroObjects.Cast<IStructure2D>())
            {
                var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structure2D.Structure2DType);
                var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure2D);
                yield return structureCategory;
            }
        }

        private static void WritePolylineFile(string fileName, WaterFlowFMModel fmModel, IStructure2D structure2D)
        {
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(fmModel.MduFilePath, fileName);
            var geometryObjectsToBeWritten = new[]
            {
                new Feature2D
                {
                    Name = structure2D.Name,
                    Geometry = structure2D.Geometry
                }
            };
            new PliFile<Feature2D>().Write(pliFilePath, geometryObjectsToBeWritten);
        }

        private static void WriteTimeSeriesFile(string fileName, WaterFlowFMModel fmModel, IStructure2D structure2D)
        {
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(fmModel.MduFilePath, fileName);
            if (structure2D is IPump)
            {
                WritePumpTimeSeriesFile(timFilePath, fmModel.ReferenceTime, structure2D as IPump);
            }
            else if (structure2D is IWeir)
            {
                WriteWeirTimeSeriesFile(timFilePath, fmModel.ReferenceTime, structure2D as IWeir);
            }
            else if (structure2D is IGate)
            {
                WriteGateTimeSeriesFiles(timFilePath, fmModel.ReferenceTime, structure2D as IGate);
            }
            else if (structure2D is LeveeBreach)
            {
                WriteLeveeBreachTimeSeriesFile(timFilePath, structure2D as LeveeBreach);
            }
        }

        private static void WritePumpTimeSeriesFile(string timFilePath, DateTime referenceTime, IPump pump)
        {
            if (pump != null && pump.HasCapacityTimeSeries())
            {
                new TimFile().Write(timFilePath, pump.CapacityTimeSeries, referenceTime);
            }
        }

        private static void WriteWeirTimeSeriesFile(string timFilePath, DateTime referenceTime, IWeir weir)
        {
            if (weir != null && weir.HasCrestLevelTimeSeries())
            {
                new TimFile().Write(timFilePath, weir.CrestLevelTimeSeries, referenceTime);
            }
        }

        private static void WriteGateTimeSeriesFiles(string timFilePath, DateTime referenceTime, IGate gate)
        {
            if (gate == null) return;

            if (gate.UseSillLevelTimeSeries)
            {
                new TimFile().Write(timFilePath, gate.SillLevelTimeSeries, referenceTime);
            }
            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                new TimFile().Write(timFilePath, gate.LowerEdgeLevelTimeSeries, referenceTime);
            }
            if (gate.UseOpeningWidthTimeSeries)
            {
                new TimFile().Write(timFilePath, gate.OpeningWidthTimeSeries, referenceTime);
            }
        }

        private static void WriteLeveeBreachTimeSeriesFile(string timFilePath, LeveeBreach leveeBreach)
        {
            var leveeBreachSettings = leveeBreach?.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
            if (leveeBreach != null && leveeBreachSettings != null)
            {
                var timeSeries = leveeBreachSettings.CreateTimeSeriesFromTable();
                var commentLines = new List<string> { "Time entries are defined in minutes, relative to the breach growth start" };
                new TimFile().Write(timFilePath, timeSeries, leveeBreachSettings.StartTimeBreachGrowth, commentLines);
            }
        }

        private static bool HasCapacityTimeSeries(this IPump pump)
        {
            return pump.CanBeTimedependent
                   && pump.UseCapacityTimeSeries
                   && pump.CapacityTimeSeries != null;
        }

        private static bool HasCrestLevelTimeSeries(this IWeir weir)
        {
            return weir.CanBeTimedependent
                   && weir.UseCrestLevelTimeSeries
                   && weir.CrestLevelTimeSeries != null;
        }
    }
}
