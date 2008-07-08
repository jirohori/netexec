using System.IO;
using System.Net;
using System.Text;
using ITHit.WebDAV.Server;

namespace WebDAVServer.FileSystemListenerService
{
    public class WDResponse : IResponse
    {
        private HttpListenerResponse response;

        public WDResponse(HttpListenerResponse response)
        {
            this.response = response;
        }

        public void AddHeader(string name, string value)
        {
            response.AddHeader(name, value);
        }

        public Encoding ContentEncoding
        {
            set { response.ContentEncoding = value; }
        }

        public string ContentType
        {
            set{ response.ContentType = value; }
        }
        public long ContentLength
        {
            set { response.ContentLength64 = value; }
        }
        public bool BufferOutput
        {
            get { return !response.SendChunked; }
        }

        public Stream OutputStream
        {
            get { return response.OutputStream; }
        }

        public int StatusCode
        {
            get { return response.StatusCode; }
            set { response.StatusCode = value; }
        }

        public string StatusDescription
        {
            set { response.StatusDescription = value; }
        }

        public void Clear()
        {
            // workaround of litmus/neon WebDAV client library bug
            response.SendChunked = false; // could be commented out for any other WebDAV client
        }
    }
}
