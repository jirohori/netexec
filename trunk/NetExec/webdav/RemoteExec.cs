using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;
using System;
using System.Text;
namespace WebDAVServer.FileSystemListenerService
{   
    public class RemoteExec : HierarchyItem, IResource
    {
        public RemoteExec(FileInfo file) : base(file)
        {
            init(false);
        }

        public RemoteExec(FileInfo file, bool htmlEncode)
            : base(file)
        {
            init(htmlEncode);
        }

        void init(bool htmlEncode)
        {        
            lock (GlobalIdSyncRoot)
            {
                GlobalId = GlobalId + 1;
                _id = GlobalId;  
            } 
            
            lock (ProcessExecutionManager._syncRoot)
            {                
                if (ProcessExecutionManager.Current != null)
                {
                    _modified = ProcessExecutionManager.Current.OutputModified;                        
                 
                    using(MemoryStream mem = new MemoryStream())
                    using(StreamWriter w = new StreamWriter(mem, Encoding.UTF8))
                    {
                        string temp = ProcessExecutionManager.Current.Output;
                        if(htmlEncode)
                            temp = HttpUtility.HtmlEncode(ProcessExecutionManager.Current.Output);

                        w.Write(temp);
                        w.Flush();
                        content = mem.ToArray();
                    }                    
                }
            }
        }

        // 
        static object GlobalIdSyncRoot = new object();
        static Base36.Base36 GlobalId = 0;
        //

        DateTime _modified = DateTime.Now;
        byte[] content = new byte[] { };
        Base36.Base36 _id = 0;
       

        public new string Name
        {
            get 
            {
                return "$Remote Command Prompt _NoCache" + _id.Value; 
            }
        }

        public override string GetFullPath()
        {
            FileInfo i = fileSystemInfo as FileInfo;
            return i.Directory.FullName + '\\' + Name;
        }

        public new DateTime Created { get { return new DateTime(0); } }
        public new DateTime Modified { get { return _modified; } }
        public string ContentType { get { return "text"; } }
        public long ContentLength { get { return content.Length; } } // why returns empty
        public override WebDAVResponse Delete() { return new NotAllowedResponse(); }
        public WebDAVResponse WriteToStream(Stream output, long startIndex, long count) 
        {
            output.Write(content, (int)startIndex, (int)count);

            return new OkResponse();
        }

        public WebDAVResponse SaveFromStream(Stream content, string contentType)
        {
            lock (ProcessExecutionManager._syncRoot)
            {
                if (ProcessExecutionManager.IsProcessRunning)
                {
                    return new PreconditionFailedResponse();
                }

                using (StreamReader r = new StreamReader(content))
                {
                    string command = r.ReadToEnd();
                    ProcessExecutionManager.Run(command.Trim('"', ' ', '\r', '\n'), this.fileSystemInfo);
                }

                return new OkResponse();
            }
        }
    }
}
