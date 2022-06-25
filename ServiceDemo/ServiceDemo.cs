using System;
using System.IO;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Timers;

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
                    eventLog.WriteEntry(context.Request.Url.ToString(), EventLogEntryType.Information, _eventID++);
                    
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.StatusDescription = "OK"; 
                    context.Response.AddHeader("Server", "ServiceDemo");
                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
                    {
                        writer.Write("ServiceDemo 已收到消息");
                        writer.Close();
                        context.Response.Close();
                    }
                    //new ToastContentBuilder()
                    //    .AddArgument("action", "viewConversation")
                    //    .AddArgument("conversationId", 9813)
                    //    .AddText("Andrew sent you a picture")
                    //    .AddText("Check this out, The Enchantments in Washington!")
                    //    .Show();
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
