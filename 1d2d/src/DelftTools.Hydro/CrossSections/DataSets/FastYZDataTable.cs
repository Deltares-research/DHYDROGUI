using System;
using System.Runtime.Serialization;

namespace DelftTools.Hydro.CrossSections.DataSets
{
    [Serializable]
    public class FastYZDataTable : CrossSectionDataSet.CrossSectionYZDataTable
    {
        public FastYZDataTable()
        {
        }

        protected FastYZDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
