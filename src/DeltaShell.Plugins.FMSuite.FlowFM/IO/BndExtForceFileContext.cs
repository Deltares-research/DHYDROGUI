using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Represents the context of the reading and writing of an external forcing file.
    /// </summary>
    public sealed class BndExtForceFileContext
    {
        private readonly Dictionary<object, string> forcingFileNames;
        private readonly Dictionary<IBoundaryCondition, IniSection> iniSections;
        private readonly Dictionary<IFeature, string> pliFileNames;
        private string modelName;
        private string roofAreaFile;

        /// <summary>
        /// Initialize a new instance of the <see cref="BndExtForceFileContext"/> class.
        /// </summary>
        public BndExtForceFileContext()
        {
            iniSections = new Dictionary<IBoundaryCondition, IniSection>();
            forcingFileNames = new Dictionary<object, string>();
            pliFileNames = new Dictionary<IFeature, string>();
        }

        /// <summary>
        /// The polyline file names.
        /// </summary>
        public IEnumerable<string> PolylineFileNames => pliFileNames.Values.Distinct();

        /// <summary>
        /// The forcing file names.
        /// </summary>
        public IEnumerable<string> ForcingFileNames => forcingFileNames.Values.Distinct();

        /// <summary>
        /// The model name.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is <c>null</c> or white space.
        /// </exception>
        public string ModelName
        {
            get => modelName;
            set
            {
                Ensure.NotNullOrWhiteSpace(value, nameof(value));
                modelName = value.Trim();
            }
        }

        /// <summary>
        /// The roof area file name.
        /// When unset, the default roof area file name is returned.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ModelName"/> is not set, and default file name is requested.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is <c>null</c> or white space.
        /// </exception>
        public string RoofAreaFileName
        {
            get => roofAreaFile ?? GetDefaultRoofAreaFileName();
            set
            {
                Ensure.NotNullOrWhiteSpace(value, nameof(value));
                roofAreaFile = value;
            }
        }

        /// <summary>
        /// Clear all data from this context.
        /// </summary>
        public void Clear()
        {
            iniSections.Clear();
            pliFileNames.Clear();
            forcingFileNames.Clear();
        }

        /// <summary>
        /// Add a <see cref="IFmMeteoField"/> with its corresponding forcing file name.
        /// </summary>
        /// <param name="data"> The object to add the forcing file name for. </param>
        /// <param name="fileName"> The forcing file name. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or white space.
        /// </exception>
        public void AddForcingFileName(IFmMeteoField data, string fileName) => AddForcingFileNameCore(data, fileName);

        /// <summary>
        /// Add a <see cref="Model1DBoundaryNodeData"/> with its corresponding forcing file name.
        /// </summary>
        /// <param name="data"> The object to add the forcing file name for. </param>
        /// <param name="fileName"> The forcing file name. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or white space.
        /// </exception>
        public void AddForcingFileName(Model1DBoundaryNodeData data, string fileName) => AddForcingFileNameCore(data, fileName);

        /// <summary>
        /// Add a <see cref="Model1DLateralSourceData"/> with its corresponding forcing file name.
        /// </summary>
        /// <param name="data"> The object to add the forcing file name for. </param>
        /// <param name="fileName"> The forcing file name. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or white space.
        /// </exception>
        public void AddForcingFileName(Model1DLateralSourceData data, string fileName) => AddForcingFileNameCore(data, fileName);

        /// <summary>
        /// Add a <see cref="IBoundaryCondition"/> with its corresponding <see cref="IniSection"/>.
        /// </summary>
        /// <param name="data"> The object to add the INI section for. </param>
        /// <param name="iniSection"> The corresponding INI section. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> or <paramref name="iniSection"/> is <c>null</c>.
        /// </exception>
        public void AddIniSection(IBoundaryCondition data, IniSection iniSection)
        {
            Ensure.NotNull(data, nameof(data));
            Ensure.NotNull(iniSection, nameof(iniSection));

            iniSections[data] = iniSection;
        }

        /// <summary>
        /// Add a <see cref="IFeature"/> with its corresponding polyline file name.
        /// </summary>
        /// <param name="feature"> The feature. </param>
        /// <param name="fileName"> The polyline file name. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="feature"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or white space.
        /// </exception>
        public void AddPolylineFileName(IFeature feature, string fileName)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNullOrWhiteSpace(fileName, nameof(fileName));

            pliFileNames[feature] = fileName;
        }

        /// <summary>
        /// Get the first corresponding <see cref="IFeature"/> for a polyline file name.
        /// </summary>
        /// <param name="fileName"> The polyline file name. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or white space.
        /// </exception>
        /// <returns>
        /// If found, the first corresponding <see cref="IFeature"/>; otherwise, <c>null</c>.
        /// </returns>
        public IFeature GetFeatureForPolylineFileName(string fileName)
        {
            Ensure.NotNullOrWhiteSpace(fileName, nameof(fileName));
            return pliFileNames.FirstOrDefault(kvp => kvp.Value == fileName).Key;
        }

        /// <summary>
        /// Get the corresponding forcing file name for a <see cref="IFmMeteoField"/>.
        /// </summary>
        /// <param name="data"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ModelName"/> is not set, and default file name is requested.
        /// </exception>
        /// <returns>
        /// If found, the corresponding forcing file; otherwise, the default file name.
        /// </returns>
        public string GetForcingFileName(IFmMeteoField data) =>
            GetForcingFileNameCore(data) ?? GetDefaultFileName("meteo");

        /// <summary>
        /// Get the corresponding forcing file name for a <see cref="Model1DBoundaryNodeData"/>.
        /// </summary>
        /// <param name="data"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ModelName"/> is not set, and default file name is requested.
        /// </exception>
        /// <returns>
        /// If found, the corresponding forcing file; otherwise, the default file name.
        /// </returns>
        public string GetForcingFileName(Model1DBoundaryNodeData data) =>
            GetForcingFileNameCore(data) ?? GetDefaultFileName("boundaryconditions1d");

        /// <summary>
        /// Get the corresponding forcing file name for a <see cref="Model1DLateralSourceData"/>.
        /// </summary>
        /// <param name="data"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ModelName"/> is not set, and default file name is requested.
        /// </exception>
        /// <returns>
        /// If found, the corresponding forcing file; otherwise, the default file name.
        /// </returns>
        public string GetForcingFileName(Model1DLateralSourceData data) =>
            GetForcingFileNameCore(data) ?? GetDefaultFileName("lateral_sources");

        /// <summary>
        /// Get the corresponding <see cref="IniSection"/> for a <see cref="IBoundaryCondition"/>.
        /// </summary>
        /// <param name="data"> The object to get the INI section for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// If found, the corresponding <see cref="IniSection"/>; otherwise, <c>null</c>.
        /// </returns>
        public IniSection GetIniSection(IBoundaryCondition data)
        {
            Ensure.NotNull(data, nameof(data));
            return iniSections.TryGetValue(data, out IniSection iniSection) ? iniSection : null;
        }

        /// <summary>
        /// Get the corresponding polyline file name for a <see cref="IFeature"/>.
        /// </summary>
        /// <param name="feature"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="feature"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// If found, the corresponding polyline file name; otherwise, the default file name.
        /// </returns>
        public string GetPolylineFileName(IFeature feature) => GetPolylineFileNameCore(feature);

        /// <summary>
        /// Get the corresponding polyline file name for a <see cref="BoundaryConditionSet"/>.
        /// </summary>
        /// <param name="boundaryConditionsSet"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryConditionsSet"/> or <paramref name="boundaryConditionsSet.Feature"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// If found, the corresponding polyline file name; otherwise, the default file name.
        /// </returns>
        public string GetPolylineFileName(BoundaryConditionSet boundaryConditionsSet)
        {
            Ensure.NotNull(boundaryConditionsSet, nameof(boundaryConditionsSet));
            return GetPolylineFileNameCore(boundaryConditionsSet.Feature) ?? ExtForceFileHelper.GetPliFileName(boundaryConditionsSet);
        }

        /// <summary>
        /// Get the corresponding polyline file name for a <see cref="Embankment"/>.
        /// </summary>
        /// <param name="embankment"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="embankment"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// If found, the corresponding polyline file name; otherwise, the default file name.
        /// </returns>
        public string GetPolylineFileName(Embankment embankment) => 
            GetPolylineFileNameCore(embankment) ?? embankment.Name + "_bnk.pliz";

        /// <summary>
        /// Get the corresponding polyline file name for a <see cref="IFmMeteoField"/>.
        /// </summary>
        /// <param name="meteoField"> The object to get the forcing file name for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="meteoField"/>, <paramref name="meteoField.FeatureData"/> or
        /// <paramref name="meteoField.FeatureData.Feature"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// If found, the corresponding polyline file name; otherwise, the default file name.
        /// </returns>
        public string GetPolylineFileName(IFmMeteoField meteoField)
        {
            Ensure.NotNull(meteoField, nameof(meteoField));
            Ensure.NotNull(meteoField.FeatureData, nameof(meteoField.FeatureData));
            return GetPolylineFileNameCore(meteoField.FeatureData.Feature) ?? ExtForceFileHelper.GetPliFileName(meteoField.FeatureData);
        }

        private void AddForcingFileNameCore(object dataObject, string forcingFile)
        {
            Ensure.NotNull(dataObject, nameof(dataObject));
            Ensure.NotNullOrWhiteSpace(forcingFile, nameof(forcingFile));

            forcingFileNames[dataObject] = forcingFile;
        }

        private string GetForcingFileNameCore(object dataObject)
        {
            Ensure.NotNull(dataObject, nameof(dataObject));
            return forcingFileNames.TryGetValue(dataObject, out string fileName) ? fileName : null;
        }

        private string GetPolylineFileNameCore(IFeature feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return pliFileNames.TryGetValue(feature, out string fileName) ? fileName : null;
        }

        private string GetDefaultFileName(string fileDescription)
        {
            EnsureSetModelName();
            return $"{ModelName}_{fileDescription}{BcFile.Extension}";
        }

        private string GetDefaultRoofAreaFileName()
        {
            EnsureSetModelName();
            return ModelName + FileConstants.RoofAreaFileExtension;
        }

        private void EnsureSetModelName()
        {
            if (ModelName == null)
            {
                throw new InvalidOperationException($"Cannot generate a default file name when {nameof(ModelName)} is not set.");
            }
        }
    }
}