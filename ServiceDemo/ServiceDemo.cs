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
                        Interop.CreateProcess(res["start"], context.Request.Url.ToString());
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

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

}
