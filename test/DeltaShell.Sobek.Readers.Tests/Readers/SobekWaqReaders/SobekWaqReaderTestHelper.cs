using System;
using System.IO;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    public static class SobekWaqReaderTestHelper
    {
        /// <summary>
        /// Performs <paramref name="action"/> and returns the log
        /// </summary>
        public static string PerformActionAndGetLog(Action action)
        {
            string log;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var reader = new StreamReader(stream);

            // Redirect Console.Out
            var oldOut = Console.Out;
            Console.SetOut(writer);

            try
            {
                // Perform action
                action();
            }
            catch (Exception)
            {
                // Do nothing
            }
            finally
            {
                // Read the log
                writer.Flush();
                stream.Position = 0;
                log = reader.ReadToEnd();

                // Reset Console.Out
                Console.SetOut(oldOut);

                // Close all readers and writers
                writer.Close();
                reader.Close();
                stream.Close();
            }

            return log;
        }
    }
}
