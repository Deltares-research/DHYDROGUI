using System.Configuration;
using NSubstitute;

namespace DeltaShell.NGHS.TestUtils
{
    public class ApplicationTestHelper
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
    }
}