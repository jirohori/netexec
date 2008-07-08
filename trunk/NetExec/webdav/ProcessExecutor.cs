using System.Collections;
using System.Collections.Generic;
using System.IO;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;
using System;
using System.Text;
using System.Diagnostics;
namespace WebDAVServer.FileSystemListenerService
{
    class ProcessExecutor
    {
        string _command;
        FileSystemInfo _context;

        public string Command { get { return _command; } }
        public FileSystemInfo Context { get { return _context; } }

        string _output = string.Empty;
        DateTime _outputModified = DateTime.Now;
        
        public string Output { get { return _output; } }
        public DateTime OutputModified { get { return _outputModified; } }

        Process _process;
        public bool IsRunning { get { return _process == null ? false : !_process.HasExited; } }

        internal ProcessExecutor(string command, FileSystemInfo context)
        {
            _command = command;
            _context = context;
        }

        public void Start()
        {            
            //if (_process != null)
            //    throw new Exception("Process already started");

            string file = Guid.NewGuid().ToString("N");
            file = Path.ChangeExtension(file, ".bat");
            file = Path.Combine(Path.GetDirectoryName(_context.FullName), file);
            File.WriteAllText(file, Command);

            ProcessStartInfo info = new ProcessStartInfo(file);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            info.WorkingDirectory = Path.GetDirectoryName(_context.FullName);

            _process = new Process();
            _process.StartInfo = info;
            _process.EnableRaisingEvents = true;
            
            _process.OutputDataReceived += new DataReceivedEventHandler(_process_DataReceived);                
            _process.ErrorDataReceived += new DataReceivedEventHandler(_process_DataReceived);                
            _process.Exited += delegate(object sender, EventArgs e)
            {                   
                File.Delete(file);
                //    File.WriteAllText(Path.Combine(Path.GetDirectoryName(_context.FullName), "$resp.txt"), _output);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            
        }

        void _process_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (ProcessExecutionManager._syncRoot)
            {
                Console.WriteLine(e.Data);
                _output += e.Data + "\r\n" ;
                _outputModified = DateTime.Now;
            }
        }
    }

    class ProcessExecutionManager
    {
        internal static object _syncRoot = new object();

        static ProcessExecutor _current;
        public static ProcessExecutor Current
        {
            get
            {
                return _current;
            }
        }

        public static bool IsProcessRunning
        {
            get
            {
                lock (_syncRoot)
                {
                    return _current != null && _current.IsRunning;
                }
            }
        }

        public static void Run(string command, FileSystemInfo context)
        {
            lock (_syncRoot)
            {
                _current = new ProcessExecutor(command, context);
                _current.Start();
            }
        }
    }
}
