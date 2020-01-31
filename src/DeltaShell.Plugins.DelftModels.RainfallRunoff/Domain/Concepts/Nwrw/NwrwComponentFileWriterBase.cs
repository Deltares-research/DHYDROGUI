using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public abstract class NwrwComponentFileWriterBase : NGHSFileBase
    {
        private readonly string fileName;

        protected readonly IEnumerable<NwrwSurfaceType> SurfaceTypesInCorrectOrder = new[]
        {
            NwrwSurfaceType.ClosedPavedWithSlope, // a1
            NwrwSurfaceType.ClosedPavedFlat, // a2
            NwrwSurfaceType.ClosedPavedFlatStretch, // a3
            NwrwSurfaceType.OpenPavedWithSlope, // a4
            NwrwSurfaceType.OpenPavedFlat, // a5
            NwrwSurfaceType.OpenPavedFlatStretched, // a6
            NwrwSurfaceType.RoofWithSlope, // a7
            NwrwSurfaceType.RoofFlat, // a8
            NwrwSurfaceType.RoofFlatStretched, // a9
            NwrwSurfaceType.UnpavedWithSlope, // a10
            NwrwSurfaceType.UnpavedFlat, // a11
            NwrwSurfaceType.UnpavedFlatStretched // a12
        };

        private RainfallRunoffModel model;

        protected NwrwComponentFileWriterBase(RainfallRunoffModel model, string fileName)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

            this.model = model;
            this.fileName = fileName;
        }

        public bool Write(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null, empty or consist of whitespaces.");
            }

            var filePath = Path.Combine(Path.GetFullPath(path), fileName);

            try
            {
                OpenOutputFile(filePath);
                foreach (var contentLine in CreateContentLine(model))
                {
                    WriteLine(contentLine);
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                CloseOutputFile();
            }

            return true;
        }

        protected abstract IEnumerable<string> CreateContentLine(RainfallRunoffModel model);

    }
}