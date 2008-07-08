using System;
using System.Collections.Generic;
using System.Text;
using Plossum.CommandLine;
using System.Net;
using System.Diagnostics;

namespace NetDrive
{
    class Program
    {
        public static Options Options;

        static void Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            Options options = new Options();
            CommandLineParser parser = new CommandLineParser(options);
            parser.Parse();

            if (options.Help)
            {
                Console.WriteLine(parser.UsageInfo.ToString(78, false));
                return;
            }
            else if (parser.HasErrors)
            {
                Console.WriteLine(parser.UsageInfo.ToString(78, true));
                return;
            }
            //Trace.WriteLine("Listening on port " + options.Port);
            
            // save options for reference
            Program.Options = options;

            Console.WriteLine("Press ENTER to exit");

            HttpService service = new HttpService(options.Port);
            service.Start();            

            Console.ReadLine();
            service.Stop();
        }
    }
}
