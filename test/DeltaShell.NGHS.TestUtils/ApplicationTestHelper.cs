using System.Collections.Generic;
using System.Configuration;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NSubstitute;

namespace DeltaShell.NGHS.TestUtils
{
    public static class ApplicationTestHelper
    {
        /// <summary>
        /// Get a substitute for Application Setting for the Working Directory, which can be set as one of user settings.
        /// </summary>
        /// <param name="workDirectory">The specified path for the working directory setting.</param>
        /// <returns>Method will return a substitute for Application Setting</returns>
        /// <remarks>
        /// Under normal execution the work directory user setting is set as part of DeltaShellGui, however when not using this
        /// this specific value needs to be set in order for the application to work. As such, we create a substitute for this
        /// value.
        /// </remarks>
        public static ApplicationSettingsBase GetMockedApplicationSettingsBase(string workDirectory)
        {
            var applicationSettings = Substitute.For<ApplicationSettingsBase>();
            applicationSettings["WorkDirectory"] = workDirectory;

            return applicationSettings;
        }

        public static DeltaShellApplication GetApplication(string workDir, params ApplicationPlugin[] plugins)
        {
            var app = new DeltaShellApplication
            {
                UserSettings = GetMockedApplicationSettingsBase(workDir),
                IsProjectCreatedInTemporaryDirectory = true
            };

            app.Plugins.AddRange(GetStandardPlugins());
            app.Plugins.AddRange(plugins);

            app.Run();

            return app;
        }

        private static IEnumerable<ApplicationPlugin> GetStandardPlugins()
        {
            yield return new NHibernateDaoApplicationPlugin();
            yield return new CommonToolsApplicationPlugin();
            yield return new SharpMapGisApplicationPlugin();
            yield return new NetworkEditorApplicationPlugin();
        }
    }
}