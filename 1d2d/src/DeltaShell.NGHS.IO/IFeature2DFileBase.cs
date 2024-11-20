using System.Collections.Generic;

namespace DeltaShell.NGHS.IO
{
    public interface IFeature2DFileBase<TFeat>
    {
        void Write(string path, IEnumerable<TFeat> features);

        IList<TFeat> Read(string path);
    }
}
