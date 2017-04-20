using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Captures console output (also Fortran console messages)
    /// http://stackoverflow.com/questions/9061655/redirect-cout-from-c-dll-to-a-textbox-in-c-sharp
    /// </summary>
    public class ConsoleRedirector : IDisposable
    {
        protected static ConsoleRedirector instance;
        protected static ConsoleRedirector errInstance;

        /// <summary>
        /// Start capturing console messages
        /// </summary>
        /// <param name="handler">Handler for console messages (use UserState of ProgressChangedEventArgs)</param>
        /// <param name="forceConsoleRedirection">Forces console redirection</param>
        public static void Attach(ProgressChangedEventHandler handler, ProgressChangedEventHandler errHandler, bool forceConsoleRedirection)
        {
            Debug.Assert(null == instance);
            instance = new ConsoleRedirector(handler, forceConsoleRedirection, STD_OUTPUT_HANDLE);
            errInstance = new ConsoleRedirector(errHandler, forceConsoleRedirection, STD_ERROR_HANDLE);
        }

        /// <summary>
        /// Stop capturing console messages
        /// </summary>
        public static void Detatch()
        {
            instance.Dispose();
            instance = null;

            errInstance.Dispose();
            errInstance = null;
        }

        public static bool IsAttached
        {
            get
            {
                return null != instance;
            }
        }

        private static void ResetConsoleOutStream()
        {
            //Force console to recreate its output stream the next time Write/WriteLine is called
            typeof(Console).GetField("_out", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, null);
        }

        private volatile bool isDisposed;
        private readonly BackgroundWorker worker;
        private readonly IntPtr stdout;
        private readonly Mutex sync;
        private readonly Timer timer;
        private readonly char[] buffer;
        private readonly AnonymousPipeServerStream outServer;
        private readonly TextReader outClient;
        private readonly bool forceConsoleRedirection;
        private readonly int outputHandle;

        private ConsoleRedirector(ProgressChangedEventHandler handler, bool forceConsoleRedirection, int outputHandle)
        {
            this.outputHandle = outputHandle;
            this.forceConsoleRedirection = forceConsoleRedirection;

            if (!this.forceConsoleRedirection)
            {
                //Make sure Console._out is initialized before we redirect stdout, so the redirection won't affect it
                TextWriter temp = Console.Out;
            }

            worker = new BackgroundWorker();
            worker.ProgressChanged += handler;
            worker.DoWork += WorkerDoWork;
            worker.WorkerReportsProgress = true;

            stdout = GetStdHandle(outputHandle);

            sync = new Mutex();
            buffer = new char[4096];

            outServer = new AnonymousPipeServerStream(PipeDirection.Out);
            var client = new AnonymousPipeClientStream(PipeDirection.In, outServer.ClientSafePipeHandle);
            Debug.Assert(outServer.IsConnected);
            outClient = new StreamReader(client, Encoding.Default);

            Debug.Assert(SetStdHandle(outputHandle, outServer.SafePipeHandle.DangerousGetHandle()));

            if (this.forceConsoleRedirection)
            {
                ResetConsoleOutStream(); //calls to Console.Write/WriteLine will now get made against the redirected stream
            }

            worker.RunWorkerAsync(outClient);

            timer = new Timer(Flush, null, 500, 500);

        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = (BackgroundWorker)sender;
            var client = (TextReader)e.Argument;
            try
            {
                while (true)
                {
                    var read = client.Read(buffer, 0, 4096);

                    if (read > 0)
                        backgroundWorker.ReportProgress(0, new string(buffer, 0, read));
                }
            }
            catch (ObjectDisposedException)
            {
                // Pipe was closed... terminate

            }
            catch (Exception ex)
            {

            }
        }

        private void Flush(object state)
        {
            outServer.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ConsoleRedirector()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                lock (sync)
                {
                    if (!isDisposed)
                    {
                        isDisposed = true;
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        timer.Dispose();
                        Flush(null);

                        try { SetStdHandle(outputHandle, stdout); }
                        catch (Exception) { }
                        outClient.Dispose();
                        outServer.Dispose();

                        if (forceConsoleRedirection)
                        {
                            ResetConsoleOutStream(); //Calls to Console.Write/WriteLine will now get redirected to the original stdout stream
                        }

                    }
                }
            }
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
    }
}