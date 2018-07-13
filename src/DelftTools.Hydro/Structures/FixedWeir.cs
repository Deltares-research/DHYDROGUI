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
    public class FixedWeir : GroupableFeature2D
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
    }
}