using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations
{
    /// <summary>
    /// <see cref="IniFileOperator"/> implements the interface to execute
    /// a set of <see cref="IIniPropertyBehaviour"/> on a specified file.
    /// </summary>
    /// <seealso cref="IIniFileOperator" />
    public sealed class IniFileOperator : IIniFileOperator
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>
            categoryPropertyBehaviourMapping;

        private readonly IIniReader iniReader;
        private readonly IIniPostOperationBehaviour[] postOperations;

        /// <summary>
        /// Creates a new <see cref="IniFileOperator"/>.
        /// </summary>
        /// <param name="categoryPropertyBehaviourMapping">The category to property to behaviour mapping.</param>
        /// <param name="iniReader">The ini reader.</param>
        /// <param name="postOperations">The list of <see cref="IIniPostOperationBehaviour"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public IniFileOperator(
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> categoryPropertyBehaviourMapping,
            IIniReader iniReader,
            IEnumerable<IIniPostOperationBehaviour> postOperations)
        {
            Ensure.NotNull(categoryPropertyBehaviourMapping, nameof(categoryPropertyBehaviourMapping));
            Ensure.NotNull(iniReader, nameof(iniReader));

            this.postOperations = postOperations as IIniPostOperationBehaviour[] ?? postOperations?.ToArray();
            Ensure.NotNull(this.postOperations, nameof(postOperations));

            this.categoryPropertyBehaviourMapping = categoryPropertyBehaviourMapping;
            this.iniReader = iniReader;
        }

        public void Invoke(Stream sourceFileStream, string sourceFilePath, ILogHandler logHandler)
        {
            Ensure.NotNull(sourceFileStream, nameof(sourceFileStream));
            Ensure.NotNull(sourceFilePath, nameof(sourceFilePath));

            IniData iniData = iniReader.ReadIniFile(sourceFileStream, sourceFilePath);

            foreach (IniSection section in iniData.Sections)
            {
                InvokeOnSection(section, logHandler);
            }

            foreach (IIniPostOperationBehaviour postOperation in postOperations)
            {
                postOperation.Invoke(sourceFileStream, sourceFilePath, iniData, logHandler);
            }
        }

        private void InvokeOnSection(IniSection section, ILogHandler logHandler)
        {
            if (!categoryPropertyBehaviourMapping.TryGetValue(section.Name, out IReadOnlyDictionary<string, IIniPropertyBehaviour> categoryMapping))
            {
                return;
            }

            foreach (IniProperty property in section.Properties)
            {
                InvokeOnProperty(property, categoryMapping, logHandler);
            }
        }

        private static void InvokeOnProperty(IniProperty property,
                                             IReadOnlyDictionary<string, IIniPropertyBehaviour> categoryMapping,
                                             ILogHandler logHandler)
        {
            if (categoryMapping.TryGetValue(property.Key, out IIniPropertyBehaviour propertyBehaviour))
            {
                propertyBehaviour.Invoke(property, logHandler);
            }
        }
    }
}