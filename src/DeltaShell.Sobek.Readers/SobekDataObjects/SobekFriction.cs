using System.Collections.Generic;
using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekExtraResistance
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DataTable Table { get; set; }

        public SobekExtraResistance()
        {
            Table = ExtraResistanceStructure;
        }

        private static DataTable ExtraResistanceStructure
        {
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("water level difference", typeof(double));
                dataTable.Columns.Add("extra resistance", typeof(double));
                return dataTable;
            }
        }
    }

    public class SobekFriction
    {
        public SobekFriction()
        {
            GlobalBedFrictionList = new List<SobekBedFriction>();
            CrossSectionFrictionList = new List<SobekCrossSectionFriction>();
            SobekBedFrictionList = new List<SobekBedFriction>();
            StructureFrictionList = new List<SobekStructureFriction>();
            SobekExtraFrictionList = new List<SobekExtraResistance>();
        }

        /// <summary>
        /// A list with the data of the CRFR records from the friction.dat file
        /// CRFR records contain the friction values for a cross section
        /// </summary>
        public IList<SobekCrossSectionFriction> CrossSectionFrictionList { get; set; }

        public IList<SobekBedFriction> SobekBedFrictionList { get; set; }

        public IList<SobekStructureFriction> StructureFrictionList { get; set; }

        public IList<SobekExtraResistance> SobekExtraFrictionList { get; set; }

        public IList<SobekBedFriction> GlobalBedFrictionList { get; set; }
    }
}