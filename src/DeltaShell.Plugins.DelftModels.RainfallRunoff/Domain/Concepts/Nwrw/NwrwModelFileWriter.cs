using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwModelFileWriter
    {
        private readonly IEnumerable<INwrwComponentFileWriterBase> fileWriters;

        public NwrwModelFileWriter(IEnumerable<INwrwComponentFileWriterBase> fileWriters)
        {
            this.fileWriters = fileWriters ?? throw new ArgumentNullException(nameof(fileWriters));
        }

        public void WriteNwrwFiles(string path)
        {
            foreach (var componentFileWriter in fileWriters)
            {
                componentFileWriter.Write(path);
            }
        }
    }
}
