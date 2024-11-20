using System;
using System.Runtime.Serialization;

namespace DelftTools.Hydro.CrossSections.DataSets
{
    [Serializable]
    public class FastZWDataTable : CrossSectionDataSet.CrossSectionZWDataTable
    {
        public FastZWDataTable()
        {
        }

        protected FastZWDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}