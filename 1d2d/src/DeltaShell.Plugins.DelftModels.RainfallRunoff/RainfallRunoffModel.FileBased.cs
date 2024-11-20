using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// using IFileBased to get relative paths.
    /// using the events in the application plugin to save and load using the ex- and importers
    /// </summary>
    public partial class RainfallRunoffModel : IFileBased
    {
        public virtual void Delete() { }

        public virtual void CreateNew(string path)
        {
            Path = GetRRSavePathFromDeltaShellPath(path); 
        }
        
        public virtual void Close() { }
        
        public virtual void Open(string path) { }

        public virtual void CopyTo(string destinationPath)
        {
            Path = GetRRSavePathFromDeltaShellPath(destinationPath);
        }

        private string GetRRSavePathFromDeltaShellPath(string path)
        {
            var directoryName = path != null
                ? System.IO.Path.GetDirectoryName(path) ?? ""
                : "";

            // dsproj_data/<model name>/Sobek_3b.fnm
            return path != null && path.EndsWith(System.IO.Path.Combine(Name, "Sobek_3b.fnm")) ? path : System.IO.Path.Combine(directoryName, Name, "Sobek_3b.fnm");
        }
        public virtual void SwitchTo(string newPath)
        {
            Path = GetRRSavePathFromDeltaShellPath(newPath);
        }

        /// <summary>
        /// File path where the RR data is stored
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual string Path { get; set; }
        
        public virtual IEnumerable<string> Paths
        {
            get { return Enumerable.Empty<string>(); }
        }

        public virtual bool IsFileCritical
        {
            get { return true; }
        }

        public virtual bool IsOpen
        {
            get { return false; }
        }
        public virtual bool CopyFromWorkingDirectory { get; }

        public bool UseRestart { get; set; }

        public bool WriteRestart { get; set; }
        
        #region Hybride Section File-NHibernate Data

        /// <summary>
        /// Save UnpavedDataExtended is for persistency, save/load in Nhibernate
        /// See <see cref="UnpavedDataExtended"/>
        /// </summary>
        public virtual IEventedList<UnpavedDataExtended> SaveUnpavedDataExtended { get; set; }
        
        /// <summary>
        /// Method to synchronize UnpavedData and UnpavedDataExtended
        /// </summary>
        public void UpdateUnpavedDataWithExtendedData()
        {
            var unpavedDataExtendDictionary = SaveUnpavedDataExtended.ToDictionary(data => data.CatchmentName, data => data.UseLocalBoundaryData);
            var unpavedDataEnumerable = ModelData.OfType<UnpavedData>();
            unpavedDataEnumerable.ForEach(unpavedData =>
            {
                 if (unpavedDataExtendDictionary.ContainsKey(unpavedData.Name)) unpavedData.BoundarySettings.UseLocalBoundaryData = unpavedDataExtendDictionary[unpavedData.Name];
            });
        }

        /// <summary>
        /// Method to synchronize UnpavedData and UnpavedDataExtended
        /// </summary>
        public void UpdateUnpavedDataExtended()
        {
            SaveUnpavedDataExtended.Clear();
            var unpavedDataCollection = modelData.OfType<UnpavedData>();
            unpavedDataCollection.ForEach(unpavedData => SaveUnpavedDataExtended.Add(new UnpavedDataExtended(unpavedData.Name,unpavedData.BoundarySettings.UseLocalBoundaryData)));
        }
        
        #endregion Hybride Section File-NHibernate Data
    }

}