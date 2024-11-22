﻿using System.Collections.Generic;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData
{
    /// <summary>
    /// <see cref="CollectIniFileNamePostOperationBehaviour"/> implements the
    /// post operation to add the provided source file to the provided hash set.
    /// </summary>
    /// <seealso cref="IniPostOperationBehaviour"/>
    public class CollectIniFileNamePostOperationBehaviour : IniPostOperationBehaviour

    {
        private readonly HashSet<string> hashSet;
        private readonly string relativeDirectory;

        /// <summary>
        /// Creates a new <see cref="CollectIniFileNamePostOperationBehaviour"/>.
        /// </summary>
        /// <param name="hashSet">The hash set.</param>
        /// <param name="relativeDirectory">The relative directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public CollectIniFileNamePostOperationBehaviour(HashSet<string> hashSet, 
                                                        string relativeDirectory)
        {
            Ensure.NotNull(hashSet, nameof(hashSet));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));

            this.hashSet = hashSet;
            this.relativeDirectory = relativeDirectory;
        }

        public override void Invoke(Stream sourceFileStream, string sourceFilePath, IniData iniData, ILogHandler logHandler)
        {
            base.Invoke(sourceFileStream, sourceFilePath, iniData, logHandler);
            hashSet.Add(Path.Combine(relativeDirectory, sourceFilePath));
        }
    }
}