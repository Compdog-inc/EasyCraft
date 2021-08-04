using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleWrapper
{
    public class ConsoleClosedEventArgs : EventArgs
    {
        public int ExitCode { get; }
        
        public ConsoleClosedEventArgs(int exitCode)
        {
            ExitCode = exitCode;
        }
    }

    public struct ConsoleInfo
    {
        public static int WINDOW_DEFAULT { get => -1; }

        /// <summary>
        /// The relative path of the Console executable
        /// </summary>
        public string ExecutablePath;

        /// <summary>
        /// Absolute X position of console window
        /// </summary>
        public int WindowX;

        /// <summary>
        /// Absolute Y position of console window
        /// </summary>
        public int WindowY;

        /// <summary>
        /// Absolute width of console window
        /// </summary>
        public int WindowWidth;

        /// <summary>
        /// Absolute height of console window
        /// </summary>
        public int WindowHeight;

        public bool UseThreads;

        public ConsoleInfo(string executablePath)
        {
            ExecutablePath = executablePath;
            WindowX = WINDOW_DEFAULT;
            WindowY = WINDOW_DEFAULT;
            WindowWidth = WINDOW_DEFAULT;
            WindowHeight = WINDOW_DEFAULT;
            UseThreads = true;
        }
    }

    public enum LogType
    {
        None,
        Info,
        Warning,
        Error
    }

    public enum ConsoleResult
    {
        OK,
        CREATE_ERROR,
        PIPE_ERROR,
        DUPLICATE,
        NOT_STARTED
    }

    public class Console : IDisposable
    {
        private struct LogQueueItem
        {
            public LogType type;
            public string text;

            public LogQueueItem(LogType _type, string _text)
            {
                type = _type;
                text = _text;
            }
        }

        public int LogQueueCount { get => logQueue.Count; }
        public bool LogPipeOverflow { get => pipeOverflow; }
        public bool LogBatch { get => inBatch; }
        public bool EnableBatching { get; set; } = true;
        public int LogBatchSize { get; set; } = 100;
        public bool IsOpen { get => open; }
        public int ExitCode { get => exitCode; }

        public ConsoleInfo ConsoleInfo;

        public event EventHandler<ConsoleClosedEventArgs> ConsoleClosed;
        public event EventHandler PipeOverflow;

        private Process process;
        private AnonymousPipeServerStream pipeServer;
        private StreamWriter pipeWriter;
        private Queue<LogQueueItem> logQueue = new Queue<LogQueueItem>();
        private DateTime lastLogItem;
        private bool pipeOverflow = false;
        private bool inBatch = false;
        private bool open = false;
        private int exitCode = 0;

        public static Console Default
        {
            get
            {
                return new Console("Console.exe");
            }
        }

        public Console() { }

        public Console(string executablePath)
        {
            ConsoleInfo.ExecutablePath = executablePath;
            ConsoleInfo.WindowX = ConsoleInfo.WINDOW_DEFAULT;
            ConsoleInfo.WindowY = ConsoleInfo.WINDOW_DEFAULT;
            ConsoleInfo.WindowWidth = ConsoleInfo.WINDOW_DEFAULT;
            ConsoleInfo.WindowHeight = ConsoleInfo.WINDOW_DEFAULT;
            ConsoleInfo.UseThreads = true;
        }

        public Console(ConsoleInfo consoleInfo)
        {
            ConsoleInfo = consoleInfo;
        }

        public ConsoleResult Create(out Exception exception)
        {
            exception = null;
            if (open) return ConsoleResult.DUPLICATE;

            lastLogItem = DateTime.Now;
            process = new Process();
            process.StartInfo.FileName = ConsoleInfo.ExecutablePath;
            if (File.Exists(process.StartInfo.FileName))
            {
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.UseShellExecute = false;

                pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

                process.StartInfo.Arguments =
                    pipeServer.GetClientHandleAsString();

                try
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, e) =>
                    {
                        if (!process.HasExited)
                        {
                            ConsoleClosed?.Invoke(this, new ConsoleClosedEventArgs(process.ExitCode));
                            exitCode = process.ExitCode;
                            if (open)
                            {
                                pipeWriter?.Close();
                                pipeWriter?.Dispose();
                                pipeServer.Close();
                                pipeServer.Dispose();
                                process.Close();
                                process.Dispose();
                                open = false;
                            }
                        }
                    };
                    process.Start();
                    open = true;
                }
                catch (Exception e)
                {
                    exception = e;
                    return ConsoleResult.CREATE_ERROR;
                }

                if (open)
                {
                    pipeServer.DisposeLocalCopyOfClientHandle();

                    try
                    {
                        pipeWriter = new StreamWriter(pipeServer);
                        pipeWriter.AutoFlush = true;
                        pipeWriter.WriteLine("SYNC");
                        pipeServer.WaitForPipeDrain();
                    }
                    catch (IOException e)
                    {
                        open = false;
                        pipeServer.Close();
                        pipeServer.Dispose();
                        exception = e;
                        return ConsoleResult.PIPE_ERROR;
                    }

                    new Task(ConsoleTask).Start();
                }
                else
                {
                    pipeServer.DisposeLocalCopyOfClientHandle();
                    pipeWriter?.Close();
                    pipeWriter?.Dispose();
                    pipeServer.Close();
                    pipeServer.Dispose();
                }
            }

            if (!open)
            {
                return ConsoleResult.NOT_STARTED;
            }

            return ConsoleResult.OK;
        }

        public void Dispose()
        {
            if (open)
            {
                open = false;
                pipeWriter?.Close();
                pipeWriter?.Dispose();
                pipeServer.Close();
                pipeServer.Dispose();
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
                process.Close();
                process.Dispose();
            }
        }

        public void Log(LogType type, string text)
        {
            if (open)
            {
                logQueue.Enqueue(new LogQueueItem(type, text));
                lastLogItem = DateTime.Now;
            }
        }

        public void Log(string text)
        {
            Log(LogType.Info, text);
        }

        public void LogWarning(string text)
        {
            Log(LogType.Warning, text);
        }

        public void LogError(string text)
        {
            Log(LogType.Error, text);
        }

        private void ConsoleTask()
        {
            while (open)
            {
                while (logQueue.Count > 0)
                {
                    if (!EnableBatching) inBatch = false;
                    if (!inBatch || !EnableBatching)
                    {
                        TimeSpan sincePrevLog = DateTime.Now.Subtract(lastLogItem);
                        if (sincePrevLog.TotalMilliseconds < 500 && logQueue.Count > LogBatchSize)
                        {
                            if (!pipeOverflow)
                            {
                                pipeOverflow = true;
                                PipeOverflow?.Invoke(this, EventArgs.Empty);
                            }
                        }
                        else
                        {
                            pipeOverflow = false;
                        }
                    }

                    if ((!inBatch || !EnableBatching) && pipeOverflow)
                    {
                        if (logQueue.Count % LogBatchSize == 0 && EnableBatching)
                            inBatch = true;
                        else
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                    }

                    LogQueueItem item = logQueue.Dequeue();
                    pipeWriter.Write((char)(int)item.type);
                    pipeWriter.Write(item.text);
                    pipeWriter.Write((char)0);
                }
                inBatch = false;
                Thread.Sleep(1);
            }
        }
    }
}
