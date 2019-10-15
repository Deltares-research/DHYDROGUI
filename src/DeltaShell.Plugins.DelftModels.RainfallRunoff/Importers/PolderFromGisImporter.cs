using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class PolderFromGisImporter : IFileImporter
    {
        public const string NoneAttribute = "<None>";
        private static readonly ILog Log = LogManager.GetLogger(typeof (PolderFromGisImporter));
        private CatchmentFromGisImporter catchmentImporter;

        public PolderFromGisImporter()
        {           
            LandUseMappingConfiguration = new LandUseMapping();
            catchmentImporter = new CatchmentFromGisImporter
                {
                    FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>
                        {
                            new ShapeFile(),
                            new OgrFeatureProvider()
                        }
                };
        }

        public CatchmentFromGisImporter CatchmentImporter
        {
            get { return catchmentImporter; }
            set { catchmentImporter = value; }
        }
        
        public bool OpenViewAfterImport { get { return false; } }

        public LandUseMapping LandUseMappingConfiguration { get; set; }

        public bool UseAttributeMapping { get; set; }

        public IDictionary<PolderSubTypes, string> AttributeMapping { get; set; }

        public RainfallRunoffEnums.AreaUnit AttributeUnit { get; set; }

        # region ITargetItemFileImporter Members

        public string Name
        {
            get { return "Polder Concepts Catchments (GIS)"; }
        }

        public string Category { get; private set; }
        public string Description { get{ return Name; } }

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(RainfallRunoffModel); }
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
            get { return ""; }
        }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var model = target as RainfallRunoffModel;
            if (model != null)
            {
                //Import catchments on network:
                catchmentImporter.ImportItem(null, model.Basin);

                //do land use stuff:
                ConvertLandUseToPolder(model);
            }

            return target;
        }

        #endregion

        private void ConvertLandUseToPolder(RainfallRunoffModel model)
        {
            var landUseFeatures = LandUseMappingConfiguration.Use
                                                       ? LandUseMappingConfiguration.LandUseFeatureProvider.Features.Cast<IFeature>().ToList()
                                                       : new List<IFeature>();

            var catchments = model.Basin.Catchments;

            int errorCount = 0;

            foreach (var catchment in catchments)
            {
                if (LandUseMappingConfiguration.Use)
                {
                    DeterminePolderSubTypesFromLandUse(model, catchment, landUseFeatures);
                }
                else if (UseAttributeMapping)
                {
                    IFeature gisFeature = catchmentImporter.CatchmentToGisFeatureMapping[catchment];
                    foreach (PolderSubTypes type in AttributeMapping.Keys)
                    {
                        double areaSize = 0.0;
                        string columnName = AttributeMapping[type];

                        if (columnName == NoneAttribute)
                            continue;

                        try
                        {
                            areaSize = Math.Max(0, Convert.ToDouble(gisFeature.Attributes[columnName]));
                        }
                        catch (Exception)
                        {
                            if (errorCount == 0)
                            {
                                Log.ErrorFormat(
                                    "Unable to parse area value from column {0} for RR area {1}: not a valid number",
                                    columnName, catchment.Name);
                            }

                            errorCount++;
                        }

                        double areaInM2 = RainfallRunoffUnitConverter.ConvertArea(AttributeUnit,
                                                                                  RainfallRunoffEnums.AreaUnit.m2,
                                                                                  areaSize);

                        SetAreaToPolder(model, catchment, type, areaInM2);
                    }
                }
            }

            if (errorCount > 1)
            {
                Log.ErrorFormat("An additional {0} similar parse errors occurred. Please verify your mapping",
                                errorCount - 1);
            }
        }

        private void DeterminePolderSubTypesFromLandUse(RainfallRunoffModel model, Catchment catchment,
                                                        IEnumerable<IFeature> landUseFeatures)
        {
            IGeometry geometry = catchment.Geometry;
            
            double totalArea = 0.0;

            var areas = new Dictionary<PolderSubTypes, double>();

            foreach (var landUseFeature in landUseFeatures)
            {
                object landUseType = landUseFeature.Attributes[LandUseMappingConfiguration.Column];
                PolderSubTypes polderType = LandUseMappingConfiguration.Mapping[landUseType];

                double overlappingArea = GeometryHelper.GetIntersectionArea(landUseFeature.Geometry, geometry);
                totalArea += overlappingArea;

                if (!areas.ContainsKey(polderType))
                {
                    areas.Add(polderType, 0.0);
                }
                areas[polderType] += overlappingArea;
            }

            double ratio = catchment.AreaSize/totalArea;
            foreach (PolderSubTypes key in areas.Keys.ToList())
            {
                areas[key] *= ratio;
            }

            foreach (PolderSubTypes subType in areas.Keys)
            {
                SetAreaToPolder(model, catchment, subType, areas[subType]);
            }
        }

        private static void SetAreaToPolder(RainfallRunoffModel model, Catchment catchment, PolderSubTypes type, double area)
        {
            if (area < 0.01)
                return; //margin for misalignment
            
            if (!catchment.CatchmentType.Equals(CatchmentType.Polder))
            {
                catchment.CatchmentType = CatchmentType.Polder;  //no polder type yet: make polder
            }

            switch (type)
            {
                case PolderSubTypes.None:
                    break;
                case PolderSubTypes.Grass:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Grass, area);
                    break;
                case PolderSubTypes.Corn:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Corn, area);
                    break;
                case PolderSubTypes.Potatoes:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Potatoes, area);
                    break;
                case PolderSubTypes.Sugarbeet:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Sugarbeet, area);
                    break;
                case PolderSubTypes.Grain:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Grain, area);
                    break;
                case PolderSubTypes.Miscellaneous:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Miscellaneous, area);
                    break;
                case PolderSubTypes.NonArableLand:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.NonArableLand, area);
                    break;
                case PolderSubTypes.GreenhouseArea:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.GreenhouseArea, area);
                    break;
                case PolderSubTypes.Orchard:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Orchard, area);
                    break;
                case PolderSubTypes.BulbousPlants:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.BulbousPlants, area);
                    break;
                case PolderSubTypes.FoliageForest:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.FoliageForest, area);
                    break;
                case PolderSubTypes.PineForest:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.PineForest, area);
                    break;
                case PolderSubTypes.Nature:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Nature, area);
                    break;
                case PolderSubTypes.Fallow:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Fallow, area);
                    break;
                case PolderSubTypes.Vegetables:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Vegetables, area);
                    break;
                case PolderSubTypes.Flowers:
                    SetUnpavedArea(model, catchment, UnpavedEnums.CropType.Flowers, area);
                    break;
                case PolderSubTypes.Paved:
                    SetPavedArea(model, catchment, area);
                    break;
                case PolderSubTypes.lessThan500:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.lessThan500, area);
                    break;
                case PolderSubTypes.from500to1000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from500to1000, area);
                    break;
                case PolderSubTypes.from1000to1500:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from1000to1500, area);
                    break;
                case PolderSubTypes.from1500to2000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from1500to2000, area);
                    break;
                case PolderSubTypes.from2000to2500:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from2000to2500, area);
                    break;
                case PolderSubTypes.from2500to3000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from2500to3000, area);
                    break;
                case PolderSubTypes.from3000to4000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from3000to4000, area);
                    break;
                case PolderSubTypes.from4000to5000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from4000to5000, area);
                    break;
                case PolderSubTypes.from5000to6000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.from5000to6000, area);
                    break;
                case PolderSubTypes.moreThan6000:
                    SetGreenhouseArea(model, catchment, GreenhouseEnums.AreaPerGreenhouseType.moreThan6000, area);
                    break;
                case PolderSubTypes.OpenWater:
                    SetOpenWaterArea(model, catchment, area);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private static void SetPavedArea(RainfallRunoffModel model, Catchment catchment, double area)
        {
            if (area == 0.0)
                return;

            var pavedCatchment = GetOrCreateSubTypeCatchment(catchment, CatchmentType.Paved);
            var pavedData = (PavedData)model.GetCatchmentModelData(pavedCatchment);
            pavedData.CalculationArea = area;
        }

        private static void SetOpenWaterArea(RainfallRunoffModel model, Catchment catchment, double area)
        {
            if (area == 0.0)
                return;

            var openwaterCatchment = GetOrCreateSubTypeCatchment(catchment, CatchmentType.OpenWater);
            var openWaterData = (OpenWaterData)model.GetCatchmentModelData(openwaterCatchment);
            openWaterData.CalculationArea = area;
        }

        private static void SetUnpavedArea(RainfallRunoffModel model, Catchment catchment, UnpavedEnums.CropType cropType, double area)
        {
            if (area == 0.0)
                return;

            var unpavedCatchment = GetOrCreateSubTypeCatchment(catchment, CatchmentType.Unpaved);
            var unpavedData = (UnpavedData)model.GetCatchmentModelData(unpavedCatchment);
            unpavedData.AreaPerCrop[cropType] = area;
        }

        private static void SetGreenhouseArea(RainfallRunoffModel model, Catchment catchment, 
                                              GreenhouseEnums.AreaPerGreenhouseType areaPerGreenhouseType, double area)
        {
            if (area == 0.0)
                return;

            var greenhouseCatchment = GetOrCreateSubTypeCatchment(catchment, CatchmentType.GreenHouse);
            var greenhouseData = (GreenhouseData)model.GetCatchmentModelData(greenhouseCatchment);
            greenhouseData.AreaPerGreenhouse[areaPerGreenhouseType] = area;
        }

        private static Catchment GetOrCreateSubTypeCatchment(Catchment catchment, CatchmentType catchmentType)
        {
            return catchment.SubCatchments.FirstOrDefault(sc => Equals(sc.CatchmentType, catchmentType)) ??
                   catchment.AddSubCatchment(catchmentType);
        }
    }
}