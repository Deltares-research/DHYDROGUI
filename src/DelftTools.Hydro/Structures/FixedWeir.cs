using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;


namespace DelftTools.Hydro.Structures
{
    public enum FixedWeirSchemes
    {
        [Description("No scheme selected")]
        None = 0,
        [Description("Fixed Weir Scheme 6")]
        Scheme6 = 6,
        [Description("Fixed Weir Scheme 8")]
        Scheme8 = 8,
        [Description("Fixed Weir Scheme 9")]
        Scheme9 = 9,
    }


    public class FixedWeir : GroupableFeature2D, IModelDataColumnsFeature
    {
        public const double DefaultCrestLevel = 0.0;
        public IList<double> CrestLevels { get; private set; }

        public const double DefaultGroundLevel = 0.0;
        public IList<double> GroundLevelsLeft { get; private set; }
        public IList<double> GroundLevelsRight { get; private set; }

        public FixedWeir()
        {
            CrestLevels = new GeometryPointsSyncedList<double>
            {
                CreationMethod = (f, i) => DefaultCrestLevel,
                RecreateAllItems = false,
                Feature = this
            };
            GroundLevelsLeft = new GeometryPointsSyncedList<double>
            {
                CreationMethod = (f, i) => DefaultGroundLevel,
                RecreateAllItems = false,
                Feature = this
            };

            GroundLevelsRight = new GeometryPointsSyncedList<double>
            {
                CreationMethod = (f, i) => DefaultGroundLevel,
                RecreateAllItems = false,
                Feature = this
            };
            SetupAttributeToPropertyLinks();
        }

        public void SetupAttributeToPropertyLinks()
        {
            Attributes = new DictionaryFeatureAttributeCollection
            {
                {"Column3", CrestLevels},
                {"Column4", GroundLevelsLeft},
                {"Column5", GroundLevelsRight}
            };
        }

        public void InitializeAttributes() //should be called when events are not bubbling, but geometry is set (e.g. loading)
        {
            ((GeometryPointsSyncedList<double>)CrestLevels).InitializeItems();
            ((GeometryPointsSyncedList<double>)GroundLevelsLeft).InitializeItems();
            ((GeometryPointsSyncedList<double>)GroundLevelsRight).InitializeItems();
        }

        public override object Clone()
        {
            var instance = (FixedWeir) base.Clone();
            instance.CrestLevels = CrestLevels;
            instance.GroundLevelsLeft = GroundLevelsLeft;
            instance.GroundLevelsRight = GroundLevelsRight;
            return instance;
        }

        public void UpdateDataColumns(ModelFeatureCoordinateData modelDataForFeatureWithDataColumns)
        {
            var modelDataOfFixedWeir = modelDataForFeatureWithDataColumns as IModelFeatureCoordinateData;
            if (modelDataOfFixedWeir != null)
            {
                FixedWeirSchemes scheme;
                if (!Enum.TryParse(modelDataOfFixedWeir.Selector.ToString(), true, out scheme))
                    return; //or throw exception?
                SetActiveFields(modelDataOfFixedWeir.DataColumns, scheme);
            }

        }
        public IEventedList<IDataColumn> GenerateDataColumns(ModelFeatureCoordinateData modelDataForFeatureWithDataColumns)
        {
            var modelDataOfFixedWeir = modelDataForFeatureWithDataColumns as IModelFeatureCoordinateData;
            if (modelDataOfFixedWeir == null) return null;
            FixedWeirSchemes scheme = FixedWeirSchemes.None;
            if (modelDataOfFixedWeir.Selector != null && !Enum.TryParse(modelDataOfFixedWeir.Selector.ToString(), true, out scheme))
                return null; //or throw exception?

            switch (scheme)
            {
                case FixedWeirSchemes.None:
                case FixedWeirSchemes.Scheme6:
                case FixedWeirSchemes.Scheme8:
                    return DataColumnsForScheme6And8And0();
                case FixedWeirSchemes.Scheme9:
                    return DataColumnsForScheme9();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEventedList<IDataColumn> DataColumnsForScheme6And8And0()
        {
            return new EventedList<IDataColumn>()
            {
                new DataColumn<double>("Crest Levels"),
                new DataColumn<double>("Ground Levels Left"),
                new DataColumn<double>("Ground Levels Right"),
            };
        }

        private IEventedList<IDataColumn> DataColumnsForScheme9()
        {
            var list = DataColumnsForScheme6And8And0();
            list.AddRange(DataColumnsScheme9());
            return list;
        }

        private static EventedList<IDataColumn> DataColumnsScheme9()
        {
            return new EventedList<IDataColumn>()
            {
                new DataColumn<double>("Crest Length"){DefaultValue = 3.0},
                new DataColumn<double>("Talud Up"){DefaultValue = 4.0},
                new DataColumn<double>("Talud Down"){DefaultValue = 4.0},
                new DataColumn<double>("Vegetation Coefficient"),
            };
        }

        private void SetActiveFields(IEventedList<IDataColumn> dataColumns, FixedWeirSchemes scheme)
        {
                        switch (scheme)
            {
                case FixedWeirSchemes.None:
                case FixedWeirSchemes.Scheme6:
                case FixedWeirSchemes.Scheme8:
                    dataColumns.Where(dc => dc.Name == "Crest Levels").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Ground Levels Left").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Ground Levels Right").ForEach(dc => dc.IsActive = true);
                    
                    
                    if (dataColumns.Count > 3)
                    {
                        dataColumns.Where(dc => dc.Name == "Crest Length").ForEach(dc => dc.IsActive = false);
                        dataColumns.Where(dc => dc.Name == "Talud Up").ForEach(dc => dc.IsActive = false);
                        dataColumns.Where(dc => dc.Name == "Talud Down").ForEach(dc => dc.IsActive = false);
                        dataColumns.Where(dc => dc.Name == "Vegetation Coefficient").ForEach(dc => dc.IsActive = false);
                    }
                    break;
                case FixedWeirSchemes.Scheme9:
                    if(dataColumns.Count < 7)
                        dataColumns.AddRange(DataColumnsScheme9());
                    dataColumns.Where(dc => dc.Name == "Crest Levels").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Ground Levels Left").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Ground Levels Right").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Crest Length").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Talud Up").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Talud Down").ForEach(dc => dc.IsActive = true);
                    dataColumns.Where(dc => dc.Name == "Vegetation Coefficient").ForEach(dc => dc.IsActive = true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}