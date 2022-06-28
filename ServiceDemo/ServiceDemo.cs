using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Web;

namespace ServiceDemo
{
    public partial class ServiceDemo : ServiceBase
    {
        private int _eventID;
        private HttpListener listener;
        public ServiceDemo()
        {
            InitializeComponent();
            eventLog = new EventLog();
            eventLog.Source = "ServiceDemo";
            eventLog.Log = "ServiceDemoLog";
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        /// <summary>
        /// 服务启动
        /// </summary>
        /// <param name="args">启动时，传入的参数</param>
        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            eventLog.WriteEntry($"Service start {_eventID++}!");
            Task.Factory.StartNew(StartListener);
            /*Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += OnTimerEscape;
            timer.Start();*/
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// 处理 URL 回调
        /// </summary>
        private void StartListener()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:54321/");
                listener.Start();
                while (true)
                {
                    var context = listener.GetContext();
                    if (!context.Request.Url.ToString().Contains("favicon.ico"))
                    {
                        eventLog.WriteEntry(context.Request.Url.ToString(), EventLogEntryType.Information, _eventID++);
                        var res = GetParam(context.Request.Url.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.StatusDescription = "OK";
                        context.Response.AddHeader("Server", "ServiceDemo");
                        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
                        {
                            writer.Write("ServiceDemo 已收到消息");
                            writer.Close();
                            context.Response.Close();
                        }
                        eventLog.WriteEntry($"Prepare to lunch {res["start"]}", EventLogEntryType.Information, _eventID++);
                        try
                        {
                            Interop.CreateProcess(res["start"], context.Request.Url.ToString());
                        }
                        catch (Exception e)
                        {
                            eventLog.WriteEntry(e.Message, EventLogEntryType.Error, _eventID++);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                eventLog.WriteEntry($"Service Fail {e.Message}");
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                throw;
            }
        }

        private void OnTimerEscape(object sender, ElapsedEventArgs e)
        {
            eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, _eventID++);
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            listener.Stop();
            eventLog.WriteEntry($"Service stop {_eventID++}!");
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// 从 URL 中解析请求参数
        /// </summary>
        /// <param name="url">回调 URL</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetParam(string url)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            int start = 0, end = 0;
            var resstring = HttpUtility.UrlDecode(url);
            start = resstring.IndexOf("?");
            resstring = resstring.Substring(start + 1);
            start = 0;
            while (end != -1)
            {
                end = resstring.IndexOf("&", start, StringComparison.Ordinal);
                if (end != -1)
                {
                    var temp = resstring.Substring(start, end - start).Split('=');
                    res.Add(temp?[0], temp?[1]);
                }
                else
                {
                    var temp = resstring.Substring(start).Split('=');
                    if (temp.Length == 1) break;
                    res.Add(temp?[0], temp?[1]);
                }
                start = end + 1;
            }
            return res;
        }
    }
}
