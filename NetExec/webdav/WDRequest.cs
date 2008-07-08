using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using ITHit.WebDAV.Server;

namespace WebDAVServer.FileSystemListenerService
{
    class WDRequest : Request
    {
        private HttpListenerRequest request;
        private IPrincipal user;

        public WDRequest(HttpListenerRequest request, IPrincipal user)
        {
            this.request = request;
            this.user = user;
        }
        public IPrincipal User
        {
            get { return user; }
        }

        public override string ApplicationPath
        {
            get
            {

                 
                string uriPrefix = "http://+:" + NetDrive.Program.Options.Port + "/";
                
                int i = uriPrefix.IndexOf("://");
                i = uriPrefix.IndexOf('/', i + 3);

                return uriPrefix.Substring(i);
            }
        }

        public override Encoding ContentEncoding
        {
            get { return request.ContentEncoding; }
        }

        public override long ContentLength
        {
            get { return (int) request.ContentLength64; }
        }

        public override string ContentType
        {
            get { return request.ContentType; }
        }

        public override NameValueCollection Headers
        {
            get { return request.Headers; }
        }

        public override string HttpMethod
        {
            get { return request.HttpMethod; }
        }

        public override Stream InputStream
        {
            get { return request.InputStream; }
        }

        public override Uri Url
        {
            get { return request.Url; }
        }

        public override string UserAgent
        {
            get
            {
                return request.UserAgent;
            }
        }
    }
}
