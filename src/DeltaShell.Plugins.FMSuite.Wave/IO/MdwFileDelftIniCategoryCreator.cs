using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public static class MdwFileDelftIniCategoryCreator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MdwFileDelftIniCategoryCreator));
        
        public static DelftIniCategory CreateBoundaryConditionCategory(WaveBoundaryCondition boundaryCondition)
        {
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.AddProperty(KnownWaveProperties.Name, boundaryCondition.FeatureName);
            boundaryCategory.AddProperty(KnownWaveProperties.Definition, "xy-coordinates");
            boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateX, boundaryCondition.StartCoordinate.X);
            boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateX, boundaryCondition.EndCoordinate.X);
            boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateY, boundaryCondition.StartCoordinate.Y);
            boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateY, boundaryCondition.EndCoordinate.Y);

            // write spectral data:
            if (boundaryCondition.DataType != BoundaryConditionDataType.SpectrumFromFile)
            {
                boundaryCategory.AddProperty(KnownWaveProperties.SpectrumSpec, "parametric");
                boundaryCategory.AddProperty(KnownWaveProperties.ShapeType,
                                             boundaryCondition.ShapeType.GetDescription().ToLower());
                boundaryCategory.AddProperty(KnownWaveProperties.PeriodType,
                                             boundaryCondition.PeriodType.GetDescription().ToLower());
                boundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingType,
                                             boundaryCondition.DirectionalSpreadingType.GetDescription().ToLower());
                boundaryCategory.AddProperty(KnownWaveProperties.PeakEnhancementFactor, boundaryCondition.PeakEnhancementFactor);
                boundaryCategory.AddProperty(KnownWaveProperties.GaussianSpreading, boundaryCondition.GaussianSpreadingValue);
            }
            else
            {
                boundaryCategory.AddProperty(KnownWaveProperties.SpectrumSpec, "from file");
            }

            if (boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform)
            {
                switch (boundaryCondition.DataType)
                {
                    case BoundaryConditionDataType.SpectrumFromFile:
                        boundaryCategory.AddProperty(KnownWaveProperties.Spectrum, boundaryCondition.SpectrumFiles[0]);
                        break;
                    case BoundaryConditionDataType.ParameterizedSpectrumConstant:
                    {
                        WaveBoundaryParameters parameters = boundaryCondition.SpectrumParameters[0];
                        boundaryCategory.AddProperty(KnownWaveProperties.WaveHeight, parameters.Height);
                        boundaryCategory.AddProperty(KnownWaveProperties.Period, parameters.Period);
                        boundaryCategory.AddProperty(KnownWaveProperties.Direction, parameters.Direction);
                        boundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                                     parameters.Spreading);
                        break;
                    }
                }
            }
            else if (boundaryCondition.DataPointIndices.Count == 0) // spatially varying, no data points
            {
                log.WarnFormat(
                    "No data points found for boundary \'{0}\', saved boundary will default to Uniform spatial definition type",
                    boundaryCondition.FeatureName);
                const double defaultValue = 0.0;
                boundaryCategory.AddProperty(KnownWaveProperties.WaveHeight, defaultValue);
                boundaryCategory.AddProperty(KnownWaveProperties.Period, defaultValue);
                boundaryCategory.AddProperty(KnownWaveProperties.Direction, defaultValue);
                boundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, defaultValue);
            }
            else // spatially varying, with data points
            {
                // order the DataPointIndices or it will give an error in the computational core.
                foreach (int dataPointIdx in boundaryCondition.DataPointIndices.OrderBy(di => di))
                {
                    boundaryCategory.AddProperty(KnownWaveProperties.CondSpecAtDist,
                                                 boundaryCondition.GetDistanceFromFirstDataPointOverWaveBoundary(dataPointIdx));

                    if (boundaryCondition.DataType == BoundaryConditionDataType.SpectrumFromFile)
                    {
                        boundaryCategory.AddProperty(KnownWaveProperties.Spectrum, boundaryCondition.SpectrumFiles[dataPointIdx]);
                        continue;
                    }

                    if (boundaryCondition.DataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
                    {
                        WaveBoundaryParameters parameters = boundaryCondition.SpectrumParameters[dataPointIdx];
                        boundaryCategory.AddProperty(KnownWaveProperties.WaveHeight, parameters.Height);
                        boundaryCategory.AddProperty(KnownWaveProperties.Period, parameters.Period);
                        boundaryCategory.AddProperty(KnownWaveProperties.Direction, parameters.Direction);
                        boundaryCategory.AddProperty(KnownWaveProperties.DirectionalSpreadingValue,
                                                     parameters.Spreading);
                    }
                }
            }

            return boundaryCategory;
        }
    }
}