using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public interface IFeature2DFileBase<TFeat>
    {
        void Write(string filePath, IEnumerable<TFeat> features);

        IList<TFeat> Read(string filePath);
    }
}