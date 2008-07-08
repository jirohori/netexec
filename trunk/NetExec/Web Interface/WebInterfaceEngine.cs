using System.IO;

using ITHit.WebDAV.Server;
using System.Text.RegularExpressions;

using System;
using System.Collections.Specialized;
using System.Web;
using System.Text;
using System.Threading;
namespace NetExec
{
    class WebInterfaceEngine
    {
        WDRequest _request;
        WDResponse _response;

        string _urlPath;
        string _urlParentPath;
        string _localPath;

        void ProcessExecPost()
        {
            string command = string.Empty;
            using (StreamReader r = new StreamReader(_request.InputStream))
            {
                NameValueCollection vars = HttpUtility.ParseQueryString(r.ReadToEnd());
                command = vars["command"];
                //if (command.Length > 1024)
                //    return false;
            }

            using (MemoryStream mem = new MemoryStream())
            {
                using (StreamWriter w = new StreamWriter(mem))
                {
                    w.Write(command);
                    w.Flush();

                    RemoteExec r = new RemoteExec(new FileInfo(_localPath));

                    mem.Seek(0, SeekOrigin.Begin);
                    r.SaveFromStream(mem, "text");
                }
            }

            Thread.Sleep(1000);
            ProcessExecGet();
        }

        void ProcessExecGet()
        {
            _response.StatusCode = 200;
            _response.ContentType = "text/html";
            _response.AddHeader("Server", "Microsoft-IIS/6.0");

            using (StreamWriter w = new StreamWriter(_response.OutputStream))
            {
                w.Write(@"<html><head><META http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">");
                w.Write(@"<title>{0} - {1}</title></head>", _request.Url.Host, _urlPath);
                w.Write(@"<body onload=""window.scrollBy(0,99999)""><H1>{0} - {1}</H1><hr><pre>", _request.Url.Host, _urlPath);
                w.Write(@"<A HREF=""{0}"">[To Parent Directory]</A><br><br>", _urlParentPath);
                w.Flush();

                
                
                RemoteExec r = new RemoteExec(new FileInfo(_localPath), true);
                r.WriteToStream(_response.OutputStream, 0, r.ContentLength);

                w.Write("</pre><hr>");
                w.Write("<span>Awaiting your command</span>");
                w.Write("<form method=post><input type=text name=command style='width: 300px' maxlength=1024><input type=submit value=submit /></form>");
                w.Write("</body></html>");
            }
        }

        void ProcessDirectoryGet()
        {
            DirectoryInfo directory = new DirectoryInfo(_localPath);

            _response.StatusCode = 200;
            _response.ContentType = "text/html";
            _response.AddHeader("Server", "Microsoft-IIS/6.0");
            using (StreamWriter w = new StreamWriter(_response.OutputStream))
            {
                w.Write(@"<html><head><META http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">");
                w.Write(@"<title>{0} - {1}</title></head>", _request.Url.Host, _urlPath);
                w.Write(@"<body><H1>{0} - {1}</H1><hr><pre>", _request.Url.Host, _urlPath);

                if (_urlParentPath != null)
                    w.Write(@"<A HREF=""{0}"">[To Parent Directory]</A><br><br>", _urlParentPath);

                // for autocomplete
                RemoteExec r = new RemoteExec(new FileInfo(System.IO.Path.Combine(directory.FullName, "$Remote Command Prompt")));
                w.Write(@"{0}  {1}  {2}  <A HREF=""{3}"">{4}</A><br>",
                        r.Modified.ToString("dddd, M/d/yyyy").PadLeft(25),
                        r.Modified.ToString("h:mm tt").PadLeft(8),
                        r.ContentLength.ToString().PadLeft(10),
                        Path.Combine(_urlPath, r.Name).Replace('\\', '/'),
                        r.Name);

                foreach (DirectoryInfo dir in directory.GetDirectories())
                    w.Write(@"{0}  {1}  {2}  <A HREF=""{3}"">{4}</A><br>",
                        dir.LastWriteTime.ToString("dddd, M/d/yyyy").PadLeft(25),
                        dir.LastWriteTime.ToString("h:mm tt").PadLeft(8),
                        "   &lt;DIR&gt;  ",
                        Path.Combine(_urlPath, dir.Name).Replace('\\', '/'),
                        dir.Name);

                foreach (FileInfo fi in directory.GetFiles())
                    w.Write(@"{0}  {1}  {2}  <A HREF=""{3}"">{4}</A><br>",
                        fi.LastWriteTime.ToString("dddd, M/d/yyyy").PadLeft(25),
                        fi.LastWriteTime.ToString("h:mm tt").PadLeft(8),
                        fi.Length.ToString().PadLeft(10),
                        Path.Combine(_urlPath, fi.Name).Replace('\\', '/'),
                        fi.Name);

                w.Write("</pre><hr></body></html>");
            }
        }

        /// <summary>
        /// Processes the request for web browsers if it is not for webdav
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns>true if the request was processed as a vanilla http request, false if not</returns>
        public bool Run(WDRequest request, WDResponse response)
        {
            if (request.UserAgent.IndexOf("Mozilla", StringComparison.OrdinalIgnoreCase) == -1 &&
                request.UserAgent.IndexOf("Opera", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            _request = request;
            _response = response;

            _urlPath = HttpUtility.UrlDecode(request.Url.AbsolutePath.Split('?')[0].TrimEnd('/'));
            
            _urlParentPath = _urlPath == "" ? null : Regex.Replace(_urlPath, "/+[^/]*$", "/");
            _localPath = Environment.CurrentDirectory.TrimEnd('\\') + "\\" + _urlPath.TrimStart('/');

            if (Path.GetFileName(_localPath).StartsWith("$Remote Command Prompt", StringComparison.OrdinalIgnoreCase) && request.HttpMethod == "GET")
                ProcessExecGet();
            else if (Path.GetFileName(_localPath).StartsWith("$Remote Command Prompt", StringComparison.OrdinalIgnoreCase) && request.HttpMethod == "POST")
                ProcessExecPost();
            else if (request.HttpMethod == "GET" && Directory.Exists(_localPath))
                ProcessDirectoryGet();
            else
                return false;

            return true;
        }
    }
}
