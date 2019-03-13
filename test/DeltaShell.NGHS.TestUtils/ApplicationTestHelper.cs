using System.Configuration;
using Rhino.Mocks;

namespace DeltaShell.NGHS.TestUtils
{
    public class ApplicationTestHelper
    {
        /// <summary>
        /// Get a mocked Application Setting for the Working Directory, which can be set as one of user settings.  
        /// </summary>
        /// <param name="workDirectory">The specified path for the working directory setting.</param>
        /// <returns>Method will return a mocked Application Setting</returns>
        /// <remarks>
        /// Under normal execution the work directory user setting is set as part of DeltaShellGui, however when not using this
        /// this specific value needs to be set in order for the application to work. As such, we mock this value. 
        /// </remarks>
        public static ApplicationSettingsBase GetMockedApplicationSettingsBase(string workDirectory)
        {
            var applicationSettingsMock = MockRepository.GenerateStub<ApplicationSettingsBase>();
            applicationSettingsMock["WorkDirectory"] = workDirectory;

            return applicationSettingsMock;
        }
    }
}