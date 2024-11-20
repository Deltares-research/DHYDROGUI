using System;
using System.Runtime.Serialization;

namespace DelftTools.Hydro.CrossSections.DataSets
{
    [Serializable]
    public class FastXYZDataTable : CrossSectionDataSet.CrossSectionXYZDataTable
    {
        public FastXYZDataTable()
        {
        }

        protected FastXYZDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}