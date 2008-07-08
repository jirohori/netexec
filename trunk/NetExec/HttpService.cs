using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Security.Principal;
namespace NetExec
{
    class HttpService
    {
        HttpListener _listener;
        Thread _thread;
        int _port = 80;
        private bool _listening = false;

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public HttpService(int port)
        {
            _port = port;
        }
        
        public void Start()
        {
            _listening = true;
            _thread = new Thread(new ThreadStart(ThreadProc));
            _thread.Start();         
        }

        public void Stop()
        {
            _listening = false;
            _thread.Abort();
            _thread.Join();
        }


        private void ThreadProc()
        {            
            HttpListener _listener = new HttpListener();
            _listener.AuthenticationSchemes = AuthenticationSchemes.Ntlm;
            
            _listener.Prefixes.Add("http://+:" + _port + "/");
            _listener.Start();

            Trace.WriteLine("Listening on port " + _port + "...");

            while (_listening)
            {
                IAsyncResult result = _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        public static void ListenerCallback(IAsyncResult result)
        {            
            HttpListener listener = (HttpListener)result.AsyncState;
              
                        
            HttpListenerContext context = listener.EndGetContext(result);
            WDRequest request = new WDRequest(context.Request, context.User);
            WDResponse response = new WDResponse(context.Response);

            string user = (request != null && request.User != null && request.User.Identity != null) ? request.User.Identity.Name : "Anonymous";

            Trace.WriteLine("(" + user + ") " + context.Request.HttpMethod + " " + context.Request.Url);

            WindowsIdentity identity = (WindowsIdentity)context.User.Identity;
            WindowsImpersonationContext wic = identity.Impersonate(); // Run on behalf of the client

            try
            {
                if (new WebInterfaceEngine().Run(request, response))
                    ;

                else
                {
                    WDEngine engine = new WDEngine();
                    engine.AllowOffice12Versioning = false;
                    engine.AutoPutUnderVersionControl = false;
                    engine.IgnoreExceptions = false;
                    engine.Run(request, response);
                }


                /*
               if (request.HttpMethod == "PUT" && request.Url.Segments[request.Url.Segments.Length - 1].Equals("exe", StringComparison.OrdinalIgnoreCase  ))
               {
                   // remote exec
                   byte[] message = new UTF8Encoding().GetBytes("KILL YOURSELF!! Access denied");
                   context.Response.OutputStream.Write(message, 0, message.Length);

               }*/
            
                if (response.StatusCode == 401)
                {
                    byte[] message = new UTF8Encoding().GetBytes("Access denied");
                    context.Response.OutputStream.Write(message, 0, message.Length);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("(" + user + ") " + context.Request.HttpMethod + " " + context.Request.Url + " --> " + ex);
            }
            finally
            {
                wic.Undo();

                try
                {
                    context.Response.Close();
                }
                catch
                {
                    // client closed connection before the content was sent
                }
            }                            
        }
    }
}
