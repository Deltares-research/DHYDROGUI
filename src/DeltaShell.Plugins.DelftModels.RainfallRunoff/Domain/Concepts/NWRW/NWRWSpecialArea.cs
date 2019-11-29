using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    public class NWRWSpecialArea
    {
        public virtual int SpecialAreaId { get; set; }
        public virtual int Area { get; set; }
        public virtual string SpecialInflowReference { get; set; }
    }
}
