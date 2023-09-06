using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations
{
    /// <summary>
    /// <see cref="DelftIniFileOperator"/> implements the interface to execute
    /// a set of <see cref="IDelftIniPropertyBehaviour"/> on a specified file.
    /// </summary>
    /// <seealso cref="IDelftIniFileOperator" />
    public sealed class DelftIniFileOperator : IDelftIniFileOperator
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>
            categoryPropertyBehaviourMapping;

        private readonly IDelftIniReader iniReader;
        private readonly IDelftIniPostOperationBehaviour[] postOperations;

        /// <summary>
        /// Creates a new <see cref="DelftIniFileOperator"/>.
        /// </summary>
        /// <param name="categoryPropertyBehaviourMapping">The category to property to behaviour mapping.</param>
        /// <param name="iniReader">The ini reader.</param>
        /// <param name="postOperations">The list of <see cref="IDelftIniPostOperationBehaviour"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public DelftIniFileOperator(
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> categoryPropertyBehaviourMapping,
            IDelftIniReader iniReader,
            IEnumerable<IDelftIniPostOperationBehaviour> postOperations)
        {
            Ensure.NotNull(categoryPropertyBehaviourMapping, nameof(categoryPropertyBehaviourMapping));
            Ensure.NotNull(iniReader, nameof(iniReader));

            this.postOperations = postOperations as IDelftIniPostOperationBehaviour[] ?? postOperations?.ToArray();
            Ensure.NotNull(this.postOperations, nameof(postOperations));

            this.categoryPropertyBehaviourMapping = categoryPropertyBehaviourMapping;
            this.iniReader = iniReader;
        }

        public void Invoke(Stream sourceFileStream, string sourceFilePath, ILogHandler logHandler)
        {
            Ensure.NotNull(sourceFileStream, nameof(sourceFileStream));
            Ensure.NotNull(sourceFilePath, nameof(sourceFilePath));

            IniData iniData = iniReader.ReadDelftIniFile(sourceFileStream, sourceFilePath);

            foreach (IniSection section in iniData.Sections)
            {
                InvokeOnSection(section, logHandler);
            }

            foreach (IDelftIniPostOperationBehaviour postOperation in postOperations)
            {
                postOperation.Invoke(sourceFileStream, sourceFilePath, iniData, logHandler);
            }
        }

        private void InvokeOnSection(IniSection section, ILogHandler logHandler)
        {
            if (!categoryPropertyBehaviourMapping.TryGetValue(section.Name, out IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> categoryMapping))
            {
                return;
            }

            foreach (IniProperty property in section.Properties)
            {
                InvokeOnProperty(property, categoryMapping, logHandler);
            }
        }

        private static void InvokeOnProperty(IniProperty property,
                                             IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> categoryMapping,
                                             ILogHandler logHandler)
        {
            if (categoryMapping.TryGetValue(property.Key, out IDelftIniPropertyBehaviour propertyBehaviour))
            {
                propertyBehaviour.Invoke(property, logHandler);
            }
        }
    }
}