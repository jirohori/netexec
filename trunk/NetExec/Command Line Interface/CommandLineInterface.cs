using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NetExec
{
    class CommandLineInterface
    {
        string _stdout = string.Empty;
        string _stderr = string.Empty;
        string _command = string.Empty;

        public CommandLineInterface(string command)
        {
            _command = command;
        }

        public string Command { get { return _command; } }
        public string StdOut { get { return _stdout; } }
        public string StdErr { get { return _stderr; } }

        public void Run()
        {
            string file = Guid.NewGuid().ToString("N");
            file = Path.ChangeExtension(file, ".bat");

            File.WriteAllText(file, Command);

            ProcessStartInfo info = new ProcessStartInfo(file);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;

            Process p = Process.Start(info);
            p.WaitForExit();

            _stdout = p.StandardOutput.ReadToEnd();
            _stderr = p.StandardError.ReadToEnd();
            
            File.Delete(file);
        }
    }
}
