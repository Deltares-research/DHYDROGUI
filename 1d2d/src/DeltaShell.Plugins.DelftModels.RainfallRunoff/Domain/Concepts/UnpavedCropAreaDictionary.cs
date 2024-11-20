using System;
using System.Runtime.Serialization;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Serializable]
    public class CropAreaDictionary : AreaDictionary<UnpavedEnums.CropType>
    {
        protected CropAreaDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CropAreaDictionary()
        {
        }
    }
}