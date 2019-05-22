using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public interface IFeature2DFileBase<TFeat>
    {
        void Write(string path, IEnumerable<TFeat> features);

        IList<TFeat> Read(string path);
    }
}