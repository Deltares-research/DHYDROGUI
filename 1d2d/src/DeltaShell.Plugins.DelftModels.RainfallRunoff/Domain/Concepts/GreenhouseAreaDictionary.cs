using System;
using System.Runtime.Serialization;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Serializable]
    public class GreenhouseAreaDictionary : AreaDictionary<GreenhouseEnums.AreaPerGreenhouseType>
    {
        protected GreenhouseAreaDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public GreenhouseAreaDictionary()
        {
        }
    }
}