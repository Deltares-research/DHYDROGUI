using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange = false)]
    public abstract class RtcBaseObject : Unique<long>, INameable, ICloneable, ICopyFrom
    {
        public string LongName { get; set; }
        public string Name { get; set; }

        public abstract object Clone();

        public virtual void CopyFrom(object source)
        {
            var rtcBaseObject = source as RtcBaseObject;
            if (rtcBaseObject != null)
            {
                Name = rtcBaseObject.Name;
                LongName = rtcBaseObject.LongName;
            }
        }
    }
}