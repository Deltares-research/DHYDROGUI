using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.ConfigXml
{
    public static class DelftConfigXmlFileReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DelftConfigXmlFileReader));

        public static string Read(string configFilePath)
        {
            try
            {
                //DelftConfigXmlFileParser(configFilePath);
            }
            catch
            {

            }
            return null;
        }
    }
}
