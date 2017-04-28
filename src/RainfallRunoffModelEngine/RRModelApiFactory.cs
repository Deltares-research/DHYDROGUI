using System;
using System.IO;
using DelftTools.Utils.Remoting;

namespace RainfallRunoffModelEngine
{
    public static class RRModelApiFactory
    {
        public static IRRModelApi CreateApi(bool remote, string workingDirectory=null)
        {
            if (remote)
            {
                LogDirectories();

                return RemoteInstanceContainer.CreateInstance<IRRModelApi, RRModelHybridEngine>(workingDirectory);
            }
            return new RRModelHybridEngine();
        }

        private static void LogDirectories()
        {
            var dllPath = Path.GetDirectoryName(typeof (IRRModelApi).Assembly.Location);
            Console.WriteLine("Interface dll path: " + dllPath);
            Console.WriteLine("Is native rr_dll present: " + File.Exists(dllPath + "\\rr_dll.dll"));
        }

        public static void Cleanup(IRRModelApi api)
        {
            RemoteInstanceContainer.RemoveInstance(api);
        }
    }
}
